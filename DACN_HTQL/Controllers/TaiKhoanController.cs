using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DACN_HTQL.Data;
using DACN_HTQL.Models.Entities;
using DACN_HTQL.Models.ViewModels;

namespace DACN_HTQL.Controllers;

public class TaiKhoanController : Controller
{
    private readonly ClinicDbContext _db;

    public TaiKhoanController(ClinicDbContext db) { _db = db; }

    // ── Đăng nhập ─────────────────────────────────────────
    [HttpGet]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new DangNhapVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var tk = await _db.TaiKhoans
            .Include(t => t.VaiTro)
            .FirstOrDefaultAsync(t => t.TenTaiKhoan == vm.TenTaiKhoan
                                   && t.TrangThai == "HoatDong");

        if (tk == null || !BCrypt.Net.BCrypt.Verify(vm.MatKhau, tk.MatKhau))
        {
            ModelState.AddModelError(string.Empty, "Tên tài khoản hoặc mật khẩu không đúng.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, tk.MaTaiKhoan.ToString()),
            new(ClaimTypes.Name,           tk.TenTaiKhoan),
            new(ClaimTypes.Role,           tk.VaiTro.TenVaiTro),
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var props = new AuthenticationProperties
        {
            IsPersistent = vm.GhiNho,
            ExpiresUtc   = vm.GhiNho
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8),
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);

        return tk.VaiTro.TenVaiTro switch
        {
            "Admin"  => RedirectToAction("Index", "Admin"),
            "LeTan"  => RedirectToAction("Index", "LeTan"),
            "BacSi"  => RedirectToAction("Index", "BacSi"),
            _        => RedirectToAction("Index", "Home"),
        };
    }

    // ── Đăng ký (Bệnh nhân) ───────────────────────────────
    [HttpGet]
    public IActionResult DangKy()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new DangKyVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(DangKyVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _db.TaiKhoans.AnyAsync(t => t.TenTaiKhoan == vm.TenTaiKhoan))
        {
            ModelState.AddModelError(nameof(vm.TenTaiKhoan), "Tên tài khoản đã tồn tại.");
            return View(vm);
        }
        if (await _db.BenhNhans.AnyAsync(b => b.SoDienThoai == vm.SoDienThoai))
        {
            ModelState.AddModelError(nameof(vm.SoDienThoai), "Số điện thoại đã được đăng ký.");
            return View(vm);
        }

        var vaiTroBN = await _db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "BenhNhan");
        if (vaiTroBN == null)
        {
            ModelState.AddModelError(string.Empty, "Hệ thống chưa được cấu hình. Vui lòng liên hệ quản trị.");
            return View(vm);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var tk = new TaiKhoan
            {
                TenTaiKhoan = vm.TenTaiKhoan,
                MatKhau     = BCrypt.Net.BCrypt.HashPassword(vm.MatKhau),
                SoDienThoai = vm.SoDienThoai,
                TrangThai   = "HoatDong",
                MaVaiTro    = vaiTroBN.MaVaiTro,
            };
            _db.TaiKhoans.Add(tk);
            await _db.SaveChangesAsync();

            var bn = new BenhNhan
            {
                TenBenhNhan = vm.HoTen,
                SoDienThoai = vm.SoDienThoai,
                Email       = vm.Email,
            };
            _db.BenhNhans.Add(bn);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(DangNhap));
        }
        catch
        {
            await tx.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại.");
            return View(vm);
        }
    }

    // ── Đăng xuất ─────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Bạn đã đăng xuất thành công.";
        return RedirectToAction("Index", "Home");
    }

    // ── Từ chối truy cập ──────────────────────────────────
    [HttpGet]
    public IActionResult KhongCoQuyen()
    {
        Response.StatusCode = 403;
        return View();
    }
}
