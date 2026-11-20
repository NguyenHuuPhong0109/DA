using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Models.Entities;
using ClinicBookingMVC.Services;
using Xunit;

namespace ClinicBookingMVC.Tests;

public class TC02_ConcurrencyTests
{
    public TC02_ConcurrencyTests()
    {
        TestDbFixture.EnsureDatabaseInitializedAsync().Wait();
    }

    [Fact]
    public async Task HaiTacVuDatCungMotKhungGioSongSong_DungMotTacVuThanhCong_TacVuConLaiBiChan()
    {
        using var setupDb = TestDbFixture.CreateContext();
        var caTrucService = new CaTrucService(setupDb);
        var datLichServiceSetup = new DatLichService(setupDb);

        var bacSi = await setupDb.BacSis.FirstAsync();
        var admin = await setupDb.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Concurrency " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        setupDb.PhongKhams.Add(testPhong);
        await setupDb.SaveChangesAsync();

        // 1. Tạo 2 bệnh nhân riêng biệt
        var bnA = await datLichServiceSetup.LayHoacTaoBenhNhanAsync(
            "0977" + Random.Shared.Next(100000, 999999), "Bệnh Nhân Concurrency A");
        var bnB = await datLichServiceSetup.LayHoacTaoBenhNhanAsync(
            "0977" + Random.Shared.Next(100000, 999999), "Bệnh Nhân Concurrency B");

        // 2. Tạo ca trực vào ngày mai (thời gian hợp lệ tuyệt đối)
        var ngayMai = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var ca = new CaTruc
        {
            MaBacSi = bacSi.MaBacSi,
            MaPhong = testPhong.MaPhong,
            NgayTruc = ngayMai,
            CaLamViec = "CaConcur_" + Guid.NewGuid().ToString("N")[..6],
            GioBatDauCa = new TimeOnly(8, 0),
            GioKetThucCa = new TimeOnly(8, 15),
            ThoiLuongMoiKhungGio = 15
        };
        setupDb.CaTrucs.Add(ca);
        await setupDb.SaveChangesAsync();

        var slot = new KhungGio
        {
            MaCa = ca.MaCa,
            GioBatDau = new TimeOnly(8, 0),
            GioKetThuc = new TimeOnly(8, 15),
            LoaiKhungGio = "DatHen",
            TrangThai = "ConTrong"
        };
        setupDb.KhungGios.Add(slot);
        await setupDb.SaveChangesAsync();

        // Lấy thông tin slot và RowVersion mới nhất
        var maKhungGio = slot.MaKhungGio;
        var rowVersion = slot.PhienBanDong;
        Assert.NotNull(rowVersion);

        // 3. Chuẩn bị 2 DbContext và 2 Service riêng biệt để chạy song song
        using var db1 = TestDbFixture.CreateContext();
        using var db2 = TestDbFixture.CreateContext();
        var service1 = new DatLichService(db1);
        var service2 = new DatLichService(db2);

        // Dùng Barrier hoặc Task.WhenAll để kích hoạt đồng thời 2 tác vụ
        var task1 = Task.Run(async () =>
        {
            return await service1.DatLichAsync(
                maKhungGio, rowVersion, bnA.MaBenhNhan, admin.MaTaiKhoan, "Bệnh nhân A đặt lịch", false);
        });

        var task2 = Task.Run(async () =>
        {
            return await service2.DatLichAsync(
                maKhungGio, rowVersion, bnB.MaBenhNhan, admin.MaTaiKhoan, "Bệnh nhân B đặt lịch", false);
        });

        var results = await Task.WhenAll(task1, task2);
        var res1 = results[0];
        var res2 = results[1];

        // 4. Kiểm chứng kết quả Concurrency: Đúng 1 tác vụ thành công, 1 tác vụ bị chặn
        int successCount = (res1.Ok ? 1 : 0) + (res2.Ok ? 1 : 0);
        int failedCount = (!res1.Ok ? 1 : 0) + (!res2.Ok ? 1 : 0);

        Assert.Equal(1, successCount);
        Assert.Equal(1, failedCount);

        var failedResult = !res1.Ok ? res1 : res2;
        var successResult = res1.Ok ? res1 : res2;

        Assert.NotNull(successResult.Phieu);
        Assert.StartsWith($"BK{ngayMai:yyMMdd}-", successResult.Phieu.MaDatHen);

        Assert.Equal("Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác.", failedResult.LoiMsg);

        // 5. Kiểm tra dữ liệu trong DB
        using var verifyDb = TestDbFixture.CreateContext();
        var kgInDb = await verifyDb.KhungGios.FindAsync(maKhungGio);
        Assert.NotNull(kgInDb);
        Assert.Equal("DaDat", kgInDb.TrangThai);

        var countPhieuHieuLuc = await verifyDb.PhieuDatHens
            .CountAsync(p => p.MaKhungGio == maKhungGio && p.TrangThaiHen != "DaHuy");
        Assert.Equal(1, countPhieuHieuLuc);
    }
}
