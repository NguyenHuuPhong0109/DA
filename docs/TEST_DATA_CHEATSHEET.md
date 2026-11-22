# BẢNG TRA CỨU MÃ DỮ LIỆU KIỂM THỬ HỆ THỐNG (TEST DATA CHEATSHEET)
**Dự án:** Hệ thống Quản trị & Điều phối Khám bệnh Phòng khám (DACN_HTQL)  
**Thời gian tạo:** Tháng 11/2026  
**Lệnh làm mới toàn diện dữ liệu (Reseed):**  
```bash
dotnet run --project DACN_HTQL -- --reseed
```

---

## 1. TỔNG HỢP SỐ LƯỢNG DỮ LIỆU ĐÃ ĐỔ TRONG CƠ SỞ DỮ LIỆU

| Tên bảng dữ liệu | Tổng số bản ghi | Ghi chú & Mục đích kiểm thử |
| :--- | :---: | :--- |
| **`VaiTro`** | **4** | 4 vai trò chuẩn: `Admin`, `LeTan`, `BacSi`, `BenhNhan` |
| **`TaiKhoan`** | **9** | Mật khẩu chung `123456`: `admin`, `letan01`, `bacsi01`..`bacsi04`, `benhnhan01`..`benhnhan03` |
| **`ChuyenKhoa`** | **4** | Nội tổng quát, Tai Mũi Họng, Da liễu, Nhi khoa |
| **`PhongKham`** | **4** | Phòng 101 (Nội), Phòng 102 (TMH), Phòng 103 (Da liễu), Phòng 104 (Nhi khoa) |
| **`BacSi`** | **4** | BS.CKI Nguyễn Văn An, ThS.BS Trần Thị Bình, BS.CKI Lê Hoàng Cường, ThS.BS Phạm Minh Đức |
| **`BenhNhan`** | **75** | 3 BN gắn tài khoản + 10 BN cảnh báo vắng hẹn (`CoCanhBao = 1`) + 62 BN đa dạng |
| **`CaTruc`** | **76** | 28 ca lịch sử (14 ngày trước) + **48 ca dày đặc** (4 ngày: `Today` đến `Today + 3`, mỗi ngày đủ 3 ca Sáng/Chiều/Tối) |
| **`KhungGio`** | **1.056** | 808 slot `DatHen`, 248 slot `KhungDem` (tỷ lệ chuẩn 3 Đặt hẹn : 1 Khung đệm) |
| **`PhieuDatHen`** | **489** | Phủ dày 75%–85% các ca; đầy đủ các trạng thái `DaKham`, `VangHen`, `DaHuy`, `ChoXacNhan`, `DaXacNhan`, `DaCheckIn` |
| **`PhieuSoThuTu`** | **215** | Đầy đủ hàng đợi thực tế đang hoạt động hôm nay và lịch sử hoàn tất để tính KPI |

---

## 2. PHÂN BỔ DỮ LIỆU CHI TIẾT 4 NGÀY DÀY ĐẶC (`Today` $\rightarrow$ `Today + 3`)

| Ngày trực | Độ lệch ngày | Số ca trực (Sáng/Chiều/Tối) | Tổng số Khung giờ | Slot Đặt hẹn | Slot Khung đệm | Tổng Phiếu hẹn | Đã xác nhận (`DaXacNhan`) | Chờ xác nhận (`ChoXacNhan` - Cờ đỏ) | Đã Check-in | Đã Khám | Đã Hủy |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Hôm nay (`Today`)** | `+0` | 12 ca (4 phòng $\times$ 3 ca) | 152 | 116 | 36 | **68** | 4 | 8 | 32 | 24 | 0 |
| **Ngày mai (`Today + 1`)** | `+1` | 12 ca (4 phòng $\times$ 3 ca) | 152 | 116 | 36 | **75** | 59 | 10 | 0 | 0 | 6 |
| **Ngày kia (`Today + 2`)** | `+2` | 12 ca (4 phòng $\times$ 3 ca) | 152 | 116 | 36 | **75** | 59 | 10 | 0 | 0 | 6 |
| **Ngày kìa (`Today + 3`)** | `+3` | 12 ca (4 phòng $\times$ 3 ca) | 152 | 116 | 36 | **75** | 58 | 10 | 0 | 0 | 7 |

> [!NOTE]
> Trong 3 ngày tới (`Today + 1`, `Today + 2`, `Today + 3`), mỗi ngày có khoảng **20%–25% khung giờ `DatHen` ở trạng thái `ConTrong`** phân bố đều ở các ca Sáng, Chiều, Tối để người kiểm thử có thể tự thực hiện đặt thêm lịch mới.

---

## 3. HÀNG ĐỢI TRỰC TIẾP HÔM NAY TẠI MỖI PHÒNG KHÁM (`PhieuSoThuTu`)

Tại **mỗi phòng khám** (`Phòng 101`, `Phòng 102`, `Phòng 103`, `Phòng 104`), hệ thống đã đổ sẵn **13 phiếu thứ tự đang hoạt động** để kiểm thử ngay:

| Mã số thứ tự hiển thị | Mức ưu tiên | Loại khách | Trạng thái phiếu | Đặc điểm kịch bản kiểm thử |
| :--- | :---: | :--- | :--- | :--- |
| **`A-001`** | `1` (Cao nhất) | `DatTruoc` | `DangKham` | Bác sĩ đang khám cho bệnh nhân này (bấm **"Hoàn tất khám"** để thử nghiệm) |
| **`A-002`** | `1` (Cao nhất) | `DatTruoc` | `DangCho` | Khách đặt trước đến đúng giờ, giờ hẹn trong cửa sổ gọi |
| **`A-003`** | `1` (Cao nhất) | `DatTruoc` | `DangCho` | Khách đặt trước đến đúng giờ, chuẩn bị đến lượt |
| **`A-004`** | `1` (Cao nhất) | `DatTruoc` | `DangCho` | Khách đặt trước đến đúng giờ |
| **`A-005`** | `1` (Cao nhất) | `DatTruoc` | `DangCho` | **Khách đến sớm hơn giờ hẹn > 30 phút:** Kiểm chứng thuật toán **không cho nhảy cóc** trước khách `W` đang chờ |
| **`W-001`** | `2` (Trung bình) | `VangLai` | `DangCho` | Khách vãng lai đến trước, được gọi sau khách A đúng giờ và trước khách A đến quá sớm |
| **`W-002`** | `2` (Trung bình) | `VangLai` | `DangCho` | Khách vãng lai chờ lượt tiếp theo |
| **`W-003`** | `2` (Trung bình) | `VangLai` | `DangCho` | Khách vãng lai |
| **`W-004`** | `2` (Trung bình) | `VangLai` | `DangCho` | Khách vãng lai |
| **`L-001`** | `3` (Ưu tiên thấp) | `DatTruocTre` | `DangCho` | Khách đặt trước đến trễ 16–30 phút, xếp sau khách vãng lai |
| **`L-002`** | `3` (Ưu tiên thấp) | `DatTruocTre` | `DangCho` | Khách đặt trước đến trễ |
| **`A-006`** | `1` (Đã gọi 3 lần) | `DatTruoc` | `QuaLuot` | Bị chuyển qua lượt (`SoLanGoi = 3`). Dùng để test nút **"Khôi phục lượt"** của Lễ tân |
| **`W-005`** | `2` (Đã gọi 3 lần) | `VangLai` | `QuaLuot` | Khách vãng lai bị qua lượt. Dùng để test nút **"Khôi phục lượt"** của Lễ tân |

---

## 4. DANH SÁCH MÃ ĐẶT HẸN MẪU DÀNH CHO CÁC KỊCH BẢN KIỂM THỬ

### 4.1. Kịch bản 1: Test duyệt phiếu hẹn Bệnh nhân CẢNH BÁO ĐỎ (`CoCanhBao = 1`, `ChoXacNhan`)
Đăng nhập tài khoản Lễ tân (`letan01` / `123456`) vào `/LeTan/Index`. Tìm các phiếu sau có nhãn đỏ rực rỡ và bấm nút **"Duyệt"**:

| Mã đặt hẹn | Họ tên bệnh nhân | Số điện thoại | Số lần vắng hẹn | Phòng khám | Giờ hẹn hôm nay |
| :--- | :--- | :---: | :---: | :--- | :---: |
| **`BK261121-1431`** | Hoàng Văn Cảnh Báo | `0988000101` | **3 lần** | Phòng 101 (Nội) | 10:45 |
| **`BK261121-1432`** | Trần Thị Vắng | `0988000102` | **2 lần** | Phòng 101 (Nội) | 11:00 |
| **`BK261121-1448`** | Đặng Văn Hẹn | `0988000103` | **3 lần** | Phòng 102 (TMH) | 10:45 |
| **`BK261121-1449`** | Vũ Thị Muộn | `0988000104` | **2 lần** | Phòng 102 (TMH) | 11:00 |
| **`BK261121-1465`** | Ngô Tấn Quên | `0988000105` | **3 lần** | Phòng 103 (Da liễu) | 10:45 |
| **`BK261121-1482`** | Dương Đình Trễ | `0988000107` | **3 lần** | Phòng 104 (Nhi khoa) | 10:45 |

*(Sau khi Lễ tân bấm **"Duyệt"**, phiếu sẽ chuyển ngay sang `DaXacNhan` và mở khóa tính năng Check-in).*

---

### 4.2. Kịch bản 2: Test nút "Quét quá hạn ngay" và `VangHenBackgroundService`
Các phiếu sau ở trạng thái `DaXacNhan` nhưng đã quá giờ hẹn > 30 phút mà bệnh nhân chưa đến check-in. Khi Lễ tân bấm nút **"Quét quá hạn ngay"** (hoặc đợi tiến trình ngầm 5 phút), các phiếu này sẽ tự động chuyển sang **`VangHen`**:

| Mã đặt hẹn | Họ tên bệnh nhân | Số điện thoại | Phòng khám | Giờ hẹn hôm nay | Trạng thái ban đầu | Trạng thái sau quét |
| :--- | :--- | :---: | :--- | :---: | :---: | :---: |
| **`BK261121-1430`** | Lê Minh Khánh | `0977000011` | Phòng 101 | 10:30 | `DaXacNhan` | `VangHen` |
| **`BK261121-1447`** | Dương Minh Sơn | `0977000031` | Phòng 102 | 10:30 | `DaXacNhan` | `VangHen` |
| **`BK261121-1464`** | Bùi Minh Cường | `0977000051` | Phòng 103 | 10:30 | `DaXacNhan` | `VangHen` |
| **`BK261121-1481`** | Bùi Bích Lỡ | `0988000106` | Phòng 104 | 10:30 | `DaXacNhan` | `VangHen` |

---

### 4.3. Kịch bản 3: Test Tra cứu & Hủy lịch hẹn hợp lệ ($\ge 2$ giờ) của Bệnh nhân
Đăng nhập tài khoản Bệnh nhân (`benhnhan01` / `123456` hoặc `benhnhan02` / `123456`), vào trang chi tiết phiếu hẹn hoặc mục tra cứu để test:

| Tài khoản | Mã đặt hẹn | Ngày hẹn khám | Giờ khám | Phòng khám | Bác sĩ | Thao tác kiểm thử |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| **`benhnhan01`** | **`BK261122-1197`** | **Ngày mai (`Today + 1`)** | 08:00 | Phòng 101 | BS.CKI Nguyễn Văn An | Bấm **"Hủy lịch hẹn"** -> Hủy thành công vì cách giờ hẹn $\ge 2$ giờ; slot mở lại `ConTrong` |
| **`benhnhan01`** | **`BK261123-1198`** | **Ngày kia (`Today + 2`)** | 14:00 | Phòng 102 | ThS.BS Trần Thị Bình | Tra cứu phiếu hẹn, xem mã QR, xem chi tiết phiếu |
| **`benhnhan02`** | **`BK261122-1199`** | **Ngày mai (`Today + 1`)** | 15:00 | Phòng 103 | BS.CKI Lê Hoàng Cường | Bấm **"Hủy lịch hẹn"** -> Khung giờ tự động trả về `ConTrong` |
| **`benhnhan02`** | **`BK261123-1200`** | **Ngày kia (`Today + 2`)** | 09:00 | Phòng 104 | ThS.BS Phạm Minh Đức | Tra cứu lịch hẹn tại `/DatLich/TraCuu` |
| **`benhnhan03`** | **`BK261122-1201`** | **Ngày mai (`Today + 1`)** | 18:20 (Ca Tối) | Phòng 101 | BS.CKI Nguyễn Văn An | Test ca khám buổi tối |

---

### 4.4. Kịch bản 4: Test Bác sĩ Gọi số tiếp theo, Gọi lại và Loa Bluetooth
Đăng nhập vào bất kỳ tài khoản Bác sĩ nào (`bacsi01`, `bacsi02`, `bacsi03`, `bacsi04` / mật khẩu `123456`) tại `/BacSi/Index`:
- **Gọi số tiếp theo:** Bấm **"Gọi số tiếp theo"**, hệ thống sẽ gọi bệnh nhân `A-002` (vì đã trong cửa sổ gọi), loa Bluetooth phát âm chuông bip-bip và đọc tên bệnh nhân.
- **Kiểm chứng không nhảy cóc:** Sau khi gọi hết các khách `A` đúng giờ, nếu bấm tiếp, hệ thống sẽ gọi khách `W-001` (khách vãng lai) chứ **không gọi khách `A-005`** vì khách `A-005` đến sớm hơn giờ hẹn > 30 phút.
- **Gọi lại tối đa 3 lần:** Bấm **"Gọi lại"** 3 lần, nút sẽ bị khóa `disabled` và hiển thị khuyến nghị bấm **"Qua lượt"**.
- **Bắt đầu khám & Hoàn tất:** Bấm **"Bắt đầu khám"** (`DangKham`), sau đó bấm **"Hoàn tất khám"** (`HoanTat`), kiểm tra phiếu hẹn liên kết tự động đồng bộ sang `DaKham`.
