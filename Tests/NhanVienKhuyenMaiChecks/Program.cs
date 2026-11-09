using System.Globalization;
using System.Text.Json;
using VeMayBay.BUS;
using VeMayBay.DAO;
using VeMayBay.DTO;

namespace VeMayBay.Tests;

internal static class NhanVienKhuyenMaiChecks
{
    public static void Main()
    {
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", null);
        var folder = Path.Combine(Path.GetTempPath(), "nhanvien-khuyenmai-" + Guid.NewGuid());
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
        void BadFile(string path, string content, Action load, string name)
        {
            var saved = File.ReadAllText(path);
            File.WriteAllText(path, content);
            try
            {
                load();
                throw new Exception("FAIL: " + name);
            }
            catch (Exception ex) when (ex is InvalidDataException or JsonException)
            {
                Check(File.ReadAllText(path) == content, name);
            }
            finally { File.WriteAllText(path, saved); }
        }
        void FailedSave(string path, Action save, Func<bool> unchanged, string name)
        {
            var original = File.ReadAllText(path);
            File.Delete(path);
            Directory.CreateDirectory(path);
            try
            {
                save();
                throw new Exception("FAIL: " + name);
            }
            catch (IOException) { Check(unchanged(), name); }
            finally
            {
                Directory.Delete(path);
                File.WriteAllText(path, original);
            }
            Check(Directory.GetFiles(folder, "*.tmp").Length == 0, name + ": temp cleaned");
        }
        try
        {
            var nvPath = Path.Combine(folder, "nhanvien.json");
            var kmPath = Path.Combine(folder, "ctkm.json");
            var voucherPath = Path.Combine(folder, "voucher.json");
            var nvDao = new NhanVienDAO(nvPath);
            var kmDao = new CTKMDAO(kmPath);
            var voucherDao = new VoucherDAO(voucherPath);
            var nv = new NhanVienBUS(nvDao);
            var km = new CTKMBUS(kmDao, voucherDao);
            var vouchers = new VoucherBUS(voucherDao, kmDao);
            File.WriteAllText(Path.Combine(folder, "dulieu.json"), "aircraft sentinel");
            File.WriteAllText(Path.Combine(folder, "khachhang.json"), "customer sentinel");
            Check(nv.TimKiem().Count == 0 && km.TimKiem().Count == 0 && vouchers.TimKiem().Count == 0,
                "All modules start empty");
            nv.Luu(new NhanVienDTO(" NV01 ", " Nguyễn Văn ", " An ", "Bán vé"), false);
            Check(nv.TimKiem().Cast<NhanVienDTO>().Single().MaNV == "NV01"
                && nv.TimKiem("NGUYỄN VĂN AN").Count == 1, "Employee create trims fields and searches full name");
            Reject(() => nv.Luu(new NhanVienDTO("nv01", "Trần", "Bình", "Quản lý"), false), "Duplicate employee code rejected ignoring case");
            Reject(() => nv.Luu(new NhanVienDTO("NV02", "Trần", "Bình", " "), false), "Employee position required");
            var editNv = nv.TimKiem().Cast<NhanVienDTO>().Single();
            editNv.ChucVu = "Quản lý";
            Check(nv.TimKiem("Quản lý").Count == 0, "Employee search returns detached copy");
            nv.Luu(editNv, true);
            Check(new NhanVienBUS(nvDao).TimKiem("QUẢN LÝ").Count == 1, "Employee update persists");
            Reject(() => nv.Luu(new NhanVienDTO("MISSING", "A", "B", "C"), true), "Updating absent employee rejected");
            Reject(() => nv.Xoa("MISSING"), "Deleting absent employee rejected");

            var start = DateTime.Today.AddMonths(2);
            km.Luu(new CTKMDTO(" KM01 ", " Ưu đãi mùa hè ", start.AddHours(12), start.AddDays(10)), false);
            km.Luu(new CTKMDTO("KM02", "Khuyến mãi cùng ngày", start, start), false);
            Check(km.TimKiem().Count == 2 && km.TimKiem("ƯU ĐÃI").Cast<CTKMDTO>().Single().NgayBatDau == start,
                "Future campaign dates accepted and time removed");
            Check(km.TimKiem(start.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)).Count == 2,
                "Campaign search supports formatted dates");
            Reject(() => km.Luu(new CTKMDTO("km01", "Trùng", start, start), false), "Duplicate campaign rejected ignoring case");
            Reject(() => km.Luu(new CTKMDTO("KM03", "Sai", start, start.AddDays(-1)), false), "End before start rejected");
            Reject(() => km.Luu(new CTKMDTO("KM03", "Sai", DateTime.MinValue, start), false), "Missing campaign start date rejected");
            Reject(() => km.Luu(new CTKMDTO("KM03", " ", start, start), false), "Campaign name required");
            var editKm = km.TimKiem("KM01").Cast<CTKMDTO>().Single();
            editKm.TenCTKM = "Ưu đãi đã sửa";
            Check(km.TimKiem("đã sửa").Count == 0, "Campaign search returns detached copy");
            km.Luu(editKm, true);
            Check(new CTKMBUS(kmDao, voucherDao).TimKiem("đã sửa").Count == 1, "Campaign update persists");
            Reject(() => km.Luu(new CTKMDTO("MISSING", "Sai", start, start), true), "Updating absent campaign rejected");

            Check(vouchers.LayCTKM().Count == 2, "Voucher choices read campaigns added after startup");
            Reject(() => vouchers.Luu(new VoucherDTO("V01", "MISSING", 10), false), "Voucher requires existing campaign");
            Reject(() => vouchers.Luu(new VoucherDTO("V01", "KM01", 0), false), "Zero percentage rejected");
            foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 10.555d })
                Reject(() => vouchers.Luu(new VoucherDTO("V_BAD", "KM01", invalid), false), "Non-finite or over-precision double percentage rejected");
            Reject(() => vouchers.Luu(new VoucherDTO("V01", "KM01", -1), false), "Negative percentage rejected");
            Reject(() => vouchers.Luu(new VoucherDTO("V01", "KM01", 100.01d), false), "Percentage above 100 rejected");
            Reject(() => vouchers.Luu(new VoucherDTO(" ", "KM01", 10), false), "Voucher code required");
            vouchers.Luu(new VoucherDTO(" V01 ", "km01", 10.5d), false);
            vouchers.Luu(new VoucherDTO("V02", "KM01", 100), false);
            Check(vouchers.TimKiem("10.5").Cast<VoucherDTO>().Single().MaCTKM == "KM01"
                && new VoucherBUS(voucherDao, kmDao).TimKiem().Count == 2,
                "Fractional and 100 percent vouchers persist with canonical parent code");
            Reject(() => vouchers.Luu(new VoucherDTO("v01", "KM02", 20), false), "Duplicate voucher rejected ignoring case");
            Reject(() => km.Xoa("km01"), "Referenced campaign cannot be deleted");
            var editVoucher = vouchers.TimKiem("V01").Cast<VoucherDTO>().Single();
            editVoucher.MaCTKM = "KM02";
            editVoucher.GiamGia = 25.25d;
            Check(vouchers.TimKiem("25.25").Count == 0, "Voucher search returns detached copy");
            vouchers.Luu(editVoucher, true);
            Check(new VoucherBUS(voucherDao, kmDao).TimKiem("V01").Cast<VoucherDTO>().Single().MaCTKM == "KM02",
                "Voucher can move to another valid campaign");
            Reject(() => vouchers.Luu(new VoucherDTO("MISSING", "KM02", 10), true), "Updating absent voucher rejected");
            Reject(() => vouchers.Xoa("MISSING"), "Deleting absent voucher rejected");
            vouchers.Xoa("v02");
            km.Xoa("KM01");
            Check(new CTKMBUS(kmDao, voucherDao).TimKiem().Count == 1
                && new VoucherBUS(voucherDao, kmDao).TimKiem().Count == 1,
                "Delete persists after removing or moving references");
            Reject(() => km.Xoa("MISSING"), "Deleting absent campaign rejected");
            km.Luu(new CTKMDTO("KM03", "Mới", start, start.AddDays(1)), false);
            Check(vouchers.LayCTKM().Count == 2, "Voucher choices refresh after adding campaign");

            FailedSave(nvPath, () => nv.Luu(new NhanVienDTO("NV99", "A", "B", "C"), false),
                () => nv.TimKiem().Count == 1, "Employee write failure preserves memory");
            FailedSave(kmPath, () => km.Luu(new CTKMDTO("KM99", "Test", start, start), false),
                () => km.TimKiem().Count == 2, "Campaign write failure preserves memory");
            FailedSave(voucherPath, () => vouchers.Luu(new VoucherDTO("V99", "KM02", 10), false),
                () => vouchers.TimKiem().Count == 1, "Voucher write failure preserves memory");
            BadFile(nvPath, "[null]", () => { _ = new NhanVienBUS(nvDao); }, "Null employee row rejected without changing file");
            BadFile(nvPath, JsonSerializer.Serialize(new[] { new NhanVienDTO("X", "A", "B", "C"), new NhanVienDTO("x", "A", "B", "C") }),
                () => { _ = new NhanVienBUS(nvDao); }, "Stored duplicate employees rejected without changing file");
            BadFile(kmPath, JsonSerializer.Serialize(new[] { new CTKMDTO("X", "Invalid", start, start.AddDays(-1)) }),
                () => { _ = new CTKMBUS(kmDao, voucherDao); }, "Stored invalid campaign dates rejected without changing file");
            BadFile(voucherPath, JsonSerializer.Serialize(new[] { new VoucherDTO("X", "MISSING", 10) }),
                () => { _ = new VoucherBUS(voucherDao, kmDao); }, "Stored broken voucher reference rejected without changing file");
            BadFile(voucherPath, JsonSerializer.Serialize(new[] { new VoucherDTO("X", "KM02", 101) }),
                () => { _ = new VoucherBUS(voucherDao, kmDao); }, "Stored invalid discount rejected without changing file");
            BadFile(voucherPath, "broken JSON", () => { _ = new VoucherBUS(voucherDao, kmDao); },
                "Corrupt JSON preserved");
            Check(File.ReadAllText(Path.Combine(folder, "dulieu.json")) == "aircraft sentinel"
                && File.ReadAllText(Path.Combine(folder, "khachhang.json")) == "customer sentinel",
                "New modules do not overwrite aircraft or customer files");
            nv.Xoa("nv01");
            Check(new NhanVienBUS(nvDao).TimKiem().Count == 0, "Employee delete persists");
            Console.WriteLine($"{passed} checks passed.");
        }
        finally { Directory.Delete(folder, recursive: true); }
    }
}
