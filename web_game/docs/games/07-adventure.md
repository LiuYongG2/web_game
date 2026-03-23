# 🏔️ 幻境探索 — 游戏架构文档

## 概述
3D开放世界冒险游戏。在浮空岛世界中探索，收集12颗宝石，消灭8个怪物。支持跳跃、冲刺、近战攻击。

## 技术栈
- 3D引擎: Three.js v0.160.0 (ES Module, unpkg CDN)
- 渲染: WebGLRenderer + ACES色调映射 + PCFSoft阴影
- 视角控制: Pointer Lock API
- 物理: 自实现重力+地形碰撞
- 音效: Web Audio API

## 架构

```
index.html (513行)
│
├── 加载系统
│   ├── 显示 "加载中..." 遮罩
│   ├── Three.js CDN加载完成后:
│   │   ├── 隐藏加载遮罩
│   │   ├── 启用开始按钮
│   │   └── 使用addEventListener绑定(非onclick, 避免模块作用域问题)
│   └── 所有事件监听在模块内注册
│
├── Three.js 场景
│   ├── Scene (背景0x1a1a3a, FogExp2雾效)
│   ├── Camera (PerspectiveCamera 65°)
│   ├── 光照
│   │   ├── AmbientLight (0x404070, 0.5)
│   │   ├── DirectionalLight (0xffeedd, 1.2, 1024px阴影)
│   │   └── HemisphereLight (天蓝+地棕)
│   └── Renderer (ACES色调, PCFSoft阴影)
│
├── 世界生成
│   ├── 地面 (120×120 草地纹理平面)
│   ├── 水面 (半透明蓝色平面, y=-0.5, 呼吸动画)
│   ├── 7座浮岛
│   │   └── addIsland(x, z, r, h)
│   │       ├── 石质圆柱体 (CylinderGeometry)
│   │       └── 草地顶面 (扁平圆柱)
│   ├── 25棵树
│   │   └── 树干(圆柱) + 3层球形树冠(递减尺寸)
│   ├── 30块岩石
│   │   └── DodecahedronGeometry + 随机旋转
│   ├── 12颗宝石
│   │   └── OctahedronGeometry + 发光材质 + PointLight
│   ├── 8个怪物
│   │   └── 红色球体+黄色发光眼+尖角
│   └── 200颗星星 (微小白色球体, 高空分布)
│
├── 玩家系统
│   ├── 移动: WASD + 速度6 + Shift冲刺(×1.8)
│   ├── 跳跃: Space → vy=8, 重力18
│   ├── 地形检测: getGroundH(x,z)
│   │   └── 遍历所有岛屿, 检查水平距离<半径×0.8
│   ├── 视角: mousemove → yaw/pitch → Euler'YXZ'
│   ├── 走路摆动: sin(time×0.008) 微量Y偏移
│   └── 攻击: 左键 → Raycaster前方4单位
│       └── 命中怪物: hp-1, 闪红, hp≤0死亡
│
├── 宝石收集
│   ├── 旋转动画: rotation.y += dt×1.5
│   ├── 悬浮动画: sin(time) 上下浮动
│   ├── 拾取判定: 距离<2.5单位
│   └── 拾取效果: 隐藏+分数+50+三音上行音效+消息提示
│
├── 怪物AI
│   ├── 身体浮动: sin(phase) 上下摆动
│   ├── 追踪: 距离<25 → 朝玩家移动
│   ├── 朝向: lookAt(玩家位置)
│   ├── 地形适应: 保持在岛屿表面高度
│   ├── 近战攻击: 距离<1.8 → 造成8点伤害+击退
│   └── 死亡: visible=false (不重生)
│
├── 胜负判定
│   ├── 胜利: 12宝石全收集 + 8怪物全消灭 → +500分
│   └── 失败: HP降到0 → 死亡延迟 → 结算
│
├── HUD
│   ├── HP条 (三段变色: 绿>50 / 黄>25 / 红)
│   ├── 统计: 宝石数/击杀数/分数
│   ├── 物品栏: 💎数量 + ⚔ + ❤数量
│   ├── 小地图: Canvas圆形雷达
│   │   ├── 绿色 = 玩家
│   │   ├── 绿色岛屿区域
│   │   ├── 金色点 = 宝石
│   │   └── 红色点 = 怪物
│   ├── 任务提示: "收集12颗宝石·消灭所有怪物"
│   └── 准星: 6px白色圆环 (攻击时变红放大)
│
└── 受伤反馈
    └── 红色径向渐变闪烁 (200ms)
```

## 关键技术点

### 1. 模块加载问题修复
ES Module (`<script type="module">`) 异步加载，`window.startGame` 在模块执行前不可用。解决方案:
- 开始按钮初始 `disabled`
- Three.js 导入成功后启用按钮
- 使用 `addEventListener('click', ...)` 替代 `onclick="..."` 属性
- 避免 HTML 属性引用模块内函数

### 2. 浮岛地形碰撞
`getGroundH(x, z)` 遍历所有岛屿:
```javascript
for (const isl of islands) {
  if (Math.hypot(x - isl.x, z - isl.z) < isl.r * 0.8)
    maxH = Math.max(maxH, isl.h - 0.3);
}
```
玩家Y坐标低于地面高度时强制回弹。水面设为最低地面 (-0.5+playerHeight)。

### 3. 程序化纹理
草地和石头纹理使用Canvas API生成:
- 草地: 绿色底 + 4000个随机色点 (模拟草叶)
- 石头: 灰色底 + 2000个随机色点 (模拟岩石纹理)
- `CanvasTexture` + `RepeatWrapping` 实现大面积平铺

### 4. 宝石发光效果
每颗宝石使用:
- `OctahedronGeometry` — 八面体几何(水晶外形)
- `emissive + emissiveIntensity: 0.6` — 自发光材质
- `PointLight` — 附近照亮地面的彩色点光源
- HSL 随机色相确保每颗颜色不同

### 5. 状态管理
使用单一 `state` 对象管理所有游戏状态:
```javascript
state = {
  running, hp, score, gemsCollected, kills,  // 游戏进度
  yaw, pitch,                                // 视角
  vy, onGround,                              // 物理
  moveF, moveB, moveL, moveR, sprint, jump,  // 输入
  attackCD,                                   // 攻击冷却
  vel: Vector3                                // 速度向量
}
```
`beginGame()` 完全重置此对象，确保重新开始时状态干净。
