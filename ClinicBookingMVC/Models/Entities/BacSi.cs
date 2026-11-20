using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingMVC.Models.Entities;

[Table("BacSi")]
public class BacSi
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaBacSi { get; set; }

    [Required]
    [MaxLength(100)]
    public string HoTenBacSi { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? HocVi { get; set; } // BS, ThS, TS, PGS, GS

    [MaxLength(15)]
    public string? SoDienThoai { get; set; }

    public int MaChuyenKhoa { get; set; }

    public int MaTaiKhoan { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong, TamNgung, NghiViec

    // Navigation
    [ForeignKey(nameof(MaChuyenKhoa))]
    public ChuyenKhoa ChuyenKhoa { get; set; } = null!;

    [ForeignKey(nameof(MaTaiKhoan))]
    public TaiKhoan TaiKhoan { get; set; } = null!;

    public ICollection<CaTruc> CaTrucs { get; set; } = new List<CaTruc>();
}
