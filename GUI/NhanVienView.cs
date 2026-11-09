using VeMayBay.BUS;
using VeMayBay.DTO;

namespace VeMayBay.GUI;

internal class NhanVienView : UserControl
{
    public NhanVienView(NhanVienBUS bus)
    {
        Dock = DockStyle.Fill;
        Controls.Add(new QuanLyTab<NhanVienDTO>(
            new[]
            {
                new Truong("MaNV", "Mã nhân viên", Khoa: true),
                new Truong("Ho", "Họ"),
                new Truong("Ten", "Tên"),
                new Truong("ChucVu", "Chức vụ")
            },
            q => bus.TimKiem(q).Cast<NhanVienDTO>().ToList(),
            (nv, sua) => bus.Luu(nv, sua), nv => bus.Xoa(nv.MaNV), bus.NhapDanhSach));
    }
}
