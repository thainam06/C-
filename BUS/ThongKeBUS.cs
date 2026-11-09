using System.Globalization;
using System.Text.Json;
using VeMayBay.DAO;
using VeMayBay.DTO;

namespace VeMayBay.BUS;

public enum NhomThongKe { NhanVien, KhachHang, SanPham, KhachHangSanPham, NhanVienSanPhamNam }
public enum KyThongKe { Ngay, Thang, Quy, Nam }

public class ThongKeBUS
{
    private readonly ThongKeDAO dao;
    private SoLieuThongKe data;
    public ThongKeBUS(ThongKeDAO? dao = null, bool taiNgay = true)
    { this.dao = dao ?? new(); data = taiNgay ? this.dao.Doc() : new(); KiemTra(data); }
    public SoLieuThongKe LayDuLieu() => JsonSerializer.Deserialize<SoLieuThongKe>(JsonSerializer.Serialize(data))!;
    public void TaiLai() { var next = dao.Doc(); KiemTra(next); data = next; }
    public static decimal ThanhTien(DongDoanhThu row, int soLe = 0)
    {
        if (soLe < 0 || soLe > 2) throw new ArgumentException("Số lẻ tiền phải từ 0–2.");
        return Math.Round(checked(row.SoLuong * row.DonGia * (1 - row.GiamGia / 100m)), soLe, MidpointRounding.AwayFromZero);
    }
    private static void KiemTra(SoLieuThongKe value)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in value.DoanhThu)
        {
            if (row == null || new[] { row.MaHD, row.MaKH, row.MaNV, row.MaSanPham }.Any(string.IsNullOrWhiteSpace)
                || row.STT <= 0 || row.SoLuong <= 0 || row.DonGia < 0 || row.GiamGia < 0 || row.GiamGia > 100
                || row.NgayLap < new DateTime(1753, 1, 1) || row.NgayLap.Year > 9998
                || !keys.Add(JsonSerializer.Serialize(new[] { row.MaHD.ToUpperInvariant(), row.STT.ToString() })))
                throw new ArgumentException("Chi tiết doanh thu thiếu/sai dữ liệu hoặc trùng cặp MaHD–STT.");
            _ = ThanhTien(row);
        }
        foreach (var group in value.DoanhThu.GroupBy(x => x.MaHD, StringComparer.OrdinalIgnoreCase))
        {
            var first = group.First();
            if (group.Any(x => x.NgayLap.Date != first.NgayLap.Date
                || !x.MaKH.Equals(first.MaKH, StringComparison.OrdinalIgnoreCase)
                || !x.MaNV.Equals(first.MaNV, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Các dòng cùng hóa đơn phải cùng ngày, khách hàng và nhân viên.");
        }
        keys.Clear();
        foreach (var row in value.ChiPhi)
            if (row == null || string.IsNullOrWhiteSpace(row.MaPC) || string.IsNullOrWhiteSpace(row.MaNV)
                || string.IsNullOrWhiteSpace(row.NoiDung) || row.SoTien < 0 || row.NgayChi < new DateTime(1753, 1, 1)
                || row.NgayChi.Year > 9998 || !keys.Add(row.MaPC))
                throw new ArgumentException("Phiếu chi thiếu/sai dữ liệu hoặc trùng mã.");
    }
    public void NhapDoanhThu(List<DongDoanhThu> rows)
    {
        var next = LayDuLieu(); next.DoanhThu = JsonSerializer.Deserialize<List<DongDoanhThu>>(JsonSerializer.Serialize(rows))!;
        KiemTra(next); dao.Luu(next); data = next;
    }
    public void NhapChiPhi(List<DongChiPhi> rows)
    {
        var next = LayDuLieu(); next.ChiPhi = JsonSerializer.Deserialize<List<DongChiPhi>>(JsonSerializer.Serialize(rows))!;
        KiemTra(next); dao.Luu(next); data = next;
    }
    private static void KhoangNgay(DateTime from, DateTime to)
    { if (from.Date > to.Date) throw new ArgumentException("Từ ngày phải trước hoặc bằng đến ngày."); }
    private List<DongDoanhThu> Loc(DateTime from, DateTime to)
    {
        KhoangNgay(from, to);
        return data.DoanhThu.Where(x => x.NgayLap.Date >= from.Date && x.NgayLap.Date <= to.Date).ToList();
    }
    public (decimal Thu, decimal Chi, decimal LoiNhuan) TongHop(DateTime from, DateTime to, int soLe = 0)
    {
        var thu = Loc(from, to).Sum(x => ThanhTien(x, soLe));
        var chi = data.ChiPhi.Where(x => x.NgayChi.Date >= from.Date && x.NgayChi.Date <= to.Date).Sum(x => Math.Round(x.SoTien, soLe, MidpointRounding.AwayFromZero));
        return (thu, chi, thu - chi);
    }
    public List<BangQuy> TheoQuy(NhomThongKe mode, int year, DateTime from, DateTime to, int soLe = 0)
    {
        if (mode is NhomThongKe.KhachHangSanPham or NhomThongKe.NhanVienSanPhamNam) throw new ArgumentException("Bảng quý dùng nhóm nhân viên, khách hàng hoặc sản phẩm.");
        var rows = Loc(from, to).Where(x => x.NgayLap.Year == year).ToList();
        string Key(DongDoanhThu x) => (mode == NhomThongKe.NhanVien ? x.MaNV : mode == NhomThongKe.KhachHang ? x.MaKH : x.MaSanPham).ToUpperInvariant();
        var result = rows.GroupBy(Key).OrderBy(x => x.Key).Select(g =>
        {
            var values = Enumerable.Range(1, 4).Select(q => g.Where(x => (x.NgayLap.Month - 1) / 3 + 1 == q).Sum(x => ThanhTien(x, soLe))).ToArray();
            return new BangQuy { Nhom = g.Key, Q1 = values[0], Q2 = values[1], Q3 = values[2], Q4 = values[3], Tong = values.Sum() };
        }).ToList();
        result.Add(new BangQuy { Nhom = "TỔNG CỘNG", Q1 = result.Sum(x => x.Q1), Q2 = result.Sum(x => x.Q2),
            Q3 = result.Sum(x => x.Q3), Q4 = result.Sum(x => x.Q4), Tong = result.Sum(x => x.Tong) });
        return result;
    }
    public List<DongThongKe> TheoKy(NhomThongKe group, KyThongKe period, DateTime from, DateTime to, int soLe = 0)
    {
        string Period(DongDoanhThu x) => period switch
        {
            KyThongKe.Ngay => x.NgayLap.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            KyThongKe.Thang => x.NgayLap.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            KyThongKe.Quy => $"{x.NgayLap.Year}-Q{(x.NgayLap.Month - 1) / 3 + 1}",
            _ => x.NgayLap.Year.ToString()
        };
        (string A, string B, int Nam, string Ky) Key(DongDoanhThu x) => (
            (group is NhomThongKe.NhanVien or NhomThongKe.NhanVienSanPhamNam ? x.MaNV : group == NhomThongKe.SanPham ? x.MaSanPham : x.MaKH).ToUpperInvariant(),
            group is NhomThongKe.KhachHangSanPham or NhomThongKe.NhanVienSanPhamNam ? x.MaSanPham.ToUpperInvariant() : "",
            group == NhomThongKe.NhanVienSanPhamNam ? x.NgayLap.Year : 0, Period(x));
        return Loc(from, to).GroupBy(Key).OrderBy(g => g.Key.Ky).ThenBy(g => g.Key.A).Select(g => new DongThongKe
        { Nhom = g.Key.A, MaThuHai = g.Key.B, Nam = g.Key.Nam, Ky = g.Key.Ky, Tong = g.Sum(x => ThanhTien(x, soLe)) }).ToList();
    }
}
