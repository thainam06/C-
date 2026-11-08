# Đối chiếu C-w7-YeuCauDoAn.pdf

Trạng thái ngày 08/11/2026. Chưa thể gọi là hoàn thành toàn bộ đồ án khi các mục
phụ thuộc lược đồ SQL, giao dịch vé và báo cáo cuối kỳ còn thiếu thông tin/ảnh thực tế.

| Mục PDF | Kết quả trong mã nguồn | Kiểm chứng / phần còn thiếu |
|---|---|---|
| 1. GUI–BUS–DAO, MyConnection | Tách Core chứa DTO/BUS/DAO/Services, GUI WinForms; thêm MyConnection và adapter SQL ánh xạ | Build qua; chưa nối SQL thật do thiếu lược đồ/cấu hình |
| 2. Tìm trên ArrayList tại BUS | Các BUS hiện có + LocNangCaoBUS | Test AND/OR/NOT, so sánh số/ngày/chuỗi |
| 3. Form main–sub, hai ListView | Chính–Chi tiết: Máy bay–Ghế, KH–Hành khách, CTKM–Voucher; Hóa đơn–CTHD | Mã đã build; tương tác GUI cần kiểm tra Windows |
| 4. Nút ... và biến static | ChonDuLieuForm.DaChon, reset khi mở/cancel; điền mã vào ComboBox | Cần chạy GUI Windows, không khẳng định đã click kiểm chứng |
| 5. Mã duy nhất | BUS kiểm tra và PK/unique thật của SQL khi mapping đúng; SQL lưu có chống ghi đè snapshot | Test JSON/logic qua; PK/FK SQL thực tế còn chờ |
| 6. Field thay đổi khi bán/nhập | Chưa có bảng/quy tắc đặt–bán–hủy vé của nhóm | **Chưa làm transaction tồn ghế**. Không giảm số ghế vật lý để giả lập bán vé |
| 7. Field không nhập | Bảng readonly; khóa không sửa; loại ghế/tên HV tự điền; tổng tiền/quý/lợi nhuận tính trong BUS | Logic kiểm tra qua; giao diện cần chạy Windows |
| 8. Lọc nâng cao | AND, OR, NOT, ngoặc; =, <>, !=, >, >=, <, <=, ≥, ≤; ~ chứa chuỗi; nhiều field | Test cả cú pháp sai, ngày/số sai, bảo vệ field MatKhau |
| 9. Table tham số | ThamSo DTO/BUS/DAO/View; tên đơn vị, địa chỉ, SDT, SO_LE_TIEN | JSON đã kiểm tra; bảng SQL thật cần mapping |
| 10. Thống kê 3 bảng tiền | Thu/chi/lợi nhuận; quý NV/KH/SP; ngày/tháng/quý/năm; khoảng ngày; KH+SP; NV+SP+Năm | Test phép tính với dữ liệu kiểm thử; nguồn SQL hóa đơn/phiếu chi còn chờ view thật |
| 11. Export/import XLS | NPOI 2.7.6; xuất XLS ở danh mục; nhập theo lô NV/CTKM/Voucher; XLS cho số liệu báo cáo | File OLE XLS thật, giữ mã số 0 đầu/Unicode; file sai không nhập dở; không xuất mật khẩu |
| 12. Report PDF | PDF danh mục, thống kê và hóa đơn được chọn; font tiếng Việt nhúng, nhiều trang | Test file PDF thật, kiểm tra chữ tiếng Việt; chưa có dữ liệu hóa đơn SQL thật |
| 13. Desktop/menu trái/form con | MainForm dùng menu trái và vùng nội dung | Build qua; chưa có ảnh ứng dụng chạy Windows |
| 14. Báo cáo ≤20 trang, TNR 13 | docs/BaoCaoDoAn_Nhap.docx, nội dung theo I–V, không mục lục | **Bản nháp**: thiếu tên/MSSV/lớp, ảnh GUI thật, ảnh SQL và link sản phẩm cuối |
| 15. Unicode CSDL | SqlParameter và hướng dẫn NVARCHAR; không dùng hậu tố MySQL cho SQL Server | Cần kiểm tra kiểu cột thực tế của nhóm |
| 16. Timeline | Ghi lại mốc tuần 9–12 trong hướng dẫn nộp | Thông tin kế hoạch, không phải chức năng ứng dụng |

## Thông tin đang chờ từ nhóm

1. Đã biết localhost/BanVeMayBay và một phần schema từ ảnh; cần script CREATE TABLE/PK/FK,
   kiểu dữ liệu, các bảng hóa đơn/CTHD, đặt/bán/hủy vé,
   ghế theo chuyến và phiếu nhập/chi. Không cần gửi mật khẩu vào chat.
2. Quy tắc cập nhật ghế khi bán/hủy/đổi vé, thuế/phí/giảm giá thực tế.
3. Tên đề tài đã xác nhận: **Quản lý bán vé máy bay**. Họ tên/MSSV các thành viên và lớp vẫn chưa có.
4. Ảnh màn hình chạy Windows và ảnh CSDL thực tế để hoàn thiện báo cáo.
