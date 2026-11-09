using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class GheDAO
{
    public List<GheDTO> DocDS()
    {
        List<GheDTO> danhSach = new List<GheDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(GheDTO));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaGhe")} AS `MaGhe`, {DanhMucMySql.Cot(map, "MaMayBay")} AS `MaMayBay`, {DanhMucMySql.Cot(map, "MaHV")} AS `MaHV`";
            if (map.Columns.ContainsKey("LoaiGhe")) query += $", {DanhMucMySql.Cot(map, "LoaiGhe")} AS `LoaiGhe`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(GheDTO));
            while (reader.Read())
            {
                GheDTO item = new GheDTO();
                item.MaGhe = reader.IsDBNull(reader.GetOrdinal("MaGhe")) ? "" : reader.GetString("MaGhe").TrimEnd();
                item.MaMayBay = reader.IsDBNull(reader.GetOrdinal("MaMayBay")) ? "" : reader.GetString("MaMayBay").TrimEnd();
                item.MaHV = reader.IsDBNull(reader.GetOrdinal("MaHV")) ? "" : reader.GetString("MaHV").TrimEnd();
                item.LoaiGhe = map.Columns.ContainsKey("LoaiGhe") ? (reader.IsDBNull(reader.GetOrdinal("LoaiGhe")) ? "" : reader.GetString("LoaiGhe").TrimEnd()) : GheDTO.Thuong;
                CSDLListDAO.ApDungCotSuyDien(item, map);
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin ghế: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        return danhSach;
    }

    public void Them(GheDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(GheDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaGhe")}, {DanhMucMySql.Cot(map, "MaMayBay")}, {DanhMucMySql.Cot(map, "MaHV")}";
            if (map.Columns.ContainsKey("LoaiGhe")) query += $", {DanhMucMySql.Cot(map, "LoaiGhe")}";
            query += ") VALUES (@MaGhe, @MaMayBay, @MaHV";
            if (map.Columns.ContainsKey("LoaiGhe")) query += ", @LoaiGhe";
            query += ")";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaGhe", (object?)item.MaGhe ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaMayBay", (object?)item.MaMayBay ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaHV", (object?)item.MaHV ?? DBNull.Value);
            if (map.Columns.ContainsKey("LoaiGhe")) command.Parameters.AddWithValue("@LoaiGhe", (object?)item.LoaiGhe ?? DBNull.Value);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin ghế: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(GheDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(GheDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaGhe", "MaMayBay", "MaHV" };
            if (map.Columns.ContainsKey("LoaiGhe")) cacCot = cacCot.Append("LoaiGhe").ToArray();
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaGhe", (object?)item.MaGhe ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaMayBay", (object?)item.MaMayBay ?? DBNull.Value);
            command.Parameters.AddWithValue("@MaHV", (object?)item.MaHV ?? DBNull.Value);
            if (map.Columns.ContainsKey("LoaiGhe")) command.Parameters.AddWithValue("@LoaiGhe", (object?)item.LoaiGhe ?? DBNull.Value);
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
            throw new InvalidOperationException("Lỗi sửa thông tin ghế: " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(GheDTO));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<GheDTO>(connection, transaction, map);
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
            throw new InvalidOperationException("Lỗi xóa thông tin ghế: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(GheDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(GheDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(GheDTO).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin ghế: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

}
