# Tích hợp 5 file DAO tải lên mới

- Thay phần CRUD của HanhKhach/NhanVien/KhachHang/Voucher/CTKM bằng nội dung từ 5 file người dùng gửi, theo lựa chọn "chỉnh tối thiểu để chạy trong project".
- Chỉnh namespace/provider/tên DTO cho project, decimal giảm giá, NULL ngày sinh và cột LoaiKH → LoaiHK qua mapping; giữ tương thích BUS/Doc/Luu và các kiểm tra đã có.
- Lưu nguyên 5 bản gốc ở examples/DAO-nguoi-dung; kiểm tra SHA-256 từng file trùng byte với file tải lên. Loại file tham khảo khỏi Compile của WinForms; không biên dịch namespace/provider của project khác.
- Build WinForms Release 0 lỗi/0 cảnh báo. 171 kiểm tra hồi quy qua; 92 kiểm tra schema/CRUD và 20 kiểm tra MySQL chung qua trên MySQL 8.4 riêng, Unix socket, không dùng CSDL nhóm.
- Đã kiểm tra đọc/thêm/sửa/xóa 5 DAO tích hợp cùng các DAO còn lại, mapping tùy chỉnh, giá trị NULL/decimal, lỗi FK/UNIQUE, snapshot và rollback. Chưa bấm thử WinForms trên Windows.

---

# Viết lại DAO có đầy đủ SQL trong từng hàm theo file mẫu

- Đã thay các hàm chuyển tiếp Lenh.DocDS/Them/Sua/Xoa bằng thân hàm CRUD trực tiếp trong cả 10 DAO: MySqlConnection từ GetConnection, try mở kết nối, SQL + MySqlCommand + tham số, reader/while tạo DTO, catch thông báo lỗi giữ InnerException, finally đóng kết nối.
- DocDS là đường đọc MySQL thật khi BUS gọi Doc. DocDS ở 6 DAO có Luu vẫn ghi nhận snapshot. Luu nhiều bảng giữ giao dịch chung, kiểm tra xung đột và rollback.
- Build WinForms Release **0 lỗi/0 cảnh báo**; **171 kiểm tra hồi quy qua**, **92 kiểm tra schema/CRUD qua**, **20 kiểm tra MySQL chung qua**. Cả hai bộ MySQL dùng MySQL 8.4 trong container riêng, không mở cổng mạng và không dùng CSDL nhóm.
- Đã kiểm tra lại CRUD từng DAO, mapping tùy chỉnh, ngày NULL, đọc INT vào decimal, dấu nháy/Unicode, FK/UNIQUE, giới hạn độ dài/độ chính xác, bản ghi không tồn tại, snapshot DocDS→Luu và rollback/xung đột phiên.
- Các lỗi FK/UNIQUE ở CRUD trực tiếp nay được bọc InvalidOperationException có MySqlException gốc trong InnerException theo cấu trúc catch của mẫu. Bộ kiểm tra đã được chỉnh theo hợp đồng này; các lỗi validation giữ ArgumentException.
- Chưa chạy thao tác GUI WinForms trên Windows; nghiệp vụ bán/hủy vé và thống kê chi phí chưa được bổ sung trong lần sửa cấu trúc DAO này.

---

# Cập nhật DAO theo mẫu HangHangKhongDAO của nhóm

- 10 DAO danh mục có DocDS/Them/Sua/Xoa; bổ sung MyConnection.GetConnection trả kết nối MySQL chưa mở. Các reader và câu lệnh này cũng được dùng trong đường Doc/Luu của BUS, không chỉ là ví dụ rời.
- **92 kiểm tra MySqlSchemaChecks qua**, bao gồm 49 kiểm tra schema trước đó và 43 kiểm tra CRUD/khả năng tương thích mới: thêm/sửa/xóa cả 10 danh mục, tiếng Việt/dấu nháy, số thập phân và ngày NULL, lỗi khóa ngoại/trùng mã, độ dài/độ chính xác, dòng không tồn tại, sửa không đổi dữ liệu, mapping tên bảng/cột tùy chỉnh và DocDS → Luu.
- **20 kiểm tra MySqlChecks qua** sau refactor: kiểm tra xung đột và rollback giao dịch nhiều bảng vẫn hoạt động. Hai bộ dùng MySQL 8.4 riêng qua Unix socket, không dùng CSDL nhóm.
- **171 kiểm tra hồi quy qua**; build WinForms Release cuối cùng 0 lỗi/0 cảnh báo.
- Đã gặp và sửa lỗi ép kiểu khi đọc HanhLyKg INT vào DTO decimal trong lượt thử đầu. Lượt cuối dùng chuyển đổi số đúng kiểu và cả hai bộ MySQL đã qua. Kiểm tra decimal so sánh theo giá trị, không so sánh chuỗi JSON 12.5 với 12.50.
- Chưa bấm thử WinForms trên Windows. Phần nghiệp vụ đặt/bán/hủy vé và nguồn chi phí vẫn giữ trạng thái chưa hoàn tất.

Xem DAO_THEO_MAU.md về cách gọi API. Các hàm CRUD trực tiếp ghi từng dòng, không có token phiên ở hàm Sua; giao diện qua BUS vẫn dùng kiểm tra phiên dữ liệu khi Luu danh sách. Không tự thay các quy tắc BUS bằng lời gọi DAO từ GUI.

---

# Kiểm tra DDL BanVeMayBay.sql được cung cấp

- Đã nhận file nguồn SQL Server, chỉ có schema; chuyển thành `docs/mysql/BanVeMayBay.mysql.sql`, nhập thành công bằng MySQL 8.4 vào database test mới.
- Giữ 15 bảng, 23 FK, 13 CHECK; kiểm tra PK đủ từng bảng, số cột, UNIQUE chuyến/ghế, DEFAULT tiếng Việt/GETDATE, cột tính ThanhTien.
- **49 kiểm tra MySqlSchemaChecks qua** trên chính schema đã chuyển. Bao gồm profile mặc định đóng gói, LoaiKH → LoaiHK, NULL ngày sinh qua DAO/BUS, trường tùy chọn NULL, mật khẩu băm vừa VARCHAR(255), kiểm tra phần lẻ INT/DECIMAL(18,0) và độ dài VARCHAR trước khi lưu.
- **171 kiểm tra hồi quy qua**, **20 kiểm tra MySqlChecks qua**. Build WinForms Release 0 lỗi/0 cảnh báo. Không chạy các build chung Core đồng thời.
- Các test MySQL dùng container riêng không mở port mạng, Unix socket; database `qlmb_schema_test` và `qlmb_dao_test` chỉ chứa dữ liệu giả lập. Không kết nối/sửa CSDL của nhóm.
- Chưa chuyển dữ liệu SQL Server thực tế; không kiểm tra WinForms bằng thao tác trên Windows; không xem việc import schema là hoàn thành nghiệp vụ đặt/bán/hủy vé hay thống kê chi phí. Báo cáo DOCX cũ chưa cập nhật.

Chạy bộ mới: tạo database test trống tên **qlmb_schema_test**, nhập `docs/mysql/BanVeMayBay.mysql.sql` vào đó, đặt `QLMB_SCHEMA_TEST_CONNECTION` tới database test, rồi `dotnet run --project Tests/MySqlSchemaChecks/Checks.csproj -c Release`. Bộ test thêm dữ liệu giả lập; muốn chạy lại cần chuẩn bị database test trống và nhập schema lại. Tuyệt đối không dùng dữ liệu thật. Không cấu hình biến test thì báo SKIP.

---

# Cập nhật MySQL và ListView ngày 09/11/2026

- Build WinForms Release: 0 lỗi, 0 cảnh báo; đã đổi các bảng GUI còn lại từ DataGridView sang ListView.
- 171 kiểm tra hồi quy Core/ngoại tuyến tiếp tục qua.
- **20 kiểm tra MySQL thật qua**, dùng MySQL 8.4 trong Docker riêng, không mở port mạng, kết nối Unix socket. CSDL fixture `qlmb_dao_test`, không dùng CSDL nhóm.
- Đã kiểm tra DAO insert/update/delete, Unicode, tham số hóa chuỗi chứa SQL, CHAR key, decimal/ngày, loại ghế suy diễn, stale session, lỗi UNIQUE không cập nhật nhầm khóa, FK, rollback nhiều bảng, xóa con trước cha, xuất metadata information_schema, chặn ghi bảng không phải InnoDB.
- MySqlConnector 2.6.2; restore/build không có cảnh báo lỗ hổng NuGet.
- Chưa có DDL MySQL của nhóm, chưa kiểm chứng ánh xạ/báo cáo với CSDL thực tế. Mẫu mapping là ví dụ cần đối chiếu. Chưa chạy bấm WinForms trên Windows; build không thay thế kiểm thử giao diện.
- Báo cáo DOCX cũ chưa cập nhật nội dung MySQL; vẫn là bản nháp.

Chạy hồi quy: `bash tools/test.sh`.
Kiểm tra MySQL tùy chọn: đặt `QLMB_MYSQL_TEST_CONNECTION` trỏ tới **CSDL test riêng tên qlmb_dao_test đã tạo**, rồi `dotnet run --project Tests/MySqlChecks/Checks.csproj -c Release`.
Test MySQL xóa/tạo các bảng fixture tại database test này; tuyệt đối không dùng dữ liệu thật. Không có biến test thì báo SKIP. Đây là kiểm tra adapter, không xác nhận schema riêng của nhóm.

---

# Kết quả kiểm tra ngày 08/11/2026

## Đã thực hiện trong môi trường Linux

- Build `QuanLyMayBay.csproj -c Release`: thành công, 0 lỗi/0 cảnh báo.
- KhachHangChecks: 29 kiểm tra qua.
- HanhKhachChecks: 28 kiểm tra qua.
- NhanVienKhuyenMaiChecks: 47 kiểm tra qua.
- DoAnChecks: 45 kiểm tra qua.
- SqlProfileChecks: 22 kiểm tra ngoại tuyến qua (profile theo ảnh, cột suy diễn, CHAR padding).
- Tổng: **171 kiểm tra Core/ngoại tuyến**, không có test GUI hoặc test kết nối SQL thật trong số này.

Các kiểm tra mới bao gồm cú pháp bộ lọc (AND/OR/NOT, ngày, số, nhiều field),
field không hợp lệ/mật khẩu, XLS OLE thật, giữ số 0 đầu/Unicode/decimal,
không xuất công thức từ chuỗi bắt đầu bằng `=`, không nhập formula cells,
ngày Excel dạng số, nhập theo lô không dở dang, tham số và thống kê quý/nhiều khóa.
PDF được mở lại bằng trình đọc thư viện; xuất nhiều trang, font tiếng Việt nhúng.

Báo cáo DOCX được đặt Times New Roman 13, không có trang mục lục; render kiểm tra được 11 trang, dưới giới hạn 20. Đây vẫn là bản nháp do thiếu thông tin nhóm và ảnh thực tế.

File Excel mẫu trong examples được sinh từ dữ liệu kiểm thử giả lập. Không có dữ
liệu tài chính hay thông tin cá nhân thật được thêm tự động vào ứng dụng.

Một lượt build song song ban đầu bị tranh chấp DLL Core (CS2012). Đã chờ các build
kết thúc và chạy riêng lại bộ mới; kết quả trên là lượt đã hoàn tất, không tính lượt lỗi.

## Chưa kiểm chứng / chưa hoàn tất

- Không chạy cửa sổ WinForms vì môi trường không phải Windows. Các bước cần bấm
  thực tế được ghi trong KIEM_THU_WINDOWS.md.
- QLMB_SQL_CONNECTION và QLMB_SQL_MAPPING_FILE đều chưa có trong máy kiểm tra.
  Ảnh đã cho biết localhost/BanVeMayBay và một phần schema, nhưng chưa có lược đồ đầy đủ,
  nên chưa kiểm tra live SQL, PK/FK/rollback SQL,
  adapter view thống kê và transaction bán/hủy vé/tồn ghế.
- SQL mapping đã kiểm tra ngoại tuyến; việc quote identifier được test, nhưng
  điều đó không thay thế kiểm tra kết nối và nghiệp vụ trên CSDL thật.
- Báo cáo cuối kỳ còn cần thông tin nhóm, ảnh GUI và ảnh SQL thực tế.
