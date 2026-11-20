using System.Text;
using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Data;
using ClinicBookingMVC.Models.ViewModels;

namespace ClinicBookingMVC.Services;

public class BaoCaoService
{
    private readonly ClinicDbContext _db;

    public BaoCaoService(ClinicDbContext db)
    {
        _db = db;
    }

    public async Task<BaoCaoDashboardVM> LayBaoCaoDashboardAsync(BaoCaoFilterVM filter)
    {
        // Xử lý mặc định: Nếu chưa chọn thì mặc định 14 ngày trước đến hôm nay
        var denNgayDate = filter.DenNgay?.Date ?? DateTime.Today;
        var tuNgayDate = filter.TuNgay?.Date ?? DateTime.Today.AddDays(-14);

        if (tuNgayDate > denNgayDate)
        {
            var temp = tuNgayDate;
            tuNgayDate = denNgayDate;
            denNgayDate = temp;
        }

        var tuNgayOnly = DateOnly.FromDateTime(tuNgayDate);
        var denNgayOnly = DateOnly.FromDateTime(denNgayDate);

        // 1. Query danh sách phiếu đặt hẹn trong kỳ
        var queryPhieu = _db.PhieuDatHens
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.BacSi)
                        .ThenInclude(b => b.ChuyenKhoa)
            .Include(p => p.KhungGio)
                .ThenInclude(k => k.CaTruc)
                    .ThenInclude(c => c.PhongKham)
            .Include(p => p.BenhNhan)
            .Where(p => p.KhungGio != null
                     && p.KhungGio.CaTruc != null
                     && p.KhungGio.CaTruc.NgayTruc >= tuNgayOnly
                     && p.KhungGio.CaTruc.NgayTruc <= denNgayOnly);

        if (filter.MaChuyenKhoa.HasValue && filter.MaChuyenKhoa.Value > 0)
        {
            queryPhieu = queryPhieu.Where(p => p.KhungGio.CaTruc.BacSi.MaChuyenKhoa == filter.MaChuyenKhoa.Value);
        }

        if (filter.MaBacSi.HasValue && filter.MaBacSi.Value > 0)
        {
            queryPhieu = queryPhieu.Where(p => p.KhungGio.CaTruc.MaBacSi == filter.MaBacSi.Value);
        }

        var dsPhieu = await queryPhieu.ToListAsync();

        // 2. Tính toán KPI 1: Tỷ lệ vắng hẹn
        // Mẫu số: DaXacNhan + DaCheckIn + DaKham + VangHen
        var validStatuses = new[] { "DaXacNhan", "DaCheckIn", "DaKham", "VangHen" };
        var phieuMauSoVangHen = dsPhieu.Where(p => validStatuses.Contains(p.TrangThaiHen)).ToList();
        int soVangHen = dsPhieu.Count(p => p.TrangThaiHen == "VangHen");
        int tongMauSoVangHen = phieuMauSoVangHen.Count;
        double tyLeVangHen = tongMauSoVangHen > 0 ? Math.Round((double)soVangHen / tongMauSoVangHen * 100, 1) : 0;

        // KPI 2: Tỷ lệ hoàn thành khám
        // Tử số: DaKham; Mẫu số: Tổng phiếu đặt hẹn trong kỳ
        int soDaKham = dsPhieu.Count(p => p.TrangThaiHen == "DaKham");
        int tongPhieuDatHen = dsPhieu.Count;
        double tyLeHoanThanh = tongPhieuDatHen > 0 ? Math.Round((double)soDaKham / tongPhieuDatHen * 100, 1) : 0;

        // 3. Tính toán KPI 3: Thời gian chờ khám trung bình (Phút)
        // DATEDIFF(MINUTE, ThoiGianCheckIn, ThoiGianGoiSo) trên các phiếu đã được gọi
        var queryHangDoi = _db.PhieuSoThuTus
            .Include(p => p.PhongKham)
            .Where(p => p.NgayCapSo >= tuNgayOnly
                     && p.NgayCapSo <= denNgayOnly
                     && p.ThoiGianGoiSo != null
                     && p.ThoiGianGoiSo >= p.ThoiGianCheckIn);

        if (filter.MaChuyenKhoa.HasValue && filter.MaChuyenKhoa.Value > 0)
        {
            queryHangDoi = queryHangDoi.Where(p => p.PhongKham.MaChuyenKhoa == filter.MaChuyenKhoa.Value);
        }

        if (filter.MaBacSi.HasValue && filter.MaBacSi.Value > 0)
        {
            // Lọc qua PhieuDatHen liên kết
            queryHangDoi = queryHangDoi.Where(p => p.PhieuDatHen != null
                                                && p.PhieuDatHen.KhungGio != null
                                                && p.PhieuDatHen.KhungGio.CaTruc.MaBacSi == filter.MaBacSi.Value);
        }

        var dsHangDoiDaGoi = await queryHangDoi.ToListAsync();
        double thoiGianChoTrungBinh = 0;
        int soLuotDaGoiSo = dsHangDoiDaGoi.Count;
        if (soLuotDaGoiSo > 0)
        {
            var tongPhut = dsHangDoiDaGoi.Sum(p => (p.ThoiGianGoiSo!.Value - p.ThoiGianCheckIn).TotalMinutes);
            thoiGianChoTrungBinh = Math.Round(tongPhut / soLuotDaGoiSo, 1);
        }

        // 4. Tính toán KPI 4: Tỷ lệ lấp đầy khung giờ (%)
        // KhungGio loại DatHen đã có lịch khám / Tổng KhungGio loại DatHen mở trong kỳ
        var queryKhungGio = _db.KhungGios
            .Include(k => k.CaTruc)
                .ThenInclude(c => c.BacSi)
            .Include(k => k.CaTruc)
                .ThenInclude(c => c.PhongKham)
            .Include(k => k.PhieuDatHen)
            .Where(k => k.CaTruc != null
                     && k.CaTruc.NgayTruc >= tuNgayOnly
                     && k.CaTruc.NgayTruc <= denNgayOnly
                     && k.LoaiKhungGio == "DatHen");

        if (filter.MaChuyenKhoa.HasValue && filter.MaChuyenKhoa.Value > 0)
        {
            queryKhungGio = queryKhungGio.Where(k => k.CaTruc.BacSi.MaChuyenKhoa == filter.MaChuyenKhoa.Value);
        }

        if (filter.MaBacSi.HasValue && filter.MaBacSi.Value > 0)
        {
            queryKhungGio = queryKhungGio.Where(k => k.CaTruc.MaBacSi == filter.MaBacSi.Value);
        }

        var dsKhungGio = await queryKhungGio.ToListAsync();
        int tongKhungDatHen = dsKhungGio.Count;
        int soKhungDaDat = dsKhungGio.Count(k => k.PhieuDatHen != null
                                              || k.TrangThai == "DaDat"
                                              || k.TrangThai == "TamKhoa");
        double tyLeLapDay = tongKhungDatHen > 0 ? Math.Round((double)soKhungDaDat / tongKhungDatHen * 100, 1) : 0;

        // 5. Lưu lượng bệnh nhân theo khung giờ trong ngày (07:00 - 17:00)
        var luuLuongTheoGio = new List<LuuLuongGioVM>();
        for (int h = 7; h <= 17; h++)
        {
            var label = $"{h:D2}:00 - {(h + 1):D2}:00";
            var count = dsPhieu.Count(p => p.KhungGio != null && p.KhungGio.GioBatDau.Hour == h);
            luuLuongTheoGio.Add(new LuuLuongGioVM
            {
                Gio = h,
                KhungGio = label,
                SoLuongBenhNhan = count
            });
        }

        // 6. Thống kê vắng hẹn theo từng bác sĩ
        var dsBacSi = await _db.BacSis
            .Include(b => b.ChuyenKhoa)
            .Where(b => b.TrangThai == "HoatDong")
            .ToListAsync();

        var vangHenTheoBacSi = new List<VangHenBacSiVM>();
        foreach (var bs in dsBacSi)
        {
            var phieuBacSi = dsPhieu.Where(p => p.KhungGio.CaTruc.MaBacSi == bs.MaBacSi).ToList();
            var validBacSi = phieuBacSi.Where(p => validStatuses.Contains(p.TrangThaiHen)).ToList();
            int vang = phieuBacSi.Count(p => p.TrangThaiHen == "VangHen");
            int tongHenBs = validBacSi.Count;
            double tyLeVangBs = tongHenBs > 0 ? Math.Round((double)vang / tongHenBs * 100, 1) : 0;

            vangHenTheoBacSi.Add(new VangHenBacSiVM
            {
                MaBacSi = bs.MaBacSi,
                HoTenBacSi = (!string.IsNullOrEmpty(bs.HocVi) ? bs.HocVi + " " : "") + bs.HoTenBacSi,
                TenChuyenKhoa = bs.ChuyenKhoa?.TenChuyenKhoa ?? "",
                TongHen = tongHenBs,
                SoVangHen = vang,
                TyLeVangHen = tyLeVangBs
            });
        }

        // 7. Danh sách bệnh nhân có cảnh báo vắng hẹn (SoLanVangHen >= 2 hoặc CoCanhBao)
        var dsCanhBao = await _db.BenhNhans
            .Include(b => b.PhieuDatHens)
            .Where(b => b.CoCanhBao || b.SoLanVangHen >= 2)
            .OrderByDescending(b => b.SoLanVangHen)
            .Take(50)
            .Select(b => new BenhNhanCanhBaoVM
            {
                MaBenhNhan = b.MaBenhNhan,
                TenBenhNhan = b.TenBenhNhan,
                SoDienThoai = b.SoDienThoai,
                SoLanVangHen = b.SoLanVangHen,
                CoCanhBao = b.CoCanhBao,
                LanVangGanNhat = b.PhieuDatHens
                    .Where(p => p.TrangThaiHen == "VangHen")
                    .OrderByDescending(p => p.ThoiGianTaoPhieu)
                    .Select(p => (DateTime?)p.ThoiGianTaoPhieu)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new BaoCaoDashboardVM
        {
            Filter = new BaoCaoFilterVM
            {
                TuNgay = tuNgayDate,
                DenNgay = denNgayDate,
                MaChuyenKhoa = filter.MaChuyenKhoa,
                MaBacSi = filter.MaBacSi
            },
            KPI = new BaoCaoKpiVM
            {
                TyLeVangHen = tyLeVangHen,
                SoPhieuVangHen = soVangHen,
                TongPhieuVangHenMauSo = tongMauSoVangHen,
                TyLeHoanThanh = tyLeHoanThanh,
                SoPhieuDaKham = soDaKham,
                TongPhieuDatHenTrongKy = tongPhieuDatHen,
                ThoiGianChoTrungBinh = thoiGianChoTrungBinh,
                SoLuotDaGoiSo = soLuotDaGoiSo,
                TyLeLapDay = tyLeLapDay,
                SoKhungGioDaDat = soKhungDaDat,
                TongKhungGioDatHen = tongKhungDatHen
            },
            LuuLuongTheoGio = luuLuongTheoGio,
            VangHenTheoBacSi = vangHenTheoBacSi.OrderByDescending(v => v.TyLeVangHen).ToList(),
            DanhSachCanhBao = dsCanhBao,
            DanhSachChuyenKhoa = await _db.ChuyenKhoas.Where(c => c.TrangThai == "HoatDong").ToListAsync(),
            DanhSachBacSi = dsBacSi
        };
    }

    /// <summary>
    /// Xuất báo cáo KPI & Thống kê ra file CSV có gắn UTF-8 BOM chuẩn tiếng Việt cho Microsoft Excel.
    /// </summary>
    public async Task<byte[]> ExportBaoCaoCsvAsync(BaoCaoFilterVM filter)
    {
        var vm = await LayBaoCaoDashboardAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("BÁO CÁO THỐNG KÊ KPI HOẠT ĐỘNG PHÒNG KHÁM");
        sb.AppendLine($"Thời gian: Từ {vm.Filter.TuNgay:dd/MM/yyyy} Đến {vm.Filter.DenNgay:dd/MM/yyyy}");
        sb.AppendLine($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();

        // 1. KPI tổng hợp
        sb.AppendLine("=== CHỈ SỐ KPI CỐT LÕI ===");
        sb.AppendLine("Chỉ số,Giá trị,Chi tiết");
        sb.AppendLine($"Tỷ lệ vắng hẹn (No-Show Rate),{vm.KPI.TyLeVangHen}%,{vm.KPI.SoPhieuVangHen} vắng / {vm.KPI.TongPhieuVangHenMauSo} phiếu hợp lệ");
        sb.AppendLine($"Tỷ lệ hoàn thành khám,{vm.KPI.TyLeHoanThanh}%,{vm.KPI.SoPhieuDaKham} đã khám / {vm.KPI.TongPhieuDatHenTrongKy} phiếu đặt hẹn");
        sb.AppendLine($"Thời gian chờ khám trung bình,{vm.KPI.ThoiGianChoTrungBinh} phút,Tính trên {vm.KPI.SoLuotDaGoiSo} lượt gọi số");
        sb.AppendLine($"Tỷ lệ lấp đầy khung giờ,{vm.KPI.TyLeLapDay}%,{vm.KPI.SoKhungGioDaDat} đã đặt / {vm.KPI.TongKhungGioDatHen} khung giờ mở");
        sb.AppendLine();

        // 2. Thống kê theo Bác sĩ
        sb.AppendLine("=== TỶ LỆ VẮNG HẸN THEO BÁC SĨ ===");
        sb.AppendLine("Mã BS,Họ tên Bác sĩ,Chuyên khoa,Tổng hẹn hợp lệ,Số vắng hẹn,Tỷ lệ vắng hẹn (%)");
        foreach (var bs in vm.VangHenTheoBacSi)
        {
            sb.AppendLine($"{bs.MaBacSi},\"{bs.HoTenBacSi}\",\"{bs.TenChuyenKhoa}\",{bs.TongHen},{bs.SoVangHen},{bs.TyLeVangHen}%");
        }
        sb.AppendLine();

        // 3. Lưu lượng theo giờ
        sb.AppendLine("=== LƯU LƯỢNG BỆNH NHÂN THEO KHUNG GIỜ ===");
        sb.AppendLine("Khung giờ,Số lượng bệnh nhân");
        foreach (var ll in vm.LuuLuongTheoGio)
        {
            sb.AppendLine($"\"{ll.KhungGio}\",{ll.SoLuongBenhNhan}");
        }
        sb.AppendLine();

        // 4. Danh sách bệnh nhân cảnh báo
        sb.AppendLine("=== DANH SÁCH BỆNH NHÂN CÓ CẢNH BÁO VẮNG HẸN (>= 2 LẦN) ===");
        sb.AppendLine("Mã BN,Họ tên Bác sĩ / Bệnh nhân,Số điện thoại,Số lần vắng hẹn,Trạng thái cảnh báo,Lần vắng gần nhất");
        foreach (var bn in vm.DanhSachCanhBao)
        {
            var lanVang = bn.LanVangGanNhat.HasValue ? bn.LanVangGanNhat.Value.ToString("dd/MM/yyyy HH:mm") : "N/A";
            sb.AppendLine($"{bn.MaBenhNhan},\"{bn.TenBenhNhan}\",\"{bn.SoDienThoai}\",{bn.SoLanVangHen},CẢNH BÁO CAO,\"{lanVang}\"");
        }

        // Tạo UTF-8 BOM byte array
        var bom = Encoding.UTF8.GetPreamble();
        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[bom.Length + contentBytes.Length];
        Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        Buffer.BlockCopy(contentBytes, 0, result, bom.Length, contentBytes.Length);

        return result;
    }
}
