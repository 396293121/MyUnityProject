using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// HUD UI配置 ScriptableObject
/// 基于Phaser项目中的GameHudUI设计
/// </summary>
[CreateAssetMenu(fileName = "HUDConfig", menuName = "Game/HUD Config")]
public class HUDConfig : ScriptableObject
{
    [Header("HUD基础设置")]
    [Tooltip("HUD画布预制体")]
    public GameObject hudCanvasPrefab;
    
    [Tooltip("HUD更新频率(秒)")]
    public float updateInterval = 0.1f;
    
    [Tooltip("HUD淡入淡出时间")]
    public float fadeTime = 0.3f;
    
    [Header("生命值显示")]
    [Tooltip("生命值条配置")]
    public HealthBarConfig healthBarConfig;
    
    [Header("魔法值显示")]
    [Tooltip("魔法值条配置")]
    public ManaBarConfig manaBarConfig;
    
    [Header("经验值显示")]
    [Tooltip("经验值条配置")]
    public ExperienceBarConfig experienceBarConfig;
    
    [Header("任务显示")]
    [Tooltip("任务面板配置")]
    public QuestPanelConfig questPanelConfig;
    
    [Header("小地图显示")]
    [Tooltip("小地图配置")]
    public MinimapConfig minimapConfig;
    
    [Header("技能栏显示")]
    [Tooltip("技能栏配置")]
    public SkillBarConfig skillBarConfig;
    
    [Header("物品栏显示")]
    [Tooltip("快捷物品栏配置")]
    public QuickItemBarConfig quickItemBarConfig;
    
    [Header("伤害数字显示")]
    [Tooltip("伤害数字配置")]
    public DamageNumberConfig damageNumberConfig;
    
    [Header("状态效果显示")]
    [Tooltip("状态效果配置")]
    public StatusEffectConfig statusEffectConfig;
    
    [Header("聊天窗口")]
    [Tooltip("聊天窗口配置")]
    public ChatWindowConfig chatWindowConfig;
    
    [Header("调试信息")]
    [Tooltip("调试信息配置")]
    public DebugInfoConfig debugInfoConfig;
}

/// <summary>
/// 生命值条配置
/// </summary>
[System.Serializable]
public class HealthBarConfig
{
    [Tooltip("生命值条位置")]
    public Vector2 position = new Vector2(50, 50);
    
    [Tooltip("生命值条大小")]
    public Vector2 size = new Vector2(200, 20);
    
    [Tooltip("生命值条背景色")]
    public Color backgroundColor = Color.red;
    
    [Tooltip("生命值条前景色")]
    public Color foregroundColor = Color.green;
    
    [Tooltip("生命值条边框色")]
    public Color borderColor = Color.white;
    
    [Tooltip("显示数值文本")]
    public bool showText = true;
    
    [Tooltip("文本格式")]
    public string textFormat = "{0}/{1}";
    
    [Tooltip("文本颜色")]
    public Color textColor = Color.white;
    
    [Tooltip("文本字体大小")]
    public int fontSize = 14;
    
    [Tooltip("平滑变化")]
    public bool smoothTransition = true;
    
    [Tooltip("变化速度")]
    public float transitionSpeed = 5f;
    
    [Tooltip("填充颜色")]
    public Color fillColor = Color.green;
    
    [Tooltip("动画持续时间")]
    public float animationDuration = 0.3f;
}

/// <summary>
/// 魔法值条配置
/// </summary>
[System.Serializable]
public class ManaBarConfig
{
    [Tooltip("魔法值条位置")]
    public Vector2 position = new Vector2(50, 80);
    
    [Tooltip("魔法值条大小")]
    public Vector2 size = new Vector2(200, 15);
    
    [Tooltip("魔法值条背景色")]
    public Color backgroundColor = Color.gray;
    
    [Tooltip("魔法值条前景色")]
    public Color foregroundColor = Color.blue;
    
    [Tooltip("魔法值条边框色")]
    public Color borderColor = Color.white;
    
    [Tooltip("显示数值文本")]
    public bool showText = true;
    
    [Tooltip("文本格式")]
    public string textFormat = "{0}/{1}";
    
    [Tooltip("文本颜色")]
    public Color textColor = Color.white;
    
    [Tooltip("文本字体大小")]
    public int fontSize = 12;
    
    [Tooltip("平滑变化")]
    public bool smoothTransition = true;
    
    [Tooltip("变化速度")]
    public float transitionSpeed = 5f;
    
    [Tooltip("填充颜色")]
    public Color fillColor = Color.blue;
    
    [Tooltip("动画持续时间")]
    public float animationDuration = 0.3f;
}

/// <summary>
/// 经验值条配置
/// </summary>
[System.Serializable]
public class ExperienceBarConfig
{
    [Tooltip("经验值条位置")]
    public Vector2 position = new Vector2(50, 110);
    
    [Tooltip("经验值条大小")]
    public Vector2 size = new Vector2(300, 10);
    
    [Tooltip("经验值条背景色")]
    public Color backgroundColor = Color.gray;
    
    [Tooltip("经验值条前景色")]
    public Color foregroundColor = Color.yellow;
    
    [Tooltip("显示等级文本")]
    public bool showLevelText = true;
    
    [Tooltip("等级文本位置")]
    public Vector2 levelTextPosition = new Vector2(10, 110);
    
    [Tooltip("等级文本格式")]
    public string levelTextFormat = "Lv.{0}";
    
    [Tooltip("等级文本颜色")]
    public Color levelTextColor = Color.white;
    
    [Tooltip("等级文本字体大小")]
    public int levelFontSize = 16;
    
    [Tooltip("填充颜色")]
    public Color fillColor = Color.yellow;
    
    [Tooltip("动画持续时间")]
    public float animationDuration = 0.3f;
}

/// <summary>
/// 任务面板配置
/// </summary>
[System.Serializable]
public class QuestPanelConfig
{
    [Tooltip("任务面板位置")]
    public Vector2 position = new Vector2(50, 150);
    
    [Tooltip("任务面板大小")]
    public Vector2 size = new Vector2(300, 150);
    
    [Tooltip("任务面板背景色")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    [Tooltip("任务标题颜色")]
    public Color titleColor = Color.yellow;
    
    [Tooltip("任务描述颜色")]
    public Color descriptionColor = Color.white;
    
    [Tooltip("任务进度颜色")]
    public Color progressColor = Color.green;
    
    [Tooltip("最大显示任务数")]
    public int maxQuestDisplay = 3;
    
    [Tooltip("任务字体大小")]
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