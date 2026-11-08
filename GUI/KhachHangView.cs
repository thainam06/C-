using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

internal class KhachHangView : UserControl
{
    public KhachHangView(KhachHangBUS bus)
    {
        Dock = DockStyle.Fill;
        var tab = new QuanLyTab<KhachHang>(
            new[]
            {
                new Truong("MaKH", "Mã khách hàng", Khoa: true),
                new Truong("HoTen", "Họ tên"),
                new Truong("CCCD", "CCCD"),
                new Truong("SDT", "Số điện thoại"),
                new Truong("Email", "Email"),
                new Truong("TaiKhoan", "Tài khoản"),
                new Truong("MatKhau", "Mật khẩu", LaMatKhau: true),
                new Truong("DiaChi", "Địa chỉ")
            },
            q => bus.TimKiem(q).Cast<KhachHang>().ToList(),
            (kh, sua) => bus.Luu(kh, sua),
            kh => bus.Xoa(kh.MaKH));
        Controls.Add(tab);
    }
}
