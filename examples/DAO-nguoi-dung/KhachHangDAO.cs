using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class KhachHangDAO
{
    public List<KhachHangDTO> DocDS()
    {
        List<KhachHangDTO> dskh = new List<KhachHangDTO>();
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "SELECT MaKH,HoTen,CCCD,SDT,Email,TaiKhoan,MatKhau,DiaChi FROM KhachHang";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                KhachHangDTO kh = new KhachHangDTO();
                kh.MaKH = reader.GetString("MaKH");
                kh.HoTen = reader.GetString("HoTen");
                kh.CCCD = reader.GetString("CCCD");
                kh.SDT = reader.GetString("SDT");
                kh.Email = reader.GetString("Email");
                kh.TaiKhoan = reader.GetString("TaiKhoan");
                kh.MatKhau = reader.GetString("MatKhau");
                kh.DiaChi = reader.GetString("DiaChi");
                dskh.Add(kh);
            }
            reader.Close();
        }
        catch
        {
            throw new Exception("Lỗi đọc thông tin khách hàng!");
        }
        finally
        {
            connection.Close();
        }
        return dskh;
    }

    public void Them(KhachHangDTO kh)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "INSERT INTO KhachHang(MaKH,HoTen,CCCD,SDT,Email,TaiKhoan,MatKhau,DiaChi) VALUES (@MaKH,@HoTen,@CCCD,@SDT,@Email,@TaiKhoan,@MatKhau,@DiaChi)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaKH", kh.MaKH);
            command.Parameters.AddWithValue("@HoTen", kh.HoTen);
            command.Parameters.AddWithValue("@CCCD", kh.CCCD);
            command.Parameters.AddWithValue("@SDT", kh.SDT);
            command.Parameters.AddWithValue("@Email", kh.Email);
            command.Parameters.AddWithValue("@TaiKhoan", kh.TaiKhoan);
            command.Parameters.AddWithValue("@MatKhau", kh.MatKhau);
            command.Parameters.AddWithValue("@DiaChi", kh.DiaChi);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi ghi thông tin khách hàng!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(KhachHangDTO kh)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "UPDATE KhachHang SET HoTen=@HoTen, CCCD=@CCCD, SDT=@SDT, Email=@Email, TaiKhoan=@TaiKhoan, MatKhau=@MatKhau, DiaChi=@DiaChi WHERE MaKH=@MaKH";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@HoTen", kh.HoTen);
            command.Parameters.AddWithValue("@CCCD", kh.CCCD);
            command.Parameters.AddWithValue("@SDT", kh.SDT);
            command.Parameters.AddWithValue("@Email", kh.Email);
            command.Parameters.AddWithValue("@TaiKhoan", kh.TaiKhoan);
            command.Parameters.AddWithValue("@MatKhau", kh.MatKhau);
            command.Parameters.AddWithValue("@DiaChi", kh.DiaChi);
            command.Parameters.AddWithValue("@MaKH", kh.MaKH);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi sửa thông tin khách hàng!");
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
            string query = "DELETE FROM KhachHang WHERE MaKH=@MaKH";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaKH", ma);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi xóa thông tin khách hàng!");
        }
        finally
        {
            connection.Close();
        }
    }
}
