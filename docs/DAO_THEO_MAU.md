# DAO theo mẫu HangHangKhongDAO của nhóm

Đã áp dụng cách tách DAO theo danh mục, đọc danh sách và CRUD như file mẫu:

| DAO | Hàm |
| --- | --- |
| HangHangKhongDAO | DocDS, Them, Sua, Xoa |
| MayBayDAO | DocDS, Them, Sua, Xoa |
| HangVeDAO | DocDS, Them, Sua, Xoa |
| GheDAO | DocDS, Them, Sua, Xoa |
| KhachHangDAO | DocDS, Them, Sua, Xoa |
| HanhKhachDAO | DocDS, Them, Sua, Xoa |
| NhanVienDAO | DocDS, Them, Sua, Xoa |
| CTKMDAO | DocDS, Them, Sua, Xoa |
| VoucherDAO | DocDS, Them, Sua, Xoa |
| ThamSoDAO | DocDS, Them, Sua, Xoa |

Mỗi DAO có **đầy đủ thân hàm** DocDS/Them/Sua/Xoa: tạo MySqlConnection, mở trong try, khai báo câu SELECT/INSERT/UPDATE/DELETE, tạo MySqlCommand, thêm tham số, thực thi rồi đóng connection trong finally. DocDS có MySqlDataReader, vòng while và gán từng thuộc tính DTO trực tiếp. Không còn LenhDanhMucMySql hoặc các hàm chỉ gọi Lenh.Them/Lenh.Sua.

DanhMucMySql.cs chỉ hỗ trợ ánh xạ tên bảng/cột, điều phối đọc đúng DAO và kiểm tra kiểu khóa/giới hạn; không chứa bộ thực thi CRUD dùng chung như bản trước. Giao dịch và các kiểm tra dữ liệu được giữ trong từng hàm ghi.

`MyConnection.GetConnection()` trả MySqlConnection **chưa mở**, đúng quy ước file mẫu. Bên gọi mở kết nối và dùng `using` để tự đóng/dispose cả khi có lỗi. Project hiện dùng **MySqlConnector**, nên giữ namespace MySqlConnector thay cho MySql.Data.MySqlClient trong mẫu; không trộn hai provider. DTO vẫn ở namespace QuanLyMayBay.DTO với tên đang có để BUS/GUI tiếp tục biên dịch.

## Dùng các hàm theo mẫu

```csharp
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

var dao = new MayBayDAO();
List<MayBay> danhSach = dao.DocDS();
dao.Them(new MayBay("M11", "VN")); // HangHangKhong phải có VN trước
dao.Sua(new MayBay("M11", "THA")); // THA cũng phải tồn tại
dao.Xoa("M11");                   // Không xóa nếu đang được tham chiếu
```

Ví dụ minh họa API; không tự chạy đoạn này khi khởi động app. Cần cấu hình MySQL theo KET_NOI_MYSQL.md trước. `Them` luôn INSERT và báo lỗi nếu trùng khóa; `Sua` chỉ UPDATE dòng tồn tại; `Xoa(string ma)` dùng khóa chính một cột. Với mapping khóa ghép, dùng `Xoa(dto)` và điền đủ các khóa. Không sửa khóa chính qua hàm Sua.

Giá trị dùng tham số @MaMayBay, @MHHK... thay vì nối trực tiếp vào câu SQL như mẫu. Tên có dấu nháy, Unicode và chuỗi giống lệnh SQL được giữ như dữ liệu. Tên bảng/cột vẫn lấy từ mapping và quote theo MySQL; không hardcode LoaiKH thành cột sai vì CSDL dùng LoaiHK.

## Tích hợp với ứng dụng hiện tại

- GUI tiếp tục gọi BUS để kiểm tra CCCD, email, mật khẩu băm, liên kết hạng vé/loại ghế, phần trăm voucher... DAO chỉ kiểm tra điều kiện dữ liệu của CSDL, không thay thế các quy tắc BUS. Không truyền mật khẩu thô trực tiếp vào KhachHangDAO.Them.
- Các hàm Doc/Luu cũ được giữ cho BUS và chế độ JSON/SQL Server đã có. DocDS của cả 10 DAO là hàm đọc MySQL trực tiếp. Ở 6 DAO có Luu, DocDS đồng thời ghi nhận phiên dữ liệu, nên có thể dùng DocDS rồi Luu cùng instance. Các DAO mới Máy bay/Ghế/Hạng vé/Hãng hàng không cung cấp API MySQL.
- Khi đọc MySQL qua BUS, CSDLListDAO chuyển tới DocDS của từng DAO. Khi Luu nhiều bảng, CSDLListDAO vẫn giữ một giao dịch chung, kiểm tra snapshot dưới khóa, ghi cha trước/xóa con trước và rollback nếu lỗi; không mở các giao dịch CRUD riêng để làm vỡ tính nguyên tử của lô ghi.
- Các hàm Them/Sua/Xoa trực tiếp là giao dịch cho từng dòng, yêu cầu MySQL; chúng không tự duy trì snapshot của các instance khác. Nếu vừa dùng CRUD trực tiếp rồi quay lại Luu danh sách đã đọc trước đó, tải lại Doc/DocDS trước để tránh lỗi xung đột. API Sua trực tiếp nhận giá trị hiện tại theo khóa, không có token kiểm tra phiên; luồng GUI qua BUS vẫn dùng snapshot như trước.
- Ghế vẫn chỉ ghi ba cột vật lý theo DDL nhóm; loại ghế suy ra từ hạng vé, trừ khi mapping chỉ rõ cột LoaiGhe vật lý.
- ThongKeDAO là nguồn báo cáo chỉ đọc từ view; không tạo CRUD xóa/sửa chứng từ qua view. DuLieuDAO vẫn phối hợp Máy bay/Hạng vé/Ghế trong một giao dịch.
- MySqlException được bắt và báo lỗi bằng tiếng Việt, giữ lỗi gốc trong InnerException để tra cứu FK/UNIQUE/kết nối. ArgumentException cho dữ liệu nhập không hợp lệ vẫn được giữ riêng.
- Cấu hình MySQL, file chuyển schema và form ListView hiện tại vẫn dùng được.

Đã kiểm tra CRUD từng DAO trên MySQL 8.4 riêng: tên có dấu nháy, tiếng Việt, ngày NULL, decimal, mapping tùy chỉnh, FK/UNIQUE, độ dài/độ chính xác, dòng không tồn tại, sửa dữ liệu không đổi và không làm mất dòng khác. Xem KET_QUA_KIEM_THU.md.
