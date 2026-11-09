using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;
using System.Text.Json;

namespace VeMayBay.DAO;

public class HanhKhachDAO
{
    private readonly string duongDan;
    private readonly PhienBangSql<HanhKhachDTO> sql = new();

    public HanhKhachDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "hanhkhach.json"));
    }

    public List<HanhKhachDTO> DocDS()
    {
        List<HanhKhachDTO> dshk = new List<HanhKhachDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhachDTO));
            string query = "SELECT MaHK,HoTen,NgaySinh,CCCD,GioiTinh,MaKH,LoaiKH FROM HanhKhach";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(HanhKhachDTO));
            while (reader.Read())
            {
                HanhKhachDTO hk = new HanhKhachDTO();
                hk.MaHK = (reader.IsDBNull(reader.GetOrdinal("MaHK")) ? "" : reader.GetString("MaHK").TrimEnd());
                hk.HoTen = (reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString("HoTen").TrimEnd());
                hk.NgaySinh = reader.IsDBNull(reader.GetOrdinal("NgaySinh"))
                    ? throw new InvalidDataException($"Hành khách {hk.MaHK} chưa có ngày sinh trong CSDL. DTO mới yêu cầu ngày sinh; cần bổ sung ngày đúng trước khi mở danh sách.")
                    : reader.GetDateTime("NgaySinh");
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

    public void Them(HanhKhachDTO hk)
    {
        ArgumentNullException.ThrowIfNull(hk);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhachDTO));
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

    public void Sua(HanhKhachDTO hk)
    {
        ArgumentNullException.ThrowIfNull(hk);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhachDTO));
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(HanhKhachDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép; cần cung cấp đủ khóa để xóa.");
            DanhMucMySql.KiemTraBangKhoa<HanhKhachDTO>(connection, transaction, map);
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
    public void Xoa(HanhKhachDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Xoa(item.MaHK);
    }

    // Giu API Doc/Luu cu cho BUS va du lieu JSON.
    public List<HanhKhachDTO> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<HanhKhachDTO>();
        var data = JsonSerializer.Deserialize<List<HanhKhachDTO>>(File.ReadAllText(duongDan));
        if (data == null || data.Any(x => x == null))
            throw new InvalidDataException("File hành khách không hợp lệ.");
        return data;
    }

    public void Luu(IReadOnlyCollection<HanhKhachDTO> danhSach)
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
