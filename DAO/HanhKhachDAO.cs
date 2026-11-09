using System.Data;
using MySqlConnector;
using System.Text.Json;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class HanhKhachDAO
{
    private readonly string duongDan;
    private readonly PhienBangSql<HanhKhach> sql = new();

    public HanhKhachDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "hanhkhach.json"));
    }

    public List<HanhKhach> DocDS()
    {
        List<HanhKhach> danhSach = new List<HanhKhach>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaHK")} AS `MaHK`, {DanhMucMySql.Cot(map, "HoTen")} AS `HoTen`, {DanhMucMySql.Cot(map, "NgaySinh")} AS `NgaySinh`, {DanhMucMySql.Cot(map, "CCCD")} AS `CCCD`, {DanhMucMySql.Cot(map, "GioiTinh")} AS `GioiTinh`, {DanhMucMySql.Cot(map, "MaKH")} AS `MaKH`, {DanhMucMySql.Cot(map, "LoaiKH")} AS `LoaiKH`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(HanhKhach));
            while (reader.Read())
            {
                HanhKhach item = new HanhKhach();
                item.MaHK = reader.IsDBNull(reader.GetOrdinal("MaHK")) ? "" : reader.GetString("MaHK").TrimEnd();
                item.HoTen = reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString("HoTen").TrimEnd();
                item.NgaySinh = reader.IsDBNull(reader.GetOrdinal("NgaySinh")) ? null : reader.GetDateTime("NgaySinh");
                item.CCCD = reader.IsDBNull(reader.GetOrdinal("CCCD")) ? "" : reader.GetString("CCCD").TrimEnd();
                item.GioiTinh = reader.IsDBNull(reader.GetOrdinal("GioiTinh")) ? "" : reader.GetString("GioiTinh").TrimEnd();
                item.MaKH = reader.IsDBNull(reader.GetOrdinal("MaKH")) ? "" : reader.GetString("MaKH").TrimEnd();
                item.LoaiKH = reader.IsDBNull(reader.GetOrdinal("LoaiKH")) ? "" : reader.GetString("LoaiKH").TrimEnd();
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin hành khách: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        sql.GhiNhanDaDoc(danhSach);
        return danhSach;
    }

    public void Them(HanhKhach item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaHK")}, {DanhMucMySql.Cot(map, "HoTen")}, {DanhMucMySql.Cot(map, "NgaySinh")}, {DanhMucMySql.Cot(map, "CCCD")}, {DanhMucMySql.Cot(map, "GioiTinh")}, {DanhMucMySql.Cot(map, "MaKH")}, {DanhMucMySql.Cot(map, "LoaiKH")}) VALUES (@MaHK, @HoTen, @NgaySinh, @CCCD, @GioiTinh, @MaKH, @LoaiKH)";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHK", (object?)item.MaHK ?? DBNull.Value);
            command.Parameters.AddWithValue("@HoTen", (object?)item.HoTen ?? DBNull.Value);
            command.Parameters.AddWithValue("@NgaySinh", (object?)item.NgaySinh ?? DBNull.Value);
            command.Parameters.AddWithValue("@CCCD", (object?)item.CCCD ?? DBNull.Value);
            command.Parameters.AddWithValue("@GioiTinh", (object?)item.GioiTinh ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaKH", (object?)item.MaKH ?? DBNull.Value);
            command.Parameters.AddWithValue("@LoaiKH", (object?)item.LoaiKH ?? DBNull.Value);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin hành khách: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(HanhKhach item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaHK", "HoTen", "NgaySinh", "CCCD", "GioiTinh", "MaKH", "LoaiKH" };
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHK", (object?)item.MaHK ?? DBNull.Value);
            command.Parameters.AddWithValue("@HoTen", (object?)item.HoTen ?? DBNull.Value);
            command.Parameters.AddWithValue("@NgaySinh", (object?)item.NgaySinh ?? DBNull.Value);
            command.Parameters.AddWithValue("@CCCD", (object?)item.CCCD ?? DBNull.Value);
            command.Parameters.AddWithValue("@GioiTinh", (object?)item.GioiTinh ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaKH", (object?)item.MaKH ?? DBNull.Value);
            command.Parameters.AddWithValue("@LoaiKH", (object?)item.LoaiKH ?? DBNull.Value);
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
            throw new InvalidOperationException("Lỗi sửa thông tin hành khách: " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<HanhKhach>(connection, transaction, map);
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
            throw new InvalidOperationException("Lỗi xóa thông tin hành khách: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(HanhKhach item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(HanhKhach).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin hành khách: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    // Giu API Doc/Luu cu cho BUS va du lieu JSON.
    public List<HanhKhach> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<HanhKhach>();
        var data = JsonSerializer.Deserialize<List<HanhKhach>>(File.ReadAllText(duongDan));
        if (data == null || data.Any(x => x == null))
            throw new InvalidDataException("File hành khách không hợp lệ.");
        return data;
    }

    public void Luu(IReadOnlyCollection<HanhKhach> danhSach)
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
