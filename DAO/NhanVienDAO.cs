using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class NhanVienDAO : JsonDanhSachDAO<NhanVienDTO>
{
    public NhanVienDAO(string? duongDan = null) : base("nhanvien.json", duongDan) { }

    public List<NhanVienDTO> DocDS()
    {
        List<NhanVienDTO> dsnv = new List<NhanVienDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVienDTO));
            string query = "SELECT MaNV,Ho,Ten,ChucVu FROM NhanVien";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(NhanVienDTO));
            while (reader.Read())
            {
                NhanVienDTO nv = new NhanVienDTO();
                nv.MaNV = (reader.IsDBNull(reader.GetOrdinal("MaNV")) ? "" : reader.GetString("MaNV").TrimEnd());
                nv.Ho = (reader.IsDBNull(reader.GetOrdinal("Ho")) ? "" : reader.GetString("Ho").TrimEnd());
                nv.Ten = (reader.IsDBNull(reader.GetOrdinal("Ten")) ? "" : reader.GetString("Ten").TrimEnd());
                nv.ChucVu = (reader.IsDBNull(reader.GetOrdinal("ChucVu")) ? "" : reader.GetString("ChucVu").TrimEnd());
                dsnv.Add(nv);
            }
            reader.Close();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin nhân viên!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        GhiNhanDanhSach(dsnv);
        return dsnv;
    }

    public void Them(NhanVienDTO nv)
    {
        ArgumentNullException.ThrowIfNull(nv);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVienDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, nv);
            string query = "INSERT INTO NhanVien(MaNV,Ho,Ten,ChucVu) VALUES (@MaNV,@Ho,@Ten,@ChucVu)";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaNV", nv.MaNV);
            command.Parameters.AddWithValue("@Ho", nv.Ho);
            command.Parameters.AddWithValue("@Ten", nv.Ten);
            command.Parameters.AddWithValue("@ChucVu", nv.ChucVu);
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin nhân viên!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(NhanVienDTO nv)
    {
        ArgumentNullException.ThrowIfNull(nv);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVienDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, nv);
            string query = "UPDATE NhanVien SET Ho=@Ho, Ten=@Ten, ChucVu=@ChucVu WHERE MaNV=@MaNV";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@Ho", nv.Ho);
            command.Parameters.AddWithValue("@Ten", nv.Ten);
            command.Parameters.AddWithValue("@ChucVu", nv.ChucVu);
            command.Parameters.AddWithValue("@MaNV", nv.MaNV);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin nhân viên!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(string ma)
    {
        if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(NhanVienDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép; cần cung cấp đủ khóa để xóa.");
            DanhMucMySql.KiemTraBangKhoa<NhanVienDTO>(connection, transaction, map);
            string query = "DELETE FROM NhanVien WHERE MaNV=@MaNV";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaNV", ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin nhân viên!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }
    public void Xoa(NhanVienDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Xoa(item.MaNV);
    }

}
