using System.Data;
using MySql.Data.MySqlClient;
using VeMayBay.DTO;
using System.Text.Json;

namespace VeMayBay.DAO;

public class KhachHangDAO
{
    private readonly string duongDan;
    private readonly PhienBangSql<KhachHangDTO> sql = new();
    internal string ThuMuc => Path.GetDirectoryName(duongDan)!;

    public KhachHangDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "khachhang.json"));
    }

    public List<KhachHangDTO> DocDS()
    {
        List<KhachHangDTO> dskh = new List<KhachHangDTO>();
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHangDTO));
            string query = "SELECT MaKH,HoTen,CCCD,SDT,Email,TaiKhoan,MatKhau,DiaChi FROM KhachHang";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection);
            using MySqlDataReader reader = command.ExecuteReader();
            DanhMucMySql.KiemTraKieuKhoa(reader, map, typeof(KhachHangDTO));
            while (reader.Read())
            {
                KhachHangDTO kh = new KhachHangDTO();
                kh.MaKH = (reader.IsDBNull(reader.GetOrdinal("MaKH")) ? "" : reader.GetString("MaKH").TrimEnd());
                kh.HoTen = (reader.IsDBNull(reader.GetOrdinal("HoTen")) ? "" : reader.GetString("HoTen").TrimEnd());
                kh.CCCD = (reader.IsDBNull(reader.GetOrdinal("CCCD")) ? "" : reader.GetString("CCCD").TrimEnd());
                kh.SDT = (reader.IsDBNull(reader.GetOrdinal("SDT")) ? "" : reader.GetString("SDT").TrimEnd());
                kh.Email = (reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email").TrimEnd());
                kh.TaiKhoan = (reader.IsDBNull(reader.GetOrdinal("TaiKhoan")) ? "" : reader.GetString("TaiKhoan").TrimEnd());
                kh.MatKhau = (reader.IsDBNull(reader.GetOrdinal("MatKhau")) ? "" : reader.GetString("MatKhau").TrimEnd());
                kh.DiaChi = (reader.IsDBNull(reader.GetOrdinal("DiaChi")) ? "" : reader.GetString("DiaChi").TrimEnd());
                dskh.Add(kh);
            }
            reader.Close();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi đọc thông tin khách hàng!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
        sql.GhiNhanDaDoc(dskh);
        return dskh;
    }

    public void Them(KhachHangDTO kh)
    {
        ArgumentNullException.ThrowIfNull(kh);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHangDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, kh);
            string query = "INSERT INTO KhachHang(MaKH,HoTen,CCCD,SDT,Email,TaiKhoan,MatKhau,DiaChi) VALUES (@MaKH,@HoTen,@CCCD,@SDT,@Email,@TaiKhoan,@MatKhau,@DiaChi)";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaKH", kh.MaKH);
            command.Parameters.AddWithValue("@HoTen", kh.HoTen);
            command.Parameters.AddWithValue("@CCCD", kh.CCCD);
            command.Parameters.AddWithValue("@SDT", kh.SDT);
            command.Parameters.AddWithValue("@Email", kh.Email);
            command.Parameters.AddWithValue("@TaiKhoan", kh.TaiKhoan);
            command.Parameters.AddWithValue("@MatKhau", kh.MatKhau);
            command.Parameters.AddWithValue("@DiaChi", kh.DiaChi);
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi ghi thông tin khách hàng!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }

    public void Sua(KhachHangDTO kh)
    {
        ArgumentNullException.ThrowIfNull(kh);
        using MySqlConnection connection = MyConnection.GetConnection();
        try
        {
            connection.Open();
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHangDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            DanhMucMySql.KiemTraTruocKhiGhi(connection, transaction, map, kh);
            string query = "UPDATE KhachHang SET HoTen=@HoTen, CCCD=@CCCD, SDT=@SDT, Email=@Email, TaiKhoan=@TaiKhoan, MatKhau=@MatKhau, DiaChi=@DiaChi WHERE MaKH=@MaKH";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@HoTen", kh.HoTen);
            command.Parameters.AddWithValue("@CCCD", kh.CCCD);
            command.Parameters.AddWithValue("@SDT", kh.SDT);
            command.Parameters.AddWithValue("@Email", kh.Email);
            command.Parameters.AddWithValue("@TaiKhoan", kh.TaiKhoan);
            command.Parameters.AddWithValue("@MatKhau", kh.MatKhau);
            command.Parameters.AddWithValue("@DiaChi", kh.DiaChi);
            command.Parameters.AddWithValue("@MaKH", kh.MaKH);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi sửa thông tin khách hàng!" + " " + ex.Message, ex);
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
            AnhXaBangSql map = CSDLListDAO.DocAnhXa(typeof(KhachHangDTO));
            CSDLListDAO.KiemTraBangMySql(connection, map);
            using MySqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            if (map.KeyColumns.Count != 1) throw new ArgumentException("Bảng dùng khóa ghép; cần cung cấp đủ khóa để xóa.");
            DanhMucMySql.KiemTraBangKhoa<KhachHangDTO>(connection, transaction, map);
            string query = "DELETE FROM KhachHang WHERE MaKH=@MaKH";
            query = DanhMucMySql.AnhXaCauLenh(query, map);
            using MySqlCommand command = new MySqlCommand(query, connection, transaction);
            command.Parameters.AddWithValue("@MaKH", ma);
            command.CommandText = $"SELECT 1 FROM {DanhMucMySql.Bang(map)} WHERE {DanhMucMySql.DieuKienKhoa(map)} FOR UPDATE";
            if (command.ExecuteScalar() == null) throw new ArgumentException("Không tìm thấy dòng cần sửa/xóa.");
            command.CommandText = query;
            int r = command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException("Lỗi xóa thông tin khách hàng!" + " " + ex.Message, ex);
        }
        finally
        {
            connection.Close();
        }
    }
    public void Xoa(KhachHangDTO item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Xoa(item.MaKH);
    }

    // Giu API Doc/Luu cu cho BUS va du lieu JSON.
    public List<KhachHangDTO> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<KhachHangDTO>();
        return JsonSerializer.Deserialize<List<KhachHangDTO>>(File.ReadAllText(duongDan))
            ?? throw new InvalidDataException("File khách hàng không hợp lệ.");
    }

    public void Luu(IReadOnlyCollection<KhachHangDTO> danhSach)
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
