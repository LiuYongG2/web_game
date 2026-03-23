const GameUtils = {
  randomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  },

  shuffle(array) {
    const arr = [...array];
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  },

  formatTime(seconds) {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  },

  getHighScore(gameName) {
    const key = `highscore_${gameName.replace(/\s+/g, '_').toLowerCase()}`;
    return parseInt(localStorage.getItem(key) || '0', 10);
  },

  setHighScore(gameName, score) {
    const key = `highscore_${gameName.replace(/\s+/g, '_').toLowerCase()}`;
    const current = parseInt(localStorage.getItem(key) || '0', 10);
    if (score > current) {
      localStorage.setItem(key, score.toString());
      return true;
    }
    return false;
  },

  playSound(frequency, duration, type) {
    try {
      const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
      const oscillator = audioCtx.createOscillator();
      const gainNode = audioCtx.createGain();
      oscillator.connect(gainNode);
      gainNode.connect(audioCtx.destination);
      oscillator.type = type || 'sine';
      oscillator.frequency.setValueAtTime(frequency, audioCtx.currentTime);
      gainNode.gain.setValueAtTime(0.3, audioCtx.currentTime);
      gainNode.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + (duration || 0.3));
      oscillator.start();
      oscillator.stop(audioCtx.currentTime + (duration || 0.3));
    } catch (e) {}
  },

  soundEffects: {
    click() { GameUtils.playSound(800, 0.1, 'square'); },
    match() { GameUtils.playSound(523, 0.15, 'sine'); setTimeout(() => GameUtils.playSound(659, 0.15, 'sine'), 100); setTimeout(() => GameUtils.playSound(784, 0.2, 'sine'), 200); },
    win() { [523,659,784,1047].forEach((f,i) => setTimeout(() => GameUtils.playSound(f, 0.3, 'sine'), i*150)); },
    fail() { GameUtils.playSound(300, 0.3, 'sawtooth'); setTimeout(() => GameUtils.playSound(200, 0.5, 'sawtooth'), 200); },
    shoot() { GameUtils.playSound(150, 0.08, 'sawtooth'); },
    hit() { GameUtils.playSound(1000, 0.1, 'square'); }
  },

  createBackButton(href) {
    const btn = document.createElement('a');
    btn.href = href || '/';
    btn.textContent = '← 返回平台';
    btn.style.cssText = 'display:inline-block;margin:10px 0;padding:8px 20px;background:#4a90d9;color:#fff;border-radius:6px;text-decoration:none;font-size:14px;';
    return btn;
  },

  drawRoundRect(ctx, x, y, w, h, r) {
    ctx.beginPath();
    ctx.moveTo(x + r, y);
    ctx.lineTo(x + w - r, y);
    ctx.quadraticCurveTo(x + w, y, x + w, y + r);
    ctx.lineTo(x + w, y + h - r);
    ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
    ctx.lineTo(x + r, y + h);
    ctx.quadraticCurveTo(x, y + h, x, y + h - r);
    ctx.lineTo(x, y + r);
    ctx.quadraticCurveTo(x, y, x + r, y);
    ctx.closePath();
  }
};

if (typeof module !== 'undefined' && module.exports) {
  module.exports = GameUtils;
}
