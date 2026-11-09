namespace VeMayBay.GUI;

internal class ChonDuLieuForm : Form
{
    // Bien static theo yeu cau do an; reset moi lan mo/cancel de khong lay nham lua chon cu.
    public static MucLuaChon? DaChon;
    public ChonDuLieuForm(List<MucLuaChon> options)
    {
        DaChon = null;
        Text = "Tìm và chọn dữ liệu"; Size = new Size(650, 430);
        StartPosition = FormStartPosition.CenterParent;
        var query = new TextBox { Dock = DockStyle.Top, PlaceholderText = "Tìm mã hoặc tên…" };
        var list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false };
        list.Columns.Add("Mã", 150); list.Columns.Add("Thông tin", 440);
        var button = new Button { Text = "Chọn", Dock = DockStyle.Bottom, Height = 36 };
        void Bind()
        {
            list.Items.Clear();
            foreach (var x in options.Where(x => x.HienThi.Contains(query.Text.Trim(), StringComparison.OrdinalIgnoreCase)
                || x.GiaTri.Contains(query.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                var item = new ListViewItem(new[] { x.GiaTri, x.HienThi }) { Tag = x };
                list.Items.Add(item);
            }
        }
        void Choose()
        {
            if (list.SelectedItems.Count == 0) return;
            DaChon = (MucLuaChon)list.SelectedItems[0].Tag!;
            DialogResult = DialogResult.OK;
        }
        query.TextChanged += (_, _) => Bind(); button.Click += (_, _) => Choose();
        list.DoubleClick += (_, _) => Choose();
        FormClosed += (_, _) => { if (DialogResult != DialogResult.OK) DaChon = null; };
        Controls.Add(list); Controls.Add(query); Controls.Add(button); Bind();
    }
}
