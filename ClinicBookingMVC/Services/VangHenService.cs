using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClinicBookingMVC.Data;

namespace ClinicBookingMVC.Services;

public class VangHenService
{
    private readonly ClinicDbContext _db;
    private readonly ILogger<VangHenService> _logger;

    public VangHenService(ClinicDbContext db, ILogger<VangHenService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Quét tự động các phiếu hẹn:
    /// 1. Tự động chuyển VangHen nếu quá 30 phút so với giờ hẹn mà chưa Check-in (BR-05).
    /// 2. Gửi nhắc lịch cho các lịch hẹn ngày mai chưa được nhắc.
    /// </summary>
    public async Task<(int SoVangHen, int SoNhacLich)> QuetVaXuLyAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var tomorrow = today.AddDays(1);

        int soVangHen = 0;
        int soNhacLich = 0;

        // 1. Quét vắng hẹn: Phiếu ở trạng thái ChoXacNhan hoặc DaXacNhan
        // Quá 30 phút so với giờ bắt đầu khám mà chưa Check-in
        var phieuQuaHan = await _db.PhieuDatHens
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
            .Include(p => p.BenhNhan)
            .Where(p => (p.TrangThaiHen == "ChoXacNhan" || p.TrangThaiHen == "DaXacNhan")
                     && p.KhungGio != null
                     && p.KhungGio.CaTruc != null)
            .ToListAsync(cancellationToken);

        foreach (var p in phieuQuaHan)
        {
            var thoiDiemHen = p.KhungGio.CaTruc.NgayTruc.ToDateTime(p.KhungGio.GioBatDau);
            if (now > thoiDiemHen.AddMinutes(30))
            {
                p.TrangThaiHen = "VangHen";
                if (p.BenhNhan != null)
                {
                    p.BenhNhan.SoLanVangHen += 1;
                    // SQL Server tính CoCanhBao tự động, nhưng ta có thể cập nhật cờ local nếu cần
                    p.BenhNhan.CoCanhBao = p.BenhNhan.SoLanVangHen >= 2;
                }
                soVangHen++;
                _logger.LogWarning("[VangHenService] Phiếu hẹn {MaDatHen} của bệnh nhân {TenBN} đã quá hạn 30 phút -> Đánh dấu VangHen (Số lần vắng: {SoLan})",
                    p.MaDatHen, p.BenhNhan?.TenBenhNhan, p.BenhNhan?.SoLanVangHen);
            }
        }

        // 2. Quét nhắc lịch: Phiếu diễn ra vào ngày mai có GuiNhacLich == false
        var phieuNhacLich = await _db.PhieuDatHens
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
            .Include(p => p.BenhNhan)
            .Where(p => p.GuiNhacLich == false
                     && (p.TrangThaiHen == "ChoXacNhan" || p.TrangThaiHen == "DaXacNhan")
                     && p.KhungGio != null
                     && p.KhungGio.CaTruc != null
                     && p.KhungGio.CaTruc.NgayTruc == tomorrow)
            .ToListAsync(cancellationToken);

        foreach (var p in phieuNhacLich)
        {
            p.GuiNhacLich = true;
            soNhacLich++;
            _logger.LogInformation("[VangHenService - NHẮC LỊCH] Đã gửi thông báo nhắc lịch cho {TenBN} (SĐT: {SDT}) - Giờ khám: {Gio} ngày {Ngay}",
                p.BenhNhan?.TenBenhNhan, p.BenhNhan?.SoDienThoai, p.KhungGio.GioBatDau.ToString(@"hh\:mm"), tomorrow.ToString("dd/MM/yyyy"));
        }

        if (soVangHen > 0 || soNhacLich > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return (soVangHen, soNhacLich);
    }
}
