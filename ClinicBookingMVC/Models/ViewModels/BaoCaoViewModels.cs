using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Models.ViewModels;

public class BaoCaoFilterVM
{
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public int? MaChuyenKhoa { get; set; }
    public int? MaBacSi { get; set; }
}

public class BaoCaoKpiVM
{
    // 1. Tỷ lệ vắng hẹn
    public double TyLeVangHen { get; set; }
    public int SoPhieuVangHen { get; set; }
    public int TongPhieuVangHenMauSo { get; set; } // DaXacNhan + DaCheckIn + DaKham + VangHen

    // 2. Tỷ lệ hoàn thành khám
    public double TyLeHoanThanh { get; set; }
    public int SoPhieuDaKham { get; set; }
    public int TongPhieuDatHenTrongKy { get; set; }

    // 3. Thời gian chờ khám trung bình (phút)
    public double ThoiGianChoTrungBinh { get; set; }
    public int SoLuotDaGoiSo { get; set; }

    // 4. Tỷ lệ lấp đầy khung giờ (%)
    public double TyLeLapDay { get; set; }
    public int SoKhungGioDaDat { get; set; }
    public int TongKhungGioDatHen { get; set; }
}

public class LuuLuongGioVM
{
    public string KhungGio { get; set; } = string.Empty; // e.g. "07:00 - 08:00"
    public int Gio { get; set; } // 7, 8, 9...
    public int SoLuongBenhNhan { get; set; }
}

public class VangHenBacSiVM
{
    public int MaBacSi { get; set; }
    public string HoTenBacSi { get; set; } = string.Empty;
    public string TenChuyenKhoa { get; set; } = string.Empty;
    public int TongHen { get; set; }
    public int SoVangHen { get; set; }
    public double TyLeVangHen { get; set; }
}

public class BenhNhanCanhBaoVM
{
    public int MaBenhNhan { get; set; }
    public string TenBenhNhan { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public int SoLanVangHen { get; set; }
    public bool CoCanhBao { get; set; }
    public DateTime? LanVangGanNhat { get; set; }
}

public class BaoCaoDashboardVM
{
    public BaoCaoFilterVM Filter { get; set; } = new();
    public BaoCaoKpiVM KPI { get; set; } = new();
    public List<LuuLuongGioVM> LuuLuongTheoGio { get; set; } = new();
    public List<VangHenBacSiVM> VangHenTheoBacSi { get; set; } = new();
    public List<BenhNhanCanhBaoVM> DanhSachCanhBao { get; set; } = new();
    public List<ChuyenKhoa> DanhSachChuyenKhoa { get; set; } = new();
    public List<BacSi> DanhSachBacSi { get; set; } = new();
}
