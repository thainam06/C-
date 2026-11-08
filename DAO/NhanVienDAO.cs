using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class NhanVienDAO : JsonDanhSachDAO<NhanVien>
{
    public NhanVienDAO(string? duongDan = null) : base("nhanvien.json", duongDan) { }
}
