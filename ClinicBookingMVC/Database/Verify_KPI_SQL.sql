-- ==============================================================================================
-- FILE: Verify_KPI_SQL.sql
-- MỤC ĐÍCH: Đối chiếu 4 chỉ số KPI cốt lõi giữa câu lệnh SQL Server thuần và BaoCaoService.cs
-- ĐẢM BẢO CÔNG THỨC KHỚP 100% VỚI LOGIC LINQ TRONG BaoCaoService.cs
-- ==============================================================================================

USE [ClinicBookingDB]; -- hoặc [ClinicBookingTestDB] khi chạy tự động trong test
GO

-- Khai báo tham số thời gian lọc (Mặc định: 14 ngày trước đến hôm nay, khớp logic BaoCaoService)
DECLARE @TuNgay DATE = DATEADD(DAY, -14, CAST(GETDATE() AS DATE));
DECLARE @DenNgay DATE = CAST(GETDATE() AS DATE);
DECLARE @MaChuyenKhoa INT = NULL; -- Tuỳ chọn lọc chuyên khoa (>0)
DECLARE @MaBacSi INT = NULL;      -- Tuỳ chọn lọc bác sĩ (>0)

PRINT '==============================================================================================';
PRINT 'BÁO CÁO ĐỐI CHIẾU 4 CHỈ SỐ KPI HOẠT ĐỘNG PHÒNG KHÁM (SQL THUẦN)';
PRINT 'Khoảng thời gian: ' + CONVERT(VARCHAR(10), @TuNgay, 103) + ' - ' + CONVERT(VARCHAR(10), @DenNgay, 103);
PRINT '==============================================================================================';

-- ----------------------------------------------------------------------------------------------
-- KPI 1: TỶ LỆ VẮNG HẸN (No-Show Rate %)
-- Công thức: Số phiếu VangHen / Tổng phiếu hợp lệ (DaXacNhan + DaCheckIn + DaKham + VangHen) * 100
-- Làm tròn 1 chữ số thập phân (ROUND(..., 1))
-- ----------------------------------------------------------------------------------------------
SELECT 
    'KPI 1: Tỷ lệ vắng hẹn (No-Show Rate %)' AS TenChiSo,
    COUNT(CASE WHEN p.TrangThaiHen = N'VangHen' THEN 1 END) AS SoPhieuVangHen,
    COUNT(CASE WHEN p.TrangThaiHen IN (N'DaXacNhan', N'DaCheckIn', N'DaKham', N'VangHen') THEN 1 END) AS TongPhieuHopLeMauSo,
    CAST(
        CASE 
            WHEN COUNT(CASE WHEN p.TrangThaiHen IN (N'DaXacNhan', N'DaCheckIn', N'DaKham', N'VangHen') THEN 1 END) = 0 THEN 0.0
            ELSE ROUND(
                CAST(COUNT(CASE WHEN p.TrangThaiHen = N'VangHen' THEN 1 END) AS FLOAT) * 100.0 / 
                COUNT(CASE WHEN p.TrangThaiHen IN (N'DaXacNhan', N'DaCheckIn', N'DaKham', N'VangHen') THEN 1 END), 
                1
            )
        END AS FLOAT
    ) AS GiaTriKPI_PhanTram
FROM PhieuDatHen p
JOIN KhungGio kg ON p.MaKhungGio = kg.MaKhungGio
JOIN CaTruc c ON kg.MaCa = c.MaCa
JOIN BacSi bs ON c.MaBacSi = bs.MaBacSi
WHERE c.NgayTruc BETWEEN @TuNgay AND @DenNgay
  AND (@MaChuyenKhoa IS NULL OR bs.MaChuyenKhoa = @MaChuyenKhoa)
  AND (@MaBacSi IS NULL OR c.MaBacSi = @MaBacSi);

-- ----------------------------------------------------------------------------------------------
-- KPI 2: TỶ LỆ HOÀN THÀNH KHÁM (%)
-- Công thức: Số phiếu DaKham / Tổng phiếu đặt hẹn trong kỳ * 100
-- Làm tròn 1 chữ số thập phân (ROUND(..., 1))
-- ----------------------------------------------------------------------------------------------
SELECT 
    'KPI 2: Tỷ lệ hoàn thành khám (%)' AS TenChiSo,
    COUNT(CASE WHEN p.TrangThaiHen = N'DaKham' THEN 1 END) AS SoPhieuDaKham,
    COUNT(*) AS TongPhieuDatHenTrongKy,
    CAST(
        CASE 
            WHEN COUNT(*) = 0 THEN 0.0
            ELSE ROUND(
                CAST(COUNT(CASE WHEN p.TrangThaiHen = N'DaKham' THEN 1 END) AS FLOAT) * 100.0 / 
                COUNT(*), 
                1
            )
        END AS FLOAT
    ) AS GiaTriKPI_PhanTram
FROM PhieuDatHen p
JOIN KhungGio kg ON p.MaKhungGio = kg.MaKhungGio
JOIN CaTruc c ON kg.MaCa = c.MaCa
JOIN BacSi bs ON c.MaBacSi = bs.MaBacSi
WHERE c.NgayTruc BETWEEN @TuNgay AND @DenNgay
  AND (@MaChuyenKhoa IS NULL OR bs.MaChuyenKhoa = @MaChuyenKhoa)
  AND (@MaBacSi IS NULL OR c.MaBacSi = @MaBacSi);

-- ----------------------------------------------------------------------------------------------
-- KPI 3: THỜI GIAN CHỜ KHÁM TRUNG BÌNH (Phút)
-- Công thức: AVG(DATEDIFF(SECOND, ThoiGianCheckIn, ThoiGianGoiSo) / 60.0) trên các lượt đã gọi
-- Làm tròn 1 chữ số thập phân (ROUND(..., 1))
-- ----------------------------------------------------------------------------------------------
SELECT 
    'KPI 3: Thời gian chờ khám trung bình (Phút)' AS TenChiSo,
    ROUND(SUM(CAST(DATEDIFF(SECOND, stt.ThoiGianCheckIn, stt.ThoiGianGoiSo) AS FLOAT) / 60.0), 1) AS TongSoPhutCho,
    COUNT(*) AS SoLuotDaGoiSo,
    CAST(
        CASE 
            WHEN COUNT(*) = 0 THEN 0.0
            ELSE ROUND(
                AVG(CAST(DATEDIFF(SECOND, stt.ThoiGianCheckIn, stt.ThoiGianGoiSo) AS FLOAT) / 60.0), 
                1
            )
        END AS FLOAT
    ) AS GiaTriKPI_Phut
FROM PhieuSoThuTu stt
JOIN PhongKham pk ON stt.MaPhong = pk.MaPhong
LEFT JOIN PhieuDatHen pdh ON stt.MaPhieuHen = pdh.MaPhieuHen
LEFT JOIN KhungGio kg ON pdh.MaKhungGio = kg.MaKhungGio
LEFT JOIN CaTruc c ON kg.MaCa = c.MaCa
WHERE stt.NgayCapSo BETWEEN @TuNgay AND @DenNgay
  AND stt.ThoiGianGoiSo IS NOT NULL
  AND stt.ThoiGianGoiSo >= stt.ThoiGianCheckIn
  AND (@MaChuyenKhoa IS NULL OR pk.MaChuyenKhoa = @MaChuyenKhoa)
  AND (@MaBacSi IS NULL OR c.MaBacSi = @MaBacSi);

-- ----------------------------------------------------------------------------------------------
-- KPI 4: TỶ LỆ LẤP ĐẦY KHUNG GIỜ (%)
-- Công thức: Số KhungGio (DatHen) đã đặt / Tổng KhungGio (DatHen) mở trong kỳ * 100
-- Làm tròn 1 chữ số thập phân (ROUND(..., 1))
-- ----------------------------------------------------------------------------------------------
SELECT 
    'KPI 4: Tỷ lệ lấp đầy khung giờ (%)' AS TenChiSo,
    COUNT(CASE WHEN kg.TrangThai IN (N'DaDat', N'TamKhoa') OR pdh.MaPhieuHen IS NOT NULL THEN 1 END) AS SoKhungGioDaDat,
    COUNT(*) AS TongKhungGioDatHen,
    CAST(
        CASE 
            WHEN COUNT(*) = 0 THEN 0.0
            ELSE ROUND(
                CAST(COUNT(CASE WHEN kg.TrangThai IN (N'DaDat', N'TamKhoa') OR pdh.MaPhieuHen IS NOT NULL THEN 1 END) AS FLOAT) * 100.0 / 
                COUNT(*), 
                1
            )
        END AS FLOAT
    ) AS GiaTriKPI_PhanTram
FROM KhungGio kg
JOIN CaTruc c ON kg.MaCa = c.MaCa
JOIN BacSi bs ON c.MaBacSi = bs.MaBacSi
LEFT JOIN PhieuDatHen pdh ON kg.MaKhungGio = pdh.MaKhungGio
WHERE c.NgayTruc BETWEEN @TuNgay AND @DenNgay
  AND kg.LoaiKhungGio = N'DatHen'
  AND (@MaChuyenKhoa IS NULL OR bs.MaChuyenKhoa = @MaChuyenKhoa)
  AND (@MaBacSi IS NULL OR c.MaBacSi = @MaBacSi);
GO
