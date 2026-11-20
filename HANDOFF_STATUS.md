# HANDOFF STATUS — ClinicBookingMVC (HTTT-13)

Cập nhật lần cuối: 2026-11-20 (Hoàn thành Giai đoạn 5)

---

## ✅ BUILD STATUS: PASSED — 0 Errors, 0 Warnings

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:01.04
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

---

## Chi tiết các thành phần Giai đoạn 5 đã triển khai

### 1. Tiến trình chạy ngầm & Tự động xử lý (`Services/VangHenBackgroundService.cs`, `Services/VangHenService.cs`)
- **`VangHenBackgroundService`**: Kế thừa `BackgroundService` (`IHostedService`), quét định kỳ mỗi **5 phút/lần**.
- **Tự động No-Show (BR-05)**: Quét các phiếu hẹn ở trạng thái `ChoXacNhan` hoặc `DaXacNhan` quá **30 phút** so với giờ hẹn (`NgayTruc + GioBatDau`) mà chưa check-in:
  - Cập nhật `TrangThaiHen = "VangHen"`
  - Tăng `BenhNhan.SoLanVangHen += 1` (SQL Server tự động kích hoạt `CoCanhBao = 1` khi $\ge 2$).
- **Tự động nhắc lịch**: Quét các phiếu diễn ra vào ngày mai (`Tomorrow`) có `GuiNhacLich == false`:
  - Ghi log nhắc lịch: Họ tên, SĐT, Giờ khám.
  - Cập nhật `GuiNhacLich = true`.
- **Nút "Quét quá hạn ngay"**:
  - Tích hợp trực tiếp trên cả giao diện **Quầy Lễ tân** (`/LeTan`) và **Admin Dashboard** (`/Admin`).
  - Hỗ trợ thao tác tức thì khi demo trước Hội đồng mà không phải chờ chu kỳ 5 phút.

### 2. BI Dashboard Thống kê KPI (`AdminController` + `Services/BaoCaoService.cs` + `Views/Admin/Index.cshtml`)
- **Giao diện chuẩn Hallmark Design System**: tone graphite nội bộ, phân chia bằng 1px hairline rule, typography rõ nét (`Be Vietnam Pro` + `JetBrains Mono`).
- **Bộ lọc đa chiều**: Lọc theo *Từ ngày – Đến ngày*, *Chuyên khoa*, *Bác sĩ*.
- **4 Chỉ số KPI cốt lõi**:
  1. **Tỷ lệ vắng hẹn (No-Show Rate %)**: `Số phiếu VangHen / Tổng phiếu (DaXacNhan + DaCheckIn + DaKham + VangHen) * 100`
  2. **Tỷ lệ hoàn thành khám (%)**: `Số phiếu DaKham / Tổng phiếu đặt hẹn trong kỳ * 100`
  3. **Thời gian chờ khám trung bình (Phút)**: `AVG(DATEDIFF(MINUTE, ThoiGianCheckIn, ThoiGianGoiSo))` trên các lượt đã được gọi số.
  4. **Tỷ lệ lấp đầy khung giờ (%)**: `Số KhungGio (DatHen) đã có hẹn / Tổng KhungGio (DatHen) mở trong kỳ * 100`
- **Biểu đồ trực quan (Chart.js 4.4.1)**:
  - **Lưu lượng theo khung giờ (07:00 – 17:00)**: Bar chart phát hiện giờ cao điểm phòng khám.
  - **Tỷ lệ vắng hẹn theo từng Bác sĩ**: Horizontal bar chart trực quan so sánh tỷ lệ no-show.
  - **Danh sách bệnh nhân cảnh báo**: Bảng quản lý các bệnh nhân có `CoCanhBao == true` (`SoLanVangHen >= 2`) kèm badge cảnh báo đỏ nổi bật.
- **Xuất báo cáo CSV UTF-8 BOM**:
  - Gắn tiền tố byte `EF BB BF` mở chuẩn tiếng Việt không bị lỗi font trên Microsoft Excel Windows.
  - Tích hợp đầy đủ KPI, thống kê bác sĩ, lưu lượng và danh sách bệnh nhân cảnh báo.

### 3. Bộ khởi tạo dữ liệu mẫu toàn diện (`Data/DbInitializer.cs`)
- **4 vai trò**: `Admin`, `LeTan`, `BacSi`, `BenhNhan`.
- **Mật khẩu chung**: `123456` (đã băm bằng BCrypt):
  - `admin` (Admin)
  - `letan01` (LeTan)
  - `bacsi01`, `bacsi02`, `bacsi03` (BacSi)
  - `benhnhan01`, `benhnhan02` (BenhNhan)
- **3 Chuyên khoa & 3 Phòng khám**:
  - Phòng 101 - Tầng 1: Nội tổng quát (BS.CKI Nguyễn Văn An)
  - Phòng 102 - Tầng 1: Tai Mũi Họng (ThS.BS Trần Thị Bình)
  - Phòng 103 - Tầng 2: Da liễu (BS.CKI Lê Hoàng Cường)
- **120 Ca trực**: Trải dài từ 14 ngày trước đến 5 ngày tới (mỗi ngày đủ 2 ca Sáng/Chiều cho 3 bác sĩ x 3 phòng, 8 slot/ca, chuẩn 3 DatHen - 1 KhungDem).
- **161 Phiếu đặt hẹn lịch sử**: Đầy đủ trạng thái `DaKham` (90), `VangHen` (24), `DaXacNhan` (30), `DaHuy` (12), `DaCheckIn` (5).
- **10 Bệnh nhân đang ngồi chờ hôm nay**:
  - Phân bổ đủ cả 3 loại mã số: `A-xxx` (Đặt trước đúng giờ), `W-xxx` (Vãng lai walk-in), `L-xxx` (Đặt trước trễ).
  - Có 2 bệnh nhân có `SoLanVangHen >= 2` (`CoCanhBao = 1`): *Hoàng Văn Cảnh Báo* và *Trần Thị Vắng*.
  - Tại Phòng 101 có sẵn 1 bệnh nhân `DangKham`, 1 bệnh nhân `DangGoi` và 3 bệnh nhân `DangCho` để kiểm thử Loa Bluetooth (Ding-Dong + TTS tiếng Việt) ngay lập tức.

---

## Danh sách tài khoản demo (Mật khẩu: `123456`)

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

## Hướng dẫn chạy và Demo

```powershell
cd "C:\Users\Asus\Desktop\Phong\ClinicBookingMVC"
dotnet run
```

- **Admin BI Dashboard**: Truy cập `http://localhost:5072/Admin` (Đăng nhập `admin` / `123456`)
- **Quầy Lễ tân**: Truy cập `http://localhost:5072/LeTan` (Đăng nhập `letan01` / `123456`)
- **Màn hình Bác sĩ (Gọi số + Bluetooth Speaker)**: Truy cập `http://localhost:5072/BacSi` (Đăng nhập `bacsi01` / `123456`)
- **Cổng Bệnh nhân đặt lịch**: Truy cập `http://localhost:5072`
