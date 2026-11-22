# HƯỚNG DẪN SỬ DỤNG HỆ THỐNG ĐẶT LỊCH KHÁM BỆNH PHÒNG KHÁM
**Dự án:** Hệ thống Quản trị & Điều phối Khám bệnh Thông minh (DACN_HTQL)  
**Tiêu chuẩn giao diện:** Tabler UI Design System (Chế độ Sáng - Light Mode Y tế Chuyên nghiệp)  
**Nền tảng kỹ thuật:** ASP.NET Core MVC (.NET 11.0), Entity Framework Core, SQL Server 2022  
**Đối tượng áp dụng:** Bệnh nhân, Nhân viên Lễ tân, Bác sĩ phòng khám, Quản trị viên hệ thống  

---

## MỤC LỤC TỔNG QUAN

1. [DANH SÁCH TÀI KHOẢN MẪU ĐỂ TRẢI NGHIỆM HỆ THỐNG](#1-danh-sach-tai-khoan-mau)
2. [PHẦN I: VAI TRÒ BỆNH NHÂN (GUEST / PATIENT)](#phan-i-vai-tro-benh-nhan)
   - [1.1. Xem thông tin trang chủ, chuyên khoa và bác sĩ](#11-xem-thong-tin-trang-chu-chuyen-khoa-va-bac-si)
   - [1.2. Đăng ký tài khoản bệnh nhân mới](#12-dang-ky-tai-khoan-benh-nhan-moi)
   - [1.3. Đăng nhập và đăng xuất](#13-dang-nhap-va-dang-xuat)
   - [1.4. Đặt lịch khám bệnh trực tuyến](#14-dat-lich-kham-benh-truc-tuyen)
   - [1.5. Tra cứu phiếu hẹn và xem tiến độ hàng đợi trực tiếp](#15-tra-cuu-phieu-hen-va-xem-tien-do-hang-doi-truc-tiep)
   - [1.6. Hủy lịch hẹn khám bệnh trực tuyến](#16-huy-lich-hen-kham-benh-truc-tuyen)
3. [PHẦN II: VAI TRÒ LỄ TÂN (RECEPTIONIST)](#phan-ii-vai-tro-le-tan)
   - [2.1. Đăng nhập cổng nội bộ và làm quen giao diện Lễ tân](#21-dang-nhap-cong-noi-bo-va-lam-quen-giao-dien-le-tan)
   - [2.2. Xem và lọc danh sách lịch hẹn trong ngày](#22-xem-va-loc-danh-sach-lich-hen-trong-ngay)
   - [2.3. Nhận diện cảnh báo đỏ và duyệt phiếu hẹn bệnh nhân vắng hẹn](#23-nhan-dien-canh-bao-do-va-duyet-phieu-hen-benh-nhan-vang-hen)
   - [2.4. Check-in cho bệnh nhân đặt trước (A-xxx / L-xxx)](#24-check-in-cho-benh-nhan-dat-truoc)
   - [2.5. Tiếp nhận và cấp số cho khách vãng lai (W-xxx)](#25-tiep-nhan-va-cap-so-cho-khach-vang-lai)
   - [2.6. Khôi phục lượt khám cho bệnh nhân bị qua lượt](#26-khoi-phuc-luot-kham-cho-benh-nhan-bi-qua-luot)
   - [2.7. Quét quá hạn vắng hẹn thủ công](#27-quet-qua-han-vang-hen-thu-cong)
4. [PHẦN III: VAI TRÒ BÁC SĨ (CLINICAL DOCTOR)](#phan-iii-vai-tro-bac-si)
   - [3.1. Đăng nhập buồng khám và quản lý phiên làm việc](#31-dang-nhap-buong-kham-va-quan-ly-phien-lam-viec)
   - [3.2. Gọi số thứ tự tiếp theo (Thuật toán 3 cấp ưu tiên)](#32-goi-so-thu-tu-tiep-theo)
   - [3.3. Gọi lại và xử lý qua lượt bệnh nhân](#33-goi-lai-va-xu-ly-qua-luot-benh-nhan)
   - [3.4. Bắt đầu khám và Hoàn tất ca khám (Đồng bộ DaKham)](#34-bat-dau-kham-va-hoan-tat-ca-kham)
   - [3.5. Cấu hình và sử dụng Loa gọi số Bluetooth (Web Speech / Web Audio)](#35-cau-hinh-va-su-dung-loa-goi-so-bluetooth)
   - [3.6. Xem lịch sử phục vụ trong ca trực lâm sàng](#36-xem-lich-su-phuc-vu-trong-ca-truc-lam-sang)
5. [PHẦN IV: VAI TRÒ QUẢN TRỊ VIÊN (ADMINISTRATOR)](#phan-iv-vai-tro-quan-tri-vien)
   - [4.1. Quản lý danh mục Chuyên khoa (CRUD)](#41-quan-ly-danh-muc-chuyen-khoa)
   - [4.2. Quản lý danh mục Phòng khám (CRUD)](#42-quan-ly-danh-muc-phong-kham)
   - [4.3. Quản lý danh mục Bác sĩ và Quản lý tài khoản (Bệnh nhân, Bác sĩ & Lễ tân)](#43-quan-ly-danh-muc-bac-si-va-quan-ly-tai-khoan)
   - [4.4. Quản lý Lịch làm việc và Sinh khung giờ tự động (Tỷ lệ 3:1)](#44-quan-ly-lich-lam-viec-va-sinh-khung-gio-tu-dong)
   - [4.5. Khóa và mở khóa khung giờ thủ công](#45-khoa-va-mo-khoa-khung-gio-thu-cong)
   - [4.6. Khai thác BI Dashboard KPI và Biểu đồ Chart.js](#46-khai-thac-bi-dashboard-kpi-va-bieu-do-chartjs)
   - [4.7. Xuất báo cáo dữ liệu định dạng CSV (Chuẩn Excel tiếng Việt)](#47-xuat-bao-cao-du-lieu-dinh-dang-csv)
6. [TỔNG HỢP QUY TẮC NGHIỆP VỤ CỐT LÕI (BR-01 ĐẾN BR-07)](#tong-hop-quy-tac-nghiep-vu-cot-loi)

---

## 1. DANH SÁCH TÀI KHOẢN MẪU

Dữ liệu ban đầu đã được thiết lập sẵn trong hệ thống qua [DbInitializer.cs](file:///c:/Users/Asus/Desktop/Phong/DACN_HTQL/Data/DbInitializer.cs). Mật khẩu mặc định của mọi tài khoản demo là: `123456`.

| Tên đăng nhập | Mật khẩu | Quyền hạn (Role) | Họ và tên | Chức vụ / Phân công | Số điện thoại |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `admin` | `123456` | **Admin** | Quản trị viên Hệ thống | Phụ trách toàn hệ thống | `0900000001` |
| `letan01` | `123456` | **LeTan** | Nhân viên Lễ tân | Quầy tiếp đón & Hàng đợi trung tâm | `0900000002` |
| `bacsi01` | `123456` | **BacSi** | BS.CKI Nguyễn Văn An | Khám Nội tổng quát - Phòng 101 | `0901234561` |
| `bacsi02` | `123456` | **BacSi** | ThS.BS Trần Thị Bình | Khám Tai Mũi Họng - Phòng 102 | `0901234562` |
| `bacsi03` | `123456` | **BacSi** | BS.CKI Lê Hoàng Cường | Khám Da liễu - Phòng 103 | `0901234563` |
| `benhnhan01` | `123456` | **BenhNhan** | Nguyễn Văn Nam | Bệnh nhân tiêu chuẩn (lịch sử tốt) | `0912345671` |
| `benhnhan02` | `123456` | **BenhNhan** | Lê Thị Mai | Bệnh nhân vắng 1 lần (bình thường) | `0912345672` |

---

## PHẦN I: VAI TRÒ BỆNH NHÂN (GUEST / PATIENT)

### 1.1. Xem thông tin trang chủ, chuyên khoa và bác sĩ
1. **Chức năng này để làm gì?**  
   Cung cấp cho người dân và người bệnh cái nhìn tổng quan về phòng khám, danh sách các chuyên khoa đang hoạt động, đội ngũ bác sĩ uy tín và liên kết nhanh đến chức năng đặt hẹn.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Mọi người dùng (Không yêu cầu đăng nhập).
   - Đường dẫn: `/` hoặc `/Home/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Mở trình duyệt và truy cập trang chủ phòng khám.
   - Bước 2: Quan sát danh sách các chuyên khoa (Nội tổng quát, Tai Mũi Họng, Da liễu) và thông tin các bác sĩ.
   - Bước 3: Bấm nút **"Đặt lịch khám ngay"** trên banner chính hoặc nút **"Đặt khám"** tại thẻ của chuyên khoa mong muốn.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Giao diện chuẩn Tabler Light Mode nền xám sáng nhạt `#F6F8FB`, bề mặt thẻ trắng `#FFFFFF`. Người dùng có thể tra cứu thông tin mà không cần để lại thông tin cá nhân.

---

### 1.2. Đăng ký tài khoản bệnh nhân mới
1. **Chức năng này để làm gì?**  
   Cho phép bệnh nhân mới tự tạo tài khoản để đặt lịch, lưu trữ lịch sử phiếu hẹn và theo dõi tiến độ khám bệnh.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Người dùng chưa có tài khoản.
   - Đường dẫn: `/TaiKhoan/DangKy`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Bấm nút **"Đăng ký"** ở góc phải trên cùng của thanh Menu.
   - Bước 2: Nhập đầy đủ các thông tin bắt buộc:
     * *Tên đăng nhập*: Nhập tên viết liền không dấu (VD: `nguyenvana`).
     * *Họ và tên*: Nhập họ tên đầy đủ (VD: `Nguyễn Văn A`).
     * *Số điện thoại*: Nhập 10 chữ số hợp lệ (VD: `0934567890`).
     * *Mật khẩu*: Nhập mật khẩu (tối thiểu 6 ký tự).
     * *Xác nhận mật khẩu*: Nhập lại đúng mật khẩu vừa gõ.
   - Bước 3: Bấm nút **"Đăng ký tài khoản"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Hệ thống tự động băm mật khẩu bằng thuật toán an toàn BCrypt.
   - Tự động gán quyền `BenhNhan`.
   - Đăng ký thành công sẽ tự động đăng nhập người dùng và chuyển về trang chủ kèm thông báo thành công.

---

### 1.3. Đăng nhập và đăng xuất
1. **Chức năng này để làm gì?**  
   Xác thực danh tính người dùng trước khi thực hiện các tác vụ cần định danh như đặt lịch hoặc hủy lịch.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn đăng nhập: `/TaiKhoan/DangNhap`.
   - Đường dẫn đăng xuất: `/TaiKhoan/DangXuat`.
3. **Hướng dẫn thao tác từng bước:**  
   - **Đăng nhập:**
     * Bước 1: Bấm nút **"Đăng nhập"** trên góc phải thanh điều hướng.
     * Bước 2: Nhập *Tên đăng nhập* và *Mật khẩu*.
     * Bước 3: Bấm nút **"Đăng nhập hệ thống"**.
   - **Đăng xuất:**
     * Bấm vào tên người dùng góc phải Menu -> Chọn **"Đăng xuất"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Phiên đăng nhập được duy trì bằng Cookie an toàn trong vòng 8 giờ.
   - Nếu đăng nhập với tài khoản Lễ tân, Bác sĩ hoặc Admin, hệ thống sẽ tự động hiển thị nút truy cập nhanh vào **"Cổng nội bộ"** (`/LeTan`, `/BacSi`, `/Admin`).

---

### 1.4. Đặt lịch khám bệnh trực tuyến
1. **Chức năng này để làm gì?**  
   Cho phép bệnh nhân chủ động lựa chọn Chuyên khoa, Bác sĩ, Ngày khám và Khung giờ còn trống theo nguyện vọng cá nhân.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Bệnh nhân đã đăng nhập (Nếu chưa đăng nhập sẽ được tự động chuyển hướng đến trang đăng nhập kèm tham số quay lại sau khi đăng nhập xong).
   - Đường dẫn: `/DatLich/Index` -> `/DatLich/XacNhan`.
3. **Hướng dẫn thao tác từng bước:**  
   - **Bước 1: Chọn tiêu chí khám tại `/DatLich/Index`:**
     * Tại mục *Chọn chuyên khoa*: Chọn chuyên khoa phù hợp (VD: "Nội tổng quát").
     * Tại mục *Chọn bác sĩ*: Chọn bác sĩ cụ thể hoặc để "Tất cả bác sĩ".
     * Tại mục *Chọn ngày khám*: Chọn ngày bạn muốn đi khám (hôm nay hoặc các ngày tới).
   - **Bước 2: Chọn khung giờ khám:**
     * Quan sát lưới các khung giờ hiển thị. Khung giờ màu trắng viền xanh có chữ **"Còn trống"** là khung giờ hợp lệ.
     * Bấm vào khung giờ mong muốn -> Bấm nút **"Tiếp tục xác nhận"**.
   - **Bước 3: Điền thông tin tại trang `/DatLich/XacNhan`:**
     * *Họ và tên bệnh nhân*: Hệ thống tự động điền theo tài khoản hoặc có thể chỉnh sửa nếu đặt hộ người thân.
     * *Số điện thoại*: Nhập số điện thoại liên hệ nhận thông báo.
     * *Năm sinh & Giới tính*: Chọn năm sinh và giới tính người khám.
     * *Lý do khám sơ bộ*: Nhập triệu chứng tóm tắt (VD: "Đau rát họng 3 ngày nay").
     * *Nhận nhắc lịch tự động*: Tích chọn ô vuông để nhận thông báo.
   - **Bước 4: Bấm nút "Xác nhận đặt lịch"** (Nút màu xanh Primary).
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-01, BR-02, BR-06):**  
   - **BR-01:** Khung giờ chọn phải cách thời điểm hiện tại tối thiểu 30 phút. Nếu đặt sát giờ (< 30 phút), hệ thống sẽ từ chối.
   - **BR-02:** Mỗi số điện thoại chỉ được đặt tối đa 01 lịch khám trong cùng 01 ngày. Nếu đặt lịch thứ 2 trong cùng ngày, hệ thống sẽ báo trùng.
   - **BR-06:** Đối với bệnh nhân có tiền sử vắng hẹn >= 2 lần (`CoCanhBao = true`), phiếu đặt hẹn sẽ ở trạng thái `ChoXacNhan` (Lễ tân cần liên hệ duyệt). Bệnh nhân bình thường phiếu sẽ ở trạng thái `DaXacNhan` ngay lập tức.
   - Khi hoàn tất, hệ thống chuyển sang trang **Phiếu hẹn điện tử** hiển thị Mã đặt hẹn (VD: `BK261120-101`) kèm Mã QR Code tiện lợi.

---

### 1.5. Tra cứu phiếu hẹn và xem tiến độ hàng đợi trực tiếp
1. **Chức năng này để làm gì?**  
   Giúp bệnh nhân kiểm tra lại chi tiết phiếu hẹn của mình và xem phòng khám đang phục vụ đến số mấy để chủ động thời gian có mặt.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Mọi bệnh nhân.
   - Đường dẫn: `/DatLich/TraCuu`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Truy cập Menu **"Tra cứu lịch hẹn"**.
   - Bước 2: Nhập *Mã đặt hẹn* (VD: `BK261120-101`) hoặc *Số điện thoại* đã dùng đặt lịch vào ô tìm kiếm.
   - Bước 3: Bấm nút **"Tra cứu lịch hẹn"**.
   - Bước 4: Xem kết quả phiếu hẹn. Bấm nút **"Xem tiến độ hàng đợi"** để mở modal hiển thị hàng chờ thực tế.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Hệ thống hiển thị: Số thứ tự đang được gọi, số thứ tự đang khám, và **số người đang xếp hàng phía trước bạn**.
   - Để bảo mật quyền riêng tư y tế, số điện thoại hiển thị công khai trên bảng theo dõi luôn được ẩn 3 số ở giữa (VD: `0912***671`).

---

### 1.6. Hủy lịch hẹn khám bệnh trực tuyến
1. **Chức năng này để làm gì?**  
   Cho phép bệnh nhân hủy lịch khám khi có việc đột xuất, giải phóng khung giờ cho bệnh nhân khác có nhu cầu.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Bệnh nhân là chủ sở hữu phiếu hẹn (Đã đăng nhập).
   - Đường dẫn: Màn hình chi tiết phiếu hẹn `/DatLich/PhieuHen/{id}`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Mở màn hình chi tiết phiếu hẹn của bạn.
   - Bước 2: Kéo xuống cuối trang, bấm nút màu đỏ viền ngoài: **"Hủy lịch hẹn"**.
   - Bước 3: Đọc kỹ hộp thoại xác nhận -> Bấm **"Xác nhận hủy lịch"**.
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-01, BR-03):**  
   - **BR-01:** Bệnh nhân chỉ được phép tự hủy lịch khi **còn ít nhất 2 giờ trước giờ hẹn**.
   - Nếu còn dưới 2 giờ, nút hủy sẽ bị chặn kèm thông báo: *"Chỉ được hủy lịch trước giờ hẹn ít nhất 2 giờ. Vui lòng liên hệ hotline Lễ tân để được hỗ trợ"*.
   - **BR-03:** Khi hủy thành công, trạng thái phiếu chuyển sang `DaHuy`, và khung giờ tương ứng ngay lập tức tự động mở lại trạng thái `ConTrong` trên hệ thống.

---

## PHẦN II: VAI TRÒ LỄ TÂN (RECEPTIONIST)

### 2.1. Đăng nhập cổng nội bộ và làm quen giao diện Lễ tân
1. **Chức năng này để làm gì?**  
   Khu vực làm việc chuyên dụng cho nhân viên quầy tiếp đón: check-in tiếp đón, cấp số vãng lai, điều phối hàng đợi và giám sát tỷ lệ vắng hẹn.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Tài khoản có vai trò `LeTan` (hoặc `Admin`).
   - Tài khoản mẫu: `letan01` / Mật khẩu: `123456`.
   - Đường dẫn: `/LeTan` hoặc `/LeTan/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Đăng nhập bằng tài khoản `letan01`.
   - Bước 2: Bấm vào mục **"Cổng nội bộ"** trên thanh Menu -> Chọn **"Bàn làm việc Lễ tân"**.
   - Bước 3: Quan sát bố cục 2 khu vực:
     * *Cột trái*: Bộ công cụ Check-in (Mã đặt hẹn / SĐT / Nút Check-in Walk-in).
     * *Cột phải*: Bảng danh sách phiếu hẹn trong ngày và bộ lọc trạng thái.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Thanh Menu được thu gọn chỉ hiển thị các mục Lễ tân có thẩm quyền; hoàn toàn không thể truy cập các mục Quản trị cấu hình hay Trang khám lâm sàng của Bác sĩ.

---

### 2.2. Xem và lọc danh sách lịch hẹn trong ngày
1. **Chức năng này để làm gì?**  
   Giúp Lễ tân theo dõi toàn diện lượng khách đến khám trong ca, lọc nhanh theo chuyên khoa hoặc trạng thái phiếu.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/LeTan/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Tại khu vực bộ lọc trên cùng:
     * Ô *Ngày*: Mặc định là ngày hôm nay (có thể chọn ngày khác để xem trước lịch).
     * Ô *Chuyên khoa*: Chọn chuyên khoa cần lọc (hoặc "Tất cả chuyên khoa").
     * Ô *Trạng thái*: Chọn "ChoXacNhan", "DaXacNhan", "DaCheckIn", "DaKham", "VangHen", "DaHuy".
   - Bước 2: Bấm nút **"Lọc dữ liệu"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Bảng kết quả cập nhật ngay lập tức: hiển thị rõ Mã đặt hẹn, Giờ hẹn, Tên bệnh nhân, Số điện thoại, Bác sĩ phụ trách, Phòng khám và Trạng thái hiện tại.

---

### 2.3. Nhận diện cảnh báo đỏ và duyệt phiếu hẹn bệnh nhân vắng hẹn
1. **Chức năng này để làm gì?**  
   Thực thi quy tắc kiểm soát người bệnh có lịch sử vắng hẹn nhiều lần (`BR-06`), ngăn ngừa tình trạng chiếm dụng khung giờ ảo.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/LeTan/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Quan sát bảng lịch hẹn, tìm các dòng có nhãn cảnh báo đỏ:  
     `<span class="badge bg-danger">CẢNH BÁO VẮNG HẸN (X lần)</span>`.
   - Bước 2: Trạng thái của phiếu này sẽ là `ChoXacNhan`.
   - Bước 3: Lễ tân gọi điện thoại cho bệnh nhân theo số hiển thị để xác nhận chắc chắn bệnh nhân sẽ đến.
   - Bước 4: Sau khi bệnh nhân xác nhận sẽ đến, bấm nút màu xanh lá: **"Duyệt"**.
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-06):**  
   - Phiếu hẹn được chuyển từ `ChoXacNhan` sang `DaXacNhan`.
   - Bệnh nhân chỉ có thể thực hiện Check-in sau khi phiếu đã được Lễ tân duyệt sang `DaXacNhan`.

---

### 2.4. Check-in cho bệnh nhân đặt trước (A-xxx / L-xxx)
1. **Chức năng này để làm gì?**  
   Tiếp đón bệnh nhân đã có phiếu hẹn đến phòng khám, xác thực giờ đến và cấp số thứ tự điện tử vào hàng đợi của buồng khám.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/LeTan/Index` (Khu vực "Tiếp đón bệnh nhân hẹn trước").
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Yêu cầu bệnh nhân cung cấp *Mã đặt hẹn* (hoặc quét mã QR trên điện thoại) hoặc *Số điện thoại*.
   - Bước 2: Nhập thông tin vào ô nhập liệu: **"Nhập mã đặt hẹn (BK...) hoặc SĐT"**.
   - Bước 3: Bấm nút **"Check-in"** (Nút màu xanh Primary).
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-04, BR-05):**  
   - **Trường hợp 1 - Đúng giờ hoặc Đến sớm (Trễ <= 15 phút):**  
     * Hệ thống cấp số thứ tự có tiền tố **`A`** (VD: `A-001`, `A-002`).
     * Mức ưu tiên được gán: **`MucUuTien = 1`** (Ưu tiên cao nhất).
   - **Trường hợp 2 - Đến trễ từ 16 đến 30 phút:**  
     * Hệ thống cấp số thứ tự có tiền tố **`L`** (Late - VD: `L-001`).
     * Mức ưu tiên bị hạ xuống: **`MucUuTien = 3`** (Xếp sau khách vãng lai).
   - **Trường hợp 3 - Đến trễ quá 30 phút so với giờ hẹn (`BR-05`):**  
     * Hệ thống từ chối check-in với thông báo lỗi màu đỏ:  
       *"Bệnh nhân đến trễ hơn 30 phút (giờ hẹn HH:mm). Phiếu đã quá hạn vắng hẹn. Vui lòng tiếp nhận theo diện khách vãng lai."*
   - Phiếu đặt hẹn được đồng bộ chuyển trạng thái sang `DaCheckIn`.

---

### 2.5. Tiếp nhận và cấp số cho khách vãng lai (W-xxx)
1. **Chức năng này để làm gì?**  
   Tiếp nhận bệnh nhân không đặt hẹn trước (Walk-in), tự động gợi ý buồng khám ít người chờ nhất để cân bằng tải buồng khám.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/LeTan/Index` -> Nút **"Tiếp nhận Walk-in"**.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Bấm nút **"Tiếp nhận Walk-in"** để mở cửa sổ tiếp nhận.
   - Bước 2: Nhập *Số điện thoại* của khách:
     * Nếu là khách quen đã từng khám: Hệ thống **tự động điền (Auto-fill)** Họ tên, Năm sinh, Giới tính, Email.
     * Nếu là khách mới: Lễ tân điền đầy đủ Họ tên, Năm sinh, Giới tính.
   - Bước 3: Tại mục *Chọn phòng khám*: Hệ thống hiển thị số lượng người đang chờ ở từng phòng và **tự động gợi ý phòng có hàng chờ ngắn nhất**. Lễ tân chọn phòng phù hợp.
   - Bước 4: Bấm nút **"Cấp số vãng lai"**.
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-04):**  
   - Cấp số thứ tự có tiền tố **`W`** (Walk-in - VD: `W-001`, `W-002`).
   - Mức ưu tiên được gán: **`MucUuTien = 2`** (Xếp sau khách đặt trước đúng giờ và xếp trước khách trễ hẹn/qua lượt).

---

### 2.6. Khôi phục lượt khám cho bệnh nhân bị qua lượt
1. **Chức năng này để làm gì?**  
   Đưa bệnh nhân quay lại hàng chờ khi họ có mặt lại phòng khám sau khi đã bị bác sĩ bấm "Qua lượt" do gọi 3 lần không có mặt.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/LeTan/Index` (Thẻ "Danh sách qua lượt").
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Khi bệnh nhân đến quầy báo đã quay lại, Lễ tân chuyển sang tab **"Danh sách qua lượt"**.
   - Bước 2: Tìm số thứ tự của bệnh nhân (VD: `A-003`).
   - Bước 3: Bấm nút màu vàng viền ngoài: **"Khôi phục lượt"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Phiếu STT được chuyển trạng thái từ `QuaLuot` trở lại `DangCho`.
   - Mức ưu tiên của phiếu được thiết lập là **`MucUuTien = 3`** (để đảm bảo công bằng cho những bệnh nhân khác đang xếp hàng đúng giờ).

---

### 2.7. Quét quá hạn vắng hẹn thủ công
1. **Chức năng này để làm gì?**  
   Chủ động kích hoạt quét toàn bộ các phiếu hẹn trong ngày đã quá 30 phút mà chưa đến check-in để cập nhật dữ liệu báo cáo tức thì (ngoài tiến trình chạy ngầm định kỳ 5 phút/lần).
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/LeTan/Index` (Nút trên thanh công cụ).
3. **Hướng dẫn thao tác từng bước:**  
   - Bấm nút **"Quét quá hạn ngay"** (Nút viền xám có biểu tượng đồng hồ quét).
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-05, BR-06):**  
   - Hệ thống quét các phiếu `DaXacNhan` có `GioBatDau + 30 phút < Giờ hiện tại`.
   - Chuyển trạng thái phiếu sang `VangHen`.
   - Tự động tăng `SoLanVangHen = SoLanVangHen + 1` của bệnh nhân đó.
   - Nếu `SoLanVangHen >= 2`, hệ thống tự động gắn cờ `CoCanhBao = true` cho các lần đặt hẹn kế tiếp.

---

## PHẦN III: VAI TRÒ BÁC SĨ (CLINICAL DOCTOR)

### 3.1. Đăng nhập buồng khám và quản lý phiên làm việc
1. **Chức năng này để làm gì?**  
   Không gian làm việc lâm sàng dành riêng cho Bác sĩ điều khiển hàng đợi, gọi số bệnh nhân, ghi nhận thời gian khám và phát âm thanh gọi số.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Tài khoản vai trò `BacSi`.
   - Tài khoản mẫu: `bacsi01` (Nội tổng quát - P101), `bacsi02` (TMH - P102), `bacsi03` (Da liễu - P103).
   - Đường dẫn: `/BacSi` hoặc `/BacSi/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Đăng nhập bằng tài khoản Bác sĩ (VD: `bacsi01` / `123456`).
   - Bước 2: Vào **"Cổng nội bộ"** -> Chọn **"Bàn làm việc Bác sĩ"**.
   - Bước 3: Hệ thống tự động nhận diện bác sĩ và chọn buồng khám được phân công trực trong ngày.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Bác sĩ chỉ nhìn thấy và điều phối danh sách bệnh nhân thuộc phòng khám của mình.
   - Bố cục gồm 2 phần: Cột trái là danh sách hàng đợi phân theo 3 nhóm; Cột phải là Thẻ thông tin bệnh nhân đang phục vụ và cụm nút bấm điều khiển.

---

### 3.2. Gọi số thứ tự tiếp theo (Thuật toán 3 cấp ưu tiên)
1. **Chức năng này để làm gì?**  
   Lấy bệnh nhân tiếp theo vào phòng khám theo đúng thuật toán công bằng y tế: ưu tiên người đặt trước đúng giờ, xen kẽ hợp lý người vãng lai, và hạn chế nhảy cóc.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/BacSi/Index` (Cụm nút điều khiển).
3. **Hướng dẫn thao tác từng bước:**  
   - Bấm nút to màu xanh dương: **"Gọi số tiếp theo"** (`.btn-primary`).
4. **Kết quả hệ thống & Quy tắc lưu ý (Thuật toán 3 cấp ưu tiên):**  
   - Hệ thống tự động lọc và gọi theo thứ tự chuẩn xác:
     * **Ưu tiên 1:** Khách đặt trước đến đúng giờ (`A-xxx`) **đã bước vào cửa sổ kích hoạt** (Giờ hẹn <= Giờ hiện tại + 15 phút).
     * **Ưu tiên 2:** Khách vãng lai (`W-xxx`) theo thứ tự check-in trước vào trước.
     * **Ưu tiên 3:** Khách đặt trước đến trễ (`L-xxx`) hoặc khách từng bị qua lượt được khôi phục.
   - *Quy tắc chống nhảy cóc:* Nếu bệnh nhân `A` đến quá sớm (trước giờ hẹn > 15 phút) và trong phòng đang có khách `W` chờ, hệ thống sẽ gọi khách `W` trước chứ không cho khách `A` nhảy cóc.
   - *Quy tắc không làm nghẽn buồng khám:* Nếu khách `A` chưa có mặt check-in, bác sĩ bấm gọi số sẽ được hệ thống phân bổ ngay khách `W` vào khám, buồng khám không bị trống.

---

### 3.3. Gọi lại và xử lý qua lượt bệnh nhân
1. **Chức năng này để làm gì?**  
   Phát lại âm thanh gọi số nếu bệnh nhân chưa kịp bước vào buồng khám, và chuyển trạng thái "Qua lượt" nếu gọi quá 3 lần mà không có mặt.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/BacSi/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - **Gọi lại:** Bấm nút **"Gọi lại (X/3)"** (Nút màu xanh nhạt viền ngoài).
   - **Qua lượt:** Nếu sau 3 lần gọi không có mặt, bấm nút màu đỏ viền ngoài: **"Qua lượt"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Mỗi lần bấm "Gọi lại", hệ thống phát lại âm thanh qua loa và tăng bộ đếm số lần gọi.
   - **Khóa nút gọi lại sau 3 lần:** Khi `SoLanGoi = 3`, nút "Gọi lại" sẽ tự động bị khóa mờ (`disabled`). Hệ thống hiển thị dòng khuyến nghị: *"Đã gọi 3 lần, vui lòng bấm Qua lượt để phục vụ bệnh nhân tiếp theo"*.
   - Khi bấm "Qua lượt", phiếu STT chuyển sang trạng thái `QuaLuot`, buồng khám sẵn sàng để bấm gọi người tiếp theo.

---

### 3.4. Bắt đầu khám và Hoàn tất ca khám (Đồng bộ DaKham)
1. **Chức năng này để làm gì?**  
   Ghi nhận chính xác mốc thời gian phục vụ thực tế (phục vụ tính toán KPI Thời gian chờ trung bình) và hoàn tất hồ sơ lượt khám.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/BacSi/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - **Bắt đầu khám:** Khi bệnh nhân bước vào ngồi trước bàn khám, Bác sĩ bấm nút màu xanh lá cây: **"Bắt đầu khám"** (`.btn-success`).
   - **Hoàn tất khám:** Sau khi thăm khám, kê đơn và tư vấn xong, Bác sĩ bấm nút: **"Hoàn tất khám"** (`.btn-primary`).
4. **Kết quả hệ thống & Quy tắc lưu ý (BR-07):**  
   - Bấm "Bắt đầu khám": Phiếu chuyển sang `DangKham`, ghi nhận `ThoiGianBatDauKham = DateTime.Now`.
   - Bấm "Hoàn tất khám": Phiếu STT chuyển sang `HoanTat`, ghi nhận `ThoiGianHoanTat = DateTime.Now`.
   - **BR-07:** Hệ thống tự động đồng bộ phiếu đặt hẹn liên kết (`PhieuDatHen`) sang trạng thái **`DaKham`**. Bệnh nhân được tính là hoàn thành ca khám thành công.

---

### 3.5. Cấu hình và sử dụng Loa gọi số Bluetooth (Web Speech / Web Audio)
1. **Chức năng này để làm gì?**  
   Tự động phát âm thanh chuông báo (Chime bip-bip) và đọc to số thứ tự, tên bệnh nhân và buồng khám qua loa ngoài hoặc loa Bluetooth đặt trước sảnh chờ.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/BacSi/Index` (Hộp công cụ "Loa gọi số Bluetooth" ở góc phải).
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Kết nối máy tính của Bác sĩ với Loa ngoài hoặc Loa Bluetooth qua cài đặt Windows.
   - Bước 2: Tại giao diện buồng khám, kiểm tra công tắc **"Bật âm thanh gọi số"** (đang bật ON).
   - Bước 3: Điều chỉnh các thông số:
     * *Âm lượng (Volume)*: Kéo thanh trượt từ 0% đến 100% (Mặc định 100%).
     * *Tốc độ đọc (Rate)*: Kéo thanh trượt từ 0.5x đến 1.5x (Khuyến nghị 0.9x để đọc rõ ràng).
     * *Giọng đọc (Voice)*: Chọn giọng đọc tiếng Việt (VD: "Microsoft An" hoặc "Google Tiếng Việt").
   - Bước 4: Bấm nút **"Thử phát âm thanh"** để kiểm tra loa.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Khi bác sĩ bấm "Gọi số tiếp theo" hoặc "Gọi lại", hệ thống sẽ tự động kích hoạt chuỗi âm thanh:
     1. Chuông báo hiệu 2 âm điệu Ding-dong qua Web Audio API tần số cao.
     2. Giọng đọc chuẩn: *"Xin mời bệnh nhân số... A-001, Nguyễn Văn Nam, vào phòng khám số... 101"*.
   - Hoạt động mượt mà không cần cài đặt thêm phần mềm bên ngoài.

---

### 3.6. Xem lịch sử phục vụ trong ca trực lâm sàng
1. **Chức năng này để làm gì?**  
   Giúp Bác sĩ xem lại toàn bộ các bệnh nhân mình đã khám trong buổi làm việc, kiểm tra thời gian chờ và thời gian khám của từng ca.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Bác sĩ buồng khám.
   - Đường dẫn: `/BacSi/LichSuCa`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bấm vào mục **"Lịch sử ca trực"** trên menu điều hướng.
   - Chọn ngày cần xem (mặc định hôm nay).
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Hiển thị danh sách bảng: Số thứ tự, Tên bệnh nhân, Loại khách (Đặt trước / Vãng lai), Giờ check-in, Giờ vào khám, Giờ hoàn tất, và Tổng thời lượng khám (phút).

---

## PHẦN IV: VAI TRÒ QUẢN TRỊ VIÊN (ADMINISTRATOR)

### 4.1. Quản lý danh mục Chuyên khoa (CRUD)
1. **Chức năng này để làm gì?**  
   Khai báo và điều chỉnh các Chuyên khoa khám chữa bệnh trong phòng khám.
2. **Quyền truy cập & Đường dẫn:**  
   - Quyền hạn: Tài khoản `Admin`.
   - Đường dẫn: `/Admin/ChuyenKhoa`.
3. **Hướng dẫn thao tác từng bước:**  
   - **Thêm chuyên khoa:** Bấm nút **"Thêm chuyên khoa mới"** -> Nhập *Tên chuyên khoa* (VD: "Răng Hàm Mặt"), *Mô tả tóm tắt* -> Bấm **"Lưu chuyên khoa"**.
   - **Sửa chuyên khoa:** Bấm nút **"Sửa"** trên dòng tương ứng -> Cập nhật thông tin -> Bấm **"Cập nhật"**.
   - **Xóa chuyên khoa:** Bấm nút **"Xóa"** và xác nhận.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Hệ thống áp dụng ràng buộc toàn vẹn cơ sở dữ liệu: Nếu chuyên khoa đang có Bác sĩ hoặc Phòng khám trực thuộc, hệ thống sẽ từ chối xóa và hiển thị cảnh báo an toàn.

---

### 4.2. Quản lý danh mục Phòng khám (CRUD)
1. **Chức năng này để làm gì?**  
   Quản lý danh sách các buồng khám vật lý trong cơ sở y tế.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/Admin/PhongKham`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bấm nút **"Thêm phòng khám mới"** (`/Admin/TaoPhongKham`).
   - Nhập *Tên phòng* (VD: "Phòng 201"), *Vị trí* (VD: "Tầng 2 - Khu B"), *Chuyên khoa phụ trách*.
   - Bấm **"Tạo phòng khám"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Phòng khám sau khi tạo sẽ sẵn sàng để gán vào các ca trực lâm sàng.

---

### 4.3. Quản lý danh mục Bác sĩ và Quản lý tài khoản
1. **Chức năng này để làm gì?**  
   Quản lý hồ sơ nhân sự bác sĩ, tài khoản đăng nhập cổng nội bộ và phân quyền vai trò.
2. **Quyền truy cập & Đường dẫn:**  
   - Quản lý Bác sĩ: `/Admin/BacSi`.
   - Quản lý Tài khoản (Bác sĩ & Lễ tân): `/Admin/TaiKhoan?nhom=NhanSu`.
   - Quản lý Tài khoản (Bệnh nhân): `/Admin/TaiKhoan?nhom=BenhNhan`.
3. **Hướng dẫn thao tác từng bước:**  
   - **Tạo tài khoản mới:** Truy cập `/Admin/TaoTaiKhoan`, nhập Tên đăng nhập, Mật khẩu, Số điện thoại, chọn Vai trò (`Admin`, `LeTan`, `BacSi`, `BenhNhan`) -> Bấm **"Tạo tài khoản"**.
   - **Tạo hồ sơ Bác sĩ:** Truy cập `/Admin/TaoBacSi`, nhập Họ tên, Học vị (ThS.BS, BS.CKI), Số điện thoại, chọn Chuyên khoa và chọn Tài khoản liên kết -> Bấm **"Lưu hồ sơ bác sĩ"**.
   - **Khóa tài khoản:** Tại danh sách tài khoản, bấm nút **"Khóa"** đối với nhân sự nghỉ việc/tạm ngưng.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Tài khoản bị khóa sẽ không thể đăng nhập vào hệ thống ngay lập tức.

---

### 4.4. Quản lý Lịch làm việc và Sinh khung giờ tự động (Tỷ lệ 3:1)
1. **Chức năng này để làm gì?**  
   Lập lịch làm việc cho Bác sĩ theo từng ngày và buồng khám, đồng thời tự động phân rã ca làm việc thành các khung giờ khám (Slot) theo thuật toán tỷ lệ đệm chuẩn y tế.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: Menu **"Lịch làm việc"** (`/Admin/CaTruc`) -> Nút **"Tạo lịch làm việc mới"** (`/Admin/TaoCaTruc`).
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Chọn *Bác sĩ phụ trách* (VD: "BS.CKI Nguyễn Văn An").
   - Bước 2: Chọn *Phòng khám* (VD: "Phòng 101").
   - Bước 3: Chọn *Ngày trực* và *Buổi trực* ("Sáng" hoặc "Chiều").
   - Bước 4: Nhập *Giờ bắt đầu ca* (VD: `07:30`) và *Giờ kết thúc ca* (VD: `11:30`).
   - Bước 5: Nhập *Thời lượng mỗi khung giờ* (Khuyến nghị: `30` phút hoặc `15` phút).
   - Bước 6: Nhập *Tần suất khung đệm*: Mặc định là `3` (Cứ sau 3 slot Đặt hẹn thì tự động tạo 1 slot Khung đệm).
   - Bước 7: Bấm nút **"Tạo ca trực"**.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - **Chống trùng lịch:** Hệ thống tự động kiểm tra: Nếu Bác sĩ đó HOẶC Phòng khám đó đã có ca trực trong cùng buổi và ngày được chọn, hệ thống sẽ chặn và thông báo lỗi.
   - **Tự động sinh slot (Tỷ lệ 3:1):**  
     * Ví dụ ca 4 tiếng (30 phút/slot = 8 slot): Slot 1, 2, 3 là `DatHen`; Slot 4 là `KhungDem`; Slot 5, 6, 7 là `DatHen`; Slot 8 là `KhungDem`.
     * `KhungDem` đóng vai trò giảm tải áp lực, bù trừ cho các ca khám phức tạp kéo dài hoặc dành cho tiếp nhận cấp cứu/vãng lai khẩn cấp.

---

### 4.5. Khóa và mở khóa khung giờ thủ công
1. **Chức năng này để làm gì?**  
   Cho phép Admin chủ động đóng một khung giờ cụ thể (họp đột xuất, bảo trì thiết bị phòng khám) mà không ảnh hưởng đến các khung giờ khác trong ca.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/Admin/KhungGioDetail/{maCa}`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Tại danh sách ca trực `/Admin/CaTruc`, bấm nút **"Chi tiết khung giờ"** của ca trực tương ứng.
   - Bước 2: Tại danh sách các slot:
     * Muốn tạm khóa slot nào: Bấm nút **"Khóa slot"** -> Trạng thái slot chuyển sang `TamKhoa`.
     * Muốn mở lại slot: Bấm nút **"Mở khóa"** -> Trạng thái slot chuyển lại `ConTrong`.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Các slot ở trạng thái `TamKhoa` sẽ bị ẩn khỏi màn hình đặt lịch của bệnh nhân.

---

### 4.6. Khai thác BI Dashboard KPI và Biểu đồ Chart.js
1. **Chức năng này để làm gì?**  
   Cung cấp bức tranh phân tích toàn diện cho Ban Giám đốc và Quản trị viên về hiệu suất vận hành, tỷ lệ lấp đầy phòng khám và chất lượng dịch vụ.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/Admin` hoặc `/Admin/Index`.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Chọn khoảng thời gian cần phân tích: *Từ ngày* và *Đến ngày* (mặc định 14 ngày gần nhất).
   - Bước 2: Chọn *Chuyên khoa* hoặc *Bác sĩ* cụ thể (hoặc để trống để xem toàn viện).
   - Bước 3: Bấm nút **"Lọc báo cáo"**.
   - Bước 4: Quan sát 4 thẻ KPI chỉ số vàng:
     * **1. Tỷ lệ vắng hẹn (No-Show Rate):** Tỷ lệ % bệnh nhân đặt lịch nhưng không đến khám (Mục tiêu: < 15%).
     * **2. Tỷ lệ hoàn thành (Completion Rate):** Tỷ lệ % bệnh nhân hoàn tất ca khám thành công (Mục tiêu: > 80%).
     * **3. Thời gian chờ trung bình (Avg Wait Time):** Số phút trung bình từ lúc bệnh nhân check-in đến lúc được gọi vào khám (Mục tiêu: < 20 phút).
     * **4. Tỷ lệ lấp đầy khung giờ (Slot Occupancy):** Hiệu suất sử dụng các khung giờ khám đã mở (Mục tiêu: > 70%).
   - Bước 5: Phân tích 2 biểu đồ trực quan (Chart.js Light Mode):
     * *Biểu đồ cột (Bar Chart)*: Lưu lượng bệnh nhân theo các khung giờ trong ngày (xác định giờ cao điểm để điều phối bác sĩ).
     * *Biểu đồ đường (Line Chart)*: Xu hướng đặt lịch khám và tỷ lệ hoàn thành qua các ngày.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - Dữ liệu KPI trên giao diện khớp 100% với các câu lệnh truy vấn phân tích SQL chuẩn được kiểm định trong [Verify_KPI_SQL.sql](file:///c:/Users/Asus/Desktop/Phong/DACN_HTQL/Database/Verify_KPI_SQL.sql).

---

### 4.7. Xuất báo cáo dữ liệu định dạng CSV (Chuẩn Excel tiếng Việt)
1. **Chức năng này để làm gì?**  
   Trích xuất dữ liệu chi tiết của kỳ báo cáo ra tệp tin bảng tính để phục vụ lưu trữ, kiểm toán hoặc gửi báo cáo định kỳ cho ban lãnh đạo.
2. **Quyền truy cập & Đường dẫn:**  
   - Đường dẫn: `/Admin/Index` -> Nút **"Xuất CSV (Excel)"**.
3. **Hướng dẫn thao tác từng bước:**  
   - Bước 1: Thiết lập bộ lọc ngày và chuyên khoa theo nhu cầu.
   - Bước 2: Bấm nút màu xanh lá cây: **"Xuất CSV (Excel)"** (`.btn-success`).
   - Bước 3: Trình duyệt tự động tải xuống tệp tin có tên dạng `BaoCao_DACN_HTQL_20261120_xxxxxx.csv`.
   - Bước 4: Nhấp đúp chuột để mở trực tiếp file bằng Microsoft Excel.
4. **Kết quả hệ thống & Quy tắc lưu ý:**  
   - File được xuất với mã hóa **UTF-8 kèm tiền tố BOM (`\uFEFF`)**, đảm bảo mở trực tiếp trên mọi phiên bản Excel (Windows/Mac) hiển thị tiếng Việt có dấu 100% sắc nét, hoàn toàn không bị lỗi font hay vỡ ký tự.

---

## TỔNG HỢP QUY TẮC NGHIỆP VỤ CỐT LÕI (BR-01 ĐẾN BR-07)

Nhằm đảm bảo tính minh bạch, công bằng và kỷ luật vận hành y tế, hệ thống áp dụng nghiêm ngặt 07 Quy tắc nghiệp vụ tự động:

* **BR-01 (Giới hạn thời gian Đặt & Hủy lịch):**
  - Đặt lịch trực tuyến: Phải đặt trước giờ khám ít nhất **30 phút**.
  - Hủy lịch hẹn: Bệnh nhân chỉ được tự hủy trước giờ hẹn ít nhất **02 giờ**.
* **BR-02 (Chống đặt trùng lịch cùng Số điện thoại):**
  - Cùng một số điện thoại không được phép có 02 lịch hẹn hợp lệ giao nhau thời gian trong cùng một ngày.
* **BR-03 (Tái sử dụng Khung giờ sau khi Hủy):**
  - Ngay khi một phiếu hẹn được hủy hợp lệ, khung giờ tương ứng ngay lập tức tự động mở lại trạng thái `ConTrong` để phục vụ bệnh nhân khác.
* **BR-04 (Phân loại Thứ tự Hàng đợi 3 cấp):**
  - Cấp 1 (`A-xxx`, `MucUuTien = 1`): Bệnh nhân đặt trước đến đúng giờ (hoặc sớm).
  - Cấp 2 (`W-xxx`, `MucUuTien = 2`): Khách vãng lai đến quầy tiếp đón trực tiếp.
  - Cấp 3 (`L-xxx`, `MucUuTien = 3`): Khách đặt trước đến trễ (16–30 phút) hoặc khách qua lượt được khôi phục.
* **BR-05 (Chế tài Đến trễ > 30 phút & Tự động Vắng hẹn):**
  - Bệnh nhân đến trễ quá 30 phút bị từ chối check-in theo diện đặt trước, phải chuyển sang xếp hàng vãng lai.
  - Tiến trình chạy ngầm (`VangHenBackgroundService`) tự động chuyển các phiếu quá 30 phút chưa check-in sang trạng thái `VangHen`.
* **BR-06 (Cơ chế Cảnh báo Vắng hẹn >= 2 lần):**
  - Bệnh nhân tích lũy từ 02 lần vắng hẹn trở lên sẽ bị bật cờ `CoCanhBao = true`.
  - Các lần đặt hẹn tiếp theo của bệnh nhân này bắt buộc vào trạng thái `ChoXacNhan`. Lễ tân phải gọi điện xác minh trước khi duyệt sang `DaXacNhan`.
* **BR-07 (Đồng bộ Khám xong & Thống kê KPI):**
  - Khi Bác sĩ bấm "Hoàn tất khám" trên buồng khám, phiếu hẹn liên kết tự động chuyển sang `DaKham`.
  - Mọi mốc thời gian Check-in, Bắt đầu và Hoàn tất được ghi nhận chính xác đến từng giây để tính toán 4 chỉ số KPI vận hành.
