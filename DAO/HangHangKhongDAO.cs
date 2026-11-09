using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class HangHangKhongDAO
{
    public List<HangHangKhongDTO> DocDS()
    {
        List<HangHangKhongDTO> dshhk = new List<HangHangKhongDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangHangKhongDTO));
            string query = "SELECT MaHHK,TenHang FROM HangHangKhong";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(HangHangKhongDTO));
            while (reader.Read())
            {
                HangHangKhongDTO hhk = new HangHangKhongDTO();
                hhk.MaHHK = reader.GetString("MaHHK").TrimEnd();
                hhk.TenHang = reader.IsDBNull(reader.GetOrdinal("TenHang")) ? "" : reader.GetString("TenHang").TrimEnd();
                dshhk.Add(hhk);
            }
            reader.Close();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin hãng hàng không! " + ex.Message, ex);
        }
        finally { connection.Close(); }
        return dshhk;
    }

    public void Them(HangHangKhongDTO hhk)
    {
        ArgumentNullException.ThrowIfNull(hhk);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangHangKhongDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, hhk);
            string query = "INSERT INTO HangHangKhong(MaHHK,TenHang) VALUES (@MaHHK,@TenHang)";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHHK", hhk.MaHHK);
            command.Parameters.AddWithValue("@TenHang", hhk.TenHang);
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex) { throw new InvalidOperationException("Lỗi ghi thông tin hãng hàng không! " + ex.Message, ex); }
        finally { connection.Close(); }
    }

    public void Sua(HangHangKhongDTO hhk)
    {
        ArgumentNullException.ThrowIfNull(hhk);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangHangKhongDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, hhk);
            string query = "UPDATE HangHangKhong SET TenHang=@TenHang WHERE MaHHK=@MaHHK";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@TenHang", hhk.TenHang);
            command.Parameters.AddWithValue("@MaHHK", hhk.MaHHK);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy hãng hàng không cần sửa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex) { throw new InvalidOperationException("Lỗi sửa thông tin hãng hàng không! " + ex.Message, ex); }
        finally { connection.Close(); }
    }

    public void Xoa(string ma)
    {
        if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã hãng.");
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangHangKhongDTO));
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Cần đủ các cột khóa để xóa hãng.");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<HangHangKhongDTO>(connection, transaction, map);
            string query = "DELETE FROM HangHangKhong WHERE MaHHK=@MaHHK";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHHK", ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy hãng hàng không cần xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex) { throw new InvalidOperationException("Lỗi xóa thông tin hãng hàng không! " + ex.Message, ex); }
        finally { connection.Close(); }
    }

    public void Xoa(HangHangKhongDTO hhk)
    {
        ArgumentNullException.ThrowIfNull(hhk);
        Xoa(hhk.MaHHK);
    }
}
