# HANDOFF STATUS — ClinicBookingMVC (HTTT-13)

Cập nhật lần cuối: 2026-11-20

---

## ✅ BUILD STATUS: PASSED — 0 Errors, 0 Warnings

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:05.86
```

---

## Tiến độ theo Giai đoạn

| Giai đoạn | Nội dung | Trạng thái |
|-----------|----------|------------|
| 1 | DESIGN_SYSTEM.md + Cấu trúc dự án ban đầu | ✅ Hoàn thành |
| 2 | CSS/Layout + RBAC 4 vai trò + Seed data + AdminController CRUD | ✅ Hoàn thành |
| 3 | Luồng Đặt lịch 4 bước + DatLichService (BR-01..BR-06) + Tra cứu hàng đợi | ✅ Hoàn thành |
| 4 | HangDoiService 3 mức ưu tiên + Lễ tân + Bác sĩ AJAX + Bluetooth TTS | ✅ Hoàn thành |
| 5 (Fix) | Fix build BacSi/Index.cshtml + CSS bổ sung phase5 | ✅ Hoàn thành |

---

## Cấu trúc dự án hiện tại

```
ClinicBookingMVC/
├── Controllers/
│   ├── AdminController.cs          ✅ CRUD ChuyenKhoa/PhongKham/BacSi/TaiKhoan + SinhKhungGio
│   ├── BacSiController.cs          ✅ GoiSoTiepTheo/GoiLai/BatDauKham/HoanTat/QuaLuot (AJAX)
│   ├── DatLichController.cs        ✅ Luồng 4 bước + BR-01..BR-06 + Serializable TX
│   ├── HomeController.cs           ✅ Trang chủ bệnh nhân
│   ├── LeTanController.cs          ✅ CheckIn/CheckInVangLai/TraCuuNhanh/XacNhanPhieu/KhoiPhuc
│   └── TaiKhoanController.cs       ✅ DangNhap/DangKy/DangXuat/KhongCoQuyen (BCrypt)
├── Models/
│   ├── Entities/                   ✅ 10 entity chuẩn 3NF
│   │   ├── VaiTro.cs
│   │   ├── TaiKhoan.cs
│   │   ├── ChuyenKhoa.cs
│   │   ├── PhongKham.cs
│   │   ├── BacSi.cs
│   │   ├── CaTruc.cs
│   │   ├── KhungGio.cs
│   │   ├── BenhNhan.cs
│   │   ├── PhieuDatHen.cs
│   │   └── PhieuSoThuTu.cs
│   └── ViewModels/
│       ├── Phase2ViewModels.cs     ✅ Admin VMs
│       ├── Phase3ViewModels.cs     ✅ DatLich VMs
│       └── Phase4ViewModels.cs     ✅ HangDoi VMs + BacSiDashboardVM
├── Services/
│   ├── DatLichService.cs           ✅ BR-01..BR-06, Serializable TX, RowVersion
│   ├── HangDoiService.cs           ✅ 3 mức ưu tiên + cửa sổ 15 phút + A/W/L prefix
│   └── CaTrucService.cs            ✅ SinhKhungGioTuDong (DatHen/KhungDem)
├── Data/
│   ├── ClinicDbContext.cs          ✅ 10 bảng + Unique indexes + UX_KhungGio_HieuLuc
│   └── DbInitializer.cs            ✅ Seed: admin/Admin@123, 4 vai trò, dữ liệu mẫu
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml          ✅ Cổng Bệnh nhân (ấm, thoáng)
│   │   └── _PortalLayout.cshtml   ✅ Cổng Nội bộ (graphite, dense)
│   ├── Admin/                      ✅ CRUD views + SinhKhungGio
│   ├── BacSi/
│   │   └── Index.cshtml            ✅ BT toolbar + màn hình gọi số + AJAX điều khiển (FIXED)
│   ├── DatLich/                    ✅ Luồng 4 bước
│   ├── Home/                       ✅ Trang chủ bệnh nhân
│   ├── LeTan/
│   │   └── Index.cshtml            ✅ Lưới phòng + check-in nhanh + hàng đợi A/W/L
│   └── TaiKhoan/                   ✅ DangNhap/DangKy
├── wwwroot/
│   ├── css/
│   │   ├── tokens.css              ✅ CSS custom properties (Hallmark design tokens)
│   │   ├── base.css                ✅ Reset + typography
│   │   ├── clinic.css              ✅ Component library
│   │   └── clinic-phase5.css       ✅ MỚI: phong-grid, checkin-nhanh, benhnhan-preview, color-walkin/late
│   └── js/
│       └── bluetooth-speaker.js    ✅ Web Audio API Ding-Dong + TTS vi-VN
└── Program.cs                      ✅ DI: DatLichService, HangDoiService, CaTrucService, Cookie Auth
```

---

## Quy tắc nghiệp vụ đã cài đặt

| Mã | Mô tả | File |
|----|-------|------|
| BR-01 | Chỉ đặt khi còn ≥30 phút | `DatLichService.cs` |
| BR-02 | Chống trùng Serializable TX + RowVersion + UX unique | `DatLichService.cs` |
| BR-03 | Chỉ hủy khi còn ≥2 giờ | `DatLichService.cs` |
| BR-06 | CoCanhBao (≥2 lần vắng) → TrangThaiHen=ChoXacNhan | `DatLichService.cs` |
| HQ-01 | DatTruoc ≤15 phút → A-xxx, MucUuTien=1 | `HangDoiService.cs` |
| HQ-02 | VangLai → W-xxx, MucUuTien=2 | `HangDoiService.cs` |
| HQ-03 | DatTruocTre 16-30 phút → L-xxx, MucUuTien=3 | `HangDoiService.cs` |
| HQ-04 | Trễ >30 phút → từ chối check-in theo hẹn | `HangDoiService.cs` |
| HQ-05 | GoiSoTiepTheo: 4 nhóm ưu tiên + cửa sổ 15 phút | `HangDoiService.cs` |
| HQ-06 | GoiLai tối đa 3 lần; QuaLuot → VangHen | `HangDoiService.cs` |
| HQ-07 | KhoiPhuc → DangCho, MucUuTien=3 | `HangDoiService.cs` |

---

## Tài khoản seed

| Vai trò | Tên đăng nhập | Mật khẩu |
|---------|--------------|----------|
| Admin | admin | Admin@123 |
| LeTan | letan01 | LeTan@123 |
| BacSi | bacsi01 | BacSi@123 |
| BenhNhan | (tự đăng ký) | — |

---

## Mã số thứ tự

| Prefix | Loại | Điều kiện |
|--------|------|-----------|
| `A-001` | DatTruoc | Đúng giờ / sớm / trễ ≤15 phút |
| `W-001` | VangLai | Walk-in (khách vãng lai) |
| `L-001` | DatTruocTre | Trễ 16–30 phút |

---

## Lỗi đã fix trong Session này (Giai đoạn 5)

| File | Lỗi | Giải pháp |
|------|-----|-----------|
| `Views/BacSi/Index.cshtml` | 30 Razor parser errors: `@maPhong` trong `<text>` block hiểu là constructor call | Inject data qua `<script type="application/json" id="bs-init-data">` + `JSON.parse()` trong JS |
| `Views/BacSi/Index.cshtml` | `@media`/`@keyframes` trong `@section Styles` hiểu là Razor directive | Escape thành `@@media` / `@@keyframes` |
| `Views/BacSi/Index.cshtml` | `@foreach` → `setInterval(@maPhong)` gây lỗi | Thay bằng `_maPhongList.forEach(mp => setInterval(...))` |

---

## Ghi chú kỹ thuật

- **Framework:** net11.0, EF Core 11, BCrypt.Net-Next
- **DB:** SQL Server — EnsureCreatedAsync + DbInitializer seed khi khởi động lần đầu
- **Migration:** Chưa apply migration lên SQL Server thực tế (EnsureCreatedAsync tự tạo schema)
- **TempData keys:** `"SuccessMessage"` / `"ErrorMessage"` (layout đọc tự động)
- **MaDatHen format:** `BKyyMMdd-NNN` (vd: `BK261119-001`)
- **BluetoothSpeaker:** Web Audio API (không dùng file MP3), TTS vi-VN, IIFE pattern

---

## Bước tiếp theo (nếu cần)

1. `dotnet ef migrations add InitialCreate` + `Update-Database` để apply schema lên SQL Server thực
2. Kiểm thử end-to-end: Đăng nhập → Đặt lịch → Check-in → Gọi số → Bluetooth TTS
3. Giai đoạn 6 (nếu có): Báo cáo thống kê Admin, export Excel, SMS reminder
