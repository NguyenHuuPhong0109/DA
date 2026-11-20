# DESIGN SYSTEM — Phòng Khám Trực Tuyến (HTTT-13)

> Tài liệu này chốt hệ thống thiết kế cho toàn bộ dự án.
> Nguồn tham khảo: Hallmark design library — `coral-physio-booking`, `dose`, `console`.
> **Không** dùng gradient tím-xanh mặc định, **không** lạm dụng card bo tròn bóng mờ đồng loạt.

---

## 1. Triết lý thiết kế (Anti-AI-Default Rules)

### Điều cấm tuyệt đối
- ❌ Gradient tím → xanh (`from-purple-500 to-blue-600`) làm nền hero
- ❌ Card bo tròn `rounded-2xl shadow-xl` đồng loạt trên mọi thành phần
- ❌ Icon emoji lớn làm "feature icons" (`🏥 💊 📅`)
- ❌ Nền trắng thuần `#ffffff` — dùng warm off-white thay thế
- ❌ Chữ tiêu đề thin/light weight — heading phải bold/medium rõ nét
- ❌ Lạm dụng `backdrop-blur` / glassmorphism trên nội bộ
- ❌ Badge/status pill tất cả cùng một màu xanh generic

### Nguyên tắc cốt lõi (từ Hallmark)
- ✅ **Đường kẻ thanh mảnh 1px** (`var(--rule-hair): 1px`) phân chia layout thay vì shadow
- ✅ **Mật độ thông tin rõ ràng** — bảng biểu dễ quét, nhãn trạng thái phân biệt màu
- ✅ **Chữ sắc nét** — heading `letter-spacing: -0.025em`, `font-synthesis: none`
- ✅ **Màu accent có ngữ nghĩa** — coral/warm cho bệnh nhân, amber/graphite cho nội bộ
- ✅ **Monospace cho số** — mã đặt hẹn `A-001`, số thứ tự `W-001` dùng `font-outlier`
- ✅ **Spacing theo hệ scale** — không dùng magic numbers, chỉ dùng CSS tokens

---

## 2. Font chữ

```css
/* Hỗ trợ tiếng Việt đầy đủ */
@import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:ital,wght@0,300;0,400;0,500;0,600;0,700;1,400&display=swap');
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500&display=swap');
```

| Vai trò | Font | Weight | Dùng cho |
|---------|------|--------|----------|
| `--font-display` | Be Vietnam Pro | 600–700 | Heading, wordmark, tiêu đề section |
| `--font-body` | Be Vietnam Pro | 400–500 | Body text, label, nav link |
| `--font-outlier` | JetBrains Mono | 400–500 | Mã đặt hẹn `A-001`, số thứ tự `W-001`, thời gian |

> **Lý do chọn Be Vietnam Pro:** Hỗ trợ dấu tiếng Việt đầy đủ (hơn 400 glyphs), thiết kế grotesk hiện đại, tương đương General Sans/Schibsted Grotesk trong Hallmark nhưng bản địa hóa.

---

## 3. Bảng màu — Cổng Bệnh nhân (Patient Portal)

Kế thừa từ **coral-physio-booking** — tông ấm/y tế, chuyên nghiệp không lạnh:

```css
/* tokens-patient.css */
:root {
  color-scheme: light;

  /* Paper — warm off-white, không phải trắng thuần */
  --color-paper:   oklch(97% 0.006 55);   /* kem nhạt ấm */
  --color-paper-2: oklch(94.5% 0.007 55); /* kem hơi đậm */
  --color-paper-3: oklch(91.5% 0.009 55); /* panel nhạt */

  /* Ink — dark warm brown, không phải đen thuần */
  --color-ink:     oklch(20% 0.010 35);
  --color-ink-2:   oklch(27% 0.012 40);
  --color-muted:   oklch(46% 0.012 45);

  /* Rule — đường kẻ mảnh, tông ấm */
  --color-rule:    oklch(87% 0.009 55);
  --color-rule-2:  oklch(81% 0.011 55);

  /* Accent coral — màu nhận dạng thương hiệu phòng khám */
  --color-accent:      oklch(74% 0.125 30);  /* coral trung */
  --color-accent-2:    oklch(54% 0.165 29);  /* coral đậm (hover, underline) */
  --color-accent-soft: oklch(94% 0.040 30);  /* coral nhạt (bg badge) */
  --color-accent-ink:  oklch(20% 0.010 35);  /* text trên nền coral */
  --color-focus:       oklch(54% 0.17 30);

  /* Semantic */
  --color-success: oklch(50% 0.12 150);  /* xanh lá — đã xác nhận */
  --color-error:   oklch(52% 0.19 27);   /* đỏ — cảnh báo */
  --color-warning: oklch(72% 0.14 60);   /* cam — chú ý */

  /* Font */
  --font-display: "Be Vietnam Pro", ui-sans-serif, system-ui, sans-serif;
  --font-body:    var(--font-display);
  --font-outlier: "JetBrains Mono", ui-monospace, monospace;

  /* Spacing */
  --space-3xs: 2px; --space-2xs: 4px; --space-xs: 8px;
  --space-sm: 12px; --space-md: 16px; --space-lg: 24px;
  --space-xl: 40px; --space-2xl: 64px; --space-3xl: 96px;

  /* Type scale */
  --text-xs: 0.8125rem; --text-sm: 0.875rem; --text-md: 1rem;
  --text-lg: 1.25rem;   --text-xl: 1.5625rem; --text-2xl: 1.953rem;
  --text-display: clamp(2.75rem, 5vw + 1rem, 5.25rem);
  --text-section: clamp(2rem, 2.5vw + 1rem, 3.5rem);
  --text-title:   clamp(1.25rem, 0.75vw + 0.875rem, 1.625rem);

  /* Shape */
  --radius-card:  8px;
  --radius-input: 6px;
  --radius-pill:  999px;
  --rule-hair:    1px;

  /* Shadow — chỉ dùng sparingly */
  --shadow-card: 0 1px 2px oklch(20% 0.01 35 / 0.05);
  --shadow-lift: 0 1px 2px oklch(20% 0.01 35 / 0.05),
                 0 18px 40px -22px oklch(20% 0.01 35 / 0.22);

  /* Layout */
  --container: 1280px;
  --gutter:    48px;
  --gap-col:   24px;

  /* Typography */
  --tracking-display: -0.025em;
  --display-weight:   700;

  /* Z-index */
  --z-dropdown:   100;
  --z-sticky:     200;
  --z-sticky-nav: 300;
  --z-modal:      400;
  --z-toast:      500;

  /* Easing */
  --ease-out:    cubic-bezier(0.16, 1, 0.3, 1);
  --ease-in-out: cubic-bezier(0.65, 0, 0.35, 1);
  --dur-micro:   120ms;
  --dur-short:   220ms;
  --dur-long:    420ms;
}
```

### Badge trạng thái — Cổng Bệnh nhân

| Trạng thái | Màu nền | Màu chữ | Ý nghĩa |
|------------|---------|---------|---------|
| `ChoXacNhan` | `--color-paper-2` | `--color-muted` | Chờ duyệt |
| `DaXacNhan` | `oklch(88% 0.08 160)` | `oklch(28% 0.08 155)` | Xanh lá nhạt — đã duyệt |
| `DaCheckIn` | `oklch(90% 0.07 60)` | `oklch(35% 0.09 55)` | Vàng nhạt — đã có mặt |
| `DaKham` | `--color-ink` | `--color-paper` | Đen — hoàn tất |
| `DaHuy` | `--color-paper-3` | `--color-muted` | Xám — đã hủy |
| `VangHen` | `oklch(92% 0.06 25)` | `oklch(45% 0.14 25)` | Cam nhạt — không đến |

---

## 4. Bảng màu — Cổng Nội bộ (Internal Portal)

Kế thừa từ **console** — dark graphite, mật độ cao, bảng biểu rõ ràng:

```css
/* tokens-internal.css */
:root {
  color-scheme: dark;

  /* Paper — graphite dark */
  --color-paper:   oklch(17% 0.008 250);
  --color-paper-2: oklch(21% 0.009 250);
  --color-paper-3: oklch(13.5% 0.007 250);

  /* Ink — sáng trên nền tối */
  --color-ink:   oklch(95% 0.005 250);
  --color-ink-2: oklch(83% 0.008 250);
  --color-muted: oklch(71% 0.01 250);

  /* Rule */
  --color-rule:   oklch(31% 0.01 250);
  --color-rule-2: oklch(42% 0.012 250);

  /* Accent amber — tone nội bộ chuyên nghiệp */
  --color-accent:      oklch(82% 0.15 80);   /* amber sáng */
  --color-accent-soft: oklch(29% 0.05 80);   /* amber tối nhạt */
  --color-accent-ink:  oklch(18% 0.04 75);   /* text trên amber */
  --color-focus:       oklch(82% 0.15 80);

  /* Semantic status */
  --color-ok:      oklch(76% 0.15 150);  /* xanh lá */
  --color-ok-soft: oklch(24.5% 0.03 150);
  --color-bad:     oklch(70% 0.17 25);   /* đỏ */
  --color-bad-soft:oklch(24.5% 0.035 25);
  --color-cool:    oklch(77% 0.12 245);  /* xanh dương */
  --color-cool-soft:oklch(29% 0.05 245);

  /* Font — giống patient portal */
  --font-display: "Be Vietnam Pro", ui-sans-serif, system-ui, sans-serif;
  --font-body:    var(--font-display);
  --font-outlier: "JetBrains Mono", ui-monospace, monospace;

  /* Shape — góc nhọn hơn patient */
  --radius-card:  10px;
  --radius-input: 6px;
  --radius-pill:  6px;   /* pill KHÔNG phải 999px trong nội bộ */
  --rule-hair:    1px;
  --shadow-card:  none;  /* dùng đường kẻ thay shadow */
}
```

### Badge trạng thái — Nội bộ (Hàng đợi)

| Trạng thái | Token | Dùng cho |
|------------|-------|---------|
| `DangCho` | `--color-muted` text, border `--color-rule-2` | Chờ gọi |
| `DangGoi` | `--color-cool` text, `--color-cool-soft` bg | Đang gọi lên |
| `DangKham` | `--color-ok` text, `--color-ok-soft` bg | Đang khám |
| `QuaLuot` | `--color-bad` text, `--color-bad-soft` bg | Quá lượt |
| `HoanTat` | `--color-ink` text, `--color-paper-3` bg | Xong |
| `UuTien-1` | `--color-accent` (amber), border | Ưu tiên cao nhất |

---

## 5. Hai Portal — Màn hình & Layout

### 5A. Cổng Bệnh nhân (Light — coral warm)

**Màn hình áp dụng:** Trang chủ, Đặt lịch, Tra cứu lịch hẹn, Đăng ký/Đăng nhập

```
Layout: container 1280px, gutter 48px
Nav: sticky top, background paper, border-bottom 1px rule khi scroll
Hero: 2-col grid (text 5fr / visual 7fr), min-height 80dvh
Section rhythm: gap 120px → 144px → 176px
Typography: Be Vietnam Pro, heading tracking -0.025em, weight 700
Buttons: pill shape (radius-pill 999px), primary = ink fill, secondary = outline
Cards: NÊN dùng border 1px rule thay vì shadow — chỉ dùng shadow-lift cho floating element
Forms: input height 48px, border 1px muted, focus outline 2px coral
```

**Component kit:**
- `.btn` — pill button primary/secondary/quiet
- `.status` — inline badge nhỏ, border 1px
- `.week` — lịch chọn ngày (5 cột ngày trong tuần)
- `.slots` — grid khung giờ (3 cột, min-height 48px)
- `.appt` — hàng lịch hẹn (3 cột: giờ / tên+chuyên khoa / badge)
- `.booking-code` — mã đặt hẹn `A-001` dùng `.fig` + `font-outlier`

### 5B. Cổng Nội bộ (Dark — graphite amber)

**Màn hình áp dụng:** Dashboard Lễ tân, Quản lý hàng đợi, Dashboard Bác sĩ, Admin

```
Layout: sidebar 240px fixed + main content
Nav: bar graphite, 3 panel cut by hairline (logo | links | actions)
Tables: border-top 1px ink (heavy), rows border-bottom 1px rule, thead muted small
Chips/Badge: radius-pill = 6px (không phải 999px), semantic color
Monospace: tất cả số thứ tự, mã, giờ dùng font-outlier + tabular-nums
Density: padding-block 12px mỗi row, padding-inline 16px
```

**Component kit:**
- `.bar` — 3-panel nav cut from one surface
- `.panels` — grid 1px gap + background rule (tạo đường chia)
- `.chip` — badge nhỏ amber/cool/ok/bad
- `.steps` — bảng hàng đợi (số TT | Bệnh nhân | Giờ hẹn | Trạng thái | Hành động)
- `.trail` — audit trail / lịch sử thao tác
- `.queue-number` — số thứ tự lớn `W-001`, font-outlier, tabular-nums slashed-zero
- `.seg` — segmented control (tabs nội bộ)

---

## 6. Quy tắc Typography

```css
/* BASE — áp dụng mọi nơi */
h1, h2, h3 {
  font-family: var(--font-display);
  font-weight: 700;
  font-synthesis: none;         /* QUAN TRỌNG: không để browser fake bold */
  letter-spacing: var(--tracking-display); /* -0.025em */
  line-height: 1.05;
  text-wrap: balance;
}

/* Monospace — mã và số */
.fig, .code, .queue-num, .booking-code {
  font-family: var(--font-outlier);
  font-variant-numeric: tabular-nums slashed-zero;
  font-weight: 400;
}

/* Tiếng Việt: đảm bảo dấu không bị cắt */
body {
  -webkit-font-smoothing: antialiased;
  overflow-wrap: break-word;
  line-height: 1.55; /* cao hơn tiêu chuẩn Tây để dấu không bị chạm nhau */
}
```

---

## 7. Quy tắc Input & Form

```css
/* Input chuẩn — KHÔNG dùng rounded-full cho input */
.form-input {
  min-height: 48px;
  padding: 0 var(--space-md);
  border: var(--rule-hair) solid var(--color-muted);
  border-radius: var(--radius-input); /* 6px */
  background: var(--color-paper);
  font: inherit;
  outline: 2px solid transparent;
  outline-offset: -3px;
  transition: border-color var(--dur-short);
}
.form-input:hover  { border-color: var(--color-ink-2); }
.form-input:focus  { outline-color: var(--color-focus); }
.form-input[aria-invalid] { border-color: var(--color-error); }

/* Label — luôn visible, không dùng placeholder làm label */
.form-label {
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--color-muted);
}
```

---

## 8. Quy tắc Button

```css
/* PRIMARY — solid fill */
.btn {
  display: inline-flex; align-items: center; justify-content: center;
  min-height: 48px; padding: 0 var(--space-lg);
  border: 1px solid var(--color-ink);
  border-radius: var(--radius-pill);
  background: var(--color-ink); color: var(--color-paper);
  font: 500 var(--text-md)/1 var(--font-body);
  transition: background-color var(--dur-short) var(--ease-out);
}
.btn:hover   { background: var(--color-ink-2); }
.btn:active  { transform: translateY(1px); }
.btn:focus-visible { outline: 2px solid var(--color-focus); outline-offset: 2px; }
.btn:disabled { opacity: 0.55; cursor: not-allowed; transform: none; }

/* SECONDARY — outline */
.btn--quiet {
  background: transparent; color: var(--color-ink);
  border-color: var(--color-rule-2);
}
.btn--quiet:hover { background: var(--color-paper-2); }

/* SMALL */
.btn--sm { min-height: 40px; padding: 0 var(--space-md); font-size: var(--text-sm); }
```

---

## 9. Quy tắc Table (Nội bộ)

```
- Dòng header: border-top 1px INK (heavy accent), padding-block 8px, font-size sm, muted
- Dòng data: border-bottom 1px rule, padding-block 12px 16px
- Số/mã: căn phải, dùng class .fig (tabular-nums)
- Trạng thái: .chip với semantic color — KHÔNG dùng màu xanh generic cho mọi trạng thái
- Hover row: background paper-2 (subtler vì dark mode đã đủ contrast)
- NO zebra striping — dùng border-bottom thay thế
```

---

## 10. Ánh xạ màu trạng thái → Nghiệp vụ

```
Trạng thái hẹn:
  ChoXacNhan  → neutral muted border
  DaXacNhan   → ok/green soft
  DaCheckIn   → cool/blue soft (đã có mặt tại phòng khám)
  DaKham      → ink fill (hoàn tất)
  DaHuy       → paper-3 / muted (không active)
  VangHen     → bad/red soft + cảnh báo

Hàng đợi:
  DangCho   → muted
  DangGoi   → cool/blue (đang được gọi — cần chú ý)
  DangKham  → ok/green (trong phòng khám)
  QuaLuot   → bad/red
  HoanTat   → paper-3

Phòng:
  HoatDong  → ok/green dot
  TamNgung  → warning/amber dot
  BaoTri    → bad/red dot
```

---

## 11. Responsive Breakpoints

```css
/* Mobile first — 3 breakpoints */
@media (max-width: 40rem)  { /* < 640px  — mobile */ }
@media (max-width: 60rem)  { /* < 960px  — tablet */ }
@media (max-width: 85rem)  { /* < 1360px — small desktop */ }

/* Tại 60rem: gutter 24px, section-gap giảm 20% */
/* Tại 40rem: gutter 16px, section-gap giảm 40% */
```

---

## 12. File structure CSS trong dự án

```
wwwroot/css/
├── tokens-patient.css    ← CSS custom properties cho bệnh nhân (coral warm)
├── tokens-internal.css   ← CSS custom properties cho nội bộ (graphite amber)
├── base.css              ← Reset + base styles (chung)
├── patient.css           ← Components: nav, hero, booking, slots, appointment
├── internal.css          ← Components: sidebar, table, queue, badge, panel
└── site.css              ← Import tất cả (entry point _Layout.cshtml)
```

---

*Cập nhật lần cuối: 2026-11-19 | Tham khảo: Hallmark v2.8.0 · coral-physio-booking · console · dose*
