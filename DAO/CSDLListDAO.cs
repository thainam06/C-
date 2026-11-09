using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Data.Common;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public sealed class AnhXaBangSql
{
    public string Table { get; set; } = "";
    public Dictionary<string, string> Columns { get; set; } = new();
    public List<string> KeyColumns { get; set; } = new();
    public Dictionary<string, string> DerivedColumns { get; set; } = new();
}

public static class CSDLListDAO
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static AnhXaBangSql DocAnhXa(Type type)
    {
        var path = Environment.GetEnvironmentVariable(MyConnection.DungMySql ? "QLMB_MYSQL_MAPPING_FILE" : "QLMB_SQL_MAPPING_FILE");
        if (path == null)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "QuanLyMayBay", MyConnection.DungMySql ? "mysql-mapping.json" : "sql-mapping.json");
            if (MyConnection.DungMySql && !File.Exists(path))
                path = Path.Combine(AppContext.BaseDirectory, "mysql-mapping.BanVeMayBay.json");
        }
        if (!File.Exists(path)) throw new InvalidDataException("Thiếu file ánh xạ. Cấu hình QLMB_MYSQL_MAPPING_FILE (MySQL) hoặc QLMB_SQL_MAPPING_FILE (SQL Server) theo CSDL của nhóm.");
        var maps = JsonSerializer.Deserialize<Dictionary<string, AnhXaBangSql>>(File.ReadAllText(path), Options);
        var profileName = type.Name.EndsWith("DTO", StringComparison.Ordinal) ? type.Name[..^3] : type.Name;
        if (maps == null || (!maps.TryGetValue(type.Name, out var map) && !maps.TryGetValue(profileName, out map)) || map == null)
            throw new InvalidDataException($"Thiếu ánh xạ SQL cho {type.Name}.");
        var properties = type.GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
        if (string.IsNullOrWhiteSpace(map.Table) || map.Columns == null || map.KeyColumns == null || map.DerivedColumns == null
            || map.Table.Split('.').Length > 2
            || properties.Any(x => !map.Columns.ContainsKey(x.Name) && !map.DerivedColumns.ContainsKey(x.Name))
            || map.Columns.Any(x => string.IsNullOrWhiteSpace(x.Value) || properties.All(p => p.Name != x.Key))
            || map.DerivedColumns.Any(x => type != typeof(GheDTO) || x.Key != nameof(GheDTO.LoaiGhe)
                || x.Value != "LoaiGheTuHangVe" || map.Columns.ContainsKey(x.Key))
            || map.Columns.Values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != map.Columns.Count
            || map.KeyColumns.Count == 0 || map.KeyColumns.Distinct().Count() != map.KeyColumns.Count
            || map.KeyColumns.Any(x => !map.Columns.ContainsKey(x)))
            throw new InvalidDataException($"Ánh xạ SQL của {type.Name} không hợp lệ. Cần đủ cột DTO và khóa chính.");
        return map;
    }

    public static string TenSql(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Contains('\0'))
            throw new ArgumentException("Tên SQL không hợp lệ.");
        return MyConnection.DungMySql ? "`" + value.Replace("`", "``") + "`" : "[" + value.Replace("]", "]]") + "]";
    }

    private static DbCommand Command(DbConnection connection, DbTransaction? transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        return command;
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string Table(AnhXaBangSql map) => string.Join(".", map.Table.Split('.').Select(TenSql));

    public static bool ThuDoc<T>(out List<T> data, out string? version) where T : new()
    {
        data = new List<T>(); version = null;
        if (!MyConnection.DaCauHinh) return false;
        var map = DocAnhXa(typeof(T));
        if (MyConnection.DungMySql && DanhMucMySql.ThuDoc<T>(out data))
        {
            version = Version(data.Cast<object>(), map);
            return true;
        }
        using var connection = MyConnection.MoKetNoiDuLieu();
        data = ReadRows(typeof(T), map, connection, null).Cast<T>().ToList();
        version = Version(data.Cast<object>(), map);
        return true;
    }

    private static List<object> ReadRows(Type type, AnhXaBangSql map, DbConnection connection, DbTransaction? transaction)
    {
        var data = new List<object>();
        var fields = type.GetProperties().Where(x => x.CanRead && x.CanWrite && map.Columns.ContainsKey(x.Name)).ToArray();
        using var command = Command(connection, transaction, "SELECT " + string.Join(",", fields.Select(x => TenSql(map.Columns[x.Name])))
            + " FROM " + Table(map) + (transaction == null ? "" : MyConnection.DungMySql ? " FOR UPDATE" : " WITH (UPDLOCK,HOLDLOCK)"));
        using var reader = command.ExecuteReader();
        for (int i = 0; i < fields.Length; i++)
            if (map.KeyColumns.Contains(fields[i].Name) && fields[i].PropertyType == typeof(string)
                && reader.GetFieldType(i) != typeof(string))
                throw new InvalidDataException($"Khóa {fields[i].Name} trong SQL không phải kiểu chuỗi như DTO. Cần adapter đúng kiểu trước khi đọc/ghi, không tự ép kiểu khóa.");
        while (reader.Read())
        {
            var item = Activator.CreateInstance(type)!;
            for (int i = 0; i < fields.Length; i++)
            {
                var target = Nullable.GetUnderlyingType(fields[i].PropertyType) ?? fields[i].PropertyType;
                object? value = reader.IsDBNull(i) ? target == typeof(string) ? "" : null
                    : target == typeof(string) ? Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture)?.TrimEnd()
                    : Convert.ChangeType(reader.GetValue(i), target, CultureInfo.InvariantCulture);
                if (value == null && Nullable.GetUnderlyingType(fields[i].PropertyType) == null && target.IsValueType)
                    throw new InvalidDataException($"Cột {fields[i].Name} không được NULL.");
                fields[i].SetValue(item, value);
            }
            ApDungCotSuyDien(item, map);
            data.Add(item);
        }
        return data;
    }

    public static string ChuanHoaKhoaSql(object? value) => value is string text
        ? text.TrimEnd() : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";

    public static void ApDungCotSuyDien(object item, AnhXaBangSql map)
    {
        if (item is GheDTO ghe && map.DerivedColumns.TryGetValue(nameof(GheDTO.LoaiGhe), out var rule))
        {
            if (rule != "LoaiGheTuHangVe") throw new InvalidDataException("Quy tắc suy diễn loại ghế không hợp lệ.");
            ghe.LoaiGhe = ghe.MaHV.Trim().ToUpperInvariant() is "HV02" or "HV2" or "HV07" or "HV08" or "HV09" or "HV10"
                ? GheDTO.Vip : GheDTO.Thuong;
        }
    }

    internal static string Version(IEnumerable<object> rows, AnhXaBangSql map) => string.Join("\n",
        rows.Select(row => JsonSerializer.Serialize(row.GetType().GetProperties()
            .Where(p => map.Columns.ContainsKey(p.Name)).OrderBy(p => p.Name)
            .ToDictionary(p => p.Name, p => p.GetValue(row)))).OrderBy(x => x, StringComparer.Ordinal));

    internal static bool ThuLuuNhieu(IEnumerable<LoGhiSql> tables)
    {
        if (!MyConnection.DaCauHinh) return false;
        var batches = tables.Select(x => (Batch: x, Map: DocAnhXa(x.Type))).ToList();
        using var connection = MyConnection.MoKetNoiDuLieu();
        if (MyConnection.DungMySql)
            foreach (var batch in batches) KiemTraBangMySql(connection, batch.Map);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        foreach (var batch in batches)
        {
            if (batch.Batch.Version == null || batch.Batch.Version != Version(ReadRows(batch.Batch.Type, batch.Map, connection, transaction), batch.Map))
                throw new InvalidOperationException("Dữ liệu SQL đã thay đổi ở phiên khác hoặc chưa được đọc. Đóng/mở lại ứng dụng trước khi lưu để tránh ghi đè.");
        }
        // Ghi bang cha truoc, xoa theo thu tu nguoc. Loi FK/unique se rollback ca lo.
        foreach (var b in batches) Upsert(connection, transaction, b.Batch.Type, b.Map, b.Batch.Rows);
        foreach (var b in batches.AsEnumerable().Reverse()) DeleteRemoved(connection, transaction, b.Batch.Type, b.Map, b.Batch.Rows);
        var versions = batches.Select(b => Version(ReadRows(b.Batch.Type, b.Map, connection, transaction), b.Map)).ToList();
        transaction.Commit();
        for (int i = 0; i < batches.Count; i++) batches[i].Batch.AfterCommit(versions[i]);
        return true;
    }

    internal static void KiemTraBangMySql(DbConnection connection, AnhXaBangSql map)
    {
        var names = map.Table.Split('.');
        using var check = Command(connection, null,
            "SELECT ENGINE FROM information_schema.TABLES WHERE TABLE_SCHEMA=COALESCE(@schema,DATABASE()) AND TABLE_NAME=@table");
        AddParameter(check, "@schema", names.Length == 2 ? names[0] : null);
        AddParameter(check, "@table", names[^1]);
        if (!string.Equals(Convert.ToString(check.ExecuteScalar()), "InnoDB", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Chỉ ghi bảng MySQL InnoDB để bảo đảm rollback và khóa giao dịch. Kiểm tra bảng " + map.Table);
    }

    internal static void KiemTraGiaTriMySql(DbConnection connection, DbTransaction transaction, Type type, AnhXaBangSql map, List<object> rows)
    {
        var names = map.Table.Split('.');
        using var command = Command(connection, transaction,
            "SELECT COLUMN_NAME,DATA_TYPE,NUMERIC_SCALE,CHARACTER_MAXIMUM_LENGTH FROM information_schema.COLUMNS "
            + "WHERE TABLE_SCHEMA=COALESCE(@schema,DATABASE()) AND TABLE_NAME=@table");
        AddParameter(command, "@schema", names.Length == 2 ? names[0] : null);
        AddParameter(command, "@table", names[^1]);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var propertyName = map.Columns.FirstOrDefault(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;
            if (propertyName == null) continue;
            var property = type.GetProperty(propertyName)!;
            var dataType = reader.GetString(1);
            int? scale = dataType is "tinyint" or "smallint" or "mediumint" or "int" or "bigint" ? 0
                : dataType is "decimal" ? Convert.ToInt32(reader.GetValue(2)) : null;
            var maxLength = reader.IsDBNull(3) ? (long?)null : Convert.ToInt64(reader.GetValue(3));
            foreach (var row in rows)
            {
                var value = property.GetValue(row);
                if (value is double real && !double.IsFinite(real))
                    throw new ArgumentException($"{type.Name}.{propertyName}: không nhận NaN hoặc vô cực.");
                decimal? numeric = value is decimal money ? money : value is double realValue ? Convert.ToDecimal(realValue) : null;
                if (numeric is decimal number && scale is >= 0 and <= 28 && decimal.Round(number, scale.Value) != number)
                    throw new ArgumentException($"{type.Name}.{propertyName}: cột MySQL chỉ nhận {scale} chữ số thập phân. Không tự làm tròn dữ liệu.");
                if (value is string text && maxLength.HasValue && text.EnumerateRunes().Count() > maxLength.Value)
                    throw new ArgumentException($"{type.Name}.{propertyName}: vượt quá {maxLength} ký tự của cột MySQL.");
            }
        }
    }

    private static void Upsert(DbConnection connection, DbTransaction transaction, Type type, AnhXaBangSql map, List<object> rows)
    {
        if (MyConnection.DungMySql) KiemTraGiaTriMySql(connection, transaction, type, map, rows);
        var fields = type.GetProperties().Where(x => map.Columns.ContainsKey(x.Name)).ToArray();
        var keys = fields.Where(x => map.KeyColumns.Contains(x.Name)).ToArray();
        var values = fields.Where(x => !map.KeyColumns.Contains(x.Name)).ToArray();
        foreach (var row in rows)
        {
            using var command = Command(connection, transaction, "");
            for (int i = 0; i < fields.Length; i++) AddParameter(command, MyConnection.DungMySql ? "@" + fields[i].Name : "@p" + i, fields[i].GetValue(row));
            string Param(PropertyInfo p) => MyConnection.DungMySql ? "@" + p.Name : "@p" + Array.IndexOf(fields, p);
            var where = string.Join(" AND ", keys.Select(x => TenSql(map.Columns[x.Name]) + "=" + Param(x)));
            var insert = "INSERT INTO " + Table(map) + " (" + string.Join(",", fields.Select(x => TenSql(map.Columns[x.Name])))
                + ") VALUES (" + string.Join(",", fields.Select(Param)) + ");";
            if (MyConnection.DungMySql)
            {
                command.CommandText = "SELECT COUNT(*) FROM " + Table(map) + " WHERE " + where;
                var exists = Convert.ToInt64(command.ExecuteScalar()) > 0;
                if (exists && values.Length == 0) continue;
                command.CommandText = exists
                    ? "UPDATE " + Table(map) + " SET " + string.Join(",", values.Select(x => TenSql(map.Columns[x.Name]) + "=" + Param(x))) + " WHERE " + where
                    : insert;
                command.ExecuteNonQuery();
                continue;
            }
            command.CommandText = values.Length == 0
                ? "IF NOT EXISTS (SELECT 1 FROM " + Table(map) + " WHERE " + where + ") " + insert
                : "UPDATE " + Table(map) + " SET " + string.Join(",", values.Select(x => TenSql(map.Columns[x.Name]) + "=" + Param(x)))
                    + " WHERE " + where + "; IF @@ROWCOUNT=0 " + insert;
            command.ExecuteNonQuery();
        }
    }

    private static void DeleteRemoved(DbConnection connection, DbTransaction transaction, Type type, AnhXaBangSql map, List<object> rows)
    {
        var keys = map.KeyColumns.Select(x => type.GetProperty(x)!).ToArray();
        string Key(IEnumerable<object?> values) => JsonSerializer.Serialize(values.Select(ChuanHoaKhoaSql).ToArray());
        var retained = rows.Select(x => Key(keys.Select(p => p.GetValue(x)))).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var removed = new List<object[]>();
        using (var select = Command(connection, transaction, "SELECT " + string.Join(",", keys.Select(x => TenSql(map.Columns[x.Name]))) + " FROM " + Table(map)))
        using (var reader = select.ExecuteReader())
            while (reader.Read())
            {
                var values = new object[keys.Length]; reader.GetValues(values);
                if (!retained.Contains(Key(values))) removed.Add(values);
            }
        foreach (var values in removed)
        {
            using var command = Command(connection, transaction, "");
            command.CommandText = "DELETE FROM " + Table(map) + " WHERE "
                + string.Join(" AND ", keys.Select((x, i) => TenSql(map.Columns[x.Name]) + "=@k" + i));
            for (int i = 0; i < values.Length; i++) AddParameter(command, "@k" + i, values[i]);
            command.ExecuteNonQuery();
        }
    }
}

internal sealed record LoGhiSql(Type Type, List<object> Rows, string? Version, Action<string> AfterCommit);

internal sealed class PhienBangSql<T> where T : new()
{
    private string? version;
    public bool ThuDoc(out List<T> data) => CSDLListDAO.ThuDoc(out data, out version);
    internal void GhiNhanDaDoc(IReadOnlyCollection<T> data) => version = CSDLListDAO.Version(data.Cast<object>(), CSDLListDAO.DocAnhXa(typeof(T)));
    internal LoGhiSql TaoLo(IReadOnlyCollection<T> data) => new(typeof(T), data.Cast<object>().ToList(), version, v => version = v);
    public bool ThuLuu(IReadOnlyCollection<T> data) => CSDLListDAO.ThuLuuNhieu(new[] { TaoLo(data) });
}
