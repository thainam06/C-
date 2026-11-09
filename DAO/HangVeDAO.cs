using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class HangVeDAO
{
    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(HangVe),
        new[] { "MaHV", "TenHV", "HanhLyKg", "DonGiaVe" },
        (reader, map) => new HangVe
        {
            MaHV = DanhMucMySql.Chuoi(reader, "MaHV"),
            TenHV = DanhMucMySql.Chuoi(reader, "TenHV"),
            HanhLyKg = DanhMucMySql.SoThapPhan(reader, "HanhLyKg"),
            DonGiaVe = DanhMucMySql.SoThapPhan(reader, "DonGiaVe"),
        });

    public List<HangVe> DocDS() => Lenh.DocDS<HangVe>();
    public void Them(HangVe item) => Lenh.Them(item);
    public void Sua(HangVe item) => Lenh.Sua(item);
    public void Xoa(HangVe item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
