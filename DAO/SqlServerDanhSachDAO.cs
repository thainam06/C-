using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace QuanLyMayBay.DAO;

public sealed class AnhXaBangSql
{
    public string Table { get; set; } = "";
    public Dictionary<string, string> Columns { get; set; } = new();
    public List<string> KeyColumns { get; set; } = new();
}

public static class SqlServerDanhSachDAO
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static AnhXaBangSql DocAnhXa(Type type)
    {
        var path = Environment.GetEnvironmentVariable("QLMB_SQL_MAPPING_FILE") ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanLyMayBay", "sql-mapping.json");
        if (!File.Exists(path)) throw new InvalidDataException("Thiếu sql-mapping.json. Cần ánh xạ đúng lược đồ SQL Server của nhóm trước khi dùng SQL.");
        var maps = JsonSerializer.Deserialize<Dictionary<string, AnhXaBangSql>>(File.ReadAllText(path), Options);
        if (maps == null || !maps.TryGetValue(type.Name, out var map))
            throw new InvalidDataException($"Thiếu ánh xạ SQL cho {type.Name}.");
        var properties = type.GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
        if (string.IsNullOrWhiteSpace(map.Table) || map.Columns == null || map.KeyColumns == null
            || map.Table.Split('.').Length > 2
            || properties.Any(x => !map.Columns.ContainsKey(x.Name))
            || map.Columns.Any(x => string.IsNullOrWhiteSpace(x.Value) || properties.All(p => p.Name != x.Key))
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
        return "[" + value.Replace("]", "]]") + "]";
    }

    private static string Table(AnhXaBangSql map) => string.Join(".", map.Table.Split('.').Select(TenSql));

    public static bool ThuDoc<T>(out List<T> data, out string? version) where T : new()
    {
        data = new List<T>(); version = null;
        if (!MyConnection.DaCauHinh) return false;
        var map = DocAnhXa(typeof(T));
        using var connection = MyConnection.MoKetNoi();
        data = ReadRows(typeof(T), map, connection, null).Cast<T>().ToList();
        version = Version(data.Cast<object>());
        return true;
    }

    private static List<object> ReadRows(Type type, AnhXaBangSql map, SqlConnection connection, SqlTransaction? transaction)
    {
        var data = new List<object>();
        var fields = type.GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
        using var command = new SqlCommand("SELECT " + string.Join(",", fields.Select(x => TenSql(map.Columns[x.Name])))
            + " FROM " + Table(map) + (transaction == null ? "" : " WITH (UPDLOCK,HOLDLOCK)"), connection, transaction);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var item = Activator.CreateInstance(type)!;
            for (int i = 0; i < fields.Length; i++)
            {
                var target = Nullable.GetUnderlyingType(fields[i].PropertyType) ?? fields[i].PropertyType;
                object? value = reader.IsDBNull(i) ? target == typeof(string) ? "" : null
                    : target == typeof(string) ? Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture)
                    : Convert.ChangeType(reader.GetValue(i), target, CultureInfo.InvariantCulture);
                if (value == null && Nullable.GetUnderlyingType(fields[i].PropertyType) == null && target.IsValueType)
                    throw new InvalidDataException($"Cột {fields[i].Name} không được NULL.");
                fields[i].SetValue(item, value);
            }
            data.Add(item);
        }
        return data;
    }

    private static string Version(IEnumerable<object> rows) => string.Join("\n",
        rows.Select(x => JsonSerializer.Serialize(x, x.GetType())).OrderBy(x => x, StringComparer.Ordinal));

    internal static bool ThuLuuNhieu(IEnumerable<LoGhiSql> tables)
    {
        if (!MyConnection.DaCauHinh) return false;
        var batches = tables.Select(x => (Batch: x, Map: DocAnhXa(x.Type))).ToList();
        using var connection = MyConnection.MoKetNoi();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        foreach (var batch in batches)
        {
            if (batch.Batch.Version == null || batch.Batch.Version != Version(ReadRows(batch.Batch.Type, batch.Map, connection, transaction)))
                throw new InvalidOperationException("Dữ liệu SQL đã thay đổi ở phiên khác hoặc chưa được đọc. Đóng/mở lại ứng dụng trước khi lưu để tránh ghi đè.");
        }
        // Ghi bang cha truoc, xoa theo thu tu nguoc. Loi FK/unique se rollback ca lo.
        foreach (var b in batches) Upsert(connection, transaction, b.Batch.Type, b.Map, b.Batch.Rows);
        foreach (var b in batches.AsEnumerable().Reverse()) DeleteRemoved(connection, transaction, b.Batch.Type, b.Map, b.Batch.Rows);
        var versions = batches.Select(b => Version(ReadRows(b.Batch.Type, b.Map, connection, transaction))).ToList();
        transaction.Commit();
        for (int i = 0; i < batches.Count; i++) batches[i].Batch.AfterCommit(versions[i]);
        return true;
    }

    private static void Upsert(SqlConnection connection, SqlTransaction transaction, Type type, AnhXaBangSql map, List<object> rows)
    {
        var fields = type.GetProperties().Where(x => map.Columns.ContainsKey(x.Name)).ToArray();
        var keys = fields.Where(x => map.KeyColumns.Contains(x.Name)).ToArray();
        var values = fields.Where(x => !map.KeyColumns.Contains(x.Name)).ToArray();
        foreach (var row in rows)
        {
            using var command = new SqlCommand { Connection = connection, Transaction = transaction };
            for (int i = 0; i < fields.Length; i++) command.Parameters.AddWithValue("@p" + i, fields[i].GetValue(row) ?? DBNull.Value);
            string Param(PropertyInfo p) => "@p" + Array.IndexOf(fields, p);
            var where = string.Join(" AND ", keys.Select(x => TenSql(map.Columns[x.Name]) + "=" + Param(x)));
            var insert = "INSERT INTO " + Table(map) + " (" + string.Join(",", fields.Select(x => TenSql(map.Columns[x.Name])))
                + ") VALUES (" + string.Join(",", fields.Select(Param)) + ");";
            command.CommandText = values.Length == 0
                ? "IF NOT EXISTS (SELECT 1 FROM " + Table(map) + " WHERE " + where + ") " + insert
                : "UPDATE " + Table(map) + " SET " + string.Join(",", values.Select(x => TenSql(map.Columns[x.Name]) + "=" + Param(x)))
                    + " WHERE " + where + "; IF @@ROWCOUNT=0 " + insert;
            command.ExecuteNonQuery();
        }
    }

    private static void DeleteRemoved(SqlConnection connection, SqlTransaction transaction, Type type, AnhXaBangSql map, List<object> rows)
    {
        var keys = map.KeyColumns.Select(x => type.GetProperty(x)!).ToArray();
        string Key(IEnumerable<object?> values) => JsonSerializer.Serialize(values.Select(x => Convert.ToString(x, CultureInfo.InvariantCulture)).ToArray());
        var retained = rows.Select(x => Key(keys.Select(p => p.GetValue(x)))).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var removed = new List<object[]>();
        using (var select = new SqlCommand("SELECT " + string.Join(",", keys.Select(x => TenSql(map.Columns[x.Name]))) + " FROM " + Table(map), connection, transaction))
        using (var reader = select.ExecuteReader())
            while (reader.Read())
            {
                var values = new object[keys.Length]; reader.GetValues(values);
                if (!retained.Contains(Key(values))) removed.Add(values);
            }
        foreach (var values in removed)
        {
            using var command = new SqlCommand { Connection = connection, Transaction = transaction };
            command.CommandText = "DELETE FROM " + Table(map) + " WHERE "
                + string.Join(" AND ", keys.Select((x, i) => TenSql(map.Columns[x.Name]) + "=@k" + i));
            for (int i = 0; i < values.Length; i++) command.Parameters.AddWithValue("@k" + i, values[i]);
            command.ExecuteNonQuery();
        }
    }
}

internal sealed record LoGhiSql(Type Type, List<object> Rows, string? Version, Action<string> AfterCommit);

internal sealed class PhienBangSql<T> where T : new()
{
    private string? version;
    public bool ThuDoc(out List<T> data) => SqlServerDanhSachDAO.ThuDoc(out data, out version);
    internal LoGhiSql TaoLo(IReadOnlyCollection<T> data) => new(typeof(T), data.Cast<object>().ToList(), version, v => version = v);
    public bool ThuLuu(IReadOnlyCollection<T> data) => SqlServerDanhSachDAO.ThuLuuNhieu(new[] { TaoLo(data) });
}
