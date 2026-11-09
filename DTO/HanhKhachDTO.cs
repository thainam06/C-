namespace VeMayBay.DTO;

public class HanhKhachDTO
{
    public string MaHK { get; set; } = "";
    public string HoTen { get; set; } = "";
    public DateTime NgaySinh { get; set; }
    public string CCCD { get; set; } = "";
    public string GioiTinh { get; set; } = "";
    public string MaKH { get; set; } = "";
    public string LoaiKH { get; set; } = "";

    public HanhKhachDTO() { }

    public HanhKhachDTO(string maHK, string hoTen, DateTime ngaySinh, string cccd, string gioiTinh, string maKH, string loaiKH)
    {
        MaHK = maHK;
        HoTen = hoTen;
        NgaySinh = ngaySinh;
        CCCD = cccd;
        GioiTinh = gioiTinh;
        MaKH = maKH;
        LoaiKH = loaiKH;
    }
}
