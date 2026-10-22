using System.ComponentModel;
using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

// Man hinh chinh-chi tiet: chon mot may bay de xem dung cac ghe thuoc may bay do.
internal class MayBayGheView : UserControl
{
    private readonly QuanLyBUS bus;
    private readonly DataGridView bangMayBay = TaoBang();
    private readonly DataGridView bangGhe = TaoBang();
    private readonly TextBox tuKhoa = new();
    private readonly TextBox maMayBay = OThongTin();
    private readonly TextBox maHang = OThongTin();
    private readonly TextBox soGhe = OThongTin();
    private readonly Label trangThai = new();
    private readonly GroupBox nhomGhe = new() { Text = "Ghế của máy bay", Dock = DockStyle.Fill };
    private bool dangNap;

    public MayBayGheView(QuanLyBUS bus)
    {
        this.bus = bus;
        Dock = DockStyle.Fill;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        var search = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tuKhoa.Dock = DockStyle.Fill;
        tuKhoa.Margin = new Padding(3, 8, 3, 3);
        tuKhoa.PlaceholderText = "Tìm mã máy bay hoặc mã hãng hàng không…";
        tuKhoa.AccessibleName = "Tìm máy bay";
        tuKhoa.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            LamMoi();
        };
        var tim = new Button { Text = "Tìm kiếm", Dock = DockStyle.Fill };
        var tatCa = new Button { Text = "Hiện tất cả", Dock = DockStyle.Fill };
        tim.Click += (_, _) => LamMoi();
        tatCa.Click += (_, _) => { tuKhoa.Clear(); LamMoi(); };
        search.Controls.Add(tuKhoa, 0, 0);
        search.Controls.Add(tim, 1, 0);
        search.Controls.Add(tatCa, 2, 0);

        ThemCot(bangMayBay, "MaMayBay", "Mã máy bay");
        ThemCot(bangMayBay, "MHHK", "Mã hãng hàng không");
        bangMayBay.SelectionChanged += (_, _) =>
        {
            if (!dangNap) HienChiTiet();
        };
        var nhomMayBay = new GroupBox
        {
            Text = "Danh sách máy bay — chọn một dòng để xem ghế",
            Dock = DockStyle.Fill, Padding = new Padding(8)
        };
        nhomMayBay.Controls.Add(bangMayBay);

        ThemCot(bangGhe, "MaGhe", "Mã ghế");
        ThemCot(bangGhe, "MaHV", "Mã hạng vé");
        ThemCot(bangGhe, "TenHV", "Tên hạng vé");
        ThemCot(bangGhe, "HanhLyKg", "Hành lý (kg)", "N2");
        ThemCot(bangGhe, "DonGiaVe", "Đơn giá vé (VNĐ)", "N2");
        nhomGhe.Padding = new Padding(8);
        nhomGhe.Controls.Add(bangGhe);

        var lower = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        lower.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var thongTin = new GroupBox
        {
            Text = "Thông tin máy bay đang chọn", Dock = DockStyle.Fill, Padding = new Padding(8)
        };
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        var labels = new[] { "Mã máy bay", "Mã hãng HK", "Số ghế" };
        var inputs = new[] { maMayBay, maHang, soGhe };
        for (var i = 0; i < inputs.Length; i++)
        {
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            fields.Controls.Add(new Label
            {
                Text = labels[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, i);
            inputs[i].AccessibleName = labels[i];
            fields.Controls.Add(inputs[i], 1, i);
        }
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var huongDan = new Label
        {
            Text = "Thêm/sửa/xóa dữ liệu tại các tab Máy bay, Hạng vé và Ghế.",
            Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0)
        };
        fields.Controls.Add(huongDan, 0, 3);
        fields.SetColumnSpan(huongDan, 2);
        thongTin.Controls.Add(fields);
        lower.Controls.Add(nhomGhe, 0, 0);
        lower.Controls.Add(thongTin, 1, 0);

        trangThai.Dock = DockStyle.Fill;
        trangThai.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(search, 0, 0);
        layout.Controls.Add(nhomMayBay, 0, 1);
        layout.Controls.Add(lower, 0, 2);
        layout.Controls.Add(trangThai, 0, 3);
        Controls.Add(layout);
        // Doi control gan vao form de DataGridView co BindingContext khi nap cac dong.
        Load += (_, _) => LamMoi();
    }

    public void LamMoi()
    {
        var maCu = (bangMayBay.CurrentRow?.DataBoundItem as MayBay)?.MaMayBay;
        var danhSach = bus.TimMayBay(tuKhoa.Text).Cast<MayBay>().ToList();
        dangNap = true;
        try
        {
            bangMayBay.DataSource = new BindingList<MayBay>(danhSach);
            bangMayBay.ClearSelection();
            bangMayBay.CurrentCell = null;
            var index = danhSach.FindIndex(x => string.Equals(x.MaMayBay, maCu,
                StringComparison.OrdinalIgnoreCase));
            if (index < 0 && danhSach.Count > 0) index = 0;
            if (index >= 0)
            {
                bangMayBay.CurrentCell = bangMayBay.Rows[index].Cells[0];
                bangMayBay.Rows[index].Selected = true;
            }
        }
        finally { dangNap = false; }
        HienChiTiet();
    }

    private void HienChiTiet()
    {
        var mayBay = bangMayBay.CurrentRow?.DataBoundItem as MayBay;
        var chiTiet = mayBay == null ? new List<ChiTietGhe>()
            : bus.LayGheTheoMayBay(mayBay.MaMayBay).Cast<ChiTietGhe>().ToList();
        bangGhe.DataSource = new BindingList<ChiTietGhe>(chiTiet);
        bangGhe.ClearSelection();
        bangGhe.CurrentCell = null;
        maMayBay.Text = mayBay?.MaMayBay ?? "";
        maHang.Text = mayBay?.MHHK ?? "";
        soGhe.Text = mayBay == null ? "" : chiTiet.Count.ToString();
        nhomGhe.Text = mayBay == null ? "Ghế của máy bay" : $"Ghế của máy bay {mayBay.MaMayBay}";
        trangThai.Text = mayBay == null
            ? "Không có máy bay phù hợp. Thêm dữ liệu ở tab Máy bay hoặc đổi từ khóa tìm kiếm."
            : $"Máy bay {mayBay.MaMayBay}: {chiTiet.Count} ghế. Chọn máy bay khác ở bảng trên để xem chi tiết.";
    }

    private static TextBox OThongTin() => new()
    {
        ReadOnly = true, Dock = DockStyle.Fill, Margin = new Padding(3, 6, 3, 3), TabStop = false
    };

    private static DataGridView TaoBang() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
        RowHeadersWidth = 28,
        AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 245, 250) }
    };

    private static void ThemCot(DataGridView grid, string property, string title, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = property, HeaderText = title, MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        if (format != null) column.DefaultCellStyle.Format = format;
        grid.Columns.Add(column);
    }
}
