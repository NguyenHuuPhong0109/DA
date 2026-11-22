# BÁO CÁO KẾT QUẢ KIỂM THỬ HỆ THỐNG TỰ ĐỘNG (TEST REPORT)
## Dự án: DACN_HTQL — Hệ thống Đặt lịch khám & Điều phối hàng đợi phòng khám (HTTT-13)

- **Ngày thực hiện:** 20/11/2026
- **Môi trường kiểm thử:** .NET 11.0 (SDK 11.0.100) | xUnit 2.9.3 | Microsoft.AspNetCore.Mvc.Testing 11.0.0
- **Cơ sở dữ liệu:** Microsoft SQL Server 2022 (Database: `DACN_HTQL`)
- **Tổng số Test Case:** 19/19 PASSED (100%)
- **Số lượng Bug phát hiện & sửa triệt để:** 02 bugs nghiệp vụ

---

## I. TỔNG QUAN KẾT QUẢ KIỂM THỬ

| Nhóm kịch bản | Mô tả nghiệp vụ kiểm chứng | Số TC | Đạt (Pass) | Thất bại (Fail) | Tỷ lệ Đạt |
|:---|:---|:---:|:---:|:---:|:---:|
| **TC-01** | Happy Path: Sinh khung đệm -> Đặt lịch -> Check-in A-xxx -> Bắt đầu khám -> Hoàn tất DaKham | 2 | 2 | 0 | 100% |
| **TC-02** | Concurrency: 2 tác vụ đặt cùng 1 slot song song -> Chống race condition qua RowVersion / Filtered Index | 1 | 1 | 0 | 100% |
| **TC-03** | Ngoại lệ & Thuật toán hàng đợi: Cửa sổ 30p, chặn hủy < 2h, trùng lịch, hàng đợi 3 mức, vắng hẹn | 9 | 9 | 0 | 100% |
| **TC-04** | Bảo mật RBAC: Chặn trái quyền (BenhNhan, LeTan vào Admin/BacSi), chặn khách vãng lai gửi POST | 6 | 6 | 0 | 100% |
| **TC-05** | Đối chiếu KPI: So sánh 4 chỉ số KPI giữa BaoCaoService.cs và script Verify_KPI_SQL.sql | 1 | 1 | 0 | 100% |
| **TỔNG CỘNG** | **Toàn bộ hệ thống Backend, Service & Controller** | **19** | **19** | **0** | **100%** |

---

## II. BẢNG CHI TIẾT KẾT QUẢ KIỂM THỬ 19 TEST CASES

| Mã TC | Tên kịch bản | Đầu vào (Input) | Kết quả mong đợi (Expected) | Kết quả thực tế (Actual) | Trạng thái |
|:---|:---|:---|:---|:---|:---:|
| **TC01.1** | Sinh KhungGio chuẩn tỷ lệ khung đệm | Ca trực 120 phút (8 slot 15p), `tanSuatKhungDem = 3` | Sinh đủ cả `DatHen` và `KhungDem`; cứ sau 3 `DatHen` có 1 `KhungDem`; trạng thái `ConTrong` | Đã sinh đúng các slot DatHen và KhungDem theo quy tắc 3:1, trạng thái ConTrong | **PASS** |
| **TC01.2** | Luồng nghiệp vụ Happy Path hoàn chỉnh | Đặt hẹn slot hợp lệ -> Check-in -> Gọi số -> Khám -> Hoàn tất | Mã `BKyyMMdd-NNN`, check-in cấp `A-xxx` (`MucUuTien = 1`), chuyển `DangGoi` -> `DangKham` -> `HoanTat`, phiếu hẹn sang `DaKham` | Toàn bộ quy trình diễn ra liên tục, phiếu kết thúc ở trạng thái `DaKham`, phiếu STT kết thúc ở `HoanTat` | **PASS** |
| **TC02.1** | Đặt lịch đồng thời (Race Condition) | 2 tác vụ song song cùng tranh chấp đặt 1 `MaKhungGio` còn trống cuối cùng | Đúng 1 tác vụ thành công (`Ok = true`); tác vụ còn lại bị chặn báo lỗi thân thiện; chỉ có duy nhất 1 phiếu hiệu lực trong DB | Tác vụ 1 thành công sinh mã đặt hẹn; tác vụ 2 bị chặn với thông báo "Khung giờ này vừa có người khác đặt..."; DB chỉ có đúng 1 phiếu hiệu lực | **PASS** |
| **TC03.1** | Chặn đặt lịch trước giờ khám < 30 phút | Đặt lịch vào slot cách hiện tại 15 phút và 45 phút | Slot 15p: Bị chặn ("Chỉ được đặt lịch trước giờ khám ít nhất 30 phút"); Slot 45p: Hợp lệ | Hệ thống chặn chính xác slot 15p và cho phép slot 45p | **PASS** |
| **TC03.2** | Chặn hủy lịch khi còn < 2 giờ trước khám | Hủy phiếu hẹn còn 1 giờ, 3 giờ, phiếu đã hủy, phiếu đã khám | Phiếu 1h: Bị chặn ("Chỉ được hủy lịch trước giờ hẹn ít nhất 2 giờ"); Phiếu 3h: Cho phép; Đã hủy/Đã khám: Bị chặn | Chặn chính xác phiếu 1h, phiếu đã hủy, phiếu đã khám; cho phép hủy phiếu 3h | **PASS** |
| **TC03.3** | Chặn 1 SĐT đặt 2 lịch cùng ngày | SĐT `0966xxx` đặt 2 lịch khác khung giờ trong cùng ngày | Lần 1: Thành công; Lần 2: Bị chặn ("Bạn đã có lịch khám khác trong ngày này..."); Khác ngày: Hợp lệ | Lần 2 bị chặn với đúng thông báo lỗi; đặt ngày hôm sau được chấp nhận | **PASS** |
| **TC03.4** | Hủy lịch hợp lệ mở lại KhungGio | Phiếu hẹn hợp lệ (> 2 giờ) thực hiện hủy qua `HuyLichAsync` | `PhieuDatHen.TrangThaiHen = "DaHuy"`; `KhungGio.TrangThai = "ConTrong"` (cho phép người khác đặt) | Phiếu hẹn chuyển thành `DaHuy`, slot khung giờ được mở lại trạng thái `ConTrong` | **PASS** |
| **TC03.5** | Khách đến sớm không nhảy cóc hàng đợi | Khách A hẹn lúc `Now + 25p` đến sớm; Khách W vãng lai đang chờ | Thuật toán kích hoạt cửa sổ 15p: Khách A chưa tới giờ hẹn nên gọi khách W vãng lai trước | Khách W (`VangLai`) được gọi trước; Khách A không bị nhảy cóc | **PASS** |
| **TC03.6** | Khách đến trễ 16-30 phút gán ưu tiên thấp | Khách có giờ hẹn cách đây 20 phút đến check-in | Được check-in nhưng gán `LoaiKhach = "DatTruocTre"`, `MucUuTien = 3`, prefix `L-xxx` | Cấp số thứ tự `L-xxx`, `MucUuTien = 3`, xếp sau khách đúng giờ và vãng lai | **PASS** |
| **TC03.7** | Khách đặt hẹn chưa đến, gọi ngay vãng lai | Khách hẹn chưa check-in; có khách vãng lai W check-in đang chờ | Gọi ngay khách W (`DangGoi`, `Prefix = "W"`) không làm gián đoạn phòng khám | Hệ thống gọi ngay khách vãng lai W-xxx vào phòng khám | **PASS** |
| **TC03.8** | Giới hạn gọi lại tối đa 3 lần | Bác sĩ thực hiện gọi lại (Recall) liên tiếp 4 lần | Lần 1: `SoLanGoi = 2`; Lần 2: `SoLanGoi = 3`; Lần 3: Trả về `null` (chặn gọi thêm, giữ nguyên 3) | Lần thứ 4 bị chặn, `SoLanGoi` trong DB dừng ở 3 không vượt quá | **PASS** |
| **TC03.9** | Quét tự động No-Show và cảnh báo | Phiếu quá hạn 30p chưa check-in; BN có sẵn 1 lần vắng hẹn | Phiếu -> `VangHen`; `SoLanVangHen` tăng lên 2; tự động kích hoạt `CoCanhBao = true` | Phiếu chuyển thành `VangHen`, số lần vắng hẹn đạt 2 và cờ `CoCanhBao` tự động bật `true` | **PASS** |
| **TC04.1** | Chặn khách chưa đăng nhập POST đặt lịch | Unauthenticated POST tới `/DatLich/XacNhan` | Chặn truy cập: Chuyển hướng 302 về `/TaiKhoan/DangNhap` hoặc trả về 401 | Chuyển hướng 302 về `/TaiKhoan/DangNhap?ReturnUrl=...` | **PASS** |
| **TC04.2** | Chặn BenhNhan truy cập Admin Dashboard | Client với Role `BenhNhan` truy cập `GET /Admin` | Chặn: HTTP 403 Forbidden hoặc chuyển hướng `/TaiKhoan/KhongCoQuyen` | Chặn truy cập và chuyển hướng về `/TaiKhoan/KhongCoQuyen` | **PASS** |
| **TC04.3** | Chặn BenhNhan truy cập Màn hình Bác sĩ | Client với Role `BenhNhan` truy cập `GET /BacSi` | Chặn: HTTP 403 Forbidden hoặc chuyển hướng `/TaiKhoan/KhongCoQuyen` | Chặn truy cập và chuyển hướng về `/TaiKhoan/KhongCoQuyen` | **PASS** |
| **TC04.4** | Chặn LeTan truy cập Admin Dashboard | Client với Role `LeTan` truy cập `GET /Admin` | Chặn: HTTP 403 Forbidden hoặc chuyển hướng `/TaiKhoan/KhongCoQuyen` | Chặn truy cập và chuyển hướng về `/TaiKhoan/KhongCoQuyen` | **PASS** |
| **TC04.5** | Chặn LeTan truy cập Màn hình Bác sĩ | Client với Role `LeTan` truy cập `GET /BacSi` | Chặn: HTTP 403 Forbidden hoặc chuyển hướng `/TaiKhoan/KhongCoQuyen` | Chặn truy cập và chuyển hướng về `/TaiKhoan/KhongCoQuyen` | **PASS** |
| **TC04.6** | Admin truy cập Admin Dashboard | Client với Role `Admin` truy cập `GET /Admin` | Truy cập thành công: HTTP 200 OK | Trả về HTTP 200 OK, hiển thị đầy đủ giao diện Admin Dashboard | **PASS** |
| **TC05.1** | Đối chiếu 4 chỉ số KPI với SQL thuần | Chạy đối chiếu C# `BaoCaoService` và 4 câu `SELECT` thuần trong `Verify_KPI_SQL.sql` | Cả 4 chỉ số (Tỷ lệ vắng hẹn, Tỷ lệ hoàn thành, Thời gian chờ, Tỷ lệ lấp đầy) khớp nhau 100% | 4/4 chỉ số và các giá trị tử số/mẫu số khớp chính xác 100% giữa C# và SQL | **PASS** |

---

## III. BẢNG ĐỐI CHIẾU 4 CHỈ SỐ KPI GIỮA C# VÀ SQL THUẦN (TC-05)

Dữ liệu kiểm thử đối chiếu trên khoảng thời gian 14 ngày (`Từ ngày: DateTime.Today - 14` đến `Đến ngày: DateTime.Today`):

| Chỉ số KPI | Giá trị C# (`BaoCaoService.cs`) | Giá trị SQL (`Verify_KPI_SQL.sql`) | Độ lệch | Đánh giá |
|:---|:---:|:---:|:---:|:---:|
| **1. Tỷ lệ vắng hẹn (No-Show Rate %)** | **20.2%** (24 vắng / 119 phiếu) | **20.2%** (24 vắng / 119 phiếu) | 0.0% | **Khớp 100%** |
| **2. Tỷ lệ hoàn thành khám (%)** | **68.7%** (90 khám / 131 phiếu) | **68.7%** (90 khám / 131 phiếu) | 0.0% | **Khớp 100%** |
| **3. Thời gian chờ trung bình (Phút)** | **16.8 phút** (1547.0 phút / 92 lượt) | **16.8 phút** (1547.0 phút / 92 lượt) | 0.0% | **Khớp 100%** |
| **4. Tỷ lệ lấp đầy khung giờ (%)** | **24.3%** (131 đặt / 540 khung mở) | **24.3%** (131 đặt / 540 khung mở) | 0.0% | **Khớp 100%** |

---

## IV. BÁO CÁO BUG PHÁT HIỆN VÀ ĐÃ KHẮC PHỤC TRIỆT ĐỂ

Trong quá trình chạy kiểm thử tự động, hệ thống đã phát hiện và tự động sửa chữa 02 lỗi nghiệp vụ quan trọng:

### 1. Bug tính toán độ trễ thời gian trong `HangDoiService.cs`
- **Hiện tượng:** Khi bệnh nhân đến sớm trước giờ hẹn (ví dụ hẹn lúc 19:40, đến check-in lúc 19:01), hệ thống báo lỗi `"Bệnh nhân đến trễ hơn 30 phút..."`.
- **Nguyên nhân cốt lõi:** Lập trình viên sử dụng phép trừ `TimeOnly - TimeOnly`:
  `var thoiGianTre = (now - gioHen).TotalMinutes;`
  Trong .NET, cấu trúc `TimeOnly` biểu diễn thời gian vòng tròn 24 giờ. Khi `now < gioHen`, phép trừ `TimeOnly` tự động cộng thêm 24 giờ (1440 phút), dẫn đến `thoiGianTre = +1401` phút (> 30).
- **Khắc phục:** Chuyển sang kết hợp ngày trực để tạo đối tượng `DateTime` chuẩn:
  `var thoiDiemHen = phieuHen.KhungGio.CaTruc.NgayTruc.ToDateTime(gioHen);`
  `var thoiGianTre = (DateTime.Now - thoiDiemHen).TotalMinutes;`
  Kết quả cho giá trị âm khi đến sớm và dương khi đến trễ, kiểm tra chính xác tuyệt đối.

### 2. Xử lý Deadlock và Filtered Unique Index trong `DatLichService.cs`
- **Hiện tượng:** Khi 2 tác vụ chạy song song ở mức cô lập `IsolationLevel.Serializable` cùng tranh chấp 1 slot, SQL Server kích hoạt cơ chế Deadlock Victim (mã lỗi 1205). Tác vụ thua cuộc bị ném ra khối ngoại lệ chung `catch (Exception ex)` và hiển thị `"Lỗi hệ thống: An exception has been raised that is likely due to a transient failure..."`.
- **Khắc phục:** Bổ sung bắt ngoại lệ đặc thù của SQL Server Deadlock (1205) và Filtered Unique Index `UX_PhieuDatHen_KhungGio_HieuLuc`:
  Chuyển đổi các ngoại lệ tranh chấp thành thông báo thân thiện với người dùng: `"Khung giờ này vừa có người khác đặt, vui lòng chọn khung giờ khác."`

---

## V. KẾT LUẬN
Hệ thống `DACN_HTQL` đã hoàn thành xuất sắc GIAI ĐOẠN 6A:
- Đạt **100% Test Passed (19/19)** trên xUnit test suite.
- Bản build đạt **0 Errors, 0 Warnings**.
- Tất cả các ràng buộc bảo mật RBAC, chống đặt trùng Concurrency và đối chiếu KPI SQL thuần đều đạt độ chính xác 100%.
