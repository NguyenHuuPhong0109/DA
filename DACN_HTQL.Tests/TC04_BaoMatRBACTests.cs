using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DACN_HTQL.Data;
using Xunit;

namespace DACN_HTQL.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestAuthScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Role", out var roleHeader) || string.IsNullOrWhiteSpace(roleHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = roleHeader.ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim(ClaimTypes.Name, $"user_{role.ToLower()}"),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Thay thế DbContext sang DACN_HTQL
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ClinicDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<ClinicDbContext>(options =>
                options.UseSqlServer(TestDbFixture.ConnectionString));

            // Cấu hình Authentication handler kiểm thử
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultSignInScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}

public class TC04_BaoMatRBACTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TC04_BaoMatRBACTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        TestDbFixture.EnsureDatabaseInitializedAsync().Wait();
    }

    private HttpClient CreateClientWithRole(string? role, bool followRedirects = false)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = followRedirects
        });

        if (!string.IsNullOrEmpty(role))
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        return client;
    }

    // ─────────────────────────────────────────────────────────────
    // 1. Chưa đăng nhập gửi POST tạo lịch hẹn (/DatLich/XacNhan) -> Chặn chuyển về trang đăng nhập hoặc 401
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task NguoiChuaDangNhap_GuiPostDatLich_BiChan()
    {
        var client = CreateClientWithRole(null, followRedirects: false);

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["MaKhungGio"] = "1",
            ["LyDoKham"] = "Khám chưa đăng nhập",
            ["TenBenhNhan"] = "Ẩn Danh",
            ["SoDienThoai"] = "0900000000"
        });

        var response = await client.PostAsync("/DatLich/XacNhan", content);

        // Bị chặn: 401 Unauthorized hoặc 302 Redirect về Login
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 401 or Redirect (302), but got {response.StatusCode}");

        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found)
        {
            var location = response.Headers.Location?.ToString() ?? "";
            Assert.Contains("DangNhap", location, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 2. Tài khoản BenhNhan truy cập /Admin -> Bị chặn (403 hoặc chuyển hướng KhongCoQuyen)
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task TaiKhoanBenhNhan_TruyCapAdmin_BiChan403HoacChuyenHuong()
    {
        var client = CreateClientWithRole("BenhNhan", followRedirects: false);

        var response = await client.GetAsync("/Admin");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 403 Forbidden or Redirect, but got {response.StatusCode}");

        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found)
        {
            var location = response.Headers.Location?.ToString() ?? "";
            Assert.Contains("KhongCoQuyen", location, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 3. Tài khoản BenhNhan truy cập /BacSi -> Bị chặn (403 hoặc chuyển hướng KhongCoQuyen)
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task TaiKhoanBenhNhan_TruyCapBacSi_BiChan403HoacChuyenHuong()
    {
        var client = CreateClientWithRole("BenhNhan", followRedirects: false);

        var response = await client.GetAsync("/BacSi");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 403 Forbidden or Redirect, but got {response.StatusCode}");

        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found)
        {
            var location = response.Headers.Location?.ToString() ?? "";
            Assert.Contains("KhongCoQuyen", location, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 4. Tài khoản LeTan truy cập /Admin -> Bị chặn (403 hoặc chuyển hướng KhongCoQuyen)
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task TaiKhoanLeTan_TruyCapAdmin_BiChan403HoacChuyenHuong()
    {
        var client = CreateClientWithRole("LeTan", followRedirects: false);

        var response = await client.GetAsync("/Admin");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 403 Forbidden or Redirect, but got {response.StatusCode}");

        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found)
        {
            var location = response.Headers.Location?.ToString() ?? "";
            Assert.Contains("KhongCoQuyen", location, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 5. Tài khoản LeTan truy cập /BacSi -> Bị chặn (403 hoặc chuyển hướng KhongCoQuyen)
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task TaiKhoanLeTan_TruyCapBacSi_BiChan403HoacChuyenHuong()
    {
        var client = CreateClientWithRole("LeTan", followRedirects: false);

        var response = await client.GetAsync("/BacSi");

        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected 403 Forbidden or Redirect, but got {response.StatusCode}");

        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found)
        {
            var location = response.Headers.Location?.ToString() ?? "";
            Assert.Contains("KhongCoQuyen", location, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // 6. Tài khoản Admin truy cập /Admin -> Thành công (200 OK)
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task TaiKhoanAdmin_TruyCapAdmin_ThanhCong200()
    {
        var client = CreateClientWithRole("Admin", followRedirects: true);

        var response = await client.GetAsync("/Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
