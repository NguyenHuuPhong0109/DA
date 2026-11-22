using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DACN_HTQL.Data;
using DACN_HTQL.Models.Entities;
using DACN_HTQL.Models.ViewModels;
using DACN_HTQL.Services;

namespace DACN_HTQL.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ClinicDbContext _db;
    private readonly CaTrucService _caTrucService;
    private readonly BaoCaoService _baoCaoService;
    private readonly VangHenService _vangHenService;

    public AdminController(
        ClinicDbContext db,
        CaTrucService caTrucService,
        BaoCaoService baoCaoService,
        VangHenService vangHenService)
    {
        _db = db;
        _caTrucService = caTrucService;
        _baoCaoService = baoCaoService;
        _vangHenService = vangHenService;
    }

    // ─── BI Dashboard Thống kê KPI ──────────────────────────────────────────
    public async Task<IActionResult> Index(BaoCaoFilterVM filter)
    {
        ViewData["Title"] = "BI Dashboard & Thống kê KPI";
        ViewData["ActiveMenu"] = "Dashboard";

        ViewBag.SoChuyenKhoa      = await _db.ChuyenKhoas.CountAsync();
        ViewBag.SoPhongKham       = await _db.PhongKhams.CountAsync();
        ViewBag.SoBacSi           = await _db.BacSis.CountAsync();
        ViewBag.SoTaiKhoan        = await _db.TaiKhoans.CountAsync();
        ViewBag.SoCaTrucHomNay    = await _db.CaTrucs
            .CountAsync(c => c.NgayTruc == DateOnly.FromDateTime(DateTime.Today));

        var dashboardVM = await _baoCaoService.LayBaoCaoDashboardAsync(filter);
        return View(dashboardVM);
    }

    [HttpGet]
    public async Task<IActionResult> ExportBaoCaoCsv(BaoCaoFilterVM filter)
    {
        var csvBytes = await _baoCaoService.ExportBaoCaoCsvAsync(filter);
        var fileName = $"BaoCao_KPI_PhongKham_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuetQuaHanNgay()
    {
        var (soVangHen, soNhacLich) = await _vangHenService.QuetVaXuLyAsync();
        TempData["SuccessMessage"] = $"Đã quét hệ thống: {soVangHen} phiếu quá hạn chuyển thành Vắng hẹn, {soNhacLich} lịch hẹn ngày mai được gửi nhắc khám.";
        return RedirectToAction(nameof(Index));
    }

    // ─── CHUYÊN KHOA ─────────────────────────────────────────────────────────
    public async Task<IActionResult> ChuyenKhoa()
    {
        ViewData["Title"]      = "Quản lý Chuyên khoa";
        ViewData["ActiveMenu"] = "ChuyenKhoa";
        return View(await _db.ChuyenKhoas.OrderBy(c => c.TenChuyenKhoa).ToListAsync());
    }

    [HttpGet]
    public IActionResult TaoChuyenKhoa()
    {
        ViewData["Title"]      = "Thêm Chuyên khoa";
        ViewData["ActiveMenu"] = "ChuyenKhoa";
        return View(new ChuyenKhoaVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoChuyenKhoa(ChuyenKhoaVM vm)
    {
        if (!ModelState.IsValid) { ViewData["Title"] = "Thêm Chuyên khoa"; return View(vm); }
        if (await _db.ChuyenKhoas.AnyAsync(c => c.TenChuyenKhoa == vm.TenChuyenKhoa))
        {
            ModelState.AddModelError(nameof(vm.TenChuyenKhoa), "Tên chuyên khoa đã tồn tại.");
            return View(vm);
        }
        _db.ChuyenKhoas.Add(new ChuyenKhoa
        {
            TenChuyenKhoa = vm.TenChuyenKhoa,
            MoTa          = vm.MoTa,
            TrangThai     = vm.TrangThai,
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm chuyên khoa thành công.";
        return RedirectToAction(nameof(ChuyenKhoa));
    }

    [HttpGet]
    public async Task<IActionResult> SuaChuyenKhoa(int id)
    {
        var ck = await _db.ChuyenKhoas.FindAsync(id);
        if (ck == null) return NotFound();
        ViewData["Title"]      = "Sửa Chuyên khoa";
        ViewData["ActiveMenu"] = "ChuyenKhoa";
        return View(new ChuyenKhoaVM
        {
            MaChuyenKhoa  = ck.MaChuyenKhoa,
            TenChuyenKhoa = ck.TenChuyenKhoa,
            MoTa          = ck.MoTa,
            TrangThai     = ck.TrangThai,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaChuyenKhoa(ChuyenKhoaVM vm)
    {
        if (!ModelState.IsValid) { ViewData["Title"] = "Sửa Chuyên khoa"; return View(vm); }
        var ck = await _db.ChuyenKhoas.FindAsync(vm.MaChuyenKhoa);
        if (ck == null) return NotFound();
        if (await _db.ChuyenKhoas.AnyAsync(c =>
                c.TenChuyenKhoa == vm.TenChuyenKhoa && c.MaChuyenKhoa != vm.MaChuyenKhoa))
        {
            ModelState.AddModelError(nameof(vm.TenChuyenKhoa), "Tên chuyên khoa đã tồn tại.");
            return View(vm);
        }
        ck.TenChuyenKhoa = vm.TenChuyenKhoa;
        ck.MoTa          = vm.MoTa;
        ck.TrangThai     = vm.TrangThai;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật chuyên khoa thành công.";
        return RedirectToAction(nameof(ChuyenKhoa));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaChuyenKhoa(int id)
    {
        var ck = await _db.ChuyenKhoas.FindAsync(id);
        if (ck == null) return NotFound();
        bool coPhong = await _db.PhongKhams.AnyAsync(p => p.MaChuyenKhoa == id);
        bool coBacSi = await _db.BacSis.AnyAsync(b => b.MaChuyenKhoa == id);
        if (coPhong || coBacSi)
        {
            TempData["Error"] = "Không thể xóa vì còn phòng khám hoặc bác sĩ thuộc chuyên khoa này.";
            return RedirectToAction(nameof(ChuyenKhoa));
        }
        _db.ChuyenKhoas.Remove(ck);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa chuyên khoa.";
        return RedirectToAction(nameof(ChuyenKhoa));
    }

    // ─── PHÒNG KHÁM ──────────────────────────────────────────────────────────
    public async Task<IActionResult> PhongKham()
    {
        ViewData["Title"]      = "Quản lý Phòng khám";
        ViewData["ActiveMenu"] = "PhongKham";
        return View(await _db.PhongKhams
            .Include(p => p.ChuyenKhoa)
            .OrderBy(p => p.TenPhong)
            .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> TaoPhongKham()
    {
        ViewData["Title"]      = "Thêm Phòng khám";
        ViewData["ActiveMenu"] = "PhongKham";
        await LoadChuyenKhoaListAsync();
        return View(new PhongKhamVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoPhongKham(PhongKhamVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thêm Phòng khám";
            await LoadChuyenKhoaListAsync();
            return View(vm);
        }
        if (await _db.PhongKhams.AnyAsync(p => p.TenPhong == vm.TenPhong))
        {
            ModelState.AddModelError(nameof(vm.TenPhong), "Tên phòng đã tồn tại.");
            await LoadChuyenKhoaListAsync(); return View(vm);
        }
        _db.PhongKhams.Add(new PhongKham
        {
            TenPhong     = vm.TenPhong,
            ViTri        = vm.ViTri,
            MaChuyenKhoa = vm.MaChuyenKhoa,
            TrangThai    = vm.TrangThai,
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Thêm phòng khám thành công.";
        return RedirectToAction(nameof(PhongKham));
    }

    [HttpGet]
    public async Task<IActionResult> SuaPhongKham(int id)
    {
        var p = await _db.PhongKhams.FindAsync(id);
        if (p == null) return NotFound();
        ViewData["Title"]      = "Sửa Phòng khám";
        ViewData["ActiveMenu"] = "PhongKham";
        await LoadChuyenKhoaListAsync(p.MaChuyenKhoa);
        return View(new PhongKhamVM
        {
            MaPhong      = p.MaPhong,
            TenPhong     = p.TenPhong,
            ViTri        = p.ViTri,
            MaChuyenKhoa = p.MaChuyenKhoa,
            TrangThai    = p.TrangThai,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaPhongKham(PhongKhamVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa Phòng khám";
            await LoadChuyenKhoaListAsync(vm.MaChuyenKhoa);
            return View(vm);
        }
        var p = await _db.PhongKhams.FindAsync(vm.MaPhong);
        if (p == null) return NotFound();
        if (await _db.PhongKhams.AnyAsync(x => x.TenPhong == vm.TenPhong && x.MaPhong != vm.MaPhong))
        {
            ModelState.AddModelError(nameof(vm.TenPhong), "Tên phòng đã tồn tại.");
            await LoadChuyenKhoaListAsync(vm.MaChuyenKhoa); return View(vm);
        }
        p.TenPhong     = vm.TenPhong;
        p.ViTri        = vm.ViTri;
        p.MaChuyenKhoa = vm.MaChuyenKhoa;
        p.TrangThai    = vm.TrangThai;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật phòng khám thành công.";
        return RedirectToAction(nameof(PhongKham));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaPhongKham(int id)
    {
        var p = await _db.PhongKhams.FindAsync(id);
        if (p == null) return NotFound();
        if (await _db.CaTrucs.AnyAsync(c => c.MaPhong == id))
        {
            TempData["Error"] = "Không thể xóa vì còn ca trực được gán cho phòng này.";
            return RedirectToAction(nameof(PhongKham));
        }
        _db.PhongKhams.Remove(p);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa phòng khám.";
        return RedirectToAction(nameof(PhongKham));
    }

    // ─── BÁC SĨ ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> BacSi()
    {
        ViewData["Title"]      = "Quản lý Bác sĩ";
        ViewData["ActiveMenu"] = "BacSi";
        return View(await _db.BacSis
            .Include(b => b.ChuyenKhoa)
            .Include(b => b.TaiKhoan)
            .OrderBy(b => b.HoTenBacSi)
            .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> TaoBacSi()
    {
        ViewData["Title"]      = "Thêm Bác sĩ";
        ViewData["ActiveMenu"] = "BacSi";
        await LoadChuyenKhoaListAsync();
        return View(new TaoBacSiVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoBacSi(TaoBacSiVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Thêm Bác sĩ";
            await LoadChuyenKhoaListAsync();
            return View(vm);
        }
        if (await _db.TaiKhoans.AnyAsync(t => t.TenTaiKhoan == vm.TenTaiKhoan))
        {
            ModelState.AddModelError(nameof(vm.TenTaiKhoan), "Tên tài khoản đã tồn tại.");
            await LoadChuyenKhoaListAsync(); return View(vm);
        }
        var bacSiRole = await _db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "BacSi");
        if (bacSiRole == null)
        {
            TempData["Error"] = "Vai trò BacSi chưa tồn tại. Hãy khởi động lại ứng dụng.";
            return RedirectToAction(nameof(BacSi));
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var tk = new TaiKhoan
            {
                TenTaiKhoan = vm.TenTaiKhoan,
                MatKhau     = BCrypt.Net.BCrypt.HashPassword(vm.MatKhau),
                SoDienThoai = vm.SoDienThoai,
                TrangThai   = "HoatDong",
                MaVaiTro    = bacSiRole.MaVaiTro,
            };
            _db.TaiKhoans.Add(tk);
            await _db.SaveChangesAsync();

            _db.BacSis.Add(new BacSi
            {
                HoTenBacSi   = vm.HoTenBacSi,
                HocVi        = vm.HocVi,
                SoDienThoai  = vm.SoDienThoai,
                MaChuyenKhoa = vm.MaChuyenKhoa,
                MaTaiKhoan   = tk.MaTaiKhoan,
                TrangThai    = "HoatDong",
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            TempData["Error"] = "Tạo bác sĩ thất bại. Vui lòng thử lại.";
            await LoadChuyenKhoaListAsync();
            return View(vm);
        }

        TempData["Success"] = $"Thêm bác sĩ {vm.HoTenBacSi} thành công.";
        return RedirectToAction(nameof(BacSi));
    }

    [HttpGet]
    public async Task<IActionResult> SuaBacSi(int id)
    {
        var bs = await _db.BacSis.Include(b => b.TaiKhoan)
            .FirstOrDefaultAsync(b => b.MaBacSi == id);
        if (bs == null) return NotFound();
        ViewData["Title"]      = "Sửa Bác sĩ";
        ViewData["ActiveMenu"] = "BacSi";
        await LoadChuyenKhoaListAsync(bs.MaChuyenKhoa);
        return View(new SuaBacSiVM
        {
            MaBacSi      = bs.MaBacSi,
            HoTenBacSi   = bs.HoTenBacSi,
            HocVi        = bs.HocVi,
            SoDienThoai  = bs.SoDienThoai ?? "",
            MaChuyenKhoa = bs.MaChuyenKhoa,
            TrangThai    = bs.TrangThai,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaBacSi(SuaBacSiVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa Bác sĩ";
            await LoadChuyenKhoaListAsync(vm.MaChuyenKhoa);
            return View(vm);
        }
        var bs = await _db.BacSis.FindAsync(vm.MaBacSi);
        if (bs == null) return NotFound();
        bs.HoTenBacSi   = vm.HoTenBacSi;
        bs.HocVi        = vm.HocVi;
        bs.SoDienThoai  = vm.SoDienThoai;
        bs.MaChuyenKhoa = vm.MaChuyenKhoa;
        bs.TrangThai    = vm.TrangThai;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cập nhật thông tin bác sĩ thành công.";
        return RedirectToAction(nameof(BacSi));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaBacSi(int id)
    {
        var bs = await _db.BacSis.FindAsync(id);
        if (bs == null) return NotFound();
        if (await _db.CaTrucs.AnyAsync(c => c.MaBacSi == id))
        {
            TempData["Error"] = "Không thể xóa vì bác sĩ còn ca trực.";
            return RedirectToAction(nameof(BacSi));
        }
        var tk = await _db.TaiKhoans.FindAsync(bs.MaTaiKhoan);
        _db.BacSis.Remove(bs);
        if (tk != null) _db.TaiKhoans.Remove(tk);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bác sĩ và tài khoản liên kết.";
        return RedirectToAction(nameof(BacSi));
    }

    // ─── TÀI KHOẢN ───────────────────────────────────────────────────────────
    public async Task<IActionResult> TaiKhoan(string? nhom = null)
    {
        ViewData["ActiveMenu"] = "TaiKhoan";

        var allAccounts = await _db.TaiKhoans
            .Include(t => t.VaiTro)
            .Include(t => t.BacSi)
            .OrderBy(t => t.TenTaiKhoan)
            .ToListAsync();

        ViewBag.SoBenhNhan = allAccounts.Count(t => t.VaiTro?.TenVaiTro == "BenhNhan");
        ViewBag.SoNhanSu   = allAccounts.Count(t => t.VaiTro?.TenVaiTro != "BenhNhan");
        ViewBag.SoTatCa    = allAccounts.Count;
        ViewBag.NhomHienTai = nhom;

        // Tải thông tin tên bệnh nhân theo SĐT để hiển thị họ tên
        var sdtList = allAccounts.Where(t => !string.IsNullOrEmpty(t.SoDienThoai)).Select(t => t.SoDienThoai!).Distinct().ToList();
        var benhNhans = await _db.BenhNhans.Where(b => sdtList.Contains(b.SoDienThoai)).ToListAsync();
        ViewBag.BenhNhanMap = benhNhans.GroupBy(b => b.SoDienThoai).ToDictionary(g => g.Key, g => g.First().TenBenhNhan);

        List<TaiKhoan> list;
        if (nhom == "BenhNhan")
        {
            ViewData["Title"] = "Tài khoản Bệnh nhân";
            list = allAccounts.Where(t => t.VaiTro?.TenVaiTro == "BenhNhan").ToList();
        }
        else if (nhom == "NhanSu")
        {
            ViewData["Title"] = "Tài khoản Bác sĩ & Lễ tân";
            list = allAccounts.Where(t => t.VaiTro?.TenVaiTro != "BenhNhan").ToList();
        }
        else
        {
            ViewData["Title"] = "Quản lý Tài khoản";
            list = allAccounts;
        }

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> TaoTaiKhoan(string? nhom = null)
    {
        ViewData["Title"]      = "Tạo Tài khoản";
        ViewData["ActiveMenu"] = "TaiKhoan";
        ViewBag.NhomHienTai    = nhom;

        int? preselectedRoleId = null;
        if (nhom == "BenhNhan")
        {
            var role = await _db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "BenhNhan");
            if (role != null) preselectedRoleId = role.MaVaiTro;
        }
        else if (nhom == "NhanSu")
        {
            var role = await _db.VaiTros.FirstOrDefaultAsync(v => v.TenVaiTro == "BacSi");
            if (role != null) preselectedRoleId = role.MaVaiTro;
        }

        await LoadVaiTroListAsync(preselectedRoleId);
        var vm = new TaoTaiKhoanVM();
        if (preselectedRoleId.HasValue) vm.MaVaiTro = preselectedRoleId.Value;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoTaiKhoan(TaoTaiKhoanVM vm, string? nhom = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Tạo Tài khoản";
            ViewBag.NhomHienTai = nhom;
            await LoadVaiTroListAsync(vm.MaVaiTro);
            return View(vm);
        }
        if (await _db.TaiKhoans.AnyAsync(t => t.TenTaiKhoan == vm.TenTaiKhoan))
        {
            ModelState.AddModelError(nameof(vm.TenTaiKhoan), "Tên tài khoản đã tồn tại.");
            ViewBag.NhomHienTai = nhom;
            await LoadVaiTroListAsync(vm.MaVaiTro);
            return View(vm);
        }
        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenTaiKhoan = vm.TenTaiKhoan,
            MatKhau     = BCrypt.Net.BCrypt.HashPassword(vm.MatKhau),
            SoDienThoai = vm.SoDienThoai,
            TrangThai   = "HoatDong",
            MaVaiTro    = vm.MaVaiTro,
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Tạo tài khoản thành công.";
        return RedirectToAction(nameof(TaiKhoan), new { nhom });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaTaiKhoan(int id, string? nhom = null)
    {
        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();
        if (tk.TenTaiKhoan == "admin")
        {
            TempData["Error"] = "Không thể khóa tài khoản admin gốc.";
            return RedirectToAction(nameof(TaiKhoan), new { nhom });
        }
        tk.TrangThai = "BiKhoa";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã khóa tài khoản.";
        return RedirectToAction(nameof(TaiKhoan), new { nhom });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MoKhoaTaiKhoan(int id, string? nhom = null)
    {
        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();
        tk.TrangThai = "HoatDong";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã mở khóa tài khoản.";
        return RedirectToAction(nameof(TaiKhoan), new { nhom });
    }

    // ─── CA TRỰC & KHUNG GIỜ ─────────────────────────────────────────────────
    public async Task<IActionResult> CaTruc(DateOnly? ngay)
    {
        ViewData["Title"]      = "Quản lý Ca trực";
        ViewData["ActiveMenu"] = "CaTruc";
        ngay ??= DateOnly.FromDateTime(DateTime.Today);
        ViewBag.NgayFilter = ngay;
        var list = await _db.CaTrucs
            .Include(c => c.BacSi)
            .Include(c => c.PhongKham)
            .Where(c => c.NgayTruc == ngay)
            .OrderBy(c => c.GioBatDauCa)
            .ToListAsync();
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> TaoCaTruc()
    {
        ViewData["Title"]      = "Tạo Ca trực";
        ViewData["ActiveMenu"] = "CaTruc";
        await LoadBacSiListAsync();
        await LoadPhongKhamListAsync();
        return View(new TaoCaTrucVM { NgayTruc = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoCaTruc(TaoCaTrucVM vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Tạo Ca trực";
            await LoadBacSiListAsync();
            await LoadPhongKhamListAsync();
            return View(vm);
        }

        var loi = await _caTrucService.KiemTraTrungCaAsync(
            vm.MaBacSi, vm.MaPhong, vm.NgayTruc, vm.CaLamViec);
        if (loi != null)
        {
            ModelState.AddModelError("", loi);
            await LoadBacSiListAsync();
            await LoadPhongKhamListAsync();
            return View(vm);
        }

        var caTruc = new CaTruc
        {
            MaBacSi              = vm.MaBacSi,
            MaPhong              = vm.MaPhong,
            NgayTruc             = vm.NgayTruc,
            CaLamViec            = vm.CaLamViec,
            GioBatDauCa          = vm.GioBatDauCa,
            GioKetThucCa         = vm.GioKetThucCa,
            ThoiLuongMoiKhungGio = vm.ThoiLuong,
            TrangThaiCa          = "HoatDong",
        };
        _db.CaTrucs.Add(caTruc);
        await _db.SaveChangesAsync();

        // Sinh khung giờ: service cần đối tượng CaTruc đã có MaCa
        var khungGios = _caTrucService.SinhKhungGio(caTruc, vm.TanSuatKhungDem);
        _db.KhungGios.AddRange(khungGios);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Tạo ca trực thành công: {vm.CaLamViec} ngày {vm.NgayTruc:dd/MM/yyyy} " +
                              $"({khungGios.Count} khung giờ).";
        return RedirectToAction(nameof(CaTruc), new { ngay = vm.NgayTruc });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaCaTruc(int id)
    {
        var ca = await _db.CaTrucs
            .Include(c => c.KhungGios)
            .FirstOrDefaultAsync(c => c.MaCa == id);
        if (ca == null) return NotFound();
        if (ca.KhungGios.Any(k => k.TrangThai == "DaDat"))
        {
            TempData["Error"] = "Không thể xóa ca trực vì có khung giờ đã được đặt lịch.";
            return RedirectToAction(nameof(CaTruc), new { ngay = ca.NgayTruc });
        }
        _db.CaTrucs.Remove(ca);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa ca trực và toàn bộ khung giờ.";
        return RedirectToAction(nameof(CaTruc));
    }

    public async Task<IActionResult> KhungGioDetail(int id)
    {
        var ca = await _db.CaTrucs
            .Include(c => c.BacSi)
            .Include(c => c.PhongKham)
            .Include(c => c.KhungGios)
            .FirstOrDefaultAsync(c => c.MaCa == id);
        if (ca == null) return NotFound();
        ViewData["Title"]      = $"Khung giờ — Ca {ca.CaLamViec} {ca.NgayTruc:dd/MM/yyyy}";
        ViewData["ActiveMenu"] = "CaTruc";
        return View(ca);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaKhungGio(int id, int maCa)
    {
        var loi = await _caTrucService.DoiTrangThaiKhungGioAsync(id, "TamKhoa");
        if (loi != null) TempData["Error"] = loi;
        else TempData["Success"] = "Đã khóa khung giờ.";
        return RedirectToAction(nameof(KhungGioDetail), new { id = maCa });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MoKhungGio(int id, int maCa)
    {
        var loi = await _caTrucService.DoiTrangThaiKhungGioAsync(id, "ConTrong");
        if (loi != null) TempData["Error"] = loi;
        else TempData["Success"] = "Đã mở khung giờ.";
        return RedirectToAction(nameof(KhungGioDetail), new { id = maCa });
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private async Task LoadChuyenKhoaListAsync(int? selected = null)
    {
        var list = await _db.ChuyenKhoas
            .Where(c => c.TrangThai == "HoatDong")
            .OrderBy(c => c.TenChuyenKhoa)
            .ToListAsync();
        ViewBag.ChuyenKhoaList = new SelectList(list, "MaChuyenKhoa", "TenChuyenKhoa", selected);
    }

    private async Task LoadVaiTroListAsync(int? selected = null)
    {
        var list = await _db.VaiTros.OrderBy(v => v.TenVaiTro).ToListAsync();
        ViewBag.VaiTroList = new SelectList(list, "MaVaiTro", "TenVaiTro", selected);
    }

    private async Task LoadBacSiListAsync(int? selected = null)
    {
        var list = await _db.BacSis
            .Where(b => b.TrangThai == "HoatDong")
            .OrderBy(b => b.HoTenBacSi)
            .ToListAsync();
        ViewBag.BacSiList = new SelectList(list, "MaBacSi", "HoTenBacSi", selected);
    }

    private async Task LoadPhongKhamListAsync(int? selected = null)
    {
        var list = await _db.PhongKhams
            .Where(p => p.TrangThai == "HoatDong")
            .Include(p => p.ChuyenKhoa)
            .OrderBy(p => p.TenPhong)
            .ToListAsync();
        ViewBag.PhongKhamList = new SelectList(
            list.Select(p => new
            {
                p.MaPhong,
                Ten = $"{p.TenPhong} ({p.ChuyenKhoa?.TenChuyenKhoa})"
            }),
            "MaPhong", "Ten", selected);
    }
}
