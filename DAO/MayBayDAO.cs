using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class MayBayDAO
{
    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(MayBay),
        new[] { "MaMayBay", "MHHK" },
        (reader, map) => new MayBay
        {
            MaMayBay = DanhMucMySql.Chuoi(reader, "MaMayBay"),
            MHHK = DanhMucMySql.Chuoi(reader, "MHHK"),
        });

    public List<MayBay> DocDS() => Lenh.DocDS<MayBay>();
    public void Them(MayBay item) => Lenh.Them(item);
    public void Sua(MayBay item) => Lenh.Sua(item);
    public void Xoa(MayBay item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
