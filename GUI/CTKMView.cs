using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

internal class CTKMView : UserControl
{
    public CTKMView(CTKMBUS bus)
    {
        Dock = DockStyle.Fill;
        Controls.Add(new QuanLyTab<CTKM>(
            new[]
            {
                new Truong("MaCTKM", "Mã CTKM", Khoa: true),
                new Truong("TenCTKM", "Tên CTKM"),
                new Truong("NgayBatDau", "Ngày bắt đầu", LaNgay: true),
                new Truong("NgayKetThuc", "Ngày kết thúc", LaNgay: true)
            },
            q => bus.TimKiem(q).Cast<CTKM>().ToList(),
            (km, sua) => bus.Luu(km, sua), km => bus.Xoa(km.MaCTKM)));
    }
}
