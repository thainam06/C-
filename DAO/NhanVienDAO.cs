using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class NhanVienDAO : JsonDanhSachDAO<NhanVien>
{
    public NhanVienDAO(string? duongDan = null) : base("nhanvien.json", duongDan) { }

    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(NhanVien),
        new[] { "MaNV", "Ho", "Ten", "ChucVu" },
        (reader, map) => new NhanVien
        {
            MaNV = DanhMucMySql.Chuoi(reader, "MaNV"),
            Ho = DanhMucMySql.Chuoi(reader, "Ho"),
            Ten = DanhMucMySql.Chuoi(reader, "Ten"),
            ChucVu = DanhMucMySql.Chuoi(reader, "ChucVu"),
        });

    public List<NhanVien> DocDS() => Doc();
    public void Them(NhanVien item) => Lenh.Them(item);
    public void Sua(NhanVien item) => Lenh.Sua(item);
    public void Xoa(NhanVien item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
