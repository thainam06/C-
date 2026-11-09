using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class VoucherDAO : JsonDanhSachDAO<Voucher>
{
    public VoucherDAO(string? duongDan = null) : base("voucher.json", duongDan) { }

    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(Voucher),
        new[] { "MaVoucher", "MaCTKM", "GiamGia" },
        (reader, map) => new Voucher
        {
            MaVoucher = DanhMucMySql.Chuoi(reader, "MaVoucher"),
            MaCTKM = DanhMucMySql.Chuoi(reader, "MaCTKM"),
            GiamGia = DanhMucMySql.SoThapPhan(reader, "GiamGia"),
        });

    public List<Voucher> DocDS() => Doc();
    public void Them(Voucher item) => Lenh.Them(item);
    public void Sua(Voucher item) => Lenh.Sua(item);
    public void Xoa(Voucher item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
