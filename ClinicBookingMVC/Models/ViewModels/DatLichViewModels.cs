using System.ComponentModel.DataAnnotations;
using ClinicBookingMVC.Models.Entities;
using ClinicBookingMVC.Services;

namespace ClinicBookingMVC.Models.ViewModels;

// ── Bước đặt lịch ────────────────────────────────────────────────────────────

/// <summary>
/// VM trả về cho bước chọn Bác sĩ theo Chuyên khoa + Ngày
/// </summary>
public class ChonBacSiVM
{
    public int MaChuyenKhoa { get; set; }
    public string TenChuyenKhoa { get; set; } = "";
    public List<BacSiTrongCaVM> DanhSachBacSi { get; set; } = new();
}

public class BacSiTrongCaVM
{
    public int MaBacSi { get; set; }
    public string HoTenBacSi { get; set; } = "";
    public string? HocVi { get; set; }
    public int SoSlotTrong { get; set; }
}

/// <summary>
/// VM lưới slot theo ngày + bác sĩ
/// </summary>
public class LuoiKhungGioVM
{
    public int MaBacSi { get; set; }
    public string HoTenBacSi { get; set; } = "";
    public string? HocVi { get; set; }
    public string TenChuyenKhoa { get; set; } = "";
    public string TenPhong { get; set; } = "";
    public DateOnly NgayKham { get; set; }
    public List<SlotVM> Slots { get; set; } = new();
}

public class SlotVM
{
    public int MaKhungGio { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string TrangThai { get; set; } = "ConTrong"; // ConTrong, DaDat, TamKhoa
    public string LoaiKhungGio { get; set; } = "DatHen";
    public string? PhienBanDongBase64 { get; set; }  // RowVersion → gửi kèm form
}

/// <summary>
/// Form xác nhận đặt lịch (bước 4)
/// </summary>
public class XacNhanDatLichVM
{
    [Required]
    public int MaKhungGio { get; set; }

    public string? PhienBanDongBase64 { get; set; }

    [Required(ErrorMessage = "Nhập họ tên bệnh nhân.")]
    [MaxLength(100)]
    [Display(Name = "Họ tên bệnh nhân")]
    public string TenBenhNhan { get; set; } = "";

    [Required(ErrorMessage = "Nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";

    [Range(1920, 2024, ErrorMessage = "Năm sinh không hợp lệ.")]
    [Display(Name = "Năm sinh")]
    public int? NamSinh { get; set; }

    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; }

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(500)]
    [Display(Name = "Lý do khám sơ bộ")]
    public string? LyDoKham { get; set; }

    [Display(Name = "Nhận nhắc lịch")]
    public bool GuiNhacLich { get; set; } = true;

    // Thông tin hiển thị (readonly, không submit)
    public string? TenBacSi { get; set; }
    public string? TenChuyenKhoa { get; set; }
    public string? TenPhong { get; set; }
    public DateOnly NgayKham { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
}

/// <summary>
/// Phiếu hẹn điện tử hiển thị sau khi đặt thành công
/// </summary>
public class PhieuHenDienTuVM
{
    public string MaDatHen { get; set; } = "";
    public string TrangThaiHen { get; set; } = "";
    public string TenBenhNhan { get; set; } = "";
    public string SoDienThoaiAnPhan { get; set; } = ""; // 091***678
    public string TenBacSi { get; set; } = "";
    public string TenChuyenKhoa { get; set; } = "";
    public string TenPhong { get; set; } = "";
    public DateOnly NgayKham { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public TimeOnly GioKetThuc { get; set; }
    public string? LyDoKham { get; set; }
    public DateTime ThoiGianTao { get; set; }
    public bool CoCanhBao { get; set; }
}

// ── Tra cứu ──────────────────────────────────────────────────────────────────

public class TraCuuInputVM
{
    [Display(Name = "Mã đặt hẹn hoặc Số điện thoại")]
    public string? TuKhoa { get; set; }
}

public class KetQuaTraCuuVM
{
    public string? TuKhoa { get; set; }
    public List<PhieuHenTraCuuVM> DanhSachPhieu { get; set; } = new();
}

public class PhieuHenTraCuuVM
{
    public int MaPhieuHen { get; set; }
    public string MaDatHen { get; set; } = "";
    public string TrangThaiHen { get; set; } = "";
    public string TenBenhNhan { get; set; } = "";
    public string SoDienThoaiAnPhan { get; set; } = "";
    public string TenBacSi { get; set; } = "";
    public string TenChuyenKhoa { get; set; } = "";
    public string TenPhong { get; set; } = "";
    public DateOnly NgayKham { get; set; }
    public TimeOnly GioBatDau { get; set; }
    public DateTime ThoiGianTao { get; set; }

    // Hàng đợi (nếu đã check-in hôm nay)
    public int? SoThuTuBenhNhan { get; set; }
    public HangDoiProgressDto? TienDoHangDoi { get; set; }

    // Quyền hủy
    public bool CoTheHuy { get; set; }
    public string? LyDoKhongTheHuy { get; set; }

    // Phòng (để poll AJAX)
    public int MaPhong { get; set; }
}
