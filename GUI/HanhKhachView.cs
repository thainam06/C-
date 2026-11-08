using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

internal class HanhKhachView : UserControl
{
    private readonly QuanLyTab<HanhKhach> tab;

    public HanhKhachView(HanhKhachBUS bus)
    {
        Dock = DockStyle.Fill;
        tab = new QuanLyTab<HanhKhach>(
            new[]
            {
                new Truong("MaHK", "Mã hành khách", Khoa: true),
                new Truong("HoTen", "Họ tên"),
                new Truong("NgaySinh", "Ngày sinh", LaNgaySinh: true),
                new Truong("CCCD", "CCCD (nếu có)"),
                new Truong("GioiTinh", "Giới tính", LuaChon: () => new List<string> { "Nam", "Nữ", "Khác" }),
                new Truong("MaKH", "Khách hàng", LuaChonCoNhan: () => bus.LayKhachHang().Cast<KhachHang>()
                    .Select(x => new MucLuaChon(x.MaKH, $"{x.MaKH} – {x.HoTen}")).ToList()),
                new Truong("LoaiKH", "Loại khách hàng")
            },
            q => bus.TimKiem(q).Cast<HanhKhach>().ToList(),
            (hk, sua) => bus.Luu(hk, sua),
            hk => bus.Xoa(hk.MaHK));
        Controls.Add(tab);
    }

    public void LamMoi() => tab.LamMoi();
}
