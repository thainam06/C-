using System.Text.Json;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class KhachHangDAO
{
    private readonly string duongDan;
    private readonly PhienBangSql<KhachHang> sql = new();
    internal string ThuMuc => Path.GetDirectoryName(duongDan)!;

    public KhachHangDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "khachhang.json"));
    }

    public List<KhachHang> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<KhachHang>();
        return JsonSerializer.Deserialize<List<KhachHang>>(File.ReadAllText(duongDan))
            ?? throw new InvalidDataException("File khách hàng không hợp lệ.");
    }

    public void Luu(IReadOnlyCollection<KhachHang> danhSach)
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
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(KhachHang),
        new[] { "MaKH", "HoTen", "CCCD", "SDT", "Email", "TaiKhoan", "MatKhau", "DiaChi" },
        (reader, map) => new KhachHang
        {
            MaKH = DanhMucMySql.Chuoi(reader, "MaKH"),
            HoTen = DanhMucMySql.Chuoi(reader, "HoTen"),
            CCCD = DanhMucMySql.Chuoi(reader, "CCCD"),
            SDT = DanhMucMySql.Chuoi(reader, "SDT"),
            Email = DanhMucMySql.Chuoi(reader, "Email"),
            TaiKhoan = DanhMucMySql.Chuoi(reader, "TaiKhoan"),
            MatKhau = DanhMucMySql.Chuoi(reader, "MatKhau"),
            DiaChi = DanhMucMySql.Chuoi(reader, "DiaChi"),
        });

    public List<KhachHang> DocDS() => Doc();
    public void Them(KhachHang item) => Lenh.Them(item);
    public void Sua(KhachHang item) => Lenh.Sua(item);
    public void Xoa(KhachHang item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
