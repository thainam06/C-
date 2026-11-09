using System.Text.Json;
using VeMayBay.DTO;

namespace VeMayBay.DAO;

public class ThongKeDAO
{
    private readonly string path;
    public ThongKeDAO(string? path = null) => this.path = Path.GetFullPath(path ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuanLyMayBay", "thongke.json"));
    public SoLieuThongKe Doc()
    {
        if (MyConnection.DaCauHinh)
        {
            // Anh xa toi view hoa don/chi tiet va phieu chi cua nhom, chi doc.
            CSDLListDAO.ThuDoc<DongDoanhThu>(out var revenue, out _);
            CSDLListDAO.ThuDoc<DongChiPhi>(out var cost, out _);
            return new() { DoanhThu = revenue, ChiPhi = cost };
        }
        if (!File.Exists(path)) return new();
        var result = JsonSerializer.Deserialize<SoLieuThongKe>(File.ReadAllText(path));
        if (result == null || result.DoanhThu == null || result.ChiPhi == null)
            throw new InvalidDataException("File thống kê không hợp lệ.");
        return result;
    }
    public void Luu(SoLieuThongKe data)
    {
        if (MyConnection.DaCauHinh) throw new InvalidOperationException("Số liệu thống kê SQL chỉ đọc từ view. Nhập chứng từ qua chức năng nghiệp vụ của CSDL nhóm.");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(temp, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })); File.Move(temp, path, true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
