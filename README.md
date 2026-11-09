# Quản lý bán vé máy bay

Ứng dụng C# WinForms quản lý Máy bay, Ghế, Hạng vé, Khách hàng, Hành khách,
Nhân viên, CTKM và Voucher.

Tên đề tài chính thức: **Quản lý bán vé máy bay**. Các danh mục hỗ trợ quy trình
bán vé; nghiệp vụ đặt/bán/hủy vé và tồn ghế theo chuyến chưa hoàn tất.
Đã chuyển DDL SQL Server nhóm gửi sang MySQL 8.4 như hướng dẫn bên dưới.

## DAO theo mẫu của nhóm

Đã có `DocDS`, `Them`, `Sua`, `Xoa` cho 10 DAO danh mục, gồm các DAO riêng cho
Máy bay/Ghế/Hạng vé/Hãng hàng không. Xem [cách dùng và tích hợp BUS](docs/DAO_THEO_MAU.md).
Mỗi DAO đọc các trường DTO rõ ràng; phần SQL có tham số/kết nối dùng chung.

## MySQL và ListView (cập nhật mới)

Đã thêm **MySQL qua DAO (MySqlConnector)** và đổi toàn bộ bảng GUI sang **ListView**.
Đã nhận `BanVeMayBay.sql` của nhóm (DDL SQL Server), chuyển đủ **15 bảng/23 FK/13 CHECK**
sang MySQL 8.4 và ghép profile DAO cụ thể. Xem [nhập CSDL MySQL mới](docs/mysql/README.md)
và [cấu hình kết nối](docs/KET_NOI_MYSQL.md). `MHHK → MaHHK`, `LoaiKH → LoaiHK`,
ngày sinh hỗ trợ NULL; hành lý/giá vé được kiểm tra đúng độ chính xác cột nguồn.
Ứng dụng đóng gói profile BanVeMayBay, chỉ cần đặt `QLMB_MYSQL_CONNECTION` sau khi nhập schema.
Không tự tạo/sửa bảng hoặc chép dữ liệu mẫu. File gốc không có chi phí, tham số và view báo cáo;
bảng tham số có script bổ sung tùy chọn, phần báo cáo tiền vẫn cần nguồn/quy tắc nghiệp vụ.
Cấu hình SQL Server bên dưới là hướng dẫn cũ; MySQL được ưu tiên khi đã cấu hình.

## Bản cập nhật theo PDF yêu cầu đồ án

### CSDL BanVeMayBay theo ảnh đã gửi

Đã có profile `docs/sql-mapping.BanVeMayBay.json`: `MHHK → MaHHK`, khóa ghế `MaGhe`,
loại ghế suy ra từ hạng vé (không cần cột SQL LoaiGhe), danh sách hãng từ HangHangKhong.
Chạy `tools/Chay-BanVeMayBay.ps1` trên Windows để dùng localhost/BanVeMayBay,
hoặc xem lệnh trực tiếp trong [hướng dẫn SQL](docs/KET_NOI_SQL_SERVER.md).
Các bảng chưa thấy đủ trong ảnh chưa được xác nhận. Module lỗi sẽ báo riêng,
không đóng cả chương trình. Công cụ `-XuatCauTruc` xuất schema JSON chỉ đọc để sửa tiếp.

Đã thêm menu desktop bên trái, màn chính–chi tiết bằng ListView, nút `...` chọn dữ
liệu có biến static, bộ lọc nâng cao tại BUS, XLS/PDF tiếng Việt, tham số cấu hình,
hóa đơn–CTHD dạng chỉ đọc và các bảng thống kê tiền.

- Đọc [đối chiếu từng yêu cầu](docs/DOI_CHIEU_YEU_CAU.md) trước khi đánh giá hoàn tất.
- [Hướng dẫn SQL Server](docs/KET_NOI_SQL_SERVER.md),
  [kiểm tra Windows](docs/KIEM_THU_WINDOWS.md),
  [báo cáo nháp](docs/BaoCaoDoAn_Nhap.docx).
- Các màn hình nay chọn từ **menu bên trái** thay vì hàng tab ở trên.
- Phần SQL hỗ trợ MyConnection và mapping, nhưng **chưa nối/kiểm chứng CSDL nhóm**
  vì chưa có CREATE TABLE/PK/FK/view thật. Nghiệp vụ bán/hủy vé và tồn ghế còn chờ
  lược đồ, không giả lập bằng cách xóa ghế vật lý.
- Báo cáo là **bản nháp**, thiếu thông tin nhóm, ảnh GUI Windows và ảnh SQL thực tế.

Build/test:

```powershell
.\tools\Test.ps1
```

Trên Linux/macOS có .NET 8: `bash tools/test.sh` (build WinForms chỉ để kiểm tra
biên dịch; giao diện chỉ chạy Windows). Thư viện Core chứa DTO/BUS/DAO/Services,
WinForms tham chiếu Core; các test cũng dùng đúng Core này.

### Lọc, Excel và PDF

Lọc nâng cao mở từ thanh công cụ danh mục. Ví dụ tại Voucher:
`GiamGia >= 10 AND NOT (MaVoucher ~ 'TEST')`; tại CTKM:
`NgayBatDau >= '2026-01-01' AND NgayKetThuc <= '2026-12-31'`.
Hỗ trợ AND/OR/NOT, ngoặc, =, <>, !=, >, >=, <, <=, ≥, ≤, và ~ (chứa chuỗi).
Ngày dùng yyyy-MM-dd hoặc dd/MM/yyyy, chuỗi/ngày đặt trong nháy; số dùng dấu chấm.
Mật khẩu không được lọc hoặc xuất.

Xuất XLS/PDF ở các danh mục; nhập theo lô và mẫu XLS ở Nhân viên/CTKM/Voucher.
Nhập kiểm tra cả file trước khi lưu; mã đã có được cập nhật, dòng ngoài file giữ nguyên.
XLS thực dùng NPOI 2.7.6 (Apache-2.0); PDF dùng PDFsharp 6.2.4 và font DejaVu nhúng
(giấy phép kèm Assets). Tên cột trong file mẫu là tên property DTO; không đổi tiêu đề.

Màn **Hóa đơn–Chi tiết**: chế độ JSON cho nhập tập CTHD/phiếu chi đúng mẫu và xuất
PDF hóa đơn được chọn; chế độ SQL chỉ đọc view được ánh xạ. Đây là nguồn báo cáo,
**chưa phải chức năng tạo giao dịch bán vé**. File trong examples là dữ liệu giả lập,
chỉ nhập chủ động trên dữ liệu thử. Các tổng tiền/thành tiền không nhập trực tiếp.

Màn **Thống kê**: thu, chi, lợi nhuận trong khoảng ngày (bao gồm cả hai đầu),
ba bảng quý theo NV/KH/SP với năm chọn riêng, nhóm theo ngày/tháng/quý/năm,
KH+SP và NV+SP+Năm. Màn **Tham số** điều khiển tên đơn vị, địa chỉ, SDT,
số lẻ tiền; các giá trị được dùng trên PDF và khi làm tròn tiền.

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
Các tên hạng vé mẫu cũ được đồng bộ trong danh mục và bảng tổng hợp:
HV03–HV06 dùng tên **Hạng phổ thông**, HV07–HV10 dùng tên **Hạng thương gia**.
Mã hạng vé, giá và hành lý của các dòng này giữ nguyên. Bảng tổng hợp luôn lấy
tên từ danh mục Hạng vé theo mã; không dùng tên nhập riêng ở tab Ghế.
Loại ghế cũng được đồng bộ theo danh mục: **Hạng phổ thông → Ghế thường**,
**Hạng thương gia → Ghế VIP**, kể cả khi ghế còn dùng các mã mẫu cũ.
Quy tắc áp dụng khi mở chương trình và khi lưu thay đổi; đổi tên nhóm hạng vé
cũng cập nhật loại ghế liên quan, giữ nguyên giá và hành lý.
Khi mở bản mới, ghế đã lưu thuộc nhóm phổ thông được chuyển mã sang **HV01**,
nhóm thương gia chuyển sang **HV02**. Bảng tổng hợp lấy tên, hành lý và giá từ
đúng danh mục HV01/HV02 sau khi chuyển. Định nghĩa hạng vé cũ vẫn được giữ trong
JSON; các ghế chuyển nhóm sử dụng giá/hành lý của mã đích.

Với dữ liệu mới, chương trình nạp một lần 10 máy bay M01–M10 và 10 ghế G01–G10.
G01–G06 dùng HV01, G07–G10 dùng HV02; G01 thuộc M01, …, G10 thuộc M10.
Bộ mẫu không ghi đè các mã đã có và không tạo lại sau khi người dùng xóa.
Riêng bản cập nhật mã hãng áp dụng một lần cho máy bay đã có: **M01 → VN**,
**M02 → THA**. Các máy bay khác giữ nguyên; các lần sửa thủ công sau đó không bị đặt lại.
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
  Các ghế thuộc hai nhóm phổ thông/thương gia tự chuyển mã sang HV01/HV02 khi mở chương trình.
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

### Nhân viên, CTKM và Voucher: GUI – BUS – DAO

Mỗi module có DTO, BUS, DAO và View riêng; cả ba tab đều có
Thêm/Sửa/Xóa/Nhập mới/Tìm kiếm. Mã là duy nhất, không phân biệt hoa/thường và
không đổi khi sửa. Tìm kiếm trên `ArrayList` tại BUS.

| Tab | Thuộc tính | Quy tắc |
|---|---|---|
| Nhân viên | `MaNV`, `Ho`, `Ten`, `ChucVu` | Các trường bắt buộc; tìm được cả họ và tên ghép |
| CTKM | `MaCTKM`, `TenCTKM`, `NgayBatDau`, `NgayKetThuc` | Chọn ngày bằng lịch, ngày kết thúc từ ngày bắt đầu trở đi; cho phép CTKM tương lai và cùng ngày |
| Voucher | `MaVoucher`, `MaCTKM`, `GiamGia` | Chọn CTKM theo mã – tên; giảm giá là phần trăm, lớn hơn 0 và không quá 100 |

Tạo CTKM trước khi thêm Voucher. Một CTKM có nhiều Voucher, không xóa CTKM
đang có Voucher liên quan. Chuyển/xóa các Voucher trước nếu cần xóa CTKM.
Sau khi thêm/sửa CTKM, quay lại tab Voucher để cập nhật danh sách chọn.
`GiamGia` dùng `decimal`: nhập `10.5` nghĩa là giảm **10,5%**.
Module hiện quản lý danh mục, chưa có chức năng áp Voucher vào hóa đơn/đặt vé.

Dữ liệu lưu riêng trong `%LOCALAPPDATA%\QuanLyMayBay`:
`nhanvien.json`, `ctkm.json`, `voucher.json`; không cần SQL Server.
`DAO/JsonDanhSachDAO.cs` dùng chung thao tác đọc/ghi an toàn cho ba DAO này.

Chạy kiểm tra ba module:

```powershell
dotnet run --project Tests/NhanVienKhuyenMaiChecks/Checks.csproj -c Release
```

### Hành khách: GUI – BUS – DAO

- `DTO/HanhKhach.cs`: `MaHK`, `HoTen`, `NgaySinh` (`DateTime`), `CCCD`,
  `GioiTinh`, `MaKH`, `LoaiKH`; các mã và CCCD dùng `string`.
- `GUI/HanhKhachView.cs`: tab **Hành khách** có Thêm/Sửa/Xóa/Nhập mới/Tìm kiếm;
  chọn ngày sinh bằng lịch, giới tính Nam/Nữ/Khác, chọn khách hàng theo **mã – tên**.
- `BUS/HanhKhachBUS.cs`: tìm kiếm `ArrayList`, chặn trùng mã hành khách và CCCD
  khi có nhập, chặn ngày sinh tương lai và kiểm tra `MaKH` phải tồn tại.
- `DAO/HanhKhachDAO.cs`: lưu riêng tại
  `%LOCALAPPDATA%\QuanLyMayBay\hanhkhach.json`, không cần SQL Server.

Tạo Khách hàng trước rồi thêm Hành khách. `LoaiKH` nhập tự do để dùng đúng
danh mục của nhóm; chưa áp đặt quy tắc phân loại theo tuổi. CCCD có thể trống
(ví dụ trẻ chưa có CCCD); nếu nhập thì cần 12 chữ số. Các trường còn lại bắt buộc.
Một khách hàng có nhiều hành khách. Không xóa khách hàng đang có hành khách liên quan;
cần chuyển hành khách sang khách hàng khác hoặc xóa hành khách trước.
Quay lại tab Hành khách sau khi thêm khách hàng để cập nhật danh sách chọn.

Chạy kiểm tra module Hành khách:

```powershell
dotnet run --project Tests/HanhKhachChecks/Checks.csproj -c Release
```

### Khách hàng: GUI – BUS – DAO

- `GUI/KhachHangView.cs`: tab **Khách hàng** có đủ tám ô nhập và các nút
  Thêm, Sửa, Xóa, Nhập mới, Tìm kiếm. Mật khẩu được che và không hiển thị trong bảng.
- `BUS/KhachHangBUS.cs`: tìm kiếm trên `ArrayList`; kiểm tra mã KH, CCCD, tài khoản
  không trùng; CCCD đủ 12 chữ số; SĐT 9–15 chữ số (có thể có dấu `+`); email hợp lệ.
  Các thông tin đều bắt buộc khi thêm. Khi sửa, để trống mật khẩu để giữ mật khẩu cũ.
- `DAO/KhachHangDAO.cs`: đọc/ghi file
  `%LOCALAPPDATA%\QuanLyMayBay\khachhang.json`, riêng với dữ liệu máy bay.
  Mật khẩu lưu dưới dạng PBKDF2-SHA256 có salt, không lưu nguyên văn.
- `DTO/KhachHang.cs`: dữ liệu truyền giữa các lớp; kết quả tìm kiếm gửi lên GUI
  có `MatKhau` rỗng. Mã khách hàng không đổi khi sửa. Module này chưa có chức năng đăng nhập.

Chạy kiểm tra Khách hàng (không cần Windows hay SQL Server):

```powershell
dotnet run --project Tests/KhachHangChecks/Checks.csproj -c Release
```

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
