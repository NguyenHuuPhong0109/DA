using Microsoft.EntityFrameworkCore;
using DACN_HTQL.Data;
using DACN_HTQL.Models.Entities;

namespace DACN_HTQL.Services;

/// <summary>
/// Dịch vụ tự động sinh KhungGio từ một CaTruc.
/// </summary>
public class CaTrucService
{
    private readonly ClinicDbContext _db;

    public CaTrucService(ClinicDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Kiểm tra xem ca trực có bị trùng không (theo Bác sĩ hoặc Phòng).
    /// </summary>
    public async Task<string?> KiemTraTrungCaAsync(int maBacSi, int maPhong,
        DateOnly ngayTruc, string caLamViec, int? loaiTruMaCa = null)
    {
        var query = _db.CaTrucs.Where(c =>
            c.NgayTruc == ngayTruc && c.CaLamViec == caLamViec);

        if (loaiTruMaCa.HasValue)
            query = query.Where(c => c.MaCa != loaiTruMaCa.Value);

        if (await query.AnyAsync(c => c.MaBacSi == maBacSi))
            return "Bác sĩ này đã có ca trực trong buổi và ngày được chọn.";

        if (await query.AnyAsync(c => c.MaPhong == maPhong))
            return "Phòng khám này đã có ca trực trong buổi và ngày được chọn.";

        return null; // Không trùng
    }

    /// <summary>
    /// Sinh toàn bộ KhungGio cho một CaTruc vừa tạo.
    /// tanSuatKhungDem = 0: không chèn khung đệm.
    /// tanSuatKhungDem = N: cứ N slot DatHen thì chèn 1 KhungDem.
    /// </summary>
    public List<KhungGio> SinhKhungGio(CaTruc ca, int tanSuatKhungDem = 3)
    {
        var slots = new List<KhungGio>();
        var current = ca.GioBatDauCa;
        int demDatHen = 0;

        while (current.AddMinutes(ca.ThoiLuongMoiKhungGio) <= ca.GioKetThucCa)
        {
            var next = current.AddMinutes(ca.ThoiLuongMoiKhungGio);

            // Chèn KhungDem trước slot DatHen thứ (N+1) nếu cần
            if (tanSuatKhungDem > 0 && demDatHen > 0 && demDatHen % tanSuatKhungDem == 0)
            {
                // Khung đệm: trùng vị trí thời gian nhưng loại khác
                // Thực tế chèn khung đệm VÀO TRƯỚC slot hiện tại
                slots.Add(new KhungGio
                {
                    MaCa       = ca.MaCa,
                    GioBatDau  = current,
                    GioKetThuc = next,
                    LoaiKhungGio = "KhungDem",
                    TrangThai  = "ConTrong",
                });
                // Reset đếm để không lặp liên tục
                demDatHen = 0;
                current = next;
                continue;
            }

            slots.Add(new KhungGio
            {
                MaCa       = ca.MaCa,
                GioBatDau  = current,
                GioKetThuc = next,
                LoaiKhungGio = "DatHen",
                TrangThai  = "ConTrong",
            });

            demDatHen++;
            current = next;
        }

        return slots;
    }

    /// <summary>
    /// Khóa / Mở khóa một khung giờ. Trả về thông báo lỗi nếu không thể thực hiện.
    /// </summary>
    public async Task<string?> DoiTrangThaiKhungGioAsync(int maKhungGio, string trangThaiMoi)
    {
        var kg = await _db.KhungGios
            .Include(k => k.PhieuDatHen)
            .FirstOrDefaultAsync(k => k.MaKhungGio == maKhungGio);

        if (kg == null) return "Khung giờ không tồn tại.";

        // Không cho khóa slot đã có người đặt hẹn còn hiệu lực
        if (trangThaiMoi == "TamKhoa"
            && kg.PhieuDatHen != null
            && kg.PhieuDatHen.TrangThaiHen != "DaHuy")
        {
            return "Không thể khóa khung giờ đã có lịch hẹn chưa xử lý. Vui lòng xử lý phiếu hẹn trước.";
        }

        kg.TrangThai = trangThaiMoi;
        await _db.SaveChangesAsync();
        return null;
    }

    /// <summary>
    /// Đếm số khung giờ còn trống của một ca trực.
    /// </summary>
    public async Task<int> DemKhungGioTrongAsync(int maCa)
        => await _db.KhungGios.CountAsync(k => k.MaCa == maCa
            && k.LoaiKhungGio == "DatHen"
            && k.TrangThai == "ConTrong");
}
