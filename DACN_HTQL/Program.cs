using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using DACN_HTQL.Data;
using DACN_HTQL.Services;

var builder = WebApplication.CreateBuilder(args);
bool isReseed = args.Contains("--reseed", StringComparer.OrdinalIgnoreCase);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Cấu hình Forwarded Headers hỗ trợ Reverse Proxy (Cloudflare Tunnel, IIS / MonsterASP.NET)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CaTrucService>();
builder.Services.AddScoped<DatLichService>();
builder.Services.AddScoped<HangDoiService>();
builder.Services.AddScoped<VangHenService>();
builder.Services.AddScoped<BaoCaoService>();
builder.Services.AddHostedService<VangHenBackgroundService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath           = "/TaiKhoan/DangNhap";
        options.LogoutPath          = "/TaiKhoan/DangXuat";
        options.AccessDeniedPath    = "/TaiKhoan/KhongCoQuyen";
        options.ExpireTimeSpan      = TimeSpan.FromHours(8);
        options.SlidingExpiration   = true;
        options.Cookie.HttpOnly     = true;
        options.Cookie.SameSite     = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Đặt UseForwardedHeaders ở đầu pipeline để nhận diện chính xác client IP và proto HTTPS từ proxy
app.UseForwardedHeaders();

// Khởi tạo Database và Seed data an toàn trong try-catch
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    await DbInitializer.InitializeAsync(db, isReseed);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "[DbInitializer] Lỗi trong quá trình khởi tạo cơ sở dữ liệu: {Message}", ex.Message);
}

if (isReseed)
{
    Console.WriteLine("[DbInitializer] Đã thực hiện --reseed thành công! Toàn bộ dữ liệu mẫu mới đã sẵn sàng.");
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Tránh lỗi vòng lặp chuyển hướng (ERR_TOO_MANY_REDIRECTS) khi chạy sau Reverse Proxy / Cloudflare Tunnel
var disableHttpsRedir = app.Configuration.GetValue<bool>("DisableHttpsRedirection")
    || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TUNNEL_MODE"))
    || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME"));

if (!disableHttpsRedir && app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
