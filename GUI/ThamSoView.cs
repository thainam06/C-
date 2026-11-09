using VeMayBay.BUS;
using VeMayBay.DTO;
namespace VeMayBay.GUI;
internal class ThamSoView : UserControl
{
    public ThamSoView(ThamSoBUS bus)
    {
        Dock = DockStyle.Fill;
        Controls.Add(new QuanLyTab<ThamSoDTO>(new[]
        { new Truong("MaThamSo", "Mã tham số", Khoa: true), new Truong("GiaTri", "Giá trị"), new Truong("MoTa", "Mô tả") },
            q => bus.TimKiem(q).Cast<ThamSoDTO>().ToList(), (x, sua) => bus.Luu(x, sua), x => bus.Xoa(x.MaThamSo)));
    }
}
