using MySqlConnector;
using System.Text.Json;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;
using QuanLyMayBay.Services;

namespace QuanLyMayBay.Tests;

// Opt-in only. Creates/drops fixture tables exclusively in database qlmb_dao_test.
internal static class MySqlChecks
{
    public static int Main()
    {
        var input = Environment.GetEnvironmentVariable("QLMB_MYSQL_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(input))
        { Console.WriteLine("SKIP: set QLMB_MYSQL_TEST_CONNECTION to isolated database qlmb_dao_test."); return 0; }
        var builder = new MySqlConnectionStringBuilder(input);
        if (builder.Database != "qlmb_dao_test") throw new InvalidOperationException("Test requires database qlmb_dao_test; never use group data.");
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", input);
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_MAPPING_FILE", Path.Combine(AppContext.BaseDirectory, "mysql-mapping.example.json"));
        using var connection = new MySqlConnection(input); connection.Open();
        void Sql(string sql) { using var command = new MySqlCommand(sql, connection); command.ExecuteNonQuery(); }
        Sql("SET FOREIGN_KEY_CHECKS=1");
        foreach (var table in new[] { "Voucher", "CTKM", "Ghe", "HangVe", "MayBay", "HangHangKhong", "NhanVien" }) Sql("DROP TABLE IF EXISTS `" + table + "`");
        Sql("CREATE TABLE NhanVien(MaNV CHAR(12) PRIMARY KEY,Ho VARCHAR(100) NOT NULL,Ten VARCHAR(100) NOT NULL UNIQUE,ChucVu VARCHAR(100) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        Sql("CREATE TABLE HangHangKhong(MaHHK VARCHAR(12) PRIMARY KEY,TenHang VARCHAR(100)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        Sql("CREATE TABLE MayBay(MaMayBay VARCHAR(12) PRIMARY KEY,MaHHK VARCHAR(12),FOREIGN KEY(MaHHK) REFERENCES HangHangKhong(MaHHK)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        Sql("CREATE TABLE HangVe(MaHV VARCHAR(12) PRIMARY KEY,TenHV VARCHAR(100),HanhLyKg DECIMAL(12,2),DonGiaVe DECIMAL(18,2)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        Sql("CREATE TABLE Ghe(MaGhe VARCHAR(12) PRIMARY KEY,MaMayBay VARCHAR(12),MaHV VARCHAR(12),FOREIGN KEY(MaMayBay) REFERENCES MayBay(MaMayBay),FOREIGN KEY(MaHV) REFERENCES HangVe(MaHV)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        Sql("CREATE TABLE CTKM(MaCTKM VARCHAR(12) PRIMARY KEY,TenCTKM VARCHAR(100),NgayBatDau DATETIME,NgayKetThuc DATETIME) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        Sql("CREATE TABLE Voucher(MaVoucher VARCHAR(12) PRIMARY KEY,MaCTKM VARCHAR(12),GiamGia DECIMAL(5,2),FOREIGN KEY(MaCTKM) REFERENCES CTKM(MaCTKM)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4");
        int count = 0;
        void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); count++; Console.WriteLine("PASS: " + name); }
        void Reject<T>(Action action, string name) where T : Exception
        { try { action(); } catch (T) { Check(true, name); return; } throw new Exception("FAIL: " + name); }
        Check(MyConnection.KiemTra() == "qlmb_dao_test", "Connect to real MySQL database");
        Check(CSDLListDAO.TenSql("a`b") == "`a``b`", "MySQL identifier quoting");
        var dao = new NhanVienDAO(); var rows = dao.Doc();
        rows.Add(new("NV01", "Nguyễn", "An ✈", "Bán vé")); dao.Luu(rows);
        Check(dao.Doc().Single().Ho == "Nguyễn", "Unicode insert/read through DAO");
        Check(dao.Doc().Single().MaNV == "NV01", "CHAR primary key padding normalized");
        rows = dao.Doc(); rows[0].Ten = "x'); DROP TABLE NhanVien; --"; dao.Luu(rows);
        Check(dao.Doc().Single().Ten == rows[0].Ten, "SQL-looking text is parameterized data");
        rows = dao.Doc(); rows.Add(new("NV02", "Trần", "Bình", "Quản lý")); dao.Luu(rows);
        var first = new NhanVienDAO(); var second = new NhanVienDAO();
        var a = first.Doc(); var b = second.Doc(); a[0].ChucVu = "Trưởng nhóm"; first.Luu(a);
        b[0].ChucVu = "Stale";
        Reject<InvalidOperationException>(() => second.Luu(b), "Stale session rejected");
        Check(dao.Doc().First(x => x.MaNV == "NV01").ChucVu == "Trưởng nhóm", "Concurrent write not overwritten");
        rows = dao.Doc(); rows[0].Ho = "Must rollback"; rows.Add(new("NV03", "Lê", rows[1].Ten, "Test"));
        Reject<MySqlException>(() => dao.Luu(rows), "Unrelated UNIQUE collision fails instead of updating another row");
        Check(dao.Doc().All(x => x.Ho != "Must rollback") && dao.Doc().Count == 2, "Earlier writes rollback on unique failure");
        rows = dao.Doc(); rows.RemoveAll(x => x.MaNV == "NV02"); dao.Luu(rows);
        Check(dao.Doc().Single().MaNV == "NV01", "Delete removes only requested key");
        Sql("INSERT INTO HangHangKhong VALUES('VN','Vietnam Airlines')");
        var air = new DuLieuDAO(); var data = air.Doc();
        data.MayBay.Add(new("M01", "VN")); data.HangVe.Add(new("HV01", "Hạng phổ thông", 20.25m, 1250000.50m));
        data.HangVe.Add(new("HV02", "Hạng thương gia", 40, 2500000)); data.Ghe.Add(new("G01", "M01", "HV02")); air.Luu(data);
        var saved = air.Doc();
        Check(saved.Ghe.Single().LoaiGhe == Ghe.Vip, "Derived VIP from physical three-column seat table");
        Check(saved.HangVe.Single(x => x.MaHV == "HV01").DonGiaVe == 1250000.50m, "Decimal money exact round trip");
        saved.MayBay.Add(new("M02", "VN")); saved.Ghe.Add(new("G02", "M02", "missing"));
        Reject<MySqlException>(() => air.Luu(saved), "Foreign key violation fails multi-table save");
        Check(air.Doc().MayBay.Count == 1 && air.Doc().Ghe.Count == 1, "Parent insertion rolled back with failed child");
        saved = air.Doc(); saved.Ghe.Clear(); saved.MayBay.Clear(); air.Luu(saved);
        Check(air.Doc().MayBay.Count == 0, "Children deleted before parents within one transaction");
        var promo = new CTKMDAO(); var campaigns = promo.Doc();
        campaigns.Add(new("KM01", "Khuyến mãi", new DateTime(2026,11,9), new DateTime(2026,12,31))); promo.Luu(campaigns);
        Check(promo.Doc().Single().NgayKetThuc == new DateTime(2026,12,31), "Date round trip");
        var vouchers = new VoucherDAO(); var v = vouchers.Doc(); v.Add(new("V01", "KM01", 12.5m)); vouchers.Luu(v);
        Check(vouchers.Doc().Single().GiamGia == 12.5m, "Percentage decimal round trip");
        campaigns = promo.Doc(); campaigns.Clear();
        Reject<MySqlException>(() => promo.Luu(campaigns), "Referenced campaign deletion blocked");
        var metadata = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try { SqlSchemaService.XuatCauTruc(metadata); using var json = JsonDocument.Parse(File.ReadAllText(metadata));
            Check(json.RootElement.GetProperty("ForeignKeys").GetArrayLength() == 4, "Real MySQL information_schema FK export"); }
        finally { File.Delete(metadata); }
        // Explicitly test rejection of engines that cannot honor rollback.
        Sql("ALTER TABLE NhanVien ENGINE=MyISAM"); rows = dao.Doc();
        Reject<InvalidOperationException>(() => dao.Luu(rows), "Nontransactional table rejected before writing");
        Console.WriteLine($"{count} real MySQL checks passed; fixture database only.");
        return 0;
    }
}
