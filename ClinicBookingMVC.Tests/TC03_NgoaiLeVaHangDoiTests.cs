using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ClinicBookingMVC.Models.Entities;
using ClinicBookingMVC.Services;
using Xunit;

namespace ClinicBookingMVC.Tests;

public class TC03_NgoaiLeVaHangDoiTests
{
    public TC03_NgoaiLeVaHangDoiTests()
    {
        TestDbFixture.EnsureDatabaseInitializedAsync().Wait();
    }

    // ─────────────────────────────────────────────────────────────
    // 1. Chặn đặt lịch cách hiện tại < 30 phút và chặn hủy khi < 2 giờ
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public void ChanDatLich_CachHienTaiDuoi30Phut()
    {
        using var db = TestDbFixture.CreateContext();
        var datLichService = new DatLichService(db);

        var target15 = DateTime.Now.AddMinutes(15);
        var target45 = DateTime.Now.AddMinutes(45);

        // Cách hiện tại 15 phút (< 30 phút) -> Bị chặn
        var loiDat15 = datLichService.KiemTraThoiDiemDatHen(DateOnly.FromDateTime(target15), TimeOnly.FromDateTime(target15));
        Assert.Equal("Chỉ được đặt lịch trước giờ khám ít nhất 30 phút.", loiDat15);

        // Cách hiện tại 45 phút (>= 30 phút) -> Hợp lệ
        var loiDat45 = datLichService.KiemTraThoiDiemDatHen(DateOnly.FromDateTime(target45), TimeOnly.FromDateTime(target45));
        Assert.Null(loiDat45);
    }

    [Fact]
    public void ChanHuyLich_KhiConDuoi2Gio()
    {
        using var db = TestDbFixture.CreateContext();
        var datLichService = new DatLichService(db);

        // Phiếu hẹn còn 1 giờ nữa là khám (< 2 giờ)
        var target1H = DateTime.Now.AddHours(1);
        var caTruc1H = new CaTruc { NgayTruc = DateOnly.FromDateTime(target1H) };
        var kg1H = new KhungGio { CaTruc = caTruc1H, GioBatDau = TimeOnly.FromDateTime(target1H) };
        var phieu1H = new PhieuDatHen { KhungGio = kg1H, TrangThaiHen = "DaXacNhan" };

        var loiHuy1H = datLichService.KiemTraDieuKienHuy(phieu1H);
        Assert.Equal("Chỉ được hủy lịch trước giờ hẹn ít nhất 2 giờ.", loiHuy1H);

        // Phiếu hẹn còn 3 giờ nữa mới khám (>= 2 giờ)
        var target3H = DateTime.Now.AddHours(3);
        var caTruc3H = new CaTruc { NgayTruc = DateOnly.FromDateTime(target3H) };
        var kg3H = new KhungGio { CaTruc = caTruc3H, GioBatDau = TimeOnly.FromDateTime(target3H) };
        var phieu3H = new PhieuDatHen { KhungGio = kg3H, TrangThaiHen = "DaXacNhan" };

        var loiHuy3H = datLichService.KiemTraDieuKienHuy(phieu3H);
        Assert.Null(loiHuy3H);

        // Phiếu đã hủy trước đó
        var phieuDaHuy = new PhieuDatHen { KhungGio = kg3H, TrangThaiHen = "DaHuy" };
        Assert.Equal("Phiếu hẹn này đã được hủy trước đó.", datLichService.KiemTraDieuKienHuy(phieuDaHuy));

        // Phiếu đã khám / đã check-in
        var phieuDaKham = new PhieuDatHen { KhungGio = kg3H, TrangThaiHen = "DaKham" };
        Assert.Equal("Không thể hủy phiếu hẹn đã check-in hoặc đã khám.", datLichService.KiemTraDieuKienHuy(phieuDaKham));
    }

    // ─────────────────────────────────────────────────────────────
    // 2. Chặn 1 SoDienThoai đặt 2 lịch giao nhau khung giờ trong cùng ngày
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task ChanMotSoDienThoai_Dat2LichTrongCungMotNgay()
    {
        using var db = TestDbFixture.CreateContext();
        var datLichService = new DatLichService(db);
        var bacSi = await db.BacSis.FirstAsync();
        var admin = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Trùng Lịch " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        var sdt = "0966" + Random.Shared.Next(100000, 999999);
        var bn = await datLichService.LayHoacTaoBenhNhanAsync(sdt, "Bệnh Nhân Test Trùng Lịch");

        var ngayKham = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
        var ca = new CaTruc
        {
            MaBacSi = bacSi.MaBacSi,
            MaPhong = testPhong.MaPhong,
            NgayTruc = ngayKham,
            CaLamViec = "CaTrung_" + Guid.NewGuid().ToString("N")[..6],
            GioBatDauCa = new TimeOnly(8, 0),
            GioKetThucCa = new TimeOnly(9, 0),
            ThoiLuongMoiKhungGio = 15
        };
        db.CaTrucs.Add(ca);
        await db.SaveChangesAsync();

        var slot1 = new KhungGio { MaCa = ca.MaCa, GioBatDau = new TimeOnly(8, 0), GioKetThuc = new TimeOnly(8, 15), LoaiKhungGio = "DatHen", TrangThai = "ConTrong" };
        var slot2 = new KhungGio { MaCa = ca.MaCa, GioBatDau = new TimeOnly(8, 15), GioKetThuc = new TimeOnly(8, 30), LoaiKhungGio = "DatHen", TrangThai = "ConTrong" };
        db.KhungGios.AddRange(slot1, slot2);
        await db.SaveChangesAsync();

        // Đặt lịch lần 1 ở slot1 thành công
        var (ok1, _, _) = await datLichService.DatLichAsync(
            slot1.MaKhungGio, slot1.PhienBanDong, bn.MaBenhNhan, admin.MaTaiKhoan, "Lần 1", false);
        Assert.True(ok1);

        // Kiểm tra đặt lần 2 ở slot2 cùng ngày -> Bị chặn
        var loiTrung = await datLichService.KiemTraTrungLichAsync(sdt, slot2.MaKhungGio, ngayKham);
        Assert.Equal("Bạn đã có lịch khám khác trong ngày này. Vui lòng hủy lịch cũ hoặc chọn ngày khác.", loiTrung);

        // Thử đặt lịch trực tiếp bằng DatLichAsync cũng phải bị chặn
        var (ok2, loiMsg2, _) = await datLichService.DatLichAsync(
            slot2.MaKhungGio, slot2.PhienBanDong, bn.MaBenhNhan, admin.MaTaiKhoan, "Lần 2", false);
        Assert.False(ok2);
        Assert.Equal("Bạn đã có lịch khám khác trong ngày này. Vui lòng hủy lịch cũ hoặc chọn ngày khác.", loiMsg2);

        // Kiểm tra khác ngày thì không bị chặn
        var ngayKhac = ngayKham.AddDays(1);
        var loiKhacNgay = await datLichService.KiemTraTrungLichAsync(sdt, slot2.MaKhungGio, ngayKhac);
        Assert.Null(loiKhacNgay);
    }

    // ─────────────────────────────────────────────────────────────
    // 3. Hủy lịch hợp lệ (>= 2 giờ) phải mở lại KhungGio.TrangThai = "ConTrong"
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task HuyLichHopLe_MoLaiKhungGioConTrong()
    {
        using var db = TestDbFixture.CreateContext();
        var datLichService = new DatLichService(db);
        var bacSi = await db.BacSis.FirstAsync();
        var admin = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Hủy Lịch " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        var sdt = "0966" + Random.Shared.Next(100000, 999999);
        var bn = await datLichService.LayHoacTaoBenhNhanAsync(sdt, "Bệnh Nhân Test Hủy");

        // Đặt trước 3 ngày (rất xa thời điểm hiện tại, > 2 giờ)
        var ngayKham = DateOnly.FromDateTime(DateTime.Today.AddDays(4));
        var ca = new CaTruc
        {
            MaBacSi = bacSi.MaBacSi,
            MaPhong = testPhong.MaPhong,
            NgayTruc = ngayKham,
            CaLamViec = "CaHuy_" + Guid.NewGuid().ToString("N")[..6],
            GioBatDauCa = new TimeOnly(14, 0),
            GioKetThucCa = new TimeOnly(14, 15),
            ThoiLuongMoiKhungGio = 15
        };
        db.CaTrucs.Add(ca);
        await db.SaveChangesAsync();

        var slot = new KhungGio { MaCa = ca.MaCa, GioBatDau = new TimeOnly(14, 0), GioKetThuc = new TimeOnly(14, 15), LoaiKhungGio = "DatHen", TrangThai = "ConTrong" };
        db.KhungGios.Add(slot);
        await db.SaveChangesAsync();

        var (ok, loiMsg, phieu) = await datLichService.DatLichAsync(
            slot.MaKhungGio, null, bn.MaBenhNhan, admin.MaTaiKhoan, "Khám rồi hủy", false);
        Assert.True(ok, $"Đặt lịch thất bại: {loiMsg}");
        Assert.NotNull(phieu);

        // Sau khi đặt: slot là DaDat
        var slotSauDat = await db.KhungGios.FindAsync(slot.MaKhungGio);
        Assert.Equal("DaDat", slotSauDat!.TrangThai);

        // Thực hiện hủy lịch
        var (okHuy, loiHuy) = await datLichService.HuyLichAsync(phieu.MaPhieuHen, admin.MaTaiKhoan);
        Assert.True(okHuy, $"Hủy lịch lỗi: {loiHuy}");
        Assert.Null(loiHuy);

        // Xác nhận trạng thái sau hủy
        var phieuSauHuy = await db.PhieuDatHens.FindAsync(phieu.MaPhieuHen);
        Assert.Equal("DaHuy", phieuSauHuy!.TrangThaiHen);

        var slotSauHuy = await db.KhungGios.FindAsync(slot.MaKhungGio);
        Assert.Equal("ConTrong", slotSauHuy!.TrangThai);
    }

    // ─────────────────────────────────────────────────────────────
    // 4. Thuật toán hàng đợi 3 mức:
    //    - Khách đến sớm (> 15 phút) không nhảy cóc giành lượt
    //    - Khách trễ 16-30 phút bị gán MucUuTien = 3 (prefix L)
    //    - Khách A chưa check-in thì gọi ngay W-xxx
    //    - Gọi lại tối đa 3 lần không vượt SoLanGoi = 3
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task HangDoi_KhachDatSomHon15Phut_KhongNhayCoc()
    {
        using var db = TestDbFixture.CreateContext();
        var hangDoiService = new HangDoiService(db);
        var bacSi = await db.BacSis.FirstAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Nhảy Cóc " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        var bnA = new BenhNhan { TenBenhNhan = "Khách A Đến Sớm", SoDienThoai = "0944" + Random.Shared.Next(100000, 999999) };
        var bnW = new BenhNhan { TenBenhNhan = "Khách W Vãng Lai", SoDienThoai = "0944" + Random.Shared.Next(100000, 999999) };
        db.BenhNhans.AddRange(bnA, bnW);
        await db.SaveChangesAsync();

        // Khách A: Đặt trước, giờ hẹn là Now + 25 phút (chưa tới cửa sổ 15 phút)
        var phieuSTT_A = new PhieuSoThuTu
        {
            SoThuTu = 1,
            NgayCapSo = today,
            LoaiKhach = "DatTruoc",
            MucUuTien = 1,
            GioHen = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(25)),
            ThoiGianCheckIn = DateTime.Now.AddMinutes(-5),
            TrangThaiPhieu = "DangCho",
            MaPhong = testPhong.MaPhong,
            MaBenhNhan = bnA.MaBenhNhan
        };

        // Khách W: Vãng lai (MucUuTien = 2), check-in trước
        var phieuSTT_W = new PhieuSoThuTu
        {
            SoThuTu = 1,
            NgayCapSo = today,
            LoaiKhach = "VangLai",
            MucUuTien = 2,
            GioHen = null,
            ThoiGianCheckIn = DateTime.Now.AddMinutes(-10),
            TrangThaiPhieu = "DangCho",
            MaPhong = testPhong.MaPhong,
            MaBenhNhan = bnW.MaBenhNhan
        };

        db.PhieuSoThuTus.AddRange(phieuSTT_A, phieuSTT_W);
        await db.SaveChangesAsync();

        // Gọi số tiếp theo: Vì khách A chưa vào cửa sổ kích hoạt (GioHen > Now + 15p), thuật toán không cho nhảy cóc
        // mà phải gọi khách vãng lai W trước!
        var result = await hangDoiService.GoiSoTiepTheoAsync(testPhong.MaPhong);
        Assert.NotNull(result);
        Assert.Equal(phieuSTT_W.MaPhieu, result.MaPhieu);
        Assert.Equal("W", result.Prefix);
        Assert.Equal("VangLai", result.LoaiKhach);
    }

    [Fact]
    public async Task HangDoi_KhachDenTre16Den30Phut_BiGanMucUuTien3_PrefixL()
    {
        using var db = TestDbFixture.CreateContext();
        var hangDoiService = new HangDoiService(db);
        var bacSi = await db.BacSis.FirstAsync();
        var admin = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");
        var today = DateOnly.FromDateTime(DateTime.Today);

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Khách Trễ " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        var bn = new BenhNhan { TenBenhNhan = "Khách Đến Trễ", SoDienThoai = "0933" + Random.Shared.Next(100000, 999999) };
        db.BenhNhans.Add(bn);
        await db.SaveChangesAsync();

        // Tạo ca trực và khung giờ có giờ hẹn cách đây 20 phút (trễ trong khoảng 16 - 30 phút)
        var gioHenTre20Phut = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(-20));
        var ca = new CaTruc
        {
            MaBacSi = bacSi.MaBacSi,
            MaPhong = testPhong.MaPhong,
            NgayTruc = today,
            CaLamViec = "CaTre_" + Guid.NewGuid().ToString("N")[..6],
            GioBatDauCa = gioHenTre20Phut,
            GioKetThucCa = gioHenTre20Phut.AddHours(1),
            ThoiLuongMoiKhungGio = 15
        };
        db.CaTrucs.Add(ca);
        await db.SaveChangesAsync();

        var kg = new KhungGio
        {
            MaCa = ca.MaCa,
            GioBatDau = gioHenTre20Phut,
            GioKetThuc = gioHenTre20Phut.AddMinutes(15),
            LoaiKhungGio = "DatHen",
            TrangThai = "DaDat"
        };
        db.KhungGios.Add(kg);
        await db.SaveChangesAsync();

        var phieu = new PhieuDatHen
        {
            MaDatHen = $"BK{today:yyMMdd}-{Random.Shared.Next(100, 999):D3}{Random.Shared.Next(10, 99)}",
            KenhDatLich = "Online",
            ThoiGianTaoPhieu = DateTime.Now.AddHours(-2),
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn.MaBenhNhan,
            MaNguoiTao = admin.MaTaiKhoan,
            MaKhungGio = kg.MaKhungGio
        };
        db.PhieuDatHens.Add(phieu);
        await db.SaveChangesAsync();

        // Check-in cho khách trễ 20 phút
        var (phieuSTT, err) = await hangDoiService.CheckInDatTruocAsync(phieu.MaPhieuHen, testPhong.MaPhong);
        Assert.Null(err);
        Assert.NotNull(phieuSTT);
        Assert.Equal("DatTruocTre", phieuSTT.LoaiKhach);
        Assert.Equal(3, phieuSTT.MucUuTien);

        var maSTT = HangDoiService.FormatSoThuTu(phieuSTT);
        Assert.StartsWith("L-", maSTT);
    }

    [Fact]
    public async Task HangDoi_KhachDatHenChuaDen_GoiNgayKhachVangLai()
    {
        using var db = TestDbFixture.CreateContext();
        var hangDoiService = new HangDoiService(db);
        var bacSi = await db.BacSis.FirstAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Vãng Lai " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        // Không có khách A nào check-in; chỉ có 1 khách vãng lai W check-in
        var (phieuW, isNew) = await hangDoiService.CheckInVangLaiAsync(
            "0922" + Random.Shared.Next(100000, 999999), testPhong.MaPhong);
        Assert.NotNull(phieuW);
        Assert.Equal("VangLai", phieuW.LoaiKhach);
        Assert.Equal(2, phieuW.MucUuTien);
        Assert.Equal("DangCho", phieuW.TrangThaiPhieu);

        // Gọi số tiếp theo -> Gọi ngay khách W
        var goiResult = await hangDoiService.GoiSoTiepTheoAsync(testPhong.MaPhong);
        Assert.NotNull(goiResult);
        Assert.Equal(phieuW.MaPhieu, goiResult.MaPhieu);
        Assert.Equal("W", goiResult.Prefix);
        Assert.Equal("DangGoi", goiResult.TrangThaiPhieu);
    }

    [Fact]
    public async Task HangDoi_GoiLaiToiDa3Lan_KhongVuotQua3()
    {
        using var db = TestDbFixture.CreateContext();
        var hangDoiService = new HangDoiService(db);
        var bacSi = await db.BacSis.FirstAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Gọi Lại " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        var (phieuW, _) = await hangDoiService.CheckInVangLaiAsync(
            "0911" + Random.Shared.Next(100000, 999999), testPhong.MaPhong);

        // Gọi lần 1
        var call1 = await hangDoiService.GoiSoTiepTheoAsync(testPhong.MaPhong);
        Assert.NotNull(call1);
        Assert.Equal(1, call1.SoLanGoi);

        // Gọi lại lần 2
        var recall1 = await hangDoiService.GoiLaiAsync(phieuW.MaPhieu);
        Assert.NotNull(recall1);
        Assert.Equal(2, recall1.SoLanGoi);

        // Gọi lại lần 3
        var recall2 = await hangDoiService.GoiLaiAsync(phieuW.MaPhieu);
        Assert.NotNull(recall2);
        Assert.Equal(3, recall2.SoLanGoi);

        // Gọi lại lần 4 -> Bị chặn, trả về null báo hiệu đã đạt giới hạn 3 lần
        var recall3 = await hangDoiService.GoiLaiAsync(phieuW.MaPhieu);
        Assert.Null(recall3);

        // Kiểm tra trong DB SoLanGoi vẫn dừng ở 3, không bao giờ vượt quá 3
        var inDb = await db.PhieuSoThuTus.FindAsync(phieuW.MaPhieu);
        Assert.Equal(3, inDb!.SoLanGoi);
    }

    // ─────────────────────────────────────────────────────────────
    // 5. Kiểm tra VangHenService: Quá 30 phút chưa check-in -> VangHen,
    //    cộng SoLanVangHen, bật CoCanhBao = true khi đạt >= 2
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task VangHenService_Qua30PhutChuaCheckIn_ChuyenVangHen_BatCoCanhBaoKhiDat2Lan()
    {
        using var db = TestDbFixture.CreateContext();
        var logger = NullLogger<VangHenService>.Instance;
        var vangHenService = new VangHenService(db, logger);
        var bacSi = await db.BacSis.FirstAsync();
        var admin = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");
        var today = DateOnly.FromDateTime(DateTime.Today);

        var testPhong = new PhongKham
        {
            TenPhong = "Phòng Vắng Hẹn " + Guid.NewGuid().ToString("N")[..6],
            MaChuyenKhoa = bacSi.MaChuyenKhoa,
            TrangThai = "HoatDong"
        };
        db.PhongKhams.Add(testPhong);
        await db.SaveChangesAsync();

        // Tạo bệnh nhân đã có sẵn 1 lần vắng hẹn trước đó
        var bn = new BenhNhan
        {
            TenBenhNhan = "Bệnh Nhân Test Vắng Hẹn",
            SoDienThoai = "0955" + Random.Shared.Next(100000, 999999),
            SoLanVangHen = 1,
            CoCanhBao = false
        };
        db.BenhNhans.Add(bn);
        await db.SaveChangesAsync();

        // Tạo ca trực và khung giờ có giờ hẹn cách đây 45 phút (> 30 phút so với giờ hẹn)
        var gioHenQuaHan = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(-45));
        var ca = new CaTruc
        {
            MaBacSi = bacSi.MaBacSi,
            MaPhong = testPhong.MaPhong,
            NgayTruc = today,
            CaLamViec = "CaVang_" + Guid.NewGuid().ToString("N")[..6],
            GioBatDauCa = gioHenQuaHan,
            GioKetThucCa = gioHenQuaHan.AddHours(1),
            ThoiLuongMoiKhungGio = 15
        };
        db.CaTrucs.Add(ca);
        await db.SaveChangesAsync();

        var kg = new KhungGio
        {
            MaCa = ca.MaCa,
            GioBatDau = gioHenQuaHan,
            GioKetThuc = gioHenQuaHan.AddMinutes(15),
            LoaiKhungGio = "DatHen",
            TrangThai = "DaDat"
        };
        db.KhungGios.Add(kg);
        await db.SaveChangesAsync();

        // Phiếu hẹn ở trạng thái DaXacNhan nhưng chưa check-in
        var phieu = new PhieuDatHen
        {
            MaDatHen = $"BK{today:yyMMdd}-{Random.Shared.Next(100, 999):D3}{Random.Shared.Next(10, 99)}",
            KenhDatLich = "Online",
            ThoiGianTaoPhieu = DateTime.Now.AddHours(-3),
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn.MaBenhNhan,
            MaNguoiTao = admin.MaTaiKhoan,
            MaKhungGio = kg.MaKhungGio
        };
        db.PhieuDatHens.Add(phieu);
        await db.SaveChangesAsync();

        // Chạy tiến trình quét vắng hẹn
        var (soVang, _) = await vangHenService.QuetVaXuLyAsync();
        Assert.True(soVang >= 1, "Phải quét được ít nhất 1 phiếu vắng hẹn");

        // Kiểm tra phiếu hẹn đã chuyển thành VangHen
        var phieuSauQuet = await db.PhieuDatHens.FindAsync(phieu.MaPhieuHen);
        Assert.Equal("VangHen", phieuSauQuet!.TrangThaiHen);

        // Kiểm tra bệnh nhân: SoLanVangHen tăng lên 2, CoCanhBao được bật thành true
        var bnSauQuet = await db.BenhNhans.FindAsync(bn.MaBenhNhan);
        Assert.NotNull(bnSauQuet);
        Assert.Equal(2, bnSauQuet.SoLanVangHen);
        Assert.True(bnSauQuet.CoCanhBao, "Bệnh nhân có 2 lần vắng hẹn phải được bật cờ CoCanhBao = true");
    }
}
