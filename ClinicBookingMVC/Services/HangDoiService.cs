using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Data;
using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Services;

/// <summary>
/// Dá»‹ch vá»¥ hĂ ng Ä‘á»£i 3 má»©c Æ°u tiĂªn:
///   MucUuTien=1 â€” DatTruoc    mĂ£ A-xxx  (Ä‘Ăºng giá» / sá»›m / trá»… â‰¤15 phĂºt)
///   MucUuTien=2 â€” VangLai     mĂ£ W-xxx  (walk-in)
///   MucUuTien=3 â€” DatTruocTre mĂ£ L-xxx  (trá»… 16â€“30 phĂºt hoáº·c quay láº¡i sau QuaLuot)
/// </summary>
public class HangDoiService
{
    private readonly ClinicDbContext _db;

    public HangDoiService(ClinicDbContext db) { _db = db; }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  CHECK-IN: KhĂ¡ch Ä‘áº·t trÆ°á»›c
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// Thá»±c hiá»‡n check-in cho bá»‡nh nhĂ¢n Ä‘Ă£ cĂ³ PhieuDatHen (DaXacNhan / ChoXacNhan).
    /// Tráº£ vá» (PhieuSoThuTu, errorMessage).
    /// </summary>
    public async Task<(PhieuSoThuTu? Phieu, string? Error)> CheckInDatTruocAsync(
        int maPhieuHen, int maPhong)
    {
        var phieuHen = await _db.PhieuDatHens
            .Include(p => p.BenhNhan)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
            .FirstOrDefaultAsync(p => p.MaPhieuHen == maPhieuHen);

        if (phieuHen == null)
            return (null, "KhĂ´ng tĂ¬m tháº¥y phiáº¿u háº¹n.");

        if (phieuHen.TrangThaiHen is "DaHuy" or "DaCheckIn" or "DaKham" or "VangHen")
            return (null, $"Phieu hen dang o trang thai [{phieuHen.TrangThaiHen}], khong the check-in.");

        // TĂ­nh Ä‘á»™ trá»… so vá»›i giá» háº¹n
        var gioHen   = phieuHen.KhungGio.GioBatDau;
        var thoiDiemHen = phieuHen.KhungGio.CaTruc.NgayTruc.ToDateTime(gioHen);
        var thoiGianTre = (DateTime.Now - thoiDiemHen).TotalMinutes; // dÆ°Æ¡ng = trá»…, Ă¢m = sá»›m

        if (thoiGianTre > 30)
        {
            // Trá»… quĂ¡ 30 phĂºt â†’ quĂ¡ háº¡n
            return (null,
                $"Bá»‡nh nhĂ¢n Ä‘áº¿n trá»… hÆ¡n 30 phĂºt (giá» háº¹n {gioHen:HH\\:mm}). " +
                "Phiáº¿u Ä‘Ă£ quĂ¡ háº¡n váº¯ng háº¹n. Vui lĂ²ng tiáº¿p nháº­n theo diá»‡n khĂ¡ch vĂ£ng lai.");
        }

        // PhĂ¢n loáº¡i Æ°u tiĂªn
        string loaiKhach;
        int    mucUuTien;
        string prefix;

        if (thoiGianTre <= 15)
        {
            loaiKhach  = "DatTruoc";
            mucUuTien  = 1;
            prefix     = "A";
        }
        else // 16â€“30 phĂºt
        {
            loaiKhach  = "DatTruocTre";
            mucUuTien  = 3;
            prefix     = "L";
        }

        var today      = DateOnly.FromDateTime(DateTime.Today);
        int soThuTuMoi = await DemSoThuTuTiepTheoAsync(maPhong, today, prefix);

        var phieu = new PhieuSoThuTu
        {
            SoThuTu         = soThuTuMoi,
            NgayCapSo       = today,
            LoaiKhach       = loaiKhach,
            MucUuTien       = mucUuTien,
            GioHen          = gioHen,
            ThoiGianCheckIn = DateTime.Now,
            TrangThaiPhieu  = "DangCho",
            MaPhong         = maPhong,
            MaBenhNhan      = phieuHen.MaBenhNhan,
            MaPhieuHen      = maPhieuHen,
        };

        phieuHen.TrangThaiHen = "DaCheckIn";
        _db.PhieuSoThuTus.Add(phieu);
        await _db.SaveChangesAsync();

        return (phieu, null);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  CHECK-IN: KhĂ¡ch vĂ£ng lai (Walk-in)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public async Task<(PhieuSoThuTu Phieu, bool IsNew)> CheckInVangLaiAsync(
        string soDienThoai, int maPhong)
    {
        // TĂ¬m hoáº·c táº¡o bá»‡nh nhĂ¢n
        var bn = await _db.BenhNhans
            .FirstOrDefaultAsync(b => b.SoDienThoai == soDienThoai.Trim());

        bool isNew = false;
        if (bn == null)
        {
            bn = new BenhNhan
            {
                TenBenhNhan  = "KhĂ¡ch vĂ£ng lai",
                SoDienThoai  = soDienThoai.Trim(),
                SoLanVangHen = 0,
            };
            _db.BenhNhans.Add(bn);
            await _db.SaveChangesAsync();
            isNew = true;
        }

        var today      = DateOnly.FromDateTime(DateTime.Today);
        int soThuTuMoi = await DemSoThuTuTiepTheoAsync(maPhong, today, "W");

        var phieu = new PhieuSoThuTu
        {
            SoThuTu         = soThuTuMoi,
            NgayCapSo       = today,
            LoaiKhach       = "VangLai",
            MucUuTien       = 2,
            GioHen          = null,
            ThoiGianCheckIn = DateTime.Now,
            TrangThaiPhieu  = "DangCho",
            MaPhong         = maPhong,
            MaBenhNhan      = bn.MaBenhNhan,
            MaPhieuHen      = null,
        };

        _db.PhieuSoThuTus.Add(phieu);
        await _db.SaveChangesAsync();

        return (phieu, isNew);
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  THUáº¬T TOĂN Gá»ŒI Sá» TIáº¾P THEO (cá»­a sá»• kĂ­ch hoáº¡t giá» háº¹n)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// Chá»n Ä‘Ăºng ngÆ°á»i tiáº¿p theo Ä‘á»ƒ gá»i dá»±a trĂªn 4 nhĂ³m Æ°u tiĂªn.
    /// Cáº­p nháº­t DB vĂ  tráº£ vá» DTO káº¿t quáº£.
    /// </summary>
    public async Task<GoiSoResultDto?> GoiSoTiepTheoAsync(int maPhong)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now   = TimeOnly.FromDateTime(DateTime.Now);

        // Láº¥y toĂ n bá»™ phiáº¿u DangCho cá»§a phĂ²ng hĂ´m nay
        var dangCho = await _db.PhieuSoThuTus
            .Include(p => p.BenhNhan)
            .Include(p => p.PhongKham)
            .Where(p => p.MaPhong == maPhong
                     && p.NgayCapSo == today
                     && p.TrangThaiPhieu == "DangCho")
            .ToListAsync();

        if (!dangCho.Any()) return null;

        var chuaGoiLan = now.AddMinutes(15); // cá»­a sá»• kĂ­ch hoáº¡t: GioHen â‰¤ Now + 15 phĂºt

        // NhĂ³m 1: DatTruoc Ä‘Ă£ vĂ o cá»­a sá»• kĂ­ch hoáº¡t
        var nhom1 = dangCho
            .Where(p => p.MucUuTien == 1 && p.GioHen.HasValue && p.GioHen.Value <= chuaGoiLan)
            .OrderBy(p => p.GioHen)
            .ThenBy(p => p.ThoiGianCheckIn)
            .ToList();

        // NhĂ³m 2: VĂ£ng lai
        var nhom2 = dangCho
            .Where(p => p.MucUuTien == 2)
            .OrderBy(p => p.ThoiGianCheckIn)
            .ToList();

        // NhĂ³m 3: DatTruocTre / QuaLuot quay láº¡i
        var nhom3 = dangCho
            .Where(p => p.MucUuTien == 3)
            .OrderBy(p => p.ThoiGianCheckIn)
            .ToList();

        // NhĂ³m 4 (dá»± phĂ²ng): DatTruoc chÆ°a Ä‘áº¿n cá»­a sá»•
        var nhom4 = dangCho
            .Where(p => p.MucUuTien == 1 && (!p.GioHen.HasValue || p.GioHen.Value > chuaGoiLan))
            .OrderBy(p => p.GioHen)
            .ThenBy(p => p.ThoiGianCheckIn)
            .ToList();

        // Chá»n ngÆ°á»i Ä‘á»©ng Ä‘áº§u
        PhieuSoThuTu? chon = nhom1.FirstOrDefault()
                          ?? nhom2.FirstOrDefault()
                          ?? nhom3.FirstOrDefault()
                          ?? nhom4.FirstOrDefault();

        if (chon == null) return null;

        // Cáº­p nháº­t tráº¡ng thĂ¡i
        chon.TrangThaiPhieu = "DangGoi";
        chon.SoLanGoi       = 1;
        chon.ThoiGianGoiSo  = DateTime.Now;
        await _db.SaveChangesAsync();

        return new GoiSoResultDto
        {
            MaPhieu      = chon.MaPhieu,
            SoThuTu      = chon.SoThuTu,
            LoaiKhach    = chon.LoaiKhach,
            Prefix       = LoaiKhachToPrefix(chon.LoaiKhach),
            TenBenhNhan  = chon.BenhNhan?.TenBenhNhan ?? "",
            TenPhong     = chon.PhongKham?.TenPhong ?? "",
            SoLanGoi     = chon.SoLanGoi,
            TrangThaiPhieu = chon.TrangThaiPhieu,
        };
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  Gá»ŒI Láº I (Recall)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public async Task<GoiSoResultDto?> GoiLaiAsync(int maPhieu)
    {
        var phieu = await _db.PhieuSoThuTus
            .Include(p => p.BenhNhan)
            .Include(p => p.PhongKham)
            .Include(p => p.PhieuDatHen)
                .ThenInclude(ph => ph!.BenhNhan)
            .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

        if (phieu == null || phieu.TrangThaiPhieu != "DangGoi")
            return null;

        if (phieu.SoLanGoi >= 3)
        {
            // ÄĂ£ gá»i Ä‘á»§ 3 láº§n â€” khĂ´ng gá»i thĂªm, tráº£ null bĂ¡o caller nĂªn hiá»‡n QuaLuot
            return null;
        }

        phieu.SoLanGoi++;
        phieu.ThoiGianGoiSo = DateTime.Now;
        await _db.SaveChangesAsync();

        return new GoiSoResultDto
        {
            MaPhieu        = phieu.MaPhieu,
            SoThuTu        = phieu.SoThuTu,
            LoaiKhach      = phieu.LoaiKhach,
            Prefix         = LoaiKhachToPrefix(phieu.LoaiKhach),
            TenBenhNhan    = phieu.BenhNhan?.TenBenhNhan ?? "",
            TenPhong       = phieu.PhongKham?.TenPhong ?? "",
            SoLanGoi       = phieu.SoLanGoi,
            TrangThaiPhieu = phieu.TrangThaiPhieu,
        };
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  QUA LÆ¯á»¢T
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public async Task<string?> QuaLuotAsync(int maPhieu)
    {
        var phieu = await _db.PhieuSoThuTus
            .Include(p => p.PhieuDatHen)
                .ThenInclude(ph => ph!.BenhNhan)
            .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

        if (phieu == null) return "Phiáº¿u khĂ´ng tá»“n táº¡i.";

        phieu.TrangThaiPhieu = "QuaLuot";

        // Cáº­p nháº­t PhieuDatHen náº¿u cĂ³
        if (phieu.PhieuDatHen != null)
        {
            phieu.PhieuDatHen.TrangThaiHen = "VangHen";
            phieu.PhieuDatHen.BenhNhan.SoLanVangHen++;
        }

        await _db.SaveChangesAsync();
        return null;
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  KHĂ”I PHá»¤C VĂ€O HĂ€NG CHá»œ (sau QuaLuot)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public async Task<string?> KhoiPhucHangChoAsync(int maPhieu)
    {
        var phieu = await _db.PhieuSoThuTus
            .Include(p => p.PhieuDatHen)
            .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

        if (phieu == null) return "Phiáº¿u khĂ´ng tá»“n táº¡i.";
        if (phieu.TrangThaiPhieu != "QuaLuot")
            return "Chá»‰ khĂ´i phá»¥c Ä‘Æ°á»£c phiáº¿u á»Ÿ tráº¡ng thĂ¡i QuĂ¡ lÆ°á»£t.";

        // Xuá»‘ng cuá»‘i hĂ ng Ä‘á»£i vá»›i Æ°u tiĂªn tháº¥p nháº¥t
        phieu.TrangThaiPhieu = "DangCho";
        phieu.MucUuTien      = 3;
        phieu.SoLanGoi       = 0;

        // Náº¿u phiáº¿u háº¹n Ä‘Ă£ VangHen â†’ tráº£ vá» DaXacNhan Ä‘á»ƒ trĂ¡nh khĂ³a phiáº¿u
        if (phieu.PhieuDatHen?.TrangThaiHen == "VangHen")
            phieu.PhieuDatHen.TrangThaiHen = "DaCheckIn";

        await _db.SaveChangesAsync();
        return null;
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  THá»NG KĂ: Sá»‘ ngÆ°á»i Ä‘ang chá» theo phĂ²ng (cho LeTan chá»n phĂ²ng)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    public async Task<List<PhongChoVM>> LayThongKePhongChoAsync(int maChuyenKhoa = 0)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = _db.PhongKhams
            .Where(p => p.TrangThai == "HoatDong");

        if (maChuyenKhoa > 0)
            query = query.Where(p => p.MaChuyenKhoa == maChuyenKhoa);

        var phongs = await query
            .Include(p => p.ChuyenKhoa)
            .OrderBy(p => p.TenPhong)
            .ToListAsync();

        var result = new List<PhongChoVM>();

        foreach (var p in phongs)
        {
            int soCho = await _db.PhieuSoThuTus.CountAsync(x =>
                x.MaPhong == p.MaPhong &&
                x.NgayCapSo == today &&
                x.TrangThaiPhieu == "DangCho");

            int soDangGoi = await _db.PhieuSoThuTus.CountAsync(x =>
                x.MaPhong == p.MaPhong &&
                x.NgayCapSo == today &&
                x.TrangThaiPhieu == "DangGoi");

            result.Add(new PhongChoVM
            {
                MaPhong      = p.MaPhong,
                TenPhong     = p.TenPhong,
                TenChuyenKhoa = p.ChuyenKhoa?.TenChuyenKhoa ?? "",
                SoDangCho    = soCho,
                SoDangGoi    = soDangGoi,
            });
        }

        return result;
    }

    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    //  HELPERS
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    /// <summary>
    /// Äáº¿m sá»‘ phiáº¿u hĂ´m nay cá»§a phĂ²ng theo prefix loáº¡i khĂ¡ch Ä‘á»ƒ sinh STT tiáº¿p theo.
    /// A = DatTruoc (MucUuTien=1 Ä‘Ăºng giá»)
    /// W = VangLai  (MucUuTien=2)
    /// L = DatTruocTre (MucUuTien=3)
    /// </summary>
    private async Task<int> DemSoThuTuTiepTheoAsync(int maPhong, DateOnly ngay, string prefix)
    {
        string loai = prefix switch
        {
            "A" => "DatTruoc",
            "W" => "VangLai",
            "L" => "DatTruocTre",
            _   => prefix,
        };

        return await _db.PhieuSoThuTus.CountAsync(p =>
            p.MaPhong == maPhong &&
            p.NgayCapSo == ngay &&
            p.LoaiKhach == loai) + 1;
    }

    public static string LoaiKhachToPrefix(string loaiKhach) => loaiKhach switch
    {
        "DatTruoc"    => "A",
        "VangLai"     => "W",
        "DatTruocTre" => "L",
        "ThongThuong" => "A",   // compat cÅ©
        _             => "A",
    };

    public static string FormatSoThuTu(PhieuSoThuTu p)
        => $"{LoaiKhachToPrefix(p.LoaiKhach)}-{p.SoThuTu:D3}";
}

// â”€â”€ DTOs â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

public class GoiSoResultDto
{
    public int    MaPhieu        { get; set; }
    public int    SoThuTu        { get; set; }
    public string LoaiKhach      { get; set; } = "";
    public string Prefix         { get; set; } = "A";
    public string TenBenhNhan    { get; set; } = "";
    public string TenPhong       { get; set; } = "";
    public int    SoLanGoi       { get; set; }
    public string TrangThaiPhieu { get; set; } = "";
    // Computed
    public string MaSoThuTu => $"{Prefix}-{SoThuTu:D3}";
}

public class PhongChoVM
{
    public int    MaPhong       { get; set; }
    public string TenPhong      { get; set; } = "";
    public string TenChuyenKhoa { get; set; } = "";
    public int    SoDangCho     { get; set; }
    public int    SoDangGoi     { get; set; }
}
