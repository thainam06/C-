using QuanLyMayBay.DTO;
namespace QuanLyMayBay.DAO;
public class ThamSoDAO : JsonDanhSachDAO<ThamSo>
{
    public ThamSoDAO(string? path = null) : base("thamso.json", path) { }
}
