# DESIGN SYSTEM — Phòng Khám Trực Tuyến (HTTT-13)

> **Phiên bản:** 2.0 (GIAI ĐOẠN 6B — Chuyển toàn bộ hệ thống sang Chế độ Sáng chuẩn Tabler Bootstrap 5).  
> **Phong cách:** Y tế — Sạch sẽ — Sắc nét — Mật độ thông tin cao.  
> **Tham chiếu cốt lõi:** Tabler UI Component Library (`_design-ref/tabler-dev/tabler-dev/`).

---

## 1. Triết lý thiết kế (Tabler Light Mode Y tế)

### Điều cấm tuyệt đối
- ❌ Nền tối / dark theme / sidebar đen đặc ở bất kỳ phân hệ nào.
- ❌ Gradient tím-xanh hoặc card bo tròn bóng mờ nặng nề (`shadow-2xl`, `rounded-3xl`).
- ❌ Chữ mờ hoặc chữ trắng trên nền sáng (thiếu tương phản, vi phạm WCAG).
- ❌ Tranh chấp thứ cấp giữa các nút hành động (nhiều nút Primary cùng cấp trong 1 khối).
- ❌ Nút bấm dính sát nhau hoặc rớt dòng xấu trên điện thoại màn hình hẹp (`390px`).

### Nguyên tắc cốt lõi (Chuẩn Tabler)
- ✅ **Hệ màu sáng dịu (Canvas & Surface):** Page Canvas `#F6F8FB`, Card / Table / Sidebar / Nav `#FFFFFF`.
- ✅ **Viền mảnh sắc nét & Bóng đổ nhẹ:** `1px solid #DCE1E7`, box-shadow đặc trưng Tabler `0 1px 2px rgba(24, 36, 51, 0.04)`.
- ✅ **Màu chủ đạo Y tế (Medical Teal):** Primary `#0F766E` (hover `#115E59`, soft `#F0FDFA`).
- ✅ **Hình học nút đồng bộ:** Chiều cao chuẩn `38px–40px` (`32px` cho `.btn-sm`), bo góc `6px–8px`, `font-weight: 500`.
- ✅ **Đủ 4 trạng thái tương tác:** Default, `:hover`, `:active`, `:focus-visible` (ring xanh mảnh rõ ràng), `:disabled` (`opacity: 0.55`).
- ✅ **Container `.btn-list`:** Gom các nhóm nút cạnh nhau bằng `flex-wrap` + `gap: 8px` co giãn hoàn hảo trên mọi kích thước màn hình.

---

## 2. Hệ thống Font chữ

```css
@import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:ital,wght@0,300;0,400;0,500;0,600;0,700;1,400&display=swap');
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&display=swap');
```

| Biến CSS | Font Family | Trọng số (Weight) | Mục đích sử dụng |
|----------|-------------|-------------------|------------------|
| `--font-display` | `Be Vietnam Pro`, sans-serif | 600, 700 | Heading, Page title, Wordmark thương hiệu |
| `--font-body` | `Be Vietnam Pro`, sans-serif | 400, 500 | Thân văn bản, Label, Nút bấm, Table cell |
| `--font-outlier` / `.font-mono` | `JetBrains Mono`, monospace | 500, 600, 700 | Mã đặt hẹn `A-xxx`, `W-xxx`, `L-xxx`, Giờ khám, Số điện thoại |

---

## 3. Bảng màu Chuẩn Tabler (Light Mode)

### Màu nền & Chữ (Canvas & Typography)

```css
:root {
  color-scheme: light;
  --color-paper:         #F6F8FB; /* Body canvas - xám sáng dịu chuyên nghiệp */
  --color-paper-surface: #FFFFFF; /* Bề mặt thẻ, bảng, sidebar, navbar */
  --color-paper-2:       #F1F5F9; /* Nền hover dòng bảng, panel phụ */
  --color-paper-3:       #E6EAEF; /* Nền slot disabled / đường kẻ phụ */

  --color-ink:           #182433; /* Chữ chính Tabler sắc nét */
  --color-ink-2:         #334155; /* Chữ thứ cấp */
  --color-muted:         #667382; /* Chữ phụ, nhãn, metadata */

  --color-rule:          #DCE1E7; /* Viền thẻ, bảng, form Tabler */
  --color-rule-2:        #CBD5E1; /* Viền phân cách đậm */

  --shadow-card:         0 1px 2px rgba(24, 36, 51, 0.04);
  --shadow-lift:         0 1px 3px rgba(24, 36, 51, 0.06), 0 6px 16px -2px rgba(24, 36, 51, 0.08);
}
```

### Màu Ngữ nghĩa & Nhận diện (Semantic & Medical Palette)

| Vai trò | Mã màu | Hex | Trạng thái / Ứng dụng |
|---------|--------|-----|------------------------|
| **Primary (Y tế)** | `--color-primary` | `#0F766E` | Nút hành động chính (Đặt lịch, Check-in, Gọi số, Lưu). Hover: `#115E59`, Soft: `#F0FDFA` |
| **Success** | `--color-success` | `#2FB344` | Đã khám, Hoàn tất khám, Check-in thành công. Đậm: `#15803D`, Soft: `#DCFCE7` |
| **Warning** | `--color-warning` | `#F59F00` | Nền dịu chữ đậm: nền `#FEF3C7`, viền `#FCD34D`, chữ `#92400E` (Qua lượt, Tạm khóa) |
| **Danger** | `--color-error` | `#D63939` | Hủy lịch, Xóa, Cảnh báo vắng hẹn >= 2 lần. Đậm: `#B91C1C`, Soft: `#FEE2E2` |
| **Info** | `--color-info` | `#066FD1` | Đang gọi, Trạng thái thông tin. Đậm: `#0369A1`, Soft: `#E0F2FE` |

---

## 4. Hệ thống Nút bấm (Button Architecture)

### Kích thước & Hình học
- **Nút tiêu chuẩn (`.btn`):**
  - Chiều cao: `38px` (min-height `38px`)
  - Padding: `0 16px`
  - Bo góc: `6px` (`--radius-btn`)
  - Font: `500 var(--text-sm)`
  - Hiển thị: `display: inline-flex; align-items: center; justify-content: center; gap: 6px; white-space: nowrap;`
- **Nút nhỏ trong bảng (`.btn-sm`):**
  - Chiều cao: `32px`
  - Padding: `0 10px`
  - Font: `0.8125rem`
  - Bo góc: `4px`
- **Nút lớn Hero / CTA (`.btn-lg`):**
  - Chiều cao: `44px`
  - Padding: `0 20px`
  - Font: `1rem`
- **Container nhóm nút (`.btn-list`):**
  - `display: flex; flex-wrap: wrap; gap: 8px; align-items: center;`

### Phân cấp Nút (Button Hierarchy)

```html
<!-- 1. Hành động chính: Duy nhất 1 nút trong 1 khối -->
<button class="btn btn-primary">Xác nhận đặt lịch</button>

<!-- 2. Hành động phụ: Dùng nút trắng viền mảnh sắc nét của Tabler -->
<button class="btn btn-outline-secondary">Gọi lại (1/3)</button>
<button class="btn btn-outline-secondary">Xuất CSV</button>

<!-- 3. Thành công / Tiến trình khám -->
<button class="btn btn-success">Bắt đầu khám</button>

<!-- 4. Chú ý / Cảnh báo (Nền dịu mắt, không dùng nền vàng chói chữ trắng) -->
<button class="btn btn-warning">Qua lượt</button>

<!-- 5. Phá hủy / Hủy lịch -->
<button class="btn btn-outline-danger">Hủy lịch</button>
<button class="btn btn-danger">Xóa ca trực</button>
```

---

## 5. Thẻ (Cards), Bảng (Tables) & Sidebar

### Thẻ (Card)
- Nền `#FFFFFF`, viền `1px solid #DCE1E7`, bo góc `8px`, bóng `0 1px 2px rgba(24, 36, 51, 0.04)`.
- Header thẻ phân tách bằng đường kẻ `1px solid #DCE1E7`.

### Bảng (Table)
- Đóng gói trong container `.table-wrap` (bo góc `8px`, viền sắc nét, cuộn ngang tự động).
- `thead th`: Nền `#F8FAFC`, chữ `#667382`, font `0.75rem`, uppercase, `letter-spacing: 0.04em`.
- `tbody tr:hover td`: Đổi màu sang `#F1F5F9`.

### Thanh điều hướng & Sidebar
- **Patient Nav:** Nền `#FFFFFF`, dính đỉnh (`position: sticky`), viền đáy `1px solid #DCE1E7`, brand xanh teal `#0F766E`.
- **Portal Sidebar (Admin / Lễ tân / Bác sĩ):** Nền `#FFFFFF` (loại bỏ hoàn toàn màu tối), viền phải `1px solid #DCE1E7`. Nav link active có nền `#F0FDFA`, chữ `#0F766E`, viền nhấn trái `3px solid #0F766E`.

---

## 6. Lưới chọn Khung giờ (Slot Picker)

Các slot hiển thị với độ tương phản sắc nét trên nền sáng:
- **Còn trống (`.slot--trong`):** Nền `#FFFFFF`, viền `#DCE1E7`, chữ `#182433`. Hover: nền `#F0FDFA`, viền & chữ `#0F766E`.
- **Đang chọn (`.is-selected`):** Nền `#0F766E`, chữ `#FFFFFF`, viền `#0F766E`.
- **Đã đặt (`.is-taken` / `.slot--da-dat`):** Nền `#F1F5F9`, viền `#DCE1E7`, chữ `#94A3B8`, gạch ngang, cursor `not-allowed`.
- **Tạm khóa (`.is-locked` / `.slot--tam-khoa`):** Nền `#FEF2F2`, viền `#FECACA`, chữ `#B91C1C`, cursor `not-allowed`.
- **Khung đệm (`.is-buffer`):** Nền `#F8FAFC`, viền nét đứt `1px dashed #CBD5E1`, chữ `#64748B`.

---

## 7. Biểu đồ Chart.js (Dashboard BI)

- **Màu chữ mặc định:** `#667382` (Tabler muted text).
- **Màu lưới trục X/Y:** `#E6E7E9` (thanh mảnh, tinh tế).
- **Cột Lưu lượng theo giờ:** Nền `rgba(15, 118, 110, 0.85)`, viền `#0F766E`, bo góc cột `4px`.
- **Cột Tỷ lệ Vắng hẹn theo Bác sĩ:** Nền `rgba(217, 119, 6, 0.85)`, viền `#D97706`, bo góc cột `4px`.
- **Biểu đồ tròn Cơ cấu kết quả khám (Doughnut):** Hoàn thành `#0F766E`, Vắng hẹn `#D97706`, Chờ khám/Khác `#CBD5E1`, viền trắng `2px`, cutout `65%`.
- **Tooltip:** Nền tối `#182433`, chữ trắng `#FFFFFF`, viền `#DCE1E7`, bo góc `6px`.
- **Chart Resize on Tab Switch:** Lắng nghe sự kiện chuyển Tab Bootstrap 5 (`shown.bs.tab`) và gọi `chart.resize()` để chống vỡ tỉ lệ hoặc co kích thước bằng 0 khi canvas nằm trong tab ẩn.

---

## 8. Chuẩn hóa Bố cục Dashboard 3 tầng & Hệ thống Danh mục (Tabler & Zocdoc/YouMed v2.1)

### 8.1. Dashboard 3 tầng (Tối giản — Thoáng mắt — Không cuộn dài)
- **Tầng 1 (Header & Bộ lọc thu gọn 1 hàng):**
  - Tiêu đề 1 dòng: Tiêu đề trang + badge khoảng thời gian lọc (`dd/MM/yyyy — dd/MM/yyyy`) bên trái; nút `Xuất CSV` (`.btn-outline-secondary`), `⏱️ Quét quá hạn` và nút thu gọn bộ lọc trên Mobile bên phải.
  - Thanh bộ lọc: Bố trí gọn trên **1 hàng ngang duy nhất** (chiều cao ô nhập `36px`, padding thẻ `12px 16px`), gồm Từ ngày, Đến ngày, Chuyên khoa, Bác sĩ, nút Lọc và nút Đặt lại.
- **Tầng 2 (4 thẻ KPI tối giản trên 1 hàng `row g-3`):**
  - Nền trắng `#FFFFFF`, viền mảnh `1px solid #DCE1E7`, không tô nền màu chói.
  - Chỉ hiển thị: Nhãn chỉ số nhỏ `#667382` + icon thông tin ℹ️ chứa tooltip công thức (`title="..."`) + Con số chính cỡ lớn (`28px`, `font-weight: 700`) + 1 dòng phụ ngắn (mẫu số / lượt gọi số).
- **Tầng 3 (Phân tách nội dung chi tiết bằng Bootstrap 5 Tabs chuẩn Tabler):**
  - **Tab 1 — Biểu đồ & Xu hướng (mặc định):** Bố cục 2 cột `col-lg-8` (Lưu lượng bệnh nhân theo khung giờ) và `col-lg-4` (Cơ cấu trạng thái khám Doughnut), giới hạn chiều cao `300px` nhìn trọn trên màn hình desktop.
  - **Tab 2 — Hiệu suất Bác sĩ & Chuyên khoa:** Thanh mini-bar thống kê tài nguyên (Chuyên khoa, Phòng, Bác sĩ, Tài khoản, Ca trực hôm nay) + Biểu đồ tỷ lệ vắng hẹn theo Bác sĩ + Bảng phân rã chi tiết hiệu suất từng bác sĩ.
  - **Tab 3 — Cảnh báo vắng hẹn:** Gắn badge số lượng màu đỏ nổi bật cạnh tên Tab khi có cảnh báo (> 0) + Bảng danh sách bệnh nhân có cờ cảnh báo (Quy tắc BR-06).

### 8.2. Phân nhóm Sidebar Navigation Admin
- Tổ chức lại menu Sidebar thành 3 nhóm rõ ràng với tiêu đề nhỏ mờ (`11px`, `uppercase`, `color: #667382`, `letter-spacing: 0.06em`):
  - **VẬN HÀNH:** Tổng quan (Dashboard), Ca trực & Khung giờ.
  - **DANH MỤC CƠ SỞ:** Chuyên khoa, Phòng khám, Bác sĩ.
  - **HỆ THỐNG:** Tài khoản.
- Trạng thái Active: Nền `#F0FDFA`, chữ `#0F766E`, vạch nhấn trái `3px solid #0F766E`, đồng bộ trên cả Desktop Sidebar và Mobile Offcanvas Drawer.

### 8.3. Chuẩn hóa Hệ thống Danh mục (`ChuyenKhoa`, `PhongKham`, `BacSi`, `CaTruc`, `KhungGioDetail`, `TaiKhoan`)
- **Page Header đồng nhất:** Tiêu đề trang + badge xám nhạt đếm tổng dòng bên trái; nút chính `+ Thêm ...` (`.btn-primary`) bên phải.
- **Table Toolbar:** Ô tìm kiếm nhanh (client-side pure JS filter) lọc tức thì theo tên/mã/SĐT mà không làm ảnh hưởng submit server.
- **Tối ưu Bảng dữ liệu (`.table--clean`):**
  - Header nền `#F8FAFC`, chữ in hoa nhỏ `12px`, padding cell rộng thoáng (`12px 16px`), viền ngang mảnh `1px solid #DCE1E7`, bỏ viền dọc rườm rà.
  - Gộp thông tin phụ xuống dòng dưới với class `.sub-info` (Mã phòng dưới Tên phòng; Học vị & SĐT dưới Tên bác sĩ; Chuyên khoa & Mã BS dưới Khoa).
  - Cột Thao tác căn phải, cụm nút Tabler `.btn-list justify-content-end flex-nowrap`, nút Sửa/Xem dùng `.btn-sm .btn-outline-secondary`, nút Xóa/Khóa dùng `.btn-sm .btn-outline-danger`.
  - Badge trạng thái dùng tông màu pastel dịu mắt (`bg-success-lt`, `bg-secondary-lt`, `bg-danger-lt`, `bg-info-lt`, `bg-warning-lt`).
- **Lọc nhanh Ca trực:** Nút chọn ngày nhanh (*Hôm nay | Ngày mai | Ngày kia | +3 ngày*) trên thanh công cụ `CaTruc.cshtml`.
- **Lọc slot Khung giờ:** Nút lọc trạng thái slot (*Tất cả | Còn trống | Đã đặt | Tạm khóa | Khung đệm*) trên `KhungGioDetail.cshtml`.

---

## 9. Quy tắc tối giản Hallmark (AGENTS.md), Tabler & shadcn-ui (Khử Anti-AI-Slop & Tinh chỉnh Bố cục v2.2)

### 9.1. Căn chỉnh Bảng dữ liệu chuẩn xác 100%
- **Căn thẳng hàng dọc tuyệt đối giữa `<th>` và `<td>`:**
  - Ghi đè triệt để mọi rule `text-align: center` trên `<th>`.
  - Mọi cột nội dung chữ và số liệu (Bác sĩ, Phòng khám, Chuyên khoa, Vị trí, Ca làm việc, Khung giờ, Tài khoản, Số điện thoại, Trạng thái) đều thống nhất **`text-align: left !important`** ở cả `<th>` và `<td>`.
  - Riêng cột **Thao tác** luôn căn phải (**`text-align: right !important`**) ở cả `<th>` và `<td>`.
- **Cột Thao tác nằm gọn trên đúng 1 dòng ngang:**
  - Sử dụng `.actions-wrap` (`display: inline-flex; align-items: center; justify-content: flex-end; gap: 8px; white-space: nowrap;`).
  - Toàn bộ thẻ `<form>` (nút Xóa, Khóa, Mở khóa) đều có `display: inline-flex; margin: 0;` để không bao giờ bị ngắt dòng.
- **Khít bảng vào thẻ (`.card-table`) & Khống chế bề rộng hiển thị:**
  - Vùng nội dung chính giới hạn `max-width: 1360px; margin: 0 auto; width: 100%;` tránh giãn cột quá xa trên màn hình UltraWide/FullHD.
  - Sử dụng `.card-table` bọc quanh `table--clean` (bỏ padding bao quanh của `.card` thông thường, đưa `<thead>` khít vào mép trên của thẻ), đường kẻ mảnh `1px solid #E5E7EB`, padding cell `12px 16px`, `vertical-align: middle`.

### 9.2. Việt hóa dữ liệu & Khử lạm dụng Badge màu (Anti-AI-Slop)
- **Việt hóa 100% khi render:**
  - Ca làm việc: `Sang` -> `Ca sáng`, `Chieu` -> `Ca chiều`, `Toi` -> `Ca tối`.
  - Trạng thái ca / phòng: `MoKham` / `HoatDong` -> `Đang mở`, `TamDong` / `DaDong` -> `Đã đóng`.
  - Vai trò người dùng: `Admin` -> `Quản trị viên`, `BacSi` -> `Bác sĩ`, `LeTan` -> `Lễ tân`, `BenhNhan` -> `Bệnh nhân`.
- **Chấm trạng thái (Status Dot) thay cho pill màu sặc sỡ:**
  - Bỏ bọc khung pill màu chói ở các cột Chuyên khoa, Ca làm việc, Vai trò (hiển thị bằng chữ thường gọn gàng).
  - Cột Trạng thái sử dụng chấm tròn nhỏ `.status-dot` (7px) kèm chữ:
    * `● Đang mở` / `● Hoạt động` (`.status-dot--success`, `#16A34A`).
    * `● Đã đóng` / `● Tạm ngưng` (`.status-dot--muted`, `#9CA3AF`).
    * `● Đã khóa` (`.status-dot--danger`, `#DC2626`).
- **Loại bỏ badge font mono cạnh tiêu đề trang:** Thay bằng dòng phụ chữ xám nhỏ đặt dưới tiêu đề chính (`.portal-content__sub`).

### 9.3. Xóa 100% icon và emoji bên trong tất cả nút bấm & ô tìm kiếm
- Nút bấm chỉ dùng chữ thuần ngắn gọn: `Tạo lịch làm việc`, `Thêm chuyên khoa`, `Thêm phòng khám`, `Thêm bác sĩ`, `Tạo tài khoản`, `Xem lịch`, `Khung giờ`, `Sửa`, `Xóa`, `Khóa`, `Mở khóa`, `Quét quá hạn`, `Xuất CSV`, `Bộ lọc`, `Quay lại`.
- Thuộc tính `placeholder` trong ô tìm kiếm không chứa emoji kính lúp `🔍`.
- Kiểu nút tối giản shadcn: chiều cao `36px` (`30px` cho `.btn-sm`), `font-weight: 500`, bo góc `6px`. Nút phụ viền `#D1D5DB` chữ `#1F2937`; nút nguy hiểm nền trắng viền `#FECACA` chữ `#DC2626` (hover `#FEF2F2`).

### 9.4. Cơ cấu lại Menu & Quản lý Tài khoản 2 nhóm
- Đổi tên toàn diện `Ca trực & Khung giờ` thành **`Lịch làm việc`** (và **`Quản lý Lịch làm việc`**).
- Tách phân khu `HỆ THỐNG` thành **`QUẢN LÝ TÀI KHOẢN`** với 2 mục menu con:
  - **`Bác sĩ & Lễ tân`** (`/Admin/TaiKhoan?nhom=NhanSu`)
  - **`Bệnh nhân`** (`/Admin/TaiKhoan?nhom=BenhNhan`)
- Thanh tab tối giản trong `TaiKhoan.cshtml` chuyển đổi nhanh giữa 2 nhóm tài khoản kèm bộ lọc vai trò nhân sự.
