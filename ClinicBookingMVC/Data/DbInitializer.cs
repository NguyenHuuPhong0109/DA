using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Data;

public static class DbInitializer
{
    /// <summary>
    /// Nạp dữ liệu mẫu ban đầu cho hệ thống.
    /// - Khi reseed = true (--reseed): xóa toàn bộ dữ liệu theo thứ tự ngược khóa ngoại, reset identity về 0 và nạp lại.
    /// - Khi reseed = false (chạy thường): chỉ nạp khi bảng VaiTro rỗng, tránh ghi đè dữ liệu test của người dùng.
    /// </summary>
    public static async Task InitializeAsync(ClinicDbContext db, bool reseed = false)
    {
        if (db.Database.IsRelational())
        {
            try
            {
                await db.Database.MigrateAsync();
            }
            catch
            {
                await db.Database.EnsureCreatedAsync();
            }
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        if (reseed)
        {
            await ClearAllDataAsync(db);
        }
        else
        {
            if (await db.VaiTros.AnyAsync())
            {
                return; // Đã có dữ liệu, không nạp đè
            }
        }

        // ══════════════════════════════════════════════════════════════
        // 1. SEED VAI TRÒ (4 vai trò cốt lõi)
        // ══════════════════════════════════════════════════════════════
        var roles = new[] { "Admin", "LeTan", "BacSi", "BenhNhan" };
        foreach (var r in roles)
        {
            db.VaiTros.Add(new VaiTro { TenVaiTro = r, MoTa = $"Vai trò {r}" });
        }
        await db.SaveChangesAsync();

        var roleAdmin    = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "Admin");
        var roleLeTan    = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "LeTan");
        var roleBacSi    = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "BacSi");
        var roleBenhNhan = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "BenhNhan");

        // ══════════════════════════════════════════════════════════════
        // 2. SEED TÀI KHOẢN DEMO (Mật khẩu chung 123456 băm BCrypt)
        // ══════════════════════════════════════════════════════════════
        var hash123456 = BCrypt.Net.BCrypt.HashPassword("123456");

        var demoAccounts = new (string Username, int RoleId, string Phone)[]
        {
            ("admin",      roleAdmin.MaVaiTro,    "0900000001"),
            ("letan01",    roleLeTan.MaVaiTro,    "0900000002"),
            ("bacsi01",    roleBacSi.MaVaiTro,    "0901234561"),
            ("bacsi02",    roleBacSi.MaVaiTro,    "0901234562"),
            ("bacsi03",    roleBacSi.MaVaiTro,    "0901234563"),
            ("bacsi04",    roleBacSi.MaVaiTro,    "0901234564"),
            ("benhnhan01", roleBenhNhan.MaVaiTro, "0912345671"),
            ("benhnhan02", roleBenhNhan.MaVaiTro, "0912345672"),
            ("benhnhan03", roleBenhNhan.MaVaiTro, "0912345673"),
        };

        foreach (var acc in demoAccounts)
        {
            db.TaiKhoans.Add(new TaiKhoan
            {
                TenTaiKhoan = acc.Username,
                MatKhau     = hash123456,
                SoDienThoai = acc.Phone,
                TrangThai   = "HoatDong",
                MaVaiTro    = acc.RoleId,
            });
        }
        await db.SaveChangesAsync();

        var adminUser    = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");
        var leTanUser    = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "letan01");
        var tkBs1        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi01");
        var tkBs2        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi02");
        var tkBs3        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi03");
        var tkBs4        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi04");
        var tkBn1        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "benhnhan01");
        var tkBn2        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "benhnhan02");
        var tkBn3        = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "benhnhan03");

        // ══════════════════════════════════════════════════════════════
        // 3. SEED 4 CHUYÊN KHOA
        // ══════════════════════════════════════════════════════════════
        var chuyenKhoas = new (string Ten, string MoTa)[]
        {
            ("Nội tổng quát", "Khám và điều trị các bệnh lý nội khoa người lớn, tim mạch, tiêu hóa, hô hấp"),
            ("Tai Mũi Họng",  "Khám, nội soi và điều trị các bệnh lý tai mũi họng người lớn và trẻ em"),
            ("Da liễu",       "Điều trị các bệnh ngoài da, dị ứng, viêm da cơ địa và thẩm mỹ y khoa"),
            ("Nhi khoa",      "Khám, tư vấn dinh dưỡng và điều trị bệnh lý chuyên sâu cho trẻ sơ sinh và trẻ nhỏ"),
        };

        foreach (var ck in chuyenKhoas)
        {
            db.ChuyenKhoas.Add(new ChuyenKhoa
            {
                TenChuyenKhoa = ck.Ten,
                MoTa          = ck.MoTa,
                TrangThai     = "HoatDong"
            });
        }
        await db.SaveChangesAsync();

        var ckNoi = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Nội tổng quát");
        var ckTmh = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Tai Mũi Họng");
        var ckDa  = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Da liễu");
        var ckNhi = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Nhi khoa");

        // ══════════════════════════════════════════════════════════════
        // 4. SEED 4 PHÒNG KHÁM
        // ══════════════════════════════════════════════════════════════
        var phongKhams = new (string Ten, string ViTri, int MaCk)[]
        {
            ("Phòng 101", "Tầng 1 - Khu A", ckNoi.MaChuyenKhoa),
            ("Phòng 102", "Tầng 1 - Khu A", ckTmh.MaChuyenKhoa),
            ("Phòng 103", "Tầng 2 - Khu B", ckDa.MaChuyenKhoa),
            ("Phòng 104", "Tầng 2 - Khu B", ckNhi.MaChuyenKhoa),
        };

        foreach (var pk in phongKhams)
        {
            db.PhongKhams.Add(new PhongKham
            {
                TenPhong     = pk.Ten,
                ViTri        = pk.ViTri,
                MaChuyenKhoa = pk.MaCk,
                TrangThai    = "HoatDong"
            });
        }
        await db.SaveChangesAsync();

        var p101 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 101");
        var p102 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 102");
        var p103 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 103");
        var p104 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 104");

        // ══════════════════════════════════════════════════════════════
        // 5. SEED 4 BÁC SĨ HOẠT ĐỘNG
        // ══════════════════════════════════════════════════════════════
        var bacSis = new (string HoTen, string HocVi, string Phone, int MaCk, int MaTk)[]
        {
            ("Nguyễn Văn An",  "BS.CKI", "0901234561", ckNoi.MaChuyenKhoa, tkBs1.MaTaiKhoan),
            ("Trần Thị Bình",  "ThS.BS",  "0901234562", ckTmh.MaChuyenKhoa, tkBs2.MaTaiKhoan),
            ("Lê Hoàng Cường", "BS.CKI", "0901234563", ckDa.MaChuyenKhoa,  tkBs3.MaTaiKhoan),
            ("Phạm Minh Đức",  "ThS.BS",  "0901234564", ckNhi.MaChuyenKhoa, tkBs4.MaTaiKhoan),
        };

        foreach (var bs in bacSis)
        {
            db.BacSis.Add(new BacSi
            {
                HoTenBacSi   = bs.HoTen,
                HocVi        = bs.HocVi,
                SoDienThoai  = bs.Phone,
                MaChuyenKhoa = bs.MaCk,
                MaTaiKhoan   = bs.MaTk,
                TrangThai    = "HoatDong"
            });
        }
        await db.SaveChangesAsync();

        var bsAn    = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs1.MaTaiKhoan);
        var bsBinh  = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs2.MaTaiKhoan);
        var bsCuong = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs3.MaTaiKhoan);
        var bsDuc   = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs4.MaTaiKhoan);

        // ══════════════════════════════════════════════════════════════
        // 6. SEED 75 BỆNH NHÂN (Gồm 10 bệnh nhân có SoLanVangHen >= 2)
        // Lưu ý: Tuyệt đối không gán CoCanhBao, để SQL Server tự tính
        // ══════════════════════════════════════════════════════════════
        var dsBenhNhan = new List<BenhNhan>
        {
            // 3 Bệnh nhân demo gắn tài khoản
            new BenhNhan { TenBenhNhan = "Nguyễn Văn Nam",   SoDienThoai = "0912345671", NamSinh = 1990, GioiTinh = "Nam", Email = "nam.nguyen@gmail.com", SoLanVangHen = 0 },
            new BenhNhan { TenBenhNhan = "Lê Thị Mai",       SoDienThoai = "0912345672", NamSinh = 1994, GioiTinh = "Nu",  Email = "mai.le@gmail.com",     SoLanVangHen = 1 },
            new BenhNhan { TenBenhNhan = "Phạm Quốc Dũng",   SoDienThoai = "0912345673", NamSinh = 1985, GioiTinh = "Nam", Email = "dung.pham@gmail.com",   SoLanVangHen = 0 },

            // 10 Bệnh nhân cảnh báo vắng hẹn >= 2 lần
            new BenhNhan { TenBenhNhan = "Hoàng Văn Cảnh Báo",   SoDienThoai = "0988000101", NamSinh = 1988, GioiTinh = "Nam", Email = "canhbao.hoang@gmail.com",   SoLanVangHen = 3 },
            new BenhNhan { TenBenhNhan = "Trần Thị Vắng",         SoDienThoai = "0988000102", NamSinh = 1995, GioiTinh = "Nu",  Email = "vang.tran@gmail.com",       SoLanVangHen = 2 },
            new BenhNhan { TenBenhNhan = "Đặng Văn Hẹn",          SoDienThoai = "0988000103", NamSinh = 1982, GioiTinh = "Nam", Email = "hen.dang@gmail.com",        SoLanVangHen = 3 },
            new BenhNhan { TenBenhNhan = "Vũ Thị Muộn",           SoDienThoai = "0988000104", NamSinh = 1991, GioiTinh = "Nu",  Email = "muon.vu@gmail.com",         SoLanVangHen = 2 },
            new BenhNhan { TenBenhNhan = "Ngô Tấn Quên",          SoDienThoai = "0988000105", NamSinh = 1987, GioiTinh = "Nam", Email = "quen.ngo@gmail.com",        SoLanVangHen = 3 },
            new BenhNhan { TenBenhNhan = "Bùi Bích Lỡ",           SoDienThoai = "0988000106", NamSinh = 1999, GioiTinh = "Nu",  Email = "lo.bui@gmail.com",          SoLanVangHen = 2 },
            new BenhNhan { TenBenhNhan = "Dương Đình Trễ",        SoDienThoai = "0988000107", NamSinh = 1976, GioiTinh = "Nam", Email = "tre.duong@gmail.com",       SoLanVangHen = 3 },
            new BenhNhan { TenBenhNhan = "Mai Thị Vắng Mặt",      SoDienThoai = "0988000108", NamSinh = 2002, GioiTinh = "Nu",  Email = "vangmat.mai@gmail.com",     SoLanVangHen = 2 },
            new BenhNhan { TenBenhNhan = "Lâm Thanh Trục Trặc",   SoDienThoai = "0988000109", NamSinh = 1993, GioiTinh = "Nam", Email = "tructrac.lam@gmail.com",    SoLanVangHen = 2 },
            new BenhNhan { TenBenhNhan = "Hồ Hoài Hủy Lịch",      SoDienThoai = "0988000110", NamSinh = 1984, GioiTinh = "Nu",  Email = "huy.ho@gmail.com",          SoLanVangHen = 3 },
        };

        // Thêm 62 bệnh nhân thường với dữ liệu phong phú
        var hoDem = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý" };
        var tenLotNam = new[] { "Văn", "Đức", "Minh", "Hữu", "Gia", "Quang", "Bảo", "Tuấn", "Thanh", "Công" };
        var tenLotNu = new[] { "Thị", "Phương", "Thu", "Ngọc", "Thanh", "Kim", "Mỹ", "Bích", "Hồng", "Mai" };
        var tenNam = new[] { "An", "Bình", "Cường", "Dũng", "Hải", "Hiếu", "Hoàng", "Hùng", "Huy", "Khánh", "Khoa", "Long", "Nam", "Nghĩa", "Phong", "Phúc", "Quân", "Sơn", "Thắng", "Thịnh", "Tiến", "Tùng", "Vinh" };
        var tenNu = new[] { "Anh", "Chi", "Diệp", "Dung", "Giang", "Hà", "Hằng", "Hoa", "Hương", "Lan", "Linh", "Ly", "Mai", "Nga", "Ngân", "Nhung", "Oanh", "Phương", "Quỳnh", "Thảo", "Trang", "Uyên", "Vân", "Yến" };

        for (int i = 1; i <= 62; i++)
        {
            bool isNam = (i % 2 == 1);
            string ho = hoDem[(i * 3 + 1) % hoDem.Length];
            string lot = isNam ? tenLotNam[(i * 2) % tenLotNam.Length] : tenLotNu[(i * 2) % tenLotNu.Length];
            string t = isNam ? tenNam[(i * 5) % tenNam.Length] : tenNu[(i * 5) % tenNu.Length];
            string hoTen = $"{ho} {lot} {t}";
            string phone = $"0977{i:D6}";
            int namSinh = 1960 + (i * 7) % 64; // 1960 - 2023
            string email = $"user{i:D3}@clinicmail.vn";
            int vang = (i % 9 == 0) ? 1 : 0; // Một số người có 1 lần vắng hẹn nhẹ

            dsBenhNhan.Add(new BenhNhan
            {
                TenBenhNhan = hoTen,
                SoDienThoai = phone,
                NamSinh     = namSinh,
                GioiTinh    = isNam ? "Nam" : "Nu",
                Email       = email,
                SoLanVangHen = vang
            });
        }

        db.BenhNhans.AddRange(dsBenhNhan);
        await db.SaveChangesAsync();

        var allBn = await db.BenhNhans.ToListAsync();
        var bnMap = allBn.ToDictionary(b => b.SoDienThoai);

        var doctorConfigs = new (BacSi Bs, PhongKham Pk)[]
        {
            (bsAn,    p101),
            (bsBinh,  p102),
            (bsCuong, p103),
            (bsDuc,   p104),
        };

        var today = DateOnly.FromDateTime(DateTime.Today);
        var now   = DateTime.Now;

        // ══════════════════════════════════════════════════════════════
        // 7. SEED LỊCH SỬ 14 NGÀY TRƯỚC (today - 14 đến today - 1)
        // ══════════════════════════════════════════════════════════════
        var caTrucList = new List<CaTruc>();
        var khungGioList = new List<KhungGio>();
        var phieuHenList = new List<PhieuDatHen>();
        var phieuSttList = new List<PhieuSoThuTu>();

        int bookingGlobalSeq = 1000;

        for (int dayOffset = -14; dayOffset <= -1; dayOffset++)
        {
            var targetDate = today.AddDays(dayOffset);
            // Mỗi ngày lịch sử tạo 2 ca cho 2 bác sĩ luân phiên
            int bsIdx1 = Math.Abs(dayOffset) % 4;
            int bsIdx2 = (Math.Abs(dayOffset) + 1) % 4;
            var activeConfigs = new[] { doctorConfigs[bsIdx1], doctorConfigs[bsIdx2] };

            foreach (var cfg in activeConfigs)
            {
                var ca = new CaTruc
                {
                    MaBacSi              = cfg.Bs.MaBacSi,
                    MaPhong              = cfg.Pk.MaPhong,
                    NgayTruc             = targetDate,
                    CaLamViec            = "Sang",
                    GioBatDauCa          = new TimeOnly(7, 30),
                    GioKetThucCa         = new TimeOnly(11, 30),
                    ThoiLuongMoiKhungGio = 15,
                    TrangThaiCa          = "MoKham"
                };
                caTrucList.Add(ca);
            }
        }
        db.CaTrucs.AddRange(caTrucList);
        await db.SaveChangesAsync();

        // Sinh slot và phiếu hẹn cho các ca lịch sử
        int histBnPointer = 0;
        foreach (var ca in caTrucList)
        {
            var cur = ca.GioBatDauCa;
            int slotIdx = 0;
            while (cur.AddMinutes(ca.ThoiLuongMoiKhungGio) <= ca.GioKetThucCa)
            {
                slotIdx++;
                bool isKhungDem = (slotIdx % 4 == 0);
                var next = cur.AddMinutes(ca.ThoiLuongMoiKhungGio);
                var kg = new KhungGio
                {
                    MaCa         = ca.MaCa,
                    GioBatDau    = cur,
                    GioKetThuc   = next,
                    LoaiKhungGio = isKhungDem ? "KhungDem" : "DatHen",
                    TrangThai    = "ConTrong"
                };
                khungGioList.Add(kg);
                cur = next;
            }
        }
        db.KhungGios.AddRange(khungGioList);
        await db.SaveChangesAsync();

        // Nạp phiếu hẹn lịch sử (~70% DaKham, ~15% VangHen, ~15% DaHuy)
        foreach (var ca in caTrucList)
        {
            var datHenSlots = khungGioList.Where(k => k.MaCa == ca.MaCa && k.LoaiKhungGio == "DatHen").Take(7).ToList();
            foreach (var slot in datHenSlots)
            {
                histBnPointer = (histBnPointer + 1) % allBn.Count;
                var bn = allBn[histBnPointer];
                bookingGlobalSeq++;
                var maDatHen = $"BK{ca.NgayTruc:yyMMdd}-{bookingGlobalSeq:D4}";

                int mod = (bookingGlobalSeq % 10);
                string trangThaiHen;
                if (mod < 7)       trangThaiHen = "DaKham";
                else if (mod < 9)  trangThaiHen = "VangHen";
                else               trangThaiHen = "DaHuy";

                slot.TrangThai = (trangThaiHen == "DaHuy") ? "ConTrong" : "DaDat";

                var phieu = new PhieuDatHen
                {
                    MaDatHen         = maDatHen,
                    KenhDatLich      = (bookingGlobalSeq % 3 == 0) ? "DienThoai" : "Online",
                    LyDoKhamSoBo     = "Khám định kỳ tổng quát / theo dõi điều trị",
                    ThoiGianTaoPhieu = ca.NgayTruc.ToDateTime(new TimeOnly(8, 0)).AddDays(-2),
                    GuiNhacLich      = true,
                    TrangThaiHen     = trangThaiHen,
                    MaBenhNhan       = bn.MaBenhNhan,
                    MaNguoiTao       = adminUser.MaTaiKhoan,
                    MaKhungGio       = slot.MaKhungGio
                };
                phieuHenList.Add(phieu);
            }
        }
        db.PhieuDatHens.AddRange(phieuHenList);
        await db.SaveChangesAsync();

        // Tạo PhieuSoThuTu hoàn tất cho các phiếu DaKham lịch sử
        foreach (var ph in phieuHenList.Where(p => p.TrangThaiHen == "DaKham"))
        {
            var slot = khungGioList.First(k => k.MaKhungGio == ph.MaKhungGio);
            var ca = caTrucList.First(c => c.MaCa == slot.MaCa);
            var checkIn = ca.NgayTruc.ToDateTime(slot.GioBatDau).AddMinutes(-12);
            var wait = 10 + (ph.MaPhieuHen % 15);
            var goiSo = checkIn.AddMinutes(wait);
            var batDau = goiSo.AddMinutes(2);
            var hoanTat = batDau.AddMinutes(15);

            phieuSttList.Add(new PhieuSoThuTu
            {
                SoThuTu            = (ph.MaPhieuHen % 30) + 1,
                NgayCapSo          = ca.NgayTruc,
                LoaiKhach          = "DatTruoc",
                MucUuTien          = 1,
                GioHen             = slot.GioBatDau,
                ThoiGianCheckIn    = checkIn,
                ThoiGianGoiSo      = goiSo,
                ThoiGianBatDauKham = batDau,
                ThoiGianHoanTat    = hoanTat,
                SoLanGoi           = 1,
                TrangThaiPhieu     = "HoanTat",
                MaPhong            = ca.MaPhong,
                MaBenhNhan         = ph.MaBenhNhan,
                MaPhieuHen         = ph.MaPhieuHen
            });
        }
        db.PhieuSoThuTus.AddRange(phieuSttList);
        await db.SaveChangesAsync();

        // ══════════════════════════════════════════════════════════════
        // 8. SEED CA TRỰC 4 NGÀY (Today đến Today + 3)
        // 4 Bác sĩ / 4 Phòng khám, MỖI NGÀY ĐỦ 3 CA (Sáng, Chiều, Tối)
        // ══════════════════════════════════════════════════════════════
        var denseCaTrucs = new List<CaTruc>();
        var denseKhungGios = new List<KhungGio>();

        for (int dayOffset = 0; dayOffset <= 3; dayOffset++)
        {
            var targetDate = today.AddDays(dayOffset);

            foreach (var cfg in doctorConfigs)
            {
                // Ca Sáng: 07:30 - 11:30 (slot 15 phút = 16 slot)
                denseCaTrucs.Add(new CaTruc
                {
                    MaBacSi              = cfg.Bs.MaBacSi,
                    MaPhong              = cfg.Pk.MaPhong,
                    NgayTruc             = targetDate,
                    CaLamViec            = "Sang",
                    GioBatDauCa          = new TimeOnly(7, 30),
                    GioKetThucCa         = new TimeOnly(11, 30),
                    ThoiLuongMoiKhungGio = 15,
                    TrangThaiCa          = "MoKham"
                });

                // Ca Chiều: 13:30 - 17:30 (slot 15 phút = 16 slot)
                denseCaTrucs.Add(new CaTruc
                {
                    MaBacSi              = cfg.Bs.MaBacSi,
                    MaPhong              = cfg.Pk.MaPhong,
                    NgayTruc             = targetDate,
                    CaLamViec            = "Chieu",
                    GioBatDauCa          = new TimeOnly(13, 30),
                    GioKetThucCa         = new TimeOnly(17, 30),
                    ThoiLuongMoiKhungGio = 15,
                    TrangThaiCa          = "MoKham"
                });

                // Ca Tối: 18:00 - 20:00 (slot 20 phút = 6 slot)
                denseCaTrucs.Add(new CaTruc
                {
                    MaBacSi              = cfg.Bs.MaBacSi,
                    MaPhong              = cfg.Pk.MaPhong,
                    NgayTruc             = targetDate,
                    CaLamViec            = "Toi",
                    GioBatDauCa          = new TimeOnly(18, 0),
                    GioKetThucCa         = new TimeOnly(20, 0),
                    ThoiLuongMoiKhungGio = 20,
                    TrangThaiCa          = "MoKham"
                });
            }
        }
        db.CaTrucs.AddRange(denseCaTrucs);
        await db.SaveChangesAsync();

        // Tự động sinh KhungGio cho 48 ca trực dày đặc (3 DatHen : 1 KhungDem)
        foreach (var ca in denseCaTrucs)
        {
            var cur = ca.GioBatDauCa;
            int slotIdx = 0;
            while (cur.AddMinutes(ca.ThoiLuongMoiKhungGio) <= ca.GioKetThucCa)
            {
                slotIdx++;
                bool isKhungDem = (slotIdx % 4 == 0); // Slot 4, 8, 12, 16 là KhungDem
                var next = cur.AddMinutes(ca.ThoiLuongMoiKhungGio);
                denseKhungGios.Add(new KhungGio
                {
                    MaCa         = ca.MaCa,
                    GioBatDau    = cur,
                    GioKetThuc   = next,
                    LoaiKhungGio = isKhungDem ? "KhungDem" : "DatHen",
                    TrangThai    = "ConTrong"
                });
                cur = next;
            }
        }
        db.KhungGios.AddRange(denseKhungGios);
        await db.SaveChangesAsync();

        // ══════════════════════════════════════════════════════════════
        // 9. ĐỔ LỊCH HẸN CHO 3 NGÀY TIẾP THEO (Today + 1, Today + 2, Today + 3)
        // Lấp đầy 75% - 85% khung DatHen, chừa 15% - 25% ConTrong
        // Gán 4-6 lịch hẹn cho benhnhan01, benhnhan02, benhnhan03
        // ══════════════════════════════════════════════════════════════
        var futurePhieuHenList = new List<PhieuDatHen>();
        var bookedPatientPerDay = new HashSet<(int MaBn, DateOnly Ngay)>();

        // 9.1. Gán lịch hẹn đặc biệt cho tài khoản demo bệnh nhân
        var bn1 = bnMap["0912345671"];
        var bn2 = bnMap["0912345672"];
        var bn3 = bnMap["0912345673"];

        var futureDates = new[] { today.AddDays(1), today.AddDays(2), today.AddDays(3) };

        // benhnhan01: Today + 1 (Phòng 101, Sáng 08:00); Today + 2 (Phòng 102, Chiều 14:00)
        var slotBn1D1 = denseKhungGios.First(k => k.CaTruc.NgayTruc == futureDates[0] && k.CaTruc.MaPhong == p101.MaPhong && k.CaTruc.CaLamViec == "Sang" && k.GioBatDau == new TimeOnly(8, 0));
        var slotBn1D2 = denseKhungGios.First(k => k.CaTruc.NgayTruc == futureDates[1] && k.CaTruc.MaPhong == p102.MaPhong && k.CaTruc.CaLamViec == "Chieu" && k.GioBatDau == new TimeOnly(14, 0));
        bookingGlobalSeq++;
        futurePhieuHenList.Add(new PhieuDatHen
        {
            MaDatHen = $"BK{futureDates[0]:yyMMdd}-{bookingGlobalSeq:D4}",
            KenhDatLich = "Online",
            LyDoKhamSoBo = "Tái khám kiểm tra huyết áp định kỳ",
            ThoiGianTaoPhieu = now.AddHours(-5),
            GuiNhacLich = true,
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn1.MaBenhNhan,
            MaNguoiTao = tkBn1.MaTaiKhoan,
            MaKhungGio = slotBn1D1.MaKhungGio
        });
        slotBn1D1.TrangThai = "DaDat";
        bookedPatientPerDay.Add((bn1.MaBenhNhan, futureDates[0]));

        bookingGlobalSeq++;
        futurePhieuHenList.Add(new PhieuDatHen
        {
            MaDatHen = $"BK{futureDates[1]:yyMMdd}-{bookingGlobalSeq:D4}",
            KenhDatLich = "Online",
            LyDoKhamSoBo = "Khám nội soi mũi họng",
            ThoiGianTaoPhieu = now.AddHours(-3),
            GuiNhacLich = true,
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn1.MaBenhNhan,
            MaNguoiTao = tkBn1.MaTaiKhoan,
            MaKhungGio = slotBn1D2.MaKhungGio
        });
        slotBn1D2.TrangThai = "DaDat";
        bookedPatientPerDay.Add((bn1.MaBenhNhan, futureDates[1]));

        // benhnhan02: Today + 1 (Phòng 103, Chiều 15:00); Today + 2 (Phòng 104, Sáng 09:00)
        var slotBn2D1 = denseKhungGios.First(k => k.CaTruc.NgayTruc == futureDates[0] && k.CaTruc.MaPhong == p103.MaPhong && k.CaTruc.CaLamViec == "Chieu" && k.GioBatDau == new TimeOnly(15, 0));
        var slotBn2D2 = denseKhungGios.First(k => k.CaTruc.NgayTruc == futureDates[1] && k.CaTruc.MaPhong == p104.MaPhong && k.CaTruc.CaLamViec == "Sang" && k.GioBatDau == new TimeOnly(9, 0));
        bookingGlobalSeq++;
        futurePhieuHenList.Add(new PhieuDatHen
        {
            MaDatHen = $"BK{futureDates[0]:yyMMdd}-{bookingGlobalSeq:D4}",
            KenhDatLich = "Online",
            LyDoKhamSoBo = "Kiểm tra viêm da tiếp xúc",
            ThoiGianTaoPhieu = now.AddHours(-4),
            GuiNhacLich = true,
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn2.MaBenhNhan,
            MaNguoiTao = tkBn2.MaTaiKhoan,
            MaKhungGio = slotBn2D1.MaKhungGio
        });
        slotBn2D1.TrangThai = "DaDat";
        bookedPatientPerDay.Add((bn2.MaBenhNhan, futureDates[0]));

        bookingGlobalSeq++;
        futurePhieuHenList.Add(new PhieuDatHen
        {
            MaDatHen = $"BK{futureDates[1]:yyMMdd}-{bookingGlobalSeq:D4}",
            KenhDatLich = "Online",
            LyDoKhamSoBo = "Tư vấn dinh dưỡng trẻ nhỏ",
            ThoiGianTaoPhieu = now.AddHours(-2),
            GuiNhacLich = true,
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn2.MaBenhNhan,
            MaNguoiTao = tkBn2.MaTaiKhoan,
            MaKhungGio = slotBn2D2.MaKhungGio
        });
        slotBn2D2.TrangThai = "DaDat";
        bookedPatientPerDay.Add((bn2.MaBenhNhan, futureDates[1]));

        // benhnhan03: Today + 1 (Phòng 101, Tối 18:20); Today + 3 (Phòng 102, Sáng 08:30)
        var slotBn3D1 = denseKhungGios.First(k => k.CaTruc.NgayTruc == futureDates[0] && k.CaTruc.MaPhong == p101.MaPhong && k.CaTruc.CaLamViec == "Toi" && k.GioBatDau == new TimeOnly(18, 20));
        var slotBn3D3 = denseKhungGios.First(k => k.CaTruc.NgayTruc == futureDates[2] && k.CaTruc.MaPhong == p102.MaPhong && k.CaTruc.CaLamViec == "Sang" && k.GioBatDau == new TimeOnly(8, 30));
        bookingGlobalSeq++;
        futurePhieuHenList.Add(new PhieuDatHen
        {
            MaDatHen = $"BK{futureDates[0]:yyMMdd}-{bookingGlobalSeq:D4}",
            KenhDatLich = "Online",
            LyDoKhamSoBo = "Khám dạ dày buổi tối",
            ThoiGianTaoPhieu = now.AddHours(-6),
            GuiNhacLich = true,
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn3.MaBenhNhan,
            MaNguoiTao = tkBn3.MaTaiKhoan,
            MaKhungGio = slotBn3D1.MaKhungGio
        });
        slotBn3D1.TrangThai = "DaDat";
        bookedPatientPerDay.Add((bn3.MaBenhNhan, futureDates[0]));

        bookingGlobalSeq++;
        futurePhieuHenList.Add(new PhieuDatHen
        {
            MaDatHen = $"BK{futureDates[2]:yyMMdd}-{bookingGlobalSeq:D4}",
            KenhDatLich = "Online",
            LyDoKhamSoBo = "Ù tai kéo dài",
            ThoiGianTaoPhieu = now.AddHours(-1),
            GuiNhacLich = true,
            TrangThaiHen = "DaXacNhan",
            MaBenhNhan = bn3.MaBenhNhan,
            MaNguoiTao = tkBn3.MaTaiKhoan,
            MaKhungGio = slotBn3D3.MaKhungGio
        });
        slotBn3D3.TrangThai = "DaDat";
        bookedPatientPerDay.Add((bn3.MaBenhNhan, futureDates[2]));

        // 9.2. Đổ lịch hẹn dày đặc cho các slot còn lại của 3 ngày tới
        int futureBnIdx = 3;
        foreach (var futureDate in futureDates)
        {
            var daySlots = denseKhungGios
                .Where(k => k.CaTruc.NgayTruc == futureDate && k.LoaiKhungGio == "DatHen" && k.TrangThai == "ConTrong")
                .ToList();

            // Lấy 80% slot để đặt hẹn, chừa 20% ConTrong
            int takeCount = (int)(daySlots.Count * 0.80);
            var slotsToBook = daySlots.Take(takeCount).ToList();

            foreach (var slot in slotsToBook)
            {
                // Tìm bệnh nhân chưa có lịch trong ngày này
                BenhNhan? chosenBn = null;
                for (int attempt = 0; attempt < allBn.Count; attempt++)
                {
                    var cand = allBn[(futureBnIdx + attempt) % allBn.Count];
                    if (!bookedPatientPerDay.Contains((cand.MaBenhNhan, futureDate)))
                    {
                        chosenBn = cand;
                        futureBnIdx = (futureBnIdx + attempt + 1) % allBn.Count;
                        break;
                    }
                }
                if (chosenBn == null) break;

                bookedPatientPerDay.Add((chosenBn.MaBenhNhan, futureDate));
                bookingGlobalSeq++;

                // Phân bổ trạng thái:
                // Nếu bệnh nhân có SoLanVangHen >= 2 -> ChoXacNhan
                // Nếu bệnh nhân thường: ~90% DaXacNhan, ~10% DaHuy (slot trả về ConTrong)
                string trangThaiHen;
                if (chosenBn.SoLanVangHen >= 2)
                {
                    trangThaiHen = "ChoXacNhan";
                    slot.TrangThai = "DaDat";
                }
                else if (bookingGlobalSeq % 10 == 0)
                {
                    trangThaiHen = "DaHuy";
                    slot.TrangThai = "ConTrong";
                }
                else
                {
                    trangThaiHen = "DaXacNhan";
                    slot.TrangThai = "DaDat";
                }

                futurePhieuHenList.Add(new PhieuDatHen
                {
                    MaDatHen         = $"BK{futureDate:yyMMdd}-{bookingGlobalSeq:D4}",
                    KenhDatLich      = (bookingGlobalSeq % 4 == 0) ? "DienThoai" : "Online",
                    LyDoKhamSoBo     = "Khám theo lịch hẹn trước tại chuyên khoa",
                    ThoiGianTaoPhieu = now.AddHours(-10),
                    GuiNhacLich      = true,
                    TrangThaiHen     = trangThaiHen,
                    MaBenhNhan       = chosenBn.MaBenhNhan,
                    MaNguoiTao       = (bookingGlobalSeq % 4 == 0) ? leTanUser.MaTaiKhoan : adminUser.MaTaiKhoan,
                    MaKhungGio       = slot.MaKhungGio
                });
            }
        }
        db.PhieuDatHens.AddRange(futurePhieuHenList);
        await db.SaveChangesAsync();

        // ══════════════════════════════════════════════════════════════
        // 10. ĐỔ LỊCH HẸN & HÀNG ĐỢI HÔM NAY (TODAY)
        // Phủ đủ mọi trạng thái theo trục giờ hiện tại (now)
        // Tại MỖI PHÒNG KHÁM đổ sẵn 12-14 phiếu số thứ tự đang hoạt động
        // ══════════════════════════════════════════════════════════════
        var todayPhieuHenList = new List<PhieuDatHen>();
        var todayPhieuSttList = new List<PhieuSoThuTu>();

        var warningPatients = allBn.Where(b => b.SoLanVangHen >= 2).ToList();
        int warnIdx = 0;
        int normalBnIdx = 10;

        foreach (var cfg in doctorConfigs)
        {
            var roomSlots = denseKhungGios
                .Where(k => k.CaTruc.NgayTruc == today && k.CaTruc.MaPhong == cfg.Pk.MaPhong && k.LoaiKhungGio == "DatHen")
                .OrderBy(k => k.GioBatDau)
                .ToList();

            // 1. Phục vụ 1 bệnh nhân ĐANG KHÁM (DangKham - A-001)
            var slotDangKham = roomSlots[0];
            slotDangKham.TrangThai = "DaDat";
            var bnDangKham = allBn[normalBnIdx++ % allBn.Count];
            bookingGlobalSeq++;
            var phDangKham = new PhieuDatHen
            {
                MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                KenhDatLich      = "Online",
                LyDoKhamSoBo     = "Kiểm tra triệu chứng cấp / theo dõi bệnh án",
                ThoiGianTaoPhieu = now.AddHours(-3),
                GuiNhacLich      = true,
                TrangThaiHen     = "DaCheckIn",
                MaBenhNhan       = bnDangKham.MaBenhNhan,
                MaNguoiTao       = adminUser.MaTaiKhoan,
                MaKhungGio       = slotDangKham.MaKhungGio
            };
            todayPhieuHenList.Add(phDangKham);
            bookedPatientPerDay.Add((bnDangKham.MaBenhNhan, today));

            todayPhieuSttList.Add(new PhieuSoThuTu
            {
                SoThuTu            = 1,
                NgayCapSo          = today,
                LoaiKhach          = "DatTruoc",
                MucUuTien          = 1,
                GioHen             = slotDangKham.GioBatDau,
                ThoiGianCheckIn    = now.AddMinutes(-30),
                ThoiGianGoiSo      = now.AddMinutes(-12),
                ThoiGianBatDauKham = now.AddMinutes(-10),
                SoLanGoi           = 1,
                TrangThaiPhieu     = "DangKham",
                MaPhong            = cfg.Pk.MaPhong,
                MaBenhNhan         = bnDangKham.MaBenhNhan,
                PhieuDatHen        = phDangKham
            });

            // 2. 4 khách đặt trước đúng giờ ĐANG CHỜ (A-002, A-003, A-004, A-005)
            // Trong đó A-002, A-003, A-004 có giờ hẹn sát/trong cửa sổ; A-005 có giờ hẹn xa (>30p) để kiểm tra không nhảy cóc
            for (int a = 2; a <= 5; a++)
            {
                var slotA = roomSlots[a - 1];
                slotA.TrangThai = "DaDat";
                var bnA = allBn[normalBnIdx++ % allBn.Count];
                bookingGlobalSeq++;
                var phA = new PhieuDatHen
                {
                    MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                    KenhDatLich      = "Online",
                    LyDoKhamSoBo     = $"Khám chuyên khoa theo hẹn trước lượt A-{a:D3}",
                    ThoiGianTaoPhieu = now.AddHours(-2),
                    GuiNhacLich      = true,
                    TrangThaiHen     = "DaCheckIn",
                    MaBenhNhan       = bnA.MaBenhNhan,
                    MaNguoiTao       = adminUser.MaTaiKhoan,
                    MaKhungGio       = slotA.MaKhungGio
                };
                todayPhieuHenList.Add(phA);
                bookedPatientPerDay.Add((bnA.MaBenhNhan, today));

                // A-005 đặt giờ hẹn cách hiện tại 45 phút (đến sớm > 30 phút)
                TimeOnly? gioHenA = (a == 5)
                    ? TimeOnly.FromDateTime(now.AddMinutes(45))
                    : slotA.GioBatDau;

                todayPhieuSttList.Add(new PhieuSoThuTu
                {
                    SoThuTu         = a,
                    NgayCapSo       = today,
                    LoaiKhach       = "DatTruoc",
                    MucUuTien       = 1,
                    GioHen          = gioHenA,
                    ThoiGianCheckIn = now.AddMinutes(-20 + a),
                    SoLanGoi        = 0,
                    TrangThaiPhieu  = "DangCho",
                    MaPhong         = cfg.Pk.MaPhong,
                    MaBenhNhan      = bnA.MaBenhNhan,
                    PhieuDatHen     = phA
                });
            }

            // 3. 4 khách vãng lai ĐANG CHỜ (W-001 đến W-004)
            for (int w = 1; w <= 4; w++)
            {
                var bnW = allBn[normalBnIdx++ % allBn.Count];
                todayPhieuSttList.Add(new PhieuSoThuTu
                {
                    SoThuTu         = w,
                    NgayCapSo       = today,
                    LoaiKhach       = "VangLai",
                    MucUuTien       = 2,
                    GioHen          = null,
                    ThoiGianCheckIn = now.AddMinutes(-15 + w * 2),
                    SoLanGoi        = 0,
                    TrangThaiPhieu  = "DangCho",
                    MaPhong         = cfg.Pk.MaPhong,
                    MaBenhNhan      = bnW.MaBenhNhan,
                    PhieuDatHen     = null
                });
            }

            // 4. 2 khách đặt trước đến trễ ĐANG CHỜ (L-001, L-002, MucUuTien = 3)
            for (int l = 1; l <= 2; l++)
            {
                var slotL = roomSlots[5 + l];
                slotL.TrangThai = "DaDat";
                var bnL = allBn[normalBnIdx++ % allBn.Count];
                bookingGlobalSeq++;
                var phL = new PhieuDatHen
                {
                    MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                    KenhDatLich      = "Online",
                    LyDoKhamSoBo     = $"Khách đến trễ lượt L-{l:D3}",
                    ThoiGianTaoPhieu = now.AddHours(-3),
                    GuiNhacLich      = true,
                    TrangThaiHen     = "DaCheckIn",
                    MaBenhNhan       = bnL.MaBenhNhan,
                    MaNguoiTao       = adminUser.MaTaiKhoan,
                    MaKhungGio       = slotL.MaKhungGio
                };
                todayPhieuHenList.Add(phL);
                bookedPatientPerDay.Add((bnL.MaBenhNhan, today));

                todayPhieuSttList.Add(new PhieuSoThuTu
                {
                    SoThuTu         = l,
                    NgayCapSo       = today,
                    LoaiKhach       = "DatTruocTre",
                    MucUuTien       = 3,
                    GioHen          = TimeOnly.FromDateTime(now.AddMinutes(-25 + l * 3)),
                    ThoiGianCheckIn = now.AddMinutes(-6 + l),
                    SoLanGoi        = 0,
                    TrangThaiPhieu  = "DangCho",
                    MaPhong         = cfg.Pk.MaPhong,
                    MaBenhNhan      = bnL.MaBenhNhan,
                    PhieuDatHen     = phL
                });
            }

            // 5. 2 bệnh nhân ĐÃ BỊ QUA LƯỢT (A-006, W-005 - SoLanGoi = 3, QuaLuot)
            // Dùng để test chức năng Lễ tân bấm Khôi phục lượt khám
            var slotQ = roomSlots[8];
            slotQ.TrangThai = "DaDat";
            var bnQ1 = allBn[normalBnIdx++ % allBn.Count];
            bookingGlobalSeq++;
            var phQ1 = new PhieuDatHen
            {
                MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                KenhDatLich      = "Online",
                LyDoKhamSoBo     = "Bệnh nhân vắng mặt khi gọi số, bị chuyển qua lượt",
                ThoiGianTaoPhieu = now.AddHours(-4),
                GuiNhacLich      = true,
                TrangThaiHen     = "DaCheckIn",
                MaBenhNhan       = bnQ1.MaBenhNhan,
                MaNguoiTao       = adminUser.MaTaiKhoan,
                MaKhungGio       = slotQ.MaKhungGio
            };
            todayPhieuHenList.Add(phQ1);
            bookedPatientPerDay.Add((bnQ1.MaBenhNhan, today));

            todayPhieuSttList.Add(new PhieuSoThuTu
            {
                SoThuTu         = 6, // A-006
                NgayCapSo       = today,
                LoaiKhach       = "DatTruoc",
                MucUuTien       = 1,
                GioHen          = slotQ.GioBatDau,
                ThoiGianCheckIn = now.AddMinutes(-45),
                ThoiGianGoiSo   = now.AddMinutes(-18),
                SoLanGoi        = 3,
                TrangThaiPhieu  = "QuaLuot",
                MaPhong         = cfg.Pk.MaPhong,
                MaBenhNhan      = bnQ1.MaBenhNhan,
                PhieuDatHen     = phQ1
            });

            var bnQ2 = allBn[normalBnIdx++ % allBn.Count];
            todayPhieuSttList.Add(new PhieuSoThuTu
            {
                SoThuTu         = 5, // W-005
                NgayCapSo       = today,
                LoaiKhach       = "VangLai",
                MucUuTien       = 2,
                GioHen          = null,
                ThoiGianCheckIn = now.AddMinutes(-40),
                ThoiGianGoiSo   = now.AddMinutes(-16),
                SoLanGoi        = 3,
                TrangThaiPhieu  = "QuaLuot",
                MaPhong         = cfg.Pk.MaPhong,
                MaBenhNhan      = bnQ2.MaBenhNhan,
                PhieuDatHen     = null
            });

            // 6. Cố ý 1 phiếu DaXacNhan TRỄ > 30 PHÚT CHƯA CHECK-IN trên mỗi phòng (tổng 4 phiếu)
            // Dùng để test nút "Quét quá hạn ngay" và VangHenBackgroundService
            var slotOverdue = roomSlots[9];
            slotOverdue.TrangThai = "DaDat";
            var bnOverdue = allBn[normalBnIdx++ % allBn.Count];
            bookingGlobalSeq++;
            todayPhieuHenList.Add(new PhieuDatHen
            {
                MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                KenhDatLich      = "Online",
                LyDoKhamSoBo     = "Phiếu hẹn quá 30 phút chưa check-in để test quét vắng hẹn",
                ThoiGianTaoPhieu = now.AddHours(-4),
                GuiNhacLich      = true,
                TrangThaiHen     = "DaXacNhan", // Chưa check-in!
                MaBenhNhan       = bnOverdue.MaBenhNhan,
                MaNguoiTao       = adminUser.MaTaiKhoan,
                MaKhungGio       = slotOverdue.MaKhungGio
            });
            bookedPatientPerDay.Add((bnOverdue.MaBenhNhan, today));

            // 7. Cố ý 2 phiếu ChoXacNhan cho bệnh nhân CẢNH BÁO ĐỎ (SoLanVangHen >= 2) hôm nay
            // Dùng để test hiển thị badge cảnh báo đỏ và nút Duyệt của Lễ tân
            for (int w = 0; w < 2; w++)
            {
                var slotWarn = roomSlots[10 + w];
                slotWarn.TrangThai = "DaDat";
                var bnWarn = warningPatients[warnIdx++ % warningPatients.Count];
                bookingGlobalSeq++;
                todayPhieuHenList.Add(new PhieuDatHen
                {
                    MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                    KenhDatLich      = "Online",
                    LyDoKhamSoBo     = $"Bệnh nhân có cờ cảnh báo đặt hẹn (SoLanVangHen={bnWarn.SoLanVangHen})",
                    ThoiGianTaoPhieu = now.AddHours(-1),
                    GuiNhacLich      = true,
                    TrangThaiHen     = "ChoXacNhan", // Cần Lễ tân duyệt!
                    MaBenhNhan       = bnWarn.MaBenhNhan,
                    MaNguoiTao       = adminUser.MaTaiKhoan,
                    MaKhungGio       = slotWarn.MaKhungGio
                });
                bookedPatientPerDay.Add((bnWarn.MaBenhNhan, today));
            }

            // 8. Đổ thêm các phiếu DaKham đã hoàn tất trong ca sáng/chiều hôm nay
            for (int d = 12; d < Math.Min(18, roomSlots.Count); d++)
            {
                var slotPast = roomSlots[d];
                slotPast.TrangThai = "DaDat";
                var bnPast = allBn[normalBnIdx++ % allBn.Count];
                bookingGlobalSeq++;
                var phPast = new PhieuDatHen
                {
                    MaDatHen         = $"BK{today:yyMMdd}-{bookingGlobalSeq:D4}",
                    KenhDatLich      = "Online",
                    LyDoKhamSoBo     = "Khám hoàn tất ca trong ngày",
                    ThoiGianTaoPhieu = now.AddHours(-5),
                    GuiNhacLich      = true,
                    TrangThaiHen     = "DaKham",
                    MaBenhNhan       = bnPast.MaBenhNhan,
                    MaNguoiTao       = adminUser.MaTaiKhoan,
                    MaKhungGio       = slotPast.MaKhungGio
                };
                todayPhieuHenList.Add(phPast);
                bookedPatientPerDay.Add((bnPast.MaBenhNhan, today));

                todayPhieuSttList.Add(new PhieuSoThuTu
                {
                    SoThuTu            = 10 + d,
                    NgayCapSo          = today,
                    LoaiKhach          = "DatTruoc",
                    MucUuTien          = 1,
                    GioHen             = slotPast.GioBatDau,
                    ThoiGianCheckIn    = today.ToDateTime(slotPast.GioBatDau).AddMinutes(-10),
                    ThoiGianGoiSo      = today.ToDateTime(slotPast.GioBatDau).AddMinutes(5),
                    ThoiGianBatDauKham = today.ToDateTime(slotPast.GioBatDau).AddMinutes(7),
                    ThoiGianHoanTat    = today.ToDateTime(slotPast.GioBatDau).AddMinutes(22),
                    SoLanGoi           = 1,
                    TrangThaiPhieu     = "HoanTat",
                    MaPhong            = cfg.Pk.MaPhong,
                    MaBenhNhan         = bnPast.MaBenhNhan,
                    PhieuDatHen        = phPast
                });
            }

            // 9. Để lại 1-2 slot TamKhoa để test mở khóa
            if (roomSlots.Count > 19)
            {
                roomSlots[18].TrangThai = "TamKhoa";
            }
        }

        db.PhieuDatHens.AddRange(todayPhieuHenList);
        await db.SaveChangesAsync();

        db.PhieuSoThuTus.AddRange(todayPhieuSttList);
        await db.SaveChangesAsync();

        // Đồng bộ cập nhật lại các KhungGio có trạng thái DaDat / TamKhoa
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Xóa toàn bộ dữ liệu trong Database theo thứ tự ngược khóa ngoại và Reset Identity về 0.
    /// </summary>
    public static async Task ClearAllDataAsync(ClinicDbContext db)
    {
        var sql = @"
            DELETE FROM PhieuSoThuTu;
            DELETE FROM PhieuDatHen;
            DELETE FROM KhungGio;
            DELETE FROM CaTruc;
            DELETE FROM BenhNhan;
            DELETE FROM BacSi;
            DELETE FROM PhongKham;
            DELETE FROM ChuyenKhoa;
            DELETE FROM TaiKhoan;
            DELETE FROM VaiTro;

            DBCC CHECKIDENT ('PhieuSoThuTu', RESEED, 0);
            DBCC CHECKIDENT ('PhieuDatHen', RESEED, 0);
            DBCC CHECKIDENT ('KhungGio', RESEED, 0);
            DBCC CHECKIDENT ('CaTruc', RESEED, 0);
            DBCC CHECKIDENT ('BenhNhan', RESEED, 0);
            DBCC CHECKIDENT ('BacSi', RESEED, 0);
            DBCC CHECKIDENT ('PhongKham', RESEED, 0);
            DBCC CHECKIDENT ('ChuyenKhoa', RESEED, 0);
            DBCC CHECKIDENT ('TaiKhoan', RESEED, 0);
            DBCC CHECKIDENT ('VaiTro', RESEED, 0);
        ";
        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
