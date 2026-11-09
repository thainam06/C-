## Sau khi đổi DAO theo mẫu

- Cấu hình MySQL rồi mở từng danh mục, chọn dòng và thử thêm/sửa/xóa qua BUS như trước.
- Đọc Hạng vé có HanhLyKg kiểu INT không được báo lỗi ép kiểu sang decimal; Hành khách ngày sinh NULL vẫn phải hiển thị trống.
- Thử tên có dấu nháy và tiếng Việt; lưu lại phải giữ nguyên chữ.
- Thử hai phiên ứng dụng cùng sửa: phiên lưu sau phải báo xung đột thay vì ghi đè. Việc có các hàm DAO CRUD mới không bỏ kiểm tra này khỏi luồng BUS.

## Kiểm tra theo DDL đã cung cấp

- Nhập script MySQL vào schema mới qua Workbench theo docs/mysql/README.md, cấu hình kết nối và mở project gốc.
- Thêm/đọc Hành khách: cột nguồn LoaiHK hiển thị ở trường LoaiKH; bỏ tick ngày sinh, lưu và mở lại phải vẫn trống, không tự hiện ngày hôm nay.
- Thêm Hạng vé với hành lý 20.5 hoặc giá vé 1000000.25 phải báo không khớp độ chính xác cột, không tự làm tròn; nhập số nguyên phải lưu được.
- Mã vượt 10 ký tự phải bị chặn; khi lưu lỗi các dữ liệu trước đó không đổi.
- Nếu dùng Tham số, nhập script bổ sung ThamSo.optional.sql một lần. Màn thống kê/hóa đơn chưa có adapter nguồn báo cáo từ file gốc, cần ghép tiếp theo docs/mysql/README.md.

## Kiểm tra bổ sung MySQL/ListView

Sau khi cấu hình theo KET_NOI_MYSQL.md và ghép đúng DDL:

- Mở từng danh mục: bảng là ListView Details, cột và tiếng Việt đầy đủ, có thanh cuộn.
- Chọn dòng để sửa/xóa; Nhập mới phải bỏ chọn dòng và cho nhập khóa mới. Chọn lại cùng dòng vẫn điền form được.
- Hạng vé HV01/HV02 điền đúng loại ghế, bộ lọc và popup chọn dữ liệu vẫn hoạt động.
- Xuất XLS/PDF từ danh sách đã lọc; mật khẩu không hiển thị/xuất.
- Bảng tổng hợp và 4 bảng thống kê dùng ListView, số tiền theo tham số; xuất báo cáo đúng danh sách đang xem.
- Mở 2 phiên, cùng đọc rồi sửa: phiên lưu sau phải báo xung đột nếu dữ liệu đã thay đổi.
- Sai kết nối hoặc thiếu mapping phải báo lỗi, không tự chuyển sang JSON.

Các bước này cần kiểm tra trên máy Windows thật, chưa được chạy trong môi trường Linux.

# Kiểm tra trên Windows

1. Mở **QuanLyMayBay.csproj**, restore các gói NuGet, chọn project này làm Startup Project.
   Không chạy project c# cũ đang gom cả source con; project cũ không có các dependency mới.
2. Chạy F5: menu trái phải chuyển được giữa các màn hình; các ô nhập và thanh công cụ
   không che bảng ở độ phân giải màn hình đang dùng.
3. Màn Chính–Chi tiết: lần lượt thử 3 quan hệ, chọn hai dòng chính khác nhau; bảng dưới
   chỉ hiện chi tiết theo đúng mã, không khớp chuỗi một phần (M01 không lẫn M010).
4. Nút ...: tìm và chọn mã; hủy rồi mở lại phải không lấy lựa chọn static từ lần trước.
   HV01 tự điền Ghế thường, HV02 tự điền Ghế VIP và không cho sửa trái hạng.
5. Lọc nâng cao: thử `GiamGia >= 10 AND NOT (MaVoucher ~ 'TEST')` tại Voucher;
   thử điều kiện ngày tại CTKM. Bỏ bộ lọc phải hiện lại toàn bộ danh sách.
6. Nhân viên: Mẫu XLS → điền file → nhập; thử file trùng mã/sai field ở dòng cuối,
   xác nhận không có các dòng đầu được nhập dở. File đã có mã sẽ cập nhật đúng mã.
7. Xuất XLS/PDF, mở bằng Excel và trình đọc PDF: mã giữ số 0 đầu, tiếng Việt rõ;
   không có MatKhau/hash trong file khách hàng xuất ra.
8. Menu Tham số: đổi TEN_DON_VI, DIA_CHI, SDT, SO_LE_TIEN rồi xuất PDF hóa đơn/
   thống kê để kiểm tra giá trị tham số thực sự được dùng.
9. Chế độ JSON: nhập examples/CTHDMau.xls và PhieuChiMau.xls qua Hóa đơn–Chi tiết
   (dữ liệu kiểm thử giả lập). Chọn từng HĐ để xem CTHD, xuất PDF HĐ.
10. Thống kê khoảng 01/01/2026–31/12/2026, năm quý 2026: mẫu có thu 4.100, chi 1.300,
    lợi nhuận 2.800; đối chiếu ba bảng quý và các nhóm nhiều khóa.
11. SQL: chỉ thực hiện sau khi có mapping/lược đồ thật theo KET_NOI_SQL_SERVER.md.
    Kiểm tra PK/FK/rollback và chống ghi đè hai phiên. Không xem test JSON là bằng chứng SQL.
12. Chụp ảnh thật cho báo cáo. Chưa được đánh dấu phần bán/hủy vé và tồn ghế hoàn tất
    khi chưa có transaction nghiệp vụ tương ứng với CSDL nhóm.

Kiểm tra tự động:

```powershell
.\tools\Test.ps1
```

Tests dùng thư mục tạm và tắt QLMB_SQL_CONNECTION trong tiến trình test, không ghi
dữ liệu mẫu vào CSDL thật. Cần cài .NET 8 SDK và workload .NET desktop development.
