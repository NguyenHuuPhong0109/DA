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
- **Tooltip:** Nền tối `#182433`, chữ trắng `#FFFFFF`, viền `#DCE1E7`, bo góc `6px`.
