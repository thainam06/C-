using System.Collections;
using System.Globalization;
using VeMayBay.BUS;
using VeMayBay.DTO;

namespace VeMayBay.GUI;

internal class MainSubView : UserControl
{
    private readonly ComboBox module = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ListView master = TaoList();
    private readonly ListView detail = TaoList();
    private readonly QuanLyBUS mayBay;
    private readonly KhachHangBUS khachHang;
    private readonly HanhKhachBUS hanhKhach;
    private readonly CTKMBUS ctkm;
    private readonly VoucherBUS voucher;

    public MainSubView(QuanLyBUS mb, KhachHangBUS kh, HanhKhachBUS hk, CTKMBUS km, VoucherBUS v)
    {
        (mayBay, khachHang, hanhKhach, ctkm, voucher) = (mb, kh, hk, km, v);
        Dock = DockStyle.Fill;
        module.Items.AddRange(new object[] { "Máy bay → Ghế", "Khách hàng → Hành khách", "CTKM → Voucher" });
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        var a = new GroupBox { Text = "Bảng chính — chọn một dòng", Dock = DockStyle.Fill };
        var b = new GroupBox { Text = "Các dòng chi tiết thuộc dòng đang chọn", Dock = DockStyle.Fill };
        a.Controls.Add(master); b.Controls.Add(detail); layout.Controls.Add(a, 0, 0); layout.Controls.Add(b, 0, 1);
        master.SelectedIndexChanged += (_, _) => ChiTiet();
        module.SelectedIndexChanged += (_, _) => LamMoi();
        Controls.Add(layout); Controls.Add(module);
        Load += (_, _) => LamMoi();
        module.SelectedIndex = 0;
    }
    public void LamMoi()
    {
        if (module.SelectedIndex == 0) Bind(master, mayBay.TimMayBay(), "MaMayBay", "MHHK");
        else if (module.SelectedIndex == 1) Bind(master, khachHang.TimKiem(), "MaKH", "HoTen", "SDT");
        else Bind(master, ctkm.TimKiem(), "MaCTKM", "TenCTKM", "NgayBatDau", "NgayKetThuc");
        if (master.Items.Count > 0) master.Items[0].Selected = true;
        ChiTiet();
    }
    private void ChiTiet()
    {
        detail.Items.Clear(); detail.Columns.Clear();
        if (master.SelectedItems.Count == 0) return;
        var ma = master.SelectedItems[0].Text;
        if (module.SelectedIndex == 0) Bind(detail, mayBay.LayGheTheoMayBay(ma), "MaGhe", "LoaiGhe", "MaHV", "TenHV", "HanhLyKg", "DonGiaVe");
        else if (module.SelectedIndex == 1) Bind(detail, new ArrayList(hanhKhach.TimKiem().Cast<HanhKhachDTO>()
            .Where(x => x.MaKH.Equals(ma, StringComparison.OrdinalIgnoreCase)).ToList()), "MaHK", "HoTen", "NgaySinh", "GioiTinh", "LoaiKH");
        else Bind(detail, new ArrayList(voucher.TimKiem().Cast<VoucherDTO>()
            .Where(x => x.MaCTKM.Equals(ma, StringComparison.OrdinalIgnoreCase)).ToList()), "MaVoucher", "GiamGia");
    }
    private static ListView TaoList() => new()
    { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false, HideSelection = false, GridLines = true };
    private static void Bind(ListView list, ArrayList rows, params string[] fields)
    {
        list.BeginUpdate(); list.Items.Clear(); list.Columns.Clear();
        foreach (var field in fields) list.Columns.Add(field, field.Contains("Ten") || field == "HoTen" ? 180 : 115);
        foreach (var row in rows)
        {
            var values = fields.Select(x => row.GetType().GetProperty(x)!.GetValue(row)).Select(x => x is DateTime date
                ? date.ToString("dd/MM/yyyy") : Convert.ToString(x, CultureInfo.InvariantCulture) ?? "").ToArray();
            list.Items.Add(new ListViewItem(values) { Tag = row });
        }
        list.EndUpdate();
    }
}
