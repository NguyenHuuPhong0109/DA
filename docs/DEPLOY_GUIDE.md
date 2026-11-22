# HƯỚNG DẪN TRIỂN KHAI VÀ DEMO TRÊN ĐIỆN THOẠI (GIAI ĐOẠN 7)

Tài liệu này cung cấp 02 giải pháp triển khai hệ thống **Phòng Khám Đa Khoa (DACN_HTQL)** với tên miền HTTPS hoàn toàn miễn phí, hỗ trợ kiểm thử toàn diện trên điện thoại di động (iOS Safari / Android Chrome) và phục vụ hội đồng chấm đề tài.

---

## BẢNG TÀI KHOẢN KIỂM THỬ NHANH TRÊN ĐIỆN THOẠI

Hệ thống đã tích hợp sẵn khối **"Tài khoản kiểm thử nhanh"** ngay tại màn hình đăng nhập (`/TaiKhoan/DangNhap`). Khi mở trên điện thoại, bạn chỉ cần bấm 1 chạm vào vai trò tương ứng để tự động điền thông tin:

| Vai trò | Tên đăng nhập | Mật khẩu chung | Mục đích kiểm thử chính trên điện thoại |
| :--- | :---: | :---: | :--- |
| **Quản trị viên** | `admin` | `123456` | Quản lý ca trực, mở/khóa khung giờ, xem BI Dashboard & biểu đồ KPI |
| **Lễ tân** | `letan01` | `123456` | Quét QR check-in, duyệt hẹn cảnh báo đỏ, cấp số vãng lai W |
| **Bác sĩ** | `bacsi01` | `123456` | Điều khiển buồng khám, gọi số phát loa Bluetooth TTS tiếng Việt chuẩn |
| **Bệnh nhân** | `benhnhan01` | `123456` | Đặt lịch khám 4 bước, nhận mã QR, tra cứu tiến độ buồng khám trực tiếp |

---

## CÁCH 1: DEMO TRÊN ĐIỆN THOẠI & BÁO CÁO HỘI ĐỒNG TỨC THÌ (CLOUDFLARE TUNNEL)

> **Ưu điểm vượt trội:**
> - Cấp ngay tên miền **HTTPS có chứng chỉ hợp lệ** trong 5 giây mà không cần cấu hình NAT Router hay mở port modem.
> - Điện thoại dùng mạng **4G/5G hoặc bất kỳ mạng Wi-Fi nào** đều truy cập được ngay lập tức.
> - Phát huy trọn vẹn các tính năng Web API nâng cao yêu cầu HTTPS bảo mật: **Camera quét mã QR**, **Web Speech API**, **Web Audio API** và **Phát loa Bluetooth**.

### Các bước thực hiện:

#### Bước 1: Khởi động công cụ tự động
Tại thư mục gốc dự án (`Phong`), nhấp đúp chuột vào file:
```text
scripts/start-demo-tunnel.bat
```
*(Hoặc mở PowerShell tại thư mục gốc và gõ `.\scripts\start-demo-tunnel.bat`).*

#### Bước 2: Nhận liên kết HTTPS
Hệ thống sẽ tự động khởi động ứng dụng web trên cổng `5000` và bật Cloudflare Quick Tunnel. Trên màn hình console, quan sát dòng chữ màu xanh có dạng:
```text
https://xxxxxxxx-xxxx-xxxx.trycloudflare.com
```

#### Bước 3: Mở kiểm thử trên điện thoại
1. Dán liên kết `https://*.trycloudflare.com` vào trình duyệt điện thoại (**Safari trên iPhone** hoặc **Chrome trên Android**).
2. Vào trang đăng nhập $\to$ Chạm vào nút **"👨‍⚕️ Bác sĩ (bacsi01)"** $\to$ Bấm **"Đăng nhập"**.
3. Tại màn hình Bác sĩ:
   - Bấm nút **"Kiểm tra Loa"**: Trình duyệt sẽ phát tiếng chuông Ding-Dong và đọc to rõ ràng bằng giọng tiếng Việt chuẩn tự nhiên: *"Xin mời bệnh nhân số A không không một, Nguyễn Văn An, vào Phòng 101"*.
   - Bấm **"Gọi số tiếp theo"** hoặc **"Gọi lại"**: Hệ thống tự động mở khóa âm thanh và phát loa mượt mà.
4. Mở menu ngăn kéo trượt (Hamburger) ở góc trái màn hình $\to$ Bấm **"Đăng xuất"** $\to$ Chuyển sang vai trò **Lễ tân (`letan01`)** hoặc **Bệnh nhân (`benhnhan01`)** để trải nghiệm giao diện responsive 390px sắc nét.

---

## CÁCH 2: TRIỂN KHAI ONLINE 24/7 TRÊN MONSTERASP.NET (MIỄN PHÍ)

> **Phù hợp:** Treo demo trực tuyến dài ngày để giảng viên phản biện và hội đồng chấm bài truy cập bất kỳ lúc nào mà không cần giữ máy tính cá nhân hoạt động.

### Bước 1: Xuất bản gói ứng dụng (Publish Release)
Mở terminal tại thư mục gốc dự án và chạy lệnh:
```powershell
dotnet publish DACN_HTQL/DACN_HTQL.csproj -c Release -o ./publish
```
Thư mục `./publish` sẽ được đóng gói hoàn chỉnh sẵn sàng tải lên hosting.

### Bước 2: Đăng ký dịch vụ miễn phí tại MonsterASP.NET
1. Truy cập [https://www.monsterasp.net](https://www.monsterasp.net) và đăng ký tài khoản miễn phí.
2. Tạo một **Web Site** mới (hỗ trợ .NET 8 / 9 / 10 / 11). Hệ thống sẽ cấp tên miền miễn phí dạng:
   ```text
   http(s)://<ten-ban-chon>.runasp.net
   ```
3. Tạo một **MS SQL Server Database** miễn phí (dung lượng 1GB). Ghi lại thông tin kết nối:
   - **Server:** `sql.monsterasp.net` (hoặc IP hosting cung cấp)
   - **Database Name:** `db_xxxx`
   - **User ID:** `user_xxxx`
   - **Password:** `Mật khẩu bạn đặt`

### Bước 3: Khởi tạo cấu trúc Database & Dữ liệu mẫu
1. Trên bảng điều khiển của MonsterASP.NET, mở công cụ **SQL Query Console** (hoặc dùng SQL Server Management Studio kết nối vào DB hosting).
2. Mở file [DACN_HTQL/Database/Deploy_Schema_And_Seed.sql](file:///c:/Users/Asus/Desktop/Phong/DACN_HTQL/Database/Deploy_Schema_And_Seed.sql), copy toàn bộ nội dung và dán vào cửa sổ Query.
3. Bấm **Execute**. Toàn bộ 10 bảng (có đầy đủ constraints, index chống race condition, computed column `CoCanhBao`) và 9 tài khoản mẫu sẽ được nạp hoàn chỉnh.

### Bước 4: Cấu hình Connection String cho môi trường Production
Trong thư mục `./publish`, copy file `appsettings.Production.template.json` thành `appsettings.Production.json` và điền chuỗi kết nối thực tế:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql.monsterasp.net;Database=db_xxxx;User Id=user_xxxx;Password=YourPassword123;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "DisableHttpsRedirection": true,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Bước 5: Tải mã nguồn lên Hosting
- Dùng công cụ **File Manager** trên web MonsterASP.NET hoặc phần mềm **FileZilla (FTP)**:
  - Kết nối đến FTP hosting.
  - Tải toàn bộ các file và thư mục bên trong `./publish` vào thư mục gốc `site/wwwroot/`.
- Truy cập tên miền `https://<ten-ban-chon>.runasp.net` trên điện thoại để hoàn tất!

---

## CÁC ĐIỂM CẢI TIẾN ĐẶC BIỆT DÀNH CHO DI ĐỘNG ĐÃ TÍCH HỢP

1. **Kiến trúc Hybrid TTS 2 tầng:**
   - **Tầng 1 (Ưu tiên):** API Proxy Backend `/api/tts/speak?text=...` gọi qua dịch vụ Google TTS với định dạng MP3 trực tiếp. Nhờ đó, bất kể điện thoại Android, iPhone hay máy tính Windows thiếu gói ngôn ngữ `vi-VN` đều phát âm tiếng Việt tự nhiên 100%.
   - **Tầng 2 (Dự phòng):** Tự động chuyển đổi sang Web Speech API nếu thiết bị mất kết nối mạng.
   - **Chống chặn Autoplay di động:** Cơ chế `unlockAudio()` kích hoạt đồng bộ theo cử chỉ chạm (user gesture click) trước mọi lệnh gọi bất đồng bộ.
   - **Chuẩn hóa phát âm số thứ tự:** Tự động phiên âm các mã số phiếu như `A-001` thành *"A không không một"*, `W-012` thành *"W không một hai"*, khắc phục triệt để lỗi đọc từ ngữ rời rạc.

2. **Giao diện Responsive 390px chuyên dụng:**
   - Cổng nội bộ tích hợp **Drawer Menu trượt (Offcanvas)** có lớp phủ mờ (backdrop) hiển thị đầy đủ tên, vai trò và nút Đăng xuất lớn.
   - Toàn bộ bảng dữ liệu được bọc bởi `.table-wrap` / `.table-responsive` với thuộc tính `-webkit-overflow-scrolling: touch` giúp cuộn ngang mượt mà, không gây vỡ tỷ lệ trang.
   - Vùng chạm các nút hành động lâm sàng đạt chuẩn tối thiểu `40px` $\times$ `40px`, chống bấm nhầm trên màn hình cảm ứng nhỏ.
