using QuanLyMayBay.DTO;

namespace QuanLyMayBay.DAO;

public class CTKMDAO : JsonDanhSachDAO<CTKM>
{
    public CTKMDAO(string? duongDan = null) : base("ctkm.json", duongDan) { }

    // CRUD MySQL theo mau HangHangKhongDAO cua nhom.
    internal static readonly LenhDanhMucMySql Lenh = new(typeof(CTKM),
        new[] { "MaCTKM", "TenCTKM", "NgayBatDau", "NgayKetThuc" },
        (reader, map) => new CTKM
        {
            MaCTKM = DanhMucMySql.Chuoi(reader, "MaCTKM"),
            TenCTKM = DanhMucMySql.Chuoi(reader, "TenCTKM"),
            NgayBatDau = reader.GetDateTime("NgayBatDau"),
            NgayKetThuc = reader.GetDateTime("NgayKetThuc"),
        });

    public List<CTKM> DocDS() => Doc();
    public void Them(CTKM item) => Lenh.Them(item);
    public void Sua(CTKM item) => Lenh.Sua(item);
    public void Xoa(CTKM item) => Lenh.Xoa(item);
    public void Xoa(string ma) => Lenh.Xoa(ma);
}
