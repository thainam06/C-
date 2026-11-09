using QuanLyMayBay.DTO;
namespace QuanLyMayBay.DAO;
public class ThamSoDAO : JsonDanhSachDAO<ThamSo>
{
    public ThamSoDAO(string? path = null) : base("thamso.json", path) { }

    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(ThamSo),
        new[] { "MaThamSo", "GiaTri", "MoTa" },
        (reader, map) => new ThamSo
        {
            MaThamSo = DanhMucMySql.Chuoi(reader, "MaThamSo"),
            GiaTri = DanhMucMySql.Chuoi(reader, "GiaTri"),
            MoTa = DanhMucMySql.Chuoi(reader, "MoTa"),
        });

    public List<ThamSo> DocDS() => Doc();
    public void Them(ThamSo item) => Lenh.Them(item);
    public void Sua(ThamSo item) => Lenh.Sua(item);
    public void Xoa(ThamSo item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
