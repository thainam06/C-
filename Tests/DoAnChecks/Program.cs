using System.Collections;
using System.Text.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PdfSharp.Pdf.IO;
using QuanLyMayBay.BUS;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;
using QuanLyMayBay.Services;

namespace QuanLyMayBay.Tests;

internal static class DoAnChecks
{
    public static void Main(string[] args)
    {
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", null);
        var folder = Path.Combine(Path.GetTempPath(), "doan-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        int passed = 0;
        void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); passed++; }
        void Reject(Action action, string name)
        { try { action(); } catch (ArgumentException) { Check(true, name); return; } throw new Exception("FAIL: " + name); }
        try
        {
            var vouchers = new ArrayList { new Voucher("A", "K1", 5), new Voucher("B", "K1", 10), new Voucher("C", "K2", 50) };
            Check(LocNangCaoBUS.Loc<Voucher>(vouchers, "GiamGia >= 10 AND NOT (MaCTKM = 'K2')").Cast<Voucher>().Single().MaVoucher == "B", "AND/NOT/parentheses and numeric compare");
            Check(LocNangCaoBUS.Loc<Voucher>(vouchers, "MaVoucher = 'A' OR MaVoucher = 'B' AND GiamGia > 20").Count == 1, "AND precedence over OR");
            Check(LocNangCaoBUS.Loc<Voucher>(vouchers, "GiamGia ≥ 10 AND GiamGia ≤ 50 AND MaCTKM <> 'K1'").Count == 1, "Unicode comparisons and not equal");
            Check(LocNangCaoBUS.Loc<Voucher>(vouchers, "MaCTKM ~ 'k1'").Count == 2, "Contains filter ignores case");
            Check(LocNangCaoBUS.Loc<NhanVien>(new ArrayList { new NhanVien("N", "O'Neil", "An", "Bán vé") }, "Ho = 'O''Neil'").Count == 1, "Escaped quotes in filter");
            Check(LocNangCaoBUS.Loc<CTKM>(new ArrayList { new CTKM("K", "Ưu đãi", new(2026, 1, 1), new(2026, 12, 31)) }, "NgayBatDau >= '01/01/2026' AND NgayKetThuc < '2027-01-01'").Count == 1, "Date comparisons on two fields");
            Reject(() => LocNangCaoBUS.Loc<Voucher>(new ArrayList(), "Missing = 1"), "Invalid field rejected even with empty input");
            Reject(() => LocNangCaoBUS.Loc<KhachHang>(new ArrayList(), "MatKhau ~ 'x'"), "Password cannot be filtered");
            Reject(() => LocNangCaoBUS.Loc<Voucher>(vouchers, "GiamGia >= 10; DROP TABLE Voucher"), "SQL-like injection rejected by filter parser");
            Reject(() => LocNangCaoBUS.Loc<Voucher>(vouchers, "(GiamGia > 1"), "Unbalanced expression rejected");
            Reject(() => LocNangCaoBUS.Loc<Voucher>(vouchers, string.Concat(Enumerable.Repeat("NOT ", 40)) + "GiamGia = 1"), "Filter nesting limit enforced");
            Reject(() => LocNangCaoBUS.Loc<CTKM>(new ArrayList(), "NgayBatDau >= 'wrong-date'"), "Invalid typed date rejected");

            var nvDao = new NhanVienDAO(Path.Combine(folder, "nhanvien.json"));
            var nv = new NhanVienBUS(nvDao);
            nv.Luu(new NhanVien("N0", "Giữ", "Lại", "Quản lý"), false);
            var source = new List<NhanVien> { new("0001", "Nguyễn Văn", "An", "Bán vé"), new("0002", "=1+1", "Bình", "Hỗ trợ") };
            var excelPath = Path.Combine(folder, "nhanvien.xls");
            ExcelService.Xuat(excelPath, source);
            Check(File.ReadAllBytes(excelPath).Take(4).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }), "Export is real binary XLS");
            var read = ExcelService.Nhap<NhanVien>(excelPath);
            Check(read.Count == 2 && read[0].MaNV == "0001" && read[0].Ho == "Nguyễn Văn", "XLS round trip preserves Vietnamese and leading zeroes");
            using (var stream = File.OpenRead(excelPath))
            using (var book = WorkbookFactory.Create(stream))
                Check(book.GetSheetAt(0).GetRow(2).GetCell(1).CellType == CellType.String, "Text starting with equals is not exported as formula");
            nv.NhapDanhSach(read);
            Check(nv.TimKiem().Count == 3 && new NhanVienBUS(nvDao).TimKiem("0001").Count == 1, "Batch import merges and persists without deleting other rows");
            var before = File.ReadAllText(Path.Combine(folder, "nhanvien.json"));
            Reject(() => nv.NhapDanhSach(new() { new("GOOD", "A", "B", "C"), new("BAD", "A", "B", "") }), "Entire batch validated before saving");
            Check(before == File.ReadAllText(Path.Combine(folder, "nhanvien.json")) && nv.TimKiem("GOOD").Count == 0, "Invalid batch changes neither file nor memory");
            Reject(() => nv.NhapDanhSach(new() { new("X", "A", "B", "C"), new("x", "A", "B", "C") }), "Duplicate IDs inside import rejected");
            var high = new List<HangVe> { new("H", "Hạng phổ thông", 20, 1234567890123456789.123m) };
            var highPath = Path.Combine(folder, "precision.xls"); ExcelService.Xuat(highPath, high);
            Check(ExcelService.Nhap<HangVe>(highPath).Single().DonGiaVe == high[0].DonGiaVe, "XLS preserves decimals beyond Excel numeric precision");
            var khPath = Path.Combine(folder, "privacy.xls");
            ExcelService.Xuat(khPath, new[] { new KhachHang("K", "Mẫu", "001000000001", "0900000001", "a@example.test", "demo", "Fake password!", "Mẫu") });
            using (var stream = File.OpenRead(khPath))
            using (var book = WorkbookFactory.Create(stream))
                Check(book.GetSheetAt(0).GetRow(0).Cells.All(x => x.StringCellValue != "MatKhau"), "Password excluded from XLS");
            var formulaPath = Path.Combine(folder, "formula.xls");
            using (var book = new HSSFWorkbook())
            {
                var sheet = book.CreateSheet(); var head = sheet.CreateRow(0); var row = sheet.CreateRow(1);
                var names = new[] { "MaNV", "Ho", "Ten", "ChucVu" };
                for (int i = 0; i < 4; i++) { head.CreateCell(i).SetCellValue(names[i]); row.CreateCell(i).SetCellValue("Demo"); }
                row.GetCell(1).SetCellFormula("1+1");
                using var stream = File.Create(formulaPath); book.Write(stream, true);
            }
            Reject(() => ExcelService.Nhap<NhanVien>(formulaPath), "Formula cells rejected during import");
            var missingPath = Path.Combine(folder, "missing.xls");
            using (var book = new HSSFWorkbook())
            {
                book.CreateSheet().CreateRow(0).CreateCell(0).SetCellValue("MaNV");
                using var output = File.Create(missingPath); book.Write(output, true);
            }
            Reject(() => ExcelService.Nhap<NhanVien>(missingPath), "Missing mandatory Excel headers rejected");
            var passwordPath = Path.Combine(folder, "password.xls");
            using (var book = new HSSFWorkbook())
            {
                book.CreateSheet().CreateRow(0).CreateCell(0).SetCellValue("matkhau");
                using var output = File.Create(passwordPath); book.Write(output, true);
            }
            Reject(() => ExcelService.Nhap<KhachHang>(passwordPath), "Password import header blocked ignoring case");
            var datePath = Path.Combine(folder, "dates.xls");
            using (var book = new HSSFWorkbook())
            {
                var sheet = book.CreateSheet(); var head = sheet.CreateRow(0); var row = sheet.CreateRow(1);
                var names = new[] { "MaCTKM", "TenCTKM", "NgayBatDau", "NgayKetThuc" };
                for (int i = 0; i < 4; i++) head.CreateCell(i).SetCellValue(names[i]);
                row.CreateCell(0).SetCellValue("K"); row.CreateCell(1).SetCellValue("Ngày số Excel");
                var style = book.CreateCellStyle(); style.DataFormat = book.CreateDataFormat().GetFormat("dd/mm/yyyy");
                row.CreateCell(2).SetCellValue(new DateTime(2024, 2, 29)); row.GetCell(2).CellStyle = style;
                row.CreateCell(3).SetCellValue(new DateTime(2024, 3, 1)); row.GetCell(3).CellStyle = style;
                using var stream = File.Create(datePath); book.Write(stream, true);
            }
            Check(ExcelService.Nhap<CTKM>(datePath).Single().NgayBatDau == new DateTime(2024, 2, 29), "Numeric Excel dates import correctly");

            var thamSo = new ThamSoBUS(new ThamSoDAO(Path.Combine(folder, "thamso.json")));
            Check(thamSo.SoLeTien == 0 && thamSo.Lay("TEN_DON_VI").Length > 0, "Parameter defaults exist");
            thamSo.Luu(new ThamSo { MaThamSo = "SO_LE_TIEN", GiaTri = "2", MoTa = "Tiền" }, true);
            Check(new ThamSoBUS(new ThamSoDAO(Path.Combine(folder, "thamso.json"))).SoLeTien == 2, "Parameter updates persist");
            Reject(() => thamSo.Luu(new ThamSo { MaThamSo = "SO_LE_TIEN", GiaTri = "5" }, true), "Invalid money precision rejected");
            var statsDao = new ThongKeDAO(Path.Combine(folder, "thongke.json"));
            var stats = new ThongKeBUS(statsDao);
            DongDoanhThu Revenue(string hd, int stt, DateTime date, string kh, string employee, string product, int quantity, decimal price, decimal discount = 0)
                => new() { MaHD = hd, STT = stt, NgayLap = date, MaKH = kh, MaNV = employee, MaSanPham = product, SoLuong = quantity, DonGia = price, GiamGia = discount };
            var revenue = new List<DongDoanhThu>
            {
                Revenue("H1", 1, new(2026,1,10), "K1", "N1", "S1", 2, 1000, 10),
                Revenue("H1", 2, new(2026,1,10), "K1", "N1", "S2", 1, 500),
                Revenue("H2", 1, new(2026,4,10), "K2", "N2", "S1", 1, 2000, 25),
                Revenue("H3", 1, new(2026,12,31,23,59,0), "K1", "N1", "S2", 3, 100),
                Revenue("H4", 1, new(2025,1,1), "K1", "N1", "S1", 1, 9999)
            };
            stats.NhapDoanhThu(revenue);
            var costs = new List<DongChiPhi>
            {
                new() { MaPC="C1", NgayChi=new(2026,1,1), MaNV="N1", NoiDung="Chi mẫu", SoTien=1000 },
                new() { MaPC="C2", NgayChi=new(2026,4,1), MaNV="N2", NoiDung="Chi mẫu", SoTien=200 },
                new() { MaPC="C3", NgayChi=new(2026,12,31), MaNV="N1", NoiDung="Chi mẫu", SoTien=100 }
            };
            stats.NhapChiPhi(costs);
            var from = new DateTime(2026,1,1); var to = new DateTime(2026,12,31);
            var total = stats.TongHop(from, to);
            Check(total.Thu == 4100 && total.Chi == 1300 && total.LoiNhuan == 2800, "Revenue, cost and profit with inclusive date boundaries");
            var quarters = stats.TheoQuy(NhomThongKe.NhanVien, 2026, from, to);
            Check(quarters.Single(x => x.Nhom == "N1").Q1 == 2300 && quarters.Single(x => x.Nhom == "N1").Q4 == 300
                && quarters.Last().Tong == 4100, "Employee quarterly totals and grand total");
            Check(stats.TheoQuy(NhomThongKe.KhachHang, 2026, from, to).Last().Tong == 4100
                && stats.TheoQuy(NhomThongKe.SanPham, 2026, from, to).Last().Tong == 4100, "Customer and product quarter tables reconcile");
            Check(stats.TheoKy(NhomThongKe.KhachHangSanPham, KyThongKe.Nam, from, to).Count == 3
                && stats.TheoKy(NhomThongKe.NhanVienSanPhamNam, KyThongKe.Nam, from, to).Sum(x => x.Tong) == 4100, "Two-key and three-key statistics");
            Check(stats.TheoKy(NhomThongKe.NhanVien, KyThongKe.Ngay, from, to).Count == 3
                && stats.TheoKy(NhomThongKe.KhachHang, KyThongKe.Thang, from, to).Count == 3, "Daily and monthly periods");
            Check(ThongKeBUS.ThanhTien(Revenue("X",1,from,"K","N","S",1,0.05m,10), thamSo.SoLeTien) == 0.05m, "Money rounding uses configured precision");
            revenue[0].DonGia = 99999;
            Check(stats.TongHop(from,to).Thu == 4100, "Caller cannot mutate saved statistics");
            var statsBefore = File.ReadAllText(Path.Combine(folder, "thongke.json"));
            Reject(() => stats.NhapDoanhThu(new() { Revenue("BAD",1,from,"K","N","S",0,100) }), "Invalid finance import rejected");
            Check(statsBefore == File.ReadAllText(Path.Combine(folder, "thongke.json")), "Invalid finance batch leaves previous data intact");
            Reject(() => stats.NhapDoanhThu(new() { Revenue("DUP",1,from,"K","N","S",1,100), Revenue("dup",1,from,"K","N","S",1,100) }), "Duplicate invoice line key rejected");
            Reject(() => stats.NhapDoanhThu(new() { Revenue("HD",1,from,"K1","N","S",1,100), Revenue("HD",2,from,"K2","N","S",1,100) }), "Conflicting customer on same invoice rejected");
            Reject(() => stats.TongHop(to,from), "Reversed report date range rejected");
            Check(new ThongKeBUS(statsDao).TongHop(from,to).Thu == 4100, "Statistics survive restart");

            var pdfPath = Path.Combine(folder, "BaoCao.pdf");
            PdfService.XuatBang(pdfPath, "HÓA ĐƠN KIỂM THỬ – Hạng phổ thông", new[] { "Mã", "Họ tên", "Số tiền" },
                Enumerable.Range(1, 350).Select(i => new[] { i.ToString(), "Nguyễn Văn An – Ghế thường – Hạng thương gia", "1200000" }).ToList());
            using (var pdf = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import)) Check(pdf.PageCount > 1, "Unicode PDF export creates real multi-page document");
            var mapPath = Path.Combine(folder, "mapping.json");
            Environment.SetEnvironmentVariable("QLMB_SQL_MAPPING_FILE", mapPath);
            File.WriteAllText(mapPath, JsonSerializer.Serialize(new Dictionary<string,AnhXaBangSql>
            { ["NhanVien"] = new() { Table="dbo.NhanVien", KeyColumns=new(){"MaNV"}, Columns=new() { ["MaNV"]="MaNV", ["Ho"]="Ho", ["Ten"]="Ten", ["ChucVu"]="ChucVu" } } }));
            Check(CSDLListDAO.DocAnhXa(typeof(NhanVien)).KeyColumns.Single() == "MaNV", "SQL mapping validates required columns and key");
            Check(CSDLListDAO.TenSql("x]; DROP TABLE y;--") == "[x]]; DROP TABLE y;--]", "SQL identifiers are quoted rather than interpolated executable text");
            try { MyConnection.KiemTra(); throw new Exception("Expected missing SQL configuration"); }
            catch (InvalidOperationException) { Check(true, "SQL connection is not claimed when unconfigured"); }
            Environment.SetEnvironmentVariable("QLMB_SQL_MAPPING_FILE", null);
            if (args.Length == 2 && args[0] == "--artifacts")
            {
                Directory.CreateDirectory(args[1]);
                File.Copy(pdfPath, Path.Combine(args[1], "BaoCaoKiemThu.pdf"), true);
                ExcelService.Xuat(Path.Combine(args[1], "NhanVienMau.xls"), source);
                ExcelService.Xuat(Path.Combine(args[1], "CTHDMau.xls"), stats.LayDuLieu().DoanhThu);
                ExcelService.Xuat(Path.Combine(args[1], "PhieuChiMau.xls"), costs);
            }
            Console.WriteLine($"{passed} checks passed.");
        }
        finally { Directory.Delete(folder, true); }
    }
}
