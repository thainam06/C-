using System.Text.Json;

namespace QuanLyMayBay.DAO;

// Dung chung cach doc/ghi JSON cho cac danh muc moi.
public class JsonDanhSachDAO<T> where T : class, new()
{
    private readonly string duongDan;
    private readonly PhienBangSql<T> sql = new();
    internal string ThuMuc => Path.GetDirectoryName(duongDan)!;

    protected JsonDanhSachDAO(string tenFile, string? duongDan)
    {
        this.duongDan = Path.GetFullPath(duongDan ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "QuanLyMayBay", tenFile));
    }

    public List<T> Doc()
    {
        if (sql.ThuDoc(out var dataSql)) return dataSql;
        if (!File.Exists(duongDan)) return new List<T>();
        var data = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(duongDan));
        if (data == null || data.Any(x => x == null))
            throw new InvalidDataException($"File {typeof(T).Name} không hợp lệ.");
        return data;
    }

    public void Luu(IReadOnlyCollection<T> danhSach)
    {
        if (sql.ThuLuu(danhSach)) return;
        Directory.CreateDirectory(ThuMuc);
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
