namespace VeMayBay.DTO
{
    /// <summary>
    /// Du lieu hang ve, trao doi giua cac lop GUI, BUS va DAO.
    /// </summary>
    public class HangVeDTO
    {
        public string MaHV { get; set; }

        public string TenHV { get; set; }

        /// <summary>Khoi luong hanh ly cho phep, tinh bang kg.</summary>
        public decimal HanhLyKg { get; set; }

        public decimal DonGiaVe { get; set; }

        public HangVeDTO()
        {
            MaHV = string.Empty;
            TenHV = string.Empty;
            HanhLyKg = 0m;
            DonGiaVe = 0m;
        }

        public HangVeDTO(string maHV, string tenHV, decimal hanhLyKg, decimal donGiaVe)
        {
            MaHV = maHV;
            TenHV = tenHV;
            HanhLyKg = hanhLyKg;
            DonGiaVe = donGiaVe;
        }
    }
}
