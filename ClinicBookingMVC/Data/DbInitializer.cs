using Microsoft.EntityFrameworkCore;
using ClinicBookingMVC.Models.Entities;

namespace ClinicBookingMVC.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ClinicDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        // 1. Seed VaiTro (4 vai trò cốt lõi)
        var roles = new[] { "Admin", "LeTan", "BacSi", "BenhNhan" };
        foreach (var r in roles)
        {
            if (!await db.VaiTros.AnyAsync(v => v.TenVaiTro == r))
            {
                db.VaiTros.Add(new VaiTro { TenVaiTro = r, MoTa = $"Vai trò {r}" });
            }
        }
        await db.SaveChangesAsync();

        var roleAdmin    = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "Admin");
        var roleLeTan    = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "LeTan");
        var roleBacSi    = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "BacSi");
        var roleBenhNhan = await db.VaiTros.FirstAsync(v => v.TenVaiTro == "BenhNhan");

        // 2. Seed Tài khoản demo mật khẩu chung 123456
        var hash123456 = BCrypt.Net.BCrypt.HashPassword("123456");

        var demoAccounts = new (string Username, int RoleId, string Phone)[]
        {
            ("admin",      roleAdmin.MaVaiTro,    "0900000001"),
            ("letan01",    roleLeTan.MaVaiTro,    "0900000002"),
            ("bacsi01",    roleBacSi.MaVaiTro,    "0901234561"),
            ("bacsi02",    roleBacSi.MaVaiTro,    "0901234562"),
            ("bacsi03",    roleBacSi.MaVaiTro,    "0901234563"),
            ("benhnhan01", roleBenhNhan.MaVaiTro, "0912345671"),
            ("benhnhan02", roleBenhNhan.MaVaiTro, "0912345672"),
        };

        foreach (var acc in demoAccounts)
        {
            var existing = await db.TaiKhoans.FirstOrDefaultAsync(t => t.TenTaiKhoan == acc.Username);
            if (existing == null)
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
            else
            {
                // Cập nhật lại mật khẩu chuẩn 123456 cho demo
                existing.MatKhau = hash123456;
            }
        }
        await db.SaveChangesAsync();

        // 3. Seed 3 Chuyên khoa
        var chuyenKhoas = new (string Ten, string MoTa)[]
        {
            ("Nội tổng quát", "Khám và điều trị các bệnh lý nội khoa người lớn, tim mạch, tiêu hóa, hô hấp"),
            ("Tai Mũi Họng",  "Khám, nội soi và điều trị các bệnh lý tai mũi họng người lớn và trẻ em"),
            ("Da liễu",       "Điều trị các bệnh ngoài da, dị ứng, viêm da cơ địa và thẩm mỹ y khoa"),
        };

        foreach (var ck in chuyenKhoas)
        {
            if (!await db.ChuyenKhoas.AnyAsync(c => c.TenChuyenKhoa == ck.Ten))
            {
                db.ChuyenKhoas.Add(new ChuyenKhoa
                {
                    TenChuyenKhoa = ck.Ten,
                    MoTa          = ck.MoTa,
                    TrangThai     = "HoatDong"
                });
            }
        }
        await db.SaveChangesAsync();

        var ckNoi = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Nội tổng quát");
        var ckTmh = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Tai Mũi Họng");
        var ckDa  = await db.ChuyenKhoas.FirstAsync(c => c.TenChuyenKhoa == "Da liễu");

        // 4. Seed 3 Phòng khám
        var phongKhams = new (string Ten, string ViTri, int MaCk)[]
        {
            ("Phòng 101", "Tầng 1 - Khu A", ckNoi.MaChuyenKhoa),
            ("Phòng 102", "Tầng 1 - Khu A", ckTmh.MaChuyenKhoa),
            ("Phòng 103", "Tầng 2 - Khu B", ckDa.MaChuyenKhoa),
        };

        foreach (var pk in phongKhams)
        {
            if (!await db.PhongKhams.AnyAsync(p => p.TenPhong == pk.Ten))
            {
                db.PhongKhams.Add(new PhongKham
                {
                    TenPhong     = pk.Ten,
                    ViTri        = pk.ViTri,
                    MaChuyenKhoa = pk.MaCk,
                    TrangThai    = "HoatDong"
                });
            }
        }
        await db.SaveChangesAsync();

        var p101 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 101");
        var p102 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 102");
        var p103 = await db.PhongKhams.FirstAsync(p => p.TenPhong == "Phòng 103");

        // 5. Seed 3 Bác sĩ gắn với 3 tài khoản & 3 chuyên khoa
        var tkBs1 = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi01");
        var tkBs2 = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi02");
        var tkBs3 = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "bacsi03");

        var bacSis = new (string HoTen, string HocVi, string Phone, int MaCk, int MaTk)[]
        {
            ("Nguyễn Văn An",  "BS.CKI", "0901234561", ckNoi.MaChuyenKhoa, tkBs1.MaTaiKhoan),
            ("Trần Thị Bình",  "ThS.BS",  "0901234562", ckTmh.MaChuyenKhoa, tkBs2.MaTaiKhoan),
            ("Lê Hoàng Cường", "BS.CKI", "0901234563", ckDa.MaChuyenKhoa,  tkBs3.MaTaiKhoan),
        };

        foreach (var bs in bacSis)
        {
            if (!await db.BacSis.AnyAsync(b => b.MaTaiKhoan == bs.MaTk))
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
        }
        await db.SaveChangesAsync();

        var bsAn    = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs1.MaTaiKhoan);
        var bsBinh  = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs2.MaTaiKhoan);
        var bsCuong = await db.BacSis.FirstAsync(b => b.MaTaiKhoan == tkBs3.MaTaiKhoan);

        // 6. Seed danh sách Bệnh nhân (gồm cả 2 bệnh nhân có CoCanhBao = true)
        if (!await db.BenhNhans.AnyAsync())
        {
            var dsBenhNhan = new List<BenhNhan>
            {
                // 2 Bệnh nhân cảnh báo vắng hẹn >= 2 lần
                new BenhNhan { TenBenhNhan = "Hoàng Văn Cảnh Báo", SoDienThoai = "0988000111", NamSinh = 1988, GioiTinh = "Nam", Email = "canhbao@gmail.com", SoLanVangHen = 3, CoCanhBao = true },
                new BenhNhan { TenBenhNhan = "Trần Thị Vắng",       SoDienThoai = "0977222333", NamSinh = 1995, GioiTinh = "Nu",  Email = "vanghen@gmail.com", SoLanVangHen = 2, CoCanhBao = true },

                // Các bệnh nhân bình thường
                new BenhNhan { TenBenhNhan = "Nguyễn Văn Nam",   SoDienThoai = "0912345671", NamSinh = 1990, GioiTinh = "Nam", Email = "nam.nguyen@gmail.com", SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Lê Thị Mai",       SoDienThoai = "0912345672", NamSinh = 1994, GioiTinh = "Nu",  Email = "mai.le@gmail.com",     SoLanVangHen = 1, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Phạm Quốc Dũng",   SoDienThoai = "0912345673", NamSinh = 1985, GioiTinh = "Nam", Email = "dung.pham@gmail.com",   SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Đỗ Thu Hà",        SoDienThoai = "0912345674", NamSinh = 1998, GioiTinh = "Nu",  Email = "ha.do@gmail.com",      SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Vũ Minh Khôi",     SoDienThoai = "0912345675", NamSinh = 2000, GioiTinh = "Nam", Email = "khoi.vu@gmail.com",    SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Phan Thị Nga",     SoDienThoai = "0912345676", NamSinh = 1992, GioiTinh = "Nu",  Email = "nga.phan@gmail.com",   SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Bùi Văn Phúc",     SoDienThoai = "0912345677", NamSinh = 1982, GioiTinh = "Nam", Email = "phuc.bui@gmail.com",   SoLanVangHen = 1, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Đinh Thị Quỳnh",   SoDienThoai = "0912345678", NamSinh = 1997, GioiTinh = "Nu",  Email = "quynh.dinh@gmail.com", SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Trịnh Gia Bảo",    SoDienThoai = "0912345679", NamSinh = 2003, GioiTinh = "Nam", Email = "bao.trinh@gmail.com",  SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Ngô Mỹ Linh",      SoDienThoai = "0912345680", NamSinh = 1996, GioiTinh = "Nu",  Email = "linh.ngo@gmail.com",   SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Võ Hoài Nam",      SoDienThoai = "0912345681", NamSinh = 1989, GioiTinh = "Nam", Email = "nam.vo@gmail.com",     SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Dương Thùy Trang", SoDienThoai = "0912345682", NamSinh = 2001, GioiTinh = "Nu",  Email = "trang.duong@gmail.com",SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Lý Minh Triết",    SoDienThoai = "0912345683", NamSinh = 1978, GioiTinh = "Nam", Email = "triet.ly@gmail.com",   SoLanVangHen = 1, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Đoàn Phương Thảo", SoDienThoai = "0912345684", NamSinh = 1993, GioiTinh = "Nu",  Email = "thao.doan@gmail.com",  SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Lương Thế Vinh",   SoDienThoai = "0912345685", NamSinh = 1986, GioiTinh = "Nam", Email = "vinh.luong@gmail.com", SoLanVangHen = 0, CoCanhBao = false },
                new BenhNhan { TenBenhNhan = "Tạ Bích Diệp",     SoDienThoai = "0912345686", NamSinh = 1999, GioiTinh = "Nu",  Email = "diep.ta@gmail.com",    SoLanVangHen = 0, CoCanhBao = false },
            };

            db.BenhNhans.AddRange(dsBenhNhan);
            await db.SaveChangesAsync();
        }

        var allBn = await db.BenhNhans.ToListAsync();
        var adminUser = await db.TaiKhoans.FirstAsync(t => t.TenTaiKhoan == "admin");

        // 7. Seed Lịch sử 14 ngày trước + Hôm nay + 5 ngày tới
        if (!await db.CaTrucs.AnyAsync())
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var doctorConfigs = new (BacSi Bs, PhongKham Pk)[]
            {
                (bsAn,    p101),
                (bsBinh,  p102),
                (bsCuong, p103),
            };

            int bookingSeq = 100;
            var listPhieuDatHen = new List<PhieuDatHen>();
            var listPhieuSoThuTu = new List<PhieuSoThuTu>();

            // Duyệt từ -14 ngày đến +5 ngày
            for (int dayOffset = -14; dayOffset <= 5; dayOffset++)
            {
                var targetDate = today.AddDays(dayOffset);
                bool isPast = dayOffset < 0;
                bool isToday = dayOffset == 0;
                bool isFuture = dayOffset > 0;

                foreach (var cfg in doctorConfigs)
                {
                    // Mỗi ngày tạo 2 ca: Sang (07:30 - 11:30) và Chieu (13:30 - 17:30)
                    var caSang = new CaTruc
                    {
                        MaBacSi               = cfg.Bs.MaBacSi,
                        MaPhong               = cfg.Pk.MaPhong,
                        NgayTruc              = targetDate,
                        CaLamViec             = "Sang",
                        GioBatDauCa           = new TimeOnly(7, 30),
                        GioKetThucCa          = new TimeOnly(11, 30),
                        ThoiLuongMoiKhungGio  = 30,
                        TrangThaiCa           = "MoKham"
                    };

                    var caChieu = new CaTruc
                    {
                        MaBacSi               = cfg.Bs.MaBacSi,
                        MaPhong               = cfg.Pk.MaPhong,
                        NgayTruc              = targetDate,
                        CaLamViec             = "Chieu",
                        GioBatDauCa           = new TimeOnly(13, 30),
                        GioKetThucCa          = new TimeOnly(17, 30),
                        ThoiLuongMoiKhungGio  = 30,
                        TrangThaiCa           = "MoKham"
                    };

                    db.CaTrucs.AddRange(caSang, caChieu);
                    await db.SaveChangesAsync();

                    // Sinh khung giờ cho Ca Sáng: 8 khung giờ (mỗi 30 phút, cứ 3 DatHen thì 1 KhungDem)
                    var listKhungGio = new List<KhungGio>();
                    var curTime = caSang.GioBatDauCa;
                    int slotIndex = 0;
                    while (curTime < caSang.GioKetThucCa)
                    {
                        slotIndex++;
                        bool isKhungDem = (slotIndex % 4 == 0); // cứ mỗi 3 DatHen thì 1 KhungDem
                        var endTime = curTime.AddMinutes(30);
                        listKhungGio.Add(new KhungGio
                        {
                            MaCa         = caSang.MaCa,
                            GioBatDau    = curTime,
                            GioKetThuc   = endTime,
                            LoaiKhungGio = isKhungDem ? "KhungDem" : "DatHen",
                            TrangThai    = "ConTrong"
                        });
                        curTime = endTime;
                    }

                    // Ca Chiều: 8 khung giờ
                    curTime = caChieu.GioBatDauCa;
                    slotIndex = 0;
                    while (curTime < caChieu.GioKetThucCa)
                    {
                        slotIndex++;
                        bool isKhungDem = (slotIndex % 4 == 0);
                        var endTime = curTime.AddMinutes(30);
                        listKhungGio.Add(new KhungGio
                        {
                            MaCa         = caChieu.MaCa,
                            GioBatDau    = curTime,
                            GioKetThuc   = endTime,
                            LoaiKhungGio = isKhungDem ? "KhungDem" : "DatHen",
                            TrangThai    = "ConTrong"
                        });
                        curTime = endTime;
                    }

                    db.KhungGios.AddRange(listKhungGio);
                    await db.SaveChangesAsync();

                    // ── NẠP DỮ LIỆU LỊCH SỬ (14 ngày qua) ──
                    if (isPast)
                    {
                        // Lấy các khung giờ loại DatHen để tạo phiếu hẹn
                        var datHenSlots = listKhungGio.Where(k => k.LoaiKhungGio == "DatHen").Take(3).ToList();
                        int bnIdx = (Math.Abs(dayOffset) * 3 + cfg.Bs.MaBacSi) % allBn.Count;

                        for (int i = 0; i < datHenSlots.Count; i++)
                        {
                            var slot = datHenSlots[i];
                            var bn = allBn[(bnIdx + i) % allBn.Count];
                            bookingSeq++;
                            var maDatHen = $"BK{targetDate:yyMMdd}-{bookingSeq:D3}";

                            // Phân bổ trạng thái: ~70% DaKham, ~15% VangHen, ~10% DaHuy, ~5% DaXacNhan
                            string trangThaiHen;
                            int modStatus = (bookingSeq % 10);
                            if (modStatus < 7)       trangThaiHen = "DaKham";
                            else if (modStatus < 9)  trangThaiHen = "VangHen";
                            else                     trangThaiHen = "DaHuy";

                            slot.TrangThai = (trangThaiHen == "DaHuy") ? "ConTrong" : "DaDat";

                            var phieuHen = new PhieuDatHen
                            {
                                MaDatHen         = maDatHen,
                                KenhDatLich      = "Online",
                                LyDoKhamSoBo     = "Khám định kỳ tổng quát / tái khám",
                                ThoiGianTaoPhieu = targetDate.ToDateTime(new TimeOnly(8, 0)).AddDays(-1),
                                GuiNhacLich      = true,
                                TrangThaiHen     = trangThaiHen,
                                MaBenhNhan       = bn.MaBenhNhan,
                                MaNguoiTao       = adminUser.MaTaiKhoan,
                                MaKhungGio       = slot.MaKhungGio
                            };
                            db.PhieuDatHens.Add(phieuHen);
                            await db.SaveChangesAsync();

                            // Nếu DaKham: tạo PhieuSoThuTu tương ứng đã hoàn tất
                            if (trangThaiHen == "DaKham")
                            {
                                var checkinTime = targetDate.ToDateTime(slot.GioBatDau).AddMinutes(-10);
                                var waitMinutes = 10 + (bookingSeq % 15); // chờ từ 10 - 24 phút
                                var goisoTime   = checkinTime.AddMinutes(waitMinutes);
                                var khamTime    = goisoTime.AddMinutes(2);
                                var xongTime    = khamTime.AddMinutes(15);

                                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                                {
                                    SoThuTu             = (i + 1),
                                    NgayCapSo           = targetDate,
                                    LoaiKhach           = "DatTruoc",
                                    MucUuTien           = 1,
                                    GioHen              = slot.GioBatDau,
                                    ThoiGianCheckIn     = checkinTime,
                                    ThoiGianGoiSo       = goisoTime,
                                    ThoiGianBatDauKham  = khamTime,
                                    ThoiGianHoanTat     = xongTime,
                                    SoLanGoi            = 1,
                                    TrangThaiPhieu      = "HoanTat",
                                    MaPhong             = cfg.Pk.MaPhong,
                                    MaBenhNhan          = bn.MaBenhNhan,
                                    MaPhieuHen          = phieuHen.MaPhieuHen
                                });
                                await db.SaveChangesAsync();
                            }
                        }
                    }

                    // ── NẠP DỮ LIỆU TƯƠNG LAI (Ngày mai đến +5 ngày) ──
                    if (isFuture)
                    {
                        var slots = listKhungGio.Where(k => k.LoaiKhungGio == "DatHen").Take(2).ToList();
                        int bnIdx = (dayOffset * 2 + cfg.Bs.MaBacSi) % allBn.Count;

                        for (int i = 0; i < slots.Count; i++)
                        {
                            var slot = slots[i];
                            var bn = allBn[(bnIdx + i) % allBn.Count];
                            bookingSeq++;
                            var maDatHen = $"BK{targetDate:yyMMdd}-{bookingSeq:D3}";

                            // Nếu là ngày mai (dayOffset == 1), để 1 phiếu GuiNhacLich = false để test service nhắc lịch
                            bool guiNhac = !(dayOffset == 1 && i == 0);

                            slot.TrangThai = "DaDat";
                            var phieuHen = new PhieuDatHen
                            {
                                MaDatHen         = maDatHen,
                                KenhDatLich      = "Online",
                                LyDoKhamSoBo     = "Đặt hẹn trước khám chuyên khoa",
                                ThoiGianTaoPhieu = DateTime.Now.AddHours(-3),
                                GuiNhacLich      = guiNhac,
                                TrangThaiHen     = "DaXacNhan",
                                MaBenhNhan       = bn.MaBenhNhan,
                                MaNguoiTao       = adminUser.MaTaiKhoan,
                                MaKhungGio       = slot.MaKhungGio
                            };
                            db.PhieuDatHens.Add(phieuHen);
                            await db.SaveChangesAsync();
                        }
                    }
                }
            }

            // ── 8. NẠP DỮ LIỆU HÔM NAY (TODAY): SẴN 10 BỆNH NHÂN CHỜ & CẢNH BÁO ──
            var caTrucHomNay = await db.CaTrucs
                .Include(c => c.KhungGios)
                .Where(c => c.NgayTruc == today && c.CaLamViec == "Sang")
                .ToListAsync();

            var caP101 = caTrucHomNay.FirstOrDefault(c => c.MaPhong == p101.MaPhong);
            var caP102 = caTrucHomNay.FirstOrDefault(c => c.MaPhong == p102.MaPhong);
            var caP103 = caTrucHomNay.FirstOrDefault(c => c.MaPhong == p103.MaPhong);

            var bnCanhBao1 = allBn.First(b => b.SoLanVangHen >= 3);
            var bnCanhBao2 = allBn.First(b => b.SoLanVangHen == 2);
            var now = DateTime.Now;

            // Phòng 101 (Nội tổng quát): Tạo 5 bệnh nhân (1 Đang khám, 1 Đang gọi, 3 Đang chờ)
            if (caP101 != null)
            {
                var slots101 = caP101.KhungGios.Where(k => k.LoaiKhungGio == "DatHen").ToList();

                // Phiếu 1: A-001 (Hoàng Văn Cảnh Báo - có cờ cảnh báo!) - Đang khám
                slots101[0].TrangThai = "DaDat";
                var ph1 = new PhieuDatHen
                {
                    MaDatHen = $"BK{today:yyMMdd}-001",
                    KenhDatLich = "Online",
                    LyDoKhamSoBo = "Đau tức ngực trái kéo dài, khó thở khi gắng sức",
                    ThoiGianTaoPhieu = now.AddHours(-2),
                    GuiNhacLich = true,
                    TrangThaiHen = "DaCheckIn",
                    MaBenhNhan = bnCanhBao1.MaBenhNhan,
                    MaNguoiTao = adminUser.MaTaiKhoan,
                    MaKhungGio = slots101[0].MaKhungGio
                };
                db.PhieuDatHens.Add(ph1);
                await db.SaveChangesAsync();

                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // A-001
                    NgayCapSo = today,
                    LoaiKhach = "DatTruoc",
                    MucUuTien = 1,
                    GioHen = slots101[0].GioBatDau,
                    ThoiGianCheckIn = now.AddMinutes(-35),
                    ThoiGianGoiSo = now.AddMinutes(-10),
                    ThoiGianBatDauKham = now.AddMinutes(-8),
                    SoLanGoi = 1,
                    TrangThaiPhieu = "DangKham",
                    MaPhong = p101.MaPhong,
                    MaBenhNhan = bnCanhBao1.MaBenhNhan,
                    MaPhieuHen = ph1.MaPhieuHen
                });

                // Phiếu 2: A-002 - Đang gọi (đã gọi 1 lần -> Bác sĩ có thể bấm Gọi lại hoặc Bắt đầu khám)
                slots101[1].TrangThai = "DaDat";
                var ph2 = new PhieuDatHen
                {
                    MaDatHen = $"BK{today:yyMMdd}-002",
                    KenhDatLich = "Online",
                    LyDoKhamSoBo = "Tăng huyết áp định kỳ, chóng mặt hoa mắt",
                    ThoiGianTaoPhieu = now.AddHours(-1),
                    GuiNhacLich = true,
                    TrangThaiHen = "DaCheckIn",
                    MaBenhNhan = allBn[2].MaBenhNhan,
                    MaNguoiTao = adminUser.MaTaiKhoan,
                    MaKhungGio = slots101[1].MaKhungGio
                };
                db.PhieuDatHens.Add(ph2);
                await db.SaveChangesAsync();

                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 2, // A-002
                    NgayCapSo = today,
                    LoaiKhach = "DatTruoc",
                    MucUuTien = 1,
                    GioHen = slots101[1].GioBatDau,
                    ThoiGianCheckIn = now.AddMinutes(-20),
                    ThoiGianGoiSo = now.AddMinutes(-1),
                    SoLanGoi = 1,
                    TrangThaiPhieu = "DangGoi",
                    MaPhong = p101.MaPhong,
                    MaBenhNhan = allBn[2].MaBenhNhan,
                    MaPhieuHen = ph2.MaPhieuHen
                });

                // Phiếu 3: A-003 - Đang chờ (Ưu tiên 1)
                slots101[2].TrangThai = "DaDat";
                var ph3 = new PhieuDatHen
                {
                    MaDatHen = $"BK{today:yyMMdd}-003",
                    KenhDatLich = "Online",
                    LyDoKhamSoBo = "Kiểm tra đường huyết và mỡ máu",
                    ThoiGianTaoPhieu = now.AddHours(-1),
                    GuiNhacLich = true,
                    TrangThaiHen = "DaCheckIn",
                    MaBenhNhan = allBn[3].MaBenhNhan,
                    MaNguoiTao = adminUser.MaTaiKhoan,
                    MaKhungGio = slots101[2].MaKhungGio
                };
                db.PhieuDatHens.Add(ph3);
                await db.SaveChangesAsync();

                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 3, // A-003
                    NgayCapSo = today,
                    LoaiKhach = "DatTruoc",
                    MucUuTien = 1,
                    GioHen = slots101[2].GioBatDau,
                    ThoiGianCheckIn = now.AddMinutes(-15),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p101.MaPhong,
                    MaBenhNhan = allBn[3].MaBenhNhan,
                    MaPhieuHen = ph3.MaPhieuHen
                });

                // Phiếu 4: W-001 - Khách vãng lai walk-in (Ưu tiên 2)
                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // W-001
                    NgayCapSo = today,
                    LoaiKhach = "VangLai",
                    MucUuTien = 2,
                    GioHen = null,
                    ThoiGianCheckIn = now.AddMinutes(-12),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p101.MaPhong,
                    MaBenhNhan = allBn[4].MaBenhNhan,
                    MaPhieuHen = null
                });

                // Phiếu 5: L-001 - Đặt trước đến trễ (Ưu tiên 3)
                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // L-001
                    NgayCapSo = today,
                    LoaiKhach = "DatTruocTre",
                    MucUuTien = 3,
                    GioHen = slots101[3].GioBatDau,
                    ThoiGianCheckIn = now.AddMinutes(-5),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p101.MaPhong,
                    MaBenhNhan = allBn[5].MaBenhNhan,
                    MaPhieuHen = null
                });
            }

            // Phòng 102 (Tai Mũi Họng): 3 bệnh nhân (kèm Trần Thị Vắng - Cảnh báo 2 lần!)
            if (caP102 != null)
            {
                var slots102 = caP102.KhungGios.Where(k => k.LoaiKhungGio == "DatHen").ToList();

                slots102[0].TrangThai = "DaDat";
                var ph4 = new PhieuDatHen
                {
                    MaDatHen = $"BK{today:yyMMdd}-004",
                    KenhDatLich = "Online",
                    LyDoKhamSoBo = "Đau họng sốt nhẹ, nuốt vướng 3 ngày nay",
                    ThoiGianTaoPhieu = now.AddHours(-1),
                    GuiNhacLich = true,
                    TrangThaiHen = "DaCheckIn",
                    MaBenhNhan = bnCanhBao2.MaBenhNhan,
                    MaNguoiTao = adminUser.MaTaiKhoan,
                    MaKhungGio = slots102[0].MaKhungGio
                };
                db.PhieuDatHens.Add(ph4);
                await db.SaveChangesAsync();

                // A-001 phòng 102 (Cảnh báo!) - Đang chờ
                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // A-001
                    NgayCapSo = today,
                    LoaiKhach = "DatTruoc",
                    MucUuTien = 1,
                    GioHen = slots102[0].GioBatDau,
                    ThoiGianCheckIn = now.AddMinutes(-20),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p102.MaPhong,
                    MaBenhNhan = bnCanhBao2.MaBenhNhan,
                    MaPhieuHen = ph4.MaPhieuHen
                });

                // W-001 phòng 102 (Vãng lai)
                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // W-001
                    NgayCapSo = today,
                    LoaiKhach = "VangLai",
                    MucUuTien = 2,
                    GioHen = null,
                    ThoiGianCheckIn = now.AddMinutes(-10),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p102.MaPhong,
                    MaBenhNhan = allBn[6].MaBenhNhan,
                    MaPhieuHen = null
                });

                // W-002 phòng 102 (Vãng lai)
                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 2, // W-002
                    NgayCapSo = today,
                    LoaiKhach = "VangLai",
                    MucUuTien = 2,
                    GioHen = null,
                    ThoiGianCheckIn = now.AddMinutes(-8),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p102.MaPhong,
                    MaBenhNhan = allBn[7].MaBenhNhan,
                    MaPhieuHen = null
                });
            }

            // Phòng 103 (Da liễu): 2 bệnh nhân (1 A-001, 1 W-001)
            if (caP103 != null)
            {
                var slots103 = caP103.KhungGios.Where(k => k.LoaiKhungGio == "DatHen").ToList();
                slots103[0].TrangThai = "DaDat";
                var ph5 = new PhieuDatHen
                {
                    MaDatHen = $"BK{today:yyMMdd}-005",
                    KenhDatLich = "Online",
                    LyDoKhamSoBo = "Nổi mẩn ngứa dị ứng vùng tay và cổ",
                    ThoiGianTaoPhieu = now.AddHours(-1),
                    GuiNhacLich = true,
                    TrangThaiHen = "DaCheckIn",
                    MaBenhNhan = allBn[8].MaBenhNhan,
                    MaNguoiTao = adminUser.MaTaiKhoan,
                    MaKhungGio = slots103[0].MaKhungGio
                };
                db.PhieuDatHens.Add(ph5);
                await db.SaveChangesAsync();

                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // A-001
                    NgayCapSo = today,
                    LoaiKhach = "DatTruoc",
                    MucUuTien = 1,
                    GioHen = slots103[0].GioBatDau,
                    ThoiGianCheckIn = now.AddMinutes(-18),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p103.MaPhong,
                    MaBenhNhan = allBn[8].MaBenhNhan,
                    MaPhieuHen = ph5.MaPhieuHen
                });

                db.PhieuSoThuTus.Add(new PhieuSoThuTu
                {
                    SoThuTu = 1, // W-001
                    NgayCapSo = today,
                    LoaiKhach = "VangLai",
                    MucUuTien = 2,
                    GioHen = null,
                    ThoiGianCheckIn = now.AddMinutes(-5),
                    TrangThaiPhieu = "DangCho",
                    MaPhong = p103.MaPhong,
                    MaBenhNhan = allBn[9].MaBenhNhan,
                    MaPhieuHen = null
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
