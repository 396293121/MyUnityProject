# Fungus与项目集成完整指南

## 概述

本文档详细说明了如何将Fungus对话系统完全集成到你的Unity项目中。我们已经为你创建了完整的集成方案，包括脚本、配置和使用指南。

## 已完成的集成工作

### 1. 核心系统修改

#### GameManager扩展
- ✅ 添加了 `GameState.Dialogue` 对话状态
- ✅ 新增 `StartDialogue()` 和 `EndDialogue()` 方法
- ✅ 添加 `CanStartDialogue()` 状态检查方法

#### UIManager扩展
- ✅ 添加了 `fungusDialogPanel` 对话UI面板引用
- ✅ 实现了 `ShowDialogueUI()` 和 `HideDialogueUI()` 方法
- ✅ 添加了对话开始/结束时的UI处理逻辑
- ✅ 支持游戏UI的半透明显示，避免对话时的干扰

### 2. 对话系统组件

#### DialogueManager（对话管理器）
位置：`Assets/Scripts/Dialogue/DialogueManager.cs`

**功能特性：**
- 🎯 统一管理游戏中的所有对话
- 🎯 与GameManager和UIManager无缝集成
- 🎯 支持对话事件系统
- 🎯 提供对话状态管理
- 🎯 支持音效播放
- 🎯 提供变量设置和获取接口

**主要方法：**
```csharp
// 开始对话
DialogueManager.Instance.StartDialogue("FlowchartName", "BlockName");

// 结束对话
DialogueManager.Instance.EndDialogue();

// 设置对话变量
DialogueManager.Instance.SetDialogueVariable("playerName", "张三");

// 获取对话变量
string value = DialogueManager.Instance.GetDialogueVariable("questStatus");
```

#### NPCController（NPC控制器）
位置：`Assets/Scripts/Dialogue/NPCController.cs`

**功能特性：**
- 🎯 处理NPC与玩家的交互
- 🎯 支持多种NPC类型（商人、任务给予者、普通NPC等）
- 🎯 自动检测玩家接近
- 🎯 显示交互提示UI
- 🎯 集成Fungus对话系统

**使用方法：**
1. 在NPC对象上添加 `NPCController` 组件
2. 设置NPC基础信息（名称、类型、描述）
3. 配置对话信息（流程图名称、对话块名称）
4. 设置交互范围和UI提示

#### FungusCharacterAdapter（角色适配器）
位置：`Assets/Scripts/Dialogue/FungusCharacterAdapter.cs`

**功能特性：**
- 🎯 连接项目现有角色系统与Fungus角色
- 🎯 自动同步角色信息
- 🎯 支持动态角色配置
- 🎯 提供角色信息获取接口

### 3. 自定义Fungus命令

#### CustomFungusCommands
位置：`Assets/Scripts/Dialogue/CustomFungusCommands.cs`

**包含命令：**

1. **GiveItemCommand** - 给予物品
   ```csharp
   // 在Fungus中使用：给予玩家指定物品
   itemName: "生命药水"
   quantity: 5
   ```

2. **GiveExperienceCommand** - 给予经验
   ```csharp
   // 在Fungus中使用：给予玩家经验值
   experienceAmount: 100
   ```

3. **CheckPlayerLevelCommand** - 检查玩家等级
   ```csharp
   // 在Fungus中使用：根据玩家等级跳转对话
   targetLevel: 10
   compareType: GreaterThanOrEqual
   targetBlock: "HighLevelDialogue"
   ```

4. **PlaySoundEffectCommand** - 播放音效
   ```csharp
   // 在Fungus中使用：播放指定音效
   soundName: "coin_pickup"
   volume: 0.8f
   ```

### 4. 场景设置工具

#### FungusSceneSetup
位置：`Assets/Scripts/Dialogue/FungusSceneSetup.cs`

**功能特性：**
- 🎯 一键设置Fungus对话场景
- 🎯 自动创建必要的组件
- 🎯 配置默认角色和对话框
- 🎯 支持场景清理功能

**使用方法：**
1. 在场景中创建空对象
2. 添加 `FungusSceneSetup` 组件
3. 配置设置选项
4. 点击 "设置Fungus场景" 按钮

## 集成步骤指南

### 第一步：基础设置

1. **确认Fungus已导入**
   - 检查 `Assets/Fungus` 文件夹是否存在
   - 确认所有Fungus脚本正常编译

2. **设置场景**
   - 使用 `FungusSceneSetup` 工具自动设置
   - 或按照快速入门指南手动设置

### 第二步：创建第一个NPC对话

1. **创建NPC对象**
   ```csharp
   // 1. 创建空对象，命名为 "TestNPC"
   // 2. 添加Collider2D组件（设置为Trigger）
   // 3. 添加NPCController组件
   ```

2. **配置NPCController**
   ```csharp
   // NPC基础信息
   npcName = "村民老王";
   npcType = NPCType.普通NPC;
   description = "一个友善的村民";
   
   // 对话配置
   flowchartName = "VillagerDialogue";
   blockName = "Start";
   
   // 交互设置
   interactionRange = 2f;
   requireKeyPress = true;
   interactionKey = KeyCode.E;
   ```

3. **创建对话流程图**
   ```csharp
   // 1. 在场景中创建空对象，命名为 "VillagerDialogue"
   // 2. 添加Flowchart组件
   // 3. 打开Flowchart编辑器
   // 4. 创建Start块
   // 5. 添加Say命令，输入对话内容
   ```

### 第三步：高级对话功能

1. **使用自定义命令**
   ```csharp
   // 在Flowchart中添加自定义命令
   // 例如：给予经验值
   Add Command → Custom → Give Experience
   Experience Amount: 50
   ```

2. **条件对话**
   ```csharp
   // 使用If命令检查条件
   Add Command → Flow → If
   Variable: playerLevel
   Compare: Greater Than
   Value: 5
   Target Block: HighLevelDialogue
   ```

3. **选择菜单**
   ```csharp
   // 创建选择分支
   Add Command → Narrative → Menu
   Text: "你想做什么？"
   Add Option: "购买物品" → ShopDialogue
   Add Option: "接受任务" → QuestDialogue
   Add Option: "离开" → End
   ```

### 第四步：与现有系统集成

1. **角色系统集成**
   ```csharp
   // 在角色对象上添加FungusCharacterAdapter
   var adapter = character.gameObject.AddComponent<FungusCharacterAdapter>();
   adapter.fungusCharacter = fungusCharacterComponent;
   adapter.projectCharacter = characterComponent;
   ```

2. **UI系统集成**
   ```csharp
   // UIManager已自动集成，只需在Inspector中设置
   // fungusDialogPanel引用到Fungus对话UI
   ```

3. **音频系统集成**
   ```csharp
   // 在DialogueManager中配置音效
   dialogueStartSound = "dialogue_start";
   dialogueEndSound = "dialogue_end";
   ```

## 使用示例

### 示例1：简单NPC对话

```csharp
// 1. 创建NPC
GameObject npc = new GameObject("Villager");
NPCController npcController = npc.AddComponent<NPCController>();

// 2. 配置NPC
npcController.npcName = "村民";
npcController.flowchartName = "VillagerChat";
npcController.blockName = "Greeting";

// 3. 在Flowchart中创建对话
// Say: "你好，欢迎来到我们的村庄！"
// Say: "有什么我可以帮助你的吗？"
```

### 示例2：商店NPC

```csharp
// 1. 设置商店NPC
npcController.npcType = NPCType.商人;
npcController.flowchartName = "ShopDialogue";

// 2. 在Flowchart中创建商店对话
// Say: "欢迎光临我的商店！"
// Menu: "你想要什么？"
//   - "购买物品" → BuyItems
//   - "出售物品" → SellItems
//   - "离开" → End
```

### 示例3：任务NPC

```csharp
// 1. 设置任务NPC
npcController.npcType = NPCType.任务给予者;
npcController.flowchartName = "QuestGiver";

// 2. 使用条件对话
// If: questCompleted == false
//   Say: "我有一个任务给你..."
//   Give Experience: 100
//   Set Variable: questCompleted = true
// Else:
//   Say: "感谢你完成了任务！"
```

## 调试和测试

### 1. 调试工具

```csharp
// 启用调试模式
DialogueManager.Instance.debugMode = true;
NPCController.debugMode = true;

// 查看调试信息
Debug.Log("[Dialogue] 当前对话状态: " + DialogueManager.Instance.IsInDialogue);
```

### 2. 常见问题排查

**对话不触发：**
- 检查NPCController的交互范围设置
- 确认Flowchart名称和Block名称正确
- 检查玩家是否在交互范围内

**UI显示异常：**
- 确认UIManager中的fungusDialogPanel已设置
- 检查Canvas设置和层级
- 确认SayDialog组件配置正确

**音效不播放：**
- 检查AudioManager是否存在
- 确认音效文件路径正确
- 检查音量设置

## 性能优化建议

1. **对话缓存**
   ```csharp
   // DialogueManager已实现Flowchart缓存
   // 避免重复查找提升性能
   ```

2. **UI优化**
   ```csharp
   // 使用对象池管理对话UI
   // 避免频繁创建销毁UI对象
   ```

3. **内存管理**
   ```csharp
   // 及时清理不用的对话资源
   // 使用Resources.UnloadUnusedAssets()
   ```

## 扩展功能

### 1. 自定义命令开发

```csharp
// 创建新的自定义命令
[CommandInfo("Custom", "My Command", "自定义命令描述")]
public class MyCustomCommand : Command
{
    public override void OnEnter()
    {
        // 命令逻辑
        Continue();
    }
}
```

### 2. 对话事件系统

```csharp
// 监听对话事件
DialogueManager.Instance.OnDialogueStart += OnDialogueStarted;
DialogueManager.Instance.OnDialogueEnd += OnDialogueEnded;

private void OnDialogueStarted()
{
    // 对话开始时的处理
}

private void OnDialogueEnded()
{
    // 对话结束时的处理
}
```

### 3. 本地化支持

```csharp
// 在Flowchart中启用本地化
flowchart.localizationId = "dialogue_localization";

// 创建本地化文件
// 支持多语言对话内容
```

## 总结

通过以上集成工作，你的项目现在已经完全支持Fungus对话系统：

✅ **核心系统集成完成** - GameManager、UIManager已扩展支持对话
✅ **对话管理器就绪** - 统一的对话管理和状态控制
✅ **NPC交互系统** - 完整的NPC对话交互功能
✅ **自定义命令** - 扩展的游戏逻辑命令
✅ **场景设置工具** - 快速设置和配置工具
✅ **详细文档** - 完整的使用指南和示例

现在你可以开始创建丰富的对话内容，为你的游戏添加生动的NPC交互和故事情节！