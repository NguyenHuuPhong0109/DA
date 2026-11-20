using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicBookingMVC.Models.Entities;

[Table("KhungGio")]
public class KhungGio
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaKhungGio { get; set; }

    public int MaCa { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    [Required]
    [MaxLength(20)]
    public string LoaiKhungGio { get; set; } = "DatHen"; // DatHen, KhungDem

    [Required]
    [MaxLength(20)]
    public string TrangThai { get; set; } = "ConTrong"; // ConTrong, DaDat, TamKhoa

    [Timestamp]
    public byte[]? PhienBanDong { get; set; }

    // Navigation
    [ForeignKey(nameof(MaCa))]
    public CaTruc CaTruc { get; set; } = null!;

    public PhieuDatHen? PhieuDatHen { get; set; }
}
