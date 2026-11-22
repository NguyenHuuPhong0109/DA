using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN_HTQL.Models.Entities;

[Table("TaiKhoan")]
public class TaiKhoan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaTaiKhoan { get; set; }

    [Required]
    [MaxLength(100)]
    public string TenTaiKhoan { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string MatKhau { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? SoDienThoai { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong, KhoaTam, Khoa

    public int MaVaiTro { get; set; }

    // Navigation
    [ForeignKey(nameof(MaVaiTro))]
    public VaiTro VaiTro { get; set; } = null!;

    public BacSi? BacSi { get; set; }
    public ICollection<PhieuDatHen> PhieuDatHensNguoiTao { get; set; } = new List<PhieuDatHen>();
}
