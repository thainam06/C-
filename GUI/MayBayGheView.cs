using System.ComponentModel;
using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

// Mot bang hien thi thong tin may bay, ghe va hang ve tren cung mot dong.
internal class MayBayGheView : UserControl
{
    private readonly QuanLyBUS bus;
    private readonly DataGridView bang = new();
    private readonly TextBox tuKhoa = new();

    public MayBayGheView(QuanLyBUS bus)
    {
        this.bus = bus;
        Dock = DockStyle.Fill;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var search = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        tuKhoa.Dock = DockStyle.Fill;
        tuKhoa.Margin = new Padding(3, 8, 3, 3);
        tuKhoa.PlaceholderText = "Tìm mã máy bay, mã hãng, mã ghế, mã hoặc tên hạng vé…";
        tuKhoa.AccessibleName = "Tìm kiếm bảng tổng hợp";
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

        bang.Dock = DockStyle.Fill;
        bang.ReadOnly = true;
        bang.AllowUserToAddRows = false;
        bang.AllowUserToDeleteRows = false;
        bang.AutoGenerateColumns = false;
        bang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        bang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        bang.MultiSelect = false;
        bang.BackgroundColor = Color.White;
        bang.BorderStyle = BorderStyle.FixedSingle;
        bang.RowHeadersWidth = 28;
        bang.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250);
        ThemCot("MaMayBay", "Mã máy bay");
        ThemCot("MHHK", "Mã hãng HK");
        ThemCot("MaGhe", "Mã ghế");
        ThemCot("MaHV", "Mã hạng vé");
        ThemCot("TenHV", "Tên hạng vé");
        ThemCot("HanhLyKg", "Hành lý (kg)", "N2");
        ThemCot("DonGiaVe", "Đơn giá vé (VNĐ)", "N2");

        layout.Controls.Add(search, 0, 0);
        layout.Controls.Add(bang, 0, 1);
        Controls.Add(layout);
        Load += (_, _) => LamMoi();
    }

    public void LamMoi()
    {
        var danhSach = bus.LayBangTongHop(tuKhoa.Text).Cast<DongTongHop>().ToList();
        bang.DataSource = new BindingList<DongTongHop>(danhSach);
        bang.ClearSelection();
        bang.CurrentCell = null;
    }

    private void ThemCot(string property, string title, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = property, HeaderText = title, MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        column.DefaultCellStyle.NullValue = "";
        if (format != null)
        {
            column.DefaultCellStyle.Format = format;
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        bang.Columns.Add(column);
    }
}
