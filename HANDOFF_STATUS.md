# HANDOFF STATUS — ClinicBookingMVC (HTTT-13)

**Cập nhật lần cuối:** 2026-11-20 · **Hoàn thành Giai đoạn 6C (Kiểm thử chức năng End-to-End & Cẩm nang Hướng dẫn sử dụng)**

---

## ✅ BUILD & TEST STATUS: PASSED — 0 Errors, 0 Warnings, 20/20 Tests Passed

```
Solution: ClinicBooking.sln
Framework: net11.0 | SDK: 11.0.100
Build succeeded.
    0 Warning(s)
    0 Error(s)

Test Run Successful.
    Total Tests: 20
    Passed: 20 (100%)
    Failed: 0
    Skipped: 0
    Duration: 4.0s (ClinicBookingMVC.Tests.dll)
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
| 6B | Chuyển toàn bộ hệ thống sang Chế độ Sáng (Tabler Light Mode) + Chuẩn hóa 100% Nút bấm, Thẻ, Bảng, Biểu đồ | ✅ Hoàn thành |
| 6C | Kiểm thử thực tế End-to-End 4 vai trò + Biên soạn KET_QUA_KIEM_THU_CHUC_NANG.md + Cẩm nang HUONG_DAN_SU_DUNG_HE_THONG.md | ✅ Hoàn thành |

---

## Chi tiết Giai đoạn 6C đã triển khai

### 1. Kiểm thử chức năng toàn diện (End-to-End) theo từng vai trò & tài khoản demo
- **Bệnh nhân (Guest & BenhNhan - `benhnhan01`, `benhnhan02`):**
  - Trang chủ, danh mục chuyên khoa, danh sách bác sĩ.
  - Đăng ký tài khoản mới tự động gán vai trò `BenhNhan`, mật khẩu băm BCrypt.
  - Đăng nhập, đăng xuất Cookie Auth an toàn.
  - Chặn đặt lịch khi chưa đăng nhập (tự động chuyển hướng về trang đăng nhập kèm ReturnUrl).
  - Đặt lịch trực tuyến 4 bước: Chọn Chuyên khoa -> Bác sĩ -> Ngày -> Slot `ConTrong` -> Phiếu hẹn `BK...` + QR Code UTF-8.
  - Kiểm tra các ca chặn hợp lệ: chặn đặt sát giờ (< 30 phút), chặn trùng lịch cùng SĐT trong ngày, chống race condition đặt cùng slot song song (`IsolationLevel.Serializable` + `PhienBanDong`).
  - Tra cứu phiếu hẹn và xem tiến độ hàng đợi buồng khám thực tế (che số điện thoại riêng tư).
  - Quy tắc hủy lịch: Chặn hủy khi còn < 2 giờ; hủy thành công khi >= 2 giờ và tự động mở lại khung giờ `ConTrong`.
- **Lễ tân (`letan01`):**
  - Đăng nhập Cổng nội bộ, menu lọc đúng thẩm quyền.
  - Xem và lọc danh sách lịch hẹn trong ngày; duyệt phiếu `ChoXacNhan` cho bệnh nhân có cờ cảnh báo.
  - Hiển thị cảnh báo đỏ nổi bật với bệnh nhân vắng hẹn >= 2 lần (`CoCanhBao = true`).
  - Check-in đặt trước: Đúng giờ cấp `A-xxx` (ưu tiên 1); trễ 16-30 phút cấp `L-xxx` (ưu tiên 3); trễ > 30 phút từ chối theo lịch hẹn.
  - Tiếp nhận khách vãng lai: Tự động điền thông tin khách quen qua SĐT, gợi ý phòng ít người chờ nhất, cấp `W-xxx` (ưu tiên 2).
  - Khôi phục bệnh nhân bị bác sĩ bấm "Qua lượt" trở lại hàng chờ (`DangCho`, ưu tiên 3).
  - Quét quá hạn vắng hẹn thủ công cập nhật `VangHen`, tăng `SoLanVangHen`, bật cảnh báo.
- **Bác sĩ (`bacsi01`, `bacsi02`, `bacsi03`):**
  - Đăng nhập tự động chọn buồng khám được phân công hôm nay.
  - Gọi số tiếp theo tuân thủ thuật toán 3 cấp ưu tiên (A -> W -> L), khách A đến sớm không nhảy cóc, gọi ngay W nếu A chưa check-in.
  - Gọi lại tối đa 3 lần (`SoLanGoi <= 3`), khóa nút sau lần 3, hiển thị khuyến nghị bấm "Qua lượt".
  - Bắt đầu khám (`DangKham`) và Hoàn tất khám (`HoanTat`), tự động đồng bộ `PhieuDatHen.TrangThaiHen = "DaKham"`.
  - Loa gọi số Bluetooth: Phát chuông Ding-dong Web Audio API, đọc TTS Web Speech API tiếng Việt rõ ràng, thanh trượt âm lượng/tốc độ/chọn giọng.
  - Xem lịch sử phục vụ trong ca trực lâm sàng.
- **Quản trị viên (`admin`):**
  - CRUD Chuyên khoa, Phòng khám, Bác sĩ, Tài khoản với ràng buộc khóa ngoại an toàn.
  - Tạo ca trực tự động sinh khung giờ tỷ lệ 3:1 (3 slot Đặt hẹn : 1 slot Khung đệm), chặn trùng ca trực cùng bác sĩ hoặc phòng trong buổi.
  - Khóa / Mở khóa khung giờ thủ công (`TamKhoa` <-> `ConTrong`).
  - BI Dashboard KPI (4 chỉ số vàng) và 2 biểu đồ Chart.js nền sáng Tabler, bộ lọc đa chiều.
  - Xuất báo cáo CSV UTF-8 BOM (`\uFEFF`) mở trực tiếp trên Excel không lỗi font tiếng Việt.
- **Bảo mật phân quyền chéo (RBAC):**
  - Kiểm thử ma trận truy cập trái phép chéo giữa 4 vai trò, 100% bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`.

### 2. Phát hiện & Sửa triệt để các lỗi Edge Case trong Test Suite
- **Lỗi duplicate key trong BacSiController.Index khi Bác sĩ có nhiều ca trực trong cùng phòng:** Khi Bác sĩ (như `bacsi02`) được phân công 2 ca trực trong cùng ngày tại cùng 1 phòng (ví dụ Ca Sáng và Ca Chiều tại Phòng 102), dòng `ViewBag.TenPhongMap = caTrucs.ToDictionary(c => c.MaPhong, c => c.PhongKham.TenPhong)` ném ra ngoại lệ `System.ArgumentException: An item with the same key has already been added. Key: 2`. Đã sửa thành `caTrucs.DistinctBy(c => c.MaPhong).ToDictionary(...)` và bổ sung test tự động `BacSiController_DangNhapBacSi02_NhieuCaTrucCungPhongTrongNgay_KhongBiLoiToDictionary`.
- **Lỗi tràn ranh giới ngày khi chạy test ca đêm:** Các test case thời gian (`TC01_HappyPathTests` và `TC03_NgoaiLeVaHangDoiTests`) trước đây dùng `DateTime.Now.AddMinutes(40)` và `DateTime.Now.AddHours(3)` kết hợp với `DateOnly.FromDateTime(DateTime.Today)` dẫn đến khi chạy test sau 21:00 đêm, giờ hẹn bị tràn sang ngày hôm sau trong khi ngày trực vẫn gán là hôm nay (gây vi phạm CHECK constraint `CK_CaTruc_GioHopLe` và tính sai thời gian hủy lịch). Đã sửa để tính toán đồng bộ cả `DateOnly` và `TimeOnly` từ thời điểm đích, đảm bảo 100% test case pass bất kể thời điểm chạy trong ngày.

### 3. Xuất xưởng 02 Tài liệu Markdown chi tiết tại Thư mục gốc
1. `KET_QUA_KIEM_THU_CHUC_NANG.md`: Bảng tài khoản mẫu + Bảng kết quả kiểm thử chức năng chi tiết 28 kịch bản bao phủ 04 vai trò và bảo mật RBAC.
2. `HUONG_DAN_SU_DUNG_HE_THONG.md`: Cẩm nang hướng dẫn sử dụng toàn diện gồm 4 phần lớn, mỗi chức năng đều có đủ 4 mục chuẩn: Ý nghĩa nghiệp vụ, Quyền & Đường dẫn, Hướng dẫn từng bước với đúng tên nhãn/nút bấm trên giao diện, và Kết quả hệ thống kèm quy tắc nghiệp vụ `BR-01` đến `BR-07`.

---

## Chi tiết Giai đoạn 6B đã triển khai

### 1. Hệ màu Light Mode chuẩn Tabler Y tế (`tokens.css`, `base.css`, `clinic.css`, `clinic-phase5.css`)
- **Nền trang (Page Canvas):** `#F6F8FB` (sáng dịu, thanh lịch, chuyên nghiệp).
- **Bề mặt (Surface - Thẻ, Bảng, Navbar, Sidebar):** `#FFFFFF` đi kèm viền sắc nét `1px solid #DCE1E7` và bóng đổ nhẹ đặc trưng Tabler (`0 1px 2px rgba(24, 36, 51, 0.04)`).
- **Màu chữ:** Chữ chính `#182433` (đậm rõ nét, không dùng đen tuyền), chữ thứ cấp `#334155`, chữ phụ/nhãn `#667382`. Nền hover dòng bảng `#F1F5F9`.
- **Màu chủ đạo Y tế (Medical Teal):** Primary `#0F766E` (hover `#115E59`, soft `#F0FDFA`).
- **Màu trạng thái ngữ nghĩa:**
  - `Success`: `#2FB344` (đậm `#15803D`, soft `#DCFCE7`).
  - `Warning`: Nền dịu mắt `#FEF3C7`, viền `#FCD34D`, chữ `#92400E` (không dùng nền vàng chói chữ trắng).
  - `Danger`: `#D63939` (đậm `#B91C1C`, soft `#FEE2E2`).
  - `Info`: `#066FD1` (đậm `#0369A1`, soft `#E0F2FE`).
- **Loại bỏ hoàn toàn Dark Mode:** Xóa bỏ toàn bộ nền tối, sidebar đen đặc, hoặc style inline màu tối còn sót lại ở cả `_Layout.cshtml` (Bệnh nhân) lẫn `_PortalLayout.cshtml` (Admin, Lễ tân, Bác sĩ).

### 2. Chuẩn hóa 100% Hệ thống Nút bấm (Button Architecture)
- **Hình học chuẩn:** Chiều cao `38px` (min-height `38px`), bo góc `6px` (`--radius-btn`), font `500 var(--text-sm)`, inline-flex canh giữa kèm `gap: 6px`. Nút nhỏ bảng `.btn-sm` có chiều cao `32px`, bo góc `4px`.
- **4 trạng thái tương tác hoàn chỉnh:**
  - Default
  - `:hover`
  - `:active` (`transform: translateY(1px)`)
  - `:focus-visible` (ring outline 2px solid `#0F766E` offset 2px kèm glow nhẹ)
  - `:disabled` (`opacity: 0.55; cursor: not-allowed; pointer-events: none;`)
- **Phân cấp ngữ nghĩa chuẩn Tabler:**
  - `.btn-primary`: Hành động chính (Xác nhận đặt lịch, Gọi số tiếp theo, Check-in, Lưu).
  - `.btn-outline-secondary`: Hành động phụ (Gọi lại, Kiểm tra Loa Bluetooth, Xuất CSV, Lọc, Quay lại, Xem phiếu).
  - `.btn-success`: Bắt đầu khám, Hoàn tất khám, Xác nhận phiếu.
  - `.btn-warning`: Qua lượt (Skip), Khôi phục hàng đợi.
  - `.btn-outline-danger` / `.btn-danger`: Hủy lịch hẹn, Xóa phòng/bác sĩ/ca trực.
- **Container `.btn-list`:** Gom tất cả nhóm nút nằm cạnh nhau bằng container `.btn-list` (`flex-wrap: wrap; gap: 8px`), đảm bảo co giãn hoàn hảo và không bao giờ bị dính sát hoặc rớt dòng xấu trên điện thoại `390px`.

### 3. Chuẩn hóa Biểu đồ Chart.js (`Views/Admin/Index.cshtml`)
- Đổi toàn bộ font trục X/Y sang `Be Vietnam Pro` / `JetBrains Mono` màu `#667382`.
- Màu lưới trục X/Y: `#E6E7E9` sắc nét trên nền trắng.
- Cột Lưu lượng theo giờ: Nền `rgba(15, 118, 110, 0.85)` viền `#0F766E` bo góc `4px`.
- Cột Tỷ lệ vắng hẹn theo bác sĩ: Nền `rgba(217, 119, 6, 0.85)` viền `#D97706` bo góc `4px`.
- Tooltip: Nền thẻ tối `#182433` chữ trắng `#FFFFFF` bo góc `6px` viền `#DCE1E7`.

### 4. Chuẩn hóa Bảng, Thẻ & Lưới chọn Khung giờ
- **Bảng (Tables):** Container `.table-wrap` viền bo tròn sắc nét, tiêu đề cột `thead th` nền `#F8FAFC` chữ hoa `#667382`, dòng `tbody tr:hover td` đổi màu sang `#F1F5F9`.
- **Lưới chọn Khung giờ:** Slot còn trống nền trắng `#FFFFFF` viền `#DCE1E7` hover xanh `#0F766E`, slot đang chọn nền `#0F766E` chữ trắng, slot đã đặt/tạm khóa nền mờ dịu mắt không gây chói.
- **Bảo toàn 100% logic:** Giữ nguyên toàn bộ `id`, `name`, `data-*`, routes, hook JS của Loa Bluetooth, AJAX Check-in/Gọi số và QR Code.

---

## Cấu trúc thư mục dự án

```
Phong/
├── ClinicBooking.sln                ✅ Solution chứa cả 2 project
├── HANDOFF_STATUS.md                ✅ File tổng hợp trạng thái dự án (duy nhất ở thư mục gốc)
├── DESIGN_SYSTEM.md                 ✅ Tài liệu hệ thống thiết kế Tabler Light Mode v2.0
├── TEST_REPORT.md                   ✅ Báo cáo chi tiết kết quả kiểm thử 19 Test Cases xUnit
├── KET_QUA_KIEM_THU_CHUC_NANG.md    ✅ Báo cáo kiểm thử chức năng thực tế 28 kịch bản (4 vai trò + RBAC)
├── HUONG_DAN_SU_DUNG_HE_THONG.md    ✅ Cẩm nang hướng dẫn sử dụng toàn diện theo chuẩn Tabler (4 vai trò)
│
├── ClinicBookingMVC/                ✅ Project chính ASP.NET Core MVC (.NET 11)
│   ├── Controllers/                 ✅ 5 Controller (Home, DatLich, LeTan, BacSi, Admin, TaiKhoan)
│   ├── Services/                    ✅ 5 Service nghiệp vụ cốt lõi
│   ├── Models/                      ✅ 10 Entity EF Core 3NF + ViewModels
│   ├── Database/                    ✅ Script khởi tạo + Seed data + Verify_KPI_SQL.sql
│   ├── Views/                       ✅ 25 View Razor chuẩn Tabler Light Mode
│   └── wwwroot/                     ✅ CSS tokens/clinic v2.0, JS Bluetooth speaker
│
└── ClinicBookingMVC.Tests/          ✅ Project kiểm thử tự động xUnit
    ├── Fixtures/                    ✅ TestDbFixture.cs (SQL Server LocalDB / ClinicBookingTestDB)
    └── TestCases/                   ✅ 5 File kiểm thử tự động tương ứng 5 nhóm kịch bản
```
