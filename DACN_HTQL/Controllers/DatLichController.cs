using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DACN_HTQL.Data;
using DACN_HTQL.Models.Entities;
using DACN_HTQL.Models.ViewModels;
using DACN_HTQL.Services;

namespace DACN_HTQL.Controllers;

public class DatLichController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly DatLichService _datLichSvc;

    public DatLichController(ClinicDbContext db, DatLichService datLichSvc)
    {
        _db        = db;
        _datLichSvc = datLichSvc;
    }

    // ─── Trang chính đặt lịch ───────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Đặt lịch khám";
        var chuyenKhoas = await _db.ChuyenKhoas
            .Where(c => c.TrangThai == "HoatDong")
            .OrderBy(c => c.TenChuyenKhoa)
            .ToListAsync();
        return View(chuyenKhoas);
    }

    // ─── AJAX: Lấy danh sách Bác sĩ theo Chuyên khoa + Ngày ───────────────
    [HttpGet]
    public async Task<IActionResult> LayBacSi(int maChuyenKhoa, string ngay)
    {
        if (!DateOnly.TryParse(ngay, out var ngayKham))
            return BadRequest("Ngày không hợp lệ.");

        var bacSis = await _db.BacSis
            .Where(b => b.MaChuyenKhoa == maChuyenKhoa && b.TrangThai == "HoatDong")
            .Select(b => new
            {
                b.MaBacSi,
                b.HoTenBacSi,
                b.HocVi,
                SoSlotTrong = _db.KhungGios.Count(k =>
                    k.CaTruc.MaBacSi == b.MaBacSi &&
                    k.CaTruc.NgayTruc == ngayKham &&
                    k.LoaiKhungGio == "DatHen" &&
                    k.TrangThai == "ConTrong"),
            })
            .Where(b => b.SoSlotTrong > 0)
            .OrderBy(b => b.HoTenBacSi)
            .ToListAsync();

        return Json(bacSis);
    }

    // ─── AJAX: Lấy lưới khung giờ theo Bác sĩ + Ngày ──────────────────────
    [HttpGet]
    public async Task<IActionResult> LayKhungGio(int maBacSi, string ngay)
    {
        if (!DateOnly.TryParse(ngay, out var ngayKham))
            return BadRequest();

        var ca = await _db.CaTrucs
            .Include(c => c.BacSi).ThenInclude(b => b.ChuyenKhoa)
            .Include(c => c.PhongKham)
            .Include(c => c.KhungGios)
            .Where(c => c.MaBacSi == maBacSi && c.NgayTruc == ngayKham)
            .FirstOrDefaultAsync();

        if (ca == null) return Json(null);

        var vm = new LuoiKhungGioVM
        {
            MaBacSi       = ca.MaBacSi,
            HoTenBacSi    = ca.BacSi.HoTenBacSi,
            HocVi         = ca.BacSi.HocVi,
            TenChuyenKhoa = ca.BacSi.ChuyenKhoa?.TenChuyenKhoa ?? "",
            TenPhong      = ca.PhongKham.TenPhong,
            NgayKham      = ngayKham,
            Slots = ca.KhungGios
                .Where(k => k.LoaiKhungGio == "DatHen")
                .OrderBy(k => k.GioBatDau)
                .Select(k => new SlotVM
                {
                    MaKhungGio         = k.MaKhungGio,
                    GioBatDau          = k.GioBatDau,
                    GioKetThuc         = k.GioKetThuc,
                    TrangThai          = k.TrangThai,
                    LoaiKhungGio       = k.LoaiKhungGio,
                    PhienBanDongBase64 = k.PhienBanDong == null
                        ? null : Convert.ToBase64String(k.PhienBanDong),
                })
                .ToList(),
        };

        return Json(vm);
    }

    // ─── Trang xác nhận đặt lịch (GET) ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> XacNhan(int maKhungGio)
    {
        // Yêu cầu đăng nhập
        if (!User.Identity!.IsAuthenticated)
            return RedirectToAction("DangNhap", "TaiKhoan",
                new { returnUrl = $"/DatLich/XacNhan?maKhungGio={maKhungGio}" });

        var kg = await _db.KhungGios
            .Include(k => k.CaTruc)
                .ThenInclude(c => c.BacSi)
                    .ThenInclude(b => b.ChuyenKhoa)
            .Include(k => k.CaTruc)
                .ThenInclude(c => c.PhongKham)
            .FirstOrDefaultAsync(k => k.MaKhungGio == maKhungGio);

        if (kg == null) return NotFound();
        if (kg.TrangThai != "ConTrong" || kg.LoaiKhungGio != "DatHen")
        {
            TempData["Error"] = "Khung giờ này không còn trống. Vui lòng chọn khung giờ khác.";
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra thời điểm hợp lệ (BR-01)
        var loiThoiDiem = _datLichSvc.KiemTraThoiDiemDatHen(
            kg.CaTruc.NgayTruc, kg.GioBatDau);
        if (loiThoiDiem != null)
        {
            TempData["Error"] = loiThoiDiem;
            return RedirectToAction(nameof(Index));
        }

        var vm = new XacNhanDatLichVM
        {
            MaKhungGio         = kg.MaKhungGio,
            PhienBanDongBase64 = kg.PhienBanDong == null
                ? null : Convert.ToBase64String(kg.PhienBanDong),
            TenBacSi     = $"{kg.CaTruc.BacSi.HocVi} {kg.CaTruc.BacSi.HoTenBacSi}".Trim(),
            TenChuyenKhoa = kg.CaTruc.BacSi.ChuyenKhoa?.TenChuyenKhoa,
            TenPhong     = kg.CaTruc.PhongKham.TenPhong,
            NgayKham     = kg.CaTruc.NgayTruc,
            GioBatDau    = kg.GioBatDau,
            GioKetThuc   = kg.GioKetThuc,
            GuiNhacLich  = true,
        };

        ViewData["Title"] = "Xác nhận đặt lịch";
        return View(vm);
    }

    // ─── Xác nhận đặt lịch (POST) ───────────────────────────────────────────
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhan(XacNhanDatLichVM vm)
    {
        if (!ModelState.IsValid)
        {
            // Nạp lại thông tin slot
            await NapLaiThongTinSlot(vm);
            ViewData["Title"] = "Xác nhận đặt lịch";
            return View(vm);
        }

        int maTaiKhoan = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Kiểm tra trùng lịch trước (BR-02 lightweight check)
        var loiTrung = await _datLichSvc.KiemTraTrungLichAsync(
            vm.SoDienThoai, vm.MaKhungGio,
            vm.NgayKham == default ? DateOnly.FromDateTime(DateTime.Today) : vm.NgayKham);
        if (loiTrung != null)
        {
            ModelState.AddModelError("", loiTrung);
            await NapLaiThongTinSlot(vm);
            ViewData["Title"] = "Xác nhận đặt lịch";
            return View(vm);
        }

        // Lấy / tạo BenhNhan
        var bn = await _datLichSvc.LayHoacTaoBenhNhanAsync(
            vm.SoDienThoai, vm.TenBenhNhan,
            vm.NamSinh, vm.GioiTinh, vm.Email);

        // Chuyển RowVersion từ Base64
        byte[]? phienBanDong = string.IsNullOrEmpty(vm.PhienBanDongBase64)
            ? null : Convert.FromBase64String(vm.PhienBanDongBase64);

        var (ok, loiMsg, phieu) = await _datLichSvc.DatLichAsync(
            vm.MaKhungGio, phienBanDong,
            bn.MaBenhNhan, maTaiKhoan,
            vm.LyDoKham ?? "", vm.GuiNhacLich);

        if (!ok)
        {
            ModelState.AddModelError("", loiMsg!);
            await NapLaiThongTinSlot(vm);
            ViewData["Title"] = "Xác nhận đặt lịch";
            return View(vm);
        }

        // Thành công → chuyển sang trang phiếu hẹn
        return RedirectToAction(nameof(PhieuHen),
            new { maDatHen = phieu!.MaDatHen });
    }

    // ─── Phiếu hẹn điện tử ──────────────────────────────────────────────────
    public async Task<IActionResult> PhieuHen(string maDatHen)
    {
        var phieu = await _db.PhieuDatHens
            .Include(p => p.BenhNhan)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.BacSi)
                        .ThenInclude(b => b.ChuyenKhoa)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.PhongKham)
            .FirstOrDefaultAsync(p => p.MaDatHen == maDatHen);

        if (phieu == null) return NotFound();

        var sdt = phieu.BenhNhan.SoDienThoai;
        var vm = new PhieuHenDienTuVM
        {
            MaDatHen           = phieu.MaDatHen,
            TrangThaiHen       = phieu.TrangThaiHen,
            TenBenhNhan        = phieu.BenhNhan.TenBenhNhan,
            SoDienThoaiAnPhan  = AnPhanSoDienThoai(sdt),
            TenBacSi           = $"{phieu.KhungGio.CaTruc.BacSi.HocVi} {phieu.KhungGio.CaTruc.BacSi.HoTenBacSi}".Trim(),
            TenChuyenKhoa      = phieu.KhungGio.CaTruc.BacSi.ChuyenKhoa?.TenChuyenKhoa ?? "",
            TenPhong           = phieu.KhungGio.CaTruc.PhongKham.TenPhong,
            NgayKham           = phieu.KhungGio.CaTruc.NgayTruc,
            GioBatDau          = phieu.KhungGio.GioBatDau,
            GioKetThuc         = phieu.KhungGio.GioKetThuc,
            LyDoKham           = phieu.LyDoKhamSoBo,
            ThoiGianTao        = phieu.ThoiGianTaoPhieu,
            CoCanhBao          = phieu.BenhNhan.CoCanhBao,
        };

        ViewData["Title"] = $"Phiếu hẹn {phieu.MaDatHen}";
        return View(vm);
    }

    // ─── Hủy phiếu ──────────────────────────────────────────────────────────
    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyPhieu(int maPhieuHen, string? returnUrl)
    {
        int maTaiKhoan = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var (ok, loiMsg) = await _datLichSvc.HuyLichAsync(maPhieuHen, maTaiKhoan);
        if (ok)
            TempData["Success"] = "Hủy lịch hẹn thành công.";
        else
            TempData["Error"] = loiMsg;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(TraCuu));
    }

    // ─── Tra cứu ────────────────────────────────────────────────────────────
    public async Task<IActionResult> TraCuu(string? q)
    {
        ViewData["Title"] = "Tra cứu lịch hẹn";
        var vm = new KetQuaTraCuuVM { TuKhoa = q };

        if (string.IsNullOrWhiteSpace(q))
        {
            // Nếu đã đăng nhập → hiển thị lịch của tài khoản
            if (User.Identity!.IsAuthenticated)
            {
                int maTaiKhoan = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var cacPhieu = await LayDanhSachPhieuAsync(
                    p => p.MaNguoiTao == maTaiKhoan);
                vm.DanhSachPhieu = cacPhieu;
            }
            return View(vm);
        }

        // Tra theo MaDatHen hoặc SoDienThoai
        List<PhieuHenTraCuuVM> results;
        if (q.StartsWith("BK", StringComparison.OrdinalIgnoreCase))
        {
            results = await LayDanhSachPhieuAsync(
                p => p.MaDatHen == q.Trim().ToUpper());
        }
        else
        {
            results = await LayDanhSachPhieuAsync(
                p => p.BenhNhan.SoDienThoai == q.Trim());
        }

        vm.DanhSachPhieu = results;
        return View(vm);
    }

    // ─── AJAX: Tiến độ hàng đợi (poll mỗi 15s) ─────────────────────────────
    [HttpGet]
    public async Task<IActionResult> TienDoHangDoi(int maPhong, int? soThuTu)
    {
        var dto = await _datLichSvc.LayTienDoHangDoiAsync(maPhong, soThuTu);
        return Json(dto);
    }

    // ─── AJAX: Tra cứu BenhNhan nhanh theo SoDienThoai ─────────────────────
    [HttpGet]
    public async Task<IActionResult> TraCuuBenhNhan(string sdt)
    {
        bool leTanMode = User.IsInRole("LeTan") || User.IsInRole("Admin");
        var result = await _datLichSvc.TraCuuBenhNhanAsync(sdt, leTanMode);
        if (result == null) return Json(null);
        return Json(result);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private static string AnPhanSoDienThoai(string sdt)
    {
        if (sdt.Length < 7) return sdt;
        return sdt[..3] + "***" + sdt[^3..];
    }

    private async Task NapLaiThongTinSlot(XacNhanDatLichVM vm)
    {
        var kg = await _db.KhungGios
            .Include(k => k.CaTruc)
                .ThenInclude(c => c.BacSi)
                    .ThenInclude(b => b.ChuyenKhoa)
            .Include(k => k.CaTruc)
                .ThenInclude(c => c.PhongKham)
            .FirstOrDefaultAsync(k => k.MaKhungGio == vm.MaKhungGio);

        if (kg == null) return;
        vm.TenBacSi     = $"{kg.CaTruc.BacSi.HocVi} {kg.CaTruc.BacSi.HoTenBacSi}".Trim();
        vm.TenChuyenKhoa = kg.CaTruc.BacSi.ChuyenKhoa?.TenChuyenKhoa;
        vm.TenPhong     = kg.CaTruc.PhongKham.TenPhong;
        vm.NgayKham     = kg.CaTruc.NgayTruc;
        vm.GioBatDau    = kg.GioBatDau;
        vm.GioKetThuc   = kg.GioKetThuc;
        vm.PhienBanDongBase64 = kg.PhienBanDong == null
            ? null : Convert.ToBase64String(kg.PhienBanDong);
    }

    private async Task<List<PhieuHenTraCuuVM>> LayDanhSachPhieuAsync(
        System.Linq.Expressions.Expression<Func<PhieuDatHen, bool>> filter)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var phieus = await _db.PhieuDatHens
            .Include(p => p.BenhNhan)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.BacSi)
                        .ThenInclude(b => b.ChuyenKhoa)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.PhongKham)
            .Include(p => p.PhieuSoThuTu)
            .Where(filter)
            .OrderByDescending(p => p.ThoiGianTaoPhieu)
            .Take(20)
            .ToListAsync();

        var result = new List<PhieuHenTraCuuVM>();
        foreach (var p in phieus)
        {
            var stt = p.PhieuSoThuTu;
            HangDoiProgressDto? tienDo = null;
            if (stt != null
                && p.KhungGio.CaTruc.NgayTruc == today
                && stt.TrangThaiPhieu is "DangCho" or "DangGoi" or "DangKham")
            {
                tienDo = await _datLichSvc.LayTienDoHangDoiAsync(
                    p.KhungGio.CaTruc.MaPhong, stt.SoThuTu);
            }

            // Kiểm tra điều kiện hủy
            var loiHuy = _datLichSvc.KiemTraDieuKienHuy(p);
            var sdt    = p.BenhNhan.SoDienThoai;

            result.Add(new PhieuHenTraCuuVM
            {
                MaPhieuHen        = p.MaPhieuHen,
                MaDatHen          = p.MaDatHen,
                TrangThaiHen      = p.TrangThaiHen,
                TenBenhNhan       = p.BenhNhan.TenBenhNhan,
                SoDienThoaiAnPhan = AnPhanSoDienThoai(sdt),
                TenBacSi          = $"{p.KhungGio.CaTruc.BacSi.HocVi} {p.KhungGio.CaTruc.BacSi.HoTenBacSi}".Trim(),
                TenChuyenKhoa     = p.KhungGio.CaTruc.BacSi.ChuyenKhoa?.TenChuyenKhoa ?? "",
                TenPhong          = p.KhungGio.CaTruc.PhongKham.TenPhong,
                NgayKham          = p.KhungGio.CaTruc.NgayTruc,
                GioBatDau         = p.KhungGio.GioBatDau,
                ThoiGianTao       = p.ThoiGianTaoPhieu,
                SoThuTuBenhNhan   = stt?.SoThuTu,
                TienDoHangDoi     = tienDo,
                CoTheHuy          = loiHuy == null,
                LyDoKhongTheHuy   = loiHuy,
                MaPhong           = p.KhungGio.CaTruc.MaPhong,
            });
        }
        return result;
    }
}
