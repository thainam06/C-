using VeMayBay.BUS;
using VeMayBay.DTO;
using VeMayBay.DAO;

namespace VeMayBay.GUI;

public class MainForm : Form
{
    // Khong cache loi: neu mapping duoc sua, nut Thu lai co the nap lai module do.
    private sealed class DuLieuMuon<T> where T : class
    {
        private readonly Func<T> factory;
        private T? value;
        public DuLieuMuon(Func<T> factory) => this.factory = factory;
        public T Lay() => value ??= factory();
    }

    public MainForm()
    {
        Text = "Quản lý bán vé máy bay";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 650);
        Size = new Size(1100, 760);
        Font = new Font("Segoe UI", 10);

        var air = new DuLieuMuon<QuanLyBUS>(() =>
        {
            var bus = new QuanLyBUS();
            if (!MyConnection.DaCauHinh)
            {
                bus.NapBoDuLieuMauMotLan(); bus.CauHinhHangVeMacDinh(); bus.CapNhatHangM01M02MotLan();
            }
            return bus;
        });
        var customers = new DuLieuMuon<KhachHangBUS>(() => new());
        var passengers = new DuLieuMuon<HanhKhachBUS>(() => new());
        var employees = new DuLieuMuon<NhanVienBUS>(() => new());
        var promotions = new DuLieuMuon<CTKMBUS>(() => new());
        var vouchers = new DuLieuMuon<VoucherBUS>(() => new());
        var parameters = new DuLieuMuon<ThamSoBUS>(() => new());
        var ledger = new DuLieuMuon<ThongKeBUS>(() => new(taiNgay: false));

        var pages = new (string Ten, Func<Control> Tao)[]
        {
            ("Bảng tổng hợp", () => new MayBayGheView(air.Lay())),
            ("Chính – chi tiết", () => new MainSubView(air.Lay(), customers.Lay(), passengers.Lay(), promotions.Lay(), vouchers.Lay())),
            ("Máy bay", () => MayBayForm(air.Lay())),
            ("Hạng vé", () => HangVeForm(air.Lay())),
            ("Ghế", () => GheForm(air.Lay())),
            ("Khách hàng", () => new KhachHangView(customers.Lay())),
            ("Hành khách", () => new HanhKhachView(passengers.Lay())),
            ("Nhân viên", () => new NhanVienView(employees.Lay())),
            ("CTKM", () => new CTKMView(promotions.Lay())),
            ("Voucher", () => new VoucherView(vouchers.Lay())),
            ("Hóa đơn – Chi tiết", () => new HoaDonView(ledger.Lay(), parameters.Lay())),
            ("Thống kê", () => new BaoCaoView(ledger.Lay(), parameters.Lay())),
            ("Tham số", () => new ThamSoView(parameters.Lay())),
            ("Kết nối CSDL", () => new CauHinhSqlView())
        };
        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var cache = new Dictionary<int, Control>();
        var menu = new ListBox
        {
            Dock = DockStyle.Left, Width = 195, Font = new Font("Segoe UI", 10), DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 34, IntegralHeight = false, BackColor = Color.FromArgb(30, 58, 95), ForeColor = Color.White, BorderStyle = BorderStyle.None
        };
        menu.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            using var brush = new SolidBrush((e.State & DrawItemState.Selected) != 0 ? Color.FromArgb(45, 100, 155) : menu.BackColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
            e.Graphics.DrawString(Convert.ToString(menu.Items[e.Index]), e.Font!, Brushes.White, e.Bounds.Left + 10, e.Bounds.Top + 7);
            e.DrawFocusRectangle();
        };
        void HienTrang()
        {
            int index = menu.SelectedIndex;
            if (index < 0) return;
            foreach (Control view in content.Controls) view.Visible = false;
            try
            {
                if (!cache.TryGetValue(index, out var active))
                {
                    active = pages[index].Tao(); active.Dock = DockStyle.Fill;
                    content.Controls.Add(active); cache[index] = active;
                }
                active.Visible = true; active.BringToFront();
                switch (active)
                {
                    case MayBayGheView v: v.LamMoi(); break;
                    case MainSubView v: v.LamMoi(); break;
                    case QuanLyTab<GheDTO> v: v.LamMoi(); break;
                    case HanhKhachView v: v.LamMoi(); break;
                    case VoucherView v: v.LamMoi(); break;
                    case BaoCaoView v: v.LamMoi(); break;
                    case HoaDonView v: v.LamMoi(); break;
                }
            }
            catch (Exception ex)
            {
                if (cache.Remove(index, out var failed)) { content.Controls.Remove(failed); failed.Dispose(); }
                var error = new Panel { Dock = DockStyle.Fill };
                var text = new TextBox
                {
                    Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                    Text = "Chưa mở được " + pages[index].Ten + ".\r\n\r\n" + ex.Message
                        + "\r\n\r\nCác màn hình khác vẫn có thể mở. Xem Kết nối CSDL để xuất cấu trúc CSDL và kiểm tra mapping."
                };
                var retry = new Button { Text = "Thử lại", Dock = DockStyle.Top, Height = 38 };
                retry.Click += (_, _) => { cache.Remove(index); content.Controls.Remove(error); error.Dispose(); HienTrang(); };
                error.Controls.Add(text); error.Controls.Add(retry);
                content.Controls.Add(error); cache[index] = error; error.BringToFront();
            }
        }
        menu.SelectedIndexChanged += (_, _) => HienTrang();
        var title = new Label
        {
            Text = "QUẢN LÝ BÁN VÉ MÁY BAY", Dock = DockStyle.Top, Height = 66, Padding = new Padding(20, 18, 0, 0),
            Font = new Font("Segoe UI", 17, FontStyle.Bold), BackColor = Color.FromArgb(30, 58, 95), ForeColor = Color.White
        };
        Controls.Add(content); Controls.Add(menu); Controls.Add(title);
        menu.DataSource = pages.Select(x => x.Ten).ToList();
    }

    private static Control MayBayForm(QuanLyBUS bus)
    {
        var hang = MyConnection.DaCauHinh
            ? new Truong("MHHK", "Hãng hàng không", LuaChonCoNhan: () => bus.LayHangHangKhong().Cast<HangHangKhongDTO>()
                .Select(x => new MucLuaChon(x.MaHHK, $"{x.MaHHK} – {x.TenHang}")).ToList())
            : new Truong("MHHK", "Mã hãng hàng không");
        return new QuanLyTab<MayBayDTO>(new[] { new Truong("MaMayBay", "Mã máy bay", Khoa: true), hang },
            q => bus.TimMayBay(q).Cast<MayBayDTO>().ToList(),
            (x, sua) => bus.LuuMayBay(x.MaMayBay, x.MHHK, sua), x => bus.XoaMayBay(x.MaMayBay));
    }

    private static Control HangVeForm(QuanLyBUS bus)
    {
        var tab = new QuanLyTab<HangVeDTO>(new[]
        {
            new Truong("MaHV", "Mã hạng vé", Khoa: true), new Truong("TenHV", "Tên hạng vé", LuaChon: bus.LayTenHangVe),
            new Truong("HanhLyKg", "Hành lý (kg)", So: true), new Truong("DonGiaVe", "Đơn giá vé (VNĐ)", So: true)
        }, q => bus.TimHangVe(q).Cast<HangVeDTO>().ToList(),
            (x, sua) => bus.LuuHangVe(x.MaHV, x.TenHV, x.HanhLyKg, x.DonGiaVe, sua), x => bus.XoaHangVe(x.MaHV));
        tab.RangBuocLuaChon("MaHV", "TenHV", QuanLyBUS.TenHangVeTheoMa);
        return tab;
    }

    private static Control GheForm(QuanLyBUS bus)
    {
        var tab = new QuanLyTab<GheDTO>(new[]
        {
            new Truong("MaGhe", "Mã ghế", Khoa: true),
            new Truong("MaHV", "Hạng vé", LuaChon: () => bus.TimHangVe().Cast<HangVeDTO>()
                .Where(x => QuanLyBUS.LoaiGheTheoHangVe(x.MaHV) != null).OrderBy(x => x.MaHV).Select(x => x.MaHV).ToList()),
            new Truong("MaMayBay", "Máy bay", Khoa: true, LuaChon: () => bus.TimMayBay().Cast<MayBayDTO>().Select(x => x.MaMayBay).ToList()),
            new Truong("LoaiGhe", "Loại ghế", LuaChon: () => new List<string> { GheDTO.Thuong, GheDTO.Vip })
        }, q => bus.TimGhe(q).Cast<GheDTO>().ToList(),
            (x, sua) => bus.LuuGhe(x.MaGhe, x.MaMayBay, x.MaHV, sua, x.LoaiGhe), x => bus.XoaGhe(x.MaGhe, x.MaMayBay));
        tab.RangBuocLuaChon("MaHV", "LoaiGhe", QuanLyBUS.LoaiGheTheoHangVe);
        return tab;
    }
}
