using System.Data;
using MySqlConnector;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class VoucherDAO : JsonDanhSachDAO<Voucher>
{
    public VoucherDAO(string? duongDan = null) : base("voucher.json", duongDan) { }

    public List<Voucher> DocDS()
    {
        List<Voucher> dsvc = new List<Voucher>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(Voucher));
            string query = "SELECT MaVoucher,MaCTKM,GiamGia FROM Voucher";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(Voucher));
            while (reader.Read())
            {
                Voucher vc = new Voucher();
                vc.MaVoucher = (reader.IsDBNull(reader.GetOrdinal("MaVoucher")) ? "" : reader.GetString("MaVoucher").TrimEnd());
                vc.MaCTKM = (reader.IsDBNull(reader.GetOrdinal("MaCTKM")) ? "" : reader.GetString("MaCTKM").TrimEnd());
                vc.GiamGia = reader.GetDecimal("GiamGia");
                dsvc.Add(vc);
            }
            reader.Close();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin voucher!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        GhiNhanDanhSach(dsvc);
        return dsvc;
    }

    public void Them(Voucher vc)
    {
        ArgumentNullException.ThrowIfNull(vc);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(Voucher));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, vc);
            string query = "INSERT INTO Voucher(MaVoucher,MaCTKM,GiamGia) VALUES (@MaVoucher,@MaCTKM,@GiamGia)";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaVoucher", vc.MaVoucher);
            command.Parameters.AddWithValue("@MaCTKM", vc.MaCTKM);
            command.Parameters.AddWithValue("@GiamGia", vc.GiamGia);
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin voucher!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(Voucher vc)
    {
        ArgumentNullException.ThrowIfNull(vc);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(Voucher));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, vc);
            string query = "UPDATE Voucher SET MaCTKM=@MaCTKM, GiamGia=@GiamGia WHERE MaVoucher=@MaVoucher";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaCTKM", vc.MaCTKM);
            command.Parameters.AddWithValue("@GiamGia", vc.GiamGia);
            command.Parameters.AddWithValue("@MaVoucher", vc.MaVoucher);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin voucher!" + " " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(Voucher));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép; cần cung cấp đủ khóa để xóa.");
            DanhMucMySql.KiemTraBangKhoa<Voucher>(connection, transaction, map);
            string query = "DELETE FROM Voucher WHERE MaVoucher=@MaVoucher";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaVoucher", ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin voucher!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }
    public void Xoa(Voucher item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Xoa(item.MaVoucher);
    }

}
