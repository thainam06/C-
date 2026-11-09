using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class HangHangKhongDAO
{
    public List<HangHangKhongDTO> DocDS()
    {
        List<HangHangKhongDTO> dshhk = new List<HangHangKhongDTO>();
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "SELECT MaHHK,TenHang FROM HangHangKhong";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string ma = reader.GetString(0);
                string ten = reader.GetString("TenHang");
                HangHangKhongDTO hhk = new HangHangKhongDTO();
                hhk.MaHHK = ma;
                hhk.TenHang = ten;
                dshhk.Add(hhk);
            }
            reader.Close();
        }
        catch
        {
            throw new Exception("Lỗi đọc thông tin hãng hàng không!");
        }
        finally
        {
            connection.Close();
        }
        return dshhk;
    }

    public void Them(HangHangKhongDTO hhk)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "INSERT INTO HangHangKhong VALUES ('" + hhk.MaHHK + "','" + hhk.TenHang + "')";
            MySqlCommand command = new MySqlCommand(query, connection);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi ghi thông tin hãng hàng không!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(HangHangKhongDTO hhk)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "UPDATE HangHangKhong SET TenHang='" + hhk.TenHang + "' WHERE MaHHK='" + hhk.MaHHK + "'";
            MySqlCommand command = new MySqlCommand(query, connection);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi sửa thông tin hãng hàng không!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(string ma)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "DELETE FROM HangHangKhong WHERE MaHHK='" + ma + "'";
            MySqlCommand command = new MySqlCommand(query, connection);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi xóa thông tin hãng hàng không!");
        }
        finally
        {
            connection.Close();
        }
    }
}
