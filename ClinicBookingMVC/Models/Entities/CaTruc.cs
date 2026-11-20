using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingMVC.Models.Entities;

[Table("CaTruc")]
public class CaTruc
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaCa { get; set; }

    public int MaBacSi { get; set; }

    public int MaPhong { get; set; }

    public DateOnly NgayTruc { get; set; }

    [Required]
    [MaxLength(20)]
    public string CaLamViec { get; set; } = string.Empty; // Sang, Chieu, Toi

    public TimeOnly GioBatDauCa { get; set; }

    public TimeOnly GioKetThucCa { get; set; }

    public int ThoiLuongMoiKhungGio { get; set; } = 30; // phút

    [Required]
    [MaxLength(20)]
    public string TrangThaiCa { get; set; } = "MoKham"; // MoKham, TamDong, DaKham, Huy

    // Navigation
    [ForeignKey(nameof(MaBacSi))]
    public BacSi BacSi { get; set; } = null!;

    [ForeignKey(nameof(MaPhong))]
    public PhongKham PhongKham { get; set; } = null!;

    public ICollection<KhungGio> KhungGios { get; set; } = new List<KhungGio>();
}
