using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class VoucherDAO : JsonDanhSachDAO<Voucher>
{
    public VoucherDAO(string? duongDan = null) : base("voucher.json", duongDan) { }
}
