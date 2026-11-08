using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;
namespace QuanLyMayBay.GUI;
internal class ThamSoView : UserControl
{
    public ThamSoView(ThamSoBUS bus)
    {
        Dock = DockStyle.Fill;
        Controls.Add(new QuanLyTab<ThamSo>(new[]
        { new Truong("MaThamSo", "Mã tham số", Khoa: true), new Truong("GiaTri", "Giá trị"), new Truong("MoTa", "Mô tả") },
            q => bus.TimKiem(q).Cast<ThamSo>().ToList(), (x, sua) => bus.Luu(x, sua), x => bus.Xoa(x.MaThamSo)));
    }
}
