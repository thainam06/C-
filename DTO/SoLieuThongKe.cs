namespace QuanLyMayBay.DTO;

// Dau vao chuan hoa tu hoa don/CTHD cua CSDL nhom hoac file Excel.
public class DongDoanhThu
{
    public string MaHD { get; set; } = "";
    public int STT { get; set; }
    public DateTime NgayLap { get; set; }
    public string MaKH { get; set; } = "";
    public string MaNV { get; set; } = "";
    public string MaSanPham { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal GiamGia { get; set; }
}
public class DongChiPhi
{
    public string MaPC { get; set; } = "";
    public DateTime NgayChi { get; set; }
    public string MaNV { get; set; } = "";
    public string NoiDung { get; set; } = "";
    public decimal SoTien { get; set; }
}
public class SoLieuThongKe
{
    public List<DongDoanhThu> DoanhThu { get; set; } = new();
    public List<DongChiPhi> ChiPhi { get; set; } = new();
}
public class BangQuy
{
    public string Nhom { get; set; } = "";
    public decimal Q1 { get; set; }
    public decimal Q2 { get; set; }
    public decimal Q3 { get; set; }
    public decimal Q4 { get; set; }
    public decimal Tong { get; set; }
}
public class DongThongKe
{
    public string Nhom { get; set; } = "";
    public string MaThuHai { get; set; } = "";
    public int Nam { get; set; }
    public string Ky { get; set; } = "";
    public decimal Tong { get; set; }
}
