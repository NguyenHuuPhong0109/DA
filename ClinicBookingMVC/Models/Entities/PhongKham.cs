using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingMVC.Models.Entities;

[Table("PhongKham")]
public class PhongKham
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhong { get; set; }

    [Required]
    [MaxLength(100)]
    public string TenPhong { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ViTri { get; set; }

    public int MaChuyenKhoa { get; set; }

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong, TamNgung, BaoTri

    // Navigation
    [ForeignKey(nameof(MaChuyenKhoa))]
    public ChuyenKhoa ChuyenKhoa { get; set; } = null!;

    public ICollection<CaTruc> CaTrucs { get; set; } = new List<CaTruc>();
    public ICollection<PhieuSoThuTu> PhieuSoThuTus { get; set; } = new List<PhieuSoThuTu>();
}
