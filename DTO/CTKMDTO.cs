namespace VeMayBay.DTO;

public class CTKMDTO
{
    public string MaCTKM { get; set; } = "";
    public string TenCTKM { get; set; } = "";
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }

    public CTKMDTO() { }

    public CTKMDTO(string maCTKM, string tenCTKM, DateTime ngayBatDau, DateTime ngayKetThuc)
    {
        MaCTKM = maCTKM;
        TenCTKM = tenCTKM;
        NgayBatDau = ngayBatDau;
        NgayKetThuc = ngayKetThuc;
    }
}
