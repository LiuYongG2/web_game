# MyWeb 游戏平台 — 整体架构与技术文档

## 1. 系统架构概览

```
┌─────────────────────────────────────────────────────────────┐
│                      客户端 (浏览器)                         │
│                                                             │
│  ┌─────────────┐    ┌────────────────────────────────────┐ │
│  │  平台主页     │    │         7 款独立游戏                │ │
│  │  EJS 模板渲染  │    │  (各自独立的单文件 HTML 应用)       │ │
│  │  · 游戏卡片    │    │                                    │ │
│  │  · 高分展示    │───→│  Canvas2D × 5   Three.js × 2     │ │
│  │  · 导航链接    │    │  ┌────┐┌────┐┌────┐┌────┐┌────┐ │ │
│  └─────────────┘    │  │鹅  ││棋  ││藏  ││找  ││消  │ │ │
│         │           │  └────┘└────┘└────┘└────┘└────┘ │ │
│         │           │  ┌─────────┐  ┌─────────┐       │ │
│  ┌──────┴───────┐   │  │FPS 射击  │  │幻境探索  │       │ │
│  │ localStorage  │←──│  │Three.js  │  │Three.js  │       │ │
│  │ 高分数据持久化 │   │  └─────────┘  └─────────┘       │ │
│  └──────────────┘   └────────────────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTP GET
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Node.js 服务器 (server.js)                   │
│                                                             │
│  Express.js v5.2.1 + EJS v5.0.1                            │
│                                                             │
│  中间件管道 (按顺序执行):                                     │
│  ①static(public/)  → 平台 CSS/JS                           │
│  ②static(assets/)  → 公共资源                               │
│  ③static(shared/)  → 游戏共享工具库                          │
│  ④static(games/)   → 7款游戏的HTML文件                      │
│  ⑤json()+urlencoded() → 请求体解析                          │
│  ⑥路由: GET / → EJS渲染 | GET /about → EJS渲染             │
│  ⑦        GET /api/hello → JSON | * → 404                  │
│                                                             │
│  监听端口: process.env.PORT || 3000                          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                        文件系统                              │
│  myweb/                                                    │
│  ├── server.js ............. 服务器入口 (35行)              │
│  ├── package.json .......... 项目配置                       │
│  ├── views/ ................ EJS 模板                       │
│  │   ├── index.ejs ......... 游戏平台主页 (含7款游戏卡片)    │
│  │   ├── about.ejs ......... 关于页面                       │
│  │   ├── 404.ejs ........... 错误页面                       │
│  │   └── partials/ ......... 可复用模板片段                  │
│  ├── public/ ............... 平台静态资源                    │
│  ├── games/ ................ 7款游戏 (各自目录)              │
│  ├── shared/ ............... 共享工具库                      │
│  ├── assets/ ............... 公共资源                        │
│  └── docs/ ................. 本文档目录                      │
└─────────────────────────────────────────────────────────────┘
```

## 2. 核心技术栈

| 层级 | 技术 | 版本 | 用途 |
|------|------|------|------|
| **运行时** | Node.js | v24.14.0 | JavaScript 服务端执行环境 |
| **包管理** | npm | v11.9.0 | 依赖管理和脚本执行 |
| **Web框架** | Express.js | v5.2.1 | HTTP服务、路由、静态文件托管 |
| **模板引擎** | EJS | v5.0.1 | 服务端HTML模板编译渲染 |
| **2D渲染** | Canvas 2D API | 浏览器原生 | 5款游戏的图形绘制 |
| **3D渲染** | Three.js | v0.160.0 (CDN) | FPS射击和幻境探索的3D场景 |
| **后处理** | Three.js Addons | v0.160.0 (CDN) | Bloom泛光、FXAA抗锯齿 |
| **音频** | Web Audio API | 浏览器原生 | 全部游戏的程序化音效生成 |
| **数据存储** | localStorage | 浏览器原生 | 各游戏高分记录持久化 |
| **鼠标锁定** | Pointer Lock API | 浏览器原生 | FPS和探索游戏的视角控制 |

## 3. 服务器架构详解

### 3.1 Express 中间件管道

请求到达服务器后按以下顺序经过中间件处理：

```
HTTP请求 ─→ static(public/) ─→ static(assets/) ─→ static(shared/) ─→ static(games/)
                                                                          │
    匹配静态文件? ─── 是 ──→ 直接返回文件（不经过路由）                        │
         │                                                                 │
         否 ─→ json() ─→ urlencoded() ─→ 路由匹配                          │
                                            │                              │
                              GET / ──→ EJS渲染index.ejs                   │
                              GET /about ──→ EJS渲染about.ejs              │
                              GET /api/hello ──→ JSON响应                   │
                              * ──→ 404.ejs                                │
```

**关键设计决策**: 游戏文件作为纯静态HTML托管，不经过EJS模板引擎。每款游戏是完全独立的单文件应用，可脱离服务器在浏览器直接打开。

### 3.2 静态文件映射

| 中间件注册 | 文件系统目录 | URL 路径 | 内容说明 |
|-----------|-------------|----------|---------|
| `express.static('public')` | `public/` | `/*` | 平台级CSS和JavaScript |
| `express.static('assets')` | `assets/` | `/assets/*` | 公共样式、图片、音频 |
| `express.static('shared')` | `shared/` | `/shared/*` | GameTemplate.js, utils.js |
| `express.static('games')` | `games/` | `/games/*` | 7款游戏HTML文件 |

### 3.3 启动模式

```bash
npm start   # 生产: node server.js
npm run dev # 开发: node --watch server.js (文件变动自动重启)
PORT=8080 npm start # 自定义端口
```

## 4. 前端架构

### 4.1 平台主页 (views/index.ejs)

**渲染方式**: EJS 服务端模板渲染，注入 `title` 变量

**布局系统**: CSS Grid (`grid-template-columns: repeat(auto-fill, minmax(300px, 1fr))`)

**视觉设计**: 暗色宇宙风格，三色渐变背景 (`#0f0c29 → #302b63 → #24243e`)

**高分读取**: 页面加载时 JavaScript 遍历 `.high-score[data-game]` 元素，从 localStorage 读取对应游戏高分并显示

**响应式**: `@media (max-width: 640px)` 断点，移动端单列布局

### 4.2 游戏架构模式

每款游戏遵循统一的架构模式:

```
单文件 HTML
├── <style> ─── 全部CSS内联，暗色主题，毛玻璃UI
├── HTML结构 ── Canvas画布 + HUD覆盖层 + 模态弹窗 + 工具栏
└── <script> ── 全部JS内联
    ├── 常量定义 (关卡配置、游戏参数)
    ├── 状态对象 (score, level, hp, etc.)
    ├── 音效系统 (Web Audio API oscillator+gain)
    ├── 渲染循环 (requestAnimationFrame 或事件驱动)
    ├── 输入处理 (click/touchstart/keydown/mousemove)
    ├── 游戏逻辑 (规则引擎、碰撞检测、AI)
    ├── UI更新  (DOM操作更新分数/血量/弹药)
    └── 高分存储 (localStorage读写)
```

### 4.3 共享工具库

**GameTemplate.js** (114行) — 游戏生命周期基类:
- `init()` → `setup()` → `start()` → `pause()/resume()` → `gameOver()` → `destroy()`
- 内置: Canvas工厂、倒计时器、计分系统、高分持久化

**utils.js** (86行) — 通用工具:
- `shuffle()` — Fisher-Yates洗牌
- `playSound()` — Web Audio API封装
- `soundEffects.*` — 预置音效 (click/match/win/fail/shoot/hit)
- `drawRoundRect()` — Canvas圆角矩形

## 5. 音效系统架构

全部音效通过 Web Audio API 实时程序化合成，零外部音频文件:

```
AudioContext
    │
    ├── OscillatorNode (振荡器)
    │   ├── type: sine / square / sawtooth / triangle
    │   └── frequency: 50Hz ~ 2000Hz
    │
    ├── GainNode (音量包络)
    │   └── exponentialRampToValueAtTime (衰减曲线)
    │
    ├── BiquadFilterNode (滤波器, 仅FPS使用)
    │   └── lowpass 2500Hz (模拟枪声)
    │
    └── BufferSourceNode (白噪声, 仅FPS使用)
        └── 随机采样数据 + 线性衰减 (模拟爆破声)
```

| 音效类型 | 实现方式 | 频率范围 |
|---------|---------|---------|
| 点击/选中 | 单振荡器 square | 600-800Hz, 40-60ms |
| 消除/匹配 | 三音上行 C5→E5→G5 | 523→659→784Hz |
| 胜利 | 四音凯旋 C5→E5→G5→C6 | 523→1047Hz |
| 失败 | 下行 sawtooth | 300→200Hz |
| 枪声 | 白噪声 + 双振荡器 | 50-200Hz, 40-150ms |
| 命中 | 高频 square + sine | 800-1200Hz |

## 6. 数据持久化

使用浏览器 localStorage，key 命名规则: `highscore_{游戏标识}`

| localStorage Key | 游戏 | 数据类型 |
|-----------------|------|---------|
| `highscore_goose_game` | 抓大鹅 | 累计最高分 |
| `highscore_chinese_chess` | 中国象棋 | 累计胜局数 |
| `highscore_hide_seek` | 捉迷藏 | 累计最高分 |
| `highscore_spot_diff` | 找不同 | 累计最高分 |
| `highscore_match_game` | 消消乐 | 累计最高分 |
| `highscore_fps_game` | FPS射击 | 单局最高分 |
| `highscore_adventure` | 幻境探索 | 单局最高分 |

## 7. 响应式与跨平台

### 触控适配策略
- 所有Canvas同时绑定 `click` + `touchstart` 事件
- `touchstart` 调用 `preventDefault()` 阻止默认滚动
- 点击坐标统一通过 `getBoundingClientRect()` + 缩放比转换

### Canvas自适应
- `max-width: 100%; height: auto` — CSS缩放
- 逻辑坐标 = 物理坐标 × (canvas.width / rect.width) — 精确映射

### 已知平台限制
- FPS射击和幻境探索依赖 Pointer Lock API，不支持移动端
- Three.js 游戏首次需联网加载 CDN 资源 (~600KB)

## 8. 性能优化策略

| 优化项 | 实现方式 |
|-------|---------|
| **帧率控制** | `requestAnimationFrame` 自适应刷新率 |
| **deltaTime** | `Math.min((time-lastTime)/1000, 0.05)` 防帧率波动 |
| **Three.js像素比** | `Math.min(devicePixelRatio, 2)` 高DPI设备限制 |
| **阴影图** | 1024×1024 或 2048×2048，根据场景调整 |
| **雾效** | `FogExp2` 裁剪远处物体渲染 |
| **粒子回收** | 基于生命周期自动移除，防内存泄漏 |
| **Canvas文本缓存** | Emoji 使用 `fillText` 直接渲染，利用系统字体缓存 |

## 9. 安全考虑

- 无用户输入传入服务器（纯静态托管）
- 无数据库连接、无用户认证
- localStorage 数据纯客户端，无敏感信息
- Three.js CDN 使用 HTTPS，unpkg.com 可信源
