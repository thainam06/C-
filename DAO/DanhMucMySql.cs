using System.Data;
using System.Globalization;
using MySqlConnector;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

// Dung chung vong doi connection/command va lenh co tham so cho cac DAO danh muc.
// Moi DAO khai bao ro cac cot va cach tao DTO tu MySqlDataReader.
internal sealed class LenhDanhMucMySql
{
    internal Type Kieu { get; }
    private readonly string[] cot;
    private readonly Func<MySqlDataReader, AnhXaBangSql, object> taoDTO;

    internal LenhDanhMucMySql(Type kieu, string[] cot, Func<MySqlDataReader, AnhXaBangSql, object> taoDTO)
    { Kieu = kieu; this.cot = cot; this.taoDTO = taoDTO; }

    private AnhXaBangSql AnhXa()
    {
        if (!MyConnection.DungMySql)
            throw new InvalidOperationException("Các hàm CRUD MySQL cần cấu hình QLMB_MYSQL_CONNECTION.");
        return CSDLListDAO.DocAnhXa(Kieu);
    }

    private string[] CotVatLy(AnhXaBangSql map) => cot.Where(map.Columns.ContainsKey).ToArray();
    private static string Bang(AnhXaBangSql map) => string.Join(".", map.Table.Split('.').Select(CSDLListDAO.TenSql));
    private static string Cot(AnhXaBangSql map, string name) => CSDLListDAO.TenSql(map.Columns[name]);
    private static string DieuKienKhoa(AnhXaBangSql map) => string.Join(" AND ", map.KeyColumns.Select(p => Cot(map, p) + "=@" + p));
    internal string LenhDoc(AnhXaBangSql map, bool khoa = false) => "SELECT "
        + string.Join(",", CotVatLy(map).Select(p => Cot(map, p) + " AS " + CSDLListDAO.TenSql(p)))
        + " FROM " + Bang(map) + (khoa ? " FOR UPDATE" : "");
    internal string LenhThem(AnhXaBangSql map) => "INSERT INTO " + Bang(map) + " ("
        + string.Join(",", CotVatLy(map).Select(p => Cot(map, p))) + ") VALUES ("
        + string.Join(",", CotVatLy(map).Select(p => "@" + p)) + ")";
    internal string LenhSua(AnhXaBangSql map) => "UPDATE " + Bang(map) + " SET "
        + string.Join(",", CotVatLy(map).Where(p => !map.KeyColumns.Contains(p)).Select(p => Cot(map, p) + "=@" + p))
        + " WHERE " + DieuKienKhoa(map);
    internal string LenhXoa(AnhXaBangSql map) => "DELETE FROM " + Bang(map) + " WHERE " + DieuKienKhoa(map);

    private void KiemTraKhoa(MySqlDataReader reader, AnhXaBangSql map)
    {
        foreach (var key in map.KeyColumns)
            if (Kieu.GetProperty(key)!.PropertyType == typeof(string) && reader.GetFieldType(reader.GetOrdinal(key)) != typeof(string))
                throw new InvalidDataException($"Khóa {key} trong MySQL phải có kiểu chuỗi như DTO; không tự ép kiểu khóa.");
    }

    internal List<object> Doc(MySqlConnection connection, MySqlTransaction? transaction, AnhXaBangSql map)
    {
        using var command = new MySqlCommand(LenhDoc(map, transaction != null), connection, transaction);
        using var reader = command.ExecuteReader();
        KiemTraKhoa(reader, map);
        var result = new List<object>();
        while (reader.Read())
        {
            var dto = taoDTO(reader, map);
            CSDLListDAO.ApDungCotSuyDien(dto, map);
            result.Add(dto);
        }
        return result;
    }

    internal List<T> DocDS<T>()
    {
        using var connection = MyConnection.GetConnection();
        connection.Open();
        return Doc(connection, null, AnhXa()).Cast<T>().ToList();
    }

    internal void Them(object dto) => Ghi(dto, "Them");
    internal void Sua(object dto) => Ghi(dto, "Sua");
    internal void Xoa(object dto) => Ghi(dto, "Xoa");
    internal void Xoa(string ma)
    {
        var map = AnhXa();
        if (map.KeyColumns.Count != 1)
            throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto) và điền đủ khóa.");
        var dto = Activator.CreateInstance(Kieu)!;
        Kieu.GetProperty(map.KeyColumns[0])!.SetValue(dto, ma);
        Xoa(dto);
    }

    private void Ghi(object dto, string thaoTac)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var map = AnhXa();
        foreach (var key in map.KeyColumns)
            if (string.IsNullOrWhiteSpace(Convert.ToString(Kieu.GetProperty(key)!.GetValue(dto))))
                throw new ArgumentException("Vui lòng nhập đủ khóa chính: " + key);
        using var connection = MyConnection.GetConnection();
        connection.Open();
        CSDLListDAO.KiemTraBangMySql(connection, map);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        // Kiem tra kieu khoa ke ca Them vao bang dang rong.
        using (var schema = new MySqlCommand(LenhDoc(map) + " LIMIT 0", connection, transaction))
        using (var reader = schema.ExecuteReader()) KiemTraKhoa(reader, map);
        if (thaoTac != "Xoa") CSDLListDAO.KiemTraGiaTriMySql(connection, transaction, Kieu, map, new() { dto });
        using var command = new MySqlCommand { Connection = connection, Transaction = transaction };
        foreach (var property in CotVatLy(map))
            command.Parameters.AddWithValue("@" + property, Kieu.GetProperty(property)!.GetValue(dto) ?? DBNull.Value);
        if (thaoTac != "Them")
        {
            command.CommandText = "SELECT 1 FROM " + Bang(map) + " WHERE " + DieuKienKhoa(map) + " FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
        }
        command.CommandText = thaoTac switch { "Them" => LenhThem(map), "Sua" => LenhSua(map), _ => LenhXoa(map) };
        command.ExecuteNonQuery();
        transaction.Commit();
        // Loi se tu rollback khi dispose transaction; khong che mat loi FK/UNIQUE/TLS.
    }
}

internal static class DanhMucMySql
{
    internal static LenhDanhMucMySql? Tim(Type type) => type.Name switch
    {
        nameof(HangHangKhong) when type == typeof(HangHangKhong) => HangHangKhongDAO.Lenh,
        nameof(MayBay) when type == typeof(MayBay) => MayBayDAO.Lenh,
        nameof(HangVe) when type == typeof(HangVe) => HangVeDAO.Lenh,
        nameof(Ghe) when type == typeof(Ghe) => GheDAO.Lenh,
        nameof(KhachHang) when type == typeof(KhachHang) => KhachHangDAO.Lenh,
        nameof(HanhKhach) when type == typeof(HanhKhach) => HanhKhachDAO.Lenh,
        nameof(NhanVien) when type == typeof(NhanVien) => NhanVienDAO.Lenh,
        nameof(CTKM) when type == typeof(CTKM) => CTKMDAO.Lenh,
        nameof(Voucher) when type == typeof(Voucher) => VoucherDAO.Lenh,
        nameof(ThamSo) when type == typeof(ThamSo) => ThamSoDAO.Lenh,
        _ => null
    };

    // HanhLyKg la INT trong DDL nhom, nhung DTO dung decimal.
    internal static decimal SoThapPhan(MySqlDataReader reader, string name) =>
        Convert.ToDecimal(reader.GetValue(reader.GetOrdinal(name)), CultureInfo.InvariantCulture);

    internal static string Chuoi(MySqlDataReader reader, string name) => reader.IsDBNull(reader.GetOrdinal(name)) ? "" : reader.GetString(name).TrimEnd();
}
