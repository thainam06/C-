using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;
namespace VeMayBay.DAO;
public class ThamSoDAO : JsonDanhSachDAO<ThamSoDTO>
{
    public ThamSoDAO(string? path = null) : base("thamso.json", path) { }

    public List<ThamSoDTO> DocDS()
    {
        List<ThamSoDTO> danhSach = new List<ThamSoDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSoDTO));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaThamSo")} AS `MaThamSo`, {DanhMucMySql.Cot(map, "GiaTri")} AS `GiaTri`, {DanhMucMySql.Cot(map, "MoTa")} AS `MoTa`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(ThamSoDTO));
            while (reader.Read())
            {
                ThamSoDTO item = new ThamSoDTO();
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

    public void Them(ThamSoDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSoDTO));
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

    public void Sua(ThamSoDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSoDTO));
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSoDTO));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<ThamSoDTO>(connection, transaction, map);
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

    public void Xoa(ThamSoDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(ThamSoDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(ThamSoDTO).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
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
