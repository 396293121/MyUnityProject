# Fungus对话系统集成指南

## 目录
1. [Fungus简介](#fungus简介)
2. [项目集成步骤](#项目集成步骤)
3. [基础使用教程](#基础使用教程)
4. [与现有项目适配](#与现有项目适配)
5. [高级功能](#高级功能)
6. [常见问题解决](#常见问题解决)
7. [最佳实践](#最佳实践)

## Fungus简介

Fungus是Unity中最流行的可视化对话系统插件，它提供了：
- 可视化的流程图编辑器
- 丰富的对话命令
- 角色管理系统
- 本地化支持
- Lua脚本集成

### 核心组件
- **Flowchart**: 流程图容器，管理对话流程
- **Block**: 对话块，包含一系列命令
- **Command**: 具体的对话命令（Say、Menu、If等）
- **Character**: 角色定义
- **SayDialog**: 对话框UI

## 项目集成步骤

### 1. 检查现有集成
您的项目已经包含了Fungus插件，位于：
```
Assets/Fungus/
```

### 2. 创建基础对话场景

#### 2.1 创建Flowchart
1. 在场景中创建空GameObject，命名为"DialogueManager"
2. 添加Flowchart组件：`Add Component > Fungus > Flowchart`
3. 在Inspector中配置Flowchart属性

#### 2.2 创建对话UI
1. 创建Canvas：`GameObject > UI > Canvas`
2. 添加SayDialog预制体或手动创建对话框UI
3. 配置对话框组件

### 3. 与现有角色系统集成

#### 3.1 扩展Character类
由于您的项目已有Character基类，需要创建适配器：

```csharp
// 创建 FungusCharacterAdapter.cs
using UnityEngine;
using Fungus;

public class FungusCharacterAdapter : MonoBehaviour
{
    [Header("Fungus角色配置")]
    public Fungus.Character fungusCharacter;
    
    [Header("项目角色引用")]
    public Character projectCharacter;
    
    void Start()
    {
        // 同步角色信息
        if (fungusCharacter != null && projectCharacter != null)
        {
            // 可以在这里同步角色名称、头像等信息
        }
    }
    
    public void StartDialogue(string flowchartName, string blockName)
    {
        var flowchart = FindObjectOfType<Flowchart>();
        if (flowchart != null)
        {
            flowchart.ExecuteBlock(blockName);
        }
    }
}
```

#### 3.2 修改PlayerController集成对话
在您的PlayerController中添加对话交互：

```csharp
// 在PlayerController.cs中添加
[Header("对话系统")]
public KeyCode dialogueKey = KeyCode.E;
public float dialogueRange = 2f;

private void Update()
{
    // 现有的Update代码...
    
    // 检查对话交互
    if (Input.GetKeyDown(dialogueKey))
    {
        CheckForDialogue();
    }
}

private void CheckForDialogue()
{
    Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, dialogueRange);
    
    foreach (var obj in nearbyObjects)
    {
        var npc = obj.GetComponent<NPCController>();
        if (npc != null)
        {
            npc.StartDialogue();
            break;
        }
    }
}
```

### 4. 创建NPC控制器

```csharp
// 创建 NPCController.cs
using UnityEngine;
using Fungus;

public class NPCController : MonoBehaviour
{
    [Header("NPC配置")]
    public string npcName = "NPC";
    public Sprite npcPortrait;
    
    [Header("对话配置")]
    public string flowchartName = "NPCDialogue";
    public string startBlockName = "Start";
    
    [Header("交互提示")]
    public GameObject interactionPrompt;
    
    private bool playerInRange = false;
    private Flowchart dialogueFlowchart;
    
    void Start()
    {
        // 查找对话流程图
        dialogueFlowchart = GameObject.Find(flowchartName)?.GetComponent<Flowchart>();
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    public void StartDialogue()
    {
        if (playerInRange && dialogueFlowchart != null)
        {
            // 暂停游戏
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
            
            // 开始对话
            dialogueFlowchart.ExecuteBlock(startBlockName);
        }
    }
    
    // 对话结束时调用
    public void OnDialogueEnd()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
}
```

## 基础使用教程

### 1. 创建简单对话

1. **打开Flowchart窗口**：
   - 选择包含Flowchart组件的GameObject
   - 点击Inspector中的"Open Flowchart Window"

2. **创建对话块**：
   - 右键点击空白区域 > "Add Block"
   - 重命名为"Greeting"

3. **添加Say命令**：
   - 选择Block，点击"+"按钮
   - 选择"Narrative > Say"
   - 输入对话文本

4. **设置角色**：
   - 在Say命令中设置Character字段
   - 可以设置Portrait（头像）

### 2. 创建选择菜单

1. **添加Menu命令**：
   - 在Block中添加"Narrative > Menu"
   - 设置菜单文本

2. **添加选项**：
   - 点击"Add Option"
   - 设置选项文本和目标Block

### 3. 使用变量系统

1. **创建变量**：
   - 在Flowchart的Variables中添加变量
   - 支持Boolean、Integer、Float、String类型

2. **设置变量**：
   - 使用"Variable > Set Variable"命令

3. **条件判断**：
   - 使用"Flow > If"命令进行条件判断

## 与现有项目适配

### 1. UI系统集成

修改您的UIManager以支持Fungus对话：

```csharp
// 在UIManager.cs中添加
[Header("Fungus对话系统")]
public GameObject fungusDialogPanel;

public void ShowFungusDialog()
{
    if (fungusDialogPanel != null)
    {
        fungusDialogPanel.SetActive(true);
        // 隐藏其他UI面板
        HideGameplayUI();
    }
}

public void HideFungusDialog()
{
    if (fungusDialogPanel != null)
    {
        fungusDialogPanel.SetActive(false);
        // 显示游戏UI
        ShowGameplayUI();
    }
}

private void HideGameplayUI()
{
    if (gameplayPanel != null)
        gameplayPanel.SetActive(false);
}

private void ShowGameplayUI()
{
    if (gameplayPanel != null)
        gameplayPanel.SetActive(true);
}
```

### 2. 游戏状态管理

在GameManager中添加对话状态：

```csharp
// 在GameState枚举中添加
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Dialogue,  // 新增对话状态
    GameOver
}

// 在GameManager中添加方法
public void StartDialogue()
{
    ChangeGameState(GameState.Dialogue);
    Time.timeScale = 0f; // 暂停游戏
}

public void EndDialogue()
{
    ChangeGameState(GameState.Playing);
    Time.timeScale = 1f; // 恢复游戏
}
```

### 3. 存档系统集成

如果需要保存对话进度：

```csharp
// 创建 DialogueSaveData.cs
[System.Serializable]
public class DialogueSaveData
{
    public string currentFlowchart;
    public string currentBlock;
    public bool[] dialogueFlags;
    public Dictionary<string, object> variables;
}

// 在SaveSystem中添加对话数据保存
public void SaveDialogueData()
{
    var flowchart = FindObjectOfType<Flowchart>();
    if (flowchart != null)
    {
        DialogueSaveData dialogueData = new DialogueSaveData();
        // 保存当前对话状态
        // 实现具体的保存逻辑
    }
}
```

## 高级功能

### 1. 自定义命令

创建自定义Fungus命令：

```csharp
// 创建 CustomGiveItemCommand.cs
using UnityEngine;
using Fungus;

[CommandInfo("Custom", "Give Item", "给玩家物品")]
public class CustomGiveItemCommand : Command
{
    [SerializeField] protected string itemName;
    [SerializeField] protected int quantity = 1;
    
    public override void OnEnter()
    {
        // 调用您的物品系统
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            var inventory = player.GetComponent<Inventory>();
            if (inventory != null)
            {
                // inventory.AddItem(itemName, quantity);
                Debug.Log($"给予玩家 {quantity} 个 {itemName}");
            }
        }
        
        Continue();
    }
    
    public override string GetSummary()
    {
        return $"给予 {quantity} 个 {itemName}";
    }
}
```

### 2. Lua脚本集成

如果启用了FungusLua：

```lua
-- 在Lua脚本中调用Unity方法
function givePlayerExperience(amount)
    gamemanager = fungus.gamemanager
    gamemanager:AddPlayerExperience(amount)
end

function checkPlayerLevel()
    gamemanager = fungus.gamemanager
    return gamemanager:GetPlayerLevel()
end
```

### 3. 本地化支持

1. **设置本地化**：
   - 在Flowchart中设置Localization Id
   - 创建本地化文件

2. **多语言文本**：
   - 使用Localization组件
   - 支持CSV格式的翻译文件

## 常见问题解决

### 1. 对话框不显示
- 检查SayDialog组件是否正确配置
- 确认Canvas设置正确
- 检查UI层级关系

### 2. 角色头像不显示
- 确认Character组件中设置了Portraits
- 检查Sprite导入设置
- 验证Portrait字段引用

### 3. 对话无法继续
- 检查Block连接是否正确
- 确认Continue按钮事件绑定
- 验证条件判断逻辑

### 4. 变量不生效
- 检查变量名称拼写
- 确认变量作用域
- 验证变量类型匹配

## 最佳实践

### 1. 项目结构
```
Assets/
├── Fungus/                 # Fungus插件
├── Scripts/
│   ├── Dialogue/          # 对话相关脚本
│   │   ├── NPCController.cs
│   │   ├── DialogueManager.cs
│   │   └── CustomCommands/
│   └── ...
├── Prefabs/
│   ├── UI/
│   │   ├── DialogueCanvas.prefab
│   │   └── NPCInteraction.prefab
│   └── Characters/
└── Scenes/
    ├── DialogueTest.scene  # 对话测试场景
    └── ...
```

### 2. 命名规范
- Flowchart: `NPC_[角色名]_Dialogue`
- Block: `Start`, `Greeting`, `Quest_Accept`等
- Variables: `player_level`, `quest_completed`等

### 3. 性能优化
- 合理使用对话缓存
- 避免在Update中频繁查找Flowchart
- 使用对象池管理对话UI

### 4. 调试技巧
- 使用Flowchart的Debug模式
- 在Say命令中添加调试信息
- 利用Unity Console查看Fungus日志

## 示例场景设置

### 1. 创建测试NPC
1. 创建空GameObject，命名为"TestNPC"
2. 添加Sprite Renderer和Collider2D
3. 添加NPCController脚本
4. 设置触发器范围

### 2. 创建对话流程图
1. 创建Flowchart GameObject
2. 设置基础对话流程
3. 添加角色和头像
4. 测试对话功能

### 3. 集成到主场景
1. 将对话系统添加到主游戏场景
2. 配置UI层级
3. 测试与现有系统的兼容性

通过以上指南，您应该能够成功将Fungus对话系统集成到您的项目中。建议先在测试场景中熟悉基础功能，然后逐步集成到主项目中。