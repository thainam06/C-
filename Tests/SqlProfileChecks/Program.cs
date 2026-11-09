using System.Text.Json;
using QuanLyMayBay.BUS;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;
using QuanLyMayBay.Services;

namespace QuanLyMayBay.Tests;

internal static class SqlProfileChecks
{
    public static void Main()
    {
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", null);
        var path = Path.Combine(AppContext.BaseDirectory, "sql-mapping.BanVeMayBay.json");
        Environment.SetEnvironmentVariable("QLMB_SQL_MAPPING_FILE", path);
        int count = 0;
        void Check(bool ok, string name)
        { if (!ok) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); count++; }
        var folder = Path.Combine(Path.GetTempPath(), "sql-profile-" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        try
        {
            var mayBay = CSDLListDAO.DocAnhXa(typeof(MayBay));
            Check(mayBay.Table == "dbo.MayBay" && mayBay.Columns["MHHK"] == "MaHHK", "DTO MHHK maps to observed SQL MaHHK");
            var ghe = CSDLListDAO.DocAnhXa(typeof(Ghe));
            Check(ghe.Columns.Count == 3 && !ghe.Columns.ContainsKey("LoaiGhe"), "Only three physical seat columns selected/written");
            Check(ghe.DerivedColumns["LoaiGhe"] == "LoaiGheTuHangVe", "Seat type marked as derived");
            Check(ghe.KeyColumns.SequenceEqual(new[] { "MaGhe" }), "Seat key is global MaGhe as shown in diagram");
            var hang = CSDLListDAO.DocAnhXa(typeof(HangHangKhong));
            Check(hang.Columns.Keys.OrderBy(x => x).SequenceEqual(new[] { "MaHHK", "TenHang" }), "Airline choices map observed columns");
            Check(CSDLListDAO.DocAnhXa(typeof(HangVe)).Columns.Count == 4, "Ticket class mapping remains complete");
            Check(CSDLListDAO.DocAnhXa(typeof(CTKM)).Columns.ContainsKey("NgayKetThuc"), "Observed campaign columns mapped");
            Check(CSDLListDAO.DocAnhXa(typeof(Voucher)).Columns.ContainsKey("GiamGia"), "Observed voucher columns mapped");
            Check(CSDLListDAO.ChuanHoaKhoaSql("HV01     ") == "HV01", "CHAR padding does not mark retained SQL keys as deleted");
            Check(CSDLListDAO.ChuanHoaKhoaSql(" HV01 ") == " HV01", "Leading spaces are not silently merged into another key");
            var normal = new Ghe("G1", "M1", "HV01", Ghe.Vip);
            CSDLListDAO.ApDungCotSuyDien(normal, ghe);
            Check(normal.LoaiGhe == Ghe.Thuong, "HV01 derives normal seat without SQL LoaiGhe column");
            var vip = new Ghe("G2", "M1", "HV02");
            CSDLListDAO.ApDungCotSuyDien(vip, ghe);
            Check(vip.LoaiGhe == Ghe.Vip, "HV02 derives VIP seat");
            vip.MaHV = " hv2 ";
            CSDLListDAO.ApDungCotSuyDien(vip, ghe);
            Check(vip.LoaiGhe == Ghe.Vip, "Derived rule handles legacy code and whitespace");
            var profiles = JsonSerializer.Deserialize<Dictionary<string, AnhXaBangSql>>(File.ReadAllText(path))!;
            var tempMap = Path.Combine(folder, "mapping.json");
            void RejectMapping(Action<Dictionary<string, AnhXaBangSql>> change, Type type, string name)
            {
                var candidate = JsonSerializer.Deserialize<Dictionary<string, AnhXaBangSql>>(JsonSerializer.Serialize(profiles))!;
                change(candidate); File.WriteAllText(tempMap, JsonSerializer.Serialize(candidate));
                Environment.SetEnvironmentVariable("QLMB_SQL_MAPPING_FILE", tempMap);
                try { CSDLListDAO.DocAnhXa(type); throw new Exception("FAIL: " + name); }
                catch (InvalidDataException) { Check(true, name); }
                finally { Environment.SetEnvironmentVariable("QLMB_SQL_MAPPING_FILE", path); }
            }
            RejectMapping(x => x["Ghe"].DerivedColumns["LoaiGhe"] = "SELECT 1; DROP TABLE Ghe", typeof(Ghe), "Derived rule cannot contain executable SQL");
            RejectMapping(x => x["Ghe"].Columns["LoaiGhe"] = "LoaiGhe", typeof(Ghe), "Field cannot be both physical and derived");
            RejectMapping(x => x["Ghe"].KeyColumns = new() { "LoaiGhe" }, typeof(Ghe), "Derived property cannot be SQL primary key");
            RejectMapping(x => x["MayBay"].Columns.Remove("MHHK"), typeof(MayBay), "Unmapped required property still rejected");
            RejectMapping(x => x["MayBay"].DerivedColumns["MHHK"] = "LoaiGheTuHangVe", typeof(MayBay), "Derived whitelist is restricted to seat type");

            var dao = new DuLieuDAO(Path.Combine(folder, "dulieu.json"));
            dao.Luu(new DuLieu
            {
                MayBay = new() { new("M1", "VN"), new("M2", "THA") },
                HangVe = new() { new("HV01", "Hạng phổ thông", 20, 100), new("HV02", "Hạng thương gia", 40, 200) },
                Ghe = new() { new("1A", "M1", "HV01"), new("1A", "M2", "HV02", Ghe.Vip) }
            });
            var bus = new QuanLyBUS(dao);
            Check(bus.TimGhe().Count == 2, "JSON mode retains seat uniqueness per aircraft");
            bus.LuuGhe("1A", "M1", "HV02", true, Ghe.Thuong);
            Check(bus.LayBangTongHop().Cast<DongTongHop>().Single(x => x.MaMayBay == "M1").LoaiGhe == Ghe.Vip,
                "Changing fare still updates derived seat type in JSON");
            Check(bus.LayHangHangKhong().Cast<HangHangKhong>().Select(x => x.MaHHK).OrderBy(x => x).SequenceEqual(new[] { "THA", "VN" }),
                "JSON airline choices use actual saved codes");
            var export = Path.Combine(folder, "schema.json");
            try { SqlSchemaService.XuatCauTruc(export); throw new Exception("Expected missing connection"); }
            catch (InvalidOperationException) { Check(!File.Exists(export), "Schema exporter does not create fake metadata without SQL"); }
            Console.WriteLine($"{count} offline checks passed. SQL Server live connection is not tested here.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("QLMB_SQL_MAPPING_FILE", null);
            Directory.Delete(folder, true);
        }
    }
}
