namespace VeMayBay.DTO
{
    /// <summary>
    /// Du lieu ghe tren may bay, trao doi giua cac lop GUI, BUS va DAO.
    /// </summary>
    public class GheDTO
    {
        public const string Thuong = "Ghế thường";
        public const string Vip = "Ghế VIP";

        public string MaGhe { get; set; }

        public string MaMayBay { get; set; }

        public string MaHV { get; set; }

        public string LoaiGhe { get; set; } = Thuong;

        public GheDTO()
        {
            MaGhe = string.Empty;
            MaMayBay = string.Empty;
            MaHV = string.Empty;
        }

        public GheDTO(string maGhe, string maMayBay, string maHV, string loaiGhe = Thuong)
        {
            MaGhe = maGhe;
            MaMayBay = maMayBay;
            MaHV = maHV;
            LoaiGhe = loaiGhe;
        }
    }
}
