class GameTemplate {
  constructor(config) {
    this.name = config.name || 'Game';
    this.containerId = config.containerId || 'game-container';
    this.width = config.width || 800;
    this.height = config.height || 600;
    this.score = 0;
    this.level = config.level || 1;
    this.isRunning = false;
    this.isPaused = false;
    this.timer = null;
    this.timeLeft = config.timeLimit || 0;
    this.canvas = null;
    this.ctx = null;
    this.container = null;
    this.onGameOver = config.onGameOver || null;
  }

  init() {
    this.container = document.getElementById(this.containerId);
    if (!this.container) {
      console.error(`Container #${this.containerId} not found`);
      return;
    }
    this.loadHighScore();
    this.setup();
  }

  createCanvas() {
    this.canvas = document.createElement('canvas');
    this.canvas.width = this.width;
    this.canvas.height = this.height;
    this.canvas.style.maxWidth = '100%';
    this.canvas.style.height = 'auto';
    this.container.appendChild(this.canvas);
    this.ctx = this.canvas.getContext('2d');
    return this.canvas;
  }

  setup() {}

  start() {
    this.isRunning = true;
    this.isPaused = false;
    if (this.timeLeft > 0) {
      this.startTimer();
    }
  }

  pause() {
    this.isPaused = true;
    if (this.timer) clearInterval(this.timer);
  }

  resume() {
    this.isPaused = false;
    if (this.timeLeft > 0) this.startTimer();
  }

  startTimer() {
    if (this.timer) clearInterval(this.timer);
    this.timer = setInterval(() => {
      if (!this.isPaused) {
        this.timeLeft--;
        this.onTimerTick(this.timeLeft);
        if (this.timeLeft <= 0) {
          clearInterval(this.timer);
          this.gameOver(false);
        }
      }
    }, 1000);
  }

  onTimerTick(timeLeft) {}

  addScore(points) {
    this.score += points;
    this.onScoreChange(this.score);
  }

  onScoreChange(score) {}

  gameOver(won) {
    this.isRunning = false;
    if (this.timer) clearInterval(this.timer);
    this.saveHighScore();
    if (this.onGameOver) this.onGameOver(won, this.score);
  }

  getHighScoreKey() {
    return `highscore_${this.name.replace(/\s+/g, '_').toLowerCase()}`;
  }

  loadHighScore() {
    return parseInt(localStorage.getItem(this.getHighScoreKey()) || '0', 10);
  }

  saveHighScore() {
    const current = this.loadHighScore();
    if (this.score > current) {
      localStorage.setItem(this.getHighScoreKey(), this.score.toString());
    }
  }

  destroy() {
    this.isRunning = false;
    if (this.timer) clearInterval(this.timer);
    if (this.container) this.container.innerHTML = '';
  }
}

if (typeof module !== 'undefined' && module.exports) {
  module.exports = GameTemplate;
}
