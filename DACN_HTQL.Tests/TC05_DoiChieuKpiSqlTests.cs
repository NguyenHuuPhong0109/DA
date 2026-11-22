using Microsoft.Data.SqlClient;
using DACN_HTQL.Models.ViewModels;
using DACN_HTQL.Services;
using Xunit;

namespace DACN_HTQL.Tests;

public class TC05_DoiChieuKpiSqlTests
{
    public TC05_DoiChieuKpiSqlTests()
    {
        TestDbFixture.EnsureDatabaseInitializedAsync().Wait();
    }

    [Fact]
    public async Task DoiChieu4ChiSoKPI_GiuaBaoCaoServiceVaSqlThuan_Khop100PhanTram()
    {
        using var db = TestDbFixture.CreateContext();
        var baoCaoService = new BaoCaoService(db);

        var tuNgay = DateTime.Today.AddDays(-14);
        var denNgay = DateTime.Today;

        var filter = new BaoCaoFilterVM
        {
            TuNgay = tuNgay,
            DenNgay = denNgay,
            MaChuyenKhoa = null,
            MaBacSi = null
        };

        // 1. Lấy kết quả KPI từ BaoCaoService (C# LINQ)
        var baoCaoResult = await baoCaoService.LayBaoCaoDashboardAsync(filter);
        var kpiCsharp = baoCaoResult.KPI;

        // 2. Chạy 4 câu lệnh SQL thuần từ file Verify_KPI_SQL.sql qua ADO.NET trực tiếp trên SQL Server
        await using var conn = new SqlConnection(TestDbFixture.ConnectionString);
        await conn.OpenAsync();

        // --- SQL KPI 1: Tỷ lệ vắng hẹn ---
        var sqlKpi1 = @"
            DECLARE @TuNgay DATE = @pTuNgay;
            DECLARE @DenNgay DATE = @pDenNgay;
            SELECT 
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
                ) AS TyLeVangHen
            FROM PhieuDatHen p
            JOIN KhungGio kg ON p.MaKhungGio = kg.MaKhungGio
            JOIN CaTruc c ON kg.MaCa = c.MaCa
            WHERE c.NgayTruc BETWEEN @TuNgay AND @DenNgay;";

        await using var cmd1 = new SqlCommand(sqlKpi1, conn);
        cmd1.Parameters.AddWithValue("@pTuNgay", DateOnly.FromDateTime(tuNgay));
        cmd1.Parameters.AddWithValue("@pDenNgay", DateOnly.FromDateTime(denNgay));
        await using var reader1 = await cmd1.ExecuteReaderAsync();
        Assert.True(await reader1.ReadAsync());
        int sqlSoVang = reader1.GetInt32(0);
        int sqlMauSoVang = reader1.GetInt32(1);
        double sqlTyLeVang = reader1.GetDouble(2);
        await reader1.CloseAsync();

        // --- SQL KPI 2: Tỷ lệ hoàn thành khám ---
        var sqlKpi2 = @"
            DECLARE @TuNgay DATE = @pTuNgay;
            DECLARE @DenNgay DATE = @pDenNgay;
            SELECT 
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
                ) AS TyLeHoanThanh
            FROM PhieuDatHen p
            JOIN KhungGio kg ON p.MaKhungGio = kg.MaKhungGio
            JOIN CaTruc c ON kg.MaCa = c.MaCa
            WHERE c.NgayTruc BETWEEN @TuNgay AND @DenNgay;";

        await using var cmd2 = new SqlCommand(sqlKpi2, conn);
        cmd2.Parameters.AddWithValue("@pTuNgay", DateOnly.FromDateTime(tuNgay));
        cmd2.Parameters.AddWithValue("@pDenNgay", DateOnly.FromDateTime(denNgay));
        await using var reader2 = await cmd2.ExecuteReaderAsync();
        Assert.True(await reader2.ReadAsync());
        int sqlSoDaKham = reader2.GetInt32(0);
        int sqlTongPhieu = reader2.GetInt32(1);
        double sqlTyLeHoanThanh = reader2.GetDouble(2);
        await reader2.CloseAsync();

        // --- SQL KPI 3: Thời gian chờ trung bình ---
        var sqlKpi3 = @"
            DECLARE @TuNgay DATE = @pTuNgay;
            DECLARE @DenNgay DATE = @pDenNgay;
            SELECT 
                COUNT(*) AS SoLuotDaGoiSo,
                CAST(
                    CASE 
                        WHEN COUNT(*) = 0 THEN 0.0
                        ELSE ROUND(
                            AVG(CAST(DATEDIFF(SECOND, stt.ThoiGianCheckIn, stt.ThoiGianGoiSo) AS FLOAT) / 60.0), 
                            1
                        )
                    END AS FLOAT
                ) AS ThoiGianChoTrungBinh
            FROM PhieuSoThuTu stt
            WHERE stt.NgayCapSo BETWEEN @TuNgay AND @DenNgay
              AND stt.ThoiGianGoiSo IS NOT NULL
              AND stt.ThoiGianGoiSo >= stt.ThoiGianCheckIn;";

        await using var cmd3 = new SqlCommand(sqlKpi3, conn);
        cmd3.Parameters.AddWithValue("@pTuNgay", DateOnly.FromDateTime(tuNgay));
        cmd3.Parameters.AddWithValue("@pDenNgay", DateOnly.FromDateTime(denNgay));
        await using var reader3 = await cmd3.ExecuteReaderAsync();
        Assert.True(await reader3.ReadAsync());
        int sqlSoLuotDaGoi = reader3.GetInt32(0);
        double sqlThoiGianCho = reader3.GetDouble(1);
        await reader3.CloseAsync();

        // --- SQL KPI 4: Tỷ lệ lấp đầy khung giờ ---
        var sqlKpi4 = @"
            DECLARE @TuNgay DATE = @pTuNgay;
            DECLARE @DenNgay DATE = @pDenNgay;
            SELECT 
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
                ) AS TyLeLapDay
            FROM KhungGio kg
            JOIN CaTruc c ON kg.MaCa = c.MaCa
            LEFT JOIN PhieuDatHen pdh ON kg.MaKhungGio = pdh.MaKhungGio
            WHERE c.NgayTruc BETWEEN @TuNgay AND @DenNgay
              AND kg.LoaiKhungGio = N'DatHen';";

        await using var cmd4 = new SqlCommand(sqlKpi4, conn);
        cmd4.Parameters.AddWithValue("@pTuNgay", DateOnly.FromDateTime(tuNgay));
        cmd4.Parameters.AddWithValue("@pDenNgay", DateOnly.FromDateTime(denNgay));
        await using var reader4 = await cmd4.ExecuteReaderAsync();
        Assert.True(await reader4.ReadAsync());
        int sqlSoKhungDaDat = reader4.GetInt32(0);
        int sqlTongKhungDatHen = reader4.GetInt32(1);
        double sqlTyLeLapDay = reader4.GetDouble(2);
        await reader4.CloseAsync();

        // 3. Khẳng định đối chiếu khớp 100% giữa Service và SQL thuần
        // KPI 1: Tỷ lệ vắng hẹn
        Assert.Equal(sqlSoVang, kpiCsharp.SoPhieuVangHen);
        Assert.Equal(sqlMauSoVang, kpiCsharp.TongPhieuVangHenMauSo);
        Assert.Equal(sqlTyLeVang, kpiCsharp.TyLeVangHen);

        // KPI 2: Tỷ lệ hoàn thành khám
        Assert.Equal(sqlSoDaKham, kpiCsharp.SoPhieuDaKham);
        Assert.Equal(sqlTongPhieu, kpiCsharp.TongPhieuDatHenTrongKy);
        Assert.Equal(sqlTyLeHoanThanh, kpiCsharp.TyLeHoanThanh);

        // KPI 3: Thời gian chờ trung bình
        Assert.Equal(sqlSoLuotDaGoi, kpiCsharp.SoLuotDaGoiSo);
        Assert.Equal(sqlThoiGianCho, kpiCsharp.ThoiGianChoTrungBinh);

        // KPI 4: Tỷ lệ lấp đầy khung giờ
        Assert.Equal(sqlSoKhungDaDat, kpiCsharp.SoKhungGioDaDat);
        Assert.Equal(sqlTongKhungDatHen, kpiCsharp.TongKhungGioDatHen);
        Assert.Equal(sqlTyLeLapDay, kpiCsharp.TyLeLapDay);
    }
}
