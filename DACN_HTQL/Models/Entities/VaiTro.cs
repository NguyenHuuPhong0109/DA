using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN_HTQL.Models.Entities;

[Table("VaiTro")]
public class VaiTro
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaVaiTro { get; set; }

    [Required]
    [MaxLength(50)]
    public string TenVaiTro { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MoTa { get; set; }

    // Navigation
    public ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
