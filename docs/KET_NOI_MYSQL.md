# MySQL qua DAO và form ListView

Ứng dụng dùng MySqlConnector trong lớp `MyConnection`; các DAO gọi `CSDLListDAO` để đọc/ghi bằng lệnh có tham số, giao dịch và kiểm tra phiên dữ liệu. GUI chỉ gọi BUS, không chứa SQL. Các bảng giao diện dùng WinForms ListView (Details), chọn dòng để sửa/xóa; vẫn có lọc, xuất Excel/PDF.

Nhóm đã có CSDL MySQL riêng: **không chạy script tạo bảng hoặc tự nạp JSON vào CSDL**. File `mysql-mapping.example.json` là ví dụ theo DTO, chưa xác nhận khớp CSDL của nhóm. Cần đối chiếu DDL: tên bảng/cột phân biệt hoa thường tùy server, kiểu dữ liệu, khóa chính, khóa ngoại, bảng InnoDB, cột bắt buộc/auto increment, UNIQUE. Không dùng tiền tố `dbo` của SQL Server trong MySQL. Những bảng/view chưa có sẽ báo lỗi riêng khi mở màn hình tương ứng.

## Cấu hình trên Windows

Mở PowerShell tại repository, cấu hình trong cửa sổ đó rồi chạy app:

```powershell
cd "C:\Users\boydy\source\repos\c#\C-"
$env:QLMB_MYSQL_CONNECTION = Read-Host "Nhap chuoi ket noi MySQL tren may ban"
$env:QLMB_MYSQL_MAPPING_FILE = "$PWD\mysql-mapping.local.json"
dotnet run --project .\QuanLyMayBay.csproj
```

Trước đó sao chép `docs/mysql-mapping.example.json` thành `mysql-mapping.local.json` và sửa theo DDL. File local đã được gitignore. Không nhập mật khẩu trong chat hoặc commit vào git. `Read-Host` nhập tại máy, không lưu chuỗi thật trong lệnh lịch sử; chuỗi vẫn tồn tại trong bộ nhớ phiên PowerShell. Đóng cửa sổ khi dùng xong. Nếu chạy bằng Visual Studio, cần mở VS từ phiên có những biến môi trường này hoặc cấu hình biến người dùng rồi mở lại VS.

Mẫu chuỗi (thay bằng tài khoản có quyền trên database của nhóm):

```text
Server=localhost;Port=3306;Database=BanVeMayBay;User ID=TEN_TAI_KHOAN;Password=MAT_KHAU;SslMode=VerifyFull;SslCa=C:\duong-dan\ca.pem
```

Dùng chứng chỉ CA của MySQL và hostname khớp chứng chỉ. Đây là MySQL: không dùng `Integrated Security` hoặc `TrustServerCertificate` của SQL Server. MySQL trên Unix có thể dùng socket cục bộ qua `ConnectionProtocol=UnixSocket;Server=/duong-dan/mysql.sock;...`. Không thay chính sách TLS của server để xử lý lỗi kết nối.

Khi có `QLMB_MYSQL_CONNECTION`, ứng dụng ưu tiên MySQL. Lỗi kết nối/ánh xạ sẽ báo lỗi, **không chuyển âm thầm sang JSON**. Không cấu hình MySQL và SQL Server thì ứng dụng dùng JSON cục bộ như trước; SQL Server cũ vẫn được hỗ trợ để tránh mất khả năng dùng cấu hình cũ.

## Xuất cấu trúc để ghép đúng CSDL

Sau khi đặt biến kết nối, chọn **Kết nối CSDL → Kiểm tra kết nối → Xuất cấu trúc CSDL**. Hoặc:

```powershell
dotnet run --project .\tools\SqlInspector\SqlInspector.csproj -- --schema .\BanVeMayBay.schema.local.json
```

Chỉ xuất metadata bảng/cột, kiểu, PK/FK từ information_schema; không xuất dữ liệu người dùng hay chuỗi kết nối. Có thể gửi file metadata này hoặc file `.sql` chỉ chứa DDL để hoàn thiện mapping. Tài khoản cần quyền xem metadata.

## Quy tắc lưu

- Mã DTO dạng chuỗi yêu cầu khóa CSDL cũng là chuỗi; không tự đổi khóa số thành chuỗi rồi ghi đè.
- Lưu nhiều bảng trong cùng giao dịch Serializable: ghi cha trước, xóa con trước. FK/UNIQUE hoặc lỗi lưu sẽ rollback; không tắt foreign_key_checks.
- So sánh dữ liệu đã đọc với CSDL trước khi lưu; nếu phiên khác sửa thì yêu cầu tải lại, tránh ghi đè.
- Trường `Ghe.LoaiGhe` có thể suy ra từ `MaHV` qua `DerivedColumns`, chỉ dùng nếu CSDL không lưu cột đó. HV01 → Ghế thường/Hạng phổ thông; HV02 → Ghế VIP/Hạng thương gia.
- Báo cáo cần hai view `vDoanhThuThongKe`, `vChiPhiThongKe` với các trường trong mapping. Chưa có DDL hóa đơn thực tế thì chưa thể ghép các view chính xác. Phần này chỉ đọc.
