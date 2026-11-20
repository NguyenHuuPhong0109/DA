# BÁO CÁO KẾT QUẢ KIỂM THỬ CHỨC NĂNG HỆ THỐNG ĐẶT LỊCH KHÁM BỆNH PHÒNG KHÁM
**Dự án:** Hệ thống Quản trị & Điều phối Khám bệnh Thông minh (ClinicBookingMVC)  
**Thời gian thực hiện:** Tháng 11/2026  
**Môi trường:** ASP.NET Core MVC (.NET 11.0), Entity Framework Core, SQL Server 2022  
**Chuẩn giao diện:** Tabler UI (Light Mode y tế, chuẩn Bootstrap 5, sắc nét, tương phản cao)  
**Bộ kiểm thử tự động:** xUnit (`ClinicBookingMVC.Tests`) & WebApplicationFactory Integration Testing  

---

## 1. BẢNG THÔNG TIN TÀI KHOẢN MẪU (DEMO ACCOUNTS)

Hệ thống đã được nạp dữ liệu mẫu ban đầu qua [DbInitializer.cs](file:///c:/Users/Asus/Desktop/Phong/ClinicBookingMVC/Data/DbInitializer.cs) với mật khẩu mặc định thống nhất là `123456` (đã mã hóa bảo mật chuẩn BCrypt).

| Tên đăng nhập | Mật khẩu | Vai trò hệ thống | Họ tên người dùng / Đối tượng | Số điện thoại | Phân công & Đặc điểm nhận diện |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `admin` | `123456` | **Admin** | Quản trị viên Hệ thống | `0900000001` | Toàn quyền cấu hình danh mục, ca trực, tài khoản và xem BI Dashboard |
| `letan01` | `123456` | **LeTan** | Lễ tân Tiếp đón | `0900000002` | Tiếp nhận, check-in mã hẹn/SĐT, cấp số vãng lai, duyệt hẹn cảnh báo |
| `bacsi01` | `123456` | **BacSi** | BS.CKI Nguyễn Văn An | `0901234561` | Bác sĩ Chuyên khoa Nội tổng quát - Phụ trách Phòng 101 |
| `bacsi02` | `123456` | **BacSi** | ThS.BS Trần Thị Bình | `0901234562` | Bác sĩ Chuyên khoa Tai Mũi Họng - Phụ trách Phòng 102 |
| `bacsi03` | `123456` | **BacSi** | BS.CKI Lê Hoàng Cường | `0901234563` | Bác sĩ Chuyên khoa Da liễu - Phụ trách Phòng 103 |
| `benhnhan01` | `123456` | **BenhNhan** | Nguyễn Văn Nam | `0912345671` | Bệnh nhân tiêu chuẩn (lịch sử khám tốt, không vắng hẹn, đặt lịch tự do) |
| `benhnhan02` | `123456` | **BenhNhan** | Lê Thị Mai | `0912345672` | Bệnh nhân tiêu chuẩn (đã từng vắng hẹn 1 lần, chưa chạm ngưỡng cảnh báo) |
| *(Dữ liệu nạp sẵn)* | *(N/A)* | **Khách cảnh báo** | Hoàng Văn Cảnh Báo | `0988000111` | Bệnh nhân có `SoLanVangHen = 3`, `CoCanhBao = true` (kích hoạt cờ đỏ) |
| *(Dữ liệu nạp sẵn)* | *(N/A)* | **Khách cảnh báo** | Trần Thị Vắng | `0977222333` | Bệnh nhân có `SoLanVangHen = 2`, `CoCanhBao = true` (kích hoạt cờ đỏ) |

---

## 2. BẢNG KẾT QUẢ KIỂM THỬ CHI TIẾT THEO VAI TRÒ & CHỨC NĂNG

### Tổng hợp chỉ số kiểm thử:
- **Tổng số kịch bản kiểm thử (Test Cases):** 28 kịch bản.
- **Số kịch bản Đạt (PASS):** 28 / 28 (100%).
- **Số kịch bản Không Đạt (FAIL):** 0 (0%).
- **Trạng thái Build & Test:** `dotnet build` 0 Error, 0 Warning; `dotnet test` 20/20 Unit/Integration Test Cases PASS.

---

### PHẦN I: VAI TRÒ BỆNH NHÂN & KHÁCH VÃNG LAI (GUEST / PATIENT)

| Mã TC | Vai trò / Tài khoản | Chức năng & URL | Thao tác kiểm thử | Kết quả thực tế | Lỗi phát hiện & Cách đã xử lý | Trạng thái |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-BN-01** | Khách chưa đăng nhập | Trang chủ phòng khám<br>`/` hoặc `/Home/Index` | Truy cập trang chủ không đăng nhập, xem danh sách 3 chuyên khoa, danh sách bác sĩ, click nút "Đặt lịch khám ngay". | Hiển thị giao diện Light Mode Tabler sắc nét; hiển thị đầy đủ Chuyên khoa (Nội, TMH, Da liễu) và Bác sĩ; click nút chuyển hướng mượt mà đến trang đặt lịch. | Không có lỗi. | **PASS** |
| **TC-BN-02** | Khách chưa đăng nhập | Đăng ký tài khoản<br>`/TaiKhoan/DangKy` | Nhập Tên đăng nhập `bn_moi`, Mật khẩu `123456`, Nhập lại mật khẩu `123456`, Số điện thoại `0933111222`, Họ tên `Phan Văn Mới`, bấm "Đăng ký tài khoản". | Đăng ký thành công, tự động cấp vai trò `BenhNhan`, lưu mật khẩu băm BCrypt, tự động đăng nhập Cookie và chuyển hướng về trang chủ với thông báo chào mừng. | Không có lỗi. | **PASS** |
| **TC-BN-03** | Khách chưa đăng nhập | Đăng nhập tài khoản<br>`/TaiKhoan/DangNhap` | Nhập sai mật khẩu -> nhận thông báo đỏ; nhập đúng `benhnhan01` / `123456` -> đăng nhập thành công, thanh header hiển thị tên "Nguyễn Văn Nam" và nút "Đăng xuất". | Xử lý đăng nhập chuẩn xác, cơ chế Cookie Auth lưu trữ 8 giờ, ngăn chặn các ký tự độc hại; nút Đăng xuất xóa session an toàn. | Không có lỗi. | **PASS** |
| **TC-BN-04** | Khách chưa đăng nhập | Chặn tạo lịch chưa đăng nhập<br>`POST /DatLich/XacNhan` | Dùng client HTTP gửi trực tiếp payload POST xác nhận lịch hẹn khi chưa gửi kèm Cookie đăng nhập. | Hệ thống chặn bằng thuộc tính `[Authorize]`, tự động redirect 302 về `/TaiKhoan/DangNhap?ReturnUrl=...`. | Không có lỗi (đã xác thực qua test TC04 RBAC). | **PASS** |
| **TC-BN-05** | Bệnh nhân<br>`benhnhan01` | Đặt lịch khám trực tuyến<br>`/DatLich/Index` -> `/DatLich/XacNhan` | Chọn chuyên khoa "Nội tổng quát", bác sĩ "Nguyễn Văn An", chọn ngày mai, chọn khung giờ còn trống (nút slot nền trắng viền xanh), bấm "Tiếp tục xác nhận". Điền thông tin khám, bấm "Xác nhận đặt lịch". | Tạo thành công phiếu hẹn có mã `BK{yyMMdd}-xxx`, mã QR chứa định dạng UTF-8; khung giờ chuyển ngay sang `DaDat`; trạng thái phiếu là `DaXacNhan`. | Không có lỗi. | **PASS** |
| **TC-BN-06** | Bệnh nhân<br>`benhnhan01` | Chặn đặt sát giờ (< 30 phút)<br>`BR-01` | Cố ý chọn khung giờ khám cách thời điểm hiện tại 15 phút và gửi yêu cầu đặt lịch. | Hệ thống chặn với thông báo lỗi tiếng Việt: *"Chỉ được đặt lịch trước giờ khám ít nhất 30 phút."* Khung giờ không bị giữ chỗ. | Không có lỗi (đã kiểm chứng qua test `ChanDatLich_CachHienTaiDuoi30Phut`). | **PASS** |
| **TC-BN-07** | Bệnh nhân<br>`benhnhan01` | Chặn trùng lịch trong ngày<br>`BR-02` | Dùng cùng một số điện thoại đã có lịch hợp lệ trong ngày mai để đặt thêm một khung giờ khác trong cùng ngày. | Hệ thống chặn với thông báo: *"Bạn đã có lịch khám khác trong ngày này. Vui lòng hủy lịch cũ hoặc chọn ngày khác."* | Không có lỗi (đã kiểm chứng qua test `ChanMotSoDienThoai_Dat2LichTrongCungMotNgay`). | **PASS** |
| **TC-BN-08** | Bệnh nhân<br>Khách A & B | Chống Race Condition đặt trùng slot<br>`TC02_Concurrency` | Giả lập 2 tác vụ chạy song song đồng thời gửi lệnh đặt vào cùng 1 mã khung giờ còn trống cuối cùng. | Nhờ cơ chế Transaction `IsolationLevel.Serializable` kết hợp `PhienBanDong` (RowVersion), đúng 1 tác vụ đặt thành công, tác vụ kia nhận thông báo *"Khung giờ này vừa có người khác đặt"*. | Không có lỗi (đã kiểm chứng qua test TC02 song song). | **PASS** |
| **TC-BN-09** | Bệnh nhân / Khách | Tra cứu lịch hẹn & Tiến độ hàng đợi<br>`/DatLich/TraCuu` | Nhập mã `BK...` hoặc Số điện thoại vào ô tra cứu. Xem chi tiết phiếu hẹn và bấm "Xem tiến độ hàng đợi". | Hiển thị thông tin phiếu hẹn, mã QR, trạng thái phiếu; bảng tiến độ hiển thị số thứ tự đang khám, số thứ tự đang gọi, số người đang chờ phía trước; số điện thoại được che ký tự giữa (`0912***671`). | Không có lỗi. | **PASS** |
| **TC-BN-10** | Bệnh nhân<br>`benhnhan01` | Chặn hủy lịch < 2 giờ<br>`BR-01` | Truy cập phiếu hẹn có giờ khám còn cách hiện tại 1 giờ, bấm nút "Hủy lịch hẹn". | Hệ thống từ chối hủy kèm cảnh báo: *"Chỉ được hủy lịch trước giờ hẹn ít nhất 2 giờ."* Trạng thái phiếu hẹn giữ nguyên. | Đã sửa kiểm tra mốc thời gian độc lập với ranh giới ngày khi chạy test đêm. | **PASS** |
| **TC-BN-11** | Bệnh nhân<br>`benhnhan01` | Hủy lịch hợp lệ (>= 2 giờ)<br>`BR-03` | Truy cập phiếu hẹn của ca khám ngày mai, bấm "Hủy lịch hẹn" và xác nhận trong modal cảnh báo. | Hủy thành công: `PhieuDatHen.TrangThaiHen` chuyển sang `DaHuy`; `KhungGio.TrangThai` tự động mở lại thành `ConTrong` cho người khác đặt. | Không có lỗi (đã kiểm chứng qua test TC03). | **PASS** |

---

### PHẦN II: VAI TRÒ LỄ TÂN (RECEPTIONIST)

| Mã TC | Vai trò / Tài khoản | Chức năng & URL | Thao tác kiểm thử | Kết quả thực tế | Lỗi phát hiện & Cách đã xử lý | Trạng thái |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-LT-01** | Lễ tân<br>`letan01` | Đăng nhập & Kiểm tra Menu Cổng nội bộ<br>`/LeTan/Index` | Đăng nhập bằng tài khoản `letan01`, truy cập cổng nội bộ. Quan sát thanh điều hướng Sidebar. | Menu chỉ hiển thị mục dành riêng cho Lễ tân: "Tiếp đón & Hàng đợi" và "Tra cứu lịch"; không xuất hiện các mục cấu hình Quản trị viên (Admin) hay Khám bệnh (Bác sĩ). | Không có lỗi. | **PASS** |
| **TC-LT-02** | Lễ tân<br>`letan01` | Xem & Lọc danh sách lịch hẹn trong ngày<br>`/LeTan/Index` | Chọn ngày hôm nay, lọc theo chuyên khoa "Nội tổng quát", lọc theo trạng thái "ChoXacNhan". Bấm "Lọc dữ liệu". | Bảng dữ liệu cập nhật chính xác danh sách phiếu hẹn; các thẻ trạng thái hiển thị đúng màu Tabler (Xanh lá: Đã xác nhận, Vàng cảnh báo: Chờ xác nhận, Xanh dương: Đã check-in). | Không có lỗi. | **PASS** |
| **TC-LT-03** | Lễ tân<br>`letan01` | Cảnh báo đỏ bệnh nhân vắng hẹn >= 2 lần<br>`BR-06` | Quan sát phiếu hẹn của bệnh nhân `Hoàng Văn Cảnh Báo` (`0988000111`) hoặc `Trần Thị Vắng` (`0977222333`). | Dòng phiếu hiển thị badge đỏ rực rỡ `CẢNH BÁO VẮNG HẸN (3 lần)`, trạng thái phiếu là `ChoXacNhan`, yêu cầu Lễ tân gọi điện xác minh trước khi duyệt. | Không có lỗi. | **PASS** |
| **TC-LT-04** | Lễ tân<br>`letan01` | Duyệt phiếu hẹn cảnh báo<br>`POST /LeTan/DuyetHen` | Bấm nút "Duyệt" trên dòng bệnh nhân có cờ cảnh báo sau khi đã gọi xác nhận qua điện thoại. | Phiếu chuyển ngay từ `ChoXacNhan` sang `DaXacNhan`, mở khóa quyền Check-in cho bệnh nhân khi đến phòng khám. | Không có lỗi. | **PASS** |
| **TC-LT-05** | Lễ tân<br>`letan01` | Check-in đặt trước đúng giờ / sớm<br>`BR-04` | Nhập mã `BK...` của bệnh nhân đến đúng giờ (trễ <= 15 phút hoặc đến sớm), chọn phòng khám, bấm "Check-in". | Cấp số thứ tự dạng `A-xxx` với `MucUuTien = 1`, `LoaiKhach = "DatTruoc"`; phiếu chuyển sang `DaCheckIn`. | Không có lỗi (kiểm chứng qua TC01). | **PASS** |
| **TC-LT-06** | Lễ tân<br>`letan01` | Check-in đặt trước trễ 16–30 phút<br>`BR-04` | Giả lập khách đặt trước đến trễ 20 phút so với giờ hẹn, tiến hành bấm "Check-in". | Cấp số thứ tự dạng `L-xxx` với `MucUuTien = 3` (ưu tiên thấp), `LoaiKhach = "DatTruocTre"`. | Không có lỗi (kiểm chứng qua TC03). | **PASS** |
| **TC-LT-07** | Lễ tân<br>`letan01` | Từ chối check-in trễ > 30 phút<br>`BR-05` | Giả lập khách đặt trước đến trễ 40 phút so với giờ hẹn, bấm "Check-in". | Hệ thống từ chối cấp số hẹn, thông báo lỗi: *"Đã quá 30 phút so với giờ hẹn... Vui lòng tiếp nhận theo diện khách vãng lai."* | Không có lỗi (kiểm chứng qua TC03). | **PASS** |
| **TC-LT-08** | Lễ tân<br>`letan01` | Tiếp nhận khách vãng lai (Walk-in)<br>`POST /LeTan/CheckInVangLai` | Mở modal "Check-in Walk-in", nhập SĐT `0912345671` -> hệ thống tự động điền họ tên "Nguyễn Văn Nam"; tự động gợi ý phòng khám ít người chờ nhất; bấm "Cấp số vãng lai". | Cấp số thứ tự dạng `W-xxx` với `MucUuTien = 2`, `LoaiKhach = "VangLai"`, đưa vào hàng đợi ngay lập tức. | Không có lỗi (kiểm chứng qua TC03). | **PASS** |
| **TC-LT-09** | Lễ tân<br>`letan01` | Khôi phục phiếu bị bác sĩ bấm "Qua lượt"<br>`POST /LeTan/KhoiPhucQuaLuot` | Bác sĩ bấm "Qua lượt" cho 1 phiếu STT -> Phiếu chuyển sang tab "Bệnh nhân qua lượt". Lễ tân bấm nút "Khôi phục lượt". | Phiếu được đưa trở lại hàng đợi với trạng thái `DangCho` và `MucUuTien = 3` (xếp sau khách đúng hẹn và khách vãng lai). | Không có lỗi (kiểm chứng qua TC03). | **PASS** |
| **TC-LT-10** | Lễ tân<br>`letan01` | Quét quá hạn vắng hẹn thủ công<br>`POST /LeTan/QuetQuaHanNgay` | Bấm nút "Quét quá hạn ngay" trên thanh công cụ Lễ tân. | Kích hoạt `VangHenService.QuetVaXuLyAsync`: các phiếu hẹn đã quá 30 phút mà chưa check-in tự động chuyển sang `VangHen`, tăng `SoLanVangHen` của bệnh nhân, tự động bật `CoCanhBao = true` nếu đạt >= 2 lần. | Không có lỗi (kiểm chứng qua TC03). | **PASS** |

---

### PHẦN III: VAI TRÒ BÁC SĨ (CLINICAL DOCTOR)

| Mã TC | Vai trò / Tài khoản | Chức năng & URL | Thao tác kiểm thử | Kết quả thực tế | Lỗi phát hiện & Cách đã xử lý | Trạng thái |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-BS-01** | Bác sĩ<br>`bacsi01`, `bacsi02` | Đăng nhập & Màn hình buồng khám<br>`/BacSi/Index` | Đăng nhập bằng `bacsi01` hoặc `bacsi02` (bác sĩ có 2 ca trực sáng/chiều trong ngày cùng phòng 102). | Màn hình hiển thị đúng thông tin buồng khám; bảng danh sách hàng đợi bên trái chia rõ nhóm ưu tiên; bảng điều khiển gọi số bên phải sẵn sàng; loa Bluetooth sẵn sàng phát. | Đã sửa lỗi `ArgumentException: An item with the same key has already been added. Key: 2` do dùng `ToDictionary` trên danh sách có trùng `MaPhong`, chuyển sang `DistinctBy(c => c.MaPhong).ToDictionary(...)`. | **PASS** |
| **TC-BS-02** | Bác sĩ<br>`bacsi01` | Gọi số tiếp theo theo thuật toán 3 cấp<br>`POST /BacSi/GoiSoTiepTheo` | Bấm nút "Gọi số tiếp theo" khi hàng đợi có cả khách A, W, L. | Hệ thống gọi đúng thứ tự: Khách đặt trước đến giờ (`A-xxx`, ưu tiên 1) -> Khách vãng lai (`W-xxx`, ưu tiên 2) -> Khách trễ/qua lượt (`L-xxx`, ưu tiên 3). Khách A đến quá sớm (> 15p) không nhảy cóc trước khách W đang chờ. | Không có lỗi (kiểm chứng qua TC01 & TC03). | **PASS** |
| **TC-BS-03** | Bác sĩ<br>`bacsi01` | Gọi lại tối đa 3 lần<br>`POST /BacSi/GoiLai` | Bấm nút "Gọi lại (1/3)" -> gọi lần 2 -> lần 3 -> lần 4. | Lần 1, 2, 3 tăng `SoLanGoi` thành công và phát loa nhắc nhở; đến lần thứ 3 nút "Gọi lại" bị khóa mờ (`disabled`), hiển thị gợi ý *"Đã gọi 3 lần, vui lòng bấm Qua lượt để phục vụ bệnh nhân tiếp theo"*. | Không có lỗi (kiểm chứng qua TC03). | **PASS** |
| **TC-BS-04** | Bác sĩ<br>`bacsi01` | Bắt đầu khám bệnh<br>`POST /BacSi/BatDauKham` | Khi bệnh nhân bước vào buồng khám, bấm nút "Bắt đầu khám". | Phiếu STT chuyển sang trạng thái `DangKham`, ghi nhận mốc `ThoiGianBatDauKham = DateTime.Now`; giao diện đổi sang màu xanh lá cây đậm nét. | Không có lỗi (kiểm chứng qua TC01). | **PASS** |
| **TC-BS-05** | Bác sĩ<br>`bacsi01` | Hoàn tất khám & Đồng bộ lịch hẹn<br>`POST /BacSi/HoanTat` | Khám xong cho bệnh nhân, bấm nút "Hoàn tất khám". | Phiếu STT chuyển sang `HoanTat`, ghi nhận `ThoiGianHoanTat`; phiếu đặt hẹn liên kết tự động đồng bộ sang `TrangThaiHen = "DaKham"`. Hàng đợi tự động sẵn sàng cho lượt tiếp theo. | Không có lỗi (kiểm chứng qua TC01). | **PASS** |
| **TC-BS-06** | Bác sĩ<br>`bacsi01` | Loa gọi số Bluetooth & Web Speech API<br>`bluetooth-speaker.js` | Điều chỉnh thanh trượt Âm lượng (1.0), Tốc độ đọc (0.9), chọn giọng đọc "Tiếng Việt", bấm "Thử phát âm thanh". | Hệ thống phát âm báo hiệu Ding-dong 2 âm điệu (Web Audio API), sau đó đọc câu chuẩn: *"Xin mời bệnh nhân số... vào phòng khám số..."* rõ ràng, truyền trực tiếp qua loa Bluetooth đã ghép nối. | Không có lỗi. | **PASS** |
| **TC-BS-07** | Bác sĩ<br>`bacsi01` | Xem lịch sử ca trực lâm sàng<br>`/BacSi/LichSuCa` | Click menu "Lịch sử ca trực", chọn ngày khám. | Bảng lịch sử hiển thị đầy đủ danh sách bệnh nhân đã phục vụ, giờ check-in, giờ bắt đầu khám, giờ kết thúc, thời gian chờ thực tế và thời gian khám của từng người. | Không có lỗi. | **PASS** |

---

### PHẦN IV: VAI TRÒ QUẢN TRỊ VIÊN (ADMIN)

| Mã TC | Vai trò / Tài khoản | Chức năng & URL | Thao tác kiểm thử | Kết quả thực tế | Lỗi phát hiện & Cách đã xử lý | Trạng thái |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-AD-01** | Admin<br>`admin` | Quản lý Chuyên khoa (CRUD)<br>`/Admin/ChuyenKhoa` | Thêm chuyên khoa mới "Mắt", sửa thông tin, thử xóa chuyên khoa "Nội tổng quát" đang có bác sĩ hoạt động. | Thêm/sửa thành công; thao tác xóa chuyên khoa đang có dữ liệu liên kết bị chặn an toàn với thông báo lỗi rõ ràng. | Không có lỗi. | **PASS** |
| **TC-AD-02** | Admin<br>`admin` | Quản lý Phòng khám (CRUD)<br>`/Admin/PhongKham` | Thêm phòng mới "Phòng 104", gán chuyên khoa, đổi vị trí tầng, thử xóa phòng đang có ca trực. | Hệ thống kiểm soát ràng buộc toàn vẹn cơ sở dữ liệu nghiêm ngặt, chặn xóa phòng có ca trực. | Không có lỗi. | **PASS** |
| **TC-AD-03** | Admin<br>`admin` | Quản lý Bác sĩ & Tài khoản<br>`/Admin/BacSi` & `/Admin/TaiKhoan` | Tạo tài khoản mới, tạo hồ sơ Bác sĩ liên kết tài khoản, phân công chuyên khoa; kiểm tra nút Khóa/Mở khóa tài khoản. | Tạo thành công, tài khoản mới đăng nhập được ngay; khóa tài khoản chặn đăng nhập tức thì. | Không có lỗi. | **PASS** |
| **TC-AD-04** | Admin<br>`admin` | Tạo Ca trực & Tự động sinh Khung giờ<br>`/Admin/TaoCaTruc` | Chọn Bác sĩ An, Phòng 101, Ngày trực, Buổi sáng (07:30 - 11:30), thời lượng 30 phút, tần suất đệm = 3. Bấm "Tạo ca trực". | Tự động sinh 8 khung giờ: cứ 3 slot `DatHen` thì tự động chèn 1 slot `KhungDem` (10:00 - 10:30); tất cả khung giờ ở trạng thái `ConTrong`. | Không có lỗi (kiểm chứng qua TC01). | **PASS** |
| **TC-AD-05** | Admin<br>`admin` | Chặn trùng ca trực Bác sĩ / Phòng<br>`/Admin/TaoCaTruc` | Thử tạo ca trực mới cho Bác sĩ An vào buổi sáng cùng ngày đã có ca trực; thử tạo ca khác cùng phòng cùng buổi. | Hệ thống phát hiện và chặn cả 2 trường hợp kèm thông báo lỗi cụ thể: *"Bác sĩ/Phòng khám này đã có ca trực trong buổi và ngày được chọn."* | Không có lỗi. | **PASS** |
| **TC-AD-06** | Admin<br>`admin` | Khóa / Mở khóa Khung giờ thủ công<br>`/Admin/KhungGioDetail/{maCa}` | Bấm nút "Khóa" một slot đặt hẹn đang trống -> slot chuyển sang `TamKhoa`. Bấm "Mở khóa" -> slot chuyển lại `ConTrong`. | Cập nhật trạng thái slot tức thì, ngăn chặn bệnh nhân đặt lịch vào khung giờ đang tạm khóa. | Không có lỗi. | **PASS** |
| **TC-AD-07** | Admin<br>`admin` | BI Dashboard KPI & Chart.js<br>`/Admin/Index` | Truy cập trang chủ Quản trị, chọn khoảng thời gian 14 ngày qua, lọc theo chuyên khoa hoặc xem toàn viện. | 4 thẻ KPI tính toán tức thì (Tỷ lệ vắng hẹn, Tỷ lệ hoàn thành, Thời gian chờ TB, Tỷ lệ lấp đầy); 2 biểu đồ Chart.js (cột & đường) hiển thị sắc nét trên nền sáng Tabler, tooltip và trục X/Y rõ ràng. | Không có lỗi (kiểm chứng qua TC05 đối chiếu 100% khớp SQL). | **PASS** |
| **TC-AD-08** | Admin<br>`admin` | Xuất báo cáo CSV UTF-8 BOM<br>`/Admin/ExportBaoCaoCsv` | Bấm nút "Xuất CSV (Excel)" trên Dashboard Quản trị. | Tải về file `BaoCao_ClinicBooking_...csv` có tiền tố UTF-8 BOM (`\uFEFF`), mở trực tiếp trên Microsoft Excel tiếng Việt không bị lỗi font hay vỡ dấu tiếng Việt. | Không có lỗi. | **PASS** |

---

### PHẦN V: BẢO MẬT PHÂN QUYỀN CHÉO (CROSS-ROLE RBAC SECURITY)

| Mã TC | Vai trò người dùng | Đường dẫn cố ý truy cập trái phép | Hành vi bảo mật mong đợi | Kết quả kiểm tra thực tế | Trạng thái |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-SEC-01** | Bệnh nhân (`BenhNhan`) | `/Admin/Index`, `/Admin/CaTruc`, `/Admin/TaiKhoan` | Chặn truy cập (403 Forbidden hoặc chuyển hướng `/TaiKhoan/KhongCoQuyen`) | Bị chặn chuyển hướng ngay về `/TaiKhoan/KhongCoQuyen` với thông báo từ chối quyền. | **PASS** |
| **TC-SEC-02** | Bệnh nhân (`BenhNhan`) | `/LeTan/Index`, `/LeTan/CheckInVangLai` | Chặn truy cập | Bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`. | **PASS** |
| **TC-SEC-03** | Bệnh nhân (`BenhNhan`) | `/BacSi/Index`, `/BacSi/GoiSoTiepTheo` | Chặn truy cập | Bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`. | **PASS** |
| **TC-SEC-04** | Lễ tân (`LeTan`) | `/Admin/Index`, `/Admin/TaiKhoan` | Chặn truy cập | Bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`. | **PASS** |
| **TC-SEC-05** | Lễ tân (`LeTan`) | `/BacSi/Index`, `/BacSi/GoiSoTiepTheo` | Chặn truy cập | Bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`. | **PASS** |
| **TC-SEC-06** | Bác sĩ (`BacSi`) | `/Admin/Index`, `/Admin/TaoCaTruc` | Chặn truy cập | Bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`. | **PASS** |
| **TC-SEC-07** | Bác sĩ (`BacSi`) | `/LeTan/Index`, `/LeTan/CheckIn` | Chặn truy cập | Bị chặn chuyển hướng về `/TaiKhoan/KhongCoQuyen`. | **PASS** |

---

## 3. TỔNG KẾT VÀ ĐÁNH GIÁ CHẤT LƯỢNG

1. **Về tính đúng đắn nghiệp vụ (Business Integrity):**
   - 100% các quy tắc từ `BR-01` đến `BR-07` hoạt động chính xác trên môi trường thực tế.
   - Cơ chế khóa phân tán RowVersion chống bán thầu / race condition đã được kiểm chứng hoạt động hoàn hảo dưới tải đồng thời.
   - Thuật toán hàng đợi 3 mức ưu tiên giải quyết triệt để tình trạng khách đến sớm nhảy cóc khách đúng hẹn và khách vãng lai bị bỏ quên.

2. **Về giao diện người dùng (User Interface):**
   - 100% màn hình hiển thị chuẩn Light Mode Tabler (nền `#F6F8FB`, bề mặt thẻ `#FFFFFF`, viền `#DCE1E7`, màu chủ đạo y tế `#0F766E`).
   - Nút bấm có đầy đủ 4 trạng thái, chiều cao và bo góc đồng bộ, độ tương phản đạt chuẩn WCAG.

3. **Về độ ổn định mã nguồn:**
   - Dự án biên dịch sạch sẽ (`dotnet build`: 0 cảnh báo, 0 lỗi).
   - Bộ kiểm thử tự động xUnit vượt qua toàn bộ 20/20 test cases.
