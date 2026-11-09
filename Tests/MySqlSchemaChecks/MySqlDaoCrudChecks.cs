using System.Text.Json;
using MySql.Data.MySqlClient;
using VeMayBay.DAO;
using VeMayBay.DTO;

namespace VeMayBay.Tests;

internal static class MySqlDaoCrudChecks
{
    private sealed record Case(Action Add, Action Update, Action Delete, Func<bool> HasAdded, Func<bool> HasUpdated, Func<bool> IsDeleted, string Name);

    internal static void Run(Action<bool, string> check, MySqlConnection connection)
    {
        using (var command = new MySqlCommand(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "ThamSo.optional.sql")), connection)) command.ExecuteNonQuery();
        var cases = new List<Case>();
        void Add<T>(T first, T second, Func<List<T>> read, Action<T> insert, Action<T> update, Action<string> delete, Func<T, string> key)
        {
            var id = key(first);
            bool Matches(T expected) => read().Any(x => key(x) == id && typeof(T).GetProperties().All(p => Equals(p.GetValue(x), p.GetValue(expected))));
            cases.Add(new(() => insert(first), () => update(second), () => delete(id), () => Matches(first), () => Matches(second), () => read().All(x => key(x) != id), typeof(T).Name));
        }
        var airline = new HangHangKhongDAO();
        Add(new HangHangKhongDTO { MaHHK = "T_HHK", TenHang = "O'Hare Việt ✈" }, new HangHangKhongDTO { MaHHK = "T_HHK", TenHang = "Hãng đã sửa" }, airline.DocDS, airline.Them, airline.Sua, airline.Xoa, x => x.MaHHK);
        var plane = new MayBayDAO();
        Add(new MayBayDTO("T_MB", "T_HHK"), new MayBayDTO("T_MB", "VN"), plane.DocDS, plane.Them, plane.Sua, plane.Xoa, x => x.MaMayBay);
        var fare = new HangVeDAO();
        Add(new HangVeDTO("HV02", "Hạng thương gia", 30, 2000000), new HangVeDTO("HV02", "Hạng thương gia", 40, 3000000), fare.DocDS, fare.Them, fare.Sua, fare.Xoa, x => x.MaHV);
        var seat = new GheDAO();
        Add(new GheDTO("T_GHE", "T_MB", "HV01", GheDTO.Thuong), new GheDTO("T_GHE", "T_MB", "HV02", GheDTO.Vip), seat.DocDS, seat.Them, seat.Sua, seat.Xoa, x => x.MaGhe);
        // Use an existing BUS-generated hash rather than bypassing the app's password rule.
        var hash = new KhachHangDAO().DocDS().Single(x => x.MaKH == "KH01").MatKhau;
        var customer = new KhachHangDAO();
        Add(new KhachHangDTO("T_KH", "Khách O'Nam", "001000000002", "0901234567", "test@example.test", "dao-test", hash, "Hà Nội"),
            new KhachHangDTO("T_KH", "Khách đã sửa", "001000000002", "0901234567", "test@example.test", "dao-test", hash, "Đà Nẵng"), customer.DocDS, customer.Them, customer.Sua, customer.Xoa, x => x.MaKH);
        var passenger = new HanhKhachDAO();
        Add(new HanhKhachDTO("T_HK", "Hành khách", new DateTime(2000,1,1), "", "Nam", "T_KH", "Người lớn"), new HanhKhachDTO("T_HK", "Hành khách sửa", new DateTime(2000,1,2), "", "Nam", "T_KH", "Trẻ em"), passenger.DocDS, passenger.Them, passenger.Sua, passenger.Xoa, x => x.MaHK);
        var staff = new NhanVienDAO();
        Add(new NhanVienDTO("T_NV", "Lê", "O'An", "Bán vé"), new NhanVienDTO("T_NV", "Lê", "O'An", "Quản lý"), staff.DocDS, staff.Them, staff.Sua, staff.Xoa, x => x.MaNV);
        var campaign = new CTKMDAO();
        Add(new CTKMDTO("T_KM", "Ưu đãi", new DateTime(2026,1,1), new DateTime(2026,12,31)), new CTKMDTO("T_KM", "Ưu đãi sửa", new DateTime(2026,1,2), new DateTime(2026,12,31)), campaign.DocDS, campaign.Them, campaign.Sua, campaign.Xoa, x => x.MaCTKM);
        var voucher = new VoucherDAO();
        Add(new VoucherDTO("T_V", "T_KM", 12.5d), new VoucherDTO("T_V", "T_KM", 15.75d), voucher.DocDS, voucher.Them, voucher.Sua, voucher.Xoa, x => x.MaVoucher);
        var setting = new ThamSoDAO();
        Add(new ThamSoDTO { MaThamSo = "T_TS", GiaTri = "Việt Nam", MoTa = "Mẫu" }, new ThamSoDTO { MaThamSo = "T_TS", GiaTri = "Đã sửa", MoTa = "Mẫu" }, setting.DocDS, setting.Them, setting.Sua, setting.Xoa, x => x.MaThamSo);
        using (var unopened = MyConnection.GetConnection()) check(unopened.State == System.Data.ConnectionState.Closed, "GetConnection matches sample: returns unopened MySQL connection");
        foreach (var test in cases) { test.Add(); check(test.HasAdded(), test.Name + ": Them + DocDS preserve typed fields"); }
        foreach (var test in cases) { test.Update(); check(test.HasUpdated(), test.Name + ": Sua + DocDS preserve typed fields"); }
        void Reject<T>(Action action, string name) where T : Exception
        { try { action(); } catch (T) { check(true, name); return; } throw new Exception("FAIL: " + name); }
        Reject<InvalidOperationException>(() => customer.Xoa("T_KH"), "Direct Xoa respects parent foreign key");
        Reject<InvalidOperationException>(() => staff.Them(new NhanVienDTO("T_NV", "Duplicate", "User", "Test")), "Direct Them rejects duplicate rather than replacing row");
        Reject<ArgumentException>(() => voucher.Sua(new VoucherDTO("T_V", "T_KM", double.NaN)), "Direct MySql.Data write rejects NaN percentage");
        Reject<ArgumentException>(() => voucher.Sua(new VoucherDTO("T_V", "T_KM", 12.345d)), "Double write rejects precision beyond DECIMAL(18,2)");
        Reject<ArgumentException>(() => fare.Sua(new HangVeDTO("HV02", "Hạng thương gia", 40.5m, 1)), "Direct Sua rejects fractional integer luggage");
        Reject<ArgumentException>(() => plane.Them(new MayBayDTO("TOO_LONG_CODE", "VN")), "Direct Them enforces actual VARCHAR length");
        Reject<ArgumentException>(() => staff.Sua(new NhanVienDTO("MISSING", "A", "B", "Test")), "Direct Sua reports missing key");
        Reject<ArgumentException>(() => staff.Xoa("MISSING"), "Direct Xoa reports missing key");
        Reject<ArgumentException>(() => staff.Them(new NhanVienDTO("", "A", "B", "Test")), "Direct Them rejects blank primary key");
        var baseline = staff.DocDS(); staff.Luu(baseline);
        check(staff.DocDS().Count == baseline.Count, "DocDS initializes the legacy batch-save snapshot");
        var same = staff.DocDS().Single(x => x.MaNV == "T_NV"); staff.Sua(same);
        check(staff.DocDS().Count(x => x.MaNV == "T_NV") == 1, "Unchanged update is valid and does not duplicate");
        // Alternate mappings must remain supported by the new explicit reader.
        var oldPath = Environment.GetEnvironmentVariable("QLMB_MYSQL_MAPPING_FILE");
        var mapPath = Path.Combine(AppContext.BaseDirectory, "mysql-mapping.BanVeMayBay.json");
        var maps = JsonSerializer.Deserialize<Dictionary<string, AnhXaBangSql>>(File.ReadAllText(mapPath))!;
        using (var command = new MySqlCommand("CREATE TABLE StaffAlt(Code varchar(10) PRIMARY KEY,Family varchar(50),Given varchar(50),RoleName varchar(50)) ENGINE=InnoDB", connection)) command.ExecuteNonQuery();
        maps["NhanVien"].Table = "StaffAlt"; maps["NhanVien"].Columns = new() { ["MaNV"] = "Code", ["Ho"] = "Family", ["Ten"] = "Given", ["ChucVu"] = "RoleName" };
        var temporary = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(maps)); Environment.SetEnvironmentVariable("QLMB_MYSQL_MAPPING_FILE", temporary);
            staff.Them(new NhanVienDTO("ALT", "Tên", "Khác", "Test"));
            check(staff.DocDS().Single().Ho == "Tên", "Explicit DAO supports mapped table/column names");
            staff.Sua(new NhanVienDTO("ALT", "Đã sửa", "Khác", "Test")); staff.Xoa("ALT");
            check(staff.DocDS().Count == 0, "Mapped Sua and Xoa target actual mapped key");
        }
        finally { Environment.SetEnvironmentVariable("QLMB_MYSQL_MAPPING_FILE", oldPath); File.Delete(temporary); }
        foreach (var test in cases.AsEnumerable().Reverse()) { test.Delete(); check(test.IsDeleted(), test.Name + ": Xoa removes only the requested row"); }
        check(staff.DocDS().Any(x => x.MaNV == "NV01"), "CRUD leaves unrelated fixture records intact");
    }
}
