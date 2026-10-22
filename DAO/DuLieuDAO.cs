using System.Text.Json;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class DuLieu
{
    public List<MayBay> MayBay { get; set; } = new();
    public List<Ghe> Ghe { get; set; } = new();
    public List<HangVe> HangVe { get; set; } = new();
    public bool DaNapBoDuLieuMau { get; set; }
    public bool DaCapNhatHangM01M02 { get; set; }
}

public class DuLieuDAO
{
    private readonly string duongDan;

    public DuLieuDAO(string? duongDan = null)
    {
        this.duongDan = duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", "dulieu.json");
    }

    public DuLieu Doc()
    {
        if (!File.Exists(duongDan))
            return new DuLieu();

        // Khong ghi de neu file hien tai bi hong hoac khong doc duoc.
        var duLieu = JsonSerializer.Deserialize<DuLieu>(File.ReadAllText(duongDan));
        if (duLieu == null || duLieu.MayBay == null || duLieu.Ghe == null || duLieu.HangVe == null)
            throw new InvalidDataException("File dữ liệu không hợp lệ.");

        return duLieu;
    }

    public void Luu(DuLieu duLieu)
    {
        var thuMuc = Path.GetDirectoryName(Path.GetFullPath(duongDan))!;
        Directory.CreateDirectory(thuMuc);
        var tam = duongDan + ".tmp";
        File.WriteAllText(tam, JsonSerializer.Serialize(duLieu,
            new JsonSerializerOptions { WriteIndented = true }));
        File.Move(tam, duongDan, overwrite: true);
    }
}
