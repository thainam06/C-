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

Hạng vé mặc định:

| Mã HV | Tên hạng vé | Loại ghế | Hành lý mẫu (kg) | Giá mẫu (VNĐ) |
|---|---|---|---:|---:|
| HV01 | Hạng phổ thông | Ghế thường | 20 | 1.200.000 |
| HV02 | Hạng thương gia | Ghế VIP | 40 | 3.000.000 |

Tên hạng vé và loại ghế tự điền, khóa theo mã. Hành lý và giá có thể sửa;
không xóa hai hạng vé mặc định. Khi cập nhật dữ liệu cũ, chương trình sửa tên
HV01/HV02 và đổi liên kết ghế dùng HV1/HV2 sang HV01/HV02. Giá và hành lý
của HV01/HV02 đã có được giữ nguyên; nếu mã mới chưa có thì lấy từ HV1/HV2
cũ khi có, hoặc dùng giá/hành lý mẫu. Các hạng vé cũ khác vẫn được giữ trong JSON.

Với dữ liệu mới, chương trình nạp một lần 10 máy bay M01–M10 và 10 ghế G01–G10.
G01–G06 dùng HV01, G07–G10 dùng HV02; G01 thuộc M01, …, G10 thuộc M10.
Bộ mẫu không ghi đè các mã đã có và không tạo lại sau khi người dùng xóa.
Không cần nút nạp hay kết nối SQL Server. Các giá trị mẫu chỉ dùng kiểm thử.

- **Bảng tổng hợp:** mở đầu tiên, hiển thị chung máy bay, ghế và hạng vé trong một bảng.
  Các cột: mã máy bay, mã hãng, mã ghế, loại ghế, mã hạng vé, tên hạng vé, hành lý và đơn giá.
  Mỗi ghế là một dòng; máy bay có nhiều ghế sẽ xuất hiện trên nhiều dòng.
  Máy bay chưa có ghế vẫn xuất hiện với các cột chi tiết để trống.
  Hạng vé chưa được ghế nào dùng có thể xem ở tab Hạng vé.
  Tìm kiếm theo các mã hoặc tên hạng vé, không phân biệt hoa/thường.
  Khi quay lại tab này sau khi sửa dữ liệu, bảng tự cập nhật.
- **Máy bay:** nhập mã máy bay và mã hãng hàng không.
- **Hạng vé:** nhập mã, chọn tên hạng vé, nhập hành lý (kg) và đơn giá vé (VNĐ).
- Ô **Tên hạng vé** chỉ có **Hạng phổ thông** và **Hạng thương gia** để chọn.
  Nhập mã HV01/HV02 thì tên tự điền và khóa theo mã.
- **Ghế:** chọn máy bay và hạng vé từ danh sách.
  Ô **Hạng vé** chỉ hiển thị **HV01** hoặc **HV02**. Chọn HV01 tự điền
  **Ghế thường**, chọn HV02 tự điền **Ghế VIP**, không chọn loại trái với hạng vé.
  Các ghế dùng hạng vé cũ khác được giữ nguyên; chọn lại HV01/HV02 rồi Sửa để đổi.
  Loại ghế được lưu vào JSON và có thể tìm bằng từ khóa `VIP` hoặc `thường`.
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
