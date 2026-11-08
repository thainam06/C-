using System.Collections;
using System.Globalization;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.BUS;

public class HanhKhachBUS
{
    private readonly HanhKhachDAO dao;
    private readonly KhachHangDAO khachHangDAO;
    private List<HanhKhach> danhSach;

    public HanhKhachBUS(HanhKhachDAO? dao = null, KhachHangDAO? khachHangDAO = null)
    {
        this.khachHangDAO = khachHangDAO ?? new KhachHangDAO();
        this.dao = dao ?? new HanhKhachDAO(Path.Combine(this.khachHangDAO.ThuMuc, "hanhkhach.json"));
        danhSach = this.dao.Doc();
        var cacKhachHang = LayKhachHang().Cast<KhachHang>()
            .Select(x => x.MaKH).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ma = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cccd = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var hk in danhSach)
            {
                KiemTraThongTin(hk);
                if (!ma.Add(hk.MaHK) || (hk.CCCD.Length > 0 && !cccd.Add(hk.CCCD))
                    || !cacKhachHang.Contains(hk.MaKH))
                    throw new InvalidDataException();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
        {
            throw new InvalidDataException("Dữ liệu hành khách không hợp lệ, trùng mã/CCCD hoặc mất liên kết khách hàng. File được giữ nguyên.");
        }
    }

    private static bool CungMa(string a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static string BatBuoc(string? value, string ten)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"Vui lòng nhập/chọn {ten}.");
        return value.Trim();
    }

    private static HanhKhach SaoChep(HanhKhach hk) => new(hk.MaHK, hk.HoTen,
        hk.NgaySinh, hk.CCCD, hk.GioiTinh, hk.MaKH, hk.LoaiKH);

    private static void KiemTraThongTin(HanhKhach hk)
    {
        BatBuoc(hk.MaHK, "mã hành khách");
        BatBuoc(hk.HoTen, "họ tên");
        BatBuoc(hk.MaKH, "khách hàng");
        BatBuoc(hk.LoaiKH, "loại khách hàng");
        if (hk.NgaySinh.Date < new DateTime(1753, 1, 1) || hk.NgaySinh.Date > DateTime.Today)
            throw new ArgumentException("Ngày sinh không hợp lệ hoặc lớn hơn hôm nay.");
        if (!new[] { "Nam", "Nữ", "Khác" }.Contains(hk.GioiTinh))
            throw new ArgumentException("Vui lòng chọn giới tính Nam, Nữ hoặc Khác.");
        if (hk.CCCD == null || (hk.CCCD.Length > 0
            && (hk.CCCD.Length != 12 || !hk.CCCD.All(c => c >= '0' && c <= '9'))))
            throw new ArgumentException("CCCD nếu nhập phải gồm đúng 12 chữ số.");
    }

    public ArrayList LayKhachHang() => new KhachHangBUS(khachHangDAO, dao).TimKiem();

    public ArrayList TimKiem(string tuKhoa = "")
    {
        var ketQua = new ArrayList();
        var nguon = new ArrayList(danhSach);
        foreach (HanhKhach hk in nguon)
        {
            var cacCot = new[] { hk.MaHK, hk.HoTen, hk.NgaySinh.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                hk.CCCD, hk.GioiTinh, hk.MaKH, hk.LoaiKH };
            if (cacCot.Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(SaoChep(hk));
        }
        return ketQua;
    }

    public void Luu(HanhKhach hk, bool sua)
    {
        ArgumentNullException.ThrowIfNull(hk);
        var gioiTinh = new[] { "Nam", "Nữ", "Khác" }
            .FirstOrDefault(x => CungMa(x, hk.GioiTinh?.Trim() ?? "")) ?? "";
        var moi = new HanhKhach(BatBuoc(hk.MaHK, "mã hành khách"), BatBuoc(hk.HoTen, "họ tên"),
            hk.NgaySinh.Date, hk.CCCD?.Trim() ?? "", gioiTinh,
            BatBuoc(hk.MaKH, "khách hàng"), BatBuoc(hk.LoaiKH, "loại khách hàng"));
        KiemTraThongTin(moi);
        var khachHang = LayKhachHang().Cast<KhachHang>().FirstOrDefault(x => CungMa(x.MaKH, moi.MaKH));
        if (khachHang == null) throw new ArgumentException("Mã khách hàng không tồn tại. Hãy thêm khách hàng trước.");
        moi.MaKH = khachHang.MaKH;
        var index = danhSach.FindIndex(x => CungMa(x.MaHK, moi.MaHK));
        if (sua && index < 0) throw new ArgumentException("Không tìm thấy hành khách cần sửa.");
        if (!sua && index >= 0) throw new ArgumentException("Mã hành khách đã tồn tại.");
        if (moi.CCCD.Length > 0 && danhSach.Any(x => !CungMa(x.MaHK, moi.MaHK) && x.CCCD == moi.CCCD))
            throw new ArgumentException("CCCD đã được dùng cho hành khách khác.");
        var danhSachMoi = danhSach.Select(SaoChep).ToList();
        if (sua) danhSachMoi[index] = moi;
        else danhSachMoi.Add(moi);
        dao.Luu(danhSachMoi);
        danhSach = danhSachMoi;
    }

    public void Xoa(string maHK)
    {
        maHK = BatBuoc(maHK, "mã hành khách");
        var moi = danhSach.Select(SaoChep).ToList();
        if (moi.RemoveAll(x => CungMa(x.MaHK, maHK)) == 0)
            throw new ArgumentException("Không tìm thấy hành khách cần xóa.");
        dao.Luu(moi);
        danhSach = moi;
    }
}
