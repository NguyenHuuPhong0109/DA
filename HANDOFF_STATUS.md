# HANDOFF STATUS — ClinicBookingMVC (HTTT-13)

**Cập nhật lần cuối:** 2026-11-21 · **Hoàn thành Giai đoạn 7 (Hybrid TTS tiếng Việt Loa Bluetooth, Tối ưu Mobile 390px, Deploy HTTPS Cloudflare Tunnel & MonsterASP.NET)**

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
    Duration: 5.0s (ClinicBookingMVC.Tests.dll)

Publish Release: Succeeded (./publish)
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
| 6D | Cơ chế làm mới an toàn (--reseed) + Đổ dữ liệu dày đặc 4 ngày (Today..Today+3) + TEST_DATA_CHEATSHEET.md | ✅ Hoàn thành |
| 7 | Hybrid TTS tiếng Việt chuẩn (Loa Bluetooth) + Tối ưu Mobile 390px + Đóng gói Deploy Cloudflare/MonsterASP.NET | ✅ Hoàn thành |
| 8 | Tinh chỉnh bố cục Dashboard 3 tầng & Chuẩn hóa Hệ thống Danh mục theo chuẩn Tabler và Zocdoc/YouMed | ✅ Hoàn thành |
| 9 | Tối giản Hallmark (AGENTS.md), Tabler & shadcn-ui: Khắc phục lệch bảng, xóa emoji nút, tách quản lý tài khoản 2 nhóm, đổi tên Lịch làm việc | ✅ Hoàn thành |

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

### 4. Chi tiết Giai đoạn 6D đã triển khai (Cơ chế --reseed & Đổ dữ liệu dày đặc 4 ngày)
- **Cơ chế `--reseed` an toàn:**
  - Hỗ trợ tham số dòng lệnh `--reseed` khi chạy `dotnet run --project ClinicBookingMVC -- --reseed`.
  - Xóa dữ liệu theo đúng thứ tự ngược khóa ngoại (`PhieuSoThuTu` -> `PhieuDatHen` -> `KhungGio` -> `CaTruc` -> `BenhNhan` -> `BacSi` -> `PhongKham` -> `ChuyenKhoa` -> `TaiKhoan` -> `VaiTro`) và gọi `DBCC CHECKIDENT` reset về 0, sau đó nạp lại toàn bộ dữ liệu mới và thoát sạch sẽ.
  - Khi chạy thường (không có `--reseed`), kiểm tra bảng `VaiTro` đã có dữ liệu thì bỏ qua, không làm mất dữ liệu test do người dùng tạo.
- **Quy mô dữ liệu nền:**
  - 4 Chuyên khoa: Nội tổng quát, Tai Mũi Họng, Da liễu, Nhi khoa.
  - 4 Phòng khám: Phòng 101, Phòng 102, Phòng 103, Phòng 104.
  - 4 Bác sĩ hoạt động: BS An (P101), BS Bình (P102), BS Cường (P103), BS Đức (P104).
  - 75 Bệnh nhân với SĐT 10 số duy nhất, gồm 10 bệnh nhân có `SoLanVangHen >= 2` (SQL Server tự tính `CoCanhBao = 1`).
- **Mật độ dữ liệu 4 ngày liên tục (`Today` đến `Today + 3`):**
  - Đủ 3 ca mỗi ngày cho tất cả 4 phòng: Sáng (07:30 - 11:30, slot 15p), Chiều (13:30 - 17:30, slot 15p), Tối (18:00 - 20:00, slot 20p) = 48 ca trực dày đặc.
  - Tự động sinh KhungGio theo tỷ lệ chuẩn 3 Đặt hẹn : 1 Khung đệm (1.056 slot).
  - 489 phiếu hẹn: phủ đầy 75%–85% các ca, lấp đầy đủ mọi trạng thái (`DaKham`, `VangHen`, `DaHuy`, `ChoXacNhan`, `DaXacNhan`, `DaCheckIn`).
  - Cố ý tạo sẵn 4 phiếu `DaXacNhan` quá giờ hẹn > 30p chưa check-in để test nút "Quét quá hạn ngay" và `VangHenBackgroundService`.
  - Cố ý tạo sẵn 8 phiếu `ChoXacNhan` gắn bệnh nhân cờ đỏ cảnh báo để test hiển thị badge đỏ và nút Duyệt của Lễ tân.
  - Gán sẵn các lịch hẹn tương lai cho các tài khoản `benhnhan01`, `benhnhan02`, `benhnhan03` để test hủy lịch ($\ge 2$ giờ) và tra cứu.
  - Hàng đợi trực tiếp hôm nay (`PhieuSoThuTu`): Mỗi phòng khám có sẵn 13 phiếu thứ tự hoạt động (1 DangKham, 4 DangCho A, 4 DangCho W, 2 DangCho L, 2 QuaLuot để test khôi phục lượt).
- **Xuất bản `TEST_DATA_CHEATSHEET.md`:** Cẩm nang tra cứu nhanh các mã `MaDatHen` và số điện thoại mẫu để phục vụ kiểm thử tất cả các tính năng của hệ thống.

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

## Chi tiết Giai đoạn 7 đã triển khai (TTS Loa Bluetooth, Tối ưu Mobile 390px & Đóng gói Deploy)

### 1. Kiến trúc Hybrid TTS 2 tầng phát giọng tiếng Việt chuẩn tự nhiên
- **Backend Proxy (`Controllers/TtsController.cs`):**
  - Endpoint `[HttpGet("/api/tts/speak")]` (quyền `BacSi,LeTan,Admin`), validate chuỗi $\le 220$ ký tự, stream trực tiếp từ Google TTS (`client=tw-ob`, `tl=vi`) kèm header `User-Agent` chuẩn và cache 1 giờ.
  - Khắc phục triệt để lỗi không có giọng `vi-VN` cục bộ trên Windows, Edge, Chrome và iOS/Android.
- **Bộ điều khiển âm thanh Loa Bluetooth (`wwwroot/js/bluetooth-speaker.js`):**
  - Tự động chuẩn hóa số thứ tự sang tiếng Việt chữ (ví dụ: `A-001` $\to$ `"A không không một"`, `W-012` $\to$ `"W không một hai"`, `L-005` $\to$ `"L không không năm"`), không bị ngắc ngứ hay đọc dấu gạch ngang.
  - Cơ chế **`unlockAudio()` đồng bộ** trong cử chỉ click của nút "Gọi số tiếp theo", "Gọi lại" và "Kiểm tra Loa" để vượt qua rào cản Autoplay Policy trên iOS Safari và Android Chrome.
  - Chuỗi phát âm thanh: Chuông Ding-Dong Web Audio API $\to$ chờ 600ms $\to$ Tầng 1 (API Proxy Backend) $\to$ Tự động fallback sang Tầng 2 (Web Speech API) nếu mất mạng.
  - Nút "Kiểm tra Loa" phát câu mẫu thực tế: *"Xin mời bệnh nhân số A không không một, Nguyễn Văn An, vào Phòng 101"*.

### 2. Tối ưu giao diện kiểm thử 4 vai trò trên điện thoại di động (Mobile 390px)
- **Drawer Menu trượt (Offcanvas Navigation) cho Cổng nội bộ (`_PortalLayout.cshtml` & `clinic.css`):**
  - Trên màn hình $< 992px$, bổ sung header cố định có nút Hamburger $\to$ chạm vào mở ngăn kéo trượt mượt mà có backdrop mờ.
  - Hiển thị đầy đủ Menu theo phân quyền, avatar người dùng, tên đăng nhập, vai trò và nút **Đăng xuất lớn** (đạt chiều cao tối thiểu 40px) giúp đổi vai trò tức thì trên điện thoại.
- **Khối chuyển nhanh tài khoản trên `Views/TaiKhoan/DangNhap.cshtml`:**
  - Thêm thẻ "Tài khoản kiểm thử nhanh" với 4 nút chạm tự động điền thông tin đăng nhập cho `admin`, `letan01`, `bacsi01`, `benhnhan01` (mật khẩu `123456`), không cần gõ phím trên màn hình cảm ứng nhỏ.
- **Cuộn bảng an toàn trên Mobile:**
  - Toàn bộ bảng dữ liệu trong `LeTan`, `BacSi`, `Admin` được bao bọc bởi `.table-wrap` / `.table-responsive` hỗ trợ `-webkit-overflow-scrolling: touch`, không làm tràn layout hay vỡ khung nhìn di động.

### 3. Đóng gói Deploy & Tên miền HTTPS miễn phí
- **Cấu hình Reverse Proxy trong `Program.cs`:**
  - `ForwardedHeadersOptions` với `XForwardedFor | XForwardedProto`, xóa `KnownIPNetworks` và `KnownProxies`.
  - Cookie Authentication: `SameSite = SameSiteMode.Lax` và `SecurePolicy = CookieSecurePolicy.SameAsRequest`.
  - Cơ chế bỏ qua HttpsRedirection khi chạy sau Tunnel/Proxy để chống lỗi lặp chuyển hướng (`ERR_TOO_MANY_REDIRECTS`).
- **Bộ file triển khai hoàn chỉnh:**
  - `start-demo-tunnel.bat`: 1-click khởi chạy song song máy chủ cục bộ và Cloudflare Tunnel, cấp liên kết HTTPS `https://*.trycloudflare.com` mở trực tiếp trên điện thoại qua mạng 4G/Wi-Fi.
  - `DEPLOY_GUIDE.md`: Cẩm nang hướng dẫn chi tiết Cách 1 (Demo tức thì với Cloudflare) và Cách 2 (Treo online 24/7 trên MonsterASP.NET miễn phí).
  - `ClinicBookingMVC/Database/Deploy_Schema_And_Seed.sql`: Script SQL thuần 1-click (không chứa `CREATE DATABASE`), nạp toàn bộ cấu trúc bảng và 9 tài khoản mẫu trên bất kỳ hosting SQL Server nào.
  - `ClinicBookingMVC/appsettings.Production.template.json`: Mẫu cấu hình môi trường Production.
  - `dotnet publish -c Release -o ./publish`: Xuất bản thành công thư mục deploy hoàn chỉnh (0 warning, 0 error).

---

## Chi tiết Giai đoạn 8 đã triển khai (Tinh chỉnh Dashboard 3 tầng & Hệ thống Danh mục Tabler v2.1)

### 1. Tái cấu trúc Dashboard 3 tầng (`Views/Admin/Index.cshtml`)
- **Tầng 1 (Header 1 dòng & Bộ lọc thu gọn):**
  - Tiêu đề gọn gàng kèm badge kỳ lọc ngày (`dd/MM/yyyy — dd/MM/yyyy`), nút Xuất CSV (`.btn-outline-secondary`), Quét quá hạn và nút đóng/mở bộ lọc trên Mobile.
  - Form bộ lọc bố trí trên **1 hàng ngang duy nhất** (input `36px`, padding `12px 16px`), không chiếm diện tích màn hình.
- **Tầng 2 (4 Thẻ KPI tối giản trên 1 hàng `bi-kpi-row`):**
  - Nền trắng `#FFFFFF`, viền mảnh `1px solid #DCE1E7`, số liệu chính cỡ lớn `28px font-weight: 700`, nhãn phụ ngắn gọn.
  - Đoạn mô tả dài và công thức chuyển vào thuộc tính `title` (tooltip) gắn trên icon thông tin nhỏ ℹ️.
- **Tầng 3 (Bootstrap 5 Tabs chuẩn Tabler):**
  - **Tab "Biểu đồ & Xu hướng":** Biểu đồ lưu lượng theo giờ (`col-lg-8`, chiều cao `300px`) và Biểu đồ Doughnut cơ cấu trạng thái khám (`col-lg-4`, chiều cao `300px`).
  - **Tab "Hiệu suất Bác sĩ & Chuyên khoa":** 5 thẻ mini-bar tài nguyên + Biểu đồ tỷ lệ vắng hẹn theo Bác sĩ + Bảng phân rã chi tiết hiệu suất từng bác sĩ.
  - **Tab "Cảnh báo vắng hẹn":** Gắn badge đỏ cạnh tên Tab khi có cảnh báo (> 0) + Bảng cảnh báo theo quy tắc BR-06.
  - Tự động gọi `chart.resize()` khi chuyển tab Bootstrap (`shown.bs.tab`) tránh lỗi co kích thước bằng 0.

### 2. Phân nhóm Sidebar Navigation (`_PortalLayout.cshtml`)
- Phân tách menu Admin thành 3 nhóm có tiêu đề nhỏ mờ (`11px`, `uppercase`, `color: #667382`):
  - **VẬN HÀNH:** Tổng quan (Dashboard), Ca trực & Khung giờ.
  - **DANH MỤC CƠ SỞ:** Chuyên khoa, Phòng khám, Bác sĩ.
  - **HỆ THỐNG:** Tài khoản.
- Trạng thái Active đồng bộ trên cả Desktop Sidebar và Mobile Offcanvas Drawer (`#F0FDFA` bg, `#0F766E` text, `3px` left highlight bar).

### 3. Chuẩn hóa Hệ thống Danh mục (`ChuyenKhoa`, `PhongKham`, `BacSi`, `CaTruc`, `KhungGioDetail`, `TaiKhoan`)
- Page Header đồng nhất: Tiêu đề + badge đếm tổng dòng + nút chính `+ Thêm ...`.
- Table Toolbar: Tìm kiếm nhanh tức thì bằng client-side JS thuần.
- Bảng dữ liệu sạch: Nền trắng, header `#F8FAFC` chữ in hoa 12px, padding `12px 16px`, viền ngang mảnh, bỏ viền dọc.
- Gộp thông tin phụ xuống dòng dưới (`.sub-info`), cột Thao tác căn phải với cụm nút Tabler `.btn-list justify-content-end flex-nowrap` (`.btn-sm .btn-outline-secondary` Sửa/Xem, `.btn-sm .btn-outline-danger` Xóa/Khóa).
- Badge trạng thái dùng tông pastel dịu mắt (`bg-success-lt`, `bg-secondary-lt`, `bg-danger-lt`, `bg-info-lt`, `bg-warning-lt`).
- Bổ sung thanh lọc nhanh ngày (*Hôm nay | Ngày mai | Ngày kia | +3 ngày*) trong `CaTruc.cshtml` và lọc slot trong `KhungGioDetail.cshtml`.

---

## Chi tiết Giai đoạn 9 đã triển khai (Quy tắc tối giản Hallmark, Tabler & shadcn-ui)

### 1. Sửa triệt để lỗi lệch bố cục bảng & Khử phong cách "AI làm"
- **Căn thẳng hàng dọc 100% giữa `<th>` và `<td>`:**
  - Ghi đè sạch mọi rule `text-align: center` đang áp lên `th`.
  - Toàn bộ cột dữ liệu (Bác sĩ, Phòng khám, Chuyên khoa, Vị trí, Ca trực, Khung giờ, Tài khoản, Số điện thoại, Trạng thái) đều căn trái (`text-align: left !important`) ở cả `<th>` và `<td>`.
  - Riêng cột **Thao tác** căn phải (`text-align: right !important`) ở cả `<th>` và `<td>`.
- **Cột Thao tác nằm gọn trên đúng 1 dòng ngang:**
  - Áp dụng `.actions-wrap` (`display: inline-flex; align-items: center; justify-content: flex-end; gap: 8px; white-space: nowrap;`).
  - Mọi thẻ `<form>` (nút Xóa, Khóa, Mở khóa) đều dùng `display: inline-flex; margin: 0;` triệt tiêu tình trạng rớt xuống dòng thứ 2.
- **Khít bảng vào thẻ (`.card-table`) & Khống chế bề rộng hiển thị:**
  - Giới hạn chiều rộng vùng nội dung chính trong `_PortalLayout.cshtml` (`max-width: 1360px; margin: 0 auto; width: 100%;`).
  - Bọc bảng trực tiếp bằng `.card-table` bỏ padding thừa, header bảng nền `#F9FAFB` chữ in hoa nhỏ `12px` nằm khít mép thẻ, đường kẻ mảnh `1px solid #E5E7EB`, padding cell `12px 16px`, `vertical-align: middle`.
- **Việt hóa dữ liệu hiển thị & Khử lạm dụng Badge màu (Anti-AI-Slop):**
  - Ca làm việc: `Ca sáng`, `Ca chiều`, `Ca tối` (chữ thường, không bọc badge).
  - Trạng thái: `Đang mở`, `Đã đóng`, `Hoạt động`, `Tạm ngưng`, `Đã khóa`.
  - Vai trò: `Quản trị viên`, `Bác sĩ`, `Lễ tân`, `Bệnh nhân` (chữ thường, không bọc pill màu chói gắt).
  - Cột Trạng thái sử dụng chấm tròn trạng thái nhỏ `.status-dot` (`.status-dot--success`, `.status-dot--muted`, `.status-dot--danger`).
  - Loại bỏ các badge font monospace cạnh tiêu đề trang, thay bằng dòng phụ chữ xám nhỏ đặt dưới tiêu đề chính (`.portal-content__sub`).

### 2. Xóa 100% icon và emoji bên trong tất cả nút bấm & ô tìm kiếm
- Xóa sạch icon emoji bên trong mọi `<button>` và `<a class="btn ...">` trên toàn bộ cổng Admin: `Tạo lịch làm việc`, `Thêm chuyên khoa`, `Thêm phòng khám`, `Thêm bác sĩ`, `Tạo tài khoản`, `Xem lịch`, `Khung giờ`, `Sửa`, `Xóa`, `Khóa`, `Mở khóa`, `Quét quá hạn`, `Xuất CSV`, `Bộ lọc`, `Quay lại`.
- Xóa emoji kính lúp `🔍` trong thuộc tính `placeholder` của tất cả ô tìm kiếm.
- Kiểu nút tối giản shadcn: chiều cao `36px` (`30px` cho `.btn-sm`), `font-weight: 500`, bo góc `6px`. Nút phụ viền `#D1D5DB` chữ `#1F2937`; nút nguy hiểm nền trắng viền `#FECACA` chữ `#DC2626` (hover `#FEF2F2`).

### 3. Đổi tên mục "Ca trực & Khung giờ" thành "Lịch làm việc"
- Đổi toàn bộ nhãn hiển thị trong `_PortalLayout.cshtml` (Sidebar Desktop và Mobile Drawer), `<title>` và tiêu đề `<h1>` của `CaTruc.cshtml`, `TaoCaTruc.cshtml`, `KhungGioDetail.cshtml` sang **`Lịch làm việc`** (và **`Quản lý Lịch làm việc`**).
- Giữ nguyên tên Controller action `/Admin/CaTruc`, `/Admin/TaoCaTruc`, `/Admin/KhungGioDetail`.

### 4. Tách mục "Tài khoản" thành "Quản lý tài khoản": Bệnh nhân và Bác sĩ, Lễ tân
- Sidebar `_PortalLayout.cshtml` phân tách thành 2 mục con:
  - **`Bác sĩ & Lễ tân`** (`/Admin/TaiKhoan?nhom=NhanSu`)
  - **`Bệnh nhân`** (`/Admin/TaiKhoan?nhom=BenhNhan`)
- `AdminController.cs`: Action `TaiKhoan(string? nhom = null)` và `TaoTaiKhoan(string? nhom = null)` hỗ trợ lọc theo nhóm, tính số lượng cho từng nhóm qua ViewBag, tra cứu Họ tên bệnh nhân theo SĐT, tự động chọn vai trò tương ứng khi tạo mới tài khoản.
- `TaiKhoan.cshtml`: Tiêu đề linh hoạt theo nhóm, thanh tab tối giản chuyển đổi giữa 2 nhóm tài khoản (kèm số lượng dạng chữ thường), bộ lọc nhanh theo vai trò nhân sự.

---

## Cấu trúc thư mục dự án

```
Phong/
├── ClinicBooking.sln                ✅ Solution chứa cả 2 project
├── HANDOFF_STATUS.md                ✅ File tổng hợp trạng thái dự án (duy nhất ở thư mục gốc)
├── DESIGN_SYSTEM.md                 ✅ Tài liệu hệ thống thiết kế Tabler Light Mode v2.0
├── TEST_REPORT.md                   ✅ Báo cáo chi tiết kết quả kiểm thử 20 Test Cases xUnit
├── KET_QUA_KIEM_THU_CHUC_NANG.md    ✅ Báo cáo kiểm thử chức năng thực tế 28 kịch bản (4 vai trò + RBAC)
├── HUONG_DAN_SU_DUNG_HE_THONG.md    ✅ Cẩm nang hướng dẫn sử dụng toàn diện theo chuẩn Tabler (4 vai trò)
├── TEST_DATA_CHEATSHEET.md          ✅ Bảng tra cứu dữ liệu kiểm thử nhanh (75 BN, 48 Ca, 489 Phiếu)
├── DEPLOY_GUIDE.md                  ✅ Hướng dẫn triển khai HTTPS Cloudflare Tunnel & MonsterASP.NET
├── start-demo-tunnel.bat            ✅ Script 1-click mở tên miền HTTPS miễn phí cho điện thoại & hội đồng
│
├── ClinicBookingMVC/                ✅ Project chính ASP.NET Core MVC (.NET 11)
│   ├── Controllers/                 ✅ 6 Controller (Home, DatLich, LeTan, BacSi, Admin, TaiKhoan, Tts)
│   ├── Services/                    ✅ 5 Service nghiệp vụ cốt lõi + 1 BackgroundService
│   ├── Models/                      ✅ 10 Entity EF Core 3NF + ViewModels
│   ├── Database/                    ✅ Deploy_Schema_And_Seed.sql, ClinicBookingDB.sql, Verify_KPI_SQL.sql
│   ├── Views/                       ✅ 25 View Razor chuẩn Tabler Light Mode & Mobile Drawer
│   ├── wwwroot/                     ✅ CSS tokens/clinic v2.0, JS Bluetooth speaker Hybrid TTS
│   └── appsettings.Production.template.json ✅ Mẫu cấu hình Production
│
├── ClinicBookingMVC.Tests/          ✅ Project kiểm thử tự động xUnit (20/20 Test Cases Pass)
│   ├── Fixtures/                    ✅ TestDbFixture.cs (SQL Server LocalDB / ClinicBookingTestDB)
│   └── TestCases/                   ✅ 5 File kiểm thử tự động tương ứng 5 nhóm kịch bản
│
└── publish/                         ✅ Thư mục đóng gói Release sẵn sàng deploy hosting/IIS
```
