-- ============================================================================
-- SCRIPT DEPLOY DATABASE CHO PRODUCTION / HOSTING (MONSTERASP.NET / SQL SERVER)
-- Lưu ý: Script không chứa lệnh CREATE DATABASE hay USE [Database]
--        Chạy trực tiếp 01 lần duy nhất trên cơ sở dữ liệu trống của hosting.
-- ============================================================================

-- 1. BẢNG THEO DÕI MIGRATION EF CORE
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

-- 2. TẠO CÁC BẢNG DỮ LIỆU CỐT LÕI (3NF)
IF OBJECT_ID(N'[BenhNhan]') IS NULL
BEGIN
    CREATE TABLE [BenhNhan] (
        [MaBenhNhan] int NOT NULL IDENTITY,
        [TenBenhNhan] nvarchar(100) NOT NULL,
        [SoDienThoai] nvarchar(15) NOT NULL,
        [NamSinh] int NULL,
        [GioiTinh] nvarchar(10) NULL,
        [Email] nvarchar(100) NULL,
        [SoLanVangHen] int NOT NULL CONSTRAINT [DF_BenhNhan_SoLanVangHen] DEFAULT 0,
        [CoCanhBao] AS CAST(CASE WHEN [SoLanVangHen] >= 2 THEN 1 ELSE 0 END AS BIT) PERSISTED,
        CONSTRAINT [PK_BenhNhan] PRIMARY KEY ([MaBenhNhan]),
        CONSTRAINT [CK_BenhNhan_SoLanVangHen] CHECK ([SoLanVangHen] >= 0)
    );
END;
GO

IF OBJECT_ID(N'[ChuyenKhoa]') IS NULL
BEGIN
    CREATE TABLE [ChuyenKhoa] (
        [MaChuyenKhoa] int NOT NULL IDENTITY,
        [TenChuyenKhoa] nvarchar(100) NOT NULL,
        [MoTa] nvarchar(500) NULL,
        [TrangThai] nvarchar(20) NOT NULL CONSTRAINT [DF_ChuyenKhoa_TrangThai] DEFAULT N'HoatDong',
        CONSTRAINT [PK_ChuyenKhoa] PRIMARY KEY ([MaChuyenKhoa])
    );
END;
GO

IF OBJECT_ID(N'[VaiTro]') IS NULL
BEGIN
    CREATE TABLE [VaiTro] (
        [MaVaiTro] int NOT NULL IDENTITY,
        [TenVaiTro] nvarchar(50) NOT NULL,
        [MoTa] nvarchar(500) NULL,
        CONSTRAINT [PK_VaiTro] PRIMARY KEY ([MaVaiTro])
    );
END;
GO

IF OBJECT_ID(N'[PhongKham]') IS NULL
BEGIN
    CREATE TABLE [PhongKham] (
        [MaPhong] int NOT NULL IDENTITY,
        [TenPhong] nvarchar(100) NOT NULL,
        [ViTri] nvarchar(200) NULL,
        [MaChuyenKhoa] int NOT NULL,
        [TrangThai] nvarchar(20) NOT NULL CONSTRAINT [DF_PhongKham_TrangThai] DEFAULT N'HoatDong',
        CONSTRAINT [PK_PhongKham] PRIMARY KEY ([MaPhong]),
        CONSTRAINT [FK_PhongKham_ChuyenKhoa_MaChuyenKhoa] FOREIGN KEY ([MaChuyenKhoa]) REFERENCES [ChuyenKhoa] ([MaChuyenKhoa]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[TaiKhoan]') IS NULL
BEGIN
    CREATE TABLE [TaiKhoan] (
        [MaTaiKhoan] int NOT NULL IDENTITY,
        [TenTaiKhoan] nvarchar(100) NOT NULL,
        [MatKhau] nvarchar(255) NOT NULL,
        [SoDienThoai] nvarchar(15) NULL,
        [TrangThai] nvarchar(20) NOT NULL CONSTRAINT [DF_TaiKhoan_TrangThai] DEFAULT N'HoatDong',
        [MaVaiTro] int NOT NULL,
        CONSTRAINT [PK_TaiKhoan] PRIMARY KEY ([MaTaiKhoan]),
        CONSTRAINT [FK_TaiKhoan_VaiTro_MaVaiTro] FOREIGN KEY ([MaVaiTro]) REFERENCES [VaiTro] ([MaVaiTro]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[BacSi]') IS NULL
BEGIN
    CREATE TABLE [BacSi] (
        [MaBacSi] int NOT NULL IDENTITY,
        [HoTenBacSi] nvarchar(100) NOT NULL,
        [HocVi] nvarchar(50) NULL,
        [SoDienThoai] nvarchar(15) NULL,
        [MaChuyenKhoa] int NOT NULL,
        [MaTaiKhoan] int NOT NULL,
        [TrangThai] nvarchar(20) NOT NULL CONSTRAINT [DF_BacSi_TrangThai] DEFAULT N'HoatDong',
        CONSTRAINT [PK_BacSi] PRIMARY KEY ([MaBacSi]),
        CONSTRAINT [FK_BacSi_ChuyenKhoa_MaChuyenKhoa] FOREIGN KEY ([MaChuyenKhoa]) REFERENCES [ChuyenKhoa] ([MaChuyenKhoa]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BacSi_TaiKhoan_MaTaiKhoan] FOREIGN KEY ([MaTaiKhoan]) REFERENCES [TaiKhoan] ([MaTaiKhoan]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[CaTruc]') IS NULL
BEGIN
    CREATE TABLE [CaTruc] (
        [MaCa] int NOT NULL IDENTITY,
        [MaBacSi] int NOT NULL,
        [MaPhong] int NOT NULL,
        [NgayTruc] date NOT NULL,
        [CaLamViec] nvarchar(20) NOT NULL,
        [GioBatDauCa] time NOT NULL,
        [GioKetThucCa] time NOT NULL,
        [ThoiLuongMoiKhungGio] int NOT NULL,
        [TrangThaiCa] nvarchar(20) NOT NULL CONSTRAINT [DF_CaTruc_TrangThaiCa] DEFAULT N'Mo',
        CONSTRAINT [PK_CaTruc] PRIMARY KEY ([MaCa]),
        CONSTRAINT [CK_CaTruc_GioHopLe] CHECK ([GioBatDauCa] < [GioKetThucCa]),
        CONSTRAINT [FK_CaTruc_BacSi_MaBacSi] FOREIGN KEY ([MaBacSi]) REFERENCES [BacSi] ([MaBacSi]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CaTruc_PhongKham_MaPhong] FOREIGN KEY ([MaPhong]) REFERENCES [PhongKham] ([MaPhong]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[KhungGio]') IS NULL
BEGIN
    CREATE TABLE [KhungGio] (
        [MaKhungGio] int NOT NULL IDENTITY,
        [MaCa] int NOT NULL,
        [GioBatDau] time NOT NULL,
        [GioKetThuc] time NOT NULL,
        [LoaiKhungGio] nvarchar(20) NOT NULL,
        [TrangThai] nvarchar(20) NOT NULL CONSTRAINT [DF_KhungGio_TrangThai] DEFAULT N'ConTrong',
        [PhienBanDong] rowversion NULL,
        CONSTRAINT [PK_KhungGio] PRIMARY KEY ([MaKhungGio]),
        CONSTRAINT [CK_KhungGio_Loai] CHECK ([LoaiKhungGio] IN (N'DatHen', N'KhungDem')),
        CONSTRAINT [CK_KhungGio_TrangThai] CHECK ([TrangThai] IN (N'ConTrong', N'DaDat', N'TamKhoa')),
        CONSTRAINT [FK_KhungGio_CaTruc_MaCa] FOREIGN KEY ([MaCa]) REFERENCES [CaTruc] ([MaCa]) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[PhieuDatHen]') IS NULL
BEGIN
    CREATE TABLE [PhieuDatHen] (
        [MaPhieuHen] int NOT NULL IDENTITY,
        [MaDatHen] nvarchar(20) NOT NULL,
        [KenhDatLich] nvarchar(30) NOT NULL,
        [LyDoKhamSoBo] nvarchar(500) NULL,
        [ThoiGianTaoPhieu] datetime2 NOT NULL,
        [GuiNhacLich] bit NOT NULL CONSTRAINT [DF_PhieuDatHen_GuiNhacLich] DEFAULT 0,
        [TrangThaiHen] nvarchar(20) NOT NULL,
        [MaBenhNhan] int NOT NULL,
        [MaNguoiTao] int NOT NULL,
        [MaKhungGio] int NOT NULL,
        CONSTRAINT [PK_PhieuDatHen] PRIMARY KEY ([MaPhieuHen]),
        CONSTRAINT [CK_PhieuDatHen_TrangThai] CHECK ([TrangThaiHen] IN (N'ChoXacNhan', N'DaXacNhan', N'DaCheckIn', N'DaKham', N'DaHuy', N'VangHen')),
        CONSTRAINT [FK_PhieuDatHen_BenhNhan_MaBenhNhan] FOREIGN KEY ([MaBenhNhan]) REFERENCES [BenhNhan] ([MaBenhNhan]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuDatHen_KhungGio_MaKhungGio] FOREIGN KEY ([MaKhungGio]) REFERENCES [KhungGio] ([MaKhungGio]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuDatHen_TaiKhoan_MaNguoiTao] FOREIGN KEY ([MaNguoiTao]) REFERENCES [TaiKhoan] ([MaTaiKhoan]) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'[PhieuSoThuTu]') IS NULL
BEGIN
    CREATE TABLE [PhieuSoThuTu] (
        [MaPhieu] int NOT NULL IDENTITY,
        [SoThuTu] int NOT NULL,
        [NgayCapSo] date NOT NULL,
        [LoaiKhach] nvarchar(20) NOT NULL,
        [MucUuTien] int NOT NULL,
        [GioHen] time NULL,
        [ThoiGianCheckIn] datetime2 NOT NULL,
        [ThoiGianGoiSo] datetime2 NULL,
        [ThoiGianBatDauKham] datetime2 NULL,
        [ThoiGianHoanTat] datetime2 NULL,
        [SoLanGoi] int NOT NULL CONSTRAINT [DF_PhieuSTT_SoLanGoi] DEFAULT 0,
        [TrangThaiPhieu] nvarchar(20) NOT NULL,
        [MaPhong] int NOT NULL,
        [MaBenhNhan] int NOT NULL,
        [MaPhieuHen] int NULL,
        CONSTRAINT [PK_PhieuSoThuTu] PRIMARY KEY ([MaPhieu]),
        CONSTRAINT [CK_PhieuSTT_MucUuTien] CHECK ([MucUuTien] IN (1, 2, 3)),
        CONSTRAINT [CK_PhieuSTT_SoLanGoi] CHECK ([SoLanGoi] BETWEEN 0 AND 3),
        CONSTRAINT [CK_PhieuSTT_TrangThai] CHECK ([TrangThaiPhieu] IN (N'DangCho', N'DangGoi', N'DangKham', N'QuaLuot', N'HoanTat')),
        CONSTRAINT [FK_PhieuSoThuTu_BenhNhan_MaBenhNhan] FOREIGN KEY ([MaBenhNhan]) REFERENCES [BenhNhan] ([MaBenhNhan]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PhieuSoThuTu_PhieuDatHen_MaPhieuHen] FOREIGN KEY ([MaPhieuHen]) REFERENCES [PhieuDatHen] ([MaPhieuHen]) ON DELETE SET NULL,
        CONSTRAINT [FK_PhieuSoThuTu_PhongKham_MaPhong] FOREIGN KEY ([MaPhong]) REFERENCES [PhongKham] ([MaPhong]) ON DELETE NO ACTION
    );
END;
GO

-- 3. TẠO INDEX VÀ UNIQUE FILTERED INDEXES
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_BenhNhan_SoDienThoai')
    CREATE UNIQUE INDEX [UX_BenhNhan_SoDienThoai] ON [BenhNhan] ([SoDienThoai]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_ChuyenKhoa_TenChuyenKhoa')
    CREATE UNIQUE INDEX [UX_ChuyenKhoa_TenChuyenKhoa] ON [ChuyenKhoa] ([TenChuyenKhoa]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_VaiTro_TenVaiTro')
    CREATE UNIQUE INDEX [UX_VaiTro_TenVaiTro] ON [VaiTro] ([TenVaiTro]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PhongKham_MaChuyenKhoa')
    CREATE INDEX [IX_PhongKham_MaChuyenKhoa] ON [PhongKham] ([MaChuyenKhoa]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_PhongKham_TenPhong')
    CREATE UNIQUE INDEX [UX_PhongKham_TenPhong] ON [PhongKham] ([TenPhong]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_TaiKhoan_MaVaiTro')
    CREATE INDEX [IX_TaiKhoan_MaVaiTro] ON [TaiKhoan] ([MaVaiTro]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_TaiKhoan_TenTaiKhoan')
    CREATE UNIQUE INDEX [UX_TaiKhoan_TenTaiKhoan] ON [TaiKhoan] ([TenTaiKhoan]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_BacSi_MaChuyenKhoa')
    CREATE INDEX [IX_BacSi_MaChuyenKhoa] ON [BacSi] ([MaChuyenKhoa]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_BacSi_MaTaiKhoan')
    CREATE UNIQUE INDEX [UX_BacSi_MaTaiKhoan] ON [BacSi] ([MaTaiKhoan]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_CaTruc_BacSi_Ngay_Ca')
    CREATE UNIQUE INDEX [UX_CaTruc_BacSi_Ngay_Ca] ON [CaTruc] ([MaBacSi], [NgayTruc], [CaLamViec]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_CaTruc_Phong_Ngay_Ca')
    CREATE UNIQUE INDEX [UX_CaTruc_Phong_Ngay_Ca] ON [CaTruc] ([MaPhong], [NgayTruc], [CaLamViec]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_KhungGio_Ca_GioBatDau')
    CREATE UNIQUE INDEX [UX_KhungGio_Ca_GioBatDau] ON [KhungGio] ([MaCa], [GioBatDau]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PhieuDatHen_MaBenhNhan')
    CREATE INDEX [IX_PhieuDatHen_MaBenhNhan] ON [PhieuDatHen] ([MaBenhNhan]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PhieuDatHen_MaNguoiTao')
    CREATE INDEX [IX_PhieuDatHen_MaNguoiTao] ON [PhieuDatHen] ([MaNguoiTao]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_PhieuDatHen_KhungGio_HieuLuc')
    CREATE UNIQUE INDEX [UX_PhieuDatHen_KhungGio_HieuLuc] ON [PhieuDatHen] ([MaKhungGio]) WHERE [TrangThaiHen] NOT IN (N'DaHuy');

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_PhieuDatHen_MaDatHen')
    CREATE UNIQUE INDEX [UX_PhieuDatHen_MaDatHen] ON [PhieuDatHen] ([MaDatHen]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PhieuSoThuTu_MaBenhNhan')
    CREATE INDEX [IX_PhieuSoThuTu_MaBenhNhan] ON [PhieuSoThuTu] ([MaBenhNhan]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PhieuSoThuTu_MaPhong')
    CREATE INDEX [IX_PhieuSoThuTu_MaPhong] ON [PhieuSoThuTu] ([MaPhong]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_PhieuSTT_MaPhieuHen')
    CREATE UNIQUE INDEX [UX_PhieuSTT_MaPhieuHen] ON [PhieuSoThuTu] ([MaPhieuHen]) WHERE [MaPhieuHen] IS NOT NULL;
GO

-- 4. GHI NHẬN MIGRATION HISTORY CHO EF CORE
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20261119104438_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261119104438_InitialCreate', N'11.0.0');
END;
GO

-- 5. NẠP DỮ LIỆU CƠ BẢN (CHỈ NẠP NẾU BẢNG VaiTro RỖNG)
IF NOT EXISTS (SELECT 1 FROM [VaiTro])
BEGIN
    -- A. Vai trò
    SET IDENTITY_INSERT [VaiTro] ON;
    INSERT INTO [VaiTro] ([MaVaiTro], [TenVaiTro], [MoTa]) VALUES
    (1, N'Admin', N'Quản trị viên toàn quyền hệ thống'),
    (2, N'LeTan', N'Nhân viên lễ tân tiếp nhận & phân luồng hàng đợi'),
    (3, N'BacSi', N'Bác sĩ khám bệnh & điều khiển buồng khám'),
    (4, N'BenhNhan', N'Bệnh nhân đăng ký & đặt lịch trực tuyến');
    SET IDENTITY_INSERT [VaiTro] OFF;

    -- B. Tài khoản demo (Mật khẩu: 123456 - Mã băm BCrypt chuẩn)
    SET IDENTITY_INSERT [TaiKhoan] ON;
    INSERT INTO [TaiKhoan] ([MaTaiKhoan], [TenTaiKhoan], [MatKhau], [SoDienThoai], [TrangThai], [MaVaiTro]) VALUES
    (1, N'admin',       N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0900000001', N'HoatDong', 1),
    (2, N'letan01',     N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0900000002', N'HoatDong', 2),
    (3, N'bacsi01',     N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0900000003', N'HoatDong', 3),
    (4, N'bacsi02',     N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0900000004', N'HoatDong', 3),
    (5, N'bacsi03',     N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0900000005', N'HoatDong', 3),
    (6, N'bacsi04',     N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0900000006', N'HoatDong', 3),
    (7, N'benhnhan01',  N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0901000001', N'HoatDong', 4),
    (8, N'benhnhan02',  N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0901000002', N'HoatDong', 4),
    (9, N'benhnhan03',  N'$2a$11$MsAbwKSyL/l6fcrYb3wjQuxPfS4S9R3sKT.8T8gvU3XMgSWFaFiR6', N'0901000003', N'HoatDong', 4);
    SET IDENTITY_INSERT [TaiKhoan] OFF;

    -- C. Chuyên khoa
    SET IDENTITY_INSERT [ChuyenKhoa] ON;
    INSERT INTO [ChuyenKhoa] ([MaChuyenKhoa], [TenChuyenKhoa], [MoTa], [TrangThai]) VALUES
    (1, N'Nội tổng quát', N'Khám và điều trị các bệnh lý nội khoa người lớn', N'HoatDong'),
    (2, N'Tai Mũi Họng',  N'Khám và điều trị chuyên sâu bệnh lý tai mũi họng', N'HoatDong'),
    (3, N'Da liễu',       N'Chẩn đoán và điều trị bệnh ngoài da, dị ứng', N'HoatDong'),
    (4, N'Nhi khoa',      N'Chăm sóc và điều trị bệnh lý ở trẻ em', N'HoatDong');
    SET IDENTITY_INSERT [ChuyenKhoa] OFF;

    -- D. Phòng khám
    SET IDENTITY_INSERT [PhongKham] ON;
    INSERT INTO [PhongKham] ([MaPhong], [TenPhong], [ViTri], [MaChuyenKhoa], [TrangThai]) VALUES
    (1, N'Phòng 101', N'Tầng 1 - Khu A', 1, N'HoatDong'),
    (2, N'Phòng 102', N'Tầng 1 - Khu B', 2, N'HoatDong'),
    (3, N'Phòng 103', N'Tầng 2 - Khu A', 3, N'HoatDong'),
    (4, N'Phòng 104', N'Tầng 2 - Khu B', 4, N'HoatDong');
    SET IDENTITY_INSERT [PhongKham] OFF;

    -- E. Bác sĩ
    SET IDENTITY_INSERT [BacSi] ON;
    INSERT INTO [BacSi] ([MaBacSi], [HoTenBacSi], [HocVi], [SoDienThoai], [MaChuyenKhoa], [MaTaiKhoan], [TrangThai]) VALUES
    (1, N'Nguyễn Văn An',  N'BS. CKI', N'0900000003', 1, 3, N'HoatDong'),
    (2, N'Trần Thị Bình',  N'ThS. BS', N'0900000004', 2, 4, N'HoatDong'),
    (3, N'Lê Hoàng Cường', N'BS. CKII',N'0900000005', 3, 5, N'HoatDong'),
    (4, N'Phạm Minh Đức',  N'BS',      N'0900000006', 4, 6, N'HoatDong');
    SET IDENTITY_INSERT [BacSi] OFF;

    -- F. Bệnh nhân mẫu (Gồm 3 tài khoản demo và các ca cảnh báo)
    SET IDENTITY_INSERT [BenhNhan] ON;
    INSERT INTO [BenhNhan] ([MaBenhNhan], [TenBenhNhan], [SoDienThoai], [NamSinh], [GioiTinh], [Email], [SoLanVangHen]) VALUES
    (1, N'Nguyễn Thị Demo 1',  N'0901000001', 1990, N'Nữ',  N'benhnhan01@gmail.com', 0),
    (2, N'Trần Văn Demo 2',    N'0901000002', 1985, N'Nam', N'benhnhan02@gmail.com', 0),
    (3, N'Lê Minh Demo 3',     N'0901000003', 1995, N'Nam', N'benhnhan03@gmail.com', 1),
    (4, N'Hoàng Cảnh Báo 1',   N'0902000001', 1988, N'Nam', N'canhbao1@gmail.com',   2),
    (5, N'Vũ Cảnh Báo 2',      N'0902000002', 1992, N'Nữ',  N'canhbao2@gmail.com',   3),
    (6, N'Đỗ Cảnh Báo 3',      N'0902000003', 1980, N'Nam', N'canhbao3@gmail.com',   2);
    SET IDENTITY_INSERT [BenhNhan] OFF;
END;
GO

PRINT N'Hoàn tất cấu hình Schema và Dữ liệu mẫu chuẩn cho DACN_HTQL!';
GO
