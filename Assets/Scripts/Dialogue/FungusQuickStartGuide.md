# Fungus对话系统快速入门指南

## 目录
1. [基础概念](#基础概念)
2. [快速开始](#快速开始)
3. [创建第一个对话](#创建第一个对话)
4. [高级功能](#高级功能)
5. [与项目集成](#与项目集成)
6. [常见问题](#常见问题)

## 基础概念

### 什么是Fungus？
Fungus是Unity中一个强大的可视化脚本工具，专门用于创建交互式故事、对话系统和视觉小说。它提供了一个直观的节点编辑器，让你无需编程就能创建复杂的对话流程。

### 核心组件
- **Flowchart（流程图）**: 对话逻辑的容器，包含所有的对话块和命令
- **Block（块）**: 对话流程中的一个节点，包含一系列命令
- **Command（命令）**: 具体的操作，如显示文本、播放音效、设置变量等
- **Character（角色）**: 对话中的角色，包含名称、颜色、肖像等信息
- **SayDialog（对话框）**: 显示对话文本的UI组件

## 快速开始

### 1. 使用自动设置工具

我们为你的项目创建了一个自动设置工具，可以快速配置Fungus场景：

1. 在场景中创建一个空的GameObject
2. 添加 `FungusSceneSetup` 组件
3. 在Inspector中配置设置选项
4. 点击 "设置Fungus场景" 按钮或在运行时自动设置

### 2. 手动设置步骤

如果你想手动设置，请按以下步骤操作：

#### 步骤1: 创建Flowchart
1. 在Hierarchy中右键 → Create Empty
2. 重命名为 "MainFlowchart"
3. 添加 `Flowchart` 组件

#### 步骤2: 创建对话框
1. 在Canvas下创建UI对象
2. 添加 `SayDialog` 组件
3. 配置UI元素（文本、按钮、背景等）

#### 步骤3: 创建角色
1. 在Flowchart下创建子对象
2. 添加 `Character` 组件
3. 设置角色名称、颜色等属性

## 创建第一个对话

### 1. 打开Flowchart编辑器
选中Flowchart对象，在Inspector中点击 "Open Flowchart Window" 按钮。

### 2. 创建对话块
1. 在Flowchart窗口中右键 → Add Block
2. 重命名为 "Start"
3. 双击块进入编辑模式

### 3. 添加Say命令
1. 点击 "Add Command" 按钮
2. 选择 "Narrative" → "Say"
3. 在Story Text中输入对话内容
4. 选择说话的角色

### 4. 测试对话
1. 在Start块上右键 → Execute
2. 或者运行游戏，对话会自动开始

## 高级功能

### 1. 选择菜单
```
使用Menu命令创建选择分支：
1. 添加Menu命令
2. 设置选项文本
3. 连接到不同的对话块
```

### 2. 变量系统
```
使用变量存储游戏状态：
1. 在Flowchart中定义变量
2. 使用Set Variable命令修改
3. 使用If命令进行条件判断
```

### 3. 自定义命令
项目中已包含以下自定义命令：
- `GiveItemCommand`: 给予物品
- `GiveExperienceCommand`: 给予经验
- `CheckPlayerLevelCommand`: 检查玩家等级
- `PlaySoundEffectCommand`: 播放音效

### 4. Lua脚本集成
```lua
-- 在Lua命令中可以访问游戏对象
local player = fungus.gameobject.find("Player")
local character = player:GetComponent("Character")
character.currentHealth = character.maxHealth
```

## 与项目集成

### 1. NPC对话
使用 `NPCController` 脚本：
```csharp
// 在NPC对象上添加NPCController组件
// 设置flowchartName和blockName
// 配置交互范围和UI提示
```

### 2. 角色系统集成
使用 `FungusCharacterAdapter` 脚本：
```csharp
// 连接项目角色与Fungus角色
// 自动同步角色信息
// 支持动态对话内容
```

### 3. UI系统集成
UIManager已集成Fungus支持：
- 自动显示/隐藏对话UI
- 处理游戏状态切换
- 管理UI层级

### 4. 游戏状态管理
GameManager已添加对话状态：
- `GameState.Dialogue`: 对话进行中
- `StartDialogue()`: 开始对话
- `EndDialogue()`: 结束对话

## 常见问题

### Q: 对话不显示怎么办？
A: 检查以下几点：
1. SayDialog是否正确配置
2. Canvas是否存在且激活
3. Flowchart是否有Start块
4. 对话块是否连接正确

### Q: 如何创建分支对话？
A: 使用Menu命令：
1. 添加Menu命令
2. 设置选项文本和目标块
3. 创建对应的对话块

### Q: 如何保存对话进度？
A: 使用变量系统：
1. 创建进度变量
2. 在关键点设置变量值
3. 使用If命令检查进度

### Q: 如何添加音效？
A: 使用自定义的PlaySoundEffectCommand：
1. 添加PlaySoundEffect命令
2. 设置音效名称
3. 配置音量和循环

### Q: 如何本地化对话？
A: 使用Fungus的本地化功能：
1. 在Flowchart中启用本地化
2. 设置本地化ID
3. 创建本地化文件

## 示例场景

### 简单NPC对话
```
1. 创建NPC对象
2. 添加NPCController组件
3. 设置flowchartName = "NPCDialogue"
4. 创建对应的Flowchart
5. 添加对话内容
```

### 商店对话
```
1. 使用Menu命令创建选项
2. 连接到购买/出售/离开块
3. 使用自定义命令处理交易
4. 更新玩家金币和物品
```

### 任务对话
```
1. 使用变量跟踪任务状态
2. 根据任务进度显示不同对话
3. 使用If命令进行条件判断
4. 集成任务系统API
```

## 最佳实践

1. **组织结构**: 为不同类型的对话创建单独的Flowchart
2. **命名规范**: 使用清晰的块名和变量名
3. **模块化**: 将常用功能封装为自定义命令
4. **测试**: 经常测试对话流程，确保逻辑正确
5. **性能**: 避免在对话中进行复杂计算
6. **备份**: 定期备份Flowchart配置

## 进阶技巧

### 1. 动态对话内容
```csharp
// 使用代码动态设置对话文本
var flowchart = FindObjectOfType<Flowchart>();
flowchart.SetStringVariable("playerName", GameManager.Instance.GetPlayerName());
```

### 2. 条件对话
```
使用If命令根据游戏状态显示不同对话：
- 检查玩家等级
- 检查物品数量
- 检查任务状态
```

### 3. 对话动画
```
结合Unity动画系统：
- 角色表情变化
- 对话框动画效果
- 背景变化
```

这个指南应该能帮助你快速上手Fungus对话系统。如果遇到问题，可以查看Fungus官方文档或在Unity社区寻求帮助。