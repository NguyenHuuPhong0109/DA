using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBookingMVC.Migrations;

/// <inheritdoc />
public partial class _20261119104438_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BenhNhan",
            columns: table => new
            {
                MaBenhNhan = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenBenhNhan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                NamSinh = table.Column<int>(type: "int", nullable: true),
                GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                SoLanVangHen = table.Column<int>(type: "int", nullable: false),
                CoCanhBao = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CAST(CASE WHEN [SoLanVangHen] >= 2 THEN 1 ELSE 0 END AS BIT)", stored: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BenhNhan", x => x.MaBenhNhan);
                table.CheckConstraint("CK_BenhNhan_SoLanVangHen", "[SoLanVangHen] >= 0");
            });

        migrationBuilder.CreateTable(
            name: "ChuyenKhoa",
            columns: table => new
            {
                MaChuyenKhoa = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenChuyenKhoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChuyenKhoa", x => x.MaChuyenKhoa);
            });

        migrationBuilder.CreateTable(
            name: "VaiTro",
            columns: table => new
            {
                MaVaiTro = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
            });

        migrationBuilder.CreateTable(
            name: "PhongKham",
            columns: table => new
            {
                MaPhong = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenPhong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ViTri = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                MaChuyenKhoa = table.Column<int>(type: "int", nullable: false),
                TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PhongKham", x => x.MaPhong);
                table.ForeignKey(
                    name: "FK_PhongKham_ChuyenKhoa_MaChuyenKhoa",
                    column: x => x.MaChuyenKhoa,
                    principalTable: "ChuyenKhoa",
                    principalColumn: "MaChuyenKhoa",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TaiKhoan",
            columns: table => new
            {
                MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TenTaiKhoan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                MaVaiTro = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                table.ForeignKey(
                    name: "FK_TaiKhoan_VaiTro_MaVaiTro",
                    column: x => x.MaVaiTro,
                    principalTable: "VaiTro",
                    principalColumn: "MaVaiTro",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "BacSi",
            columns: table => new
            {
                MaBacSi = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                HoTenBacSi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                HocVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                MaChuyenKhoa = table.Column<int>(type: "int", nullable: false),
                MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BacSi", x => x.MaBacSi);
                table.ForeignKey(
                    name: "FK_BacSi_ChuyenKhoa_MaChuyenKhoa",
                    column: x => x.MaChuyenKhoa,
                    principalTable: "ChuyenKhoa",
                    principalColumn: "MaChuyenKhoa",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_BacSi_TaiKhoan_MaTaiKhoan",
                    column: x => x.MaTaiKhoan,
                    principalTable: "TaiKhoan",
                    principalColumn: "MaTaiKhoan",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CaTruc",
            columns: table => new
            {
                MaCa = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MaBacSi = table.Column<int>(type: "int", nullable: false),
                MaPhong = table.Column<int>(type: "int", nullable: false),
                NgayTruc = table.Column<DateOnly>(type: "date", nullable: false),
                CaLamViec = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                GioBatDauCa = table.Column<TimeOnly>(type: "time", nullable: false),
                GioKetThucCa = table.Column<TimeOnly>(type: "time", nullable: false),
                ThoiLuongMoiKhungGio = table.Column<int>(type: "int", nullable: false),
                TrangThaiCa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CaTruc", x => x.MaCa);
                table.CheckConstraint("CK_CaTruc_GioHopLe", "[GioBatDauCa] < [GioKetThucCa]");
                table.ForeignKey(
                    name: "FK_CaTruc_BacSi_MaBacSi",
                    column: x => x.MaBacSi,
                    principalTable: "BacSi",
                    principalColumn: "MaBacSi",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CaTruc_PhongKham_MaPhong",
                    column: x => x.MaPhong,
                    principalTable: "PhongKham",
                    principalColumn: "MaPhong",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "KhungGio",
            columns: table => new
            {
                MaKhungGio = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MaCa = table.Column<int>(type: "int", nullable: false),
                GioBatDau = table.Column<TimeOnly>(type: "time", nullable: false),
                GioKetThuc = table.Column<TimeOnly>(type: "time", nullable: false),
                LoaiKhungGio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                PhienBanDong = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_KhungGio", x => x.MaKhungGio);
                table.CheckConstraint("CK_KhungGio_Loai", "[LoaiKhungGio] IN (N'DatHen', N'KhungDem')");
                table.CheckConstraint("CK_KhungGio_TrangThai", "[TrangThai] IN (N'ConTrong', N'DaDat', N'TamKhoa')");
                table.ForeignKey(
                    name: "FK_KhungGio_CaTruc_MaCa",
                    column: x => x.MaCa,
                    principalTable: "CaTruc",
                    principalColumn: "MaCa",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PhieuDatHen",
            columns: table => new
            {
                MaPhieuHen = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                MaDatHen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                KenhDatLich = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                LyDoKhamSoBo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ThoiGianTaoPhieu = table.Column<DateTime>(type: "datetime2", nullable: false),
                GuiNhacLich = table.Column<bool>(type: "bit", nullable: false),
                TrangThaiHen = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                MaBenhNhan = table.Column<int>(type: "int", nullable: false),
                MaNguoiTao = table.Column<int>(type: "int", nullable: false),
                MaKhungGio = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PhieuDatHen", x => x.MaPhieuHen);
                table.CheckConstraint("CK_PhieuDatHen_TrangThai", "[TrangThaiHen] IN (N'ChoXacNhan', N'DaXacNhan', N'DaCheckIn', N'DaKham', N'DaHuy', N'VangHen')");
                table.ForeignKey(
                    name: "FK_PhieuDatHen_BenhNhan_MaBenhNhan",
                    column: x => x.MaBenhNhan,
                    principalTable: "BenhNhan",
                    principalColumn: "MaBenhNhan",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PhieuDatHen_KhungGio_MaKhungGio",
                    column: x => x.MaKhungGio,
                    principalTable: "KhungGio",
                    principalColumn: "MaKhungGio",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PhieuDatHen_TaiKhoan_MaNguoiTao",
                    column: x => x.MaNguoiTao,
                    principalTable: "TaiKhoan",
                    principalColumn: "MaTaiKhoan",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PhieuSoThuTu",
            columns: table => new
            {
                MaPhieu = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SoThuTu = table.Column<int>(type: "int", nullable: false),
                NgayCapSo = table.Column<DateOnly>(type: "date", nullable: false),
                LoaiKhach = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                MucUuTien = table.Column<int>(type: "int", nullable: false),
                GioHen = table.Column<TimeOnly>(type: "time", nullable: true),
                ThoiGianCheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                ThoiGianGoiSo = table.Column<DateTime>(type: "datetime2", nullable: true),
                ThoiGianBatDauKham = table.Column<DateTime>(type: "datetime2", nullable: true),
                ThoiGianHoanTat = table.Column<DateTime>(type: "datetime2", nullable: true),
                SoLanGoi = table.Column<int>(type: "int", nullable: false),
                TrangThaiPhieu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                MaPhong = table.Column<int>(type: "int", nullable: false),
                MaBenhNhan = table.Column<int>(type: "int", nullable: false),
                MaPhieuHen = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PhieuSoThuTu", x => x.MaPhieu);
                table.CheckConstraint("CK_PhieuSTT_MucUuTien", "[MucUuTien] IN (1, 2, 3)");
                table.CheckConstraint("CK_PhieuSTT_SoLanGoi", "[SoLanGoi] BETWEEN 0 AND 3");
                table.CheckConstraint("CK_PhieuSTT_TrangThai", "[TrangThaiPhieu] IN (N'DangCho', N'DangGoi', N'DangKham', N'QuaLuot', N'HoanTat')");
                table.ForeignKey(
                    name: "FK_PhieuSoThuTu_BenhNhan_MaBenhNhan",
                    column: x => x.MaBenhNhan,
                    principalTable: "BenhNhan",
                    principalColumn: "MaBenhNhan",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_PhieuSoThuTu_PhieuDatHen_MaPhieuHen",
                    column: x => x.MaPhieuHen,
                    principalTable: "PhieuDatHen",
                    principalColumn: "MaPhieuHen",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_PhieuSoThuTu_PhongKham_MaPhong",
                    column: x => x.MaPhong,
                    principalTable: "PhongKham",
                    principalColumn: "MaPhong",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BacSi_MaChuyenKhoa",
            table: "BacSi",
            column: "MaChuyenKhoa");

        migrationBuilder.CreateIndex(
            name: "UX_BacSi_MaTaiKhoan",
            table: "BacSi",
            column: "MaTaiKhoan",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_BenhNhan_SoDienThoai",
            table: "BenhNhan",
            column: "SoDienThoai",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_CaTruc_BacSi_Ngay_Ca",
            table: "CaTruc",
            columns: new[] { "MaBacSi", "NgayTruc", "CaLamViec" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_CaTruc_Phong_Ngay_Ca",
            table: "CaTruc",
            columns: new[] { "MaPhong", "NgayTruc", "CaLamViec" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_ChuyenKhoa_TenChuyenKhoa",
            table: "ChuyenKhoa",
            column: "TenChuyenKhoa",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_KhungGio_Ca_GioBatDau",
            table: "KhungGio",
            columns: new[] { "MaCa", "GioBatDau" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PhieuDatHen_MaBenhNhan",
            table: "PhieuDatHen",
            column: "MaBenhNhan");

        migrationBuilder.CreateIndex(
            name: "IX_PhieuDatHen_MaNguoiTao",
            table: "PhieuDatHen",
            column: "MaNguoiTao");

        migrationBuilder.CreateIndex(
            name: "UX_PhieuDatHen_KhungGio_HieuLuc",
            table: "PhieuDatHen",
            column: "MaKhungGio",
            unique: true,
            filter: "[TrangThaiHen] NOT IN (N'DaHuy')");

        migrationBuilder.CreateIndex(
            name: "UX_PhieuDatHen_MaDatHen",
            table: "PhieuDatHen",
            column: "MaDatHen",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PhieuSoThuTu_MaBenhNhan",
            table: "PhieuSoThuTu",
            column: "MaBenhNhan");

        migrationBuilder.CreateIndex(
            name: "IX_PhieuSoThuTu_MaPhong",
            table: "PhieuSoThuTu",
            column: "MaPhong");

        migrationBuilder.CreateIndex(
            name: "UX_PhieuSTT_MaPhieuHen",
            table: "PhieuSoThuTu",
            column: "MaPhieuHen",
            unique: true,
            filter: "[MaPhieuHen] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_PhongKham_MaChuyenKhoa",
            table: "PhongKham",
            column: "MaChuyenKhoa");

        migrationBuilder.CreateIndex(
            name: "UX_PhongKham_TenPhong",
            table: "PhongKham",
            column: "TenPhong",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_TaiKhoan_MaVaiTro",
            table: "TaiKhoan",
            column: "MaVaiTro");

        migrationBuilder.CreateIndex(
            name: "UX_TaiKhoan_TenTaiKhoan",
            table: "TaiKhoan",
            column: "TenTaiKhoan",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_VaiTro_TenVaiTro",
            table: "VaiTro",
            column: "TenVaiTro",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PhieuSoThuTu");

        migrationBuilder.DropTable(
            name: "PhieuDatHen");

        migrationBuilder.DropTable(
            name: "BenhNhan");

        migrationBuilder.DropTable(
            name: "KhungGio");

        migrationBuilder.DropTable(
            name: "CaTruc");

        migrationBuilder.DropTable(
            name: "BacSi");

        migrationBuilder.DropTable(
            name: "PhongKham");

        migrationBuilder.DropTable(
            name: "TaiKhoan");

        migrationBuilder.DropTable(
            name: "ChuyenKhoa");

        migrationBuilder.DropTable(
            name: "VaiTro");
    }
}
