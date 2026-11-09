using System.Collections;
using VeMayBay.DAO;
using VeMayBay.DTO;

namespace VeMayBay.BUS;

public class NhanVienBUS
{
    private readonly NhanVienDAO dao;
    private List<NhanVienDTO> danhSach;

    public NhanVienBUS(NhanVienDAO? dao = null)
    {
        this.dao = dao ?? new NhanVienDAO();
        danhSach = this.dao.Doc();
        var ma = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var nv in danhSach)
            {
                KiemTra(nv, dangDoc: true);
                if (!ma.Add(nv.MaNV)) throw new InvalidDataException();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidDataException)
        {
            throw new InvalidDataException("Dữ liệu nhân viên bị thiếu hoặc trùng mã. File được giữ nguyên.");
        }
    }

    private static string BatBuoc(string? value, string ten) => !string.IsNullOrWhiteSpace(value)
        ? value.Trim() : throw new ArgumentException($"Vui lòng nhập {ten}.");

    private static NhanVienDTO SaoChep(NhanVienDTO nv) => new(nv.MaNV, nv.Ho, nv.Ten, nv.ChucVu);

    private static void KiemTra(NhanVienDTO nv, bool dangDoc = false)
    {
        BatBuoc(nv.MaNV, "mã nhân viên");
        BatBuoc(nv.Ho, "họ");
        BatBuoc(nv.Ten, "tên");
        if (!dangDoc || !MyConnection.DaCauHinh) BatBuoc(nv.ChucVu, "chức vụ");
    }

    public ArrayList TimKiem(string tuKhoa = "")
    {
        var ketQua = new ArrayList();
        foreach (NhanVienDTO nv in new ArrayList(danhSach))
            if (new[] { nv.MaNV, nv.Ho, nv.Ten, nv.ChucVu, $"{nv.Ho} {nv.Ten}" }
                .Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(SaoChep(nv));
        return ketQua;
    }

    public void Luu(NhanVienDTO nv, bool sua)
    {
        ArgumentNullException.ThrowIfNull(nv);
        var moi = new NhanVienDTO(BatBuoc(nv.MaNV, "mã nhân viên"), BatBuoc(nv.Ho, "họ"),
            BatBuoc(nv.Ten, "tên"), BatBuoc(nv.ChucVu, "chức vụ"));
        var index = danhSach.FindIndex(x => string.Equals(x.MaNV, moi.MaNV, StringComparison.OrdinalIgnoreCase));
        if (sua && index < 0) throw new ArgumentException("Không tìm thấy nhân viên cần sửa.");
        if (!sua && index >= 0) throw new ArgumentException("Mã nhân viên đã tồn tại.");
        var data = danhSach.Select(SaoChep).ToList();
        if (sua) data[index] = moi;
        else data.Add(moi);
        dao.Luu(data);
        danhSach = data;
    }

    public void NhapDanhSach(List<NhanVienDTO> rows)
    {
        var next = NhapLoatBUS.HopNhat(danhSach, rows, x => x.MaNV,
            x => new NhanVienDTO(BatBuoc(x.MaNV, "mã nhân viên"), BatBuoc(x.Ho, "họ"),
                BatBuoc(x.Ten, "tên"), BatBuoc(x.ChucVu, "chức vụ")), nv => KiemTra(nv));
        dao.Luu(next); danhSach = next;
    }

    public void Xoa(string maNV)
    {
        maNV = BatBuoc(maNV, "mã nhân viên");
        var data = danhSach.Select(SaoChep).ToList();
        if (data.RemoveAll(x => string.Equals(x.MaNV, maNV, StringComparison.OrdinalIgnoreCase)) == 0)
            throw new ArgumentException("Không tìm thấy nhân viên cần xóa.");
        dao.Luu(data);
        danhSach = data;
    }
}
