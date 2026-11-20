using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Models.ViewModels;

// ── Lễ tân ───────────────────────────────────────────────────────────────────

public class LeTanIndexVM
{
    public List<PhieuSoThuTu> HangDoi       { get; set; } = new();
    public List<PhieuDatHen>  ChoXacNhan    { get; set; } = new();
    public int?    MaPhongDangChon           { get; set; }
    public DateOnly NgayHienTai              { get; set; }
    // stats
    public int SoDangCho   { get; set; }
    public int SoDangGoi   { get; set; }
    public int SoDangKham  { get; set; }
    public int SoHoanTat   { get; set; }
    public int SoChoXacNhan { get; set; }
}

// ── Bác sĩ ───────────────────────────────────────────────────────────────────

public class BenhNhanTrongCaVM
{
    public int    MaPhieuHen        { get; set; }
    public string MaDatHen          { get; set; } = "";
    public string TenBenhNhan       { get; set; } = "";
    public string SoDienThoaiAnPhan { get; set; } = "";
    public int?   NamSinh           { get; set; }
    public string? GioiTinh         { get; set; }
    public bool   CoCanhBao         { get; set; }
    public string? LyDoKhamSoBo     { get; set; }
    public TimeOnly GioBatDau       { get; set; }
    public string TrangThaiHen      { get; set; } = "";
    public int?   SoThuTu           { get; set; }
    public string? TrangThaiPhieu   { get; set; }
}

public class BacSiCaTrucVM
{
    public int     MaCa         { get; set; }
    public string  CaLamViec    { get; set; } = "";
    public TimeOnly GioBatDauCa { get; set; }
    public TimeOnly GioKetThucCa{ get; set; }
    public string  TenPhong     { get; set; } = "";
    public string  TrangThaiCa  { get; set; } = "";
    public List<BenhNhanTrongCaVM> BenhNhans { get; set; } = new();
}

public class BacSiDashboardVM
{
    public string  HoTenBacSi   { get; set; } = "";
    public string? HocVi        { get; set; }
    public DateOnly NgayHomNay  { get; set; }
    public List<BacSiCaTrucVM> CaTrucsHomNay { get; set; } = new();
    public int TongBenhNhan  { get; set; }
    public int SoChoXacNhan  { get; set; }
    public int SoDaKham      { get; set; }
}

// ── Hàng đợi thực tế (PhieuSoThuTu) hiển thị ở màn hình Bác sĩ ─────────────
public class HangDoiPhieuVM
{
    public int    MaPhieu        { get; set; }
    public string SoThuTu        { get; set; } = ""; // "A-001", "W-003", "L-001"
    public string TenBenhNhan    { get; set; } = "";
    public string TrangThaiPhieu { get; set; } = "";
    public string? GioHen        { get; set; }
    public int    SoLanGoi       { get; set; }
    public int    MucUuTien      { get; set; }
    public string LoaiKhach      { get; set; } = "";
}
