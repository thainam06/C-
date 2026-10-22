# Quản lý máy bay

Ứng dụng C# WinForms quản lý Máy bay, Ghế và Hạng vé.

## Mở bằng Visual Studio

1. Dùng Windows và Visual Studio 2022 phiên bản 17.8 trở lên, cài workload
   **.NET desktop development** và .NET 8 SDK.
2. Mở file **QuanLyMayBay.csproj** trong repository này bằng
   **File → Open → Project/Solution**. Không cần tạo project hoặc Form1 mới.
3. Trong Solution Explorer, nhấp phải project **QuanLyMayBay** →
   **Set as Startup Project**, rồi nhấn **F5**.

Nếu đang dùng project Form1 tự tạo trước đó, hãy mở project mới ở bước 2.
Nếu Git báo file cục bộ sẽ bị ghi đè khi pull, hãy sao lưu project cũ hoặc clone
repository vào một thư mục mới trước khi mở project này.

## Sử dụng

- **Máy bay:** nhập mã máy bay và mã hãng hàng không.
  Bấm **Nạp M01–M10** để thêm nhanh 10 máy bay mẫu:
  M01–M03 thuộc VN, M04–M06 thuộc VJ, M07–M08 thuộc QH, M09–M10 thuộc BL.
  Chỉ thêm mã còn thiếu (không phân biệt hoa/thường), không ghi đè mã đã có.
  Dữ liệu mẫu được lưu ngay vào JSON và có thể sửa/xóa như dữ liệu nhập tay.
- **Hạng vé:** nhập mã, tên, hành lý (kg) và đơn giá vé (VNĐ).
- **Ghế:** tạo máy bay và hạng vé trước, sau đó chọn chúng trong danh sách.
  Mã ghế là duy nhất trong từng máy bay; hai máy bay có thể cùng có ghế `1A`.
- **Thêm:** tạo dòng mới. **Nhập mới:** xóa ô nhập và thoát chế độ sửa.
- Chọn một dòng trong bảng để **Sửa** hoặc **Xóa**. Khóa của dòng không được sửa;
  với ghế, khóa gồm mã máy bay và mã ghế.
- **Tìm kiếm:** tìm theo mã/tên, không phân biệt chữ hoa và chữ thường.
  Với ghế có thể tìm bằng mã ghế, mã máy bay hoặc mã hạng vé.
- Không cho xóa máy bay/hạng vé đang có ghế liên quan. Hành lý và đơn giá không âm.

Dữ liệu tự lưu sau mỗi lần thêm/sửa/xóa tại
`%LOCALAPPDATA%\QuanLyMayBay\dulieu.json` và được đọc lại khi mở ứng dụng.
Nếu file hỏng hoặc không đọc được, ứng dụng báo lỗi và đóng, không ghi đè file.
Chỉ chạy một phiên ứng dụng tại một thời điểm khi sửa cùng file dữ liệu.

## Cấu trúc

- `DTO`: `MayBay`, `Ghe`, `HangVe` chứa dữ liệu.
- `GUI`: `MainForm` và các tab nhập liệu/hiển thị.
- `BUS`: kiểm tra dữ liệu, thao tác quản lý và tìm kiếm trên `ArrayList`.
- `DAO`: đọc/ghi JSON cục bộ, chưa kết nối cơ sở dữ liệu.

Đây là giao diện và các chức năng quản lý cơ bản cho ba đối tượng trên,
chưa phải toàn bộ đồ án trong PDF. Khi có lược đồ CSDL, cần bổ sung `MyConnection`
và thay DAO JSON bằng DAO kết nối CSDL. `MHHK` hiện nhập dạng mã văn bản;
chưa kiểm tra tham chiếu hãng hàng không vì chưa có danh mục tương ứng.

## Build

```powershell
dotnet build QuanLyMayBay.csproj -c Release
dotnet run --project QuanLyMayBay.csproj
```

Giao diện WinForms chỉ chạy trên Windows. Có thể kiểm tra biên dịch trên Linux
nhờ `EnableWindowsTargeting`, nhưng không thể chạy giao diện tại đó.
