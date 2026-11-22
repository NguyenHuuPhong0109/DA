using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN_HTQL.Models.Entities;

[Table("BenhNhan")]
public class BenhNhan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaBenhNhan { get; set; }

    [Required]
    [MaxLength(100)]
    public string TenBenhNhan { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    public string SoDienThoai { get; set; } = string.Empty;

    public int? NamSinh { get; set; }

    [MaxLength(10)]
    public string? GioiTinh { get; set; } // Nam, Nu, Khac

    [MaxLength(100)]
    public string? Email { get; set; }

    public int SoLanVangHen { get; set; } = 0;

    // Computed column - will be configured via Fluent API
    // CoCanhBao = CAST(CASE WHEN SoLanVangHen >= 2 THEN 1 ELSE 0 END AS BIT)
    public bool CoCanhBao { get; set; }

    // Navigation
    public ICollection<PhieuDatHen> PhieuDatHens { get; set; } = new List<PhieuDatHen>();
    public ICollection<PhieuSoThuTu> PhieuSoThuTus { get; set; } = new List<PhieuSoThuTu>();
}
