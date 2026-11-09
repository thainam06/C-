using System.Data;
using MySqlConnector;
using QuanLyMayBay.DTO;
namespace QuanLyMayBay.DAO;
public class ThamSoDAO : JsonDanhSachDAO<ThamSo>
{
    public ThamSoDAO(string? path = null) : base("thamso.json", path) { }

    public List<ThamSo> DocDS()
    {
        List<ThamSo> danhSach = new List<ThamSo>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSo));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaThamSo")} AS `MaThamSo`, {DanhMucMySql.Cot(map, "GiaTri")} AS `GiaTri`, {DanhMucMySql.Cot(map, "MoTa")} AS `MoTa`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(ThamSo));
            while (reader.Read())
            {
                ThamSo item = new ThamSo();
                item.MaThamSo = reader.IsDBNull(reader.GetOrdinal("MaThamSo")) ? "" : reader.GetString("MaThamSo").TrimEnd();
                item.GiaTri = reader.IsDBNull(reader.GetOrdinal("GiaTri")) ? "" : reader.GetString("GiaTri").TrimEnd();
                item.MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? "" : reader.GetString("MoTa").TrimEnd();
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin tham số: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        GhiNhanDanhSach(danhSach);
        return danhSach;
    }

    public void Them(ThamSo item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSo));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaThamSo")}, {DanhMucMySql.Cot(map, "GiaTri")}, {DanhMucMySql.Cot(map, "MoTa")}) VALUES (@MaThamSo, @GiaTri, @MoTa)";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaThamSo", (object?)item.MaThamSo ?? DBNull.Value);
            command.Parameters.AddWithValue("@GiaTri", (object?)item.GiaTri ?? DBNull.Value);
            command.Parameters.AddWithValue("@MoTa", (object?)item.MoTa ?? DBNull.Value);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin tham số: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(ThamSo item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSo));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaThamSo", "GiaTri", "MoTa" };
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaThamSo", (object?)item.MaThamSo ?? DBNull.Value);
            command.Parameters.AddWithValue("@GiaTri", (object?)item.GiaTri ?? DBNull.Value);
            command.Parameters.AddWithValue("@MoTa", (object?)item.MoTa ?? DBNull.Value);
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
            throw new InvalidOperationException("Lỗi sửa thông tin tham số: " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSo));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<ThamSo>(connection, transaction, map);
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
            throw new InvalidOperationException("Lỗi xóa thông tin tham số: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(ThamSo item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSo));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(ThamSo).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin tham số: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

}
