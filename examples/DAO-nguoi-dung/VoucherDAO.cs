using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class VoucherDAO
{
    public List<VoucherDTO> DocDS()
    {
        List<VoucherDTO> dsvc = new List<VoucherDTO>();
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "SELECT MaVoucher,MaCTKM,GiamGia FROM Voucher";
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                VoucherDTO vc = new VoucherDTO();
                vc.MaVoucher = reader.GetString("MaVoucher");
                vc.MaCTKM = reader.GetString("MaCTKM");
                vc.GiamGia = reader.GetDouble("GiamGia");
                dsvc.Add(vc);
            }
            reader.Close();
        }
        catch
        {
            throw new Exception("Lỗi đọc thông tin voucher!");
        }
        finally
        {
            connection.Close();
        }
        return dsvc;
    }

    public void Them(VoucherDTO vc)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "INSERT INTO Voucher(MaVoucher,MaCTKM,GiamGia) VALUES (@MaVoucher,@MaCTKM,@GiamGia)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaVoucher", vc.MaVoucher);
            command.Parameters.AddWithValue("@MaCTKM", vc.MaCTKM);
            command.Parameters.AddWithValue("@GiamGia", vc.GiamGia);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi ghi thông tin voucher!");
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(VoucherDTO vc)
    {
        MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            string query = "UPDATE Voucher SET MaCTKM=@MaCTKM, GiamGia=@GiamGia WHERE MaVoucher=@MaVoucher";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaCTKM", vc.MaCTKM);
            command.Parameters.AddWithValue("@GiamGia", vc.GiamGia);
            command.Parameters.AddWithValue("@MaVoucher", vc.MaVoucher);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi sửa thông tin voucher!");
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
            string query = "DELETE FROM Voucher WHERE MaVoucher=@MaVoucher";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@MaVoucher", ma);
            int r = command.ExecuteNonQuery();
        }
        catch
        {
            throw new Exception("Lỗi xóa thông tin voucher!");
        }
        finally
        {
            connection.Close();
        }
    }
}
