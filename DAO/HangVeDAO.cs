using System.Data;
using System.Globalization;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class HangVeDAO
{
    public List<HangVeDTO> DocDS()
    {
        List<HangVeDTO> danhSach = new List<HangVeDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangVeDTO));
            string query = $"SELECT {DanhMucMySql.Cot(map, "MaHV")} AS `MaHV`, {DanhMucMySql.Cot(map, "TenHV")} AS `TenHV`, {DanhMucMySql.Cot(map, "HanhLyKg")} AS `HanhLyKg`, {DanhMucMySql.Cot(map, "DonGiaVe")} AS `DonGiaVe`";
            query += $" FROM {DanhMucMySql.Bang(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(HangVeDTO));
            while (reader.Read())
            {
                HangVeDTO item = new HangVeDTO();
                item.MaHV = reader.IsDBNull(reader.GetOrdinal("MaHV")) ? "" : reader.GetString("MaHV").TrimEnd();
                item.TenHV = reader.IsDBNull(reader.GetOrdinal("TenHV")) ? "" : reader.GetString("TenHV").TrimEnd();
                item.HanhLyKg = Convert.ToDecimal(reader["HanhLyKg"], CultureInfo.InvariantCulture);
                item.DonGiaVe = Convert.ToDecimal(reader["DonGiaVe"], CultureInfo.InvariantCulture);
                danhSach.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin hạng vé: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        return danhSach;
    }

    public void Them(HangVeDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangVeDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string query = $"INSERT INTO {DanhMucMySql.Bang(map)} ({DanhMucMySql.Cot(map, "MaHV")}, {DanhMucMySql.Cot(map, "TenHV")}, {DanhMucMySql.Cot(map, "HanhLyKg")}, {DanhMucMySql.Cot(map, "DonGiaVe")}) VALUES (@MaHV, @TenHV, @HanhLyKg, @DonGiaVe)";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHV", (object?)item.MaHV ?? DBNull.Value);
            command.Parameters.AddWithValue("@TenHV", (object?)item.TenHV ?? DBNull.Value);
            command.Parameters.AddWithValue("@HanhLyKg", item.HanhLyKg);
            command.Parameters.AddWithValue("@DonGiaVe", item.DonGiaVe);
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin hạng vé: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(HangVeDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangVeDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item);
            string[] cacCot = new[] { "MaHV", "TenHV", "HanhLyKg", "DonGiaVe" };
            string query = $"UPDATE {DanhMucMySql.Bang(map)} SET "
                + string.Join(", ", cacCot.Where(x => !map.KeyColumns.Contains(x)).Select(x => DanhMucMySql.Cot(map, x) + "=@" + x))
                + " WHERE " + DanhMucMySql.DieuKienKhoa(map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHV", (object?)item.MaHV ?? DBNull.Value);
            command.Parameters.AddWithValue("@TenHV", (object?)item.TenHV ?? DBNull.Value);
            command.Parameters.AddWithValue("@HanhLyKg", item.HanhLyKg);
            command.Parameters.AddWithValue("@DonGiaVe", item.DonGiaVe);
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
            throw new InvalidOperationException("Lỗi sửa thông tin hạng vé: " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangVeDTO));
            if (string.IsNullOrWhiteSpace(ma)) throw new ArgumentException("Vui lòng nhập mã cần xóa.");
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép. Hãy dùng Xoa(dto).");
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraBangKhoa<HangVeDTO>(connection, transaction, map);
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
            throw new InvalidOperationException("Lỗi xóa thông tin hạng vé: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Xoa(HangVeDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HangVeDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, item, xoa: true);
            string query = $"DELETE FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)}";
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            foreach (string khoa in map.KeyColumns)
                command.Parameters.AddWithValue("@" + khoa, typeof(HangVeDTO).GetProperty(khoa)!.GetValue(item) ?? DBNull.Value);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần xóa.");
            command.CommandText = query;
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin hạng vé: " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

}
