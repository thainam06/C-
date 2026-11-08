using System.Collections;
using System.Globalization;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.BUS;

public class CTKMBUS
{
    private readonly CTKMDAO dao;
    private readonly VoucherDAO voucherDAO;
    private List<CTKM> danhSach;

    public CTKMBUS(CTKMDAO? dao = null, VoucherDAO? voucherDAO = null)
    {
        this.dao = dao ?? new CTKMDAO();
        this.voucherDAO = voucherDAO ?? new VoucherDAO(Path.Combine(this.dao.ThuMuc, "voucher.json"));
        danhSach = this.dao.Doc();
        var ma = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var km in danhSach)
            {
                KiemTra(km);
                if (!ma.Add(km.MaCTKM)) throw new InvalidDataException();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
        {
            throw new InvalidDataException("Dữ liệu CTKM bị thiếu, trùng mã hoặc sai khoảng ngày. File được giữ nguyên.");
        }
    }

    private static string BatBuoc(string? value, string ten) => !string.IsNullOrWhiteSpace(value)
        ? value.Trim() : throw new ArgumentException($"Vui lòng nhập {ten}.");

    private static CTKM SaoChep(CTKM km) => new(km.MaCTKM, km.TenCTKM, km.NgayBatDau, km.NgayKetThuc);

    private static void KiemTra(CTKM km)
    {
        BatBuoc(km.MaCTKM, "mã CTKM");
        BatBuoc(km.TenCTKM, "tên CTKM");
        if (km.NgayBatDau.Date < new DateTime(1753, 1, 1) || km.NgayKetThuc.Date > new DateTime(9998, 12, 31)
            || km.NgayKetThuc.Date < km.NgayBatDau.Date)
            throw new ArgumentException("Khoảng ngày không hợp lệ. Ngày kết thúc phải từ ngày bắt đầu trở đi.");
    }

    public ArrayList TimKiem(string tuKhoa = "")
    {
        var ketQua = new ArrayList();
        foreach (CTKM km in new ArrayList(danhSach))
            if (new[] { km.MaCTKM, km.TenCTKM, km.NgayBatDau.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                km.NgayKetThuc.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) }
                .Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(SaoChep(km));
        return ketQua;
    }

    public void Luu(CTKM km, bool sua)
    {
        ArgumentNullException.ThrowIfNull(km);
        var moi = new CTKM(BatBuoc(km.MaCTKM, "mã CTKM"), BatBuoc(km.TenCTKM, "tên CTKM"),
            km.NgayBatDau.Date, km.NgayKetThuc.Date);
        KiemTra(moi);
        var index = danhSach.FindIndex(x => string.Equals(x.MaCTKM, moi.MaCTKM, StringComparison.OrdinalIgnoreCase));
        if (sua && index < 0) throw new ArgumentException("Không tìm thấy CTKM cần sửa.");
        if (!sua && index >= 0) throw new ArgumentException("Mã CTKM đã tồn tại.");
        var data = danhSach.Select(SaoChep).ToList();
        if (sua) data[index] = moi;
        else data.Add(moi);
        dao.Luu(data);
        danhSach = data;
    }

    public void Xoa(string maCTKM)
    {
        maCTKM = BatBuoc(maCTKM, "mã CTKM");
        if (voucherDAO.Doc().Any(x => string.Equals(x.MaCTKM, maCTKM, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("CTKM đang có Voucher. Hãy chuyển Voucher sang CTKM khác hoặc xóa Voucher trước.");
        var data = danhSach.Select(SaoChep).ToList();
        if (data.RemoveAll(x => string.Equals(x.MaCTKM, maCTKM, StringComparison.OrdinalIgnoreCase)) == 0)
            throw new ArgumentException("Không tìm thấy CTKM cần xóa.");
        dao.Luu(data);
        danhSach = data;
    }
}
