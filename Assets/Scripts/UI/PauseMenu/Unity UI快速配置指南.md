# Unity UI 快速配置指南

## 预制体创建清单

### 1. 暂停菜单主预制体 (PauseMenuPrefab)

#### 基础结构
```
PauseMenuCanvas (Canvas)
├── PauseMenuPanel (GameObject + PauseMenuManager)
    ├── Background (Image) 
    │   └── Color: (0,0,0,128) 半透明黑色
    ├── MainPanel (Image)
    │   ├── 大小: 800x600
    │   ├── 锚点: Center
    │   └── 颜色: 深蓝灰色
    ├── Header (GameObject)
    │   └── TitleText (TextMeshPro) "游戏暂停"
    ├── TabContainer (Horizontal Layout Group)
    │   ├── CharacterTab (Button) "角色"
    │   ├── QuestsTab (Button) "任务"  
    │   ├── ItemsTab (Button) "物品"
    │   ├── SkillsTab (Button) "技能"
    │   ├── EquipmentTab (Button) "装备"
    │   ├── AwakeningTab (Button) "觉醒"
    │   └── SettingsTab (Button) "设置"
    ├── ContentContainer (GameObject)
    │   ├── CharacterPanel (GameObject + CharacterPanel)
    │   └── QuestPanel (GameObject + QuestPanel)
    └── ButtonContainer (Horizontal Layout Group)
        ├── ContinueButton (Button) "继续游戏"
        ├── SaveButton (Button) "保存游戏"
        └── ExitButton (Button) "退出游戏"
```

### 2. 角色面板配置

#### UI组件设置
```
CharacterPanel
├── LeftSidebar (Vertical Layout Group)
│   ├── CharacterPortrait (Image) 128x128
│   ├── CharacterName (TextMeshPro) 字号:20
│   ├── CharacterLevel (TextMeshPro) 字号:16
│   ├── CharacterClass (TextMeshPro) 字号:16
│   ├── HealthBar (Slider) 颜色:红色
│   ├── ManaBar (Slider) 颜色:蓝色
│   └── ExpBar (Slider) 颜色:绿色
├── AttributesPanel (Grid Layout Group 2x2)
│   ├── StrengthText (TextMeshPro) "力量: 0"
│   ├── AgilityText (TextMeshPro) "敏捷: 0"
│   ├── IntelligenceText (TextMeshPro) "智力: 0"
│   └── staminaText (TextMeshPro) "体力: 0"
└── CombatStatsPanel (Grid Layout Group 2x3)
    ├── AttackPowerText (TextMeshPro) "攻击力: 0"
    ├── DefenseText (TextMeshPro) "防御力: 0"
    ├── MagicAttackText (TextMeshPro) "魔法攻击: 0"
    ├── MagicDefenseText (TextMeshPro) "魔法防御: 0"
    ├── MoveSpeedText (TextMeshPro) "移动速度: 0"
    └── AttackSpeedText (TextMeshPro) "攻击速度: 0"
```

### 3. 任务面板配置

#### UI组件设置
```
QuestPanel
├── LeftPanel (300px宽)
│   ├── CategoryButtons (Horizontal Layout Group)
│   │   ├── AllQuestsButton (Button) "全部"
│   │   ├── MainQuestsButton (Button) "主线"
│   │   ├── SideQuestsButton (Button) "支线"
│   │   └── CompletedQuestsButton (Button) "已完成"
│   └── QuestScrollRect (Scroll Rect)
│       └── QuestListContainer (Vertical Layout Group)
└── RightPanel (500px宽)
    └── QuestDetailPanel
        ├── QuestTitle (TextMeshPro) 字号:18
        ├── QuestIcon (Image) 64x64
        ├── QuestType (TextMeshPro) 字号:14
        ├── QuestDescription (TextMeshPro) 字号:12
        ├── QuestProgressSlider (Slider)
        ├── ObjectiveListContainer (Vertical Layout Group)
        └── ActionButtons (Horizontal Layout Group)
            ├── AbandonQuestButton (Button) "放弃任务"
            └── TrackQuestButton (Button) "追踪任务"
```

### 4. 预制体创建步骤

#### 步骤1: 创建QuestItem预制体
```
1. 创建空GameObject命名为"QuestItem"
2. 添加Image组件作为背景
3. 添加Button组件
4. 创建子对象:
   - QuestIcon (Image)
   - QuestNameText (TextMeshPro)
   - QuestTypeText (TextMeshPro)  
   - ProgressSlider (Slider)
   - ProgressText (TextMeshPro)
5. 添加QuestItemUI脚本
6. 保存为预制体
```

#### 步骤2: 创建ObjectiveItem预制体
```
1. 创建空GameObject命名为"ObjectiveItem"
2. 添加Horizontal Layout Group
3. 创建子对象:
   - CheckIcon (Image) - 勾选图标
   - ObjectiveText (TextMeshPro)
   - ProgressText (TextMeshPro)
4. 添加ObjectiveItemUI脚本
5. 保存为预制体
```

### 5. 脚本配置清单

#### PauseMenuManager配置
```csharp
[Header("UI References")]
public GameObject pauseMenuPanel;           // 指向PauseMenuPanel
public Transform tabButtonContainer;        // 指向TabContainer
public Transform contentContainer;          // 指向ContentContainer

[Header("Tab Configuration")]
public PauseMenuTab[] tabs = new PauseMenuTab[7]
{
    new PauseMenuTab { tabId = "Character", tabName = "角色", tabButton = null },
    new PauseMenuTab { tabId = "Quests", tabName = "任务", tabButton = null },
    new PauseMenuTab { tabId = "Items", tabName = "物品", tabButton = null },
    new PauseMenuTab { tabId = "Skills", tabName = "技能", tabButton = null },
    new PauseMenuTab { tabId = "Equipment", tabName = "装备", tabButton = null },
    new PauseMenuTab { tabId = "Awakening", tabName = "觉醒", tabButton = null },
    new PauseMenuTab { tabId = "Settings", tabName = "设置", tabButton = null }
};

[Header("Control Buttons")]
public Button continueButton;               // 指向继续游戏按钮
public Button saveButton;                   // 指向保存游戏按钮
public Button exitButton;                   // 指向退出游戏按钮
```

#### CharacterPanel配置
```csharp
[Header("Character Info")]
public Image characterPortrait;             // 角色头像
public TextMeshProUGUI characterName;       // 角色名称
public TextMeshProUGUI characterLevel;      // 角色等级
public TextMeshProUGUI characterClass;      // 角色职业

[Header("Status Bars")]
public Slider healthSlider;                 // 生命值滑条
public TextMeshProUGUI healthText;          // 生命值文本
public Slider manaSlider;                   // 魔法值滑条
public TextMeshProUGUI manaText;            // 魔法值文本
public Slider expSlider;                    // 经验值滑条
public TextMeshProUGUI expText;             // 经验值文本

[Header("Basic Attributes")]
public TextMeshProUGUI strengthText;        // 力量
public TextMeshProUGUI agilityText;         // 敏捷
public TextMeshProUGUI intelligenceText;    // 智力
public TextMeshProUGUI staminaText;        // 体力

[Header("Combat Attributes")]
public TextMeshProUGUI attackPowerText;     // 攻击力
public TextMeshProUGUI defenseText;         // 防御力
public TextMeshProUGUI magicAttackText;     // 魔法攻击
public TextMeshProUGUI magicDefenseText;    // 魔法防御
public TextMeshProUGUI moveSpeedText;       // 移动速度
public TextMeshProUGUI attackSpeedText;     // 攻击速度

[Header("Other Info")]
public TextMeshProUGUI goldText;            // 金币
public TextMeshProUGUI playTimeText;        // 游戏时间
```

#### QuestPanel配置
```csharp
[Header("Category Buttons")]
public Button allQuestsButton;              // 全部任务按钮
public Button mainQuestsButton;             // 主线任务按钮
public Button sideQuestsButton;             // 支线任务按钮
public Button completedQuestsButton;        // 已完成任务按钮

[Header("Quest List")]
public Transform questListContainer;        // 任务列表容器
public GameObject questItemPrefab;          // 任务项预制体
public ScrollRect questScrollRect;          // 滚动视图

[Header("Quest Detail")]
public GameObject questDetailPanel;         // 任务详情面板
public TextMeshProUGUI questTitle;          // 任务标题
public Image questIcon;                     // 任务图标
public TextMeshProUGUI questType;           // 任务类型
public TextMeshProUGUI questDescription;    // 任务描述
public Slider questProgressSlider;          // 任务进度
public TextMeshProUGUI questProgressText;   // 进度文本
public Transform objectiveListContainer;    // 目标列表容器
public GameObject objectiveItemPrefab;      // 目标项预制体
public Button abandonQuestButton;          // 放弃任务按钮
public Button trackQuestButton;             // 追踪任务按钮
```

### 6. 快速设置技巧

#### Layout Group设置
```
Horizontal Layout Group:
- Spacing: 10
- Child Alignment: Middle Center
- Control Child Size: Width ✓, Height ✓
- Use Child Scale: ✓
- Child Force Expand: Width ✓

Vertical Layout Group:
- Spacing: 5
- Child Alignment: Upper Center
- Control Child Size: Width ✓, Height ✗
- Use Child Scale: ✓
- Child Force Expand: Width ✓

Grid Layout Group:
- Cell Size: (150, 30)
- Spacing: (10, 5)
- Start Corner: Upper Left
- Start Axis: Horizontal
- Child Alignment: Upper Left
```

#### Content Size Fitter设置
```
Horizontal Fit: Preferred Size
Vertical Fit: Preferred Size
```

#### Slider设置
```
Min Value: 0
Max Value: 100
Whole Numbers: ✗
Fill Rect: 指向Fill区域
Handle Rect: 可选，简单进度条可不设置
```

### 7. 测试检查清单

- [ ] ESC键能正常打开/关闭暂停菜单
- [ ] 标签页切换正常工作
- [ ] 角色属性正确显示
- [ ] 任务列表能正常显示
- [ ] 任务详情能正确更新
- [ ] 按钮点击有正确响应
- [ ] UI布局在不同分辨率下正常
- [ ] 文本显示完整，无截断
- [ ] 滑条和进度条正常工作
- [ ] 滚动视图能正常滚动

### 8. 常用快捷操作

#### 快速创建UI元素
```
右键 Hierarchy -> UI -> 选择对应组件
- Image: 创建图片
- Button: 创建按钮  
- Text - TextMeshPro: 创建文本
- Slider: 创建滑条
- Scroll View: 创建滚动视图
```

#### 快速复制UI设置
```
1. 选中源UI元素
2. 右键 -> Copy Component
3. 选中目标UI元素
4. 右键 -> Paste Component Values
```

#### 批量设置字体
```
1. 选中所有TextMeshPro组件
2. 在Inspector中修改Font Asset
3. 所有选中的组件会同时更新
```

这个快速配置指南可以帮助您更高效地在Unity中创建暂停菜单系统。建议按照清单逐步完成，确保每个步骤都正确配置。