using VeMayBay.BUS;
using VeMayBay.DTO;

namespace VeMayBay.GUI;

internal class CTKMView : UserControl
{
    public CTKMView(CTKMBUS bus)
    {
        Dock = DockStyle.Fill;
        Controls.Add(new QuanLyTab<CTKMDTO>(
            new[]
            {
                new Truong("MaCTKM", "Mã CTKM", Khoa: true),
                new Truong("TenCTKM", "Tên CTKM"),
                new Truong("NgayBatDau", "Ngày bắt đầu", LaNgay: true),
                new Truong("NgayKetThuc", "Ngày kết thúc", LaNgay: true)
            },
            q => bus.TimKiem(q).Cast<CTKMDTO>().ToList(),
            (km, sua) => bus.Luu(km, sua), km => bus.Xoa(km.MaCTKM), bus.NhapDanhSach));
    }
}
