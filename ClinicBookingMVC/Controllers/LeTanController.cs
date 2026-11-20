using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Data;
using ClinicBookingMVC.Models.Entities;
using ClinicBookingMVC.Models.ViewModels;
using ClinicBookingMVC.Services;

namespace ClinicBookingMVC.Controllers;

[Authorize(Roles = "LeTan,Admin")]
public class LeTanController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly HangDoiService  _hangDoi;

    public LeTanController(ClinicDbContext db, HangDoiService hangDoi)
    {
        _db      = db;
        _hangDoi = hangDoi;
    }

    // ══════════════════════════════════════════════════════════════
    //  TRANG CHÍNH — Hàng đợi + Check-in nhanh + Chờ xác nhận
    // ══════════════════════════════════════════════════════════════
    public async Task<IActionResult> Index(int? maPhong, int? maChuyenKhoa)
    {
        ViewData["Title"]      = "Quầy lễ tân";
        ViewData["ActiveMenu"] = "LeTan";

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Danh sách phòng + thống kê chờ
        var thongKePhong = await _hangDoi.LayThongKePhongChoAsync(maChuyenKhoa ?? 0);
        ViewBag.ThongKePhong   = thongKePhong;
        ViewBag.DanhSachPhong  = thongKePhong; // alias for select

        // Danh sách chuyên khoa (filter)
        ViewBag.DanhSachChuyenKhoa = await _db.ChuyenKhoas
            .Where(c => c.TrangThai == "HoatDong")
            .OrderBy(c => c.TenChuyenKhoa)
            .ToListAsync();

        var vm = new LeTanIndexVM
        {
            MaPhongDangChon = maPhong,
            NgayHienTai     = today,
        };

        if (maPhong.HasValue)
        {
            // Hàng đợi hôm nay — sắp theo thuật toán 3 mức
            var hangDoi = await _db.PhieuSoThuTus
                .Include(p => p.BenhNhan)
                .Include(p => p.PhieuDatHen)
                .Where(p => p.MaPhong == maPhong.Value && p.NgayCapSo == today)
                .OrderBy(p => p.MucUuTien)
                .ThenBy(p => p.GioHen == null)
                .ThenBy(p => p.GioHen)
                .ThenBy(p => p.ThoiGianCheckIn)
                .ToListAsync();

            vm.HangDoi    = hangDoi;
            vm.SoDangCho  = hangDoi.Count(h => h.TrangThaiPhieu == "DangCho");
            vm.SoDangGoi  = hangDoi.Count(h => h.TrangThaiPhieu == "DangGoi");
            vm.SoDangKham = hangDoi.Count(h => h.TrangThaiPhieu == "DangKham");
            vm.SoHoanTat  = hangDoi.Count(h => h.TrangThaiPhieu == "HoanTat");

            // Phiếu ChoXacNhan của phòng hôm nay
            var choXacNhan = await _db.PhieuDatHens
                .Include(p => p.BenhNhan)
                .Include(p => p.KhungGio)
                    .ThenInclude(k => k.CaTruc)
                        .ThenInclude(c => c.BacSi)
                .Where(p =>
                    p.TrangThaiHen == "ChoXacNhan" &&
                    p.KhungGio.CaTruc.MaPhong == maPhong.Value &&
                    p.KhungGio.CaTruc.NgayTruc == today)
                .OrderBy(p => p.KhungGio.GioBatDau)
                .ToListAsync();

            vm.ChoXacNhan   = choXacNhan;
            vm.SoChoXacNhan = choXacNhan.Count;
        }

        return View(vm);
    }

    // ══════════════════════════════════════════════════════════════
    //  CHECK-IN: Khách đặt trước (theo MaDatHen hoặc SoDienThoai)
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int maPhieuHen, int maPhong)
    {
        var (phieu, error) = await _hangDoi.CheckInDatTruocAsync(maPhieuHen, maPhong);

        if (error != null)
            TempData["ErrorMessage"] = error;
        else
            TempData["SuccessMessage"] = $"Check-in thành công. Số: {HangDoiService.FormatSoThuTu(phieu!)}";

        return RedirectToAction(nameof(Index), new { maPhong });
    }

    // ══════════════════════════════════════════════════════════════
    //  CHECK-IN: Vãng lai (Walk-in) + hỗ trợ auto-fill khách quen
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInVangLai(string soDienThoai, int maPhong)
    {
        if (string.IsNullOrWhiteSpace(soDienThoai))
        {
            TempData["ErrorMessage"] = "Nhập số điện thoại để check-in.";
            return RedirectToAction(nameof(Index), new { maPhong });
        }

        var (phieu, isNew) = await _hangDoi.CheckInVangLaiAsync(soDienThoai, maPhong);
        TempData["SuccessMessage"] =
            $"Check-in vãng lai thành công. Số: {HangDoiService.FormatSoThuTu(phieu)}" +
            (isNew ? " (khách mới)" : "");

        return RedirectToAction(nameof(Index), new { maPhong });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Tra cứu nhanh theo MaDatHen (cho khung Check-in nhanh)
    // ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> TraCuuNhanh(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(null);

        q = q.Trim().ToUpper();
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Tìm theo MaDatHen hoặc SoDienThoai
        var phieu = await _db.PhieuDatHens
            .Include(p => p.BenhNhan)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.PhongKham)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.BacSi)
            .Where(p =>
                (p.MaDatHen.ToUpper() == q || p.BenhNhan.SoDienThoai == q) &&
                p.KhungGio.CaTruc.NgayTruc == today &&
                p.TrangThaiHen != "DaHuy")
            .FirstOrDefaultAsync();

        if (phieu == null) return Json(null);

        return Json(new
        {
            phieu.MaPhieuHen,
            phieu.MaDatHen,
            phieu.TrangThaiHen,
            TenBenhNhan    = phieu.BenhNhan.TenBenhNhan,
            SoDienThoai    = phieu.BenhNhan.SoDienThoai,
            CoCanhBao      = phieu.BenhNhan.CoCanhBao,
            SoLanVangHen   = phieu.BenhNhan.SoLanVangHen,
            GioHen         = phieu.KhungGio.GioBatDau.ToString(@"HH\:mm"),
            TenBacSi       = $"{phieu.KhungGio.CaTruc.BacSi.HocVi} {phieu.KhungGio.CaTruc.BacSi.HoTenBacSi}".Trim(),
            TenPhong       = phieu.KhungGio.CaTruc.PhongKham.TenPhong,
            MaPhong        = phieu.KhungGio.CaTruc.MaPhong,
        });
    }

    // ══════════════════════════════════════════════════════════════
    //  AJAX: Tra cứu BenhNhan theo SoDienThoai (auto-fill walk-in)
    // ══════════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> TraCuuBenhNhan(string sdt)
    {
        if (string.IsNullOrWhiteSpace(sdt)) return Json(null);
        var bn = await _db.BenhNhans
            .FirstOrDefaultAsync(b => b.SoDienThoai == sdt.Trim());
        if (bn == null) return Json(null);

        return Json(new
        {
            bn.MaBenhNhan,
            bn.TenBenhNhan,
            bn.SoDienThoai,
            bn.NamSinh,
            bn.GioiTinh,
            bn.Email,
            bn.CoCanhBao,
            bn.SoLanVangHen,
        });
    }

    // ══════════════════════════════════════════════════════════════
    //  XÁC NHẬN PHIẾU ChoXacNhan → DaXacNhan (Lễ tân)
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanPhieu(int maPhieuHen, int maPhong)
    {
        var phieu = await _db.PhieuDatHens
            .FirstOrDefaultAsync(p => p.MaPhieuHen == maPhieuHen);

        if (phieu != null && phieu.TrangThaiHen == "ChoXacNhan")
        {
            phieu.TrangThaiHen = "DaXacNhan";
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xác nhận phiếu {phieu.MaDatHen}.";
        }

        return RedirectToAction(nameof(Index), new { maPhong });
    }

    // ══════════════════════════════════════════════════════════════
    //  KHÔI PHỤC VÀO HÀNG CHỜ (sau QuaLuot)
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoiPhuc(int maPhieu, int maPhong)
    {
        var error = await _hangDoi.KhoiPhucHangChoAsync(maPhieu);
        if (error != null)
            TempData["ErrorMessage"] = error;
        else
            TempData["SuccessMessage"] = "Đã khôi phục bệnh nhân vào hàng chờ (ưu tiên thấp).";

        return RedirectToAction(nameof(Index), new { maPhong });
    }

    // ══════════════════════════════════════════════════════════════
    //  Các thao tác cũ (tương thích BacSiController gọi)
    // ══════════════════════════════════════════════════════════════
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BatDauKham(int maPhieu, int maPhong)
    {
        var p = await _db.PhieuSoThuTus.Include(x => x.PhieuDatHen)
            .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);
        if (p != null)
        {
            p.TrangThaiPhieu     = "DangKham";
            p.ThoiGianBatDauKham = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { maPhong });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HoanTat(int maPhieu, int maPhong)
    {
        var p = await _db.PhieuSoThuTus.Include(x => x.PhieuDatHen)
            .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);
        if (p != null)
        {
            p.TrangThaiPhieu  = "HoanTat";
            p.ThoiGianHoanTat = DateTime.Now;
            if (p.PhieuDatHen != null)
                p.PhieuDatHen.TrangThaiHen = "DaKham";
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { maPhong });
    }
}
