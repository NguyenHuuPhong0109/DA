# DACN_HTQL - Hệ Thống Quản Lý Đặt Lịch & Hàng Đợi Khám Bệnh

> **Đồ án Chuyên ngành (HTTT-13)**  
> Ứng dụng web ASP.NET Core MVC quản lý đặt hẹn trực tuyến, cấp số thứ tự, điều phối hàng đợi khám bệnh thông minh và báo cáo BI thời gian thực.

---

## 🛠️ Công Nghệ Sử Dụng

- **Backend:** C# / .NET 8.0, ASP.NET Core MVC.
- **Cơ sở dữ liệu:** Microsoft SQL Server (LocalDB / Express / Enterprise), Entity Framework Core 8 (Code-First & Migrations).
- **Giao diện (Frontend):** Razor Views (.cshtml), Bootstrap 5, phong cách tối giản Tabler Light Mode & shadcn-ui, Chart.js.
- **Âm thanh & Điều hướng buồng khám:** Hybrid TTS tiếng Việt chuẩn tự nhiên (Web Speech API + Server-side Fallback API qua Google Translate TTS), kết nối Loa Bluetooth / Màn hình gọi số.
- **Bảo mật & Phân quyền:** Cookie Authentication, Role-based Access Control (RBAC) với 4 vai trò độc lập, Anti-forgery Token (CSRF).
- **Kiểm thử tự động:** xUnit, Microsoft.AspNetCore.Mvc.Testing, EF Core In-Memory / SQL Server ADO.NET (20/20 test cases).

---

## 📁 Cấu Trúc Dự Án

```text
DACN_HTQL/
├── DACN_HTQL.slnx                 # Solution cấu hình toàn bộ dự án
├── README.md                      # Giới thiệu & Hướng dẫn sử dụng tổng quan
├── .gitignore                     # Cấu hình bỏ qua file build & file nhạy cảm
│
├── DACN_HTQL/                     # Web Application (ASP.NET Core 8 MVC)
│   ├── Controllers/               # Bộ điều khiển (Admin, BacSi, LeTan, DatLich, TaiKhoan, Tts)
│   ├── Data/                      # DbContext, DbInitializer (Seed data)
│   ├── Database/                  # Script SQL tạo schema & đối chiếu KPI (DACN_HTQL.sql)
│   ├── Migrations/                # EF Core Migrations
│   ├── Models/                    # Thực thể (Entities) & ViewModels
│   ├── Services/                  # Nghiệp vụ (Đặt lịch, Hàng đợi, Vắng hẹn, Báo cáo BI)
│   ├── Views/                     # Giao diện Razor (Bootstrap 5, Tabler-inspired)
│   └── wwwroot/                   # CSS (clinic.css, clinic-phase5.css), JS, Vendor assets
│
├── DACN_HTQL.Tests/               # Unit & Integration Tests (xUnit, 20 test cases)
│   ├── TC01_HappyPathTests.cs     # Luồng đặt lịch & khám thành công
│   ├── TC02_ConcurrencyTests.cs   # Kiểm thử tranh chấp suất khám đồng thời
│   ├── TC03_NgoaiLeVaHangDoiTests.cs # Kiểm thử ngoại lệ, vắng hẹn, hủy lịch
│   ├── TC04_BaoMatRBACTests.cs    # Kiểm thử phân quyền RBAC & xác thực
│   ├── TC05_DoiChieuKpiSqlTests.cs# Đối chiếu 4 chỉ số KPI giữa C# và SQL thuần
│   └── TestDbFixture.cs           # Fixture kết nối CSDL kiểm thử
│
├── docs/                          # Thư mục tài liệu chi tiết
│   ├── DEPLOY_GUIDE.md            # Hướng dẫn triển khai (Local, IIS, MonsterASP.NET, Cloudflare)
│   ├── DESIGN_SYSTEM.md           # Quy chuẩn thiết kế giao diện (Tabler, shadcn-ui)
│   ├── HANDOFF_STATUS.md          # Biên bản bàn giao kỹ thuật & lịch sử 10 giai đoạn
│   ├── HUONG_DAN_SU_DUNG_HE_THONG.md # Cẩm nang hướng dẫn sử dụng 4 vai trò
│   ├── KET_QUA_KIEM_THU_CHUC_NANG.md # Báo cáo kết quả kiểm thử chức năng
│   ├── TEST_DATA_CHEATSHEET.md    # Bảng tra cứu dữ liệu kiểm thử dày đặc
│   └── TEST_REPORT.md             # Báo cáo tổng hợp kiểm thử tự động
│
└── scripts/                       # Kịch bản tự động hóa & kiểm thử tích hợp
    ├── start-demo-tunnel.bat      # Khởi chạy demo tunnel Cloudflare HTTPS cho mobile
    └── test_integration.ps1       # Script PowerShell kiểm thử tích hợp 11/11 endpoint
```

---

## 👥 Danh Sách Tài Khoản Kiểm Thử Mẫu (Mật khẩu chung: `123456`)

| Tên tài khoản | Vai trò | Chức danh / Mô tả | SĐT liên hệ |
| :--- | :---: | :--- | :---: |
| `admin` | **Admin** | Quản trị viên hệ thống | `0900000001` |
| `letan01` | **LeTan** | Nhân viên tiếp đón & điều phối quầy | `0900000002` |
| `bacsi01` | **BacSi** | BS.CKI Nguyễn Văn An (Nội tổng quát - P101) | `0901234561` |
| `bacsi02` | **BacSi** | ThS.BS Trần Thị Bình (Tai Mũi Họng - P102) | `0901234562` |
| `bacsi03` | **BacSi** | BS.CKI Lê Hoàng Cường (Da liễu - P103) | `0901234563` |
| `bacsi04` | **BacSi** | ThS.BS Phạm Minh Đức (Nhi khoa - P104) | `0901234564` |
| `bacsi05` | **BacSi** | BS.CKII Võ Quốc Hưng (Tim mạch - P105) | `0901234565` |
| `bacsi06` | **BacSi** | ThS.BS Đặng Thùy Linh (Cơ xương khớp - P106) | `0901234566` |
| `bacsi07` | **BacSi** | BS.CKI Hoàng Tuấn Kiệt (Tiêu hóa - Gan mật - P107) | `0901234567` |
| `bacsi08` | **BacSi** | BS.CKII Nguyễn Mai Phương (Sản phụ khoa - P108) | `0901234568` |
| `bacsi09` | **BacSi** | ThS.BS Bùi Quang Huy (Mắt - P109) | `0901234569` |
| `bacsi10` | **BacSi** | BS.CKI Đỗ Thanh Thảo (Răng Hàm Mặt - P110) | `0901234570` |
| `benhnhan01` | **BenhNhan** | Bệnh nhân mẫu 01 (Lịch hẹn hoạt động) | `0912345671` |
| `benhnhan02` | **BenhNhan** | Bệnh nhân mẫu 02 | `0912345672` |
| `benhnhan03` | **BenhNhan** | Bệnh nhân mẫu 03 | `0912345673` |

---

## 🏥 Quy Mô Dữ Liệu & Điểm Nổi Bật

- **Quy mô danh mục chuẩn:** 10 chuyên khoa lâm sàng, 10 phòng khám khang trang (Phòng 101 – 110), 10 bác sĩ chuyên khoa với đầy đủ ca trực sáng/chiều/tối.
- **Bộ nhận diện hình ảnh y khoa SVG Duotone xanh lá:** Toàn bộ 10 chuyên khoa được tích hợp hình minh họa SVG duotone chuẩn `healthicons` tối ưu màu sắc xanh lá (#16A34A / #DCFCE7), phong cách tối giản phẳng và không gây rối mắt.
- **Kiến trúc Hàng đợi 3 luồng thông minh (Priority Queue):**
  - `A-xxx` (Lượt đúng giờ): Dành cho bệnh nhân đặt trước check-in đúng giờ hoặc trễ $\le 15$ phút (Mức ưu tiên 1).
  - `L-xxx` (Lượt đến trễ): Dành cho bệnh nhân đặt trước đến trễ 16–30 phút, bảo lưu lượt nhưng xếp sau lượt đúng giờ và vãng lai (Mức ưu tiên 3).
  - `W-xxx` (Lượt vãng lai): Dành cho bệnh nhân đăng ký trực tiếp tại quầy chưa đặt trước (Mức ưu tiên 2).
  - Khách đến sớm $> 30$ phút được bảo vệ không nhảy cóc trước khách vãng lai đã chờ sẵn.
- **Quy tắc cảnh báo vắng hẹn (BR-06):**
  - Quá 30 phút sau giờ hẹn chưa check-in: Hệ thống tự động chuyển trạng thái `VangHen` và tăng bộ đếm vắng hẹn.
  - Bệnh nhân vắng hẹn $\ge 2$ lần bị gắn cờ đỏ `CoCanhBao`. Khi đặt lịch mới, phiếu sẽ rơi vào trạng thái `ChoXacNhan` và yêu cầu lễ tân hoặc bác sĩ duyệt trước khi có hiệu lực.
- **Giao diện chuẩn Tabler Light Mode:** Thiết kế tinh giản, phong cách phẳng không viền bo màu mè, 100% nút bấm không chứa icon hay emoji rối mắt, căn lề bảng thẳng thớm và footer dính đáy màn hình (Sticky Footer).

---

## 🚀 Hướng Dẫn Vận Hành

### 1. Khởi tạo Cơ sở dữ liệu lần đầu
```bash
dotnet ef database update --project DACN_HTQL
```

### 2. Làm mới toàn bộ dữ liệu mẫu (Reseed)
Lệnh này sẽ làm sạch DB, tái tạo dữ liệu mẫu dày đặc 4 ngày hoạt động và 14 ngày lịch sử phục vụ phân tích BI:
```bash
dotnet run --project DACN_HTQL -- --reseed
```

### 3. Khởi chạy ứng dụng Web
```bash
dotnet run --project DACN_HTQL
```
> Mặc định ứng dụng chạy tại: `http://localhost:5000` (hoặc cổng cấu hình trong `launchSettings.json`).

### 4. Chạy toàn bộ 20 bộ kiểm thử tự động (xUnit)
```bash
dotnet test DACN_HTQL.slnx
```

### 5. Khởi chạy Demo qua Cloudflare Tunnel (Kiểm thử trên điện thoại)
Nhấp đúp chuột vào file:
```text
scripts/start-demo-tunnel.bat
```
*(Hoặc chạy lệnh từ terminal: `.\scripts\start-demo-tunnel.bat`)*  
Hệ thống sẽ tự động khởi động server và cấp một đường link HTTPS công khai dạng `https://*.trycloudflare.com` để mở trên điện thoại (4G / Wi-Fi).

### 6. Kiểm thử tích hợp tự động qua PowerShell
```powershell
.\scripts\test_integration.ps1
```
*(Kiểm tra tự động 12/12 luồng: Trang chủ, Đăng nhập 4 vai trò, RBAC, Đặt lịch, Tiếp nhận, 11/11 ảnh SVG duotone chuyên khoa, Gọi số & API TTS, Dashboard & Xuất CSV).*

---

## 📚 Tài Liệu Tham Khảo Chi Tiết

- 📖 [Hướng dẫn triển khai & Demo điện thoại](docs/DEPLOY_GUIDE.md)
- 🎨 [Quy chuẩn thiết kế giao diện Design System](docs/DESIGN_SYSTEM.md)
- 📋 [Biên bản bàn giao kỹ thuật Handoff Status](docs/HANDOFF_STATUS.md)
- 🧑‍⚕️ [Cẩm nang hướng dẫn sử dụng hệ thống 4 vai trò](docs/HUONG_DAN_SU_DUNG_HE_THONG.md)
- 🧪 [Báo cáo kết quả kiểm thử chức năng](docs/KET_QUA_KIEM_THU_CHUC_NANG.md)
- 📊 [Bảng tra cứu dữ liệu kiểm thử Cheatsheet](docs/TEST_DATA_CHEATSHEET.md)
- 📈 [Báo cáo tổng hợp kiểm thử xUnit & SQL](docs/TEST_REPORT.md)
