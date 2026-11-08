# Dữ liệu kiểm thử giả lập

Các file XLS ở đây được sinh từ bộ kiểm tra, không chứa số liệu bán hàng thật và
không tự nạp vào ứng dụng. Chỉ nhập khi cần thử chức năng trên tập dữ liệu thử.

- NhanVienMau.xls: hai nhân viên giả lập, có mã 0001/0002 để kiểm tra giữ số 0 đầu.
- CTHDMau.xls: dòng hóa đơn năm 2025/2026 để thử lọc năm/quý và giảm giá phần trăm.
- PhieuChiMau.xls: ba phiếu chi giả lập năm 2026.

Trong chế độ JSON, nhập CTHD/phiếu chi **thay thế toàn bộ tập tương ứng dùng cho
thống kê**, sau bước xác nhận. Dùng dữ liệu thử riêng, không ghi đè tập số liệu thật.
