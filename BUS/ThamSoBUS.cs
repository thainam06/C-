using System.Collections;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.BUS;

public class ThamSoBUS
{
    private readonly ThamSoDAO dao;
    private List<ThamSo> data;
    public ThamSoBUS(ThamSoDAO? dao = null)
    {
        this.dao = dao ?? new ThamSoDAO();
        data = this.dao.Doc();
        if (data.Count == 0)
        {
            data = new()
            {
                new() { MaThamSo = "TEN_DON_VI", GiaTri = "Quản lý bán vé máy bay", MoTa = "Tên hiển thị trên PDF" },
                new() { MaThamSo = "DIA_CHI", GiaTri = "Chưa cung cấp", MoTa = "Địa chỉ đơn vị trên PDF" },
                new() { MaThamSo = "SDT", GiaTri = "Chưa cung cấp", MoTa = "Số liên hệ trên PDF" },
                new() { MaThamSo = "SO_LE_TIEN", GiaTri = "0", MoTa = "Số chữ số làm tròn tiền: 0–2" }
            };
            this.dao.Luu(data);
        }
        if (data.GroupBy(x => x.MaThamSo, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
            throw new InvalidDataException("Trùng mã tham số.");
        foreach (var item in data) KiemTra(item);
        if (!MyConnection.DaCauHinh && data.Any(x => x.MaThamSo == "TEN_DON_VI"
            && x.GiaTri is "Quản lý vé máy bay" or "Quản lý máy bay"))
        {
            var next = data.Select(Copy).ToList();
            next.First(x => x.MaThamSo == "TEN_DON_VI").GiaTri = "Quản lý bán vé máy bay";
            this.dao.Luu(next); data = next;
        }
    }
    private static ThamSo Copy(ThamSo x) => new() { MaThamSo = x.MaThamSo, GiaTri = x.GiaTri, MoTa = x.MoTa };
    private static void KiemTra(ThamSo x)
    {
        if (string.IsNullOrWhiteSpace(x.MaThamSo) || string.IsNullOrWhiteSpace(x.GiaTri))
            throw new ArgumentException("Mã và giá trị tham số không được trống.");
        if (x.MaThamSo.Equals("SO_LE_TIEN", StringComparison.OrdinalIgnoreCase)
            && (!int.TryParse(x.GiaTri, out var n) || n < 0 || n > 2))
            throw new ArgumentException("SO_LE_TIEN phải là số nguyên từ 0 đến 2.");
    }
    public string Lay(string ma) => data.FirstOrDefault(x => x.MaThamSo.Equals(ma, StringComparison.OrdinalIgnoreCase))?.GiaTri ?? "";
    public int SoLeTien => int.TryParse(Lay("SO_LE_TIEN"), out var n) ? n : 0;
    public ArrayList TimKiem(string q = "") => new(data.Where(x => new[] { x.MaThamSo, x.GiaTri, x.MoTa }
        .Any(v => v.Contains(q.Trim(), StringComparison.OrdinalIgnoreCase))).Select(Copy).ToList());
    public void Luu(ThamSo x, bool sua)
    {
        var item = new ThamSo { MaThamSo = x.MaThamSo.Trim(), GiaTri = x.GiaTri.Trim(), MoTa = x.MoTa.Trim() };
        KiemTra(item);
        var index = data.FindIndex(v => v.MaThamSo.Equals(item.MaThamSo, StringComparison.OrdinalIgnoreCase));
        if (sua && index < 0) throw new ArgumentException("Không tìm thấy tham số.");
        if (!sua && index >= 0) throw new ArgumentException("Trùng mã tham số.");
        var next = data.Select(Copy).ToList();
        if (sua) next[index] = item; else next.Add(item);
        dao.Luu(next); data = next;
    }
    public void Xoa(string ma) => throw new ArgumentException("Tham số cấu hình được sửa giá trị, không xóa trên giao diện.");
}
