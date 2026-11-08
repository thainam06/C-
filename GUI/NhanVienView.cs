using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

internal class NhanVienView : UserControl
{
    public NhanVienView(NhanVienBUS bus)
    {
        Dock = DockStyle.Fill;
        Controls.Add(new QuanLyTab<NhanVien>(
            new[]
            {
                new Truong("MaNV", "Mã nhân viên", Khoa: true),
                new Truong("Ho", "Họ"),
                new Truong("Ten", "Tên"),
                new Truong("ChucVu", "Chức vụ")
            },
            q => bus.TimKiem(q).Cast<NhanVien>().ToList(),
            (nv, sua) => bus.Luu(nv, sua), nv => bus.Xoa(nv.MaNV)));
    }
}
