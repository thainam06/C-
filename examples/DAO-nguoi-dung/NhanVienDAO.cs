using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class NhanVienDAO
{
    public List<NhanVienDTO> DocDS()
    {
        List<NhanVienDTO> dsnv = new List<NhanVienDTO>();
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "SELECT MaNV,Ho,Ten,ChucVu FROM NhanVien";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                NhanVienDTO nv = new NhanVienDTO();
                nv.MaNV = reader.GetString("MaNV");
                nv.Ho = reader.GetString("Ho");
                nv.Ten = reader.GetString("Ten");
                nv.ChucVu = reader.GetString("ChucVu");
                dsnv.Add(nv);
            }
            reader.Close();
        }
        catch
        {
            throw new Exception("Lỗi đọc thông tin nhân viên!");
        }
        finally
        {
            connection.Close();
        }
        return dsnv;
    }

    public void Them(NhanVienDTO nv)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "INSERT INTO NhanVien(MaNV,Ho,Ten,ChucVu) VALUES (@MaNV,@Ho,@Ten,@ChucVu)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaNV", nv.MaNV);
            command.Parameters.AddWithValue("@Ho", nv.Ho);
            command.Parameters.AddWithValue("@Ten", nv.Ten);
            command.Parameters.AddWithValue("@ChucVu", nv.ChucVu);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi ghi thông tin nhân viên!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(NhanVienDTO nv)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "UPDATE NhanVien SET Ho=@Ho, Ten=@Ten, ChucVu=@ChucVu WHERE MaNV=@MaNV";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Ho", nv.Ho);
            command.Parameters.AddWithValue("@Ten", nv.Ten);
            command.Parameters.AddWithValue("@ChucVu", nv.ChucVu);
            command.Parameters.AddWithValue("@MaNV", nv.MaNV);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi sửa thông tin nhân viên!");
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
            string query = "DELETE FROM NhanVien WHERE MaNV=@MaNV";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaNV", ma);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi xóa thông tin nhân viên!");
        }
        finally
        {
            connection.Close();
        }
    }
}
