using System.Text.Json;
using QuanLyMayBay.BUS;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.Tests;

internal static class HanhKhachChecks
{
    public static void Main()
    {
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", null);
        var folder = Path.Combine(Path.GetTempPath(), "hanhkhach-checks-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        int passed = 0;
        void Check(bool ok, string name)
        {
            if (!ok) throw new Exception("FAIL: " + name);
            Console.WriteLine("PASS: " + name);
            passed++;
        }
        void Reject(Action action, string name)
        {
            try { action(); }
            catch (ArgumentException) { Check(true, name); return; }
            throw new Exception("FAIL: " + name);
        }
        HanhKhach Sample(string ma = "HK01", string cccd = "001000000011", string maKH = "KH01") =>
            new(ma, "Nguyễn Hành Khách Mẫu", new DateTime(2001, 1, 2), cccd, "Nam", maKH, "Người lớn");
        try
        {
            var customerPath = Path.Combine(folder, "khachhang.json");
            var passengerPath = Path.Combine(folder, "hanhkhach.json");
            var khDao = new KhachHangDAO(customerPath);
            var hkDao = new HanhKhachDAO(passengerPath);
            var customers = new KhachHangBUS(khDao, hkDao);
            var passengers = new HanhKhachBUS(hkDao, khDao);
            Check(passengers.TimKiem().Count == 0 && passengers.LayKhachHang().Count == 0, "First run is empty");
            Reject(() => passengers.Luu(Sample(), false), "Unknown customer rejected");
            customers.Luu(new KhachHang("KH01", "Khách Hàng Mẫu", "001000000001", "0900000001",
                "kh01@example.test", "kh01", "Fake password!", "Địa chỉ mẫu"), false);
            Check(passengers.LayKhachHang().Count == 1
                && passengers.LayKhachHang().Cast<KhachHang>().Single().MatKhau == "",
                "Customer dropdown reads newly saved customer without password");
            var customerSnapshot = File.ReadAllText(customerPath);
            var sample = Sample(" HK01 ", "001000000011", "kh01");
            sample.GioiTinh = "nữ";
            sample.NgaySinh = new DateTime(2001, 1, 2, 13, 30, 0);
            passengers.Luu(sample, false);
            var first = passengers.TimKiem().Cast<HanhKhach>().Single();
            Check(first.MaHK == "HK01" && first.MaKH == "KH01" && first.GioiTinh == "Nữ"
                && first.NgaySinh == new DateTime(2001, 1, 2) && first.CCCD == "001000000011",
                "Create normalizes code, parent, gender and date while preserving leading zeroes");
            Reject(() => passengers.Luu(Sample("hk01", "001000000012"), false), "Case-insensitive duplicate passenger code rejected");
            Reject(() => passengers.Luu(Sample("HK02"), false), "Duplicate passenger CCCD rejected");
            var invalid = Sample("HK02", "001000000012");
            invalid.NgaySinh = DateTime.Today.AddDays(1);
            Reject(() => passengers.Luu(invalid, false), "Future birth date rejected");
            invalid.NgaySinh = DateTime.MinValue;
            Reject(() => passengers.Luu(invalid, false), "Missing birth date rejected");
            invalid = Sample("HK02", "bad");
            Reject(() => passengers.Luu(invalid, false), "Invalid supplied CCCD rejected");
            invalid = Sample("HK02", "001000000012");
            invalid.GioiTinh = "Invalid";
            Reject(() => passengers.Luu(invalid, false), "Invalid gender rejected");
            invalid = Sample("HK02", "001000000012");
            invalid.LoaiKH = " ";
            Reject(() => passengers.Luu(invalid, false), "Customer type required");
            invalid = Sample("HK02", "001000000012");
            invalid.HoTen = "";
            Reject(() => passengers.Luu(invalid, false), "Name required");
            passengers.Luu(Sample("HK02", ""), false);
            passengers.Luu(Sample("HK03", ""), false);
            Check(passengers.TimKiem().Count == 3, "Multiple passengers without CCCD are allowed");
            Check(passengers.TimKiem("NGUYỄN").Count == 3 && passengers.TimKiem("02/01/2001").Count == 3
                && passengers.TimKiem("kh01").Count == 3 && passengers.TimKiem("NGƯỜI LỚN").Count == 3,
                "Search supports name, birth date, parent and type ignoring case");
            first.HoTen = "Changed outside BUS";
            Check(passengers.TimKiem("Changed outside BUS").Count == 0, "Search returns detached copies");
            var edit = passengers.TimKiem().Cast<HanhKhach>().Single(x => x.MaHK == "HK01");
            edit.HoTen = "Họ tên đã sửa";
            edit.LoaiKH = "Loại kiểm thử";
            passengers.Luu(edit, true);
            Check(new HanhKhachBUS(hkDao, khDao).TimKiem("Họ tên đã sửa").Count == 1,
                "Update persists across restart");
            Reject(() => customers.Xoa("kh01"), "Referenced customer cannot be deleted");
            Reject(() => passengers.Luu(Sample("MISSING", "001000000099"), true), "Updating absent passenger rejected");
            Reject(() => passengers.Xoa("MISSING"), "Deleting absent passenger rejected");
            Check(File.ReadAllText(customerPath) == customerSnapshot, "Passenger CRUD does not overwrite customer file");
            customers.Luu(new KhachHang("KH02", "Khách thứ hai", "001000000002", "0900000002",
                "kh02@example.test", "kh02", "Fake password!", "Địa chỉ mẫu"), false);
            Check(passengers.LayKhachHang().Count == 2, "Customer choices refresh after adding another customer");
            edit.MaKH = "KH02";
            passengers.Luu(edit, true);
            Check(passengers.TimKiem().Cast<HanhKhach>().Single(x => x.MaHK == "HK01").MaKH == "KH02",
                "Can move a passenger to another valid customer");
            passengers.Xoa("hk02");
            passengers.Xoa("HK03");
            customers.Xoa("KH01");
            Check(customers.TimKiem().Count == 1 && passengers.TimKiem().Count == 1,
                "Unreferenced customer can be deleted after moving/removing passengers");
            var saved = File.ReadAllText(passengerPath);
            File.Delete(passengerPath);
            Directory.CreateDirectory(passengerPath);
            try { passengers.Xoa("HK01"); throw new Exception("Expected write failure"); }
            catch (IOException) { Check(passengers.TimKiem().Count == 1, "Write failure preserves in-memory passengers"); }
            Check(Directory.GetFiles(folder, "*.tmp").Length == 0, "Failed save cleans temporary file");
            Directory.Delete(passengerPath);
            File.WriteAllText(passengerPath, saved);
            Check(new HanhKhachBUS(hkDao, khDao).TimKiem().Count == 1, "Valid passenger data reloads after failure prerequisite restored");
            var bad = hkDao.Doc();
            bad[0].MaKH = "MISSING";
            hkDao.Luu(bad);
            var badFile = File.ReadAllText(passengerPath);
            try { _ = new HanhKhachBUS(hkDao, khDao); throw new Exception("Expected invalid reference"); }
            catch (InvalidDataException) { Check(File.ReadAllText(passengerPath) == badFile, "Invalid stored customer reference rejected without changing file"); }
            File.WriteAllText(passengerPath, "broken JSON");
            try { _ = new HanhKhachBUS(hkDao, khDao); throw new Exception("Expected corrupt file rejection"); }
            catch (JsonException) { Check(File.ReadAllText(passengerPath) == "broken JSON", "Corrupt passenger file preserved"); }
            Console.WriteLine($"{passed} checks passed.");
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
