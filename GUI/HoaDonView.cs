using System.Globalization;
using VeMayBay.BUS;
using VeMayBay.DAO;
using VeMayBay.DTO;
using VeMayBay.Services;

namespace VeMayBay.GUI;

internal class HoaDonView : UserControl
{
    private readonly ThongKeBUS bus;
    private readonly ThamSoBUS thamSo;
    private readonly ListView master = List();
    private readonly ListView detail = List();
    private readonly Label total = new() { Dock = DockStyle.Bottom, Height = 36 };
    public HoaDonView(ThongKeBUS bus, ThamSoBUS thamSo)
    {
        this.bus = bus; this.thamSo = thamSo; Dock = DockStyle.Fill;
        var tools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 86, WrapContents = true };
        void Button(string text, Action action, bool requireJson = false)
        {
            var b = new Button { Text = text, Width = 140, Height = 32, Enabled = !requireJson || !MyConnection.DaCauHinh };
            b.Click += (_, _) => Run(action); tools.Controls.Add(b);
        }
        Button("Tải dữ liệu", LamMoi);
        Button("Mẫu CTHD XLS", () => Template<DongDoanhThu>("MauCTHD.xls"));
        Button("Mẫu phiếu chi", () => Template<DongChiPhi>("MauPhieuChi.xls"));
        Button("Nhập CTHD", () => Import(true), true); Button("Nhập phiếu chi", () => Import(false), true);
        Button("PDF hóa đơn", XuatHoaDon);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        var a = new GroupBox { Text = "Hóa đơn — chọn một dòng", Dock = DockStyle.Fill };
        var b = new GroupBox { Text = "Chi tiết hóa đơn được chọn", Dock = DockStyle.Fill };
        a.Controls.Add(master); b.Controls.Add(detail); layout.Controls.Add(a, 0, 0); layout.Controls.Add(b, 0, 1);
        master.SelectedIndexChanged += (_, _) => ChiTiet();
        Controls.Add(layout); Controls.Add(tools); Controls.Add(total);
    }
    private static ListView List() => new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
        MultiSelect = false, HideSelection = false, GridLines = true };
    private void Run(Action action) { try { action(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Chưa thể thực hiện"); } }
    public void LamMoi()
    {
        master.Items.Clear(); master.Columns.Clear(); detail.Items.Clear(); detail.Columns.Clear();
        try
        {
            bus.TaiLai();
            foreach (var name in new[] { "Mã HĐ", "Ngày lập", "Mã KH", "Mã NV", "Tổng tiền" }) master.Columns.Add(name, 145);
            foreach (var group in bus.LayDuLieu().DoanhThu.GroupBy(x => x.MaHD, StringComparer.OrdinalIgnoreCase))
            {
                var first = group.First();
                master.Items.Add(new ListViewItem(new[] { first.MaHD, first.NgayLap.ToString("dd/MM/yyyy"), first.MaKH, first.MaNV,
                    group.Sum(x => ThongKeBUS.ThanhTien(x, thamSo.SoLeTien)).ToString("N" + thamSo.SoLeTien) }) { Tag = first.MaHD });
            }
            if (master.Items.Count > 0) master.Items[0].Selected = true;
            total.Text = $"Có {master.Items.Count} hóa đơn. Chọn hóa đơn để xem các dòng CTHD.";
            ChiTiet();
        }
        catch (Exception ex) { total.Text = "Chưa tải được số liệu: " + ex.Message; }
    }
    private List<DongDoanhThu> DaChon()
    {
        if (master.SelectedItems.Count == 0) return new();
        var id = (string)master.SelectedItems[0].Tag!;
        return bus.LayDuLieu().DoanhThu.Where(x => x.MaHD.Equals(id, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.STT).ToList();
    }
    private void ChiTiet()
    {
        detail.Items.Clear(); detail.Columns.Clear();
        foreach (var name in new[] { "STT", "Mã sản phẩm/vé", "Số lượng", "Đơn giá", "Giảm (%)", "Thành tiền" }) detail.Columns.Add(name, 130);
        foreach (var x in DaChon()) detail.Items.Add(new ListViewItem(new[] { x.STT.ToString(), x.MaSanPham, x.SoLuong.ToString(),
            x.DonGia.ToString("N" + thamSo.SoLeTien), x.GiamGia.ToString(CultureInfo.InvariantCulture),
            ThongKeBUS.ThanhTien(x, thamSo.SoLeTien).ToString("N" + thamSo.SoLeTien) }));
    }
    private void Template<T>(string name)
    {
        using var file = new SaveFileDialog { Filter = "Excel (*.xls)|*.xls", FileName = name };
        if (file.ShowDialog(this) == DialogResult.OK) ExcelService.Xuat(file.FileName, new List<T>());
    }
    private void Import(bool doanhThu)
    {
        using var file = new OpenFileDialog { Filter = "Excel (*.xls;*.xlsx)|*.xls;*.xlsx" };
        if (file.ShowDialog(this) != DialogResult.OK) return;
        if (MessageBox.Show("Thay toàn bộ tập số liệu " + (doanhThu ? "CTHD" : "phiếu chi")
            + " dùng cho thống kê bằng file này? Tập còn lại được giữ nguyên.", "Nhập số liệu", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        if (doanhThu) bus.NhapDoanhThu(ExcelService.Nhap<DongDoanhThu>(file.FileName));
        else bus.NhapChiPhi(ExcelService.Nhap<DongChiPhi>(file.FileName));
        LamMoi();
    }
    private void XuatHoaDon()
    {
        var rows = DaChon(); if (rows.Count == 0) throw new ArgumentException("Chọn một hóa đơn trước.");
        using var file = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = "HoaDon.pdf" };
        if (file.ShowDialog(this) != DialogResult.OK) return;
        var output = rows.Select(x => new[] { x.STT.ToString(), x.MaSanPham, x.SoLuong.ToString(), x.DonGia.ToString("N" + thamSo.SoLeTien),
            x.GiamGia.ToString(CultureInfo.InvariantCulture), ThongKeBUS.ThanhTien(x, thamSo.SoLeTien).ToString("N" + thamSo.SoLeTien) }).ToList();
        output.Add(new[] { "", "TỔNG CỘNG", "", "", "", rows.Sum(x => ThongKeBUS.ThanhTien(x, thamSo.SoLeTien)).ToString("N" + thamSo.SoLeTien) });
        var first = rows[0];
        PdfService.XuatBang(file.FileName, $"HÓA ĐƠN {first.MaHD}\nNgày {first.NgayLap:dd/MM/yyyy} · KH {first.MaKH} · NV {first.MaNV}",
            new[] { "STT", "Sản phẩm / vé", "Số lượng", "Đơn giá", "Giảm (%)", "Thành tiền" }, output,
            thamSo.Lay("TEN_DON_VI") + "\n" + thamSo.Lay("DIA_CHI") + " · " + thamSo.Lay("SDT"));
    }
}
