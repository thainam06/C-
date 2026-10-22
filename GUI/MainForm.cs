using QuanLyMayBay.BUS;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.GUI;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Quản lý máy bay";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 650);
        Size = new Size(1100, 760);
        Font = new Font("Segoe UI", 10);

        QuanLyBUS bus;
        try
        {
            bus = new QuanLyBUS();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể đọc dữ liệu. Chương trình sẽ đóng để bảo vệ file hiện tại.\n"
                + ex.Message, "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Shown += (_, _) => Close();
            return;
        }

        var tabs = new TabControl { Dock = DockStyle.Fill };
        var mayBay = new QuanLyTab<MayBay>(
            new[]
            {
                new Truong("MaMayBay", "Mã máy bay", Khoa: true),
                new Truong("MHHK", "Mã hãng hàng không")
            },
            q => bus.TimMayBay(q).Cast<MayBay>().ToList(),
            (x, sua) => bus.LuuMayBay(x.MaMayBay, x.MHHK, sua),
            x => bus.XoaMayBay(x.MaMayBay));

        mayBay.ThemNut("Nạp M01–M10", () =>
        {
            var soDaThem = bus.NapMayBayMau();
            MessageBox.Show(soDaThem == 0
                ? "Đã có đủ mã M01–M10. Dữ liệu hiện tại được giữ nguyên."
                : $"Đã thêm và lưu {soDaThem} máy bay mẫu. Các mã đã có được giữ nguyên.",
                "Dữ liệu mẫu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });

        var hangVe = new QuanLyTab<HangVe>(
            new[]
            {
                new Truong("MaHV", "Mã hạng vé", Khoa: true),
                new Truong("TenHV", "Tên hạng vé"),
                new Truong("HanhLyKg", "Hành lý (kg)", So: true),
                new Truong("DonGiaVe", "Đơn giá vé (VNĐ)", So: true)
            },
            q => bus.TimHangVe(q).Cast<HangVe>().ToList(),
            (x, sua) => bus.LuuHangVe(x.MaHV, x.TenHV, x.HanhLyKg, x.DonGiaVe, sua),
            x => bus.XoaHangVe(x.MaHV));

        var ghe = new QuanLyTab<Ghe>(
            new[]
            {
                new Truong("MaGhe", "Mã ghế", Khoa: true),
                new Truong("MaMayBay", "Máy bay", Khoa: true,
                    LuaChon: () => bus.TimMayBay().Cast<MayBay>().Select(x => x.MaMayBay).ToList()),
                new Truong("MaHV", "Hạng vé",
                    LuaChon: () => bus.TimHangVe().Cast<HangVe>().Select(x => x.MaHV).ToList())
            },
            q => bus.TimGhe(q).Cast<Ghe>().ToList(),
            (x, sua) => bus.LuuGhe(x.MaGhe, x.MaMayBay, x.MaHV, sua),
            x => bus.XoaGhe(x.MaGhe, x.MaMayBay));

        foreach (var (ten, noiDung) in new (string, Control)[]
        {
            ("Máy bay", mayBay), ("Hạng vé", hangVe), ("Ghế", ghe)
        })
        {
            var page = new TabPage(ten) { Padding = new Padding(12) };
            page.Controls.Add(noiDung);
            tabs.TabPages.Add(page);
        }
        tabs.SelectedIndexChanged += (_, _) =>
        {
            // Nap lai cac ma tham chieu khi chuyen tu may bay/hang ve sang ghe.
            if (tabs.SelectedIndex == 2) ghe.LamMoi();
        };

        var title = new Label
        {
            Text = "QUẢN LÝ MÁY BAY · GHẾ · HẠNG VÉ",
            Dock = DockStyle.Top, Height = 66, Padding = new Padding(20, 18, 0, 0),
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            BackColor = Color.FromArgb(30, 58, 95), ForeColor = Color.White
        };
        var note = new Label
        {
            Text = "Tạo máy bay và hạng vé trước khi thêm ghế. Dữ liệu tự lưu trên máy này.",
            Dock = DockStyle.Bottom, Height = 38, Padding = new Padding(16, 8, 0, 0)
        };
        Controls.Add(tabs);
        Controls.Add(title);
        Controls.Add(note);
    }
}
