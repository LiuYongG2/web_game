# 🦆 抓大鹅 — 游戏架构文档

## 概述
类"羊了个羊"的三消堆叠游戏。三层互相遮挡的方块，点击未被遮挡的方块移入槽位，三个相同自动消除。

## 技术栈
- 渲染: Canvas 2D API
- 音效: Web Audio API (OscillatorNode)
- 粒子: 全屏覆盖 Canvas 层

## 架构

```
index.html (单文件)
│
├── 双Canvas层
│   ├── #gameCanvas — 游戏主画面 (方块绘制)
│   └── #particleCanvas — 全屏粒子层 (消除特效, position:fixed)
│
├── HTML DOM层
│   ├── 槽位区 (.slot-area) — 动态生成 div 元素
│   ├── 工具栏 — 移出/撤回/洗牌/重来 按钮
│   └── 模态弹窗 — 过关/失败 覆盖层
│
└── JavaScript逻辑
    ├── 数据层
    │   ├── tiles[] — 方块数组 {id,emoji,layer,x,y,w,h,removed}
    │   ├── slot[] — 槽位数组 {emoji,tileId}
    │   └── history[] — 操作历史栈 (撤回用)
    │
    ├── 生成算法
    │   └── genTiles(levelIdx)
    │       ├── 创建成对emoji (每种3个, 保证可解)
    │       ├── Fisher-Yates洗牌
    │       └── 按3层分布, 每层偏移14px
    │
    ├── 遮挡检测
    │   └── isBlocked(tile)
    │       └── 遍历更高层方块, AABB矩形碰撞判断
    │
    ├── 消除逻辑
    │   └── checkMatch()
    │       ├── 统计槽位中各emoji数量
    │       ├── >=3 触发消除动画 (CSS animation)
    │       └── 500ms后移除, 检查胜负
    │
    ├── 渲染循环
    │   └── draw() — requestAnimationFrame
    │       ├── 背景浮动粒子
    │       ├── 方块: 渐变填充+阴影+高光+emoji
    │       └── 粒子层: 消除爆炸效果
    │
    └── 道具系统
        ├── 移出 — slot.pop()
        ├── 撤回 — history.pop() 恢复
        └── 洗牌 — 收集剩余emoji重新shuffle分配
```

## 关键技术点

### 1. 三层遮挡系统
方块按 `layer` (0/1/2) 分层，高层方块偏移 14px 产生堆叠视觉。点击检测从高层往低层遍历，`isBlocked()` 使用 AABB 矩形碰撞判断某方块是否被更高层遮挡。

### 2. 可解性保证
生成方块时每种emoji严格3个一组，总数必为3的倍数。洗牌后随机分布，理论上总能全部消除。

### 3. 双Canvas粒子系统
主游戏Canvas绘制方块，独立的全屏Canvas层 (`position:fixed; pointer-events:none`) 负责粒子特效。粒子带速度、重力、生命周期，超期自动移除。

### 关卡配置

| 关卡 | 图案种类 | 方块总数 | 网格列数 |
|------|---------|---------|---------|
| 1 | 6 | 36 | 6 |
| 2 | 8 | 48 | 8 |
| 3 | 10 | 60 | 8 |
