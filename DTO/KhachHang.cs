namespace QuanLyMayBay.DTO
{
    /// <summary>
    /// Du lieu khach hang, trao doi giua cac lop GUI, BUS va DAO.
    /// </summary>
    public class KhachHang
    {
        public string MaKH { get; set; }

        public string HoTen { get; set; }

        public string CCCD { get; set; }

        public string SDT { get; set; }

        public string Email { get; set; }

        public string TaiKhoan { get; set; }

        public string MatKhau { get; set; }

        public string DiaChi { get; set; }

        public KhachHang()
        {
            MaKH = string.Empty;
            HoTen = string.Empty;
            CCCD = string.Empty;
            SDT = string.Empty;
            Email = string.Empty;
            TaiKhoan = string.Empty;
            MatKhau = string.Empty;
            DiaChi = string.Empty;
        }

        public KhachHang(string maKH, string hoTen, string cccd, string sdt,
            string email, string taiKhoan, string matKhau, string diaChi)
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
}
