# Đồng bộ theo 11 file DAO/DTO người dùng gửi

Bản chạy dùng namespace **VeMayBay** trên DAO, DTO, BUS, GUI, Services và các công cụ. Các DAO dùng **MySql.Data.MySqlClient** (MySql.Data 26.10.0), như file mẫu. Không còn provider MySqlConnector trong project.

## Các lớp và file

Năm DTO được cung cấp đã có tên file/lớp tương ứng trong DTO/: KhachHangDTO, HanhKhachDTO, NhanVienDTO, CTKMDTO, VoucherDTO. Khai báo thuộc tính và kiểu dữ liệu theo mẫu. Giữ thêm constructor để BUS có thể khởi tạo như trước.

Các danh mục còn lại được đổi nhất quán thành MayBayDTO, HangVeDTO, GheDTO, HangHangKhongDTO và ThamSoDTO. Không còn lớp cũ cùng tên thiếu hậu tố DTO. Việc đổi tên được thực hiện theo symbol, không đổi tên bảng SQL, cột, thuộc tính hoặc khóa JSON.

Sáu DAO KhachHang/HanhKhach/NhanVien/CTKM/Voucher/HangHangKhong theo cấu trúc file gửi: DocDS/Them/Sua/Xoa có SQL, MySqlCommand, đọc/gán DTO và try/catch/finally trực tiếp. Các DAO còn lại dùng cùng provider/kiểu DTO. BUS và GUI gọi các kiểu mới thật sự, không chỉ lưu các file mẫu rời.

Bản gốc cả 11 file ở examples/Mau-DAO-DTO, giữ nguyên byte và loại khỏi Compile. Bản chạy giữ thêm using để tự giải phóng kết nối/reader, tham số SQL, thông tin lỗi gốc, kiểm tra dữ liệu và giao dịch. Riêng mẫu hãng hàng không nối chuỗi được chuyển thành tham số để tên chứa dấu nháy vẫn lưu đúng.

## Kiểu dữ liệu theo DTO mới

| Trường | Kiểu theo mẫu | Hành vi |
| --- | --- | --- |
| VoucherDTO.GiamGia | double | Phần trăm, lớn hơn 0 đến 100, tối đa 2 số lẻ theo DECIMAL(18,2); chặn NaN/vô cực |
| HanhKhachDTO.NgaySinh | DateTime | Bắt buộc, không nullable, form không có lựa chọn bỏ trống |
| CTKMDTO.NgayBatDau/NgayKetThuc | DateTime | Giữ kiểm tra thứ tự ngày |
| Các mã/tên/thông tin liên hệ | string | Khởi tạo chuỗi rỗng theo file mẫu |

Ô NumericUpDown của form chuyển decimal sang double khi ghi VoucherDTO và chuyển ngược khi điền lại; không còn ép kiểu boxing decimal gây lỗi. ListView định dạng double theo số lẻ, lọc và Excel hỗ trợ phần trăm có phần lẻ. Tiền vé và số liệu tài chính khác vẫn dùng decimal như trước.

**Ngày sinh cũ NULL:** file DDL SQL Server/MySQL của nhóm cho phép NULL, nhưng DTO mới yêu cầu DateTime. App báo rõ mã hành khách thiếu ngày sinh, không tự đổi thành hôm nay hoặc xóa bản ghi. Cần bổ sung ngày thật trong CSDL trước khi mở danh sách. Có thể kiểm tra bằng truy vấn chỉ đọc:

```sql
SELECT MaHK, HoTen FROM HanhKhach WHERE NgaySinh IS NULL;
```

JSON cũ chứa NgaySinh:null cũng cần bổ sung ngày đúng trước khi đọc bằng DTO mới; app không tự ghi đè file lỗi. Các chuỗi tùy chọn NULL khác vẫn được đọc thành chuỗi trống.

## Tương thích CSDL và project

- Tên bảng và mapping giữ nguyên: MayBay, Ghe, HangVe, KhachHang... không thêm DTO vào tên bảng/cột.
- Mapper nhận cả tên lớp mới có hậu tố DTO và khóa profile cũ không có hậu tố. Các file mapping hiện có vẫn dùng được.
- LoaiKH của DTO vẫn ánh xạ đúng cột LoaiHK trong DDL người dùng gửi; không tự ALTER TABLE để đổi tên cột.
- Root JSON DuLieu vẫn có MayBay/HangVe/Ghe và các đường dẫn JSON/AppData cũ. Đổi namespace không tự chuyển/xóa dữ liệu.
- Tên project và file chạy vẫn QuanLyMayBay.csproj/QuanLyMayBay.exe để giữ các lệnh đã dùng. RootNamespace là VeMayBay cho mã WinForms tự sinh.
- Mở project gốc QuanLyMayBay.csproj, restore NuGet rồi build; không mở project c# cũ ở thư mục cha.
- Biến môi trường QLMB_MYSQL_CONNECTION và mapping giữ như KET_NOI_MYSQL.md. Chỉ kiểm chứng MySQL 8.4 trong môi trường test riêng; chưa bấm thử WinForms trên Windows.

## Kiểm tra

Build WinForms và CLI: 0 lỗi/0 cảnh báo. 179 kiểm tra hồi quy, 95 kiểm tra schema/CRUD và 20 kiểm tra MySQL chung qua. Có kiểm tra double qua Excel/lọc/ghi DB, chặn NaN/vô cực/độ chính xác sai, bắt buộc ngày sinh khi SQL NULL, FK/UNIQUE, snapshot, giao dịch và rollback. Việc này chưa triển khai thêm nghiệp vụ bán/hủy vé hay nguồn chi phí.
