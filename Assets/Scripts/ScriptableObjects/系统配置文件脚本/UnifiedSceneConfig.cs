using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 统一场景配置 - 适用于所有游戏关卡的通用配置系统
/// 整合了原有的多个配置文件，减少重复和冲突
/// 支持未来扩展（任务、对话等）
/// </summary>
[CreateAssetMenu(fileName = "UnifiedSceneConfig", menuName = "Game/Unified Scene Config")]

public class UnifiedSceneConfig : ScriptableObject
{
    #region 场景基础信息
    [TitleGroup("基础配置")]
    [FoldoutGroup("基础配置/场景信息", expanded: true)]
    [HorizontalGroup("基础配置/场景信息/信息设置")]
    [VerticalGroup("基础配置/场景信息/信息设置/基本设置")]
    [LabelText("场景名称")]
    [Required("必须指定场景名称")]
    public string sceneName;

    [VerticalGroup("基础配置/场景信息/信息设置/基本设置")]
    [LabelText("场景描述")]
    [TextArea(2, 4)]
    public string sceneDescription;
    [VerticalGroup("基础配置/场景信息/信息设置/基本设置")]
    [LabelText("场景类型")]
    public ScenesType ScenesType = ScenesType.Battle;
    #endregion
    #region 玩家生成配置
    [FoldoutGroup("基础配置/玩家生成", expanded: true)]
    [LabelText("玩家生成点位置")]
    [InfoBox("玩家生成点位置，角色配置已迁移到MapSystemConfig")]
    public Vector3 playerSpawnPosition;
    #endregion


    #region 扩展配置（未来功能）
    [TitleGroup("扩展配置")]
    [FoldoutGroup("扩展配置/任务系统", expanded: false)]
    [HorizontalGroup("扩展配置/任务系统/任务设置")]
    [VerticalGroup("扩展配置/任务系统/任务设置/任务配置")]
    [LabelText("场景任务配置")]
    [InfoBox("此场景中可触发的任务列表")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "questName")]
    public List<QuestConfig> questConfigs = new List<QuestConfig>();

    [FoldoutGroup("扩展配置/对话系统", expanded: false)]
    [HorizontalGroup("扩展配置/对话系统/对话设置")]
    [VerticalGroup("扩展配置/对话系统/对话设置/对话配置")]
    [LabelText("NPC对话配置")]
    [InfoBox("此场景中NPC的对话配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "npcName")]
    public List<DialogueConfig> dialogueConfigs = new List<DialogueConfig>();

    [FoldoutGroup("扩展配置/物品系统", expanded: false)]
    [HorizontalGroup("扩展配置/物品系统/物品设置")]
    [VerticalGroup("扩展配置/物品系统/物品设置/物品配置")]
    [LabelText("场景物品配置")]
    [InfoBox("此场景中可拾取的物品配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "itemName")]
    public List<ItemSpawnConfig> itemConfigs = new List<ItemSpawnConfig>();
    #endregion

    #region 调试配置
    [TitleGroup("调试配置")]
    [FoldoutGroup("调试配置/调试选项", expanded: false)]
    [HorizontalGroup("调试配置/调试选项/调试设置")]
    [VerticalGroup("调试配置/调试选项/调试设置/调试配置")]
    [LabelText("启用调试模式")]
    public bool enableDebugMode = false;

    [VerticalGroup("调试配置/调试选项/调试设置/调试配置")]
    [LabelText("显示调试UI")]
    [ShowIf("enableDebugMode")]
    public bool showDebugUI = false;

    [VerticalGroup("调试配置/调试选项/调试设置/调试配置")]
    [LabelText("调试日志级别")]
    [ShowIf("enableDebugMode")]
    public DebugLogLevel debugLogLevel = DebugLogLevel.Info;
    #endregion



    #region 辅助方法
    /// <summary>
    /// 获取玩家生成位置
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        // 查找带有玩家生成点标签的GameObject
        return playerSpawnPosition;
    }

#endregion

    /// <summary>
    /// 验证配置完整性
    /// </summary>
    [Button("验证配置", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.8f)]
    public void ValidateConfiguration()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrEmpty(sceneName))
            errors.Add("场景名称不能为空");

        // 角色配置验证已迁移到MapSystemConfig
        
        if (errors.Count > 0)
        {
            Debug.LogError($"[UnifiedSceneConfig] 配置验证失败:\n{string.Join("\n", errors)}");
        }
        else
        {
            Debug.Log($"[UnifiedSceneConfig] 配置验证通过！");
        }
    }
}

#region 配置数据结构

/// <summary>
/// 场景类型枚举
/// </summary>
public enum ScenesType
{
    Battle,     // 战斗场景
    Town,       // 城镇场景
    Dungeon,    // 地牢场景
    Boss,       // Boss场景
    Tutorial,   // 教程场景
    Cutscene    // 过场动画场景
}

/// <summary>
/// 调试日志级别
/// </summary>
public enum DebugLogLevel
{
    None,
    Error,
    Warning,
    Info,
    Verbose
}

/// <summary>
/// HUD显示配置
/// </summary>
[System.Serializable]
public class HUDDisplayConfig
{
    [LabelText("显示生命值")]
    public bool showHealth = true;

    [LabelText("显示魔法值")]
    public bool showMana = true;

    [LabelText("显示经验值")]
    public bool showExperience = true;

    [LabelText("显示小地图")]
    public bool showMinimap = false;

    [LabelText("显示技能冷却")]
    public bool showSkillCooldown = true;
}



/// <summary>
/// 任务配置（扩展功能）
/// </summary>
[System.Serializable]
public class QuestConfig
{
    [LabelText("任务名称")]
    public string questName;

    [LabelText("任务ID")]
    public string questId;

    [LabelText("是否自动触发")]
    public bool autoTrigger = false;

    [LabelText("触发条件")]
    [TextArea(2, 3)]
    public string triggerCondition;
}


/// <summary>
/// 物品生成配置（扩展功能）
/// </summary>
[System.Serializable]
public class ItemSpawnConfig
{
    [LabelText("物品名称")]
    public string itemName;

    [LabelText("物品ID")]
    public string itemId;

    [LabelText("生成位置")]
    public Vector3 spawnPosition;

    [LabelText("生成概率")]
    [Range(0f, 1f)]
    public float spawnChance = 1f;
}

#endregion