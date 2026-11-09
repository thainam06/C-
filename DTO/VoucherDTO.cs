namespace VeMayBay.DTO;

public class VoucherDTO
{
    public string MaVoucher { get; set; } = "";
    public string MaCTKM { get; set; } = "";
    public double GiamGia { get; set; }

    public VoucherDTO() { }

    public VoucherDTO(string maVoucher, string maCTKM, double giamGia)
    {
        MaVoucher = maVoucher;
        MaCTKM = maCTKM;
        GiamGia = giamGia;
    }
}
