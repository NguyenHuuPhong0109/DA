using Microsoft.EntityFrameworkCore;
using DACN_HTQL.Models.Entities;

namespace DACN_HTQL.Data;

public class ClinicDbContext : DbContext
{
    public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<VaiTro> VaiTros { get; set; }
    public DbSet<TaiKhoan> TaiKhoans { get; set; }
    public DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }
    public DbSet<PhongKham> PhongKhams { get; set; }
    public DbSet<BacSi> BacSis { get; set; }
    public DbSet<CaTruc> CaTrucs { get; set; }
    public DbSet<KhungGio> KhungGios { get; set; }
    public DbSet<BenhNhan> BenhNhans { get; set; }
    public DbSet<PhieuDatHen> PhieuDatHens { get; set; }
    public DbSet<PhieuSoThuTu> PhieuSoThuTus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===================== VaiTro =====================
        modelBuilder.Entity<VaiTro>(e =>
        {
            e.HasIndex(x => x.TenVaiTro).IsUnique().HasDatabaseName("UX_VaiTro_TenVaiTro");
        });

        // ===================== TaiKhoan =====================
        modelBuilder.Entity<TaiKhoan>(e =>
        {
            e.HasIndex(x => x.TenTaiKhoan).IsUnique().HasDatabaseName("UX_TaiKhoan_TenTaiKhoan");
            e.Property(x => x.MatKhau).HasMaxLength(255);
            e.HasOne(x => x.VaiTro)
             .WithMany(v => v.TaiKhoans)
             .HasForeignKey(x => x.MaVaiTro)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== ChuyenKhoa =====================
        modelBuilder.Entity<ChuyenKhoa>(e =>
        {
            e.HasIndex(x => x.TenChuyenKhoa).IsUnique().HasDatabaseName("UX_ChuyenKhoa_TenChuyenKhoa");
        });

        // ===================== PhongKham =====================
        modelBuilder.Entity<PhongKham>(e =>
        {
            e.HasIndex(x => x.TenPhong).IsUnique().HasDatabaseName("UX_PhongKham_TenPhong");
            e.HasOne(x => x.ChuyenKhoa)
             .WithMany(ck => ck.PhongKhams)
             .HasForeignKey(x => x.MaChuyenKhoa)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== BacSi =====================
        modelBuilder.Entity<BacSi>(e =>
        {
            e.HasIndex(x => x.MaTaiKhoan).IsUnique().HasDatabaseName("UX_BacSi_MaTaiKhoan");
            e.HasOne(x => x.ChuyenKhoa)
             .WithMany(ck => ck.BacSis)
             .HasForeignKey(x => x.MaChuyenKhoa)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TaiKhoan)
             .WithOne(tk => tk.BacSi)
             .HasForeignKey<BacSi>(x => x.MaTaiKhoan)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== CaTruc =====================
        modelBuilder.Entity<CaTruc>(e =>
        {
            // UNIQUE (MaBacSi, NgayTruc, CaLamViec)
            e.HasIndex(x => new { x.MaBacSi, x.NgayTruc, x.CaLamViec })
             .IsUnique()
             .HasDatabaseName("UX_CaTruc_BacSi_Ngay_Ca");
            // UNIQUE (MaPhong, NgayTruc, CaLamViec)
            e.HasIndex(x => new { x.MaPhong, x.NgayTruc, x.CaLamViec })
             .IsUnique()
             .HasDatabaseName("UX_CaTruc_Phong_Ngay_Ca");
            // CHECK GioBatDauCa < GioKetThucCa
            e.ToTable(tb => tb.HasCheckConstraint("CK_CaTruc_GioHopLe",
                "[GioBatDauCa] < [GioKetThucCa]"));
            e.HasOne(x => x.BacSi)
             .WithMany(bs => bs.CaTrucs)
             .HasForeignKey(x => x.MaBacSi)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PhongKham)
             .WithMany(pk => pk.CaTrucs)
             .HasForeignKey(x => x.MaPhong)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== KhungGio =====================
        modelBuilder.Entity<KhungGio>(e =>
        {
            // UNIQUE (MaCa, GioBatDau)
            e.HasIndex(x => new { x.MaCa, x.GioBatDau })
             .IsUnique()
             .HasDatabaseName("UX_KhungGio_Ca_GioBatDau");
            // CHECK LoaiKhungGio IN ('DatHen','KhungDem')
            e.ToTable(tb => tb.HasCheckConstraint("CK_KhungGio_Loai",
                "[LoaiKhungGio] IN (N'DatHen', N'KhungDem')"));
            // CHECK TrangThai IN ('ConTrong','DaDat','TamKhoa')
            e.ToTable(tb => tb.HasCheckConstraint("CK_KhungGio_TrangThai",
                "[TrangThai] IN (N'ConTrong', N'DaDat', N'TamKhoa')"));
            e.Property(x => x.PhienBanDong).IsRowVersion();
            e.HasOne(x => x.CaTruc)
             .WithMany(ca => ca.KhungGios)
             .HasForeignKey(x => x.MaCa)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ===================== BenhNhan =====================
        modelBuilder.Entity<BenhNhan>(e =>
        {
            e.HasIndex(x => x.SoDienThoai).IsUnique().HasDatabaseName("UX_BenhNhan_SoDienThoai");
            // CHECK SoLanVangHen >= 0
            e.ToTable(tb => tb.HasCheckConstraint("CK_BenhNhan_SoLanVangHen",
                "[SoLanVangHen] >= 0"));
            // CoCanhBao: Persisted Computed Column
            e.Property(x => x.CoCanhBao)
             .HasComputedColumnSql("CAST(CASE WHEN [SoLanVangHen] >= 2 THEN 1 ELSE 0 END AS BIT)", stored: true);
        });

        // ===================== PhieuDatHen =====================
        modelBuilder.Entity<PhieuDatHen>(e =>
        {
            e.HasIndex(x => x.MaDatHen).IsUnique().HasDatabaseName("UX_PhieuDatHen_MaDatHen");
            // CHECK TrangThaiHen IN (...)
            e.ToTable(tb => tb.HasCheckConstraint("CK_PhieuDatHen_TrangThai",
                "[TrangThaiHen] IN (N'ChoXacNhan', N'DaXacNhan', N'DaCheckIn', N'DaKham', N'DaHuy', N'VangHen')"));
            // Filtered Unique Index: 1 KhungGio chỉ có thể có 1 phiếu hẹn hiệu lực
            e.HasIndex(x => x.MaKhungGio)
             .IsUnique()
             .HasFilter("[TrangThaiHen] <> N'DaHuy'")
             .HasDatabaseName("UX_PhieuDatHen_KhungGio_HieuLuc");
            e.HasOne(x => x.BenhNhan)
             .WithMany(bn => bn.PhieuDatHens)
             .HasForeignKey(x => x.MaBenhNhan)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.NguoiTao)
             .WithMany(tk => tk.PhieuDatHensNguoiTao)
             .HasForeignKey(x => x.MaNguoiTao)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.KhungGio)
             .WithOne(kg => kg.PhieuDatHen)
             .HasForeignKey<PhieuDatHen>(x => x.MaKhungGio)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== PhieuSoThuTu =====================
        modelBuilder.Entity<PhieuSoThuTu>(e =>
        {
            // CHECK MucUuTien IN (1,2,3)
            e.ToTable(tb => tb.HasCheckConstraint("CK_PhieuSTT_MucUuTien",
                "[MucUuTien] IN (1, 2, 3)"));
            // CHECK SoLanGoi BETWEEN 0 AND 3
            e.ToTable(tb => tb.HasCheckConstraint("CK_PhieuSTT_SoLanGoi",
                "[SoLanGoi] BETWEEN 0 AND 3"));
            // CHECK TrangThaiPhieu IN (...)
            e.ToTable(tb => tb.HasCheckConstraint("CK_PhieuSTT_TrangThai",
                "[TrangThaiPhieu] IN (N'DangCho', N'DangGoi', N'DangKham', N'QuaLuot', N'HoanTat')"));
            // Filtered Unique Index trên MaPhieuHen (chỉ khi NOT NULL)
            e.HasIndex(x => x.MaPhieuHen)
             .IsUnique()
             .HasFilter("[MaPhieuHen] IS NOT NULL")
             .HasDatabaseName("UX_PhieuSTT_MaPhieuHen");
            e.HasOne(x => x.PhongKham)
             .WithMany(pk => pk.PhieuSoThuTus)
             .HasForeignKey(x => x.MaPhong)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.BenhNhan)
             .WithMany(bn => bn.PhieuSoThuTus)
             .HasForeignKey(x => x.MaBenhNhan)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PhieuDatHen)
             .WithOne(pdh => pdh.PhieuSoThuTu)
             .HasForeignKey<PhieuSoThuTu>(x => x.MaPhieuHen)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
