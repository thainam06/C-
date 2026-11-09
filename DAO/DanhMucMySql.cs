using MySqlConnector;
using System.Text.RegularExpressions;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

// Chi ho tro anh xa va kiem tra kieu; SQL CRUD nam truc tiep trong tung DAO.
internal static class DanhMucMySql
{
    internal static bool ThuDoc<T>(out List<T> rows)
    {
        object? data = typeof(T).Name switch
        {
            nameof(HangHangKhong) when typeof(T) == typeof(HangHangKhong) => new HangHangKhongDAO().DocDS(),
            nameof(MayBay) when typeof(T) == typeof(MayBay) => new MayBayDAO().DocDS(),
            nameof(HangVe) when typeof(T) == typeof(HangVe) => new HangVeDAO().DocDS(),
            nameof(Ghe) when typeof(T) == typeof(Ghe) => new GheDAO().DocDS(),
            nameof(KhachHang) when typeof(T) == typeof(KhachHang) => new KhachHangDAO().DocDS(),
            nameof(HanhKhach) when typeof(T) == typeof(HanhKhach) => new HanhKhachDAO().DocDS(),
            nameof(NhanVien) when typeof(T) == typeof(NhanVien) => new NhanVienDAO().DocDS(),
            nameof(CTKM) when typeof(T) == typeof(CTKM) => new CTKMDAO().DocDS(),
            nameof(Voucher) when typeof(T) == typeof(Voucher) => new VoucherDAO().DocDS(),
            nameof(ThamSo) when typeof(T) == typeof(ThamSo) => new ThamSoDAO().DocDS(),
            _ => null
        };
        rows = data is List<T> typed ? typed : new();
        return data != null;
    }

    // SQL trong file nguoi dung giu ten thuoc tinh DTO; chuyen sang ten cot CSDL.
    // Chi ho tro cac lenh CRUD tham so hoa mot bang, khong nhan SQL tuy y tu GUI.
    internal static string AnhXaCauLenh(string query, AnhXaBangSql map)
    {
        var select = Regex.Match(query, @"^SELECT ([A-Za-z0-9_, ]+) FROM [A-Za-z0-9_]+$", RegexOptions.IgnoreCase);
        if (select.Success)
            return "SELECT " + string.Join(",", select.Groups[1].Value.Split(',').Select(p => p.Trim())
                .Select(p => Cot(map, p) + " AS " + CSDLListDAO.TenSql(p))) + " FROM " + Bang(map);
        // Table token after INSERT INTO/UPDATE/DELETE FROM; column tokens never alter @parameters.
        var table = Regex.Match(query, @"^(?:INSERT INTO|UPDATE|DELETE FROM) ([A-Za-z0-9_]+)", RegexOptions.IgnoreCase);
        if (!table.Success) throw new ArgumentException("Câu SQL không phải CRUD một bảng được hỗ trợ.");
        var tableToken = table.Groups[1];
        var before = query[..tableToken.Index];
        var after = query[(tableToken.Index + tableToken.Length)..];
        after = Regex.Replace(after, @"(?<![@\w])[A-Za-z_][A-Za-z0-9_]*", match =>
            map.Columns.TryGetValue(match.Value, out var column) ? CSDLListDAO.TenSql(column) : match.Value);
        return before + Bang(map) + after;
    }

    internal static string Bang(AnhXaBangSql map) => string.Join(".", map.Table.Split('.').Select(CSDLListDAO.TenSql));
    internal static string Cot(AnhXaBangSql map, string name) => CSDLListDAO.TenSql(map.Columns[name]);
    internal static string DieuKienKhoa(AnhXaBangSql map) => string.Join(" AND ", map.KeyColumns.Select(p => Cot(map, p) + "=@" + p));

    internal static void KiemTraKieuKhoa(MySqlDataReader reader, AnhXaBangSql map, Type type)
    {
        foreach (var key in map.KeyColumns)
            if (type.GetProperty(key)!.PropertyType == typeof(string) && reader.GetFieldType(reader.GetOrdinal(key)) != typeof(string))
                throw new InvalidDataException($"Khóa {key} trong MySQL phải có kiểu chuỗi như DTO.");
    }

    internal static void KiemTraBangKhoa<T>(MySqlConnection connection, MySqlTransaction transaction, AnhXaBangSql map)
    {
        string query = "SELECT " + string.Join(",", map.KeyColumns.Select(p => Cot(map, p) + " AS " + CSDLListDAO.TenSql(p))) + " FROM " + Bang(map) + " LIMIT 0";
        using var command = new MySqlCommand(query, connection, transaction);
        using var reader = command.ExecuteReader();
        KiemTraKieuKhoa(reader, map, typeof(T));
    }

    internal static void KiemTraTruocKhiGhi<T>(MySqlConnection connection, MySqlTransaction transaction, AnhXaBangSql map, T item, bool xoa = false)
    {
        foreach (var key in map.KeyColumns)
            if (string.IsNullOrWhiteSpace(Convert.ToString(typeof(T).GetProperty(key)!.GetValue(item))))
                throw new ArgumentException("Vui lòng nhập đủ khóa chính: " + key);
        KiemTraBangKhoa<T>(connection, transaction, map);
        if (!xoa) CSDLListDAO.KiemTraGiaTriMySql(connection, transaction, typeof(T), map, new() { item! });
    }
}
