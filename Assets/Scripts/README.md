# Unity测试场景系统

基于Phaser项目TestScene.js的Unity实现，采用ScriptableObject配置驱动的架构设计。

## 项目概述

本项目将Phaser游戏引擎中的测试场景完全迁移到Unity，实现了以下核心功能：
- 战士角色的移动、攻击和技能系统
- 野猪怪物的AI状态机和交互系统
- 完整的HUD界面和UI管理
- 音频系统（背景音乐、音效、环境音）
- 输入系统（键盘、鼠标、手柄支持）
- 事件驱动的解耦架构

## 架构设计

### 核心设计原则
- **配置驱动**: 使用ScriptableObject实现高度可配置的系统
- **低耦合**: 通过事件总线实现组件间解耦通信
- **高可维护性**: 清晰的代码结构和完善的注释
- **可扩展性**: 模块化设计便于功能扩展

### 系统架构图
```
TestSceneController (主控制器)
├── TestSceneConfig (场景配置)
├── EnemySystemConfig (敌人系统配置)
├── HUDConfig (UI配置)
├── InputSystemConfig (输入配置)
├── TestSceneEventBus (事件总线)
├── TestSceneEnemySystem (敌人管理系统)
├── TestSceneUIManager (UI管理系统)
├── TestSceneInputManager (输入管理系统)
└── TestSceneAudioManager (音频管理系统)
```

## 文件结构

### ScriptableObject配置文件
```
Assets/Data/ScriptableObjects/
├── TestSceneConfig.cs          # 测试场景主配置
├── EnemySystemConfig.cs        # 敌人系统配置
├── HUDConfig.cs               # HUD界面配置
└── InputSystemConfig.cs       # 输入系统配置
```

### 系统管理器
```
Assets/Scripts/Systems/
├── TestSceneController.cs      # 主场景控制器
├── TestSceneEventBus.cs       # 事件总线系统
├── TestSceneEnemySystem.cs    # 敌人管理系统
├── TestSceneInputManager.cs   # 输入管理系统
└── TestSceneAudioManager.cs   # 音频管理系统
```

### UI管理器
```
Assets/Scripts/UI/
└── TestSceneUIManager.cs      # UI管理系统
```

### 角色系统
```
Assets/Scripts/Characters/
└── Enemy.cs                   # 敌人基础类
```

## 主要功能模块

### 1. TestSceneController (主控制器)
- **职责**: 统一管理所有子系统的初始化、更新和清理
- **特点**: 异步初始化、配置验证、生命周期管理
- **关键方法**:
  - `InitializeTestSceneAsync()`: 异步初始化场景
  - `ValidateConfigurations()`: 验证配置完整性
  - `CleanupResources()`: 资源清理

### 2. TestSceneEventBus (事件总线)
- **职责**: 提供解耦的事件通信机制
- **特点**: 类型安全、延迟触发、队列处理
- **支持事件类型**:
  - 玩家事件 (攻击、死亡、技能使用)
  - 敌人事件 (生成、死亡、攻击)
  - UI事件 (按钮点击、状态更新)
  - 游戏状态事件 (暂停、开始、结束)

### 3. TestSceneEnemySystem (敌人系统)
- **职责**: 管理敌人的生成、AI行为和生命周期
- **特点**: 波次生成、状态机AI、性能优化
- **核心功能**:
  - 敌人波次生成和配置
  - AI状态机 (巡逻、追击、攻击、冲锋)
  - 敌人生命周期管理
  - 性能优化 (对象池、LOD)

### 4. TestSceneUIManager (UI管理)
- **职责**: 管理所有UI元素和交互
- **特点**: 模块化UI、动画效果、响应式设计
- **UI组件**:
  - HUD (生命值、魔法值、经验值)
  - 技能栏和快捷栏
  - 小地图系统
  - 暂停菜单
  - 通知系统
  - 调试界面

### 5. TestSceneInputManager (输入系统)
- **职责**: 处理所有输入设备的输入
- **特点**: 多设备支持、输入缓冲、组合键
- **支持设备**:
  - 键盘输入
  - 鼠标输入
  - 手柄输入
  - 触摸输入 (移动端扩展)

### 6. TestSceneAudioManager (音频系统)
- **职责**: 管理所有音频播放和控制
- **特点**: 3D音频、音频池、淡入淡出
- **音频类型**:
  - 背景音乐
  - 音效 (攻击、技能、环境)
  - 环境音
  - UI音效

## 配置系统详解

### TestSceneConfig
主场景配置，包含：
- 场景基础设置
- 地图配置
- 玩家配置
- 敌人波次配置
- 音频配置
- UI配置
- 摄像机配置
- 调试设置

### EnemySystemConfig
敌人系统配置，包含：
- 敌人类型配置
- AI状态机配置
- 行为参数配置
- 碰撞检测配置

### HUDConfig
UI界面配置，包含：
- HUD元素配置
- 动画设置
- 布局参数
- 样式配置

### InputSystemConfig
输入系统配置，包含：
- 键位映射
- 输入灵敏度
- 设备支持
- 组合键配置

## 使用指南

### 1. 场景设置
1. 在场景中创建空GameObject
2. 添加TestSceneController组件
3. 配置所需的ScriptableObject资源
4. 设置场景引用 (生成点、摄像机等)

### 2. 配置创建
```csharp
// 创建场景配置
[CreateAssetMenu(fileName = "TestSceneConfig", menuName = "Game/Test Scene Config")]
public class TestSceneConfig : ScriptableObject
{
    // 配置字段...
}
```

### 3. 事件监听
```csharp
// 注册事件监听
eventBus.OnPlayerAttack += OnPlayerAttack;
eventBus.OnEnemyDeath += OnEnemyDeath;

// 触发事件
eventBus.TriggerEvent("PlayerAttack", attackData);
```

### 4. 系统扩展
```csharp
// 扩展新的管理器
public class CustomSystemManager : MonoBehaviour
{
    public void Initialize(TestSceneConfig config, TestSceneEventBus eventBus)
    {
        // 初始化逻辑
    }
}
```

## 性能优化

### 1. 对象池
- 敌人对象复用
- UI元素复用
- 音效源复用

### 2. 事件优化
- 事件队列批处理
- 延迟事件触发
- 自动清理机制

### 3. 渲染优化
- LOD系统
- 视锥剔除
- 批处理优化

### 4. 内存管理
- 资源预加载
- 自动垃圾回收
- 内存池管理

## 调试功能

### 1. 调试UI
- 系统状态显示
- 性能监控
- 实时参数调整

### 2. 日志系统
- 分级日志输出
- 系统状态追踪
- 错误诊断

### 3. 可视化调试
- Gizmos绘制
  - 技能碰撞框可视化（单体、圆形AOE、矩形AOE）
  - 状态可视化
  - 路径显示
- 技能系统调试
  - 技能冷却时间显示
  - 技能范围预览
  - 技能保护状态指示

## 扩展建议

### 1. 新功能添加
- 创建对应的配置ScriptableObject
- 实现管理器类
- 在主控制器中集成
- 添加事件支持

### 2. 平台适配
- 移动端输入适配
- 不同分辨率UI适配
- 性能分级设置

### 3. 多人游戏支持
- 网络同步
- 状态管理
- 事件同步

## 注意事项

### 1. 配置管理
- 确保所有ScriptableObject配置正确引用
- 避免循环引用
- 定期备份配置文件

### 2. 性能监控
- 监控内存使用
- 检查帧率稳定性
- 优化热点代码

### 3. 错误处理
- 添加异常捕获
- 提供降级方案
- 记录错误日志

## 版本历史

### v1.0.0
- 基础架构实现
- 核心系统完成
- 配置系统建立

### 未来计划
- 多人游戏支持
- 更多敌人类型
- 技能系统扩展
- 关卡编辑器

---

**开发团队**: Unity适配AI生成规范项目组  
**最后更新**: 2024年  
**联系方式**: 项目仓库Issues页面