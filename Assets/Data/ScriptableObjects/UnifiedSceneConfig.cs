using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

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
    
    [VerticalGroup("基础配置/场景信息/信息设置/视觉设置")]
    [LabelText("背景颜色")]
    public Color backgroundColor = Color.black;
    
    [VerticalGroup("基础配置/场景信息/信息设置/视觉设置")]
    [LabelText("地图预制体")]
    [AssetsOnly]
    public GameObject mapPrefab;
    #endregion
    
    #region 角色配置
    [FoldoutGroup("基础配置/角色系统", expanded: true)]
    [HorizontalGroup("基础配置/角色系统/角色设置")]
    [VerticalGroup("基础配置/角色系统/角色设置/角色预制体")]
    [LabelText("支持的角色类型")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "characterType")]
    public List<CharacterPrefabConfig> characterPrefabs = new List<CharacterPrefabConfig>();
    
    [VerticalGroup("基础配置/角色系统/角色设置/生成设置")]
    [LabelText("默认角色类型")]
    [ValueDropdown("GetCharacterTypes")]
    public string defaultCharacterType = "warrior";
    
    [VerticalGroup("基础配置/角色系统/角色设置/生成设置")]
    [LabelText("玩家生成点标签")]
    [InfoBox("场景中带有此标签的GameObject将作为玩家生成点")]
    public GameObject playerSpawnTag;
    #endregion
    
    #region 敌人配置
    [FoldoutGroup("基础配置/敌人系统", expanded: true)]
    [HorizontalGroup("基础配置/敌人系统/敌人设置")]
    [VerticalGroup("基础配置/敌人系统/敌人设置/敌人预制体")]
    [LabelText("敌人预制体配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "enemyType")]
    public List<EnemyPrefabConfig> enemyPrefabs = new List<EnemyPrefabConfig>();
    
    [VerticalGroup("基础配置/敌人系统/敌人设置/生成设置")]
    [LabelText("敌人生成点标签")]
    [InfoBox("场景中带有此标签的GameObject将作为敌人生成点")]
    public string enemySpawnTag = "EnemySpawn";
    
    [VerticalGroup("基础配置/敌人系统/敌人设置/生成设置")]
    [LabelText("敌人生成配置")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<EnemySpawnConfig> enemySpawns = new List<EnemySpawnConfig>();
    
    [VerticalGroup("基础配置/敌人系统/敌人设置/波次设置")]
    [LabelText("敌人波次配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "waveName")]
    public List<EnemyWaveConfig> enemyWaves = new List<EnemyWaveConfig>();
    
    [VerticalGroup("基础配置/敌人系统/敌人设置/系统配置")]
    [LabelText("敌人系统配置")]
    [InfoBox("敌人系统的全局配置，包括更新频率、AI设置等")]
    public EnemySystemConfig enemySystemConfig;
    #endregion
    
    #region UI配置
    [TitleGroup("界面配置")]
    [FoldoutGroup("界面配置/UI系统", expanded: true)]
    [HorizontalGroup("界面配置/UI系统/UI设置")]
    [VerticalGroup("界面配置/UI系统/UI设置/界面预制体")]
    [LabelText("HUD界面预制体")]
    [AssetsOnly]
    public GameObject hudUIPrefab;
    
    [VerticalGroup("界面配置/UI系统/UI设置/界面预制体")]
    [LabelText("暂停菜单预制体")]
    [AssetsOnly]
    public GameObject pauseMenuPrefab;
    
    [VerticalGroup("界面配置/UI系统/UI设置/界面预制体")]
    [LabelText("游戏结束界面预制体")]
    [AssetsOnly]
    public GameObject gameOverUIPrefab;
    
    [VerticalGroup("界面配置/UI系统/UI设置/HUD设置")]
    [LabelText("HUD配置")]
    public HUDDisplayConfig hudConfig;
    #endregion
    
    #region 输入配置
    [FoldoutGroup("界面配置/输入系统", expanded: true)]
    [HorizontalGroup("界面配置/输入系统/输入设置")]
    [VerticalGroup("界面配置/输入系统/输入设置/输入配置")]
    [LabelText("输入配置")]
    public InputControlConfig inputConfig;
    #endregion
    
    #region 音频配置
    [TitleGroup("媒体配置")]
    [FoldoutGroup("媒体配置/音频系统", expanded: true)]
    [HorizontalGroup("媒体配置/音频系统/音频设置")]
    [VerticalGroup("媒体配置/音频系统/音频设置/背景音乐")]
    [LabelText("背景音乐")]
    [AssetsOnly]
    public AudioClip backgroundMusic;
    
    [VerticalGroup("媒体配置/音频系统/音频设置/环境音效")]
    [LabelText("环境音效")]
    [AssetsOnly]
    public AudioClip ambientSound;
    
    [VerticalGroup("媒体配置/音频系统/音频设置/音频配置")]
    [LabelText("音频配置")]
    public AudioSystemConfig audioConfig;
    
    [VerticalGroup("媒体配置/音频系统/音频设置/音效列表")]
    [LabelText("音效配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "soundName")]
    public List<SoundEffectConfig> soundEffects = new List<SoundEffectConfig>();
    #endregion
    
    #region 摄像机配置
    [FoldoutGroup("媒体配置/摄像机系统", expanded: true)]
    [HorizontalGroup("媒体配置/摄像机系统/摄像机设置")]
    [VerticalGroup("媒体配置/摄像机系统/摄像机设置/跟随设置")]
    [LabelText("摄像机跟随配置")]
    public CameraFollowConfig cameraConfig;
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
    /// 获取角色类型列表（用于下拉菜单）
    /// </summary>
    private IEnumerable<string> GetCharacterTypes()
    {
        foreach (var character in characterPrefabs)
        {
            if (!string.IsNullOrEmpty(character.characterType))
                yield return character.characterType;
        }
    }
    
    /// <summary>
    /// 根据角色类型获取预制体
    /// </summary>
    public GameObject GetCharacterPrefab(string characterType)
    {
        var config = characterPrefabs.Find(c => c.characterType == characterType);
        return config?.prefab;
    }
    
    /// <summary>
    /// 根据敌人类型获取预制体
    /// </summary>
    public GameObject GetEnemyPrefab(string enemyType)
    {
        var config = enemyPrefabs.Find(e => e.enemyType == enemyType);
        return config?.prefab;
    }
    
    /// <summary>
    /// 根据音效名称获取音效配置
    /// </summary>
    public SoundEffectConfig GetSoundEffect(string soundName)
    {
        return soundEffects.Find(s => s.soundName == soundName);
    }
    
    /// <summary>
    /// 获取玩家生成位置
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        // 查找带有玩家生成点标签的GameObject
        return playerSpawnTag != null ? playerSpawnTag.transform.position : Vector3.zero;
    }
    
    /// <summary>
    /// 获取敌人生成点列表
    /// </summary>
    public Transform[] GetEnemySpawnPoints()
    {
        var spawnPoints = GameObject.FindGameObjectsWithTag(enemySpawnTag);
        Transform[] transforms = new Transform[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            transforms[i] = spawnPoints[i].transform;
        }
        return transforms;
    }
    
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
        
        if (characterPrefabs.Count == 0)
            errors.Add("至少需要配置一个角色预制体");
        
        foreach (var character in characterPrefabs)
        {
            if (character.prefab == null)
                errors.Add($"角色类型 '{character.characterType}' 的预制体未配置");
        }
        
        foreach (var enemy in enemyPrefabs)
        {
            if (enemy.prefab == null)
                errors.Add($"敌人类型 '{enemy.enemyType}' 的预制体未配置");
        }
        
        if (errors.Count > 0)
        {
            Debug.LogError($"[UnifiedSceneConfig] 配置验证失败:\n{string.Join("\n", errors)}");
        }
        else
        {
            Debug.Log($"[UnifiedSceneConfig] 配置验证通过！");
        }
    }
    #endregion
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
/// 角色预制体配置
/// </summary>
[System.Serializable]
public class CharacterPrefabConfig
{
    [LabelText("角色类型")]
    public string characterType;
    
    [LabelText("角色预制体")]
    [AssetsOnly]
    public GameObject prefab;
    
    [LabelText("角色名称")]
    public string characterName;
    
    [LabelText("角色描述")]
    [TextArea(2, 3)]
    public string description;
}

/// <summary>
/// 敌人预制体配置
/// </summary>
[System.Serializable]
public class EnemyPrefabConfig
{
    [LabelText("敌人类型")]
    public string enemyType;
    
    [LabelText("敌人预制体")]
    [AssetsOnly]
    public GameObject prefab;
    
    [LabelText("敌人名称")]
    public string enemyName;
    
    [LabelText("敌人描述")]
    [TextArea(2, 3)]
    public string description;
}

/// <summary>
/// 敌人生成配置
/// </summary>
[System.Serializable]
public class EnemySpawnConfig
{
    [LabelText("敌人类型")]
    public string enemyType;
    
    [LabelText("敌人预制体")]
    [AssetsOnly]
    public GameObject enemyPrefab;
    
    [LabelText("敌人系统配置引用")]
    [InfoBox("从EnemySystemConfig获取敌人配置", InfoMessageType.Info)]
    [ReadOnly]
    [ShowInInspector]
    private string configSource = "使用EnemySystemConfig.wildBoarConfig";
    
    [LabelText("生成位置")]
    public Vector3 spawnPosition;
    
    [LabelText("生成延迟")]
    [SuffixLabel("秒")]
    public float spawnDelay = 0f;
    
    [LabelText("生成数量")]
    [Range(1, 10)]
    public int spawnCount = 1;
    
    [LabelText("随机化位置")]
    public bool randomizePosition = false;
    
    [LabelText("生成半径")]
    [ShowIf("randomizePosition")]
    [SuffixLabel("米")]
    public float spawnRadius = 2f;
    
    [LabelText("是否自动生成")]
    public bool autoSpawn = true;
    
    [LabelText("自动生成巡逻点")]
    public bool autoGeneratePatrolPoints = true;
    
    [LabelText("巡逻半径")]
    [ShowIf("autoGeneratePatrolPoints")]
    [SuffixLabel("米")]
    public float patrolRadius = 5f;
    
    [LabelText("巡逻点列表")]
    [HideIf("autoGeneratePatrolPoints")]
    public List<Vector3> patrolPoints = new List<Vector3>();
}

/// <summary>
/// 敌人波次配置
/// </summary>
[System.Serializable]
public class EnemyWaveConfig
{
    [LabelText("波次名称")]
    public string waveName;
    
    [LabelText("波次延迟")]
    [SuffixLabel("秒")]
    public float waveDelay = 0f;
    
    [LabelText("敌人列表")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<EnemySpawnConfig> enemies = new List<EnemySpawnConfig>();
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
/// 输入控制配置
/// </summary>
[System.Serializable]
public class InputControlConfig
{
    [LabelText("输入缓冲时间")]
    [SuffixLabel("秒")]
    public float inputBufferTime = 0.1f;
    
    [LabelText("启用手柄支持")]
    public bool enableGamepadSupport = true;
    
    [LabelText("手柄死区")]
    [Range(0f, 1f)]
    public float gamepadDeadzone = 0.2f;
}

/// <summary>
/// 音频系统配置
/// </summary>
[System.Serializable]
public class AudioSystemConfig
{
    [LabelText("主音量")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [LabelText("音乐音量")]
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;
    
    [LabelText("音效音量")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [LabelText("语音音量")]
    [Range(0f, 1f)]
    public float voiceVolume = 1f;
}

/// <summary>
/// 音效配置
/// </summary>
[System.Serializable]
public class SoundEffectConfig
{
    [LabelText("音效名称")]
    public string soundName;
    
    [LabelText("音频文件")]
    [AssetsOnly]
    public AudioClip audioClip;
    
    [LabelText("音量")]
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [LabelText("是否循环")]
    public bool loop = false;
    
    [LabelText("音调")]
    [Range(-3f, 3f)]
    public float pitch = 1f;
    
    [LabelText("冷却时间")]
    [SuffixLabel("秒")]
    public float cooldownTime = 0.1f;
}

/// <summary>
/// 摄像机跟随配置
/// </summary>
[System.Serializable]
public class CameraFollowConfig
{
    [LabelText("跟随速度")]
    public float followSpeed = 5f;
    
    [LabelText("偏移量")]
    public Vector3 offset = new Vector3(0, 2, -10);
    
    [LabelText("边界限制")]
    public bool useBounds = false;
    
    [LabelText("边界范围")]
    [ShowIf("useBounds")]
    public Bounds cameraBounds;
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
/// 对话配置（扩展功能）
/// </summary>
[System.Serializable]
public class DialogueConfig
{
    [LabelText("NPC名称")]
    public string npcName;
    
    [LabelText("对话ID")]
    public string dialogueId;
    
    [LabelText("对话文本")]
    [TextArea(3, 5)]
    public string dialogueText;
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