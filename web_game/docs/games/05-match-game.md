# 🍬 开心消消乐 — 游戏架构文档

## 概述
经典 Match-3 三消益智游戏。8×8网格中交换相邻方块消除，支持重力掉落和连锁消除。

## 技术栈
- 渲染: Canvas 2D API
- 粒子: 全屏覆盖Canvas层
- 音效: Web Audio API
- 异步: async/await 控制动画节奏

## 架构

```
index.html
│
├── 双Canvas层
│   ├── #gameCanvas — 8×8网格绘制
│   └── #particleCanvas — 全屏粒子特效层
│
├── 数据模型
│   ├── board[8][8] — 二维数组, 每格一个emoji字符串
│   ├── TYPES[6] — 6种水果 emoji (🍎🍊🍋🍇🍓🫐)
│   ├── TARGETS[3] — 关卡目标分数 [500, 1500, 3000]
│   ├── selected — {r, c} | null
│   └── comboCount — 连击计数
│
├── 核心算法
│   ├── findMatches() — 横向+纵向扫描连续3+相同
│   ├── applyGravity() — 每列压实+顶部填充随机新方块
│   ├── hasMatchAt(r,c) — 初始化时防止预存匹配
│   ├── hasValidMoves() — 遍历所有相邻交换检查有效走法
│   └── shuffleBoard() — 无有效走法时自动重排
│
├── 消除流程 (async)
│   └── processMatches()
│       while(true):
│         matches = findMatches()
│         if (!matches) break
│         removeMatches(matches) → 扣除+粒子+音效
│         await sleep(180ms)
│         applyGravity() → 填充
│         await sleep(180ms)
│         comboCount++
│
└── 交互
    ├── 第1次点击 → 选中高亮(金色边框)
    ├── 第2次点击相邻 → 交换
    │   ├── 产生匹配 → 执行消除 + 步数-1
    │   └── 无匹配 → 交换回 + 失败音效
    └── 第2次点击非相邻 → 重新选中
```

## 关键技术点

### 1. 消除检测算法
`findMatches()` 双趟扫描:
- **横向**: 每行从左到右，找连续3+相同emoji，延伸到最长
- **纵向**: 每列从上到下，同上
- 使用 `Set("row,col")` 去重 (交叉位置不重复)

### 2. 重力掉落
`applyGravity()` 对每列从底向上扫描:
1. `write` 指针从底部开始
2. 遇到非空格向下压实
3. 顶部空位填入随机新方块

### 3. 连锁消除 (async/await)
`processMatches()` 使用 ES2017 async/await:
```javascript
while(true) {
  matches = findMatches();
  if (!matches.length) break;
  removeMatches(matches);  // 消除 + 粒子
  await sleep(180);         // 等待视觉效果
  applyGravity();          // 掉落填补
  await sleep(180);         // 等待掉落
  comboCount++;            // 连击递增
}
```
每层循环分数乘以 `(1 + combo × 0.5)` 倍率。

### 4. 死局检测
`hasValidMoves()` 遍历所有相邻对:
```javascript
交换(A,B) → findMatches() → 有匹配? → 交换回 → 返回true
```
全部遍历无有效走法 → `shuffleBoard()` 自动重排。

### 5. 无预存匹配初始化
生成棋盘时，每放一格就检查左边两个和上面两个是否相同:
```javascript
do { board[r][c] = random } while (hasMatchAt(r, c));
```
确保初始棋盘没有可消除组合。
