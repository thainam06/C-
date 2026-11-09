using System.Security.Cryptography;
using System.Text.Json;
using VeMayBay.BUS;
using VeMayBay.DAO;
using VeMayBay.DTO;

namespace VeMayBay.Tests;

internal static class KhachHangChecks
{
    public static void Main()
    {
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", null);
        // Du lieu gia lap, khong su dung thong tin ca nhan that.
        var folder = Path.Combine(Path.GetTempPath(), "khachhang-checks-" + Guid.NewGuid());
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
        KhachHangDTO Sample(string ma = "KH01", string cccd = "001000000001", string account = "kh01") =>
            new(ma, "Nguyễn Văn Mẫu", cccd, "0900000001", $"{account}@example.test", account,
                "Fake password 123!", "Địa chỉ kiểm thử");
        bool Matches(string password, string stored)
        {
            var parts = stored.Split('$');
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(parts[2]),
                int.Parse(parts[1]), HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(parts[3]));
        }
        try
        {
            var path = Path.Combine(folder, "khachhang.json");
            var aircraftFile = Path.Combine(folder, "dulieu.json");
            File.WriteAllText(aircraftFile, "aircraft data sentinel");
            var dao = new KhachHangDAO(path);
            var bus = new KhachHangBUS(dao);
            Check(bus.TimKiem().Count == 0, "First run is empty");
            var first = Sample();
            first.MaKH = " KH01 ";
            bus.Luu(first, false);
            var shown = bus.TimKiem().Cast<KhachHangDTO>().Single();
            Check(shown.MaKH == "KH01" && shown.CCCD == "001000000001" && shown.SDT == "0900000001",
                "Create preserves leading zeroes and normalizes code");
            Check(shown.MatKhau == "", "Search results never expose password or hash");
            var originalHash = dao.Doc().Single().MatKhau;
            Check(originalHash.StartsWith("PBKDF2-SHA256$") && Matches(first.MatKhau, originalHash)
                && !File.ReadAllText(path).Contains(first.MatKhau), "Only a salted password hash is stored");
            bus.Luu(Sample("KH02", "001000000002", "kh02"), false);
            Check(originalHash != dao.Doc().Single(x => x.MaKH == "KH02").MatKhau,
                "Equal passwords use different random salts");
            Reject(() => bus.Luu(Sample("kh01", "001000000003", "kh03"), false), "Case-insensitive duplicate customer code rejected");
            Reject(() => bus.Luu(Sample("KH03", "001000000001", "kh03"), false), "Duplicate CCCD rejected");
            Reject(() => bus.Luu(Sample("KH03", "001000000003", "KH01"), false), "Case-insensitive duplicate account rejected");
            var invalid = Sample("KH03", "001000000003", "kh03");
            invalid.HoTen = " ";
            Reject(() => bus.Luu(invalid, false), "Missing required name rejected");
            invalid = Sample("KH03", "bad-cccd", "kh03");
            Reject(() => bus.Luu(invalid, false), "Invalid CCCD rejected");
            invalid = Sample("KH03", "001000000003", "kh03");
            invalid.SDT = "090abc";
            Reject(() => bus.Luu(invalid, false), "Invalid phone rejected");
            invalid = Sample("KH03", "001000000003", "kh03");
            invalid.Email = "not-an-email";
            Reject(() => bus.Luu(invalid, false), "Invalid email rejected");
            invalid = Sample("KH03", "001000000003", "kh03");
            invalid.MatKhau = "";
            Reject(() => bus.Luu(invalid, false), "Password required when creating");
            Check(bus.TimKiem("NGUYỄN").Count == 2 && bus.TimKiem("kh02").Count == 1
                && bus.TimKiem("0900000001").Count == 2 && bus.TimKiem("example.test").Count == 2
                && bus.TimKiem("ĐỊA CHỈ").Count == 2, "Search covers customer fields ignoring case");
            Check(bus.TimKiem(first.MatKhau).Count == 0, "Password cannot be searched");
            shown.HoTen = "Changed outside BUS";
            Check(bus.TimKiem("Changed outside BUS").Count == 0, "Search returns detached copies");
            var edit = bus.TimKiem("KH01").Cast<KhachHangDTO>().Single();
            edit.HoTen = "Tên đã sửa";
            bus.Luu(edit, true);
            Check(dao.Doc().Single(x => x.MaKH == "KH01").MatKhau == originalHash,
                "Blank password on update retains existing hash");
            edit.MatKhau = "  New fake password!  ";
            bus.Luu(edit, true);
            var newHash = dao.Doc().Single(x => x.MaKH == "KH01").MatKhau;
            Check(newHash != originalHash && Matches(edit.MatKhau, newHash) && !Matches(first.MatKhau, newHash),
                "Updating password preserves spaces and changes stored hash");
            edit.CCCD = "001000000002";
            Reject(() => bus.Luu(edit, true), "Update cannot take another customer's CCCD");
            var reload = new KhachHangBUS(dao);
            Check(reload.TimKiem("Tên đã sửa").Count == 1 && reload.TimKiem().Count == 2,
                "Customer data survives restart");
            Reject(() => reload.Luu(Sample("MISSING"), true), "Updating absent customer rejected");
            reload.Xoa("kh02");
            Check(new KhachHangBUS(dao).TimKiem().Count == 1, "Delete persists and ignores code case");
            Reject(() => reload.Xoa("MISSING"), "Deleting absent customer rejected");
            Check(File.ReadAllText(aircraftFile) == "aircraft data sentinel", "Customer operations do not touch aircraft data");
            var saved = File.ReadAllText(path);
            File.Delete(path);
            Directory.CreateDirectory(path);
            try { reload.Xoa("KH01"); throw new Exception("Expected write failure"); }
            catch (IOException) { Check(reload.TimKiem().Count == 1, "Write failure leaves memory unchanged"); }
            Check(Directory.GetFiles(folder, "*.tmp").Length == 0, "Temporary file cleaned after failed save");
            Directory.Delete(path);
            File.WriteAllText(path, saved);
            Check(new KhachHangBUS(dao).TimKiem().Count == 1, "Saved data remains readable after retry prerequisite restored");
            var bad = dao.Doc();
            bad[0].MatKhau = "plaintext";
            dao.Luu(bad);
            var badFile = File.ReadAllText(path);
            try { _ = new KhachHangBUS(dao); throw new Exception("Expected invalid data rejection"); }
            catch (InvalidDataException) { Check(File.ReadAllText(path) == badFile, "Invalid stored password rejected without overwriting file"); }
            File.WriteAllText(path, "broken JSON");
            try { _ = new KhachHangBUS(dao); throw new Exception("Expected corrupt JSON rejection"); }
            catch (JsonException) { Check(File.ReadAllText(path) == "broken JSON", "Corrupt JSON preserved"); }
            Console.WriteLine($"{passed} checks passed.");
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
