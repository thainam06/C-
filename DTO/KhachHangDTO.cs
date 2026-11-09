namespace VeMayBay.DTO;

public class KhachHangDTO
{
    public string MaKH { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string CCCD { get; set; } = "";
    public string SDT { get; set; } = "";
    public string Email { get; set; } = "";
    public string TaiKhoan { get; set; } = "";
    public string MatKhau { get; set; } = "";
    public string DiaChi { get; set; } = "";

    public KhachHangDTO() { }

    public KhachHangDTO(string maKH, string hoTen, string cccd, string sdt, string email, string taiKhoan, string matKhau, string diaChi)
    {
        MaKH = maKH;
        HoTen = hoTen;
        CCCD = cccd;
        SDT = sdt;
        Email = email;
        TaiKhoan = taiKhoan;
        MatKhau = matKhau;
        DiaChi = diaChi;
    }
}
