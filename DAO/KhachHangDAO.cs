using System.Text.Json;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class KhachHangDAO
{
    private readonly string duongDan;

    public KhachHangDAO(string? duongDan = null)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "khachhang.json"));
    }

    public List<KhachHang> Doc()
    {
        if (!File.Exists(duongDan)) return new List<KhachHang>();
        return JsonSerializer.Deserialize<List<KhachHang>>(File.ReadAllText(duongDan))
            ?? throw new InvalidDataException("File khách hàng không hợp lệ.");
    }

    public void Luu(IReadOnlyCollection<KhachHang> danhSach)
    {
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
