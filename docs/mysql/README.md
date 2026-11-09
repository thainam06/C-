> Cập nhật DTO mới: namespace VeMayBay, MySql.Data.MySqlClient, VoucherDTO.GiamGia dùng double và ngày sinh DateTime bắt buộc. NgaySinh NULL trong schema cần bổ sung giá trị thật trước khi đọc bằng DTO mới; hướng dẫn checkbox/nullable của phiên bản cũ bên dưới không còn áp dụng. Xem ../DONG_BO_MAU_DAO_DTO.md.

# CSDL MySQL chuyển từ BanVeMayBay.sql của nhóm

Nguồn là **DDL SQL Server**, không có INSERT dữ liệu. Script `BanVeMayBay.mysql.sql` chuyển đủ 15 bảng, 23 FK, 13 CHECK, các UNIQUE/default/index và cột tính ThanhTien. Không bao gồm lệnh/tùy chọn hệ thống SQL Server như GO, đường dẫn MDF/LDF, broker, compatibility level.

## Nhập bằng MySQL Workbench

1. Cài/chạy **MySQL Server 8.4** và kết nối bằng MySQL Workbench. SSMS và SQL Server hiện có là hệ quản trị khác, không chạy được script MySQL này.
2. Tạo một schema **mới, trống**, ví dụ `BanVeMayBayMySql`, chọn charset utf8mb4 và collation utf8mb4_0900_ai_ci. Nhấp đúp schema để đặt làm schema mặc định.
3. File → Open SQL Script → mở `docs/mysql/BanVeMayBay.mysql.sql`; chạy toàn bộ script. Script không chứa USE hay DROP, không đổi/xóa CSDL SQL Server cũ. Nếu có bảng trùng tên thì dừng để kiểm tra, không bật chế độ bỏ qua lỗi.
4. Nếu muốn dùng màn Tham số và thiết lập PDF, chạy thêm `ThamSo.optional.sql` trong cùng schema. Đây là bảng bổ sung của ứng dụng, **không có trong file gốc**. App sẽ tạo các giá trị tham số mặc định ở lần mở đầu tiên; không tạo dữ liệu bán vé mẫu.
5. Đặt chuỗi kết nối như bên dưới rồi chạy project WinForms.

MySQL DDL tự commit từng lệnh. Nếu nhập bị lỗi thì có thể đã tạo một phần bảng; kiểm tra CSDL mới trước khi chạy lại. Script không tự xóa bảng để làm lại.

## Chạy ứng dụng trên Windows

```powershell
cd "C:\Users\boydy\source\repos\c#\C-"
$env:QLMB_MYSQL_CONNECTION = Read-Host "Nhap chuoi ket noi MySQL"
Remove-Item Env:QLMB_MYSQL_MAPPING_FILE -ErrorAction SilentlyContinue
dotnet run --project .\QuanLyMayBay.csproj
```

Ví dụ chuỗi nhập tại máy (thay user/password/đường dẫn CA):

```text
Server=localhost;Port=3306;Database=BanVeMayBayMySql;User ID=TEN_USER;Password=MAT_KHAU;SslMode=VerifyFull;SslCa=C:\certs\mysql-ca.pem
```

Ứng dụng đóng gói `mysql-mapping.BanVeMayBay.json`. Thứ tự ưu tiên: biến `QLMB_MYSQL_MAPPING_FILE` → file `%LOCALAPPDATA%\QuanLyMayBay\mysql-mapping.json` nếu có → profile đóng gói. Nếu đã có file cũ trong AppData, cập nhật file đó hoặc đặt biến trỏ thẳng tới `$PWD\docs\mysql-mapping.BanVeMayBay.json`. Chuỗi kết nối và CA phải thuộc MySQL; không dùng Windows Authentication của SQL Server. Không gửi mật khẩu lên chat/git. Nếu muốn chạy F5, mở Visual Studio từ phiên có biến môi trường hoặc đặt biến người dùng rồi mở lại VS.

## Ánh xạ và khác biệt

- `MayBay.MHHK → MayBay.MaHHK`; `HanhKhach.LoaiKH → HanhKhach.LoaiHK`.
- Ghe có đúng ba cột vật lý MaGhe/MaMayBay/MaHV, khóa chính MaGhe toàn bảng. LoaiGhe suy ra từ MaHV; HV01 thường/phổ thông, HV02 VIP/thương gia.
- DDL nguồn cho phép NgaySinh NULL nhưng DTO mới không cho phép: cần bổ sung ngày thật trước khi mở danh sách; không tự thay bằng ngày hôm nay. Các chuỗi SQL NULL được biểu diễn bằng chuỗi trống trong DTO hiện tại.
- Mở dữ liệu hiện có chấp nhận các trường tùy chọn NULL (SDT/Email/DiaChi khách hàng, GioiTinh/LoaiHK hành khách, ChucVu nhân viên). Khi thêm/sửa qua form, BUS vẫn yêu cầu thông tin như trước; điền các trường bắt buộc trên form trước khi lưu. Khách hàng cần mật khẩu băm PBKDF2 theo định dạng của app; không tự chuyển mật khẩu dữ liệu cũ.
- HanhLyKg là INT; DonGiaVe/giá vé trong nguồn là DECIMAL(18,0). DAO chặn phần lẻ trước khi ghi thay vì để MySQL làm tròn. Kiểm tra chiều dài chuỗi theo cột thật. Voucher.GiamGia vẫn là phần trăm theo yêu cầu, BUS giới hạn lớn hơn 0 đến 100; file gốc chỉ ràng buộc không âm.
- NVARCHAR/VARCHAR chuyển thành VARCHAR utf8mb4 để giữ tiếng Việt. Collation MySQL không thể bảo đảm mọi phép so sánh y hệt collation SQL Server (file nguồn không ghi collation cụ thể); cần rà soát mã trùng trước khi chuyển dữ liệu thật.
- SQL Server DATETIME → DATETIME(3); DATE giữ nguyên; TIME(7) → TIME(6) vì MySQL tối đa 6 chữ số phần giây. Có thể giảm độ chính xác 100 ns nếu sau này chuyển dữ liệu thời gian; file hiện chỉ có DDL.
- ThanhTien → DECIMAL(29,0) GENERATED ALWAYS AS (SL * DonGiaVe) STORED; GETDATE → CURRENT_TIMESTAMP(3). Không ghi trực tiếp vào cột tính.
- Chuyển cấu trúc không tự chuyển dữ liệu SQL Server sang MySQL. Script không thêm hãng, máy bay, khách hàng, hóa đơn mẫu; cần có HangHangKhong trước khi thêm MayBay.

## Phần chưa có trong file gốc

Nguồn có HoaDon/ChiTietHoaDon/DatVe/Ve nhưng không có bảng chi phí, bảng ThamSo hoặc view vDoanhThuThongKe/vChiPhiThongKe. Profile mới không giả lập các view này. Các màn hóa đơn/thống kê hiện phụ thuộc adapter view nên sẽ báo thiếu nguồn, cần ghép tiếp theo quy tắc thực tế: trạng thái hóa đơn được tính, GiamGia của DatVe là phần trăm hay tiền và cách phân bổ theo dòng. Chưa coi chi phí bằng 0 hoặc lợi nhuận bằng doanh thu.

Các bảng ChuyenBay/Ve/DatVe đã có trong schema chuyển đổi, nhưng việc này chưa triển khai giao dịch đặt/bán/hủy vé và cập nhật tồn ghế trong app. Không chạy script thay đổi dữ liệu thật để mô phỏng đã hoàn tất nghiệp vụ.

## Kiểm tra

Đã nhập thành công vào MySQL 8.4 riêng qua Unix socket và kiểm tra DAO/ràng buộc từ chính schema này, không dùng CSDL nhóm. Bộ `Tests/MySqlSchemaChecks` yêu cầu database test `qlmb_schema_test` trống, nhập script chính trước; test chỉ thêm dữ liệu giả lập vào database test này. Xem KET_QUA_KIEM_THU.md. Giao diện WinForms cần bấm thử trên Windows.
