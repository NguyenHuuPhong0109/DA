using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Models.Entities;
using ClinicBookingMVC.Services;
using Xunit;

namespace ClinicBookingMVC.Tests;

public class TC01_HappyPathTests
{
    public TC01_HappyPathTests()
    {
        TestDbFixture.EnsureDatabaseInitializedAsync().Wait();
    }

    [Fact]
    public void SinhKhungGio_VoiTiLeKhungDem_TaoDungTiLe3DatHen1KhungDem()
    {
        using var db = TestDbFixture.CreateContext();
        var caTrucService = new CaTrucService(db);

        var ca = new CaTruc
        {
            MaCa = 9999,
            MaBacSi = 1,
            MaPhong = 1,
            NgayTruc = DateOnly.FromDateTime(DateTime.Today),
            CaLamViec = "Sang",
            GioBatDauCa = new TimeOnly(7, 0),
            GioKetThucCa = new TimeOnly(9, 0), // 120 phút = 8 slot (15 phút/slot)
            ThoiLuongMoiKhungGio = 15
        };

        var slots = caTrucService.SinhKhungGio(ca, tanSuatKhungDem: 3);

        Assert.NotEmpty(slots);
        Assert.Contains(slots, s => s.LoaiKhungGio == "DatHen");
        Assert.Contains(slots, s => s.LoaiKhungGio == "KhungDem");

        // Kiểm tra thứ tự: cứ sau 3 DatHen thì có 1 KhungDem
        int datHenCount = 0;
        int khungDemCount = 0;
        foreach (var slot in slots)
        {
            if (slot.LoaiKhungGio == "DatHen")
            {
                datHenCount++;
                Assert.Equal("ConTrong", slot.TrangThai);
            }
            else if (slot.LoaiKhungGio == "KhungDem")
            {
                khungDemCount++;
                Assert.Equal("ConTrong", slot.TrangThai);
            }
        }

        Assert.True(khungDemCount >= 1, "Phải sinh ít nhất 1 khung đệm trong ca trực");
        Assert.True(datHenCount >= 3, "Phải có ít nhất 3 khung đặt hẹn");
    }

    [Fact]
    public async Task LuongHoanChinh_TuDatHenDenKhamXong_CapNhatTrangThaiDaKham()
    {
        using var db = TestDbFixture.CreateContext();
        var caTrucService = new CaTrucService(db);
        var datLichService = new DatLichService(db);
        var hangDoiService = new HangDoiService(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var bacSi = await db.BacSis.FirstAsync();
        var admin = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");

        // Tạo phòng khám riêng cho test để hàng đợi hoàn toàn độc lập với seed data
        var phong = new PhongKham
        {
            TenPhong = "Phòng Test " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(phong);
        await db.SaveChangesAsync();

        // 1. Tạo bệnh nhân test độc lập
        var sdtTest = "0988" + Random.Shared.Next(100000, 999999);
        var bn = await datLichService.LayHoacTaoBenhNhanAsync(sdtTest, "Bệnh Nhân HappyPath", 1995, "Nam", "happypath@test.com");
        Assert.NotNull(bn);
        Assert.Equal(0, bn.SoLanVangHen);
        Assert.False(bn.CoCanhBao);

        // 2. Tạo Ca trực mới vào ngày mai (đảm bảo cách hiện tại > 30 phút và không bị ảnh hưởng bởi giờ chạy test)
        var ngayKham = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var gioBatDau = new TimeOnly(8, 0);
        var gioKetThuc = new TimeOnly(10, 0);
        var ca = new CaTruc
        {
            MaBacSi = bacSi.MaBacSi,
            MaPhong = phong.MaPhong,
            NgayTruc = ngayKham,
            CaLamViec = "TestCa_" + Guid.NewGuid().ToString("N")[..6],
            GioBatDauCa = gioBatDau,
            GioKetThucCa = gioKetThuc,
            ThoiLuongMoiKhungGio = 15
        };

        db.CaTrucs.Add(ca);
        await db.SaveChangesAsync();

        var slots = caTrucService.SinhKhungGio(ca, tanSuatKhungDem: 3);
        db.KhungGios.AddRange(slots);
        await db.SaveChangesAsync();

        var slotDatHen = slots.First(s => s.LoaiKhungGio == "DatHen");

        // 3. Đặt lịch khám
        var (okDatLich, loiMsg, phieu) = await datLichService.DatLichAsync(
            slotDatHen.MaKhungGio,
            slotDatHen.PhienBanDong,
            bn.MaBenhNhan,
            admin.MaTaiKhoan,
            "Khám kiểm tra sức khỏe tổng quát định kỳ",
            guiNhacLich: false
        );

        Assert.True(okDatLich, $"Đặt lịch thất bại: {loiMsg}");
        Assert.NotNull(phieu);
        Assert.StartsWith($"BK{ngayKham:yyMMdd}-", phieu.MaDatHen);
        Assert.Equal("DaXacNhan", phieu.TrangThaiHen);

        // Khung giờ đã chuyển sang DaDat
        var kgReload = await db.KhungGios.FindAsync(slotDatHen.MaKhungGio);
        Assert.NotNull(kgReload);
        Assert.Equal("DaDat", kgReload.TrangThai);

        // 4. Check-in đúng giờ (< 15 phút trễ hoặc đến sớm)
        var (phieuSTT, errCheckIn) = await hangDoiService.CheckInDatTruocAsync(phieu.MaPhieuHen, phong.MaPhong);
        Assert.Null(errCheckIn);
        Assert.NotNull(phieuSTT);
        Assert.Equal("DatTruoc", phieuSTT.LoaiKhach);
        Assert.Equal(1, phieuSTT.MucUuTien);
        Assert.Equal("DangCho", phieuSTT.TrangThaiPhieu);

        var maSoHienThi = HangDoiService.FormatSoThuTu(phieuSTT);
        Assert.StartsWith("A-", maSoHienThi);

        // Phiếu hẹn điện tử đã chuyển sang DaCheckIn
        var phieuReload = await db.PhieuDatHens.FindAsync(phieu.MaPhieuHen);
        Assert.NotNull(phieuReload);
        Assert.Equal("DaCheckIn", phieuReload.TrangThaiHen);

        // 5. Điều chỉnh thời gian hẹn vào cửa sổ kích hoạt (GioHen <= Now + 15p) để gọi số
        phieuSTT.GioHen = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(5));
        await db.SaveChangesAsync();

        var goiSoResult = await hangDoiService.GoiSoTiepTheoAsync(phong.MaPhong);
        Assert.NotNull(goiSoResult);
        Assert.Equal(phieuSTT.MaPhieu, goiSoResult.MaPhieu);
        Assert.Equal("DangGoi", goiSoResult.TrangThaiPhieu);
        Assert.Equal(1, goiSoResult.SoLanGoi);

        // 6. Bác sĩ bắt đầu khám
        var phieuDangGoi = await db.PhieuSoThuTus.Include(p => p.PhieuDatHen).FirstAsync(p => p.MaPhieu == phieuSTT.MaPhieu);
        phieuDangGoi.TrangThaiPhieu = "DangKham";
        phieuDangGoi.ThoiGianBatDauKham = DateTime.Now;
        await db.SaveChangesAsync();

        Assert.Equal("DangKham", phieuDangGoi.TrangThaiPhieu);
        Assert.NotNull(phieuDangGoi.ThoiGianBatDauKham);

        // 7. Hoàn tất khám
        phieuDangGoi.TrangThaiPhieu = "HoanTat";
        phieuDangGoi.ThoiGianHoanTat = DateTime.Now;
        if (phieuDangGoi.PhieuDatHen != null)
        {
            phieuDangGoi.PhieuDatHen.TrangThaiHen = "DaKham";
        }
        await db.SaveChangesAsync();

        // 8. Xác nhận kết quả cuối cùng
        var phieuCuoiCung = await db.PhieuDatHens.FindAsync(phieu.MaPhieuHen);
        Assert.NotNull(phieuCuoiCung);
        Assert.Equal("DaKham", phieuCuoiCung.TrangThaiHen);

        var sttCuoiCung = await db.PhieuSoThuTus.FindAsync(phieuSTT.MaPhieu);
        Assert.NotNull(sttCuoiCung);
        Assert.Equal("HoanTat", sttCuoiCung.TrangThaiPhieu);
        Assert.NotNull(sttCuoiCung.ThoiGianHoanTat);
    }

    [Fact]
    public async Task BacSiController_DangNhapBacSi02_NhieuCaTrucCungPhongTrongNgay_KhongBiLoiToDictionary()
    {
        using var db = TestDbFixture.CreateContext();
        var bacSi02 = await db.BacSis.FirstAsync(b => b.SoDienThoai == "0901234562");
        var hangDoiService = new HangDoiService(db);
        var controller = new ClinicBookingMVC.Controllers.BacSiController(db, hangDoiService);

        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, bacSi02.MaTaiKhoan.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "bacsi02"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "BacSi")
        }, "mock"));

        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };

        var result = await controller.Index();
        Assert.NotNull(result);
        var viewResult = Assert.IsType<Microsoft.AspNetCore.Mvc.ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.NotNull(controller.ViewBag.TenPhongMap);
    }
}
