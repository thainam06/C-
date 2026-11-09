using System.Text.Json;
using QuanLyMayBay.DTO;

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

    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(HanhKhach),
        new[] { "MaHK", "HoTen", "NgaySinh", "CCCD", "GioiTinh", "MaKH", "LoaiKH" },
        (reader, map) => new HanhKhach
        {
            MaHK = DanhMucMySql.Chuoi(reader, "MaHK"),
            HoTen = DanhMucMySql.Chuoi(reader, "HoTen"),
            NgaySinh = reader.IsDBNull(reader.GetOrdinal("NgaySinh")) ? null : reader.GetDateTime("NgaySinh"),
            CCCD = DanhMucMySql.Chuoi(reader, "CCCD"),
            GioiTinh = DanhMucMySql.Chuoi(reader, "GioiTinh"),
            MaKH = DanhMucMySql.Chuoi(reader, "MaKH"),
            LoaiKH = DanhMucMySql.Chuoi(reader, "LoaiKH"),
        });

    public List<HanhKhach> DocDS() => Doc();
    public void Them(HanhKhach item) => Lenh.Them(item);
    public void Sua(HanhKhach item) => Lenh.Sua(item);
    public void Xoa(HanhKhach item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
