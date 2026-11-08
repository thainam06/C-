using System.Collections;
using System.Net.Mail;
using System.Security.Cryptography;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.BUS;

public class KhachHangBUS
{
    private readonly KhachHangDAO dao;
    private readonly HanhKhachDAO hanhKhachDAO;
    private List<KhachHang> danhSach;

    public KhachHangBUS(KhachHangDAO? dao = null, HanhKhachDAO? hanhKhachDAO = null)
    {
        this.dao = dao ?? new KhachHangDAO();
        this.hanhKhachDAO = hanhKhachDAO ?? new HanhKhachDAO(Path.Combine(this.dao.ThuMuc, "hanhkhach.json"));
        danhSach = this.dao.Doc();
        try
        {
            var ma = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cccd = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var taiKhoan = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kh in danhSach)
            {
                if (kh == null) throw new InvalidDataException();
                KiemTraThongTin(kh);
                if (!ma.Add(kh.MaKH) || !cccd.Add(kh.CCCD) || !taiKhoan.Add(kh.TaiKhoan)
                    || !LaMatKhauDaBam(kh.MatKhau))
                    throw new InvalidDataException();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
        {
            throw new InvalidDataException("Dữ liệu khách hàng bị thiếu, trùng hoặc không hợp lệ. File được giữ nguyên.");
        }
    }

    private static bool CungMa(string a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static string BatBuoc(string? value, string ten)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"Vui lòng nhập {ten}.");
        return value.Trim();
    }

    private static KhachHang SaoChep(KhachHang kh, bool kemMatKhau = true) => new(
        kh.MaKH, kh.HoTen, kh.CCCD, kh.SDT, kh.Email, kh.TaiKhoan,
        kemMatKhau ? kh.MatKhau : string.Empty, kh.DiaChi);

    private static bool LaChuSo(string value) => value.All(c => c >= '0' && c <= '9');

    private static void KiemTraThongTin(KhachHang kh)
    {
        BatBuoc(kh.MaKH, "mã khách hàng");
        BatBuoc(kh.HoTen, "họ tên");
        var cccd = BatBuoc(kh.CCCD, "CCCD");
        if (cccd.Length != 12 || !LaChuSo(cccd))
            throw new ArgumentException("CCCD phải gồm đúng 12 chữ số.");
        var sdt = BatBuoc(kh.SDT, "số điện thoại");
        var cacSo = sdt.StartsWith('+') ? sdt[1..] : sdt;
        if (cacSo.Length < 9 || cacSo.Length > 15 || !LaChuSo(cacSo))
            throw new ArgumentException("Số điện thoại phải có 9–15 chữ số, có thể bắt đầu bằng dấu +.");
        var email = BatBuoc(kh.Email, "email");
        if (!MailAddress.TryCreate(email, out var diaChi) || !CungMa(diaChi.Address, email))
            throw new ArgumentException("Địa chỉ email không hợp lệ.");
        BatBuoc(kh.TaiKhoan, "tài khoản");
        BatBuoc(kh.DiaChi, "địa chỉ");
    }

    private static string BamMatKhau(string matKhau)
    {
        const int soVong = 600000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(matKhau, salt, soVong, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2-SHA256${soVong}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool LaMatKhauDaBam(string? value)
    {
        if (value == null) return false;
        var cacPhan = value.Split('$');
        if (cacPhan.Length != 4 || cacPhan[0] != "PBKDF2-SHA256" || cacPhan[1] != "600000") return false;
        try
        {
            return Convert.FromBase64String(cacPhan[2]).Length == 16
                && Convert.FromBase64String(cacPhan[3]).Length == 32;
        }
        catch (FormatException) { return false; }
    }

    // Tim kiem tren ArrayList tai BUS, khong tim kiem/tra ve mat khau.
    public ArrayList TimKiem(string tuKhoa = "")
    {
        var ketQua = new ArrayList();
        var nguon = new ArrayList(danhSach);
        foreach (KhachHang kh in nguon)
        {
            var cacCot = new[] { kh.MaKH, kh.HoTen, kh.CCCD, kh.SDT, kh.Email, kh.TaiKhoan, kh.DiaChi };
            if (cacCot.Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(SaoChep(kh, kemMatKhau: false));
        }
        return ketQua;
    }

    public void Luu(KhachHang kh, bool sua)
    {
        ArgumentNullException.ThrowIfNull(kh);
        var moi = new KhachHang(
            BatBuoc(kh.MaKH, "mã khách hàng"), BatBuoc(kh.HoTen, "họ tên"),
            BatBuoc(kh.CCCD, "CCCD"), BatBuoc(kh.SDT, "số điện thoại"),
            BatBuoc(kh.Email, "email"), BatBuoc(kh.TaiKhoan, "tài khoản"),
            kh.MatKhau ?? string.Empty, BatBuoc(kh.DiaChi, "địa chỉ"));
        KiemTraThongTin(moi);
        var index = danhSach.FindIndex(x => CungMa(x.MaKH, moi.MaKH));
        if (sua && index < 0) throw new ArgumentException("Không tìm thấy khách hàng cần sửa.");
        if (!sua && index >= 0) throw new ArgumentException("Mã khách hàng đã tồn tại.");
        if (danhSach.Any(x => !CungMa(x.MaKH, moi.MaKH) && CungMa(x.CCCD, moi.CCCD)))
            throw new ArgumentException("CCCD đã được dùng cho khách hàng khác.");
        if (danhSach.Any(x => !CungMa(x.MaKH, moi.MaKH) && CungMa(x.TaiKhoan, moi.TaiKhoan)))
            throw new ArgumentException("Tài khoản đã tồn tại.");
        if (string.IsNullOrWhiteSpace(moi.MatKhau))
        {
            if (!sua) throw new ArgumentException("Vui lòng nhập mật khẩu khi thêm khách hàng.");
            moi.MatKhau = danhSach[index].MatKhau;
        }
        else moi.MatKhau = BamMatKhau(moi.MatKhau);

        var danhSachMoi = danhSach.Select(x => SaoChep(x)).ToList();
        if (sua) danhSachMoi[index] = moi;
        else danhSachMoi.Add(moi);
        dao.Luu(danhSachMoi);
        danhSach = danhSachMoi;
    }

    public void Xoa(string maKH)
    {
        maKH = BatBuoc(maKH, "mã khách hàng");
        if (hanhKhachDAO.Doc().Any(x => CungMa(x.MaKH, maKH)))
            throw new ArgumentException("Khách hàng đang có hành khách liên quan. Hãy đổi khách hàng hoặc xóa hành khách trước.");
        var moi = danhSach.Select(x => SaoChep(x)).ToList();
        if (moi.RemoveAll(x => CungMa(x.MaKH, maKH)) == 0)
            throw new ArgumentException("Không tìm thấy khách hàng cần xóa.");
        dao.Luu(moi);
        danhSach = moi;
    }
}
