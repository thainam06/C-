namespace QuanLyMayBay.DTO
{
    /// <summary>
    /// Du lieu may bay, trao doi giua cac lop GUI, BUS va DAO.
    /// </summary>
    public class MayBay
    {
        /// <summary>Ma may bay; kiem tra tinh duy nhat tai BUS va CSDL.</summary>
        public string MaMayBay { get; set; }

        /// <summary>Ma hang hang khong so huu may bay.</summary>
        public string MHHK { get; set; }

        public MayBay()
        {
            MaMayBay = string.Empty;
            MHHK = string.Empty;
        }

        public MayBay(string maMayBay, string mhhk)
        {
            MaMayBay = maMayBay;
            MHHK = mhhk;
        }
    }
}
