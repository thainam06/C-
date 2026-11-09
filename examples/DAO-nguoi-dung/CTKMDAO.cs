using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class CTKMDAO
{
    public List<CTKMDTO> DocDS()
    {
        List<CTKMDTO> dsctkm = new List<CTKMDTO>();
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "SELECT MaCTKM,TenCTKM,NgayBatDau,NgayKetThuc FROM CTKM";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                CTKMDTO ctkm = new CTKMDTO();
                ctkm.MaCTKM = reader.GetString("MaCTKM");
                ctkm.TenCTKM = reader.GetString("TenCTKM");
                ctkm.NgayBatDau = reader.GetDateTime("NgayBatDau");
                ctkm.NgayKetThuc = reader.GetDateTime("NgayKetThuc");
                dsctkm.Add(ctkm);
            }
            reader.Close();
        }
        catch
        {
            throw new Exception("Lỗi đọc thông tin chương trình khuyến mãi!");
        }
        finally
        {
            connection.Close();
        }
        return dsctkm;
    }

    public void Them(CTKMDTO ctkm)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "INSERT INTO CTKM(MaCTKM,TenCTKM,NgayBatDau,NgayKetThuc) VALUES (@MaCTKM,@TenCTKM,@NgayBatDau,@NgayKetThuc)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaCTKM", ctkm.MaCTKM);
            command.Parameters.AddWithValue("@TenCTKM", ctkm.TenCTKM);
            command.Parameters.AddWithValue("@NgayBatDau", ctkm.NgayBatDau);
            command.Parameters.AddWithValue("@NgayKetThuc", ctkm.NgayKetThuc);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi ghi thông tin chương trình khuyến mãi!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(CTKMDTO ctkm)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "UPDATE CTKM SET TenCTKM=@TenCTKM, NgayBatDau=@NgayBatDau, NgayKetThuc=@NgayKetThuc WHERE MaCTKM=@MaCTKM";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@TenCTKM", ctkm.TenCTKM);
            command.Parameters.AddWithValue("@NgayBatDau", ctkm.NgayBatDau);
            command.Parameters.AddWithValue("@NgayKetThuc", ctkm.NgayKetThuc);
            command.Parameters.AddWithValue("@MaCTKM", ctkm.MaCTKM);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi sửa thông tin chương trình khuyến mãi!");
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
            string query = "DELETE FROM CTKM WHERE MaCTKM=@MaCTKM";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaCTKM", ma);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi xóa thông tin chương trình khuyến mãi!");
        }
        finally
        {
            connection.Close();
        }
    }
}
