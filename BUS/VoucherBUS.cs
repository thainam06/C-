using System.Collections;
using System.Globalization;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.BUS;

public class VoucherBUS
{
    private readonly VoucherDAO dao;
    private readonly CTKMDAO ctkmDAO;
    private List<Voucher> danhSach;

    public VoucherBUS(VoucherDAO? dao = null, CTKMDAO? ctkmDAO = null)
    {
        this.ctkmDAO = ctkmDAO ?? new CTKMDAO();
        this.dao = dao ?? new VoucherDAO(Path.Combine(this.ctkmDAO.ThuMuc, "voucher.json"));
        danhSach = this.dao.Doc();
        var cacCTKM = LayCTKM().Cast<CTKM>().Select(x => x.MaCTKM).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var ma = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var voucher in danhSach)
            {
                KiemTra(voucher);
                if (!ma.Add(voucher.MaVoucher) || !cacCTKM.Contains(voucher.MaCTKM))
                    throw new InvalidDataException();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
        {
            throw new InvalidDataException("Dữ liệu Voucher bị thiếu, trùng mã, sai mức giảm hoặc mất liên kết CTKM. File được giữ nguyên.");
        }
    }

    private static string BatBuoc(string? value, string ten) => !string.IsNullOrWhiteSpace(value)
        ? value.Trim() : throw new ArgumentException($"Vui lòng nhập/chọn {ten}.");

    private static Voucher SaoChep(Voucher v) => new(v.MaVoucher, v.MaCTKM, v.GiamGia);

    private static void KiemTra(Voucher v)
    {
        BatBuoc(v.MaVoucher, "mã Voucher");
        BatBuoc(v.MaCTKM, "CTKM");
        if (v.GiamGia <= 0 || v.GiamGia > 100)
            throw new ArgumentException("Giảm giá phải lớn hơn 0% và không quá 100%.");
    }

    public ArrayList LayCTKM() => new CTKMBUS(ctkmDAO, dao).TimKiem();

    public ArrayList TimKiem(string tuKhoa = "")
    {
        var ketQua = new ArrayList();
        foreach (Voucher v in new ArrayList(danhSach))
            if (new[] { v.MaVoucher, v.MaCTKM, v.GiamGia.ToString(CultureInfo.InvariantCulture) }
                .Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(SaoChep(v));
        return ketQua;
    }

    public void Luu(Voucher voucher, bool sua)
    {
        ArgumentNullException.ThrowIfNull(voucher);
        var moi = new Voucher(BatBuoc(voucher.MaVoucher, "mã Voucher"), BatBuoc(voucher.MaCTKM, "CTKM"), voucher.GiamGia);
        KiemTra(moi);
        var km = LayCTKM().Cast<CTKM>()
            .FirstOrDefault(x => string.Equals(x.MaCTKM, moi.MaCTKM, StringComparison.OrdinalIgnoreCase));
        if (km == null) throw new ArgumentException("Mã CTKM không tồn tại. Hãy thêm CTKM trước.");
        moi.MaCTKM = km.MaCTKM;
        var index = danhSach.FindIndex(x => string.Equals(x.MaVoucher, moi.MaVoucher, StringComparison.OrdinalIgnoreCase));
        if (sua && index < 0) throw new ArgumentException("Không tìm thấy Voucher cần sửa.");
        if (!sua && index >= 0) throw new ArgumentException("Mã Voucher đã tồn tại.");
        var data = danhSach.Select(SaoChep).ToList();
        if (sua) data[index] = moi;
        else data.Add(moi);
        dao.Luu(data);
        danhSach = data;
    }

    public void Xoa(string maVoucher)
    {
        maVoucher = BatBuoc(maVoucher, "mã Voucher");
        var data = danhSach.Select(SaoChep).ToList();
        if (data.RemoveAll(x => string.Equals(x.MaVoucher, maVoucher, StringComparison.OrdinalIgnoreCase)) == 0)
            throw new ArgumentException("Không tìm thấy Voucher cần xóa.");
        dao.Luu(data);
        danhSach = data;
    }
}
