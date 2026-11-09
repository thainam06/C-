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
