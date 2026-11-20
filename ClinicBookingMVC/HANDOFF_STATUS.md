# HANDOFF STATUS — ClinicBookingMVC (HTTT-13)

**Cập nhật lần cuối:** 2026-11-20 · **Phase 4 HOÀN THÀNH**

---

## Tình trạng Build

```
dotnet build → ✅ Build succeeded. 0 Errors, 0 Warnings
Framework: net11.0 | SDK: 11.0.100
```

---

## Cấu trúc thư mục

```
ClinicBookingMVC/
├── Controllers/
│   ├── AdminController.cs        ✅ Full CRUD: ChuyenKhoa, PhongKham, BacSi, TaiKhoan, CaTruc + KhungGio
│   ├── BacSiController.cs        ✅ Dashboard ca trực, XacNhanPhieu, LichSuCa
│   ├── DatLichController.cs      ✅ Full: Index(4-step), LayBacSi, LayKhungGio, XacNhan, PhieuHen, HuyPhieu, TraCuu, TienDoHangDoi, TraCuuBenhNhan
│   ├── HomeController.cs         ✅ Mặc định
│   ├── LeTanController.cs        ✅ Index(hàng đợi), CheckIn, CheckInVangLai, GoiSo, BatDauKham, HoanTat, XacNhanPhieu
│   └── TaiKhoanController.cs     ✅ DangNhap/DangKy/DangXuat/KhongCoQuyen (HTTP 403)
│
├── Data/
│   ├── ClinicDbContext.cs         ✅ 10 DbSets + Full Fluent API
│   └── DbInitializer.cs          ✅ Seed: 4 VaiTro + admin/Admin@123
│
├── Database/
│   └── ClinicBookingDB.sql        ✅ Idempotent SQL script (EF-generated)
│
├── Migrations/
│   └── 20261119104438_InitialCreate.cs  ✅
│
├── Models/
│   ├── Entities/                  ✅ 10 entity classes
│   │   ├── VaiTro.cs, TaiKhoan.cs, ChuyenKhoa.cs, PhongKham.cs
│   │   ├── BacSi.cs, CaTruc.cs, KhungGio.cs, BenhNhan.cs
│   │   ├── PhieuDatHen.cs, PhieuSoThuTu.cs
│   └── ViewModels/
│       ├── AllViewModels.cs       ✅ DangNhapVM, DangKyVM, ChuyenKhoaVM, PhongKhamVM,
│       │                             TaoBacSiVM, SuaBacSiVM, TaoTaiKhoanVM, TaoCaTrucVM
│       └── Phase4ViewModels.cs   ✅ LeTanIndexVM, BenhNhanTrongCaVM, BacSiCaTrucVM, BacSiDashboardVM
│          + DatLichViewModels.cs ✅ ChonBacSiVM, BacSiTrongCaVM, LuoiKhungGioVM, SlotVM,
│                                    XacNhanDatLichVM, PhieuHenDienTuVM, TraCuuInputVM,
│                                    KetQuaTraCuuVM, PhieuHenTraCuuVM
│
├── Services/
│   ├── CaTrucService.cs           ✅ KiemTraTrungCaAsync, SinhKhungGio, DoiTrangThaiKhungGioAsync
│   └── DatLichService.cs          ✅ DatLichAsync (BR-01,02,03,06), HuyLichAsync, SinhMaDatHenAsync,
│                                     LayHoacTaoBenhNhanAsync, TraCuuBenhNhanAsync, LayTienDoHangDoiAsync
│
├── Views/
│   ├── Admin/                     ✅ 15 views: Index, ChuyenKhoa CRUD, PhongKham CRUD,
│   │                                  BacSi CRUD, TaiKhoan, CaTruc, KhungGioDetail
│   ├── BacSi/
│   │   ├── Index.cshtml           ✅ Dashboard ca trực, bảng bệnh nhân, nút XacNhan
│   │   └── LichSuCa.cshtml       ✅ Bảng ca trực theo tuần (tuan=0 tuần này)
│   ├── DatLich/
│   │   ├── Index.cshtml           ✅ 4-step booking UI (Chuyên khoa→Bác sĩ→Ngày→Khung giờ)
│   │   ├── XacNhan.cshtml         ✅ Form thông tin bệnh nhân + AJAX auto-fill SoDienThoai
│   │   ├── PhieuHen.cshtml        ✅ Phiếu hẹn điện tử + QRCode.js + nút in
│   │   └── TraCuu.cshtml          ✅ Tra cứu + thẻ trạng thái + AJAX poll 15s
│   ├── Home/Index.cshtml          ✅ Hero + 3-step features (no Bootstrap)
│   ├── LeTan/Index.cshtml         ✅ Hàng đợi thời gian thực, check-in, GoiSo, BatDauKham, HoanTat
│   ├── TaiKhoan/                  ✅ DangNhap.cshtml, DangKy.cshtml, KhongCoQuyen.cshtml
│   └── Shared/
│       ├── _Layout.cshtml         ✅ Cổng Bệnh nhân (coral warm)
│       └── _PortalLayout.cshtml   ✅ Cổng Nội bộ (graphite + amber, sidebar RBAC)
│
├── wwwroot/css/
│   ├── tokens.css                 ✅ CSS custom properties (Patient + Internal portals)
│   ├── base.css                   ✅ Reset, typography, container
│   ├── clinic.css                 ✅ Full component library (~700 lines)
│   │                                 + Phase 3 (booking, slots, e-ticket, tracuu, queue)
│   │                                 + Phase 4 (letan components, bacsi ca-section)
│   └── site.css                   ✅ Minimal
│
├── appsettings.json               ✅ ConnectionString → .\SQLEXPRESS;ClinicBookingDB
├── Program.cs                     ✅ Cookie Auth + CaTrucService + DatLichService DI + DbInitializer seed
├── DESIGN_SYSTEM.md               ✅ Hệ thống thiết kế 2 cổng
└── HANDOFF_STATUS.md              ✅ File này
```

---

## Tài khoản mặc định (seed)

| Tên tài khoản | Mật khẩu  | Vai trò |
|:---|:---|:---|
| `admin` | `Admin@123` | Admin |

---

## Phân quyền RBAC

| Controller | Vai trò được phép |
|:---|:---|
| `AdminController` | Admin |
| `LeTanController` | LeTan, Admin |
| `BacSiController` | BacSi |
| `TaiKhoanController` | Public (DangNhap, DangKy) + KhongCoQuyen |
| `DatLichController` | Public (Index, TraCuu) + [Authorize] (XacNhan, HuyPhieu) |
| `HomeController` | Public |

---

## Logic nghiệp vụ đã cài đặt

### CaTrucService
- **KiemTraTrungCaAsync** — kiểm tra trùng ca theo `(BacSi, NgayTruc, CaLamViec)` và `(PhongKham, NgayTruc, CaLamViec)`
- **SinhKhungGio** — sinh khung giờ tự động với tỉ lệ khung đệm (walk-in) configurable
- **DoiTrangThaiKhungGioAsync** — khóa/mở khóa, chặn khóa slot đã có phiếu hẹn chưa xử lý

### DatLichService
- **BR-01** — chỉ đặt lịch nếu còn ít nhất 30 phút trước giờ hẹn
- **BR-02** — Serializable transaction + RowVersion (`PhienBanDong`) chống đặt trùng
- **BR-03** — chỉ hủy lịch nếu còn ít nhất 2 giờ trước giờ hẹn
- **BR-06** — `CoCanhBao == true` → TrangThaiHen = `"ChoXacNhan"` thay vì `"DaXacNhan"`
- **SinhMaDatHenAsync** — format `BKyyMMdd-NNN` (vd: `BK261119-001`)
- **LayTienDoHangDoiAsync** — tiến độ hàng đợi theo phòng + số thứ tự

### AdminController
- CRUD đầy đủ với kiểm tra ràng buộc FK trước khi xóa
- Tạo BacSi → transaction tự động tạo TaiKhoan vai trò BacSi đi kèm (1-1)
- Tạo CaTruc → gọi `CaTrucService.SinhKhungGio` ngay sau khi lưu
- Khóa/Mở khóa KhungGio từng slot một

### LeTanController
- **CheckIn** — tạo PhieuSoThuTu, cập nhật PhieuDatHen → `DaCheckIn`
- **CheckInVangLai** — tạo/tìm BenhNhan, tạo PhieuSoThuTu (MaPhieuHen = null)
- **GoiSo** — gọi số, gọi lại (tối đa 3 lần), tự động → `QuaLuot` + `VangHen` nếu vắng
- **BatDauKham** — chuyển `DangKham`, ghi `ThoiGianBatDauKham`
- **HoanTat** — chuyển `HoanTat`, cập nhật PhieuDatHen → `DaKham`
- **XacNhanPhieu** — `ChoXacNhan` → `DaXacNhan` (Lễ tân duyệt)

### BacSiController
- **Index** — dashboard ca trực hôm nay, bảng bệnh nhân per-ca
- **XacNhanPhieu** — `ChoXacNhan` → `DaXacNhan` (Bác sĩ duyệt)
- **LichSuCa** — lịch sử ca theo tuần (tuan=0, -1, -2...)

---

## Chuỗi kết nối
```
Server=.\SQLEXPRESS;Database=ClinicBookingDB;Trusted_Connection=True;TrustServerCertificate=True;
```

## Ghi chú kỹ thuật
- **Framework**: net11.0 (EF Core 11, .NET SDK 11.0.100)
- **Font**: Be Vietnam Pro (Patient portal) + JetBrains Mono (codes/times)
- **CSS**: tokens.css → base.css → clinic.css (~700 lines, NO Bootstrap)
- **Auth**: Cookie-based, 8h expire, sliding, roles từ bảng VaiTro
- **KhungGio.PhienBanDong**: ROWVERSION cho optimistic concurrency (1-1 với PhieuDatHen)
- **BenhNhan.CoCanhBao**: Computed persisted column (`SoLanVangHen >= 2`)
- **KhungGio** ↔ **PhieuDatHen**: quan hệ 1-1 (một khung giờ — một phiếu hẹn hiệu lực)
- **Auto-refresh**: LeTan/Index tự làm mới mỗi 30s (JS setTimeout reload), TraCuu poll AJAX 15s

---

## GIAI ĐOẠN 5 — Tùy chọn nâng cao (nếu cần)

| Hạng mục | Mô tả |
|:---|:---|
| Phân trang | BenhNhan, PhieuDatHen (PagedList hoặc LINQ Skip/Take) |
| Tìm kiếm/lọc | Filter PhieuDatHen theo ngày, trạng thái; tìm BenhNhan theo tên, SĐT |
| Email/SMS nhắc lịch | Background job gửi nhắc lịch (`GuiNhacLich == true`) |
| Báo cáo Admin | Thống kê số lượt khám theo ngày/tuần/bác sĩ/chuyên khoa |
| Xuất Excel | Export danh sách bệnh nhân trong ngày ra Excel |
| Màn hình TV hàng đợi | Trang public `/HangDoi/{maPhong}` hiển thị số đang gọi (không cần SignalR — AJAX poll 5s) |


---

## Tình trạng Build

```
dotnet build → ✅ Build succeeded. 0 Errors, 0 Warnings
Framework: net11.0 | SDK: 11.0.100
```

---

## Cấu trúc thư mục

```
ClinicBookingMVC/
├── Controllers/
│   ├── AdminController.cs        ✅ Full CRUD: ChuyenKhoa, PhongKham, BacSi, TaiKhoan, CaTruc + KhungGio
│   ├── BacSiController.cs        ✅ Stub [Authorize(Roles="BacSi")]
│   ├── DatLichController.cs      ✅ Public Index/TraCuu + [Authorize] TaoPhieu/HuyPhieu
│   ├── HomeController.cs         ✅ Mặc định
│   ├── LeTanController.cs        ✅ Stub [Authorize(Roles="LeTan,Admin")]
│   └── TaiKhoanController.cs     ✅ DangNhap/DangKy/DangXuat/KhongCoQuyen (HTTP 403)
│
├── Data/
│   ├── ClinicDbContext.cs         ✅ 10 DbSets + Full Fluent API
│   └── DbInitializer.cs          ✅ Seed: 4 VaiTro + admin/Admin@123
│
├── Database/
│   └── ClinicBookingDB.sql        ✅ Idempotent SQL script (EF-generated)
│
├── Migrations/
│   └── 20261119104438_InitialCreate.cs  ✅
│
├── Models/
│   ├── Entities/                  ✅ 10 entity classes
│   │   ├── VaiTro.cs, TaiKhoan.cs, ChuyenKhoa.cs, PhongKham.cs
│   │   ├── BacSi.cs, CaTruc.cs, KhungGio.cs, BenhNhan.cs
│   │   ├── PhieuDatHen.cs, PhieuSoThuTu.cs
│   └── ViewModels/
│       └── AllViewModels.cs       ✅ DangNhapVM, DangKyVM, ChuyenKhoaVM, PhongKhamVM,
│                                     TaoBacSiVM, SuaBacSiVM, TaoTaiKhoanVM, TaoCaTrucVM
│
├── Services/
│   └── CaTrucService.cs           ✅ KiemTraTrungCaAsync, SinhKhungGio, DoiTrangThaiKhungGioAsync
│
├── Views/
│   ├── Admin/                     ✅ 10 views: Index, ChuyenKhoa, TaoChuyenKhoa, SuaChuyenKhoa,
│   │                                  PhongKham, TaoPhongKham, SuaPhongKham,
│   │                                  BacSi, TaoBacSi, SuaBacSi,
│   │                                  TaiKhoan, TaoTaiKhoan,
│   │                                  CaTruc, TaoCaTruc, KhungGioDetail
│   ├── BacSi/Index.cshtml         ✅ Stub
│   ├── DatLich/                   ✅ Index.cshtml, TraCuu.cshtml
│   ├── Home/Index.cshtml          ✅ Hero + 3-step features (no Bootstrap)
│   ├── LeTan/Index.cshtml         ✅ Stub
│   ├── TaiKhoan/                  ✅ DangNhap.cshtml, DangKy.cshtml, KhongCoQuyen.cshtml
│   └── Shared/
│       ├── _Layout.cshtml         ✅ Cổng Bệnh nhân (coral warm)
│       └── _PortalLayout.cshtml   ✅ Cổng Nội bộ (graphite + amber, sidebar RBAC)
│
├── wwwroot/css/
│   ├── tokens.css                 ✅ CSS custom properties (Patient + Internal portals)
│   ├── base.css                   ✅ Reset, typography, container
│   ├── clinic.css                 ✅ Full component library (btn, badge, table, form, slot-grid...)
│   └── site.css                   ✅ Minimal (không dùng Bootstrap defaults)
│
├── appsettings.json               ✅ ConnectionString → .\SQLEXPRESS;ClinicBookingDB
├── Program.cs                     ✅ Cookie Auth + CaTrucService DI + DbInitializer seed
├── DESIGN_SYSTEM.md               ✅ Hệ thống thiết kế 2 cổng
└── HANDOFF_STATUS.md              ✅ File này
```

---

## Tài khoản mặc định (seed)

| Tên tài khoản | Mật khẩu  | Vai trò |
|:---|:---|:---|
| `admin` | `Admin@123` | Admin |

---

## Phân quyền RBAC

| Controller | Vai trò được phép |
|:---|:---|
| `AdminController` | Admin |
| `LeTanController` | LeTan, Admin |
| `BacSiController` | BacSi |
| `TaiKhoanController` | Public (DangNhap, DangKy) + KhongCoQuyen |
| `DatLichController` | Public (Index, TraCuu) + [Authorize] (TaoPhieu, HuyPhieu) |
| `HomeController` | Public |

---

## Logic nghiệp vụ đã cài đặt

### CaTrucService
- **KiemTraTrungCaAsync** — kiểm tra trùng ca theo `(BacSi, NgayTruc, CaLamViec)` và `(PhongKham, NgayTruc, CaLamViec)`
- **SinhKhungGio** — sinh khung giờ tự động với tỉ lệ khung đệm (walk-in) configurable
  - `tanSuatKhungDem = 0`: không chèn walk-in
  - `tanSuatKhungDem = N`: cứ N slot DatHen chèn 1 KhungDem
- **DoiTrangThaiKhungGioAsync** — khóa/mở khóa, chặn khóa slot đã có phiếu hẹn chưa xử lý

### AdminController
- CRUD đầy đủ với kiểm tra ràng buộc FK trước khi xóa
- Tạo BacSi → transaction tự động tạo TaiKhoan vai trò BacSi đi kèm (1-1)
- Tạo CaTruc → gọi `CaTrucService.SinhKhungGio` ngay sau khi lưu
- Khóa/Mở khóa KhungGio từng slot một

---

## GIAI ĐOẠN 3 — Việc cần làm tiếp theo

### 3.1. Luồng Đặt lịch (Bệnh nhân)
- `DatLichController.Index` — hiển thị form chọn ChuyenKhoa → BacSi → Ngày → KhungGio
- `DatLichController.TaoPhieu` — tạo `PhieuDatHen` với optimistic concurrency trên `KhungGio.PhienBanDong`
- `DatLichController.TraCuu` — tra cứu theo `MaDatHen` hoặc `SoDienThoai`
- `DatLichController.HuyPhieu` — hủy phiếu, trả `KhungGio` về `ConTrong`
- Sinh `MaDatHen` theo format `DH-YYYYMMDD-NNN`

### 3.2. Check-in & Hàng đợi (Lễ tân)
- `LeTanController.Index` — hiển thị danh sách bệnh nhân trong ngày theo phòng
- `LeTanController.CheckIn` — xác nhận bệnh nhân đến, tạo `PhieuSoThuTu`
- `LeTanController.GoiSo` — gọi số (tăng `SoLanGoi`, đặt `ThoiGianGoiSo`)
- `LeTanController.BatDauKham` — chuyển trạng thái sang `DangKham`
- `LeTanController.HoanTat` — hoàn tất khám, tính `SoLanVangHen` nếu vắng

### 3.3. Dashboard Bác sĩ
- `BacSiController.Index` — danh sách bệnh nhân trong ca trực hiện tại
- `BacSiController.XacNhanPhieu` — xác nhận `PhieuDatHen` (ChoXacNhan → DaXacNhan)

### 3.4. Tiện ích
- Phân trang cho các bảng lớn (BenhNhan, PhieuDatHen)
- Tìm kiếm/lọc BenhNhan theo tên, SoDienThoai
- Filter PhieuDatHen theo ngày, trạng thái
- Xử lý `DbUpdateConcurrencyException` khi nhiều người cùng đặt 1 khung giờ

---

## Chuỗi kết nối
```
Server=.\SQLEXPRESS;Database=ClinicBookingDB;Trusted_Connection=True;TrustServerCertificate=True;
```

## Ghi chú kỹ thuật
- **Framework**: net11.0 (EF Core 11, .NET SDK 11.0.100)
- **Font**: Be Vietnam Pro (Patient portal) + JetBrains Mono (codes/times)
- **CSS**: tokens.css → base.css → clinic.css (NO Bootstrap)
- **Auth**: Cookie-based, 8h expire, sliding, roles từ bảng VaiTro
- **KhungGio.PhienBanDong**: ROWVERSION cho optimistic concurrency
- **BenhNhan.CoCanhBao**: Computed persisted column (`SoLanVangHen >= 2`)
