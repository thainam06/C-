"""Tao ban nhap bao cao; khong gia lap anh giao dien hay anh CSDL thuc te.
Chay: python tools/TaoBaoCao.py (can python-docx, Pillow, Graphviz).
"""
from pathlib import Path
import subprocess
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
IMAGES = ROOT / "docs" / "images"
IMAGES.mkdir(parents=True, exist_ok=True)


def diagram(name, body):
    source = IMAGES / (name + ".dot")
    source.write_text('digraph G { graph [rankdir=LR,bgcolor="white",dpi=170]; '
                      'node [shape=record,fontname="DejaVu Sans",fontsize=12]; '
                      'edge [fontname="DejaVu Sans",fontsize=11]; ' + body + '}')
    output = IMAGES / (name + ".png")
    subprocess.run(["dot", "-Tpng", str(source), "-o", str(output)], check=True)
    return output


diagrams = [
    diagram("erd-maybay", 'm[label="MayBay|MaMayBay (PK)|MHHK"]; '
            'g[label="Ghe|MaMayBay + MaGhe (PK)|MaHV (FK)|LoaiGhe"]; '
            'h[label="HangVe|MaHV (PK)|TenHV|HanhLyKg|DonGiaVe"]; '
            'm -> g[label="1 - n"]; h -> g[label="1 - n"];'),
    diagram("erd-khachhang", 'k[label="KhachHang|MaKH (PK)|HoTen|CCCD, SDT, Email|TaiKhoan, MatKhau(hash)|DiaChi"]; '
            'h[label="HanhKhach|MaHK (PK)|HoTen, NgaySinh|CCCD, GioiTinh|MaKH (FK), LoaiKH"]; k -> h[label="1 - n"];'),
    diagram("erd-khuyenmai", 'k[label="CTKM|MaCTKM (PK)|TenCTKM|NgayBatDau|NgayKetThuc"]; '
            'v[label="Voucher|MaVoucher (PK)|MaCTKM (FK)|GiamGia (%)"]; k -> v[label="1 - n"];'),
]
architecture = diagram("kien-truc", 'g[label="GUI (WinForms)|Menu trai|Form danh muc|Main-sub, Hoa don, Bao cao"]; '
                       'b[label="BUS|Kiem tra / ArrayList|Loc nang cao|Nhap lo, Thong ke"]; '
                       'd[label="DAO|MyConnection + SQL mapping|Transaction / snapshot|JSON khi chua bat SQL"]; '
                       'g -> b; b -> d;')

tree_lines = ["C-/"]
for folder in ["GUI", "BUS", "DAO", "DTO", "Services", "Core", "Tests", "docs"]:
    tree_lines.append("  " + folder + "/")
    files = sorted(p.name for p in (ROOT / folder).glob("*.cs"))
    tree_lines.extend("    " + name for name in files[:6])
    if len(files) > 6:
        tree_lines.append("    ...")
font = ImageFont.truetype(str(ROOT / "Assets" / "DejaVuSans.ttf"), 17)
tree = Image.new("RGB", (960, len(tree_lines) * 24 + 30), "white")
draw = ImageDraw.Draw(tree)
for i, text in enumerate(tree_lines):
    draw.text((20, 15 + i * 24), text, fill="black", font=font)
tree_path = IMAGES / "cay-source.png"
tree.save(tree_path)

doc = Document()
doc.core_properties.author = "Nhóm chưa cung cấp thông tin"
doc.core_properties.title = "Báo cáo nháp — Quản lý bán vé máy bay"
section = doc.sections[0]
section.page_width, section.page_height = Cm(21), Cm(29.7)
section.top_margin = section.bottom_margin = Cm(1.8)
section.left_margin = section.right_margin = Cm(2)
for name in ["Normal", "Title", "Heading 1", "Heading 2", "Heading 3"]:
    style = doc.styles[name]
    style.font.name, style.font.size = "Times New Roman", Pt(13)
    fonts = style._element.get_or_add_rPr().rFonts
    for attr in ["asciiTheme", "hAnsiTheme", "eastAsiaTheme", "cstheme"]:
        fonts.attrib.pop(qn("w:" + attr), None)
    for attr in ["ascii", "hAnsi", "eastAsia", "cs"]:
        fonts.set(qn("w:" + attr), "Times New Roman")
    style.font.color.rgb = RGBColor(0, 0, 0)
    style.paragraph_format.space_after = Pt(5)
    style.paragraph_format.line_spacing = 1.1
normal = doc.styles["Normal"]
lang = OxmlElement("w:lang"); lang.set(qn("w:val"), "vi-VN")
normal._element.get_or_add_rPr().append(lang)
footer = section.footer.paragraphs[0]
footer.alignment = 2
footer.add_run("BẢN NHÁP — ")
page = OxmlElement("w:fldSimple"); page.set(qn("w:instr"), "PAGE"); footer._p.append(page)


def p(text, bold=False):
    para = doc.add_paragraph()
    run = para.add_run(text); run.bold = bold
    return para


def heading(text):
    return doc.add_heading(text, level=1)


def new_page(text):
    heading(text).paragraph_format.page_break_before = True


def table(headers, rows):
    t = doc.add_table(rows=1, cols=len(headers)); t.style = "Table Grid"
    for cell, text in zip(t.rows[0].cells, headers):
        cell.text = text
        for run in cell.paragraphs[0].runs: run.bold = True
    for row in rows:
        for cell, text in zip(t.add_row().cells, row): cell.text = str(text)


for text in ["[TRƯỜNG / KHOA: CHƯA CUNG CẤP]", "", "ĐỒ ÁN HỌC PHẦN LẬP TRÌNH C#",
             "QUẢN LÝ BÁN VÉ MÁY BAY", "", "BẢN NHÁP — CHƯA HOÀN THIỆN ĐỂ NỘP",
             "", "Thành viên / MSSV: [CHƯA CUNG CẤP]", "Lớp: [CHƯA CUNG CẤP]",
             "GVHD: Phùng Thái Thiên Trang", "", "Tháng 12/2026"]:
    para = p(text, "ĐỒ ÁN" in text or "BẢN NHÁP" in text)
    para.alignment = 1

new_page("I. Giới thiệu đề tài")
p("Tên đề tài chính thức: Quản lý bán vé máy bay. Đề tài quản lý các danh mục máy bay, ghế, hạng vé, khách hàng, hành khách, nhân viên, "
  "chương trình khuyến mãi và voucher. Ứng dụng desktop C# WinForms tổ chức theo GUI–BUS–DAO.")
p("Bản cập nhật bổ sung tìm kiếm nâng cao, chọn dữ liệu bằng nút ..., màn chính–chi tiết, "
  "tham số cấu hình, nhập/xuất XLS, PDF và thống kê tiền từ nguồn số liệu được chuẩn hóa.")
p("Trạng thái thực tế", True)
p("Mã nguồn đã build và được kiểm tra Core trên Linux. Chưa chạy tương tác cửa sổ Windows, "
  "chưa kết nối SQL Server của nhóm. Người dùng cho biết nhóm có CSDL riêng nhưng chưa cung cấp lược đồ. "
  "Nghiệp vụ bán/đặt/hủy vé và tồn ghế chưa được ghép vào CSDL thật.")
p("Báo cáo này không phải bản hoàn chỉnh để nộp: còn thiếu thông tin sinh viên/lớp, ảnh giao diện "
  "chạy thật, ảnh CSDL thật và phần nghiệp vụ tồn ghế. Không sử dụng hình mô phỏng để giả làm ảnh chạy ứng dụng.")
p("Phạm vi dữ liệu", True)
p("Khi chưa bật SQL, dữ liệu danh mục lưu JSON trong thư mục LocalApplicationData/QuanLyMayBay. "
  "Khi cấu hình QLMB_SQL_CONNECTION và mapping đúng, DAO dùng MyConnection và SqlCommand có tham số. "
  "Các tập số liệu kiểm thử Excel được ghi rõ giả lập, không tự nạp vào doanh thu thật.")

new_page("II. Thiết kế mô hình CSDL")
p("Các hình dưới thể hiện quan hệ logic hiện có trong DTO/BUS, KHÔNG thay thế ảnh lược đồ SQL Server của nhóm. "
  "Tên bảng/cột và PK/FK vật lý phải được đối chiếu với script CREATE TABLE thực tế.")
for path, caption in zip(diagrams, ["Máy bay – Ghế – Hạng vé", "Khách hàng – Hành khách", "CTKM – Voucher"]):
    doc.add_picture(str(path), width=Cm(14))
    p(caption)

new_page("III. Các chức năng chính — quản lý danh mục")
table(["Chức năng", "Đầu vào", "Đầu ra / xử lý"], [
    ["Máy bay, ghế, hạng vé", "Mã, hãng, loại ghế, giá", "Chặn trùng; HV01 phổ thông/ghế thường, HV02 thương gia/ghế VIP"],
    ["Khách hàng", "Thông tin liên hệ, tài khoản", "CCCD/tài khoản duy nhất; mật khẩu băm PBKDF2, không xuất mật khẩu"],
    ["Hành khách", "Thông tin, ngày sinh, MaKH", "Chọn khách hàng đã có; chặn ngày sinh tương lai và mất liên kết"],
    ["Nhân viên", "MaNV, Ho, Ten, ChucVu", "Thêm/sửa/xóa, tìm cả họ tên; không sửa khóa"],
    ["CTKM, voucher", "Khoảng ngày, GiamGia (%)", "Ngày kết thúc không trước ngày đầu; giảm >0 và <=100%; FK CTKM"]
])
p("Mỗi màn hình có các ô nhập và bảng chỉ đọc, các nút Thêm/Sửa/Xóa/Nhập mới/Tìm kiếm. "
  "Mã là duy nhất không phân biệt hoa/thường. Việc lưu chỉ cập nhật bộ nhớ sau khi DAO ghi thành công.")
p("[CHÈN ẢNH THẬT: màn hình desktop/menu trái và một form danh mục. Chú thích mã, vùng nhập, "
  "bảng dữ liệu và các nút chức năng.]")

new_page("III. Các chức năng chính — liên kết và tìm kiếm")
p("1. Form chính–chi tiết: hai ListView. Chọn một dòng chính để hiển thị đúng các dòng liên quan "
  "theo khóa bằng nhau, không lọc mã bằng phép chứa chuỗi. Các cặp đã có: máy bay–ghế, "
  "khách hàng–hành khách, CTKM–voucher; màn hóa đơn hiển thị hóa đơn–CTHD từ số liệu báo cáo.")
p("2. Nút ...: mở cửa sổ tìm/chọn, lưu lựa chọn vào ChonDuLieuForm.DaChon (static), điền mã "
  "về ComboBox. Biến được xóa khi mở/cancel để không lấy nhầm lựa chọn cũ.")
p("3. Tìm kiếm nâng cao tại BUS trên ArrayList: AND/OR/NOT, ngoặc, =, <>, !=, >, >=, <, <=, ≥, ≤; "
  "~ để kiểm tra chứa chuỗi. Kiểu số/ngày được kiểm tra trước khi lọc. Trường MatKhau bị loại khỏi bộ lọc.")
p("Ví dụ Voucher: GiamGia >= 10 AND NOT (MaVoucher ~ 'TEST').")
p("Ví dụ CTKM: NgayBatDau >= '2026-01-01' AND NgayKetThuc <= '2026-12-31'.")
p("Đầu vào: danh sách và biểu thức. Đầu ra: danh sách thỏa điều kiện hoặc thông báo cú pháp/kiểu dữ liệu sai. "
  "Không chạy biểu thức người dùng bằng SQL hay eval.")
p("[CHÈN ẢNH THẬT: hai ListView khi chọn hai dòng khác nhau; cửa sổ ...; hộp lọc nâng cao và kết quả.]")

new_page("III. Các chức năng chính — thống kê và công thức")
p("Thành tiền dòng = Round(Số lượng × Đơn giá × (1 − Giảm giá / 100), SO_LE_TIEN). "
  "Làm tròn AwayFromZero. Tổng hóa đơn bằng tổng các dòng đã làm tròn. Không nhập tổng tiền trực tiếp.")
p("Tổng thu = tổng thành tiền trong khoảng ngày; tổng chi = tổng phiếu chi trong khoảng; "
  "lợi nhuận = thu − chi. Hai đầu khoảng ngày đều được tính. Các khoản thuế/phí khác cần căn cứ "
  "lược đồ và quy tắc thật của nhóm; chưa tự thêm thuế/phí không có trong dữ liệu.")
p("Ba bảng quý theo nhân viên, khách hàng và sản phẩm có Q1–Q4, tổng dòng và tổng cộng. "
  "Bảng quý lọc năm riêng. Nhóm mở rộng hỗ trợ ngày/tháng/quý/năm, khách hàng+sản phẩm, "
  "nhân viên+sản phẩm+năm.")
table(["Kiểm thử giả lập 2026", "Giá trị VNĐ"], [["Thu", "4.100"], ["Chi", "1.300"], ["Lợi nhuận", "2.800"]])
p("Các con số trên là dữ liệu kiểm thử, không phải kết quả kinh doanh của nhóm. Nguồn SQL cần ánh xạ "
  "hai view chuẩn hóa từ hóa đơn/chi tiết và phiếu chi thật; hiện chưa nhận được các view này.")
p("[CHÈN ẢNH THẬT: 3 bảng quý và bộ lọc ngày/năm; đối chiếu tổng dòng/cột bằng chứng từ của nhóm.]")

new_page("III. Các chức năng chính — Excel, PDF và tham số")
p("XLS xuất là định dạng Excel 97–2003 thật (NPOI), hỗ trợ nhập XLS/XLSX. Mã chuỗi giữ số 0 đầu, "
  "tiếng Việt được bảo toàn; decimal quá giới hạn chính xác Excel được ghi chuỗi. Không xuất mật khẩu/hash. "
  "Ô công thức bị từ chối khi nhập. Nhân viên/CTKM/Voucher nhập theo lô, kiểm tra cả file rồi mới ghi.")
p("PDF dùng PDFsharp và font DejaVu nhúng, hỗ trợ tiếng Việt và chia trang. Có PDF danh mục, "
  "bảng thống kê và hóa đơn được chọn. Nội dung danh sách và thành tiền lấy từ dữ liệu thực được nạp.")
p("Table tham số gồm TEN_DON_VI, DIA_CHI, SDT, SO_LE_TIEN. Tên/địa chỉ/liên hệ dùng trên PDF; "
  "SO_LE_TIEN (0–2) điều khiển công thức tiền. Mã tham số không đổi khi sửa.")
proof = IMAGES / "pdf-kiem-thu.png"
if proof.exists():
    doc.add_picture(str(proof), width=Cm(11))
    p("Ảnh trang PDF sinh thật từ bộ kiểm tra; không phải ảnh giao diện WinForms.")
p("[CHÈN ẢNH THẬT: nhập XLS thành công, lỗi dòng cuối không nhập dở; PDF hóa đơn từ số liệu của nhóm.]")

new_page("IV. Hướng dẫn cài đặt — kiến trúc và công cụ")
doc.add_picture(str(architecture), width=Cm(16))
p("GUI chỉ nhận/hiển thị dữ liệu; BUS kiểm tra và tính toán; DAO truy cập nguồn lưu. "
  "Core chứa DTO/BUS/DAO/Services và được WinForms cùng các test tham chiếu.")
p("Windows: Visual Studio 2022 17.8+, workload .NET desktop development, .NET 8 SDK. "
  "Mở QuanLyMayBay.csproj, restore NuGet, chọn Startup Project rồi F5. "
  "Không dùng project c# cũ đang tự gom cả source con vì thiếu các reference/dependency mới.")
p("Công cụ/thư viện: .NET https://dotnet.microsoft.com/download/dotnet/8.0 ; "
  "Visual Studio https://visualstudio.microsoft.com/ ; Microsoft.Data.SqlClient 6.1.8, "
  "NPOI 2.7.6 (Apache-2.0), PDFsharp 6.2.4 và font DejaVu (license trong Assets).")
p("Chạy tự động: tools/Test.ps1 trên Windows hoặc bash tools/test.sh trên Linux/macOS. "
  "GUI chỉ chạy Windows. Python/python-docx/Pillow/Graphviz chỉ cần khi tạo lại báo cáo này, "
  "không phải dependency để chạy ứng dụng.")
p("[CHÈN ẢNH THẬT: Solution Explorer, thấy rõ GUI–BUS–DAO; ảnh cài workload và restore gói.]")

new_page("IV. Cấu trúc mã nguồn")
doc.add_picture(str(tree_path), width=Cm(14))
p("Cây thư mục được trích từ source hiện tại. Đây là ảnh biểu diễn cấu trúc tệp, "
  "không phải ảnh Solution Explorer; cần bổ sung ảnh Visual Studio thật theo yêu cầu báo cáo.")

new_page("IV. Hướng dẫn cài đặt — kết nối CSDL")
p("MyConnection lấy chuỗi kết nối từ QLMB_SQL_CONNECTION. QLMB_SQL_MAPPING_FILE chỉ tới JSON "
  "ánh xạ property DTO vào bảng/cột SQL thực tế, khóa chính đúng lược đồ nhóm. "
  "Xem docs/KET_NOI_SQL_SERVER.md và sql-mapping.example.json.")
p("Khi bật SQL, không tự nạp dữ liệu mẫu hay tự tạo/sửa CSDL. Lưu dùng transaction Serializable "
  "và kiểm tra snapshot; PK/FK lỗi hoặc phiên khác đã thay đổi bảng thì từ chối/rollback. "
  "Khi chưa bật SQL, dùng JSON cục bộ để thử giao diện.")
p("Tiếng Việt: cột NVARCHAR và SqlParameter. Không nối ?useUnicode=... vào connection string SQL Server "
  "vì hậu tố đó là cách dùng MySQL. Mật khẩu lưu hash PBKDF2, không nguyên văn; "
  "không commit connection string có mật khẩu.")
p("Tình trạng: chưa có lược đồ và biến kết nối SQL thật trong môi trường kiểm tra. "
  "Cần CREATE TABLE/PK/FK/view, thông tin server/database không bí mật và cấu hình xác thực trên máy chạy. "
  "Nghiệp vụ khóa ghế theo chuyến, bán/nhập/hủy vé phải bổ sung theo schema thật; chưa được đánh dấu hoàn tất.")
p("[CHÈN ẢNH THẬT: lược đồ SQL rõ nét; SELECT DB_NAME; giao dịch PK/FK/rollback và cập nhật ghế của nhóm.]")

new_page("V. Sản phẩm và kết quả kiểm thử")
p("Source: https://github.com/thainam06/C- . Báo cáo nháp: docs/BaoCaoDoAn_Nhap.docx. "
  "Cần bổ sung link bản báo cáo DOC/PDF cuối sau khi điền thông tin và ảnh thực tế.")
table(["Bộ kiểm tra Core", "Số kiểm tra qua"], [["Khách hàng", "29"], ["Hành khách", "28"],
      ["Nhân viên / CTKM / Voucher", "47"], ["Yêu cầu đồ án mới", "45"], ["Tổng", "149"]])
p("Build Release trên Linux: 0 lỗi, 0 cảnh báo. Test không chứng minh giao diện Windows hoặc SQL Server thật đã chạy. "
  "Xem docs/KET_QUA_KIEM_THU.md và KIEM_THU_WINDOWS.md để phân biệt phần đã kiểm tra và phần còn chờ.")
p("Cần hoàn thiện trước khi nộp: (1) thông tin nhóm/lớp; (2) mapping SQL thật; "
  "(3) giao dịch bán/hủy/nhập và tồn ghế; (4) ảnh GUI/SQL; (5) chạy nghiệm thu trên Windows và "
  "đối chiếu báo cáo với chứng từ thật.")
p("Mốc theo tài liệu: tuần 9–10 kiểm tra viết hàm, tuần 11–12 chấm đồ án. Báo cáo tối đa 20 trang, "
  "Times New Roman 13, không có trang mục lục. Bìa theo yêu cầu ghi tháng 12/2026.")

output = ROOT / "docs" / "BaoCaoDoAn_Nhap.docx"
doc.save(output)
print(output)
