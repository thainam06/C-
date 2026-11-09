using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class HanhKhachDAO
{
    public List<HanhKhachDTO> DocDS()
    {
        List<HanhKhachDTO> dshk = new List<HanhKhachDTO>();
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "SELECT MaHK,HoTen,NgaySinh,CCCD,GioiTinh,MaKH,LoaiKH FROM HanhKhach";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                HanhKhachDTO hk = new HanhKhachDTO();
                hk.MaHK = reader.GetString("MaHK");
                hk.HoTen = reader.GetString("HoTen");
                hk.NgaySinh = reader.GetDateTime("NgaySinh");
                hk.CCCD = reader.GetString("CCCD");
                hk.GioiTinh = reader.GetString("GioiTinh");
                hk.MaKH = reader.GetString("MaKH");
                hk.LoaiKH = reader.GetString("LoaiKH");
                dshk.Add(hk);
            }
            reader.Close();
        }
        catch
        {
            throw new Exception("Lỗi đọc thông tin hành khách!");
        }
        finally
        {
            connection.Close();
        }
        return dshk;
    }

    public void Them(HanhKhachDTO hk)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "INSERT INTO HanhKhach(MaHK,HoTen,NgaySinh,CCCD,GioiTinh,MaKH,LoaiKH) VALUES (@MaHK,@HoTen,@NgaySinh,@CCCD,@GioiTinh,@MaKH,@LoaiKH)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaHK", hk.MaHK);
            command.Parameters.AddWithValue("@HoTen", hk.HoTen);
            command.Parameters.AddWithValue("@NgaySinh", hk.NgaySinh);
            command.Parameters.AddWithValue("@CCCD", hk.CCCD);
            command.Parameters.AddWithValue("@GioiTinh", hk.GioiTinh);
            command.Parameters.AddWithValue("@MaKH", hk.MaKH);
            command.Parameters.AddWithValue("@LoaiKH", hk.LoaiKH);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi ghi thông tin hành khách!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(HanhKhachDTO hk)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "UPDATE HanhKhach SET HoTen=@HoTen, NgaySinh=@NgaySinh, CCCD=@CCCD, GioiTinh=@GioiTinh, MaKH=@MaKH, LoaiKH=@LoaiKH WHERE MaHK=@MaHK";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@HoTen", hk.HoTen);
            command.Parameters.AddWithValue("@NgaySinh", hk.NgaySinh);
            command.Parameters.AddWithValue("@CCCD", hk.CCCD);
            command.Parameters.AddWithValue("@GioiTinh", hk.GioiTinh);
            command.Parameters.AddWithValue("@MaKH", hk.MaKH);
            command.Parameters.AddWithValue("@LoaiKH", hk.LoaiKH);
            command.Parameters.AddWithValue("@MaHK", hk.MaHK);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi sửa thông tin hành khách!");
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
            string query = "DELETE FROM HanhKhach WHERE MaHK=@MaHK";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaHK", ma);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi xóa thông tin hành khách!");
        }
        finally
        {
            connection.Close();
        }
    }
}
