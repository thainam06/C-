using System.Data;
using MySqlConnector;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class NhanVienDAO : JsonDanhSachDAO<NhanVien>
{
    public NhanVienDAO(string? duongDan = null) : base("nhanvien.json", duongDan) { }

    public List<NhanVien> DocDS()
    {
        List<NhanVien> danhSach = new List<NhanVien>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVien));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaNV")} AS `MaNV`, {DanhMucMySql.Cot(map, "Ho")} AS `Ho`, {DanhMucMySql.Cot(map, "Ten")} AS `Ten`, {DanhMucMySql.Cot(map, "ChucVu")} AS `ChucVu`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(NhanVien));
            while (reader.Read())
            {
                NhanVien item = new NhanVien();
                item.MaNV = reader.IsDBNull(reader.GetOrdinal("MaNV")) ? "" : reader.GetString("MaNV").TrimEnd();
                item.Ho = reader.IsDBNull(reader.GetOrdinal("Ho")) ? "" : reader.GetString("Ho").TrimEnd();
                item.Ten = reader.IsDBNull(reader.GetOrdinal("Ten")) ? "" : reader.GetString("Ten").TrimEnd();
                item.ChucVu = reader.IsDBNull(reader.GetOrdinal("ChucVu")) ? "" : reader.GetString("ChucVu").TrimEnd();
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin nhân viên: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        GhiNhanDanhSach(danhSach);
        return danhSach;
    }

    public void Them(NhanVien item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVien));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaNV")}, {DanhMucMySql.Cot(map, "Ho")}, {DanhMucMySql.Cot(map, "Ten")}, {DanhMucMySql.Cot(map, "ChucVu")}) VALUES (@MaNV, @Ho, @Ten, @ChucVu)";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaNV", (object?)item.MaNV ?? DBNull.Value);
            command.Parameters.AddWithValue("@Ho", (object?)item.Ho ?? DBNull.Value);
            command.Parameters.AddWithValue("@Ten", (object?)item.Ten ?? DBNull.Value);
            command.Parameters.AddWithValue("@ChucVu", (object?)item.ChucVu ?? DBNull.Value);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin nhân viên: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(NhanVien item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVien));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaNV", "Ho", "Ten", "ChucVu" };
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaNV", (object?)item.MaNV ?? DBNull.Value);
            command.Parameters.AddWithValue("@Ho", (object?)item.Ho ?? DBNull.Value);
            command.Parameters.AddWithValue("@Ten", (object?)item.Ten ?? DBNull.Value);
            command.Parameters.AddWithValue("@ChucVu", (object?)item.ChucVu ?? DBNull.Value);
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
            throw new InvalidOperationException("Lỗi sửa thông tin nhân viên: " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVien));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<NhanVien>(connection, transaction, map);
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
            throw new InvalidOperationException("Lỗi xóa thông tin nhân viên: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(NhanVien item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVien));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(NhanVien).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin nhân viên: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

}
