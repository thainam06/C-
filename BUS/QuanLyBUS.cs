using System.Collections;
using QuanLyMayBay.DAO;
using QuanLyMayBay.DTO;

namespace QuanLyMayBay.BUS;

public class QuanLyBUS
{
    private readonly DuLieuDAO dao;
    private DuLieu duLieu;

    public QuanLyBUS(DuLieuDAO? dao = null)
    {
        this.dao = dao ?? new DuLieuDAO();
        duLieu = this.dao.Doc();
        KiemTraDuLieu(duLieu);
    }

    private static bool CungMa(string a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static string BatBuoc(string giaTri, string ten)
    {
        if (string.IsNullOrWhiteSpace(giaTri))
            throw new ArgumentException($"Vui lòng nhập {ten}.");
        return giaTri.Trim();
    }

    private static void KiemTraDuLieu(DuLieu data)
    {
        var maMayBay = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var maHangVe = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Ma ghe la duy nhat trong tung may bay (vi du nhieu may bay deu co ghe 1A).
        var maGhe = new HashSet<(string, string)>();
        foreach (var mb in data.MayBay)
        {
            if (mb == null || string.IsNullOrWhiteSpace(mb.MaMayBay)
                || string.IsNullOrWhiteSpace(mb.MHHK) || !maMayBay.Add(mb.MaMayBay))
                throw new InvalidDataException("Dữ liệu máy bay bị thiếu hoặc trùng mã.");
        }
        foreach (var hv in data.HangVe)
        {
            if (hv == null || string.IsNullOrWhiteSpace(hv.MaHV)
                || string.IsNullOrWhiteSpace(hv.TenHV) || hv.HanhLyKg < 0 || hv.DonGiaVe < 0
                || !maHangVe.Add(hv.MaHV))
                throw new InvalidDataException("Dữ liệu hạng vé không hợp lệ hoặc trùng mã.");
        }
        foreach (var ghe in data.Ghe)
        {
            if (ghe == null || string.IsNullOrWhiteSpace(ghe.MaGhe)
                || string.IsNullOrWhiteSpace(ghe.MaMayBay) || string.IsNullOrWhiteSpace(ghe.MaHV)
                || !maMayBay.Contains(ghe.MaMayBay) || !maHangVe.Contains(ghe.MaHV)
                || !maGhe.Add((ghe.MaMayBay.ToUpperInvariant(), ghe.MaGhe.ToUpperInvariant())))
                throw new InvalidDataException("Dữ liệu ghế không hợp lệ hoặc mất liên kết.");
        }
    }

    // GUI nhan ban sao, khong thay doi truc tiep du lieu cua BUS.
    private DuLieu SaoChep() => new()
    {
        MayBay = duLieu.MayBay.Select(x => new MayBay(x.MaMayBay, x.MHHK)).ToList(),
        Ghe = duLieu.Ghe.Select(x => new Ghe(x.MaGhe, x.MaMayBay, x.MaHV)).ToList(),
        HangVe = duLieu.HangVe.Select(x => new HangVe(x.MaHV, x.TenHV, x.HanhLyKg, x.DonGiaVe)).ToList()
    };

    private void Luu(DuLieu moi)
    {
        KiemTraDuLieu(moi);
        // Chi cap nhat bo nho sau khi luu thanh cong.
        dao.Luu(moi);
        duLieu = moi;
    }

    // Tim kiem tren ArrayList tai BUS theo yeu cau do an.
    private static ArrayList Tim(ArrayList nguon, string tuKhoa, Func<object, string[]> cacCot)
    {
        var ketQua = new ArrayList();
        foreach (var item in nguon)
            if (cacCot(item).Any(x => x.Contains(tuKhoa.Trim(), StringComparison.OrdinalIgnoreCase)))
                ketQua.Add(item);
        return ketQua;
    }

    public ArrayList TimMayBay(string tuKhoa = "") => Tim(new ArrayList(SaoChep().MayBay), tuKhoa,
        x => new[] { ((MayBay)x).MaMayBay, ((MayBay)x).MHHK });

    public ArrayList TimHangVe(string tuKhoa = "") => Tim(new ArrayList(SaoChep().HangVe), tuKhoa,
        x => new[] { ((HangVe)x).MaHV, ((HangVe)x).TenHV });

    public ArrayList TimGhe(string tuKhoa = "") => Tim(new ArrayList(SaoChep().Ghe), tuKhoa,
        x => new[] { ((Ghe)x).MaGhe, ((Ghe)x).MaMayBay, ((Ghe)x).MaHV });

    public ArrayList LayGheTheoMayBay(string maMayBay)
    {
        var ketQua = new ArrayList();
        var hangVeTheoMa = duLieu.HangVe.ToDictionary(x => x.MaHV, StringComparer.OrdinalIgnoreCase);
        foreach (var ghe in duLieu.Ghe.Where(x => CungMa(x.MaMayBay, maMayBay)))
        {
            var hangVe = hangVeTheoMa[ghe.MaHV];
            ketQua.Add(new ChiTietGhe
            {
                MaGhe = ghe.MaGhe,
                MaHV = hangVe.MaHV,
                TenHV = hangVe.TenHV,
                HanhLyKg = hangVe.HanhLyKg,
                DonGiaVe = hangVe.DonGiaVe
            });
        }
        return ketQua;
    }

    public int NapMayBayMau()
    {
        var moi = SaoChep();
        var cacHang = new[] { "VN", "VN", "VN", "VJ", "VJ", "VJ", "QH", "QH", "BL", "BL" };
        var soDaThem = 0;
        for (var i = 0; i < cacHang.Length; i++)
        {
            var ma = $"M{i + 1:00}";
            if (moi.MayBay.Any(x => CungMa(x.MaMayBay, ma))) continue;
            moi.MayBay.Add(new MayBay(ma, cacHang[i]));
            soDaThem++;
        }
        if (soDaThem > 0) Luu(moi);
        return soDaThem;
    }

    public void LuuMayBay(string ma, string mhhk, bool sua)
    {
        ma = BatBuoc(ma, "mã máy bay");
        mhhk = BatBuoc(mhhk, "mã hãng hàng không");
        var moi = SaoChep();
        var cu = moi.MayBay.Find(x => CungMa(x.MaMayBay, ma));
        if (sua && cu == null) throw new ArgumentException("Máy bay không còn tồn tại.");
        if (!sua && cu != null) throw new ArgumentException("Mã máy bay đã tồn tại.");
        if (cu != null) cu.MHHK = mhhk;
        else moi.MayBay.Add(new MayBay(ma, mhhk));
        Luu(moi);
    }

    public void XoaMayBay(string ma)
    {
        var moi = SaoChep();
        if (moi.Ghe.Any(x => CungMa(x.MaMayBay, ma)))
            throw new ArgumentException("Máy bay đang có ghế. Hãy xóa ghế liên quan trước.");
        if (moi.MayBay.RemoveAll(x => CungMa(x.MaMayBay, ma)) == 0)
            throw new ArgumentException("Không tìm thấy máy bay.");
        Luu(moi);
    }

    public void LuuHangVe(string ma, string ten, decimal hanhLy, decimal donGia, bool sua)
    {
        ma = BatBuoc(ma, "mã hạng vé");
        ten = BatBuoc(ten, "tên hạng vé");
        if (hanhLy < 0 || donGia < 0) throw new ArgumentException("Hành lý và đơn giá không được âm.");
        var moi = SaoChep();
        var cu = moi.HangVe.Find(x => CungMa(x.MaHV, ma));
        if (sua && cu == null) throw new ArgumentException("Hạng vé không còn tồn tại.");
        if (!sua && cu != null) throw new ArgumentException("Mã hạng vé đã tồn tại.");
        if (cu != null)
        {
            cu.TenHV = ten;
            cu.HanhLyKg = hanhLy;
            cu.DonGiaVe = donGia;
        }
        else moi.HangVe.Add(new HangVe(ma, ten, hanhLy, donGia));
        Luu(moi);
    }

    public void XoaHangVe(string ma)
    {
        var moi = SaoChep();
        if (moi.Ghe.Any(x => CungMa(x.MaHV, ma)))
            throw new ArgumentException("Hạng vé đang được ghế sử dụng. Hãy đổi hạng vé hoặc xóa ghế trước.");
        if (moi.HangVe.RemoveAll(x => CungMa(x.MaHV, ma)) == 0)
            throw new ArgumentException("Không tìm thấy hạng vé.");
        Luu(moi);
    }

    public void LuuGhe(string ma, string mayBay, string hangVe, bool sua)
    {
        ma = BatBuoc(ma, "mã ghế");
        mayBay = BatBuoc(mayBay, "máy bay");
        hangVe = BatBuoc(hangVe, "hạng vé");
        var moi = SaoChep();
        var mb = moi.MayBay.Find(x => CungMa(x.MaMayBay, mayBay));
        var hv = moi.HangVe.Find(x => CungMa(x.MaHV, hangVe));
        if (mb == null || hv == null) throw new ArgumentException("Máy bay hoặc hạng vé không tồn tại.");
        var cu = moi.Ghe.Find(x => CungMa(x.MaGhe, ma) && CungMa(x.MaMayBay, mayBay));
        if (sua && cu == null) throw new ArgumentException("Ghế không còn tồn tại.");
        if (!sua && cu != null) throw new ArgumentException("Mã ghế đã tồn tại trên máy bay này.");
        if (cu != null) cu.MaHV = hv.MaHV;
        else moi.Ghe.Add(new Ghe(ma, mb.MaMayBay, hv.MaHV));
        Luu(moi);
    }

    public void XoaGhe(string ma, string mayBay)
    {
        var moi = SaoChep();
        if (moi.Ghe.RemoveAll(x => CungMa(x.MaGhe, ma) && CungMa(x.MaMayBay, mayBay)) == 0)
            throw new ArgumentException("Không tìm thấy ghế.");
        Luu(moi);
    }
}
