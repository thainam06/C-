using MySqlConnector;
using System.Text.Json;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;
using QuanLyMayBay.BUS;

namespace QuanLyMayBay.Tests;

// Requires an isolated EMPTY fixture database, with docs/mysql/BanVeMayBay.mysql.sql imported first.
internal static class MySqlSchemaChecks
{
    public static int Main()
    {
        var input = Environment.GetEnvironmentVariable("QLMB_SCHEMA_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(input)) { Console.WriteLine("SKIP: QLMB_SCHEMA_TEST_CONNECTION not configured."); return 0; }
        if (new MySqlConnectionStringBuilder(input).Database != "qlmb_schema_test") throw new InvalidOperationException("Only isolated qlmb_schema_test is allowed.");
        Environment.SetEnvironmentVariable("QLMB_MYSQL_CONNECTION", input);
        Environment.SetEnvironmentVariable("QLMB_MYSQL_MAPPING_FILE", null);
        Environment.SetEnvironmentVariable("QLMB_SQL_CONNECTION", null);
        using var connection = new MySqlConnection(input); connection.Open();
        object? Scalar(string sql) { using var command = new MySqlCommand(sql, connection); return command.ExecuteScalar(); }
        void Sql(string sql) { using var command = new MySqlCommand(sql, connection); command.ExecuteNonQuery(); }
        int count = 0;
        void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); count++; Console.WriteLine("PASS: " + name); }
        void Reject<T>(Action action, string name) where T : Exception
        { try { action(); } catch (T) { Check(true, name); return; } throw new Exception("FAIL: " + name); }
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "schema-manifest.json")));
        foreach (var table in manifest.RootElement.GetProperty("tables").EnumerateObject())
        {
            var name = table.Name;
            Check(Convert.ToInt32(Scalar($"SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='{name}'"))
                == table.Value.GetProperty("columns").GetArrayLength(), "Column count preserved: " + name);
            using var command = new MySqlCommand($"SELECT COLUMN_NAME FROM information_schema.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='{name}' AND CONSTRAINT_NAME='PRIMARY' ORDER BY ORDINAL_POSITION", connection);
            using var reader = command.ExecuteReader(); var keys = new List<string>(); while (reader.Read()) keys.Add(reader.GetString(0));
            Check(keys.SequenceEqual(table.Value.GetProperty("pk").EnumerateArray().Select(x => x.GetString()!)), "Primary key preserved: " + name);
        }
        Check(Convert.ToInt32(Scalar("SELECT COUNT(*) FROM information_schema.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE()")) == 23, "All 23 FK constraints imported");
        Check(Convert.ToInt32(Scalar("SELECT COUNT(*) FROM information_schema.CHECK_CONSTRAINTS WHERE CONSTRAINT_SCHEMA=DATABASE()")) == 13, "All 13 CHECK constraints imported");
        Check(CSDLListDAO.DocAnhXa(typeof(HanhKhach)).Columns["LoaiKH"] == "LoaiHK", "Packaged default profile maps uploaded passenger column");
        Sql("INSERT INTO HangHangKhong VALUES('VN','Vietnam Airlines')");
        var air = new DuLieuDAO(); var data = air.Doc();
        data.MayBay.Add(new("M01", "VN")); data.HangVe.Add(new("HV01", "Hạng phổ thông", 20, 1000000)); data.Ghe.Add(new("G01", "M01", "HV01")); air.Luu(data);
        Check(air.Doc().Ghe.Single().LoaiGhe == Ghe.Thuong, "Seat mapping matches uploaded schema without LoaiGhe column");
        data = air.Doc(); data.HangVe[0].HanhLyKg = 20.5m;
        Reject<ArgumentException>(() => air.Luu(data), "Integer luggage column rejects fractional kilos without rounding");
        Check(air.Doc().HangVe.Single().HanhLyKg == 20, "Rejected scale leaves original row intact");
        data = air.Doc(); data.HangVe[0].DonGiaVe = 1000000.25m;
        Reject<ArgumentException>(() => air.Luu(data), "DECIMAL(18,0) fare rejects fractional money");
        data = air.Doc(); data.MayBay[0].MaMayBay = "TOO_LONG_CODE";
        Reject<ArgumentException>(() => air.Luu(data), "Uploaded VARCHAR(10) length enforced before write");
        var customer = new KhachHangBUS();
        customer.Luu(new KhachHang("KH01", "Nguyễn An", "001000000001", "0901234567", "an@example.test", "an", "TestPassword!", "Hà Nội"), false);
        Check(new KhachHangBUS().TimKiem().Count == 1, "Customer password hash fits VARCHAR(255)");
        Sql("UPDATE KhachHang SET SDT=NULL,Email=NULL,DiaChi=NULL WHERE MaKH='KH01'");
        Sql("INSERT INTO HanhKhach(MaHK,HoTen,MaKH,LoaiHK) VALUES('HK01','Nguyễn Bình','KH01','Người lớn')");
        var hkDao = new HanhKhachDAO(); var hk = hkDao.Doc().Single();
        Check(hk.LoaiKH == "Người lớn" && hk.NgaySinh == null, "LoaiKH maps LoaiHK and NULL birth date is preserved");
        Check(new HanhKhachBUS().TimKiem().Count == 1, "Existing nullable customer/passenger fields do not block opening module");
        hkDao.Luu(new[] { hk });
        Check(Scalar("SELECT NgaySinh FROM HanhKhach WHERE MaHK='HK01'") is DBNull, "DAO round trip retains NULL birth date");
        Sql("INSERT INTO NhanVien VALUES('NV01','Trần','An',NULL)");
        Check(new NhanVienBUS().TimKiem().Count == 1, "Nullable job title can be read");
        Sql("INSERT INTO SanBay VALUES('SGN','Tân Sơn Nhất',NULL),('HAN','Nội Bài',NULL)");
        Sql("INSERT INTO ChuyenBay(MaChuyenBay,MaHHK,MaSanBayDi,MaSanBayDen,MaMayBay,NgayKhoiHanh,NgayDen,TongSoGhe,GiaVe) VALUES('CB01','VN','SGN','HAN','M01','2026-12-01 09:00:00','2026-12-01 11:00:00',10,1000000)");
        Check(Convert.ToString(Scalar("SELECT TrangThai FROM ChuyenBay WHERE MaChuyenBay='CB01'")) == "Chưa khởi hành", "Unicode SQL Server default converted correctly");
        Reject<MySqlException>(() => Sql("UPDATE ChuyenBay SET SoVeDaBan=11 WHERE MaChuyenBay='CB01'"), "Flight capacity CHECK enforced");
        Sql("INSERT INTO DatVe(MaDV,MaKH,MaChuyenBay) VALUES('DV01','KH01','CB01')");
        Sql("INSERT INTO Ve(MaVe,MaChuyenBay,MaHV,MaGhe,MaHK,DonGiaVe,MaDV) VALUES('V01','CB01','HV01','G01','HK01',1000000,'DV01')");
        Reject<MySqlException>(() => Sql("INSERT INTO Ve(MaVe,MaChuyenBay,MaHV,MaGhe,MaHK,DonGiaVe,MaDV) VALUES('V02','CB01','HV01','G01','HK01',1000000,'DV01')"), "Unique flight/seat constraint preserved");
        Reject<MySqlException>(() => Sql("UPDATE Ve SET MaChuyenBay='OTHER' WHERE MaVe='V01'"), "Ticket foreign keys enforced");
        Sql("INSERT INTO HoaDon(MaHD,MaKH,MaDV) VALUES('HD01','KH01','DV01')");
        Sql("INSERT INTO ChiTietHoaDon(MaHD,MaVe,DonGiaVe,SL) VALUES('HD01','V01',1000000,2)");
        Check(Convert.ToDecimal(Scalar("SELECT ThanhTien FROM ChiTietHoaDon WHERE MaHD='HD01'")) == 2000000, "Computed invoice total preserved as generated column");
        Check(Scalar("SELECT NgayDat FROM DatVe WHERE MaDV='DV01'") is DateTime, "GETDATE default converted to CURRENT_TIMESTAMP");
        MySqlDaoCrudChecks.Run(Check, connection);
        Console.WriteLine($"{count} uploaded-schema MySQL checks passed.");
        return 0;
    }
}
