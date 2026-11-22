using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN_HTQL.Models.Entities;

[Table("PhieuDatHen")]
public class PhieuDatHen
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieuHen { get; set; }

    [Required]
    [MaxLength(20)]
    public string MaDatHen { get; set; } = string.Empty; // A-001, A-002...

    [Required]
    [MaxLength(30)]
    public string KenhDatLich { get; set; } = "Online"; // Online, TaiQuay, DienThoai

    [MaxLength(500)]
    public string? LyDoKhamSoBo { get; set; }

    public DateTime ThoiGianTaoPhieu { get; set; } = DateTime.Now;

    public bool GuiNhacLich { get; set; } = true;

    [Required]
    [MaxLength(20)]
    public string TrangThaiHen { get; set; } = "ChoXacNhan";
    // ChoXacNhan, DaXacNhan, DaCheckIn, DaKham, DaHuy, VangHen

    public int MaBenhNhan { get; set; }

    public int MaNguoiTao { get; set; } // FK -> TaiKhoan (NOT NULL)

    public int MaKhungGio { get; set; }

    // Navigation
    [ForeignKey(nameof(MaBenhNhan))]
    public BenhNhan BenhNhan { get; set; } = null!;

    [ForeignKey(nameof(MaNguoiTao))]
    public TaiKhoan NguoiTao { get; set; } = null!;

    [ForeignKey(nameof(MaKhungGio))]
    public KhungGio KhungGio { get; set; } = null!;

    public PhieuSoThuTu? PhieuSoThuTu { get; set; }
}
