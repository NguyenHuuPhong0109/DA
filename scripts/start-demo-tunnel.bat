@echo off
chcp 65001 >nul
title Khởi chạy Demo Tunnel HTTPS - DACN_HTQL

echo ============================================================================
echo         HỆ THỐNG PHÒNG KHÁM ĐA KHOA (DACN_HTQL - HTTT-13)
echo           KHỞI TẠO TÊN MIỀN HTTPS MIỄN PHÍ KIỂM THỬ TRÊN ĐIỆN THOẠI
echo ============================================================================
echo.

:: 1. Kiểm tra Cloudflared CLI
where cloudflared >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [CẢNH BÁO] Chưa tìm thấy tiện ích cloudflared trên máy tính.
    echo.
    echo Vui lòng mở PowerShell hoặc Terminal và chạy lệnh sau để cài đặt:
    echo    winget install Cloudflare.cloudflared
    echo.
    echo Hoặc tải file thực thi tại: https://github.com/cloudflare/cloudflared/releases
    echo Sau khi cài đặt xong, hãy mở lại file .bat này.
    echo ============================================================================
    pause
    exit /b 1
)

echo [✓] Đã phát hiện Cloudflared CLI.
echo [1/2] Đang khởi chạy máy chủ DACN_HTQL trên cổng 5000...
echo.

:: 2. Thiết lập biến môi trường để chạy ổn định sau Tunnel
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://0.0.0.0:5000
set TUNNEL_MODE=1
set DisableHttpsRedirection=true

:: 3. Chạy ứng dụng trong cửa sổ console riêng với đường dẫn tương đối từ scripts/
start "DACN_HTQL Server (Port 5000)" cmd /c "dotnet run --project "%~dp0..\DACN_HTQL\DACN_HTQL.csproj" --urls http://0.0.0.0:5000"

:: 4. Chờ ứng dụng khởi động sẵn sàng
echo Đang đợi máy chủ web sẵn sàng (3 giây)...
timeout /t 3 /nobreak >nul

echo.
echo [2/2] Đang kích hoạt Cloudflare Quick Tunnel...
echo ============================================================================
echo HÃY TÌM DÒNG CHỮ CÓ ĐỊNH DẠNG:
echo    https://*.trycloudflare.com
echo.
echo 👉 Hãy copy liên kết HTTPS đó và mở trực tiếp trên điện thoại (4G / Wi-Fi)
echo    hoặc chia sẻ cho giảng viên hội đồng chấm thi!
echo ============================================================================
echo.

cloudflared tunnel --url http://localhost:5000
pause
