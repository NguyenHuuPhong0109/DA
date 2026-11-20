using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingMVC.Models.Entities;

[Table("ChuyenKhoa")]
public class ChuyenKhoa
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaChuyenKhoa { get; set; }

    [Required]
    [MaxLength(100)]
    public string TenChuyenKhoa { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MoTa { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong, TamNgung

    // Navigation
    public ICollection<BacSi> BacSis { get; set; } = new List<BacSi>();
    public ICollection<PhongKham> PhongKhams { get; set; } = new List<PhongKham>();
}
