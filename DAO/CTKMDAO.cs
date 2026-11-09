using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class CTKMDAO : JsonDanhSachDAO<CTKMDTO>
{
    public CTKMDAO(string? duongDan = null) : base("ctkm.json", duongDan) { }

    public List<CTKMDTO> DocDS()
    {
        List<CTKMDTO> dsctkm = new List<CTKMDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKMDTO));
            string query = "SELECT MaCTKM,TenCTKM,NgayBatDau,NgayKetThuc FROM CTKM";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(CTKMDTO));
            while (reader.Read())
            {
                CTKMDTO ctkm = new CTKMDTO();
                ctkm.MaCTKM = (reader.IsDBNull(reader.GetOrdinal("MaCTKM")) ? "" : reader.GetString("MaCTKM").TrimEnd());
                ctkm.TenCTKM = (reader.IsDBNull(reader.GetOrdinal("TenCTKM")) ? "" : reader.GetString("TenCTKM").TrimEnd());
                ctkm.NgayBatDau = reader.GetDateTime("NgayBatDau");
                ctkm.NgayKetThuc = reader.GetDateTime("NgayKetThuc");
                dsctkm.Add(ctkm);
            }
            reader.Close();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin chương trình khuyến mãi!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        GhiNhanDanhSach(dsctkm);
        return dsctkm;
    }

    public void Them(CTKMDTO ctkm)
    {
        ArgumentNullException.ThrowIfNull(ctkm);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKMDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, ctkm);
            string query = "INSERT INTO CTKM(MaCTKM,TenCTKM,NgayBatDau,NgayKetThuc) VALUES (@MaCTKM,@TenCTKM,@NgayBatDau,@NgayKetThuc)";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaCTKM", ctkm.MaCTKM);
            command.Parameters.AddWithValue("@TenCTKM", ctkm.TenCTKM);
            command.Parameters.AddWithValue("@NgayBatDau", ctkm.NgayBatDau);
            command.Parameters.AddWithValue("@NgayKetThuc", ctkm.NgayKetThuc);
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin chương trình khuyến mãi!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(CTKMDTO ctkm)
    {
        ArgumentNullException.ThrowIfNull(ctkm);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKMDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, ctkm);
            string query = "UPDATE CTKM SET TenCTKM=@TenCTKM, NgayBatDau=@NgayBatDau, NgayKetThuc=@NgayKetThuc WHERE MaCTKM=@MaCTKM";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@TenCTKM", ctkm.TenCTKM);
            command.Parameters.AddWithValue("@NgayBatDau", ctkm.NgayBatDau);
            command.Parameters.AddWithValue("@NgayKetThuc", ctkm.NgayKetThuc);
            command.Parameters.AddWithValue("@MaCTKM", ctkm.MaCTKM);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin chương trình khuyến mãi!" + " " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKMDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép; cần cung cấp đủ khóa để xóa.");
            DanhMucMySql.KiemTraBangKhoa<CTKMDTO>(connection, transaction, map);
            string query = "DELETE FROM CTKM WHERE MaCTKM=@MaCTKM";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaCTKM", ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin chương trình khuyến mãi!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }
    public void Xoa(CTKMDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Xoa(item.MaCTKM);
    }

}
