using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// HUD UI配置 ScriptableObject
/// 基于Phaser项目中的GameHudUI设计
/// </summary>
[CreateAssetMenu(fileName = "HUDConfig", menuName = "Game/HUD Config")]
[ShowOdinSerializedPropertiesInInspector]
public class HUDConfig : ScriptableObject
{
    [TabGroup("基础设置")]
    [LabelText("HUD画布预制体")]
    [Required]
    [AssetsOnly]
    [InlineEditor]
    [PropertyOrder(1)]
    public GameObject hudCanvasPrefab;
    
    [TabGroup("基础设置")]
    [LabelText("HUD更新频率")]
    [PropertyRange(0.01f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("HUD界面元素的更新频率，值越小更新越频繁", InfoMessageType.Info)]
    [PropertyOrder(2)]
    public float updateInterval = 0.1f;
    
    [TabGroup("基础设置")]
    [LabelText("HUD淡入淡出时间")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(3)]
    public float fadeTime = 0.3f;
    
    [TabGroup("界面元素")]
    [LabelText("生命值条配置")]
    [InlineProperty]
    [PropertyOrder(4)]
    public HealthBarConfig healthBarConfig;
    
    [TabGroup("界面元素")]
    [LabelText("魔法值条配置")]
    [InlineProperty]
    [PropertyOrder(5)]
    public ManaBarConfig manaBarConfig;
    
    [TabGroup("界面元素")]
    [LabelText("经验值条配置")]
    [InlineProperty]
    [PropertyOrder(6)]
    public ExperienceBarConfig experienceBarConfig;
    
    [TabGroup("界面元素")]
    [LabelText("任务面板配置")]
    [InlineProperty]
    [PropertyOrder(7)]
    public QuestPanelConfig questPanelConfig;
    
    [TabGroup("界面元素")]
    [LabelText("小地图配置")]
    [InlineProperty]
    [PropertyOrder(8)]
    public MinimapConfig minimapConfig;
    
    [TabGroup("界面元素")]
    [LabelText("技能栏配置")]
    [InlineProperty]
    [PropertyOrder(9)]
    public SkillBarConfig skillBarConfig;
    
    [TabGroup("界面元素")]
    [LabelText("快捷物品栏配置")]
    [InlineProperty]
    [PropertyOrder(10)]
    public QuickItemBarConfig quickItemBarConfig;
    
    [TabGroup("特效显示")]
    [LabelText("伤害数字配置")]
    [InlineProperty]
    [PropertyOrder(11)]
    public DamageNumberConfig damageNumberConfig;
    
    [TabGroup("特效显示")]
    [LabelText("状态效果配置")]
    [InlineProperty]
    [PropertyOrder(12)]
    public StatusEffectConfig statusEffectConfig;
    
    [TabGroup("交互界面")]
    [LabelText("聊天窗口配置")]
    [InlineProperty]
    [PropertyOrder(13)]
    public ChatWindowConfig chatWindowConfig;
    
    [TabGroup("调试工具")]
    [LabelText("调试信息配置")]
    [InlineProperty]
    [PropertyOrder(14)]
    public DebugInfoConfig debugInfoConfig;
}

/// <summary>
/// 生命值条配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class HealthBarConfig
{
    [FoldoutGroup("位置和大小", expanded: true)]
    [LabelText("生命值条位置")]
    [PropertyOrder(1)]
    public Vector2 position = new Vector2(50, 50);
    
    [FoldoutGroup("位置和大小")]
    [LabelText("生命值条大小")]
    [PropertyOrder(2)]
    public Vector2 size = new Vector2(200, 20);
    
    [FoldoutGroup("颜色设置", expanded: true)]
    [LabelText("背景色")]
    [PropertyOrder(3)]
    public Color backgroundColor = Color.red;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("前景色")]
    [PropertyOrder(4)]
    public Color foregroundColor = Color.green;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("边框色")]
    [PropertyOrder(5)]
    public Color borderColor = Color.white;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("填充颜色")]
    [PropertyOrder(6)]
    public Color fillColor = Color.green;
    
    [FoldoutGroup("文本设置", expanded: true)]
    [LabelText("显示数值文本")]
    [PropertyOrder(7)]
    public bool showText = true;
    
    [FoldoutGroup("文本设置")]
    [LabelText("文本格式")]
    [InfoBox("使用 {0} 表示当前值，{1} 表示最大值", InfoMessageType.Info)]
    [ShowIf("showText")]
    [PropertyOrder(8)]
    public string textFormat = "{0}/{1}";
    
    [FoldoutGroup("文本设置")]
    [LabelText("文本颜色")]
    [ShowIf("showText")]
    [PropertyOrder(9)]
    public Color textColor = Color.white;
    
    [FoldoutGroup("文本设置")]
    [LabelText("文本字体大小")]
    [PropertyRange(8, 32)]
    [ShowIf("showText")]
    [PropertyOrder(10)]
    public int fontSize = 14;
    
    [FoldoutGroup("动画设置", expanded: true)]
    [LabelText("平滑变化")]
    [PropertyOrder(11)]
    public bool smoothTransition = true;
    
    [FoldoutGroup("动画设置")]
    [LabelText("变化速度")]
    [PropertyRange(0.1f, 20f)]
    [SuffixLabel("单位/秒")]
    [ShowIf("smoothTransition")]
    [PropertyOrder(12)]
    public float transitionSpeed = 5f;
    
    [FoldoutGroup("动画设置")]
    [LabelText("动画持续时间")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(13)]
    public float animationDuration = 0.3f;
}

/// <summary>
/// 魔法值条配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class ManaBarConfig
{
    [FoldoutGroup("位置和大小", expanded: true)]
    [LabelText("魔法值条位置")]
    [PropertyOrder(1)]
    public Vector2 position = new Vector2(50, 80);
    
    [FoldoutGroup("位置和大小")]
    [LabelText("魔法值条大小")]
    [PropertyOrder(2)]
    public Vector2 size = new Vector2(200, 15);
    
    [FoldoutGroup("颜色设置", expanded: true)]
    [LabelText("背景色")]
    [PropertyOrder(3)]
    public Color backgroundColor = Color.gray;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("前景色")]
    [PropertyOrder(4)]
    public Color foregroundColor = Color.blue;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("边框色")]
    [PropertyOrder(5)]
    public Color borderColor = Color.white;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("填充颜色")]
    [PropertyOrder(6)]
    public Color fillColor = Color.blue;
    
    [FoldoutGroup("文本设置", expanded: true)]
    [LabelText("显示数值文本")]
    [PropertyOrder(7)]
    public bool showText = true;
    
    [FoldoutGroup("文本设置")]
    [LabelText("文本格式")]
    [InfoBox("使用 {0} 表示当前值，{1} 表示最大值", InfoMessageType.Info)]
    [ShowIf("showText")]
    [PropertyOrder(8)]
    public string textFormat = "{0}/{1}";
    
    [FoldoutGroup("文本设置")]
    [LabelText("文本颜色")]
    [ShowIf("showText")]
    [PropertyOrder(9)]
    public Color textColor = Color.white;
    
    [FoldoutGroup("文本设置")]
    [LabelText("文本字体大小")]
    [PropertyRange(8, 32)]
    [ShowIf("showText")]
    [PropertyOrder(10)]
    public int fontSize = 12;
    
    [FoldoutGroup("动画设置", expanded: true)]
    [LabelText("平滑变化")]
    [PropertyOrder(11)]
    public bool smoothTransition = true;
    
    [FoldoutGroup("动画设置")]
    [LabelText("变化速度")]
    [PropertyRange(0.1f, 20f)]
    [SuffixLabel("单位/秒")]
    [ShowIf("smoothTransition")]
    [PropertyOrder(12)]
    public float transitionSpeed = 5f;
    
    [FoldoutGroup("动画设置")]
    [LabelText("动画持续时间")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(13)]
    public float animationDuration = 0.3f;
}

/// <summary>
/// 经验值条配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class ExperienceBarConfig
{
    [FoldoutGroup("位置和大小", expanded: true)]
    [LabelText("经验值条位置")]
    [PropertyOrder(1)]
    public Vector2 position = new Vector2(50, 110);
    
    [FoldoutGroup("位置和大小")]
    [LabelText("经验值条大小")]
    [PropertyOrder(2)]
    public Vector2 size = new Vector2(300, 10);
    
    [FoldoutGroup("颜色设置", expanded: true)]
    [LabelText("背景色")]
    [PropertyOrder(3)]
    public Color backgroundColor = Color.gray;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("前景色")]
    [PropertyOrder(4)]
    public Color foregroundColor = Color.yellow;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("填充颜色")]
    [PropertyOrder(5)]
    public Color fillColor = Color.yellow;
    
    [FoldoutGroup("等级文本", expanded: true)]
    [LabelText("显示等级文本")]
    [PropertyOrder(6)]
    public bool showLevelText = true;
    
    [FoldoutGroup("等级文本")]
    [LabelText("等级文本位置")]
    [ShowIf("showLevelText")]
    [PropertyOrder(7)]
    public Vector2 levelTextPosition = new Vector2(10, 110);
    
    [FoldoutGroup("等级文本")]
    [LabelText("等级文本格式")]
    [InfoBox("使用 {0} 表示等级数值", InfoMessageType.Info)]
    [ShowIf("showLevelText")]
    [PropertyOrder(8)]
    public string levelTextFormat = "Lv.{0}";
    
    [FoldoutGroup("等级文本")]
    [LabelText("等级文本颜色")]
    [ShowIf("showLevelText")]
    [PropertyOrder(9)]
    public Color levelTextColor = Color.white;
    
    [FoldoutGroup("等级文本")]
    [LabelText("等级文本字体大小")]
    [PropertyRange(8, 32)]
    [ShowIf("showLevelText")]
    [PropertyOrder(10)]
    public int levelFontSize = 16;
    
    [FoldoutGroup("动画设置", expanded: true)]
    [LabelText("动画持续时间")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(11)]
    public float animationDuration = 0.3f;
}

/// <summary>
/// 任务面板配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class QuestPanelConfig
{
    [FoldoutGroup("位置和大小", expanded: true)]
    [LabelText("任务面板位置")]
    [PropertyOrder(1)]
    public Vector2 position = new Vector2(50, 150);
    
    [FoldoutGroup("位置和大小")]
    [LabelText("任务面板大小")]
    [PropertyOrder(2)]
    public Vector2 size = new Vector2(300, 150);
    
    [FoldoutGroup("颜色设置", expanded: true)]
    [LabelText("任务面板背景色")]
    [PropertyOrder(3)]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    [FoldoutGroup("颜色设置")]
    [LabelText("任务标题颜色")]
    [PropertyOrder(4)]
    public Color titleColor = Color.yellow;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("任务描述颜色")]
    [PropertyOrder(5)]
    public Color descriptionColor = Color.white;
    
    [FoldoutGroup("颜色设置")]
    [LabelText("任务进度颜色")]
    [PropertyOrder(6)]
    public Color progressColor = Color.green;
    
    [FoldoutGroup("显示设置", expanded: true)]
    [LabelText("最大显示任务数")]
    [PropertyRange(1, 10)]
    [InfoBox("同时显示在面板上的最大任务数量", InfoMessageType.Info)]
    [PropertyOrder(7)]
    public int maxQuestDisplay = 3;
    
    [FoldoutGroup("显示设置")]
    [LabelText("任务字体大小")]
    [PropertyRange(8, 24)]
    [PropertyOrder(8)]
    public int fontSize = 12;
}

/// <summary>
/// 小地图配置
/// </summary>
[System.Serializable]
public class MinimapConfig
{
    [Tooltip("小地图位置")]
    public Vector2 position = new Vector2(1600, 50);
    
    [Tooltip("小地图大小")]
    public Vector2 size = new Vector2(200, 200);
    
    [Tooltip("小地图缩放")]
    public float mapScale = 0.1f;
    
    [Tooltip("玩家图标颜色")]
    public Color playerIconColor = Color.green;
    
    [Tooltip("敌人图标颜色")]
    public Color enemyIconColor = Color.red;
    
    [Tooltip("NPC图标颜色")]
    public Color npcIconColor = Color.blue;
    
    [Tooltip("地图边框颜色")]
    public Color borderColor = Color.white;
    
    [Tooltip("地图背景颜色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.5f);
    
    [Tooltip("敌人图标预制体")]
    public GameObject enemyIconPrefab;
}

/// <summary>
/// 技能栏配置
/// </summary>
[System.Serializable]
public class SkillBarConfig
{
    [Tooltip("技能栏位置")]
    public Vector2 position = new Vector2(760, 950);
    
    [Tooltip("技能槽大小")]
    public Vector2 slotSize = new Vector2(50, 50);
    
    [Tooltip("技能槽间距")]
    public float slotSpacing = 10f;
    
    [Tooltip("技能槽数量")]
    public int slotCount = 8;
    
    [Tooltip("技能槽背景色")]
    public Color slotBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    [Tooltip("技能槽边框色")]
    public Color slotBorderColor = Color.white;
    
    [Tooltip("冷却遮罩颜色")]
    public Color cooldownMaskColor = new Color(0, 0, 0, 0.7f);
    
    [Tooltip("显示快捷键")]
    public bool showHotkeys = true;
    
    [Tooltip("快捷键颜色")]
    public Color hotkeyColor = Color.yellow;
}

/// <summary>
/// 快捷物品栏配置
/// </summary>
[System.Serializable]
public class QuickItemBarConfig
{
    [Tooltip("物品栏位置")]
    public Vector2 position = new Vector2(1200, 950);
    
    [Tooltip("物品槽大小")]
    public Vector2 slotSize = new Vector2(40, 40);
    
    [Tooltip("物品槽间距")]
    public float slotSpacing = 5f;
    
    [Tooltip("物品槽数量")]
    public int slotCount = 6;
    
    [Tooltip("物品槽背景色")]
    public Color slotBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
    
    [Tooltip("物品数量文本颜色")]
    public Color quantityTextColor = Color.white;
    
    [Tooltip("物品数量字体大小")]
    public int quantityFontSize = 10;
}

/// <summary>
/// 伤害数字配置
/// </summary>
[System.Serializable]
public class DamageNumberConfig
{
    [Tooltip("伤害数字预制体")]
    public GameObject damageNumberPrefab;
    
    [Tooltip("普通伤害颜色")]
    public Color normalDamageColor = Color.white;
    
    [Tooltip("暴击伤害颜色")]
    public Color criticalDamageColor = Color.red;
    
    [Tooltip("治疗数字颜色")]
    public Color healingColor = Color.green;
    
    [Tooltip("伤害数字字体大小")]
    public int fontSize = 20;
    
    [Tooltip("暴击字体大小")]
    public int criticalFontSize = 28;
    
    [Tooltip("数字显示时间")]
    public float displayTime = 2f;
    
    [Tooltip("数字移动速度")]
    public float moveSpeed = 2f;
    
    [Tooltip("数字移动方向")]
    public Vector2 moveDirection = Vector2.up;
}

/// <summary>
/// 状态效果配置
/// </summary>
[System.Serializable]
public class StatusEffectConfig
{
    [Tooltip("状态效果面板位置")]
    public Vector2 position = new Vector2(400, 50);
    
    [Tooltip("状态图标大小")]
    public Vector2 iconSize = new Vector2(32, 32);
    
    [Tooltip("状态图标间距")]
    public float iconSpacing = 5f;
    
    [Tooltip("最大显示状态数")]
    public int maxStatusDisplay = 10;
    
    [Tooltip("显示剩余时间")]
    public bool showRemainingTime = true;
    
    [Tooltip("时间文本颜色")]
    public Color timeTextColor = Color.white;
    
    [Tooltip("时间文本字体大小")]
    public int timeFontSize = 10;
}

/// <summary>
/// 聊天窗口配置
/// </summary>
[System.Serializable]
public class ChatWindowConfig
{
    [Tooltip("聊天窗口位置")]
    public Vector2 position = new Vector2(50, 400);
    
    [Tooltip("聊天窗口大小")]
    public Vector2 size = new Vector2(400, 200);
    
    [Tooltip("聊天窗口背景色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.6f);
    
    [Tooltip("系统消息颜色")]
    public Color systemMessageColor = Color.yellow;
    
    [Tooltip("普通消息颜色")]
    public Color normalMessageColor = Color.white;
    
    [Tooltip("错误消息颜色")]
    public Color errorMessageColor = Color.red;
    
    [Tooltip("最大消息行数")]
    public int maxMessageLines = 10;
    
    [Tooltip("消息字体大小")]
    public int fontSize = 12;
    
    [Tooltip("自动隐藏时间")]
    public float autoHideTime = 5f;
}

/// <summary>
/// 调试信息配置
/// </summary>
[System.Serializable]
public class DebugInfoConfig
{
    [Tooltip("调试信息位置")]
    public Vector2 position = new Vector2(10, 10);
    
    [Tooltip("调试信息背景色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    [Tooltip("调试信息文本颜色")]
    public Color textColor = Color.white;
    
    [Tooltip("调试信息字体大小")]
    public int fontSize = 14;
    
    [Tooltip("更新频率")]
    public float updateFrequency = 0.5f;
    
    [Tooltip("显示FPS")]
    public bool showFPS = true;
    
    [Tooltip("显示内存使用")]
    public bool showMemoryUsage = true;
    
    [Tooltip("显示玩家位置")]
    public bool showPlayerPosition = true;
    
    [Tooltip("显示敌人数量")]
    public bool showEnemyCount = true;
}