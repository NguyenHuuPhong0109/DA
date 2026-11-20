/**
 * bluetooth-speaker.js — Bộ điều khiển Loa Bluetooth cho màn hình Bác sĩ
 *
 * Tính năng:
 *  - Phát tiếng chuông "Ding-Dong" qua Web Audio API (không cần file MP3)
 *  - Đọc TTS tiếng Việt: "Xin mời bệnh nhân số [A-0-0-1], [Họ tên], vào [Phòng]"
 *  - Thanh công cụ Loa: Kiểm tra, Âm lượng, Tốc độ đọc, Bật/Tắt tự động
 *
 * Sử dụng:
 *   BluetoothSpeaker.phat({ soThuTu: "A-001", tenBenhNhan: "Nguyễn Văn A", tenPhong: "Phòng 01" });
 *   BluetoothSpeaker.kiemTra();
 */
const BluetoothSpeaker = (() => {
    // ── Trạng thái ──────────────────────────────────────────────────────────
    let _tuDongDoc  = true;
    let _amLuong    = 0.75;    // 0.0 – 1.0
    let _tocDo      = 0.9;     // 0.7 – 1.2
    let _audioCtx   = null;
    let _dangNoi    = false;

    // ── Khởi tạo AudioContext (cần sau user gesture) ─────────────────────────
    function _ensureAudioCtx() {
        if (!_audioCtx) {
            _audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        }
        if (_audioCtx.state === 'suspended') {
            _audioCtx.resume();
        }
    }

    // ── Phát chuông Ding-Dong qua Web Audio API ──────────────────────────────
    function _chuong() {
        return new Promise((resolve) => {
            try {
                _ensureAudioCtx();
                const ctx = _audioCtx;

                // Tần số chuông Ding (D5=587Hz) và Dong (G4=392Hz)
                const notes = [
                    { freq: 587.3, start: 0,   dur: 0.35 },
                    { freq: 392.0, start: 0.38, dur: 0.45 },
                ];

                let endTime = 0;

                notes.forEach(({ freq, start, dur }) => {
                    const osc  = ctx.createOscillator();
                    const gain = ctx.createGain();

                    osc.connect(gain);
                    gain.connect(ctx.destination);

                    osc.type      = 'sine';
                    osc.frequency.setValueAtTime(freq, ctx.currentTime + start);

                    // Fade-in + fade-out mềm
                    gain.gain.setValueAtTime(0, ctx.currentTime + start);
                    gain.gain.linearRampToValueAtTime(_amLuong * 0.6, ctx.currentTime + start + 0.04);
                    gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + start + dur);

                    osc.start(ctx.currentTime + start);
                    osc.stop(ctx.currentTime + start + dur);

                    endTime = Math.max(endTime, start + dur);
                });

                setTimeout(resolve, (endTime + 0.1) * 1000);
            } catch (e) {
                console.warn('[BluetoothSpeaker] Chuông lỗi:', e);
                resolve();
            }
        });
    }

    // ── Đọc từng ký tự số cách nhau (A-0-0-1) ───────────────────────────────
    function _docSoThuTu(soThuTu) {
        // "A-001" → "A, 0, 0, 1"
        return soThuTu
            .split('')
            .filter(c => c !== '-')
            .join(', ');
    }

    // ── TTS tiếng Việt ───────────────────────────────────────────────────────
    function _docTTS(text) {
        return new Promise((resolve) => {
            if (!window.speechSynthesis) { resolve(); return; }

            // Hủy bất kỳ phát âm đang chạy
            window.speechSynthesis.cancel();

            const utt  = new SpeechSynthesisUtterance(text);
            utt.lang   = 'vi-VN';
            utt.rate   = _tocDo;
            utt.volume = _amLuong;

            // Chọn giọng vi-VN nếu có
            const voices = window.speechSynthesis.getVoices();
            const giongViet = voices.find(v => v.lang === 'vi-VN' || v.lang.startsWith('vi'));
            if (giongViet) utt.voice = giongViet;

            utt.onend   = () => { _dangNoi = false; resolve(); };
            utt.onerror = () => { _dangNoi = false; resolve(); };

            _dangNoi = true;
            window.speechSynthesis.speak(utt);
        });
    }

    // ── Hàm chính: phát chuông + đọc thông báo ───────────────────────────────
    async function phat({ soThuTu, tenBenhNhan, tenPhong }) {
        if (!_tuDongDoc) return;
        if (_dangNoi) window.speechSynthesis.cancel();

        _ensureAudioCtx();

        // 1. Chuông Ding-Dong
        await _chuong();

        // 2. Chờ 0.3s rồi đọc
        await new Promise(r => setTimeout(r, 300));

        // 3. Xây chuỗi thông báo
        const soDoc  = _docSoThuTu(soThuTu);
        const vanBan = `Xin mời bệnh nhân số ${soDoc}, ${tenBenhNhan}, vào ${tenPhong}`;

        await _docTTS(vanBan);
    }

    // ── Kiểm tra loa ─────────────────────────────────────────────────────────
    async function kiemTra() {
        await _chuong();
        await new Promise(r => setTimeout(r, 300));
        await _docTTS('Loa đang hoạt động tốt. Phòng khám sẵn sàng phục vụ.');
    }

    // ── Getters / Setters ─────────────────────────────────────────────────────
    function setAmLuong(val) { _amLuong = Math.max(0, Math.min(1, parseFloat(val))); }
    function setTocDo(val)   { _tocDo   = Math.max(0.7, Math.min(1.2, parseFloat(val))); }
    function setTuDong(val)  { _tuDongDoc = !!val; }
    function isTuDong()      { return _tuDongDoc; }

    return { phat, kiemTra, setAmLuong, setTocDo, setTuDong, isTuDong };
})();

// ── Tải giọng vi-VN ngay khi có thể ────────────────────────────────────────
if (window.speechSynthesis) {
    window.speechSynthesis.onvoiceschanged = () => window.speechSynthesis.getVoices();
    window.speechSynthesis.getVoices(); // trigger lần đầu
}
