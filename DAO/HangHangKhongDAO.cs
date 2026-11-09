using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class HangHangKhongDAO
{
    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(HangHangKhong),
        new[] { "MaHHK", "TenHang" },
        (reader, map) => new HangHangKhong
        {
            MaHHK = DanhMucMySql.Chuoi(reader, "MaHHK"),
            TenHang = DanhMucMySql.Chuoi(reader, "TenHang"),
        });

    public List<HangHangKhong> DocDS() => Lenh.DocDS<HangHangKhong>();
    public void Them(HangHangKhong item) => Lenh.Them(item);
    public void Sua(HangHangKhong item) => Lenh.Sua(item);
    public void Xoa(HangHangKhong item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
