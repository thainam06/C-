using System.Globalization;

namespace VeMayBay.GUI;

internal static class BangListView
{
    internal static ListView Tao() => new()
    {
        Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true,
        GridLines = true, MultiSelect = false, HideSelection = false,
        BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle
    };

    internal static void Nap<T>(ListView list, IEnumerable<T> rows, int soLe = 2)
    {
        list.BeginUpdate();
        try
        {
            list.Items.Clear();
            foreach (var row in rows)
            {
                var cells = list.Columns.Cast<ColumnHeader>().Select(column =>
                {
                    var value = typeof(T).GetProperty(column.Name ?? throw new InvalidOperationException("Thiếu tên cột ListView."))!.GetValue(row);
                    return value switch
                    {
                        DateTime date => date.ToString("dd/MM/yyyy"),
                        double number => number.ToString("N" + soLe, CultureInfo.CurrentCulture),
                        decimal number => number.ToString("N" + soLe, CultureInfo.CurrentCulture),
                        _ => Convert.ToString(value, CultureInfo.CurrentCulture) ?? ""
                    };
                }).ToArray();
                var item = new ListViewItem(cells) { Tag = row };
                if (list.Items.Count % 2 == 1) item.BackColor = Color.FromArgb(240, 245, 250);
                list.Items.Add(item);
            }
        }
        finally { list.EndUpdate(); }
    }

    internal static void NapTuDong<T>(ListView list, List<T> rows, int soLe)
    {
        list.Columns.Clear();
        foreach (var property in typeof(T).GetProperties().Where(p => p.CanRead))
            list.Columns.Add(new ColumnHeader { Name = property.Name, Text = property.Name, Width = 150 });
        list.Tag = rows;
        Nap(list, rows, soLe);
    }
}
