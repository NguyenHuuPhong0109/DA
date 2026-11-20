using System.Data;
using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Data;
using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Services;

/// <summary>
/// Dịch vụ đặt lịch — xử lý toàn bộ nghiệp vụ PhieuDatHen.
/// </summary>
public class DatLichService
{
    private readonly ClinicDbContext _db;

    public DatLichService(ClinicDbContext db) { _db = db; }

    // ─── BR-01: Kiểm tra thời điểm đặt hợp lệ ──────────────────────────────
    /// <summary>
    /// Trả null nếu hợp lệ, trả chuỗi lỗi tiếng Việt nếu không hợp lệ.
    /// Yêu cầu: thời điểm khám phải cách hiện tại tối thiểu 30 phút.
    /// </summary>
    public string? KiemTraThoiDiemDatHen(DateOnly ngayKham, TimeOnly gioKham)
    {
        var thoiDiemKham = ngayKham.ToDateTime(gioKham);
        if (thoiDiemKham < DateTime.Now.AddMinutes(30))
            return "Chỉ được đặt lịch trước giờ khám ít nhất 30 phút.";
        return null;
    }

    // ─── BR-01: Kiểm tra điều kiện hủy ──────────────────────────────────────
    /// <summary>
    /// Bệnh nhân chỉ được tự hủy khi còn ít nhất 2 giờ trước giờ hẹn.
    /// </summary>
    public string? KiemTraDieuKienHuy(PhieuDatHen phieu)
    {
        if (phieu.TrangThaiHen == "DaHuy")
            return "Phiếu hẹn này đã được hủy trước đó.";
        if (phieu.TrangThaiHen is "DaKham" or "DaCheckIn")
            return "Không thể hủy phiếu hẹn đã check-in hoặc đã khám.";

        var kg = phieu.KhungGio;
        var ca = kg.CaTruc;
        var thoiDiemHen = ca.NgayTruc.ToDateTime(kg.GioBatDau);
        if (DateTime.Now > thoiDiemHen.AddHours(-2))
            return "Chỉ được hủy lịch trước giờ hẹn ít nhất 2 giờ.";

        return null;
    }

    // ─── Sinh MaDatHen ───────────────────────────────────────────────────────
    public async Task<string> SinhMaDatHenAsync(DateOnly ngay)
    {
        var prefix = $"BK{ngay:yyMMdd}-";
        var soLon = await _db.PhieuDatHens
            .Where(p => p.MaDatHen.StartsWith(prefix))
            .CountAsync();
        return $"{prefix}{(soLon + 1):D3}";
    }

    // ─── BR-02: Kiểm tra trùng lịch cùng SoDienThoai ────────────────────────
    /// <summary>
    /// Cùng SoDienThoai không có phiếu hiệu lực khác bị giao nhau thời gian trong cùng NgayTruc.
    /// </summary>
    public async Task<string?> KiemTraTrungLichAsync(
        string soDienThoai, int maKhungGio, DateOnly ngayTruc)
    {
        var bn = await _db.BenhNhans
            .FirstOrDefaultAsync(b => b.SoDienThoai == soDienThoai);
        if (bn == null) return null; // bệnh nhân mới, không có lịch cũ

        var coTrung = await _db.PhieuDatHens
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
            .AnyAsync(p =>
                p.MaBenhNhan == bn.MaBenhNhan
                && p.MaKhungGio != maKhungGio
                && p.TrangThaiHen != "DaHuy"
                && p.KhungGio.CaTruc.NgayTruc == ngayTruc);

        return coTrung
            ? "Bạn đã có lịch khám khác trong ngày này. Vui lòng hủy lịch cũ hoặc chọn ngày khác."
            : null;
    }

    // ─── Lấy / tạo BenhNhan ──────────────────────────────────────────────────
    /// <summary>
    /// Trả về BenhNhan hiện có theo SoDienThoai, hoặc tạo mới.
    /// </summary>
    public async Task<BenhNhan> LayHoacTaoBenhNhanAsync(
        string soDienThoai, string tenBenhNhan,
        int? namSinh = null, string? gioiTinh = null, string? email = null)
    {
        var bn = await _db.BenhNhans
            .FirstOrDefaultAsync(b => b.SoDienThoai == soDienThoai);
        if (bn != null) return bn;

        bn = new BenhNhan
        {
            TenBenhNhan = tenBenhNhan,
            SoDienThoai = soDienThoai,
            NamSinh     = namSinh,
            GioiTinh    = gioiTinh,
            Email       = email,
        };
        _db.BenhNhans.Add(bn);
        await _db.SaveChangesAsync();
        return bn;
    }

    // ─── Đặt lịch chính (BR-02 + BR-06) ─────────────────────────────────────
    public async Task<(bool Ok, string? LoiMsg, PhieuDatHen? Phieu)> DatLichAsync(
        int maKhungGio,
        byte[]? phienBanDong,
        int maBenhNhan,
        int maNguoiTao,
        string lyDoKham,
        bool guiNhacLich)
    {
        // Mở transaction Serializable để chống race condition
        await using var tx = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);
        try
        {
            // 1. Lock & kiểm tra khung giờ
            var kg = await _db.KhungGios
                .Include(k => k.CaTruc)
                .FirstOrDefaultAsync(k => k.MaKhungGio == maKhungGio);

            if (kg == null)
                return (false, "Khung giờ không tồn tại.", null);

            if (kg.LoaiKhungGio != "DatHen")
                return (false, "Khung giờ này không dành cho đặt hẹn trực tuyến.", null);

            if (kg.TrangThai != "ConTrong")
                return (false, "Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác.", null);

            // 2. Kiểm tra RowVersion (optimistic concurrency từ client)
            if (phienBanDong != null && !kg.PhienBanDong!.SequenceEqual(phienBanDong))
                return (false, "Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác.", null);

            // 3. Kiểm tra thời điểm hợp lệ (BR-01)
            var loiThoiDiem = KiemTraThoiDiemDatHen(
                kg.CaTruc.NgayTruc, kg.GioBatDau);
            if (loiThoiDiem != null)
                return (false, loiThoiDiem, null);

            // 4. Kiểm tra trùng lịch bệnh nhân (BR-02)
            var bn = await _db.BenhNhans.FindAsync(maBenhNhan);
            if (bn != null)
            {
                var loiTrung = await KiemTraTrungLichAsync(
                    bn.SoDienThoai, maKhungGio, kg.CaTruc.NgayTruc);
                if (loiTrung != null)
                    return (false, loiTrung, null);
            }

            // 5. Sinh MaDatHen
            var maDatHen = await SinhMaDatHenAsync(kg.CaTruc.NgayTruc);

            // 6. BR-06: trạng thái ban đầu dựa vào CoCanhBao
            bn = await _db.BenhNhans.FindAsync(maBenhNhan);
            var trangThaiHen = (bn?.CoCanhBao == true) ? "ChoXacNhan" : "DaXacNhan";

            // 7. Tạo PhieuDatHen
            var phieu = new PhieuDatHen
            {
                MaDatHen        = maDatHen,
                KenhDatLich     = "Online",
                LyDoKhamSoBo    = lyDoKham,
                ThoiGianTaoPhieu = DateTime.Now,
                GuiNhacLich     = guiNhacLich,
                TrangThaiHen    = trangThaiHen,
                MaBenhNhan      = maBenhNhan,
                MaNguoiTao      = maNguoiTao,
                MaKhungGio      = maKhungGio,
            };
            _db.PhieuDatHens.Add(phieu);

            // 8. Cập nhật trạng thái KhungGio → DaDat
            kg.TrangThai = "DaDat";

            await _db.SaveChangesAsync(); // có thể throw DbUpdateConcurrencyException
            await tx.CommitAsync();

            return (true, null, phieu);
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            return (false,
                "Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác.",
                null);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException?.Message.Contains("UX_KhungGio_HieuLuc") == true ||
            ex.InnerException?.Message.Contains("unique") == true)
        {
            await tx.RollbackAsync();
            return (false,
                "Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác.",
                null);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return (false, $"Lỗi hệ thống: {ex.Message}", null);
        }
    }

    // ─── Hủy lịch (BR-03) ────────────────────────────────────────────────────
    public async Task<(bool Ok, string? LoiMsg)> HuyLichAsync(
        int maPhieuHen, int maTaiKhoanYeuCau)
    {
        var phieu = await _db.PhieuDatHens
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
            .FirstOrDefaultAsync(p => p.MaPhieuHen == maPhieuHen);

        if (phieu == null)
            return (false, "Phiếu hẹn không tồn tại.");

        // Chỉ cho hủy nếu là người tạo hoặc Admin/LeTan
        if (phieu.MaNguoiTao != maTaiKhoanYeuCau)
        {
            var tk = await _db.TaiKhoans.Include(t => t.VaiTro)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoanYeuCau);
            if (tk?.VaiTro.TenVaiTro is not ("Admin" or "LeTan"))
                return (false, "Bạn không có quyền hủy phiếu hẹn này.");
        }

        var loiHuy = KiemTraDieuKienHuy(phieu);
        if (loiHuy != null)
            return (false, loiHuy);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            phieu.TrangThaiHen       = "DaHuy";
            phieu.KhungGio.TrangThai = "ConTrong"; // trả lại slot
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null);
        }
        catch
        {
            await tx.RollbackAsync();
            return (false, "Hủy lịch thất bại. Vui lòng thử lại.");
        }
    }

    // ─── AJAX: Tra cứu BenhNhan nhanh theo SoDienThoai ──────────────────────
    public async Task<object?> TraCuuBenhNhanAsync(string soDienThoai, bool leTanMode)
    {
        var bn = await _db.BenhNhans
            .FirstOrDefaultAsync(b => b.SoDienThoai == soDienThoai);
        if (bn == null) return null;

        if (leTanMode)
        {
            return new
            {
                bn.MaBenhNhan,
                bn.TenBenhNhan,
                bn.SoDienThoai,
                bn.NamSinh,
                bn.GioiTinh,
                bn.Email,
                bn.CoCanhBao,
            };
        }
        // Chế độ bệnh nhân tự đặt: chỉ trả tên (không lộ dữ liệu nhạy cảm)
        return new
        {
            bn.MaBenhNhan,
            TenBenhNhan = bn.TenBenhNhan, // để auto-fill form
            CoCanhBao   = bn.CoCanhBao,
        };
    }

    // ─── Lấy tiến độ hàng đợi theo PhòngKhám hôm nay ───────────────────────
    public async Task<HangDoiProgressDto?> LayTienDoHangDoiAsync(
        int maPhong, int? soThuTuBenhNhan)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var phieuHoatDong = await _db.PhieuSoThuTus
            .Where(p => p.MaPhong == maPhong
                     && p.NgayCapSo == today
                     && p.TrangThaiPhieu != "HoanTat"
                     && p.TrangThaiPhieu != "QuaLuot")
            .OrderBy(p => p.MucUuTien)
            .ThenBy(p => p.SoThuTu)
            .ToListAsync();

        var dangGoi = phieuHoatDong.FirstOrDefault(p => p.TrangThaiPhieu == "DangGoi");
        var dangKham = phieuHoatDong.FirstOrDefault(p => p.TrangThaiPhieu == "DangKham");

        int? soCuaBan = soThuTuBenhNhan;
        int soCho = soCuaBan.HasValue
            ? phieuHoatDong.Count(p =>
                (p.MucUuTien < (phieuHoatDong.FirstOrDefault(x => x.SoThuTu == soCuaBan.Value)?.MucUuTien ?? 3)
                || (p.MucUuTien == (phieuHoatDong.FirstOrDefault(x => x.SoThuTu == soCuaBan.Value)?.MucUuTien ?? 3)
                    && p.SoThuTu < soCuaBan.Value))
                && p.TrangThaiPhieu == "DangCho")
            : 0;

        return new HangDoiProgressDto
        {
            SoThuTuDangGoi  = dangGoi?.SoThuTu,
            SoThuTuDangKham = dangKham?.SoThuTu,
            SoCuaBan        = soCuaBan,
            SoBenhNhanChoPhamTruoc = soCho,
            TongDangCho     = phieuHoatDong.Count(p => p.TrangThaiPhieu == "DangCho"),
        };
    }
}

public class HangDoiProgressDto
{
    public int? SoThuTuDangGoi     { get; set; }
    public int? SoThuTuDangKham    { get; set; }
    public int? SoCuaBan           { get; set; }
    public int  SoBenhNhanChoPhamTruoc { get; set; }
    public int  TongDangCho        { get; set; }
}
