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
}
