using System.Collections;
using System.Globalization;
using VeMayBay.DAO;
using VeMayBay.DTO;

namespace VeMayBay.BUS;

public class VoucherBUS
{
    private readonly VoucherDAO dao;
    private readonly CTKMDAO ctkmDAO;
    private List<VoucherDTO> danhSach;

    public VoucherBUS(VoucherDAO? dao = null, CTKMDAO? ctkmDAO = null)
    {
        this.ctkmDAO = ctkmDAO ?? new CTKMDAO();
        this.dao = dao ?? new VoucherDAO(Path.Combine(this.ctkmDAO.ThuMuc, "voucher.json"));
        danhSach = this.dao.Doc();
        var cacCTKM = LayCTKM().Cast<CTKMDTO>().Select(x => x.MaCTKM).ToHashSet(StringComparer.OrdinalIgnoreCase);
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

    private static VoucherDTO SaoChep(VoucherDTO v) => new(v.MaVoucher, v.MaCTKM, v.GiamGia);

    private static void KiemTra(VoucherDTO v)
    {
        BatBuoc(v.MaVoucher, "mã Voucher");
        BatBuoc(v.MaCTKM, "CTKM");
        if (!double.IsFinite(v.GiamGia) || v.GiamGia <= 0 || v.GiamGia > 100)
            throw new ArgumentException("Giảm giá phải là số hữu hạn, lớn hơn 0% và không quá 100%.");
        if (decimal.Round(Convert.ToDecimal(v.GiamGia), 2) != Convert.ToDecimal(v.GiamGia))
            throw new ArgumentException("Giảm giá chỉ nhận tối đa 2 chữ số thập phân theo CSDL.");
    }

    public ArrayList LayCTKM() => new CTKMBUS(ctkmDAO, dao).TimKiem();

    public ArrayList TimKiem(string tuKhoa = "")
    {
        var ketQua = new ArrayList();
        foreach (VoucherDTO v in new ArrayList(danhSach))
            if (new[] { v.MaVoucher, v.MaCTKM, v.GiamGia.ToString(CultureInfo.InvariantCulture) }
                .Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(SaoChep(v));
        return ketQua;
    }

    public void Luu(VoucherDTO voucher, bool sua)
    {
        ArgumentNullException.ThrowIfNull(voucher);
        var moi = new VoucherDTO(BatBuoc(voucher.MaVoucher, "mã Voucher"), BatBuoc(voucher.MaCTKM, "CTKM"), voucher.GiamGia);
        KiemTra(moi);
        var km = LayCTKM().Cast<CTKMDTO>()
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

    public void NhapDanhSach(List<VoucherDTO> rows)
    {
        var parents = LayCTKM().Cast<CTKMDTO>().ToDictionary(x => x.MaCTKM, StringComparer.OrdinalIgnoreCase);
        var next = NhapLoatBUS.HopNhat(danhSach, rows, x => x.MaVoucher, x =>
        {
            var ma = BatBuoc(x.MaVoucher, "mã Voucher");
            if (!parents.TryGetValue(BatBuoc(x.MaCTKM, "CTKM"), out var km)) throw new ArgumentException("CTKM không tồn tại.");
            return new VoucherDTO(ma, km.MaCTKM, x.GiamGia);
        }, KiemTra);
        dao.Luu(next); danhSach = next;
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
