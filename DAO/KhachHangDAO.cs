using System.Data;
using MySqlConnector;
using System.Text.Json;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class KhachHangDAO
{
    private readonly string duongDan;
    private readonly PhienBangSql<KhachHang> sql = new();
    internal string ThuMuc => Path.GetDirectoryName(duongDan)!;

    public KhachHangDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "khachhang.json"));
    }

    public List<KhachHang> DocDS()
    {
        List<KhachHang> danhSach = new List<KhachHang>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHang));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaKH")} AS `MaKH`, {DanhMucMySql.Cot(map, "HoTen")} AS `HoTen`, {DanhMucMySql.Cot(map, "CCCD")} AS `CCCD`, {DanhMucMySql.Cot(map, "SDT")} AS `SDT`, {DanhMucMySql.Cot(map, "Email")} AS `Email`, {DanhMucMySql.Cot(map, "TaiKhoan")} AS `TaiKhoan`, {DanhMucMySql.Cot(map, "MatKhau")} AS `MatKhau`, {DanhMucMySql.Cot(map, "DiaChi")} AS `DiaChi`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(KhachHang));
            while (reader.Read())
            {
                KhachHang item = new KhachHang();
                item.MaKH = reader.IsDBNull(reader.GetOrdinal("MaKH")) ? "" : reader.GetString("MaKH").TrimEnd();
                item.HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString("HoTen").TrimEnd();
                item.CCCD = reader.IsDBNull(reader.GetOrdinal("CCCD")) ? "" : reader.GetString("CCCD").TrimEnd();
                item.SDT = reader.IsDBNull(reader.GetOrdinal("SDT")) ? "" : reader.GetString("SDT").TrimEnd();
                item.Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email").TrimEnd();
                item.TaiKhoan = reader.IsDBNull(reader.GetOrdinal("TaiKhoan")) ? "" : reader.GetString("TaiKhoan").TrimEnd();
                item.MatKhau = reader.IsDBNull(reader.GetOrdinal("MatKhau")) ? "" : reader.GetString("MatKhau").TrimEnd();
                item.DiaChi = reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString("DiaChi").TrimEnd();
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin khách hàng: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        sql.GhiNhanDaDoc(danhSach);
        return danhSach;
    }

    public void Them(KhachHang item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHang));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaKH")}, {DanhMucMySql.Cot(map, "HoTen")}, {DanhMucMySql.Cot(map, "CCCD")}, {DanhMucMySql.Cot(map, "SDT")}, {DanhMucMySql.Cot(map, "Email")}, {DanhMucMySql.Cot(map, "TaiKhoan")}, {DanhMucMySql.Cot(map, "MatKhau")}, {DanhMucMySql.Cot(map, "DiaChi")}) VALUES (@MaKH, @HoTen, @CCCD, @SDT, @Email, @TaiKhoan, @MatKhau, @DiaChi)";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaKH", (object?)item.MaKH ?? DBNull.Value);
            command.Parameters.AddWithValue("@HoTen", (object?)item.HoTen ?? DBNull.Value);
            command.Parameters.AddWithValue("@CCCD", (object?)item.CCCD ?? DBNull.Value);
            command.Parameters.AddWithValue("@SDT", (object?)item.SDT ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object?)item.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@TaiKhoan", (object?)item.TaiKhoan ?? DBNull.Value);
            command.Parameters.AddWithValue("@MatKhau", (object?)item.MatKhau ?? DBNull.Value);
            command.Parameters.AddWithValue("@DiaChi", (object?)item.DiaChi ?? DBNull.Value);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin khách hàng: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(KhachHang item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHang));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaKH", "HoTen", "CCCD", "SDT", "Email", "TaiKhoan", "MatKhau", "DiaChi" };
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaKH", (object?)item.MaKH ?? DBNull.Value);
            command.Parameters.AddWithValue("@HoTen", (object?)item.HoTen ?? DBNull.Value);
            command.Parameters.AddWithValue("@CCCD", (object?)item.CCCD ?? DBNull.Value);
            command.Parameters.AddWithValue("@SDT", (object?)item.SDT ?? DBNull.Value);
            command.Parameters.AddWithValue("@Email", (object?)item.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@TaiKhoan", (object?)item.TaiKhoan ?? DBNull.Value);
            command.Parameters.AddWithValue("@MatKhau", (object?)item.MatKhau ?? DBNull.Value);
            command.Parameters.AddWithValue("@DiaChi", (object?)item.DiaChi ?? DBNull.Value);
            // Khoa dong truoc khi UPDATE; sua khong doi du lieu van hop le.
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa.");
            command.CommandText = query;
            if (cacCot.All(map.KeyColumns.Contains)) { transaction.Commit(); return; }
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin khách hàng: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(string ma)
    {
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHang));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<KhachHang>(connection, transaction, map);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@" + map.KeyColumns[0], ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin khách hàng: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(KhachHang item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHang));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(KhachHang).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin khách hàng: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    // Giu API Doc/Luu cu cho BUS va du lieu JSON.
    public List<KhachHang> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<KhachHang>();
        return JsonSerializer.Deserialize<List<KhachHang>>(File.ReadAllText(duongDan))
            ?? throw new InvalidDataException("File khách hàng không hợp lệ.");
    }

    public void Luu(IReadOnlyCollection<KhachHang> danhSach)
    {
        if (sql.ThuLuu(danhSach)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
        var fileTam = duongDan + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(fileTam, JsonSerializer.Serialize(danhSach,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(fileTam, duongDan, overwrite: true);
        }
        finally
        {
            if (File.Exists(fileTam)) File.Delete(fileTam);
        }
    }

}
