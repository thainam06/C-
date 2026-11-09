using System.Data;
using MySqlConnector;
using QuanLyMayBay.DTO;
using System.Text.Json;

namespace QuanLyMayBay.DAO;

public class HanhKhachDAO
{
    private readonly string duongDan;
    private readonly PhienBangSql<HanhKhach> sql = new();

    public HanhKhachDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "hanhkhach.json"));
    }

    public List<HanhKhach> DocDS()
    {
        List<HanhKhach> dshk = new List<HanhKhach>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            string query = "SELECT MaHK,HoTen,NgaySinh,CCCD,GioiTinh,MaKH,LoaiKH FROM HanhKhach";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(HanhKhach));
            while (reader.Read())
            {
                HanhKhach hk = new HanhKhach();
                hk.MaHK = (reader.IsDBNull(reader.GetOrdinal("MaHK")) ? "" : reader.GetString("MaHK").TrimEnd());
                hk.HoTen = (reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString("HoTen").TrimEnd());
                hk.NgaySinh = reader.IsDBNull(reader.GetOrdinal("NgaySinh")) ? null : reader.GetDateTime("NgaySinh");
                hk.CCCD = (reader.IsDBNull(reader.GetOrdinal("CCCD")) ? "" : reader.GetString("CCCD").TrimEnd());
                hk.GioiTinh = (reader.IsDBNull(reader.GetOrdinal("GioiTinh")) ? "" : reader.GetString("GioiTinh").TrimEnd());
                hk.MaKH = (reader.IsDBNull(reader.GetOrdinal("MaKH")) ? "" : reader.GetString("MaKH").TrimEnd());
                hk.LoaiKH = (reader.IsDBNull(reader.GetOrdinal("LoaiKH")) ? "" : reader.GetString("LoaiKH").TrimEnd());
                dshk.Add(hk);
            }
            reader.Close();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin hành khách!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        sql.GhiNhanDaDoc(dshk);
        return dshk;
    }

    public void Them(HanhKhach hk)
    {
        ArgumentNullException.ThrowIfNull(hk);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, hk);
            string query = "INSERT INTO HanhKhach(MaHK,HoTen,NgaySinh,CCCD,GioiTinh,MaKH,LoaiKH) VALUES (@MaHK,@HoTen,@NgaySinh,@CCCD,@GioiTinh,@MaKH,@LoaiKH)";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHK", hk.MaHK);
            command.Parameters.AddWithValue("@HoTen", hk.HoTen);
            command.Parameters.AddWithValue("@NgaySinh", (object?)hk.NgaySinh ?? DBNull.Value);
            command.Parameters.AddWithValue("@CCCD", hk.CCCD);
            command.Parameters.AddWithValue("@GioiTinh", hk.GioiTinh);
            command.Parameters.AddWithValue("@MaKH", hk.MaKH);
            command.Parameters.AddWithValue("@LoaiKH", hk.LoaiKH);
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin hành khách!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(HanhKhach hk)
    {
        ArgumentNullException.ThrowIfNull(hk);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, hk);
            string query = "UPDATE HanhKhach SET HoTen=@HoTen, NgaySinh=@NgaySinh, CCCD=@CCCD, GioiTinh=@GioiTinh, MaKH=@MaKH, LoaiKH=@LoaiKH WHERE MaHK=@MaHK";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@HoTen", hk.HoTen);
            command.Parameters.AddWithValue("@NgaySinh", (object?)hk.NgaySinh ?? DBNull.Value);
            command.Parameters.AddWithValue("@CCCD", hk.CCCD);
            command.Parameters.AddWithValue("@GioiTinh", hk.GioiTinh);
            command.Parameters.AddWithValue("@MaKH", hk.MaKH);
            command.Parameters.AddWithValue("@LoaiKH", hk.LoaiKH);
            command.Parameters.AddWithValue("@MaHK", hk.MaHK);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin hành khách!" + " " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhach));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép; cần cung cấp đủ khóa để xóa.");
            DanhMucMySql.KiemTraBangKhoa<HanhKhach>(connection, transaction, map);
            string query = "DELETE FROM HanhKhach WHERE MaHK=@MaHK";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaHK", ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin hành khách!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }
    public void Xoa(HanhKhach item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Xoa(item.MaHK);
    }

    // Giu API Doc/Luu cu cho BUS va du lieu JSON.
    public List<HanhKhach> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<HanhKhach>();
        var data = JsonSerializer.Deserialize<List<HanhKhach>>(File.ReadAllText(duongDan));
        if (data == null || data.Any(x => x == null))
            throw new InvalidDataException("File hành khách không hợp lệ.");
        return data;
    }

    public void Luu(IReadOnlyCollection<HanhKhach> danhSach)
    {
        if (sql.ThuLuu(danhSach)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(duongDan)!);
        var fileTam = duongDan + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(fileTam, JsonSerializer.Serialize(danhSach,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(fileTam, duongDan, overwrite: true);
        }
        finally
        {
            if (File.Exists(fileTam)) File.Delete(fileTam);
        }
    }

}
