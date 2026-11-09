using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class GheDAO
{
    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(Ghe),
        new[] { "MaGhe", "MaMayBay", "MaHV", "LoaiGhe" },
        (reader, map) => new Ghe
        {
            MaGhe = DanhMucMySql.Chuoi(reader, "MaGhe"),
            MaMayBay = DanhMucMySql.Chuoi(reader, "MaMayBay"),
            MaHV = DanhMucMySql.Chuoi(reader, "MaHV"),
            LoaiGhe = map.Columns.ContainsKey("LoaiGhe") ? DanhMucMySql.Chuoi(reader, "LoaiGhe") : Ghe.Thuong,
        });

    public List<Ghe> DocDS() => Lenh.DocDS<Ghe>();
    public void Them(Ghe item) => Lenh.Them(item);
    public void Sua(Ghe item) => Lenh.Sua(item);
    public void Xoa(Ghe item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
