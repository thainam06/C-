using QuanLyMayBay.DAO;
using QuanLyMayBay.Services;
namespace QuanLyMayBay.GUI;
internal class CauHinhSqlView : UserControl
{
    public CauHinhSqlView()
    {
        Dock = DockStyle.Fill;
        var help = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
            Text = "KẾT NỐI SQL SERVER\r\n\r\n"
            + "1. Xem docs/KET_NOI_SQL_SERVER.md. Cần lược đồ thực tế của nhóm.\r\n"
            + "2. Cấu hình biến môi trường QLMB_SQL_CONNECTION trên máy chạy ứng dụng. Không commit mật khẩu.\r\n"
            + "3. Cấu hình QLMB_SQL_MAPPING_FILE trỏ tới file ánh xạ DTO → bảng/cột thực tế.\r\n"
            + "4. Khởi động lại ứng dụng. Khi chưa có chuỗi SQL, ứng dụng dùng JSON cục bộ.\r\n\r\n"
            + "Không tự tạo/sửa CSDL nhóm, không tự chép dữ liệu mẫu JSON vào SQL.\r\n"
            + "Giữ Encrypt=True và kiểm tra chứng chỉ; Unicode dùng NVARCHAR và tham số SqlCommand.\r\n"
            + "Không thêm ?useUnicode=... vào chuỗi SQL Server (đó là cú pháp MySQL).\r\n\r\n"
            + "Trạng thái cấu hình SQL: " + (MyConnection.DaCauHinh ? "Đã có biến kết nối" : "Chưa có, đang dùng JSON") };
        var test = new Button { Dock = DockStyle.Top, Height = 38, Text = "Kiểm tra kết nối SQL (chỉ đọc DB_NAME)" };
        test.Click += (_, _) =>
        {
            try { MessageBox.Show("Kết nối thành công, CSDL: " + MyConnection.KiemTra()); }
            catch { MessageBox.Show("Chưa kết nối được. Kiểm tra server, database, quyền truy cập, chứng chỉ và biến QLMB_SQL_CONNECTION. Không gửi mật khẩu vào chat."); }
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
