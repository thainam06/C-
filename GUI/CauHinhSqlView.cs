using QuanLyMayBay.DAO;
using QuanLyMayBay.Services;
namespace QuanLyMayBay.GUI;
internal class CauHinhSqlView : UserControl
{
    public CauHinhSqlView()
    {
        Dock = DockStyle.Fill;
        var help = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Text = "KẾT NỐI MYSQL\r\n\r\n"
            + "1. Xem docs/KET_NOI_MYSQL.md. Cần cấu trúc MySQL thực tế của nhóm.\r\n"
            + "2. Đặt QLMB_MYSQL_CONNECTION trên máy chạy ứng dụng. Không commit mật khẩu.\r\n"
            + "3. Đặt QLMB_MYSQL_MAPPING_FILE trỏ tới file JSON ánh xạ DTO → bảng/cột thực tế.\r\n"
            + "4. Khởi động lại ứng dụng. Khi không cấu hình CSDL, ứng dụng dùng JSON cục bộ.\r\n\r\n"
            + "Không tự tạo/sửa CSDL nhóm và không tự nạp dữ liệu mẫu vào MySQL.\r\n"
            + "File mysql-mapping.example.json chỉ là mẫu, cần đối chiếu tên bảng/cột, kiểu và khóa thực tế.\r\n\r\n"
            + "Trạng thái: " + (MyConnection.DungMySql ? "MySQL" : MyConnection.DaCauHinh ? "SQL Server (cấu hình cũ)" : "JSON cục bộ") };
        var test = new Button { Dock = DockStyle.Top, Height = 38, Text = "Kiểm tra kết nối CSDL" };
        test.Click += (_, _) =>
        {
            try { MessageBox.Show("Kết nối thành công, CSDL: " + MyConnection.KiemTra()); }
            catch { MessageBox.Show("Chưa kết nối được. Kiểm tra server, database, quyền truy cập, chứng chỉ và biến kết nối. Không gửi mật khẩu vào chat."); }
        };
        var schema = new Button { Dock = DockStyle.Top, Height = 38, Text = "Xuất cấu trúc CSDL (không xuất dữ liệu người dùng)" };
        schema.Click += (_, _) =>
        {
            using var file = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = "BanVeMayBay.schema.local.json" };
            if (file.ShowDialog(this) != DialogResult.OK) return;
            try { SqlSchemaService.XuatCauTruc(file.FileName); MessageBox.Show("Đã xuất tên bảng/cột, kiểu dữ liệu, PK/FK. Không có dữ liệu khách hàng hoặc chuỗi kết nối."); }
            catch { MessageBox.Show("Chưa xuất được cấu trúc. Kiểm tra kết nối SQL, quyền xem metadata và chứng chỉ TLS."); }
        };
        Controls.Add(help); Controls.Add(schema); Controls.Add(test);
    }
}
