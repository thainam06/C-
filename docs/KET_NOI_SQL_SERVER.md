# SQL Server và lớp MyConnection

Nhóm đã chọn dùng CSDL SQL Server hiện có. Chưa nhận được script `CREATE TABLE`,
tên server/database hoặc view hóa đơn của nhóm, nên kết nối và giao dịch SQL thực tế
**chưa được kiểm chứng**. Không có thao tác DDL tự động lên CSDL nhóm.

## Cấu hình

1. Lấy lược đồ thực tế: tên schema/bảng/cột, kiểu dữ liệu, PK, FK, unique,
   identity/computed columns; thêm định nghĩa hóa đơn–CTHD, đặt/bán/hủy vé và tồn ghế.
2. Sao chép `docs/sql-mapping.example.json` thành `sql-mapping.local.json` rồi
   sửa `Table`, `Columns`, `KeyColumns` đúng lược đồ. File mẫu chỉ minh họa,
   **không phải bằng chứng rằng CSDL nhóm có các bảng tên đó**.
3. Cấu hình trong PowerShell trên máy chạy ứng dụng, ví dụ với Windows Authentication:

```powershell
$env:QLMB_SQL_CONNECTION = 'Server=TEN_MAY\INSTANCE;Database=TEN_CSDL;Integrated Security=True;Encrypt=True;TrustServerCertificate=False;Connect Timeout=10'
$env:QLMB_SQL_MAPPING_FILE = 'C:\duong-dan\sql-mapping.local.json'
dotnet run --project QuanLyMayBay.csproj
```

Không đưa mật khẩu vào repo/chat. Nếu dùng SQL Authentication, nhập kết nối qua
cấu hình môi trường bảo mật trên máy chạy. Khi lỗi chứng chỉ, dùng chứng chỉ CA
được máy tin cậy; không tắt kiểm tra TLS để làm kết nối vượt qua lỗi.

Ứng dụng dùng JSON khi `QLMB_SQL_CONNECTION` chưa được đặt. Khi đã đặt biến này,
DAO chuyển sang SQL; lỗi ánh xạ/kết nối được báo, không âm thầm ghi sang JSON.
Chế độ SQL không tự nạp/đổi dữ liệu mẫu M01–M10 trong CSDL của nhóm.

## Quy tắc ánh xạ

- Khóa `Columns` là tên property DTO; giá trị là tên cột SQL, không phải biểu thức SQL.
- `Table` dùng dạng `schema.table` hoặc view có cột phù hợp; phần tên được quote bằng `[]`.
- `KeyColumns` dùng tên property DTO và phải khớp khóa thật. Ghế hiện có khóa
  nghiệp vụ `(MaMayBay, MaGhe)`; nếu lược đồ nhóm khác cần điều chỉnh BUS tương ứng.
- DAO danh mục cần SELECT/INSERT/UPDATE/DELETE. Bảng/view có identity/computed
  columns hoặc không khớp DTO cần adapter riêng sau khi đọc lược đồ, không đoán mapping.
- Các cột tiếng Việt phải dùng `NVARCHAR`; tham số truy vấn dùng `SqlParameter`.
  Không nối chuỗi dữ liệu thành SQL. Chuỗi `?useUnicode=...` trong PDF là kiểu MySQL,
  **không thêm vào connection string SQL Server**.
- `KhachHang.MatKhau` chứa hash PBKDF2, cần cột đủ dài (khuyến nghị NVARCHAR(256));
  không chuyển sang mật khẩu nguyên văn để hợp với cột ngắn.
- Mỗi lần lưu dùng transaction Serializable và đối chiếu snapshot đã đọc. Nếu phiên
  khác thay đổi bảng, ứng dụng từ chối ghi đè; đóng/mở lại để đọc dữ liệu mới.
  `DuLieuDAO` ghi Máy bay/Hạng vé/Ghế trong cùng transaction; lỗi PK/FK rollback cả lô.

## Số liệu hóa đơn/thống kê

`DongDoanhThu` và `DongChiPhi` chỉ đọc từ hai view được ánh xạ. File mẫu dùng
`dbo.vDoanhThuThongKe` và `dbo.vChiPhiThongKe`, nhưng nhóm phải cung cấp view/query
phù hợp với bảng hóa đơn, chi tiết và phiếu chi/phiếu nhập thật.

- Doanh thu: MaHD, STT, NgayLap, MaKH, MaNV, MaSanPham, SoLuong, DonGia, GiamGia (%).
- Chi phí: MaPC, NgayChi, MaNV, NoiDung, SoTien.
- Không nhập Excel vào hai view qua màn thống kê ở chế độ SQL. Chứng từ phải được
  ghi qua nghiệp vụ của CSDL nhóm, rồi màn báo cáo tải lại.
- Chưa có adapter bán/đặt/hủy vé và cập nhật ghế trống: cần lược đồ và quy tắc
  nghiệp vụ thật để viết transaction, khóa ghế và kiểm tra trùng vé an toàn.

Nếu chưa có SQL, màn Hóa đơn–Chi tiết cho nhập XLS đúng mẫu để thử các phép tính.
Đó là tập số liệu báo cáo, không phải chức năng bán vé hay nguồn chứng từ đã xác thực.

## Việc cần làm trên máy có SQL Server

Mở menu **Kết nối SQL Server** để thử `SELECT DB_NAME()` (chỉ đọc). Sau khi điền
mapping thật, kiểm tra từng danh mục, PK/FK, rollback và hai view báo cáo bằng dữ liệu
kiểm thử của nhóm. Các kiểm tra tự động trong repo chỉ chứng minh chế độ JSON/Core
và kiểm tra mapping ngoại tuyến, không chứng minh kết nối SQL Server thành công.
