using System.ComponentModel;
using System.Reflection;

namespace QuanLyMayBay.GUI;

internal record Truong(string ThuocTinh, string Nhan, bool Khoa = false,
    bool So = false, Func<List<string>>? LuaChon = null);

internal class QuanLyTab<T> : UserControl where T : class, new()
{
    private readonly Truong[] truong;
    private readonly Dictionary<string, Control> oNhap = new();
    private readonly Func<string, List<T>> tim;
    private readonly Action<T, bool> luu;
    private readonly Action<T> xoa;
    private readonly DataGridView bang = new();
    private readonly TextBox tuKhoa = new();
    private readonly Label thongBao = new();
    private readonly Button them;
    private readonly Button sua;
    private readonly Button nutXoa;
    private T? dangChon;
    private bool dangNap;

    public QuanLyTab(Truong[] truong, Func<string, List<T>> tim,
        Action<T, bool> luu, Action<T> xoa)
    {
        this.truong = truong;
        this.tim = tim;
        this.luu = luu;
        this.xoa = xoa;
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, truong.Length * 42 + 12));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = truong.Length };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < truong.Length; i++)
        {
            var field = truong[i];
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            editor.Controls.Add(new Label
            {
                Text = field.Nhan, AutoSize = true, Anchor = AnchorStyles.Left
            }, 0, i);
            Control input;
            if (field.So)
                input = new NumericUpDown
                {
                    DecimalPlaces = 2, Minimum = 0, Maximum = decimal.MaxValue,
                    ThousandsSeparator = true
                };
            else if (field.LuaChon != null)
                input = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            else input = new TextBox { MaxLength = 255 };
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 7, 3, 3);
            input.AccessibleName = field.Nhan;
            oNhap.Add(field.ThuocTinh, input);
            editor.Controls.Add(input, 1, i);
        }

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        them = Nut("Thêm", (_, _) => ThucHien(() =>
        {
            luu(DocForm(), false);
            LamMoi();
            Bao("Đã thêm và lưu dữ liệu.");
        }));
        sua = Nut("Sửa", (_, _) => ThucHien(() =>
        {
            if (dangChon == null) return;
            luu(DocForm(), true);
            LamMoi();
            Bao("Đã cập nhật và lưu dữ liệu.");
        }));
        nutXoa = Nut("Xóa", (_, _) => ThucHien(() =>
        {
            if (dangChon == null) return;
            if (MessageBox.Show("Xóa dòng đang chọn?", "Xác nhận xóa",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            xoa(dangChon);
            LamMoi();
            Bao("Đã xóa và lưu dữ liệu.");
        }));
        buttons.Controls.AddRange(new Control[]
        {
            them, sua, nutXoa, Nut("Nhập mới", (_, _) => LamMoi())
        });

        var search = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        tuKhoa.Dock = DockStyle.Fill;
        tuKhoa.Margin = new Padding(3, 8, 3, 3);
        tuKhoa.PlaceholderText = "Nhập mã hoặc tên cần tìm…";
        tuKhoa.AccessibleName = "Từ khóa tìm kiếm";
        tuKhoa.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            ThucHien(NapBang);
        };
        search.Controls.Add(tuKhoa, 0, 0);
        search.Controls.Add(Nut("Tìm kiếm", (_, _) => ThucHien(NapBang)), 1, 0);
        search.Controls.Add(Nut("Hiện tất cả", (_, _) =>
        {
            tuKhoa.Clear();
            ThucHien(NapBang);
        }), 2, 0);

        bang.Dock = DockStyle.Fill;
        bang.ReadOnly = true;
        bang.AllowUserToAddRows = false;
        bang.AllowUserToDeleteRows = false;
        bang.AutoGenerateColumns = false;
        bang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        bang.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        bang.MultiSelect = false;
        bang.RowHeadersVisible = false;
        bang.BackgroundColor = Color.White;
        bang.BorderStyle = BorderStyle.FixedSingle;
        bang.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250);
        foreach (var field in truong)
        {
            var column = new DataGridViewTextBoxColumn
            {
                DataPropertyName = field.ThuocTinh, HeaderText = field.Nhan,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            if (field.So) column.DefaultCellStyle.Format = "N2";
            bang.Columns.Add(column);
        }
        bang.SelectionChanged += (_, _) =>
        {
            if (dangNap || bang.SelectedRows.Count == 0) return;
            if (bang.SelectedRows[0].DataBoundItem is not T item) return;
            dangChon = item;
            foreach (var field in truong)
            {
                var value = ThuocTinh(field).GetValue(item);
                var input = oNhap[field.ThuocTinh];
                if (input is NumericUpDown number) number.Value = (decimal)value!;
                else if (input is ComboBox combo) combo.SelectedItem = (string)value!;
                else input.Text = (string)value!;
                input.Enabled = !field.Khoa;
            }
            them.Enabled = false;
            sua.Enabled = nutXoa.Enabled = true;
            Bao("Đang sửa dòng đã chọn. Bấm Nhập mới để thêm dòng khác.");
        };

        thongBao.Dock = DockStyle.Fill;
        thongBao.TextAlign = ContentAlignment.MiddleLeft;
        layout.Controls.Add(editor, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        layout.Controls.Add(search, 0, 2);
        layout.Controls.Add(bang, 0, 3);
        layout.Controls.Add(thongBao, 0, 4);
        Controls.Add(layout);
        LamMoi();
    }

    private static PropertyInfo ThuocTinh(Truong field) => typeof(T).GetProperty(field.ThuocTinh)!;

    private static Button Nut(string text, EventHandler click)
    {
        var button = new Button { Text = text, Width = 116, Height = 34, Margin = new Padding(3, 5, 6, 3) };
        button.Click += click;
        return button;
    }

    private T DocForm()
    {
        var item = new T();
        foreach (var field in truong)
        {
            var input = oNhap[field.ThuocTinh];
            object value = input is NumericUpDown number ? number.Value : input.Text.Trim();
            ThuocTinh(field).SetValue(item, value);
        }
        return item;
    }

    private void XoaForm()
    {
        dangChon = null;
        foreach (var field in truong)
        {
            var input = oNhap[field.ThuocTinh];
            input.Enabled = true;
            if (input is NumericUpDown number) number.Value = 0;
            else if (input is ComboBox combo) combo.SelectedIndex = -1;
            else input.Text = "";
        }
        them.Enabled = true;
        sua.Enabled = nutXoa.Enabled = false;
    }

    private void NapBang()
    {
        dangNap = true;
        try
        {
            var data = tim(tuKhoa.Text);
            bang.DataSource = new BindingList<T>(data);
            bang.ClearSelection();
            bang.CurrentCell = null;
            XoaForm();
            Bao($"Có {data.Count} dòng. Chọn một dòng để sửa hoặc xóa.");
        }
        finally { dangNap = false; }
    }

    public void LamMoi()
    {
        foreach (var field in truong.Where(x => x.LuaChon != null))
        {
            var combo = (ComboBox)oNhap[field.ThuocTinh];
            combo.DataSource = field.LuaChon!();
            combo.SelectedIndex = -1;
        }
        tuKhoa.Clear();
        NapBang();
    }

    private void Bao(string text)
    {
        thongBao.ForeColor = Color.FromArgb(30, 58, 95);
        thongBao.Text = text;
    }

    private void ThucHien(Action action)
    {
        try { action(); }
        catch (Exception ex)
        {
            thongBao.ForeColor = Color.Firebrick;
            thongBao.Text = "Thao tác chưa thành công.";
            MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
