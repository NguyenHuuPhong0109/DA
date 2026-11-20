using System.ComponentModel.DataAnnotations;

namespace ClinicBookingMVC.Models.ViewModels;

// ── Auth ─────────────────────────────────────────────────────────────────────
public class DangNhapVM
{
    [Required(ErrorMessage = "Nhập tên tài khoản.")]
    [Display(Name = "Tên tài khoản")]
    public string TenTaiKhoan { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool GhiNho { get; set; }

    public string? ReturnUrl { get; set; }
}

public class DangKyVM
{
    [Required(ErrorMessage = "Nhập họ tên.")]
    [Display(Name = "Họ và tên")]
    [MaxLength(100)]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Nhập tên tài khoản.")]
    [MinLength(4, ErrorMessage = "Tên tài khoản ít nhất 4 ký tự.")]
    [Display(Name = "Tên tài khoản")]
    public string TenTaiKhoan { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu.")]
    [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";

    [Required(ErrorMessage = "Nhập lại mật khẩu.")]
    [Compare(nameof(MatKhau), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhanMatKhau { get; set; } = "";
}

// ── Admin CRUD ────────────────────────────────────────────────────────────────
public class ChuyenKhoaVM
{
    public int MaChuyenKhoa { get; set; }

    [Required(ErrorMessage = "Nhập tên chuyên khoa.")]
    [MaxLength(100)]
    [Display(Name = "Tên chuyên khoa")]
    public string TenChuyenKhoa { get; set; } = "";

    [MaxLength(500)]
    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "HoatDong";
}

public class PhongKhamVM
{
    public int MaPhong { get; set; }

    [Required(ErrorMessage = "Nhập tên phòng.")]
    [MaxLength(100)]
    [Display(Name = "Tên phòng")]
    public string TenPhong { get; set; } = "";

    [MaxLength(200)]
    [Display(Name = "Vị trí")]
    public string? ViTri { get; set; }

    [Required(ErrorMessage = "Chọn chuyên khoa.")]
    [Display(Name = "Chuyên khoa")]
    public int MaChuyenKhoa { get; set; }

    [Required]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "HoatDong";
}

public class TaoBacSiVM
{
    [Required(ErrorMessage = "Nhập họ tên bác sĩ.")]
    [MaxLength(100)]
    [Display(Name = "Họ tên bác sĩ")]
    public string HoTenBacSi { get; set; } = "";

    [MaxLength(50)]
    [Display(Name = "Học vị")]
    public string? HocVi { get; set; }

    [Required(ErrorMessage = "Nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";

    [Required(ErrorMessage = "Chọn chuyên khoa.")]
    [Display(Name = "Chuyên khoa")]
    public int MaChuyenKhoa { get; set; }

    [Required(ErrorMessage = "Nhập tên tài khoản.")]
    [MinLength(4, ErrorMessage = "Tên tài khoản ít nhất 4 ký tự.")]
    [Display(Name = "Tên tài khoản đăng nhập")]
    public string TenTaiKhoan { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu.")]
    [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";
}

public class SuaBacSiVM
{
    public int MaBacSi { get; set; }

    [Required(ErrorMessage = "Nhập họ tên bác sĩ.")]
    [MaxLength(100)]
    [Display(Name = "Họ tên bác sĩ")]
    public string HoTenBacSi { get; set; } = "";

    [MaxLength(50)]
    [Display(Name = "Học vị")]
    public string? HocVi { get; set; }

    [Required(ErrorMessage = "Nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";

    [Required(ErrorMessage = "Chọn chuyên khoa.")]
    [Display(Name = "Chuyên khoa")]
    public int MaChuyenKhoa { get; set; }

    [Required]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "HoatDong";
}

public class TaoTaiKhoanVM
{
    [Required(ErrorMessage = "Nhập tên tài khoản.")]
    [MinLength(4, ErrorMessage = "Tối thiểu 4 ký tự.")]
    [Display(Name = "Tên tài khoản")]
    public string TenTaiKhoan { get; set; } = "";

    [Required(ErrorMessage = "Nhập số điện thoại.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";

    [Required(ErrorMessage = "Nhập mật khẩu.")]
    [MinLength(6, ErrorMessage = "Tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";

    [Required(ErrorMessage = "Chọn vai trò.")]
    [Display(Name = "Vai trò")]
    public int MaVaiTro { get; set; }
}

public class TaoCaTrucVM
{    [Required(ErrorMessage = "Chọn bác sĩ.")]
    [Display(Name = "Bác sĩ")]
    public int MaBacSi { get; set; }

    [Required(ErrorMessage = "Chọn phòng khám.")]
    [Display(Name = "Phòng khám")]
    public int MaPhong { get; set; }

    [Required(ErrorMessage = "Chọn ngày trực.")]
    [Display(Name = "Ngày trực")]
    public DateOnly NgayTruc { get; set; }

    [Required(ErrorMessage = "Chọn ca làm việc.")]
    [Display(Name = "Ca làm việc")]
    public string CaLamViec { get; set; } = "Sang";

    [Required]
    [Display(Name = "Giờ bắt đầu")]
    public TimeOnly GioBatDauCa { get; set; } = new TimeOnly(7, 30);

    [Required]
    [Display(Name = "Giờ kết thúc")]
    public TimeOnly GioKetThucCa { get; set; } = new TimeOnly(11, 30);

    [Required]
    [Range(15, 60, ErrorMessage = "Thời lượng từ 15–60 phút.")]
    [Display(Name = "Thời lượng mỗi slot (phút)")]
    public int ThoiLuong { get; set; } = 30;

    [Range(0, 10)]
    [Display(Name = "Tần suất khung đệm (0 = không có)")]
    public int TanSuatKhungDem { get; set; } = 3;
}
