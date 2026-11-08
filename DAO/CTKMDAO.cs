using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class CTKMDAO : JsonDanhSachDAO<CTKM>
{
    public CTKMDAO(string? duongDan = null) : base("ctkm.json", duongDan) { }
}
