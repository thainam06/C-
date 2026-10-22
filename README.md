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

Hạng vé mặc định: **HV1 = Ghế thường**, **HV2 = Ghế VIP**. Hai mã này được thêm
tự động (kể cả với dữ liệu cũ). Khi chọn HV1/HV2 trong tab Ghế, loại ghế tự điền
và khóa theo mã; khi đổi hạng vé rồi lưu, loại ghế cũng đổi theo.
Trong tab Hạng vé, tên của HV1/HV2 cũng tự điền và khóa. Có thể sửa hành lý và
đơn giá; không xóa hai hạng vé mặc định. Giá ban đầu minh họa lần lượt là
1.200.000 và 3.000.000 VNĐ, hành lý 20 và 40 kg.
Các mã cũ **HV01–HV10** vẫn giữ nguyên và khác với HV1/HV2. Muốn áp dụng quy tắc
mới cho ghế đã có, chọn ghế, đổi hạng vé sang HV1 hoặc HV2 rồi bấm Sửa.

Ở lần chạy đầu tiên sau bản cập nhật dữ liệu mẫu, chương trình tự thêm các mã còn thiếu:
10 máy bay M01–M10, 10 hạng vé HV01–HV10 và 10 ghế G01–G10.
G01 thuộc M01 và HV01, G02 thuộc M02 và HV02, …, G10 thuộc M10 và HV10.
Mã đã tồn tại được giữ nguyên (không phân biệt hoa/thường), kể cả hạng vé/ghế đã sửa.
Bộ mẫu chỉ được nạp một lần và lưu cờ trong JSON; sửa hoặc xóa sau đó sẽ không bị
tạo lại khi khởi động. Không cần nút nạp hay kết nối SQL Server.
Tên hạng vé, hạn mức hành lý và giá chỉ là số liệu minh họa để kiểm thử.

| Mã HV | Tên hạng vé | Hành lý (kg) | Đơn giá (VNĐ) |
|---|---|---:|---:|
| HV01 | Phổ thông tiết kiệm | 7 | 800.000 |
| HV02 | Phổ thông tiêu chuẩn | 15 | 1.000.000 |
| HV03 | Phổ thông linh hoạt | 20 | 1.200.000 |
| HV04 | Phổ thông đặc biệt tiết kiệm | 25 | 1.600.000 |
| HV05 | Phổ thông đặc biệt tiêu chuẩn | 30 | 1.900.000 |
| HV06 | Phổ thông đặc biệt linh hoạt | 35 | 2.200.000 |
| HV07 | Thương gia tiết kiệm | 40 | 3.000.000 |
| HV08 | Thương gia tiêu chuẩn | 45 | 3.500.000 |
| HV09 | Thương gia linh hoạt | 50 | 4.000.000 |
| HV10 | Hạng nhất | 60 | 6.000.000 |

- **Bảng tổng hợp:** mở đầu tiên, hiển thị chung máy bay, ghế và hạng vé trong một bảng.
  Các cột: mã máy bay, mã hãng, mã ghế, loại ghế, mã hạng vé, tên hạng vé, hành lý và đơn giá.
  Mỗi ghế là một dòng; máy bay có nhiều ghế sẽ xuất hiện trên nhiều dòng.
  Máy bay chưa có ghế vẫn xuất hiện với các cột chi tiết để trống.
  Hạng vé chưa được ghế nào dùng có thể xem ở tab Hạng vé.
  Tìm kiếm theo các mã hoặc tên hạng vé, không phân biệt hoa/thường.
  Khi quay lại tab này sau khi sửa dữ liệu, bảng tự cập nhật.
- **Máy bay:** nhập mã máy bay và mã hãng hàng không.
- **Hạng vé:** nhập mã, chọn tên hạng vé, nhập hành lý (kg) và đơn giá vé (VNĐ).
- Ô **Tên hạng vé** là danh sách chọn sẵn các tên trong bộ mẫu và hai tên mặc định, không cần gõ.
  Tên đã lưu từ trước cũng có trong danh sách để xem/sửa dữ liệu cũ.
- **Ghế:** tạo máy bay và hạng vé trước, sau đó chọn chúng trong danh sách.
  Ô **Hạng vé** chỉ cho chọn **HV1 – Ghế thường** và **HV2 – Ghế VIP**,
  lấy từ danh mục Hạng vé và chỉ lưu mã hạng vé. Các mã cũ không còn trong
  danh sách chọn ở tab Ghế. Khi sửa ghế đang dùng mã cũ, cần chọn HV1 hoặc HV2
  trước khi lưu; dữ liệu cũ không tự bị ghi đè.
  Với HV1/HV2, **Loại ghế** tự điền; với mã khác, chọn **Ghế thường / Ghế VIP**.
  Dữ liệu cũ chưa có loại ghế mặc định là Ghế thường; chọn dòng và Sửa để đổi.
  Loại ghế được lưu vào JSON và có thể tìm bằng từ khóa `VIP` hoặc `thường`.
  Khi khởi tạo bộ mẫu trên dữ liệu mới, G01–G06 là Ghế thường, G07–G10 là Ghế VIP.
  Các ghế mẫu đã lưu từ bản cũ được giữ nguyên, không tự đổi loại.
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
- `GUI`: `MainForm`, bảng tổng hợp `MayBayGheView` và các tab nhập liệu/hiển thị.
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
