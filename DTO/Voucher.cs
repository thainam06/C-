namespace QuanLyMayBay.DTO;

public class Voucher
{
    public string MaVoucher { get; set; } = string.Empty;
    public string MaCTKM { get; set; } = string.Empty;
    /// <summary>Phan tram giam gia, lon hon 0 va khong qua 100.</summary>
    public decimal GiamGia { get; set; }

    public Voucher() { }

    public Voucher(string maVoucher, string maCTKM, decimal giamGia)
    {
        MaVoucher = maVoucher;
        MaCTKM = maCTKM;
        GiamGia = giamGia;
    }
}
