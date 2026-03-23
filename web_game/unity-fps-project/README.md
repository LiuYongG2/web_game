# Unity FPS 项目 — 战术突击

## 快速开始

1. 安装 Unity 2022.3 LTS 或更高版本
2. 在 Unity Hub 中点击 "Open" → 选择本 `unity-fps-project` 目录
3. Unity 会自动导入所有脚本
4. 打开 `Assets/Scenes/MainMenu.unity` 场景
5. 点击 Play 运行

## 项目结构

```
Assets/
├── Scripts/
│   ├── Player/
│   │   ├── PlayerController.cs    ← 第一人称移动+跳跃+滑铲+蹲
│   │   ├── PlayerCamera.cs        ← 鼠标视角控制
│   │   ├── PlayerHealth.cs        ← HP/护甲系统
│   │   └── PlayerInventory.cs     ← 武器背包管理
│   ├── Weapons/
│   │   ├── WeaponBase.cs          ← 武器基类(射速/弹药/后坐力)
│   │   ├── WeaponManager.cs       ← 武器切换/装备
│   │   ├── Projectile.cs          ← 子弹/射线检测
│   │   └── MuzzleFlash.cs         ← 枪口焰火特效
│   ├── AI/
│   │   ├── AIController.cs        ← 敌人AI状态机(巡逻/追击/射击/掩护)
│   │   ├── AINavigation.cs        ← NavMesh寻路
│   │   └── AITeamManager.cs       ← 团队AI协调
│   ├── GameModes/
│   │   ├── GameModeBase.cs        ← 游戏模式基类
│   │   ├── FreeForAll.cs          ← 自由射击模式
│   │   ├── TeamDeathmatch.cs      ← 团队竞技
│   │   └── BombDefusal.cs         ← 拆弹模式
│   ├── UI/
│   │   ├── HUDManager.cs          ← HUD总控
│   │   ├── WeaponSelectionUI.cs   ← 武器选择界面
│   │   ├── CharacterSelectUI.cs   ← 角色选择界面
│   │   └── ScoreboardUI.cs        ← 计分板
│   ├── Audio/
│   │   └── AudioManager.cs        ← 音效管理器
│   └── Maps/
│       └── MapManager.cs          ← 地图加载管理
├── Scenes/
│   ├── MainMenu.unity             ← 主菜单(需手动创建)
│   ├── Map_Warehouse.unity        ← 仓库地图(需手动创建)
│   ├── Map_Desert.unity           ← 沙漠地图(需手动创建)
│   └── Map_Factory.unity          ← 工厂地图(需手动创建)
├── Prefabs/                       ← 预制体(需在Unity中创建)
├── Materials/                     ← 材质(需在Unity中创建)
└── Audio/                         ← 音效文件(需导入)
```

## 需要在 Unity 编辑器中手动完成的步骤

1. **创建场景**：新建 4 个场景文件（MainMenu + 3 张地图）
2. **配置 NavMesh**：每张地图烘焙 Navigation Mesh
3. **创建预制体**：将脚本组件拖到 GameObject 上
4. **导入素材**：从 Unity Asset Store 导入角色模型、武器模型、环境素材
5. **配置 Input System**：使用 Unity 新输入系统或 Legacy Input

## 推荐 Asset Store 免费资源

- **FPS Controller**: Unity Starter Assets (官方免费)
- **武器模型**: Low Poly Weapons (免费)
- **角色模型**: Mixamo (免费动画+模型)
- **环境**: Unity HDRP Sample Scene
- **音效**: Universal Sound FX (免费包)
