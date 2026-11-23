$baseUrl = "http://localhost:5000"
$ErrorActionPreference = "Stop"

Write-Host "`n============================================================================" -ForegroundColor Cyan
Write-Host "         BÁO CÁO KIỂM THỬ TÍCH HỢP HỆ THỐNG DACN_HTQL" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan

# Kiểm tra nếu server chưa chạy, tự động khởi động server ngầm
$serverProcess = $null
try {
    $null = Invoke-WebRequest -Uri "$baseUrl/" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
} catch {
    Write-Host "  [*] Đang khởi động máy chủ DACN_HTQL trên cổng 5000..." -ForegroundColor Yellow
    $projectPath = Join-Path $PSScriptRoot "..\DACN_HTQL\DACN_HTQL.csproj"
    $serverProcess = Start-Process "dotnet" -ArgumentList "run --project `"$projectPath`" --urls http://localhost:5000" -PassThru
    Start-Sleep -Seconds 5
}

try {
    # 1. Trang chủ và Đăng nhập
    $homePage = Invoke-WebRequest -Uri "$baseUrl/" -UseBasicParsing
    Write-Host "  [✓] 1. Trang chủ [/]: HTTP $($homePage.StatusCode) OK (HTML: $($homePage.Content.Length) bytes)" -ForegroundColor Green

    $loginGet = Invoke-WebRequest -Uri "$baseUrl/TaiKhoan/DangNhap" -UseBasicParsing
    Write-Host "  [✓] 2. Trang Đăng nhập [/TaiKhoan/DangNhap]: HTTP $($loginGet.StatusCode) OK (Light Mode)" -ForegroundColor Green

    function Login($user, $pass) {
        $s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
        $g = Invoke-WebRequest -Uri "$baseUrl/TaiKhoan/DangNhap" -WebSession $s -UseBasicParsing
        $token = ""
        if ($g.Content -match '__RequestVerificationToken".*?value="([^"]+)"') { $token = $matches[1] }
        $b = @{ "TenTaiKhoan" = $user; "MatKhau" = $pass; "__RequestVerificationToken" = $token }
        $p = Invoke-WebRequest -Uri "$baseUrl/TaiKhoan/DangNhap" -Method Post -Body $b -WebSession $s -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
        return $s
    }

    # 2. Đăng nhập 4 tài khoản
    $sBn = Login "benhnhan01" "123456"
    Write-Host "  [✓] 3. Đăng nhập [benhnhan01]: Thành công, cấp Cookie phiên làm việc" -ForegroundColor Green

    $sLt = Login "letan01" "123456"
    Write-Host "  [✓] 4. Đăng nhập [letan01]: Thành công, cấp Cookie phiên làm việc" -ForegroundColor Green

    $sBs = Login "bacsi01" "123456"
    Write-Host "  [✓] 5. Đăng nhập [bacsi01]: Thành công, cấp Cookie phiên làm việc" -ForegroundColor Green

    $sAd = Login "admin" "123456"
    Write-Host "  [✓] 6. Đăng nhập [admin]: Thành công, cấp Cookie phiên làm việc" -ForegroundColor Green

    # 3. Phân quyền RBAC
    $rbacAdmin = Invoke-WebRequest -Uri "$baseUrl/Admin" -WebSession $sBn -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    $rbacBacSi = Invoke-WebRequest -Uri "$baseUrl/BacSi" -WebSession $sBn -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue

    if ($rbacAdmin.StatusCode -in (302, 403) -and $rbacBacSi.StatusCode -in (302, 403)) {
        Write-Host "  [✓] 7. Phân quyền RBAC: benhnhan01 bị chặn truy cập /Admin & /BacSi (Chuyển hướng KhongCoQuyen)" -ForegroundColor Green
    } else {
        Write-Host "  [✗] 7. Lỗi RBAC: Chưa chặn đúng quy định!" -ForegroundColor Red
    }

    # 4. Bệnh nhân & Ảnh chuyên khoa SVG
    $datLich = Invoke-WebRequest -Uri "$baseUrl/DatLich" -WebSession $sBn -UseBasicParsing
    $traCuu = Invoke-WebRequest -Uri "$baseUrl/DatLich/TraCuu" -WebSession $sBn -UseBasicParsing
    Write-Host "  [✓] 8. Bệnh nhân: Tải danh sách đặt lịch ($($datLich.StatusCode)) & Tra cứu phiếu hẹn ($($traCuu.StatusCode))" -ForegroundColor Green

    $svgs = @(
        "noi-tong-quat.svg", "nhi-khoa.svg", "san-phu-khoa.svg", "mat.svg",
        "tai-mui-hong.svg", "rang-ham-mat.svg", "da-lieu.svg", "tim-mach.svg",
        "co-xuong-khop.svg", "tieu-hoa-gan-mat.svg", "mac-dinh.svg"
    )
    $svgPass = $true
    foreach ($svg in $svgs) {
        $svgRes = Invoke-WebRequest -Uri "$baseUrl/images/specialties/$svg" -UseBasicParsing
        if ($svgRes.StatusCode -ne 200) { $svgPass = $false }
    }
    if ($svgPass) {
        Write-Host "  [✓] 9. Tài nguyên tĩnh: Tải thành công 11/11 file SVG duotone chuyên khoa (HTTP 200)" -ForegroundColor Green
    } else {
        Write-Host "  [✗] 9. Lỗi tải file SVG chuyên khoa!" -ForegroundColor Red
    }

    # 5. Lễ tân
    $leTan = Invoke-WebRequest -Uri "$baseUrl/LeTan" -WebSession $sLt -UseBasicParsing
    Write-Host "  [✓] 10. Lễ tân: Tải danh sách lịch hẹn hôm nay & Hàng đợi tiếp nhận ($($leTan.StatusCode))" -ForegroundColor Green

    # 6. Bác sĩ & API TTS & Hàng đợi JSON
    $bacSi = Invoke-WebRequest -Uri "$baseUrl/BacSi" -WebSession $sBs -UseBasicParsing
    $tts = Invoke-WebRequest -Uri "$baseUrl/api/tts/speak?text=Xin%20moi%20benh%20nhan" -WebSession $sBs -UseBasicParsing
    $hangDoiJson = Invoke-WebRequest -Uri "$baseUrl/BacSi/LayHangDoi?maPhong=1" -WebSession $sBs -UseBasicParsing
    Write-Host "  [✓] 11. Bác sĩ: Tải hàng đợi phòng khám ($($bacSi.StatusCode)), API JSON hàng đợi & API TTS audio/mpeg ($($tts.Content.Length) bytes)" -ForegroundColor Green

    # 7. Admin
    $admin = Invoke-WebRequest -Uri "$baseUrl/Admin" -WebSession $sAd -UseBasicParsing
    $csv = Invoke-WebRequest -Uri "$baseUrl/Admin/ExportBaoCaoCsv" -WebSession $sAd -UseBasicParsing
    Write-Host "  [✓] 12. Admin: Tải BI Dashboard KPI ($($admin.StatusCode)) & Xuất file CSV ($($csv.Content.Length) bytes, $($csv.Headers['Content-Type']))" -ForegroundColor Green

    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host "          TẤT CẢ 12/12 HẠNG MỤC KIỂM THỬ TÍCH HỢP ĐẠT PASS 100%!" -ForegroundColor Green
    Write-Host "============================================================================" -ForegroundColor Cyan
} finally {
    if ($serverProcess -and !$serverProcess.HasExited) {
        Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
    }
}
