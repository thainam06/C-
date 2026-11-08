namespace QuanLyMayBay.DTO
{
    public class HanhKhach
    {
        public string MaHK { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; } = DateTime.Today;
        public string CCCD { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = string.Empty;
        public string MaKH { get; set; } = string.Empty;
        public string LoaiKH { get; set; } = string.Empty;

        public HanhKhach() { }

        public HanhKhach(string maHK, string hoTen, DateTime ngaySinh, string cccd,
            string gioiTinh, string maKH, string loaiKH)
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
}
