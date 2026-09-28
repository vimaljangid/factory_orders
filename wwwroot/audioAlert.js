// Sound and Vibration Alert Helper for Factory Orders Live
window.factoryAlert = {
    audioCtx: null,

    initAudio: function () {
        if (!this.audioCtx) {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (AudioContext) {
                this.audioCtx = new AudioContext();
            }
        }
        if (this.audioCtx && this.audioCtx.state === 'suspended') {
            this.audioCtx.resume();
        }
    },

    playUrgentChime: function () {
        try {
            this.initAudio();
            if (!this.audioCtx) return;

            const now = this.audioCtx.currentTime;
            const osc = this.audioCtx.createOscillator();
            const gain = this.audioCtx.createGain();

            osc.type = 'sine';
            // Alert chime: High frequency beep then higher
            osc.frequency.setValueAtTime(880, now); // A5
            osc.frequency.setValueAtTime(1174.66, now + 0.15); // D6
            osc.frequency.setValueAtTime(1760, now + 0.3); // A6

            gain.gain.setValueAtTime(0.3, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.6);

            osc.connect(gain);
            gain.connect(this.audioCtx.destination);

            osc.start(now);
            osc.stop(now + 0.6);

            // Vibrate mobile device if supported
            if ("vibrate" in navigator) {
                navigator.vibrate([250, 100, 250]);
            }
        } catch (e) {
            console.warn("Audio chime failed: ", e);
        }
    },

    playNormalChime: function () {
        try {
            this.initAudio();
            if (!this.audioCtx) return;

            const now = this.audioCtx.currentTime;
            const osc = this.audioCtx.createOscillator();
            const gain = this.audioCtx.createGain();

            osc.type = 'triangle';
            osc.frequency.setValueAtTime(523.25, now); // C5
            osc.frequency.setValueAtTime(659.25, now + 0.12); // E5

            gain.gain.setValueAtTime(0.2, now);
            gain.gain.exponentialRampToValueAtTime(0.01, now + 0.35);

            osc.connect(gain);
            gain.connect(this.audioCtx.destination);

            osc.start(now);
            osc.stop(now + 0.35);
        } catch (e) {
            console.warn("Normal chime failed: ", e);
        }
    }
};

// Enable audio context on first user interaction (standard mobile browser requirement)
document.addEventListener('click', function () {
    window.factoryAlert.initAudio();
}, { once: true });
