using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACN_HTQL.Models.Entities;

[Table("PhieuSoThuTu")]
public class PhieuSoThuTu
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaPhieu { get; set; }

    public int SoThuTu { get; set; }

    public DateOnly NgayCapSo { get; set; }

    [Required]
    [MaxLength(20)]
    public string LoaiKhach { get; set; } = "ThongThuong"; // UuTien, ThongThuong

    public int MucUuTien { get; set; } = 3; // 1=Cao, 2=Trung, 3=Thuong

    public TimeOnly? GioHen { get; set; } // NULL nếu vãng lai

    public DateTime ThoiGianCheckIn { get; set; }

    public DateTime? ThoiGianGoiSo { get; set; }

    public DateTime? ThoiGianBatDauKham { get; set; }

    public DateTime? ThoiGianHoanTat { get; set; }

    public int SoLanGoi { get; set; } = 0; // 0-3

    [Required]
    [MaxLength(20)]
    public string TrangThaiPhieu { get; set; } = "DangCho";
    // DangCho, DangGoi, DangKham, QuaLuot, HoanTat

    public int MaPhong { get; set; }

    public int MaBenhNhan { get; set; }

    public int? MaPhieuHen { get; set; } // NULL = vãng lai

    // Navigation
    [ForeignKey(nameof(MaPhong))]
    public PhongKham PhongKham { get; set; } = null!;

    [ForeignKey(nameof(MaBenhNhan))]
    public BenhNhan BenhNhan { get; set; } = null!;

    [ForeignKey(nameof(MaPhieuHen))]
    public PhieuDatHen? PhieuDatHen { get; set; }
}
