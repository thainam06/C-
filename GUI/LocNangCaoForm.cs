namespace VeMayBay.GUI;

internal class LocNangCaoForm : Form
{
    public string BieuThuc { get; private set; } = "";
    public LocNangCaoForm(string current, IEnumerable<string> fields, Action<string> validate)
    {
        Text = "Lọc nâng cao"; Size = new Size(750, 330); StartPosition = FormStartPosition.CenterParent;
        var editor = new TextBox { Dock = DockStyle.Fill, Multiline = true, Text = current };
        var help = new Label
        {
            Text = "Trường: " + string.Join(", ", fields) + "\nAND, OR, NOT; =, <>, >, >=, <, <=; ~ chứa chuỗi. Có thể dùng ngoặc.\n"
                + "Chuỗi/ngày đặt trong dấu nháy, ngày yyyy-MM-dd hoặc dd/MM/yyyy. Số dùng dấu chấm.\nVí dụ: GiamGia >= 10 AND NOT (MaVoucher ~ 'TEST')",
            Dock = DockStyle.Top, Height = 125
        };
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42 };
        var apply = new Button { Text = "Áp dụng", Width = 110 };
        var clear = new Button { Text = "Bỏ bộ lọc", Width = 110 };
        apply.Click += (_, _) =>
        {
            try { validate(editor.Text); BieuThuc = editor.Text; DialogResult = DialogResult.OK; }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Biểu thức chưa hợp lệ"); }
        };
        clear.Click += (_, _) => { BieuThuc = ""; DialogResult = DialogResult.OK; };
        buttons.Controls.AddRange(new Control[] { apply, clear });
        Controls.Add(editor); Controls.Add(help); Controls.Add(buttons);
    }
}
