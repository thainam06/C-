namespace QuanLyMayBay.DTO
{
    /// <summary>
    /// Du lieu ghe tren may bay, trao doi giua cac lop GUI, BUS va DAO.
    /// </summary>
    public class Ghe
    {
        public const string Thuong = "Ghế thường";
        public const string Vip = "Ghế VIP";

        public string MaGhe { get; set; }

        public string MaMayBay { get; set; }

        public string MaHV { get; set; }

        public string LoaiGhe { get; set; } = Thuong;

        public Ghe()
        {
            MaGhe = string.Empty;
            MaMayBay = string.Empty;
            MaHV = string.Empty;
        }

        public Ghe(string maGhe, string maMayBay, string maHV, string loaiGhe = Thuong)
        {
            MaGhe = maGhe;
            MaMayBay = maMayBay;
            MaHV = maHV;
            LoaiGhe = loaiGhe;
        }
    }
}
