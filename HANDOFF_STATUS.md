# HANDOFF STATUS — ClinicBookingMVC (HTTT-13)

**Cập nhật lần cuối:** 2026-11-20 · **Hoàn thành Giai đoạn 6A (Kiểm thử hệ thống tự động)**

---

## ✅ BUILD & TEST STATUS: PASSED — 0 Errors, 0 Warnings, 19/19 Tests Passed

```
Solution: ClinicBooking.sln
Framework: net11.0 | SDK: 11.0.100
Build succeeded.
    0 Warning(s)
    0 Error(s)

Test Run Successful.
    Total Tests: 19
    Passed: 19 (100%)
    Failed: 0
    Skipped: 0
    Duration: 5.2s
```

---

## Tiến độ theo Giai đoạn

| Giai đoạn | Nội dung | Trạng thái |
|-----------|----------|------------|
| 1 | DESIGN_SYSTEM.md + Cấu trúc dự án ban đầu + 10 Entity 3NF | ✅ Hoàn thành |
| 2 | CSS Tokens/Layout + Cookie Auth + RBAC 4 vai trò + Admin CRUD | ✅ Hoàn thành |
| 3 | Luồng Đặt lịch 4 bước + DatLichService (BR-01..BR-06) + Tra cứu hàng đợi mobile | ✅ Hoàn thành |
| 4 | Thuật toán Hàng đợi 3 mức ưu tiên + Cửa sổ 15p + Quầy Lễ tân + Màn hình Bác sĩ Loa Bluetooth | ✅ Hoàn thành |
| 5 | Tự động xử lý Vắng hẹn (BackgroundService) + BI Dashboard KPI Chart.js + Export CSV (UTF-8 BOM) + Seed Data toàn diện | ✅ Hoàn thành |
| 6A | Kiểm thử hệ thống tự động (xUnit 5 nhóm kịch bản) + Đối chiếu KPI SQL thuần + Báo cáo TEST_REPORT.md | ✅ Hoàn thành |

---

## Chi tiết Giai đoạn 6A đã triển khai

### 1. Project kiểm thử tự động (`ClinicBookingMVC.Tests`)
- **Công nghệ**: xUnit 2.9.3, `Microsoft.AspNetCore.Mvc.Testing` 11.0.0, `Microsoft.EntityFrameworkCore.SqlServer` 11.0.0, `Microsoft.NET.Test.Sdk` 17.14.1.
- **Môi trường Database**: Chạy trực tiếp trên SQL Server (`ClinicBookingTestDB`), kiểm chứng các tính năng cấp hệ quản trị:
  - `IsolationLevel.Serializable` transactions.
  - Concurrency `ROWVERSION` (`byte[] PhienBanDong`).
  - Filtered Unique Index `UX_PhieuDatHen_KhungGio_HieuLuc`.
  - Persisted Computed Column `CoCanhBao = CAST(CASE WHEN SoLanVangHen >= 2 THEN 1 ELSE 0 END AS BIT)`.
  - Check constraints và Foreign Key constraints.

### 2. Bộ 05 nhóm Test Case (19 Tests - 100% Pass)
1. **TC-01 (Happy Path)** (`TC01_HappyPathTests.cs`):
   - Sinh `KhungGio` chuẩn tỷ lệ 3 `DatHen` : 1 `KhungDem`.
   - Luồng hoàn chỉnh: Tạo ca -> Đặt lịch nhận `BKyyMMdd-NNN` -> Check-in đúng giờ nhận `A-xxx` (`MucUuTien = 1`) -> Gọi số (`DangGoi`) -> Bắt đầu khám (`DangKham`) -> Hoàn tất (`PhieuDatHen.TrangThaiHen == "DaKham"`, `PhieuSoThuTu.TrangThaiPhieu == "HoanTat"`).
2. **TC-02 (Concurrency)** (`TC02_ConcurrencyTests.cs`):
   - Chạy song song 2 tác vụ đặt cùng 1 slot trống cuối cùng.
   - Xác nhận đúng 1 tác vụ thành công, tác vụ còn lại bị chặn do `PhienBanDong` / Deadlock / `UX_PhieuDatHen_KhungGio_HieuLuc`, trả về thông báo lỗi thân thiện.
3. **TC-03 (Ngoại lệ & Thuật toán hàng đợi)** (`TC03_NgoaiLeVaHangDoiTests.cs`):
   - Chặn đặt lịch trước giờ khám `< 30 phút` (`KiemTraThoiDiemDatHen`).
   - Chặn hủy lịch khi còn `< 2 giờ` (`KiemTraDieuKienHuy`).
   - Chặn 1 số điện thoại đặt 2 lịch giao nhau trong cùng ngày (`KiemTraTrungLichAsync`).
   - Hủy lịch hợp lệ (`>= 2 giờ`) mở lại `KhungGio.TrangThai = "ConTrong"` (`HuyLichAsync`).
   - Khách đặt sớm (`> 15 phút`) không nhảy cóc giành lượt của khách vãng lai đã check-in.
   - Khách trễ 16–30 phút bị gán `MucUuTien = 3`, prefix `L-xxx`.
   - Khách `A` chưa check-in thì hệ thống tự động gọi ngay khách vãng lai `W-xxx`.
   - Gọi lại tối đa 3 lần, không vượt quá `SoLanGoi = 3`.
   - `VangHenService`: Quá 30 phút chưa check-in chuyển `VangHen`, tăng `SoLanVangHen`, tự bật `CoCanhBao = true` khi đạt $\ge 2$.
4. **TC-04 (Bảo mật RBAC)** (`TC04_BaoMatRBACTests.cs`):
   - `WebApplicationFactory<Program>` kiểm thử phân quyền:
   - Chặn người chưa đăng nhập gửi POST `/DatLich/XacNhan` (chuyển hướng `/TaiKhoan/DangNhap`).
   - Chặn tài khoản `BenhNhan` truy cập `/Admin` hoặc `/BacSi` (chuyển hướng `/TaiKhoan/KhongCoQuyen`).
   - Chặn tài khoản `LeTan` truy cập `/Admin` hoặc `/BacSi` (chuyển hướng `/TaiKhoan/KhongCoQuyen`).
   - Cho phép tài khoản `Admin` truy cập `/Admin` (200 OK).
5. **TC-05 (Đối chiếu KPI với SQL thuần)** (`TC05_DoiChieuKpiSqlTests.cs`):
   - Tạo file `ClinicBookingMVC/Database/Verify_KPI_SQL.sql` chứa 4 câu truy vấn `SELECT` thuần.
   - Đối chiếu trực tiếp giữa C# `BaoCaoService.cs` và SQL thuần:
     * Tỷ lệ vắng hẹn: C# `20.2%` == SQL `20.2%` (24/119)
     * Tỷ lệ hoàn thành: C# `68.7%` == SQL `68.7%` (90/131)
     * Thời gian chờ trung bình: C# `16.8 phút` == SQL `16.8 phút` (1547.0p/92 lượt)
     * Tỷ lệ lấp đầy khung giờ: C# `24.3%` == SQL `24.3%` (131/540)
     * Độ khớp: **100.0%**.

### 3. Hai Bug nghiệp vụ được phát hiện và sửa triệt để
1. **Lỗi Wrap-around thời gian trong `HangDoiService.cs`**:
   - Phép trừ `(TimeOnly.FromDateTime(DateTime.Now) - gioHen).TotalMinutes` bị circular wrap-around sang 1400 phút khi đến sớm.
   - Đã sửa thành `(DateTime.Now - thoiDiemHen).TotalMinutes` với `thoiDiemHen = ca.NgayTruc.ToDateTime(gioHen)`.
2. **Lỗi Deadlock 1205 rơi vào khối Exception chung trong `DatLichService.cs`**:
   - Khi 2 tác vụ Serializable xung đột, SQL Server ném deadlock 1205 khiến ứng dụng hiển thị lỗi hệ thống.
   - Đã bổ sung bộ lọc ngoại lệ nhận diện Deadlock và tên chỉ mục `UX_PhieuDatHen_KhungGio_HieuLuc`, trả về thông báo chuẩn: `"Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác."`.

---

## Cấu trúc thư mục dự án

```
Phong/
├── ClinicBooking.sln                ✅ Solution chứa cả 2 project
├── HANDOFF_STATUS.md                ✅ File tổng hợp trạng thái dự án (duy nhất ở thư mục gốc)
├── TEST_REPORT.md                   ✅ Báo cáo chi tiết kết quả kiểm thử 19 Test Cases
│
├── ClinicBookingMVC/                ✅ Project chính ASP.NET Core MVC (.NET 11)
│   ├── Controllers/
│   │   ├── AdminController.cs       ✅ Full CRUD + BI Dashboard + QuetVangHen
│   │   ├── BacSiController.cs       ✅ Dashboard ca trực, Gọi số, Bluetooth TTS, Khám, Hoàn tất
│   │   ├── DatLichController.cs     ✅ Full luồng 4 bước, xác nhận, phiếu hẹn, tra cứu
│   │   ├── HomeController.cs        ✅ Hero banner + 3 bước hướng dẫn
│   │   ├── LeTanController.cs       ✅ Hàng đợi thời gian thực, Check-in A/W/L, Quét quá hạn
│   │   └── TaiKhoanController.cs    ✅ DangNhap, DangKy, DangXuat, KhongCoQuyen (403)
│   │
│   ├── Data/
│   │   ├── ClinicDbContext.cs       ✅ 10 DbSets + Fluent API (RowVersion, Filtered Unique Index)
│   │   └── DbInitializer.cs         ✅ Seed data 4 vai trò, 3 BS, 3 PK, 120 ca, 161 phiếu
│   │
│   ├── Database/
│   │   ├── ClinicBookingDB.sql       ✅ Script DDL tạo toàn bộ schema và constraints
│   │   └── Verify_KPI_SQL.sql        ✅ Script SQL thuần đối chiếu 4 chỉ số KPI
│   │
│   ├── Migrations/
│   │   └── 20261119104438_InitialCreate.cs ✅ EF Core migration chuẩn
│   │
│   ├── Models/
│   │   ├── Entities/                 ✅ 10 Entity classes
│   │   └── ViewModels/               ✅ ViewModels theo từng luồng
│   │
│   ├── Services/
│   │   ├── CaTrucService.cs          ✅ KiemTraTrungCaAsync, SinhKhungGio, DoiTrangThaiKhungGioAsync
│   │   ├── DatLichService.cs         ✅ DatLichAsync (BR-01,02,03,06), HuyLichAsync, Concurrency handler
│   │   ├── HangDoiService.cs         ✅ Hàng đợi 3 mức (A, W, L), Cửa sổ 15p, Gọi số, Gọi lại (max 3)
│   │   ├── VangHenService.cs         ✅ Quét quá hạn 30p -> VangHen, CoCanhBao = true, Nhắc lịch
│   │   ├── VangHenBackgroundService.cs ✅ IHostedService quét ngầm 5 phút/lần
│   │   └── BaoCaoService.cs          ✅ Tính 4 KPI cốt lõi, thống kê bác sĩ, lưu lượng, xuất CSV UTF-8 BOM
│   │
│   └── Views/                        ✅ Hệ thống view chuẩn Hallmark Design System
│
└── ClinicBookingMVC.Tests/           ✅ Project kiểm thử tự động xUnit (.NET 11)
    ├── TestDbFixture.cs             ✅ Cấu hình DbContext test, khởi tạo DB, tắt song song test
    ├── TC01_HappyPathTests.cs       ✅ 2 tests: Sinh khung đệm 3:1 & Luồng nghiệp vụ khép kín
    ├── TC02_ConcurrencyTests.cs     ✅ 1 test: Chạy 2 tác vụ song song tranh chấp 1 slot
    ├── TC03_NgoaiLeVaHangDoiTests.cs ✅ 9 tests: Ngoại lệ thời gian, trùng lịch, hàng đợi 3 mức, no-show
    ├── TC04_BaoMatRBACTests.cs      ✅ 6 tests: WebApplicationFactory kiểm thử phân quyền 4 vai trò
    └── TC05_DoiChieuKpiSqlTests.cs  ✅ 1 test: Đối chiếu 4 chỉ số KPI với ADO.NET SQL thuần
```

---

## Danh sách tài khoản demo (Mật khẩu chung: `123456`)

| Tên đăng nhập | Vai trò | Mục đích demo |
|---------------|---------|--------------|
| `admin` | Admin | Xem BI Dashboard, xuất CSV, quản lý danh mục, sinh ca trực, quét quá hạn |
| `letan01` | LeTan | Quầy lễ tân: Check-in nhanh, tiếp nhận walk-in W-xxx, xem cảnh báo vắng hẹn, quét quá hạn |
| `bacsi01` | BacSi | Phòng 101: Màn hình gọi số, phát Loa Bluetooth (Gọi tiếp, Gọi lại, Bắt đầu khám, Hoàn tất, Qua lượt) |
| `bacsi02` | BacSi | Phòng 102: Tai Mũi Họng |
| `bacsi03` | BacSi | Phòng 103: Da liễu |
| `benhnhan01` | BenhNhan | Đặt lịch trực tuyến 4 bước, xem lịch sử và số thứ tự |
| `benhnhan02` | BenhNhan | Đặt lịch trực tuyến |

---

## Hướng dẫn chạy và Kiểm thử

### 1. Chạy toàn bộ Test Suite (19 tests)
```powershell
dotnet test
```

### 2. Khởi chạy ứng dụng
```powershell
cd "ClinicBookingMVC"
dotnet run
```

- **Admin BI Dashboard**: `http://localhost:5072/Admin` (`admin` / `123456`)
- **Quầy Lễ tân**: `http://localhost:5072/LeTan` (`letan01` / `123456`)
- **Màn hình Bác sĩ (Gọi số + Bluetooth Speaker)**: `http://localhost:5072/BacSi` (`bacsi01` / `123456`)
- **Cổng Bệnh nhân đặt lịch**: `http://localhost:5072`
