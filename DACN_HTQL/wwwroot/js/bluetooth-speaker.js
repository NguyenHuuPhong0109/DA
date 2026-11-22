/**
 * bluetooth-speaker.js — Bộ điều khiển Loa Bluetooth cho màn hình Bác sĩ
 *
 * Tính năng Hybrid TTS 2 tầng:
 *  - Tầng 1 (Ưu tiên): API Proxy Backend (/api/tts/speak?text=...) phát giọng tiếng Việt chuẩn tự nhiên qua Google TTS
 *  - Tầng 2 (Dự phòng): Web Speech API (window.speechSynthesis) khi mất kết nối mạng hoặc lỗi server
 *  - Chuẩn hóa số thứ tự sang tiếng Việt chữ (ví dụ: A-001 -> "A không không một", W-012 -> "W không một hai")
 *  - Mở khóa âm thanh (Audio unlock) cho trình duyệt di động iOS Safari & Android Chrome
 *  - Phát tiếng chuông Ding-Dong bằng Web Audio API trước khi đọc
 *  - Điều chỉnh âm lượng, tốc độ đọc, bật/tắt tự động đọc
 */
const BluetoothSpeaker = (() => {
    // ── Trạng thái ──────────────────────────────────────────────────────────
    let _tuDongDoc  = true;
    let _amLuong    = 0.85;    // 0.0 – 1.0
    let _tocDo      = 1.0;     // 0.7 – 1.2
    let _audioCtx   = null;
    let _dangNoi    = false;
    let _sharedAudio = null;

    // ── Lấy Audio element dùng chung ─────────────────────────────────────────
    function _getSharedAudio() {
        if (!_sharedAudio) {
            _sharedAudio = new Audio();
        }
        return _sharedAudio;
    }

    // ── Mở khóa âm thanh đồng bộ trong sự kiện click người dùng ──────────────
    function unlockAudio() {
        try {
            _ensureAudioCtx();
            if (_audioCtx && _audioCtx.state === 'suspended') {
                _audioCtx.resume();
            }

            const audio = _getSharedAudio();
            // Data URI của 1 mẫu WAV rỗng (silent) để mở khóa thẻ Audio trên iOS / Android
            audio.src = 'data:audio/wav;base64,UklGRigAAABXQVZFZm10IBIAAAABAAEARKwAAIhYAQACABAAAABkYXRhAgAAAAEA';
            const playPromise = audio.play();
            if (playPromise && typeof playPromise.then === 'function') {
                playPromise.then(() => {
                    audio.pause();
                }).catch(() => {});
            }
        } catch (e) {
            console.warn('[BluetoothSpeaker] unlockAudio thông báo:', e);
        }
    }

    // ── Khởi tạo AudioContext (Web Audio API) ─────────────────────────────────
    function _ensureAudioCtx() {
        if (!_audioCtx) {
            const AudioContextClass = window.AudioContext || window.webkitAudioContext;
            if (AudioContextClass) {
                _audioCtx = new AudioContextClass();
            }
        }
        if (_audioCtx && _audioCtx.state === 'suspended') {
            _audioCtx.resume();
        }
    }

    // ── Phát chuông Ding-Dong qua Web Audio API ──────────────────────────────
    function _chuong() {
        return new Promise((resolve) => {
            try {
                _ensureAudioCtx();
                const ctx = _audioCtx;
                if (!ctx) { resolve(); return; }

                // Tần số chuông Ding (D5=587Hz) và Dong (G4=392Hz)
                const notes = [
                    { freq: 587.3, start: 0,    dur: 0.35 },
                    { freq: 392.0, start: 0.38, dur: 0.45 },
                ];

                let endTime = 0;

                notes.forEach(({ freq, start, dur }) => {
                    const osc  = ctx.createOscillator();
                    const gain = ctx.createGain();

                    osc.connect(gain);
                    gain.connect(ctx.destination);

                    osc.type = 'sine';
                    osc.frequency.setValueAtTime(freq, ctx.currentTime + start);

                    // Fade-in + fade-out mềm mại
                    gain.gain.setValueAtTime(0, ctx.currentTime + start);
                    gain.gain.linearRampToValueAtTime(_amLuong * 0.7, ctx.currentTime + start + 0.04);
                    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + start + dur);

                    osc.start(ctx.currentTime + start);
                    osc.stop(ctx.currentTime + start + dur);

                    endTime = Math.max(endTime, start + dur);
                });

                setTimeout(resolve, (endTime + 0.05) * 1000);
            } catch (e) {
                console.warn('[BluetoothSpeaker] Chuông Web Audio lỗi:', e);
                resolve();
            }
        });
    }

    // ── Chuẩn hóa số thứ tự sang tiếng Việt chữ (tránh đọc dấu gạch ngang) ────
    function _chuanHoaSoThuTu(soThuTu) {
        if (!soThuTu) return '';
        const chuSoMap = {
            '0': 'không', '1': 'một', '2': 'hai', '3': 'ba', '4': 'bốn',
            '5': 'năm', '6': 'sáu', '7': 'bảy', '8': 'tám', '9': 'chín'
        };

        const tokens = [];
        for (let i = 0; i < soThuTu.length; i++) {
            const ch = soThuTu[i];
            if (ch === '-' || ch === ' ') continue;
            if (chuSoMap[ch]) {
                tokens.push(chuSoMap[ch]);
            } else {
                tokens.push(ch);
            }
        }
        return tokens.join(' ');
    }

    // ── Hiển thị thông báo nhỏ trên thanh công cụ loa ────────────────────────
    function _showNotice(msg, isError = false) {
        let el = document.getElementById('bt-notice');
        if (!el) {
            const bar = document.getElementById('bt-toolbar');
            if (bar) {
                el = document.createElement('span');
                el.id = 'bt-notice';
                el.style.cssText = 'font-size:12px;font-weight:500;padding:2px 8px;border-radius:4px;margin-left:auto;';
                bar.appendChild(el);
            }
        }
        if (el) {
            el.textContent = msg;
            el.style.color = isError ? '#991B1B' : '#854D0E';
            el.style.background = isError ? '#FEE2E2' : '#FEF9C3';
            el.style.display = 'inline-block';
            setTimeout(() => {
                if (el) el.style.display = 'none';
            }, 5000);
        }
    }

    // ── Tầng 1: Phát tiếng Việt qua API Proxy Backend ────────────────────────
    function _docOnlineTts(text) {
        return new Promise((resolve) => {
            try {
                const audio = _getSharedAudio();
                audio.pause();
                audio.currentTime = 0;
                audio.volume = _amLuong;
                audio.playbackRate = _tocDo;

                let finished = false;
                const done = (success) => {
                    if (finished) return;
                    finished = true;
                    _dangNoi = false;
                    audio.onended = null;
                    audio.onerror = null;
                    resolve(success);
                };

                audio.onended = () => done(true);
                audio.onerror = (e) => {
                    console.warn('[BluetoothSpeaker] Tầng 1 (API Proxy) phát lỗi, chuẩn bị chuyển Tầng 2:', e);
                    done(false);
                };

                const url = `/api/tts/speak?text=${encodeURIComponent(text)}`;
                audio.src = url;
                _dangNoi = true;

                const playP = audio.play();
                if (playP && typeof playP.catch === 'function') {
                    playP.catch((err) => {
                        console.warn('[BluetoothSpeaker] Audio.play() bị chặn hoặc lỗi:', err);
                        done(false);
                    });
                }
            } catch (e) {
                _dangNoi = false;
                console.warn('[BluetoothSpeaker] Ngoại lệ Tầng 1:', e);
                resolve(false);
            }
        });
    }

    // ── Tầng 2: Dự phòng phát qua Web Speech API (window.speechSynthesis) ─────
    function _docWebSpeech(text) {
        return new Promise((resolve) => {
            if (!window.speechSynthesis) {
                _showNotice('Trình duyệt không hỗ trợ Web Speech API.', true);
                resolve(false);
                return;
            }

            window.speechSynthesis.cancel();

            const utt = new SpeechSynthesisUtterance(text);
            utt.rate = _tocDo;
            utt.volume = _amLuong;

            const voices = window.speechSynthesis.getVoices();
            const giongViet = voices.find(v => (v.lang || '').toLowerCase().startsWith('vi'));

            if (giongViet) {
                utt.voice = giongViet;
                utt.lang = giongViet.lang;
            } else {
                utt.lang = 'vi-VN';
                _showNotice('Đang phát qua giọng mặc định (thiết bị chưa cài gói vi-VN).');
            }

            utt.onend = () => { _dangNoi = false; resolve(true); };
            utt.onerror = (e) => {
                _dangNoi = false;
                console.warn('[BluetoothSpeaker] Tầng 2 WebSpeech lỗi:', e);
                resolve(false);
            };

            _dangNoi = true;
            window.speechSynthesis.speak(utt);
        });
    }

    // ── Hàm phát câu hoàn chỉnh (Chuông -> TTS Hybrid) ───────────────────────
    async function phat({ soThuTu, tenBenhNhan, tenPhong }) {
        if (!_tuDongDoc) return;

        // Dừng bất kỳ âm thanh nào đang phát
        if (_sharedAudio) {
            _sharedAudio.pause();
        }
        if (window.speechSynthesis) {
            window.speechSynthesis.cancel();
        }

        // 1. Tiếng chuông Ding-Dong
        await _chuong();

        // 2. Chờ 600ms tạo khoảng nghỉ tự nhiên
        await new Promise(r => setTimeout(r, 600));

        // 3. Chuẩn hóa câu đọc
        const soDoc = _chuanHoaSoThuTu(soThuTu);
        const vanBan = `Xin mời bệnh nhân số ${soDoc}, ${tenBenhNhan}, vào ${tenPhong}`;

        // 4. Phát Tầng 1 -> Tự động chuyển Tầng 2 nếu thất bại
        const okOnline = await _docOnlineTts(vanBan);
        if (!okOnline) {
            await _docWebSpeech(vanBan);
        }
    }

    // ── Kiểm tra loa với câu mẫu thực tế ──────────────────────────────────────
    async function kiemTra() {
        unlockAudio();

        if (_sharedAudio) _sharedAudio.pause();
        if (window.speechSynthesis) window.speechSynthesis.cancel();

        await _chuong();
        await new Promise(r => setTimeout(r, 600));

        const mauCau = 'Xin mời bệnh nhân số A không không một, Nguyễn Văn An, vào Phòng 101';
        const okOnline = await _docOnlineTts(mauCau);
        if (!okOnline) {
            await _docWebSpeech(mauCau);
        }
    }

    // ── Getters / Setters ─────────────────────────────────────────────────────
    function setAmLuong(val) {
        _amLuong = Math.max(0, Math.min(1, parseFloat(val)));
        if (_sharedAudio) _sharedAudio.volume = _amLuong;
    }
    function setTocDo(val) {
        _tocDo = Math.max(0.7, Math.min(1.2, parseFloat(val)));
        if (_sharedAudio) _sharedAudio.playbackRate = _tocDo;
    }
    function setTuDong(val)  { _tuDongDoc = !!val; }
    function isTuDong()      { return _tuDongDoc; }

    return { phat, kiemTra, unlockAudio, setAmLuong, setTocDo, setTuDong, isTuDong };
})();

// ── Tải danh sách giọng của trình duyệt ngay khi sẵn sàng ───────────────────
if (typeof window !== 'undefined' && window.speechSynthesis) {
    window.speechSynthesis.onvoiceschanged = () => {
        try { window.speechSynthesis.getVoices(); } catch (_) {}
    };
    try { window.speechSynthesis.getVoices(); } catch (_) {}
}
