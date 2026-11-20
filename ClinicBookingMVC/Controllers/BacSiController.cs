using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Data;
using ClinicBookingMVC.Models.ViewModels;
using ClinicBookingMVC.Services;

namespace ClinicBookingMVC.Controllers;

[Authorize(Roles = "BacSi")]
public class BacSiController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly HangDoiService  _hangDoi;

    public BacSiController(ClinicDbContext db, HangDoiService hangDoi)
    {
        _db      = db;
        _hangDoi = hangDoi;
    }

    private static string AnSdt(string sdt) =>
        sdt.Length < 7 ? sdt : sdt[..3] + "***" + sdt[^3..];

    // ══════════════════════════════════════════════════════════════
    //  DASHBOARD — Ca trực hôm nay
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Index()
    {
        ViewData["Title"]      = "Phòng khám của tôi";
        ViewData["ActiveMenu"] = "BacSi";

        int maTaiKhoan = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bacSi = await _db.BacSis
            .FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);

        if (bacSi == null) return Forbid();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Lấy ca trực hôm nay
        var caTrucs = await _db.CaTrucs
            .Include(c => c.PhongKham)
            .Include(c => c.KhungGios)
            .Where(c => c.MaBacSi == bacSi.MaBacSi && c.NgayTruc == today)
            .OrderBy(c => c.GioBatDauCa)
            .ToListAsync();

        var maCaList = caTrucs.Select(c => c.MaCa).ToList();
        var maPhongList = caTrucs.Select(c => c.MaPhong).Distinct().ToList();

        // Phiếu đặt hẹn hôm nay trong phòng bác sĩ
        var phieus = await _db.PhieuDatHens
            .Include(p => p.BenhNhan)
            .Include(p => p.KhungGio)
            .Include(p => p.PhieuSoThuTu)
            .Where(p => maCaList.Contains(p.KhungGio.MaCa)
                     && p.TrangThaiHen != "DaHuy"
                     && p.TrangThaiHen != "VangHen")
            .ToListAsync();

        var caTrucVMs = new List<BacSiCaTrucVM>();
        foreach (var ca in caTrucs)
        {
            var phieuTrongCa = phieus
                .Where(p => p.KhungGio.MaCa == ca.MaCa)
                .OrderBy(p => p.KhungGio.GioBatDau)
                .ToList();

            caTrucVMs.Add(new BacSiCaTrucVM
            {
                MaCa         = ca.MaCa,
                CaLamViec    = ca.CaLamViec,
                GioBatDauCa  = ca.GioBatDauCa,
                GioKetThucCa = ca.GioKetThucCa,
                TenPhong     = ca.PhongKham.TenPhong,
                TrangThaiCa  = ca.TrangThaiCa,
                BenhNhans    = phieuTrongCa.Select(p => new BenhNhanTrongCaVM
                {
                    MaPhieuHen        = p.MaPhieuHen,
                    MaDatHen          = p.MaDatHen,
                    TenBenhNhan       = p.BenhNhan.TenBenhNhan,
                    SoDienThoaiAnPhan = AnSdt(p.BenhNhan.SoDienThoai),
                    NamSinh           = p.BenhNhan.NamSinh,
                    GioiTinh          = p.BenhNhan.GioiTinh,
                    CoCanhBao         = p.BenhNhan.CoCanhBao,
                    LyDoKhamSoBo      = p.LyDoKhamSoBo,
                    GioBatDau         = p.KhungGio.GioBatDau,
                    TrangThaiHen      = p.TrangThaiHen,
                    SoThuTu           = p.PhieuSoThuTu?.SoThuTu,
                    TrangThaiPhieu    = p.PhieuSoThuTu?.TrangThaiPhieu,
                }).ToList(),
            });
        }

        // Hàng đợi thực tế (PhieuSoThuTu) của phòng bác sĩ hôm nay
        var hangDoiPhong = new Dictionary<int, List<HangDoiPhieuVM>>();
        foreach (var maPhong in maPhongList)
        {
            var ds = await _db.PhieuSoThuTus
                .Include(p => p.BenhNhan)
                .Where(p => p.MaPhong == maPhong
                         && p.NgayCapSo == today
                         && p.TrangThaiPhieu != "HoanTat"
                         && p.TrangThaiPhieu != "QuaLuot")
                .OrderBy(p => p.MucUuTien)
                .ThenBy(p => p.GioHen == null)
                .ThenBy(p => p.GioHen)
                .ThenBy(p => p.ThoiGianCheckIn)
                .ToListAsync();

            hangDoiPhong[maPhong] = ds.Select(p => new HangDoiPhieuVM
            {
                MaPhieu        = p.MaPhieu,
                SoThuTu        = HangDoiService.FormatSoThuTu(p),
                TenBenhNhan    = p.BenhNhan?.TenBenhNhan ?? "",
                TrangThaiPhieu = p.TrangThaiPhieu,
                GioHen         = p.GioHen?.ToString(@"HH\:mm"),
                SoLanGoi       = p.SoLanGoi,
                MucUuTien      = p.MucUuTien,
                LoaiKhach      = p.LoaiKhach,
            }).ToList();
        }

        ViewBag.HangDoiPhong    = hangDoiPhong;
        ViewBag.MaPhongList     = maPhongList;
        ViewBag.TenPhongMap     = caTrucs.ToDictionary(c => c.MaPhong, c => c.PhongKham.TenPhong);

        var vm = new BacSiDashboardVM
        {
            HoTenBacSi    = bacSi.HoTenBacSi,
            HocVi         = bacSi.HocVi,
            NgayHomNay    = today,
            CaTrucsHomNay = caTrucVMs,
            TongBenhNhan  = caTrucVMs.Sum(c => c.BenhNhans.Count),
            SoChoXacNhan  = caTrucVMs.Sum(c => c.BenhNhans.Count(b => b.TrangThaiHen == "ChoXacNhan")),
            SoDaKham      = caTrucVMs.Sum(c => c.BenhNhans.Count(b => b.TrangThaiHen == "DaKham")),
        };

        return View(vm);
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Gọi số tiếp theo
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoiSoTiepTheo(int maPhong)
    {
        var result = await _hangDoi.GoiSoTiepTheoAsync(maPhong);
        if (result == null)
            return Json(new { ok = false, message = "Không còn bệnh nhân nào đang chờ." });

        return Json(new
        {
            ok             = true,
            maPhieu        = result.MaPhieu,
            soThuTu        = result.SoThuTu,
            maSoThuTu      = result.MaSoThuTu,
            tenBenhNhan    = result.TenBenhNhan,
            tenPhong       = result.TenPhong,
            soLanGoi       = result.SoLanGoi,
            trangThaiPhieu = result.TrangThaiPhieu,
        });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Gọi lại (Recall)
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GoiLai(int maPhieu)
    {
        var result = await _hangDoi.GoiLaiAsync(maPhieu);
        if (result == null)
            return Json(new { ok = false, message = "Đã gọi đủ 3 lần. Vui lòng chọn Qua lượt." });

        return Json(new
        {
            ok             = true,
            maPhieu        = result.MaPhieu,
            soThuTu        = result.SoThuTu,
            maSoThuTu      = result.MaSoThuTu,
            tenBenhNhan    = result.TenBenhNhan,
            tenPhong       = result.TenPhong,
            soLanGoi       = result.SoLanGoi,
            trangThaiPhieu = result.TrangThaiPhieu,
        });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Bắt đầu khám
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatDauKham(int maPhieu)
    {
        var p = await _db.PhieuSoThuTus.Include(x => x.PhieuDatHen)
            .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);
        if (p == null) return Json(new { ok = false });

        p.TrangThaiPhieu     = "DangKham";
        p.ThoiGianBatDauKham = DateTime.Now;
        await _db.SaveChangesAsync();
        return Json(new { ok = true, trangThaiPhieu = p.TrangThaiPhieu });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Hoàn tất khám
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HoanTat(int maPhieu)
    {
        var p = await _db.PhieuSoThuTus.Include(x => x.PhieuDatHen)
            .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);
        if (p == null) return Json(new { ok = false });

        p.TrangThaiPhieu  = "HoanTat";
        p.ThoiGianHoanTat = DateTime.Now;
        if (p.PhieuDatHen != null)
            p.PhieuDatHen.TrangThaiHen = "DaKham";
        await _db.SaveChangesAsync();
        return Json(new { ok = true, trangThaiPhieu = p.TrangThaiPhieu });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Qua lượt
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuaLuot(int maPhieu)
    {
        var error = await _hangDoi.QuaLuotAsync(maPhieu);
        if (error != null) return Json(new { ok = false, message = error });
        return Json(new { ok = true });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Polling trạng thái hàng đợi phòng (mỗi 10s)
    // ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> LayHangDoi(int maPhong)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var ds = await _db.PhieuSoThuTus
            .Include(p => p.BenhNhan)
            .Where(p => p.MaPhong == maPhong
                     && p.NgayCapSo == today
                     && p.TrangThaiPhieu != "HoanTat"
                     && p.TrangThaiPhieu != "QuaLuot")
            .OrderBy(p => p.MucUuTien)
            .ThenBy(p => p.GioHen == null)
            .ThenBy(p => p.GioHen)
            .ThenBy(p => p.ThoiGianCheckIn)
            .Select(p => new
            {
                p.MaPhieu,
                SoThuTu        = HangDoiService.FormatSoThuTu(p),
                TenBenhNhan    = p.BenhNhan != null ? p.BenhNhan.TenBenhNhan : "",
                p.TrangThaiPhieu,
                GioHen         = p.GioHen != null ? p.GioHen.Value.ToString(@"HH\:mm") : null,
                p.SoLanGoi,
                p.MucUuTien,
                p.LoaiKhach,
            })
            .ToListAsync();

        return Json(ds);
    }

    // ══════════════════════════════════════════════════════════════
    //  Xác nhận phiếu ChoXacNhan → DaXacNhan (Bác sĩ)
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanPhieu(int maPhieuHen)
    {
        var phieu = await _db.PhieuDatHens
            .FirstOrDefaultAsync(p => p.MaPhieuHen == maPhieuHen);

        if (phieu != null && phieu.TrangThaiHen == "ChoXacNhan")
        {
            phieu.TrangThaiHen = "DaXacNhan";
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xác nhận phiếu {phieu.MaDatHen}.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ══════════════════════════════════════════════════════════════
    //  Lịch sử ca trực
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> LichSuCa(int tuan = 0)
    {
        ViewData["Title"]      = "Lịch sử ca trực";
        ViewData["ActiveMenu"] = "BacSi";

        int maTaiKhoan = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bacSi = await _db.BacSis.FirstOrDefaultAsync(b => b.MaTaiKhoan == maTaiKhoan);
        if (bacSi == null) return Forbid();

        var refDay = DateTime.Today.AddDays(tuan * 7);
        int dow    = (int)refDay.DayOfWeek;
        var tuanBD = DateOnly.FromDateTime(refDay.AddDays(-(dow == 0 ? 6 : dow - 1)));
        var tuanKT = tuanBD.AddDays(6);

        var caTrucs = await _db.CaTrucs
            .Include(c => c.PhongKham)
            .Where(c => c.MaBacSi == bacSi.MaBacSi &&
                        c.NgayTruc >= tuanBD && c.NgayTruc <= tuanKT)
            .OrderBy(c => c.NgayTruc).ThenBy(c => c.GioBatDauCa)
            .ToListAsync();

        ViewBag.TuanBatDau  = tuanBD;
        ViewBag.TuanKetThuc = tuanKT;
        ViewBag.TuanOffset  = tuan;
        ViewBag.HoTenBacSi  = bacSi.HoTenBacSi;
        return View(caTrucs);
    }
}
