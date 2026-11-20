using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ClinicDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        // Seed VaiTro
        var roles = new[] { "Admin", "LeTan", "BacSi", "BenhNhan" };
        foreach (var r in roles)
        {
            if (!await db.VaiTros.AnyAsync(v => v.TenVaiTro == r))
                db.VaiTros.Add(new VaiTro { TenVaiTro = r, MoTa = r });
        }
        await db.SaveChangesAsync();

        // Seed Admin account
        if (!await db.TaiKhoans.AnyAsync(t => t.TenTaiKhoan == "admin"))
        {
            var adminRole = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "Admin");
            db.TaiKhoans.Add(new TaiKhoan
            {
                TenTaiKhoan = "admin",
                MatKhau     = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                SoDienThoai = "0000000000",
                TrangThai   = "HoatDong",
                MaVaiTro    = adminRole.MaVaiTro,
            });
            await db.SaveChangesAsync();
        }
    }
}
