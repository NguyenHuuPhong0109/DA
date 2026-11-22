# ============================================================================
# SCRIPT 01: THIẾT LẬP MÔI TRƯỜNG & KHỞI TẠO HỆ THỐNG TRONG MÁY ẢO
# Tự động hóa: Kiểm tra Tools -> Cấu hình SQL -> Khởi tạo CSDL 4 ngày -> Sẵn sàng chạy
# ============================================================================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$Host.UI.RawUI.WindowTitle = "Setup ClinicBookingMVC trong máy ảo"

function Write-Step($step, $msg) {
    Write-Host "`n[$step] $msg" -ForegroundColor Cyan
}

function Write-Success($msg) {
    Write-Host "  [✓] $msg" -ForegroundColor Green
}

function Write-Warn($msg) {
    Write-Host "  [!] $msg" -ForegroundColor Yellow
}

function Write-Err($msg) {
    Write-Host "  [✗] $msg" -ForegroundColor Red
}

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "       HỆ THỐNG PHÒNG KHÁM ĐA KHOA (CLINICBOOKING MVC - HTTT-13)            " -ForegroundColor Cyan
Write-Host "            QUY TRÌNH THIẾT LẬP TỰ ĐỘNG TRONG MÁY ẢO                        " -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptRoot

# ── BƯỚC 1: KIỂM TRA .NET SDK, GIT VÀ CLOUDFLARED ───────────────────────────
Write-Step "1/5" "Kiểm tra các công cụ môi trường cần thiết..."

# 1.1 Kiểm tra .NET
$dotnetOk = $false
try {
    $dotnetVer = & dotnet --version 2>$null
    if ($dotnetVer) {
        Write-Success ".NET SDK đã cài đặt: phiên bản $dotnetVer"
        $dotnetOk = $true
    }
} catch {}

if (-not $dotnetOk) {
    Write-Warn "Chưa phát hiện .NET SDK. Đang thử cài đặt qua winget..."
    try {
        winget install Microsoft.DotNet.SDK.8 --accept-source-agreements --accept-package-agreements --silent
        Write-Success "Đã cài đặt .NET SDK qua winget. Vui lòng mở lại PowerShell nếu lệnh dotnet chưa nhận."
    } catch {
        Write-Err "Không thể tự cài .NET SDK. Vui lòng tải và cài đặt thủ công tại: https://dotnet.microsoft.com/download"
    }
}

# 1.2 Kiểm tra Git
try {
    $gitVer = & git --version 2>$null
    if ($gitVer) {
        Write-Success "Git đã cài đặt: $gitVer"
    } else {
        Write-Warn "Chưa có Git. Đang cài Git qua winget..."
        winget install Git.Git --accept-source-agreements --accept-package-agreements --silent
    }
} catch {
    Write-Warn "Kiểm tra Git bị bỏ qua."
}

# 1.3 Kiểm tra Cloudflare CLI
try {
    $cfVer = & cloudflared --version 2>$null
    if ($cfVer) {
        Write-Success "Cloudflared đã cài đặt: $cfVer"
    } else {
        Write-Warn "Đang cài đặt Cloudflared để cấp tên miền HTTPS..."
        winget install Cloudflare.cloudflared --accept-source-agreements --accept-package-agreements --silent
    }
} catch {
    Write-Warn "Chưa thể cài Cloudflared tự động (có thể cài sau bằng: winget install Cloudflare.cloudflared)."
}

# 1.4 Cài đặt dotnet-ef global tool
Write-Step "2/5" "Cài đặt / Cập nhật Entity Framework Core CLI (dotnet-ef)..."
try {
    & dotnet tool install --global dotnet-ef 2>$null
    if ($LASTEXITCODE -ne 0) {
        & dotnet tool update --global dotnet-ef 2>$null
    }
    Write-Success "dotnet-ef global tool đã sẵn sàng."
} catch {
    Write-Warn "Không thể cập nhật dotnet-ef. Tiếp tục bằng bộ build nội bộ của dự án."
}

# ── BƯỚC 2: PHÁT HIỆN VÀ CẤU HÌNH SQL SERVER TRONG MÁY ẢO ───────────────────
Write-Step "3/5" "Kiểm tra và cấu hình kết nối SQL Server trên máy ảo..."

$instances = @("localhost", ".\SQLEXPRESS", "(localdb)\MSSQLLocalDB", "127.0.0.1")
$selectedInstance = ""

foreach ($inst in $instances) {
    Write-Host "  Đang kiểm tra SQL Instance: $inst ..." -NoNewline
    $connStr = "Server=$inst;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3;"
    try {
        $con = New-Object Microsoft.Data.SqlClient.SqlConnection($connStr) -ErrorAction SilentlyContinue
        if (-not $con) {
            $con = New-Object System.Data.SqlClient.SqlConnection($connStr) -ErrorAction SilentlyContinue
        }
        $con.Open()
        $con.Close()
        Write-Host " [Thành công!]" -ForegroundColor Green
        $selectedInstance = $inst
        break
    } catch {
        Write-Host " [Không kết nối được]" -ForegroundColor Gray
    }
}

if (-not $selectedInstance) {
    Write-Warn "Không tự động kết nối được SQL instance mặc định."
    $inputInst = Read-Host "  Nhập tên SQL Server Instance của máy ảo (mặc định: .\SQLEXPRESS)"
    if ([string]::IsNullOrWhiteSpace($inputInst)) {
        $selectedInstance = ".\SQLEXPRESS"
    } else {
        $selectedInstance = $inputInst.Trim()
    }
}

Write-Success "Sử dụng SQL Instance: $selectedInstance"

# Cập nhật connection string trong appsettings.json
$appsettingsPath = Join-Path $scriptRoot "ClinicBookingMVC\appsettings.json"
if (Test-Path $appsettingsPath) {
    $newConn = "Server=$selectedInstance;Database=ClinicBookingDB;Trusted_Connection=True;TrustServerCertificate=True;"
    $json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $json.ConnectionStrings.DefaultConnection = $newConn
    $json | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath -Encoding UTF8
    Write-Success "Đã cập nhật chuỗi kết nối trong ClinicBookingMVC\appsettings.json!"
}

# ── BƯỚC 3: PHỤC HỒI / TẠO MỚI DỮ LIỆU KIỂM THỬ DÀY ĐẶC ─────────────────────
Write-Step "4/5" "Khởi tạo Cơ sở dữ liệu và nạp dữ liệu kiểm thử dày đặc..."

$bakFile = Join-Path $scriptRoot "ClinicBookingDB.bak"
$restored = $false

if (Test-Path $bakFile) {
    $choice = Read-Host "  Tìm thấy file sao lưu ClinicBookingDB.bak. Bạn có muốn RESTORE trực tiếp? (Y/N, mặc định: N để tạo mới --reseed)"
    if ($choice -eq 'Y' -or $choice -eq 'y') {
        Write-Host "  Đang restore database từ file .bak..." -ForegroundColor Yellow
        $sqlRestore = @"
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ClinicBookingDB')
BEGIN
    ALTER DATABASE [ClinicBookingDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [ClinicBookingDB];
END;
RESTORE DATABASE [ClinicBookingDB] FROM DISK = N'$bakFile' WITH REPLACE;
"@
        try {
            & sqlcmd -S $selectedInstance -d master -Q $sqlRestore 2>$null
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Đã khôi phục thành công ClinicBookingDB từ file backup .bak!"
                $restored = $true
            }
        } catch {}
    }
}

if (-not $restored) {
    Write-Host "  Đang chạy dotnet restore..." -ForegroundColor Gray
    & dotnet restore ClinicBookingMVC\ClinicBookingMVC.csproj

    Write-Host "  Đang áp dụng migrations EF Core..." -ForegroundColor Gray
    & dotnet ef database update --project ClinicBookingMVC 2>$null

    Write-Host "  Đang nạp dữ liệu mẫu dày đặc 4 ngày bằng cơ chế --reseed..." -ForegroundColor Cyan
    & dotnet run --project ClinicBookingMVC -- --reseed
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Đã nạp toàn bộ 10 bảng và dữ liệu 4 ngày (Today .. Today+3) thành công!"
    } else {
        Write-Warn "Lệnh --reseed gặp cảnh báo, ứng dụng sẽ tự seed dữ liệu khi khởi chạy lần đầu."
    }
}

# ── BƯỚC 4: HOÀN TẤT & HƯỚNG DẪN TRUY CẬP ───────────────────────────────────
Write-Step "5/5" "HOÀN TẤT THIẾT LẬP TRÊN MÁY ẢO!"
Write-Host "============================================================================" -ForegroundColor Green
Write-Host "             HỆ THỐNG ĐÃ SẴN SÀNG HOẠT ĐỘNG TRÊN MÁY ẢO!                    " -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host @"

👉 CÁCH KHỞI CHẠY HỆ THỐNG:
   1. Chạy máy chủ cục bộ:
      dotnet run --project ClinicBookingMVC --urls http://localhost:5000

   2. Mở trình duyệt truy cập:
      http://localhost:5000

   3. Mở tên miền HTTPS miễn phí để test trên điện thoại thật:
      .\start-demo-tunnel.bat

👉 DANH SÁCH TÀI KHOẢN KIỂM THỬ (Mật khẩu chung: 123456):
   - Quản trị viên: admin       / 123456
   - Lễ tân:        letan01     / 123456
   - Bác sĩ:        bacsi01     / 123456 (Gọi loa Bluetooth tiếng Việt chuẩn)
   - Bác sĩ 02..04: bacsi02, bacsi03, bacsi04
   - Bệnh nhân:     benhnhan01  / 123456 (Đã có lịch hẹn test hủy & tra cứu)

👉 TÀI LIỆU CHI TIẾT ĐI KÈM:
   - TEST_DATA_CHEATSHEET.md: Tra cứu mã đặt hẹn & số điện thoại mẫu
   - HUONG_DAN_SU_DUNG_HE_THONG.md: Cẩm nang sử dụng 4 vai trò
   - DEPLOY_GUIDE.md: Hướng dẫn Cloudflare Tunnel & MonsterASP.NET
"@ -ForegroundColor White
Write-Host "============================================================================" -ForegroundColor Green
pause
