namespace QuanLyMayBay.DTO;

public class CTKM
{
    public string MaCTKM { get; set; } = string.Empty;
    public string TenCTKM { get; set; } = string.Empty;
    public DateTime NgayBatDau { get; set; } = DateTime.Today;
    public DateTime NgayKetThuc { get; set; } = DateTime.Today;

    public CTKM() { }

    public CTKM(string maCTKM, string tenCTKM, DateTime ngayBatDau, DateTime ngayKetThuc)
    {
        MaCTKM = maCTKM;
        TenCTKM = tenCTKM;
        NgayBatDau = ngayBatDau;
        NgayKetThuc = ngayKetThuc;
    }
}
