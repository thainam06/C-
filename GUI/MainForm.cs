using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;
using QuanLyMayBay.DAO;

namespace QuanLyMayBay.GUI;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Quản lý máy bay và khách hàng";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 650);
        Size = new Size(1100, 760);
        Font = new Font("Segoe UI", 10);

        QuanLyBUS bus;
        KhachHangBUS khachHangBus;
        HanhKhachBUS hanhKhachBus;
        NhanVienBUS nhanVienBus;
        CTKMBUS ctkmBus;
        VoucherBUS voucherBus;
        ThamSoBUS thamSoBus;
        try
        {
            bus = new QuanLyBUS();
            if (!MyConnection.DaCauHinh)
            {
                bus.NapBoDuLieuMauMotLan();
                bus.CauHinhHangVeMacDinh();
                bus.CapNhatHangM01M02MotLan();
            }
            khachHangBus = new KhachHangBUS();
            hanhKhachBus = new HanhKhachBUS();
            nhanVienBus = new NhanVienBUS();
            ctkmBus = new CTKMBUS();
            voucherBus = new VoucherBUS();
            thamSoBus = new ThamSoBUS();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể đọc hoặc chuẩn bị dữ liệu. Chương trình sẽ đóng.\n"
                + ex.Message, "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Shown += (_, _) => Close();
            return;
        }

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var menu = new ListBox { Dock = DockStyle.Left, Width = 195, Font = new Font("Segoe UI", 10),
            DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 34, IntegralHeight = false,
            BackColor = Color.FromArgb(30, 58, 95), ForeColor = Color.White, BorderStyle = BorderStyle.None };
        menu.DrawItem += (_, e) =>
        {
            if (e.Index < 0) return;
            using var brush = new SolidBrush((e.State & DrawItemState.Selected) != 0 ? Color.FromArgb(45, 100, 155) : menu.BackColor);
            e.Graphics.FillRectangle(brush, e.Bounds);
            e.Graphics.DrawString(Convert.ToString(menu.Items[e.Index]), e.Font!, Brushes.White, e.Bounds.Left + 10, e.Bounds.Top + 7);
            e.DrawFocusRectangle();
        };
        var tongQuan = new MayBayGheView(bus);
        var hanhKhach = new HanhKhachView(hanhKhachBus);
        var voucher = new VoucherView(voucherBus);
        var mainSub = new MainSubView(bus, khachHangBus, hanhKhachBus, ctkmBus, voucherBus);
        var thongKeBus = new ThongKeBUS(taiNgay: false);
        var baoCao = new BaoCaoView(thongKeBus, thamSoBus);
        var hoaDon = new HoaDonView(thongKeBus, thamSoBus);
        var mayBay = new QuanLyTab<MayBay>(
            new[]
            {
                new Truong("MaMayBay", "Mã máy bay", Khoa: true),
                new Truong("MHHK", "Mã hãng hàng không")
            },
            q => bus.TimMayBay(q).Cast<MayBay>().ToList(),
            (x, sua) => bus.LuuMayBay(x.MaMayBay, x.MHHK, sua),
            x => bus.XoaMayBay(x.MaMayBay));

        var hangVe = new QuanLyTab<HangVe>(
            new[]
            {
                new Truong("MaHV", "Mã hạng vé", Khoa: true),
                new Truong("TenHV", "Tên hạng vé", LuaChon: bus.LayTenHangVe),
                new Truong("HanhLyKg", "Hành lý (kg)", So: true),
                new Truong("DonGiaVe", "Đơn giá vé (VNĐ)", So: true)
            },
            q => bus.TimHangVe(q).Cast<HangVe>().ToList(),
            (x, sua) => bus.LuuHangVe(x.MaHV, x.TenHV, x.HanhLyKg, x.DonGiaVe, sua),
            x => bus.XoaHangVe(x.MaHV));
        hangVe.RangBuocLuaChon("MaHV", "TenHV", QuanLyBUS.TenHangVeTheoMa);

        var ghe = new QuanLyTab<Ghe>(
            new[]
            {
                new Truong("MaGhe", "Mã ghế", Khoa: true),
                new Truong("MaHV", "Hạng vé",
                    LuaChon: () => bus.TimHangVe().Cast<HangVe>()
                        .Where(x => QuanLyBUS.LoaiGheTheoHangVe(x.MaHV) != null)
                        .OrderBy(x => x.MaHV)
                        .Select(x => x.MaHV).ToList()),
                new Truong("MaMayBay", "Máy bay", Khoa: true,
                    LuaChon: () => bus.TimMayBay().Cast<MayBay>().Select(x => x.MaMayBay).ToList()),
                new Truong("LoaiGhe", "Loại ghế",
                    LuaChon: () => new List<string> { Ghe.Thuong, Ghe.Vip })
            },
            q => bus.TimGhe(q).Cast<Ghe>().ToList(),
            (x, sua) => bus.LuuGhe(x.MaGhe, x.MaMayBay, x.MaHV, sua, x.LoaiGhe),
            x => bus.XoaGhe(x.MaGhe, x.MaMayBay));
        ghe.RangBuocLuaChon("MaHV", "LoaiGhe", QuanLyBUS.LoaiGheTheoHangVe);

        var pages = new (string Ten, Control View)[]
        {
            ("Bảng tổng hợp", tongQuan), ("Chính – chi tiết", mainSub), ("Máy bay", mayBay), ("Hạng vé", hangVe), ("Ghế", ghe),
            ("Khách hàng", new KhachHangView(khachHangBus)), ("Hành khách", hanhKhach),
            ("Nhân viên", new NhanVienView(nhanVienBus)), ("CTKM", new CTKMView(ctkmBus)),
            ("Voucher", voucher), ("Hóa đơn – Chi tiết", hoaDon), ("Thống kê", baoCao),
            ("Tham số", new ThamSoView(thamSoBus)), ("Kết nối SQL Server", new CauHinhSqlView())
        };
        foreach (var page in pages)
        {
            page.View.Dock = DockStyle.Fill; page.View.Visible = false;
            content.Controls.Add(page.View);
        }
        menu.SelectedIndexChanged += (_, _) =>
        {
            if (menu.SelectedIndex < 0) return;
            var active = pages[menu.SelectedIndex].View;
            foreach (var page in pages) page.View.Visible = ReferenceEquals(page.View, active);
            active.BringToFront();
            if (ReferenceEquals(active, tongQuan)) tongQuan.LamMoi();
            if (ReferenceEquals(active, mainSub)) mainSub.LamMoi();
            if (ReferenceEquals(active, ghe)) ghe.LamMoi();
            if (ReferenceEquals(active, hanhKhach)) hanhKhach.LamMoi();
            if (ReferenceEquals(active, voucher)) voucher.LamMoi();
            if (ReferenceEquals(active, baoCao)) baoCao.LamMoi();
            if (ReferenceEquals(active, hoaDon)) hoaDon.LamMoi();
        };

        var title = new Label
        {
            Text = "QUẢN LÝ VÉ MÁY BAY",
            Dock = DockStyle.Top, Height = 66, Padding = new Padding(20, 18, 0, 0),
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            BackColor = Color.FromArgb(30, 58, 95), ForeColor = Color.White
        };
        Controls.Add(content);
        Controls.Add(menu);
        Controls.Add(title);
        menu.DataSource = pages.Select(x => x.Ten).ToList();
    }
}
