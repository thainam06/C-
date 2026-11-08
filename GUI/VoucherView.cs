using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

internal class VoucherView : UserControl
{
    private readonly QuanLyTab<Voucher> tab;

    public VoucherView(VoucherBUS bus)
    {
        Dock = DockStyle.Fill;
        tab = new QuanLyTab<Voucher>(
            new[]
            {
                new Truong("MaVoucher", "Mã Voucher", Khoa: true),
                new Truong("MaCTKM", "CTKM", LuaChonCoNhan: () => bus.LayCTKM().Cast<CTKM>()
                    .Select(x => new MucLuaChon(x.MaCTKM, $"{x.MaCTKM} – {x.TenCTKM}")).ToList()),
                new Truong("GiamGia", "Giảm giá (%)", So: true, GiaTriToiDa: 100m)
            },
            q => bus.TimKiem(q).Cast<Voucher>().ToList(),
            (v, sua) => bus.Luu(v, sua), v => bus.Xoa(v.MaVoucher), bus.NhapDanhSach);
        Controls.Add(tab);
    }

    public void LamMoi() => tab.LamMoi();
}
