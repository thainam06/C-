using System.Data;
using MySqlConnector;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class CTKMDAO : JsonDanhSachDAO<CTKM>
{
    public CTKMDAO(string? duongDan = null) : base("ctkm.json", duongDan) { }

    public List<CTKM> DocDS()
    {
        List<CTKM> danhSach = new List<CTKM>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKM));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaCTKM")} AS `MaCTKM`, {DanhMucMySql.Cot(map, "TenCTKM")} AS `TenCTKM`, {DanhMucMySql.Cot(map, "NgayBatDau")} AS `NgayBatDau`, {DanhMucMySql.Cot(map, "NgayKetThuc")} AS `NgayKetThuc`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(CTKM));
            while (reader.Read())
            {
                CTKM item = new CTKM();
                item.MaCTKM = reader.IsDBNull(reader.GetOrdinal("MaCTKM")) ? "" : reader.GetString("MaCTKM").TrimEnd();
                item.TenCTKM = reader.IsDBNull(reader.GetOrdinal("TenCTKM")) ? "" : reader.GetString("TenCTKM").TrimEnd();
                item.NgayBatDau = reader.GetDateTime("NgayBatDau");
                item.NgayKetThuc = reader.GetDateTime("NgayKetThuc");
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin chương trình khuyến mãi: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        GhiNhanDanhSach(danhSach);
        return danhSach;
    }

    public void Them(CTKM item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKM));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaCTKM")}, {DanhMucMySql.Cot(map, "TenCTKM")}, {DanhMucMySql.Cot(map, "NgayBatDau")}, {DanhMucMySql.Cot(map, "NgayKetThuc")}) VALUES (@MaCTKM, @TenCTKM, @NgayBatDau, @NgayKetThuc)";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaCTKM", (object?)item.MaCTKM ?? DBNull.Value);
            command.Parameters.AddWithValue("@TenCTKM", (object?)item.TenCTKM ?? DBNull.Value);
            command.Parameters.AddWithValue("@NgayBatDau", item.NgayBatDau);
            command.Parameters.AddWithValue("@NgayKetThuc", item.NgayKetThuc);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin chương trình khuyến mãi: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(CTKM item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKM));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaCTKM", "TenCTKM", "NgayBatDau", "NgayKetThuc" };
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaCTKM", (object?)item.MaCTKM ?? DBNull.Value);
            command.Parameters.AddWithValue("@TenCTKM", (object?)item.TenCTKM ?? DBNull.Value);
            command.Parameters.AddWithValue("@NgayBatDau", item.NgayBatDau);
            command.Parameters.AddWithValue("@NgayKetThuc", item.NgayKetThuc);
            // Khoa dong truoc khi UPDATE; sua khong doi du lieu van hop le.
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa.");
            command.CommandText = query;
            if (cacCot.All(map.KeyColumns.Contains)) { transaction.Commit(); return; }
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin chương trình khuyến mãi: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(string ma)
    {
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKM));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<CTKM>(connection, transaction, map);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@" + map.KeyColumns[0], ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin chương trình khuyến mãi: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(CTKM item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(CTKM));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(CTKM).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin chương trình khuyến mãi: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

}
