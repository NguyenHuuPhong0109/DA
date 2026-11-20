IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [BenhNhan] (
        [MaBenhNhan] int NOT NULL IDENTITY,
        [TenBenhNhan] nvarchar(100) NOT NULL,
        [SoDienThoai] nvarchar(15) NOT NULL,
        [NamSinh] int NULL,
        [GioiTinh] nvarchar(10) NULL,
        [Email] nvarchar(100) NULL,
        [SoLanVangHen] int NOT NULL,
        [CoCanhBao] AS CAST(CASE WHEN [SoLanVangHen] >= 2 THEN 1 ELSE 0 END AS BIT) PERSISTED,
        CONSTRAINT [PK_BenhNhan] PRIMARY KEY ([MaBenhNhan]),
        CONSTRAINT [CK_BenhNhan_SoLanVangHen] CHECK ([SoLanVangHen] >= 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [ChuyenKhoa] (
        [MaChuyenKhoa] int NOT NULL IDENTITY,
        [TenChuyenKhoa] nvarchar(100) NOT NULL,
        [MoTa] nvarchar(500) NULL,
        [TrangThai] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_ChuyenKhoa] PRIMARY KEY ([MaChuyenKhoa])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [VaiTro] (
        [MaVaiTro] int NOT NULL IDENTITY,
        [TenVaiTro] nvarchar(50) NOT NULL,
        [MoTa] nvarchar(500) NULL,
        CONSTRAINT [PK_VaiTro] PRIMARY KEY ([MaVaiTro])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [PhongKham] (
        [MaPhong] int NOT NULL IDENTITY,
        [TenPhong] nvarchar(100) NOT NULL,
        [ViTri] nvarchar(200) NULL,
        [MaChuyenKhoa] int NOT NULL,
        [TrangThai] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_PhongKham] PRIMARY KEY ([MaPhong]),
        CONSTRAINT [FK_PhongKham_ChuyenKhoa_MaChuyenKhoa] FOREIGN KEY ([MaChuyenKhoa]) REFERENCES [ChuyenKhoa] ([MaChuyenKhoa]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [TaiKhoan] (
        [MaTaiKhoan] int NOT NULL IDENTITY,
        [TenTaiKhoan] nvarchar(100) NOT NULL,
        [MatKhau] nvarchar(255) NOT NULL,
        [SoDienThoai] nvarchar(15) NULL,
        [TrangThai] nvarchar(20) NOT NULL,
        [MaVaiTro] int NOT NULL,
        CONSTRAINT [PK_TaiKhoan] PRIMARY KEY ([MaTaiKhoan]),
        CONSTRAINT [FK_TaiKhoan_VaiTro_MaVaiTro] FOREIGN KEY ([MaVaiTro]) REFERENCES [VaiTro] ([MaVaiTro]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [BacSi] (
        [MaBacSi] int NOT NULL IDENTITY,
        [HoTenBacSi] nvarchar(100) NOT NULL,
        [HocVi] nvarchar(50) NULL,
        [SoDienThoai] nvarchar(15) NULL,
        [MaChuyenKhoa] int NOT NULL,
        [MaTaiKhoan] int NOT NULL,
        [TrangThai] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_BacSi] PRIMARY KEY ([MaBacSi]),
        CONSTRAINT [FK_BacSi_ChuyenKhoa_MaChuyenKhoa] FOREIGN KEY ([MaChuyenKhoa]) REFERENCES [ChuyenKhoa] ([MaChuyenKhoa]) ON DELETE NO ACTION,
        CONSTRAINT [FK_BacSi_TaiKhoan_MaTaiKhoan] FOREIGN KEY ([MaTaiKhoan]) REFERENCES [TaiKhoan] ([MaTaiKhoan]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
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
        [TrangThaiCa] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_CaTruc] PRIMARY KEY ([MaCa]),
        CONSTRAINT [CK_CaTruc_GioHopLe] CHECK ([GioBatDauCa] < [GioKetThucCa]),
        CONSTRAINT [FK_CaTruc_BacSi_MaBacSi] FOREIGN KEY ([MaBacSi]) REFERENCES [BacSi] ([MaBacSi]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CaTruc_PhongKham_MaPhong] FOREIGN KEY ([MaPhong]) REFERENCES [PhongKham] ([MaPhong]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [KhungGio] (
        [MaKhungGio] int NOT NULL IDENTITY,
        [MaCa] int NOT NULL,
        [GioBatDau] time NOT NULL,
        [GioKetThuc] time NOT NULL,
        [LoaiKhungGio] nvarchar(20) NOT NULL,
        [TrangThai] nvarchar(20) NOT NULL,
        [PhienBanDong] rowversion NULL,
        CONSTRAINT [PK_KhungGio] PRIMARY KEY ([MaKhungGio]),
        CONSTRAINT [CK_KhungGio_Loai] CHECK ([LoaiKhungGio] IN (N'DatHen', N'KhungDem')),
        CONSTRAINT [CK_KhungGio_TrangThai] CHECK ([TrangThai] IN (N'ConTrong', N'DaDat', N'TamKhoa')),
        CONSTRAINT [FK_KhungGio_CaTruc_MaCa] FOREIGN KEY ([MaCa]) REFERENCES [CaTruc] ([MaCa]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE TABLE [PhieuDatHen] (
        [MaPhieuHen] int NOT NULL IDENTITY,
        [MaDatHen] nvarchar(20) NOT NULL,
        [KenhDatLich] nvarchar(30) NOT NULL,
        [LyDoKhamSoBo] nvarchar(500) NULL,
        [ThoiGianTaoPhieu] datetime2 NOT NULL,
        [GuiNhacLich] bit NOT NULL,
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
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
        [SoLanGoi] int NOT NULL,
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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BacSi_MaChuyenKhoa] ON [BacSi] ([MaChuyenKhoa]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_BacSi_MaTaiKhoan] ON [BacSi] ([MaTaiKhoan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_BenhNhan_SoDienThoai] ON [BenhNhan] ([SoDienThoai]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_CaTruc_BacSi_Ngay_Ca] ON [CaTruc] ([MaBacSi], [NgayTruc], [CaLamViec]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_CaTruc_Phong_Ngay_Ca] ON [CaTruc] ([MaPhong], [NgayTruc], [CaLamViec]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_ChuyenKhoa_TenChuyenKhoa] ON [ChuyenKhoa] ([TenChuyenKhoa]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_KhungGio_Ca_GioBatDau] ON [KhungGio] ([MaCa], [GioBatDau]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PhieuDatHen_MaBenhNhan] ON [PhieuDatHen] ([MaBenhNhan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PhieuDatHen_MaNguoiTao] ON [PhieuDatHen] ([MaNguoiTao]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PhieuDatHen_KhungGio_HieuLuc] ON [PhieuDatHen] ([MaKhungGio]) WHERE [TrangThaiHen] NOT IN (N''DaHuy'')');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PhieuDatHen_MaDatHen] ON [PhieuDatHen] ([MaDatHen]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PhieuSoThuTu_MaBenhNhan] ON [PhieuSoThuTu] ([MaBenhNhan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PhieuSoThuTu_MaPhong] ON [PhieuSoThuTu] ([MaPhong]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_PhieuSTT_MaPhieuHen] ON [PhieuSoThuTu] ([MaPhieuHen]) WHERE [MaPhieuHen] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PhongKham_MaChuyenKhoa] ON [PhongKham] ([MaChuyenKhoa]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_PhongKham_TenPhong] ON [PhongKham] ([TenPhong]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TaiKhoan_MaVaiTro] ON [TaiKhoan] ([MaVaiTro]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_TaiKhoan_TenTaiKhoan] ON [TaiKhoan] ([TenTaiKhoan]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_VaiTro_TenVaiTro] ON [VaiTro] ([TenVaiTro]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261119104438_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261119104438_InitialCreate', N'11.0.0');
END;

COMMIT;
GO

