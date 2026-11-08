namespace QuanLyMayBay.DTO;

public class NhanVien
{
    public string MaNV { get; set; } = string.Empty;
    public string Ho { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string ChucVu { get; set; } = string.Empty;

    public NhanVien() { }

    public NhanVien(string maNV, string ho, string ten, string chucVu)
    {
        MaNV = maNV;
        Ho = ho;
        Ten = ten;
        ChucVu = chucVu;
    }
}
