using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;
using QuanLyMayBay.Services;

namespace QuanLyMayBay.GUI;

internal class BaoCaoView : UserControl
{
    private readonly ThongKeBUS bus;
    private readonly ThamSoBUS thamSo;
    private readonly DateTimePicker from = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 120 };
    private readonly DateTimePicker to = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 120 };
    private readonly NumericUpDown year = new() { Minimum = 1753, Maximum = 9998, Width = 80 };
    private readonly ComboBox group = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 185 };
    private readonly ComboBox period = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly List<ListView> grids = new();
    private readonly Label summary = new() { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(8) };
    public BaoCaoView(ThongKeBUS bus, ThamSoBUS thamSo)
    {
        this.bus = bus; this.thamSo = thamSo; Dock = DockStyle.Fill;
        year.Value = DateTime.Today.Year; from.Value = new DateTime(DateTime.Today.Year, 1, 1); to.Value = new DateTime(DateTime.Today.Year, 12, 31);
        group.Items.AddRange(new object[] { "Nhân viên", "Khách hàng", "Sản phẩm", "Khách hàng + Sản phẩm", "NV + Sản phẩm + Năm" });
        period.Items.AddRange(new object[] { "Ngày", "Tháng", "Quý", "Năm" }); group.SelectedIndex = 0; period.SelectedIndex = 1;
        var tools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 94, WrapContents = true };
        void Button(string text, Action action)
        { var b = new Button { Text = text, Width = 110, Height = 30 }; b.Click += (_, _) => Run(action); tools.Controls.Add(b); }
        tools.Controls.AddRange(new Control[] { new Label { Text = "Từ ngày", AutoSize = true }, from,
            new Label { Text = "Đến ngày", AutoSize = true }, to, new Label { Text = "Năm bảng quý", AutoSize = true }, year, group, period });
        Button("Thống kê", Tinh); Button("Tải số liệu", LamMoi); Button("Xuất XLS", Xls); Button("Xuất PDF", Pdf);
        foreach (var name in new[] { "Theo nhân viên/quý", "Theo khách hàng/quý", "Theo sản phẩm/quý", "Theo kỳ / nhiều khóa" })
        {
            var grid = BangListView.Tao();
            var page = new TabPage(name); page.Controls.Add(grid); tabs.TabPages.Add(page); grids.Add(grid);
        }
        Controls.Add(tabs); Controls.Add(tools); Controls.Add(summary);
    }
    private void Run(Action action)
    { try { action(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Chưa thể lập báo cáo"); } }
    public void LamMoi()
    {
        try { bus.TaiLai(); Tinh(); }
        catch (Exception ex)
        {
            foreach (var grid in grids) { grid.Items.Clear(); grid.Tag = null; }
            summary.Text = "Chưa lấy được số liệu. " + ex.Message;
        }
    }
    private void Tinh()
    {
        var n = thamSo.SoLeTien;
        var total = bus.TongHop(from.Value, to.Value, n);
        for (int i = 0; i < 3; i++) BangListView.NapTuDong(grids[i], bus.TheoQuy((NhomThongKe)i, (int)year.Value, from.Value, to.Value, n), n);
        BangListView.NapTuDong(grids[3], bus.TheoKy((NhomThongKe)group.SelectedIndex, (KyThongKe)period.SelectedIndex, from.Value, to.Value, n), n);
        var data = bus.LayDuLieu();
        summary.Text = $"Thu: {total.Thu.ToString("N" + n)} · Chi: {total.Chi.ToString("N" + n)} · Lợi nhuận: {total.LoiNhuan.ToString("N" + n)} VNĐ. "
            + $"Nguồn: {data.DoanhThu.Count} dòng CTHD, {data.ChiPhi.Count} phiếu chi. Bảng quý: năm {year.Value}.";
    }
    private void Xls()
    {
        using var file = new SaveFileDialog { Filter = "Excel (*.xls)|*.xls", FileName = "ThongKe.xls" };
        if (file.ShowDialog(this) != DialogResult.OK) return;
        var source = grids[tabs.SelectedIndex].Tag;
        if (source is List<BangQuy> quarter) ExcelService.Xuat(file.FileName, quarter);
        else if (source is List<DongThongKe> data) ExcelService.Xuat(file.FileName, data);
        else throw new ArgumentException("Chưa có kết quả thống kê.");
    }
    private void Pdf()
    {
        var grid = grids[tabs.SelectedIndex];
        if (grid.Tag == null) throw new ArgumentException("Chưa có kết quả thống kê.");
        using var file = new SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = "ThongKe.pdf" };
        if (file.ShowDialog(this) != DialogResult.OK) return;
        PdfService.XuatBang(file.FileName, $"THỐNG KÊ {tabs.SelectedTab!.Text}\n{from.Value:dd/MM/yyyy} – {to.Value:dd/MM/yyyy} · Năm bảng quý {(int)year.Value}",
            grid.Columns.Cast<ColumnHeader>().Select(x => x.Text).ToList(),
            grid.Items.Cast<ListViewItem>().Select(x => x.SubItems.Cast<ListViewItem.ListViewSubItem>()
                .Select(c => c.Text).ToArray()).ToList(), DonVi());
    }
    private string DonVi() => thamSo.Lay("TEN_DON_VI") + "\n" + thamSo.Lay("DIA_CHI") + " · " + thamSo.Lay("SDT");
}
