using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡配置文件
/// 定义游戏中的所有关卡和地点信息
/// 从原Phaser项目的LevelData.js迁移而来
/// </summary>
[System.Serializable]
public class LevelRewards
{
    [Header("奖励配置")]
    public int experience = 0;
    public int gold = 0;
    public List<string> items;
}

[System.Serializable]
public class LevelConfig
{
    [Header("关卡基础信息")]
    public int id;
    public string key;
    public string name;
    [TextArea(3, 5)]
    public string description;
    
    [Header("关卡设置")]
    public string difficulty = "easy"; // easy, medium, hard, extreme
    public int requiredLevel = 1;
    public bool safeZone = false;
    
    [Header("场景配置")]
    public string sceneName; // Unity场景名称
    public string locationId;
    
    [Header("敌人配置")]
    public List<string> enemies;
    public List<EnemySpawnConfig> enemySpawns;
    
    [Header("音频配置")]
    public string backgroundMusic;
    public List<string> ambientSounds;
    
    [Header("奖励配置")]
    public LevelRewards rewards;
    
    [Header("前置条件")]
    public List<string> prerequisites; // 前置关卡或任务
    public List<string> requiredItems; // 需要的物品
    
    [Header("特殊配置")]
    public bool isBossLevel = false;
    public string bossId;
    public List<string> specialMechanics; // 特殊机制
}

[System.Serializable]
public class EnemySpawnConfig
{
    [Header("生成配置")]
    public string enemyId;
    public string enemyType;
    public GameObject enemyPrefab;
    public Vector3 spawnPosition;
    public int quantity = 1;
    public float spawnDelay = 0f;
    
    [Header("敌人配置")]
    public EnemyConfig enemyConfig;
    
    [Header("生成条件")]
    public string triggerCondition; // player_enter, time_based, event_based等
    public float triggerValue = 0f;
    
    [Header("生成参数")]
    public bool respawn = false;
    public float respawnTime = 30f;
    public int maxRespawns = -1; // -1表示无限制
    
    [Header("巡逻配置")]
    public bool autoGeneratePatrolPoints = true;
    public float patrolRadius = 3f;
    public List<Vector3> patrolPoints = new List<Vector3>();
}

[System.Serializable]
public class WorldRegion
{
    [Header("区域信息")]
    public string regionId;
    public string regionName;
    [TextArea(2, 4)]
    public string regionDescription;
    
    [Header("区域配置")]
    public List<int> levelIds; // 该区域包含的关卡ID
    public string regionTheme; // 区域主题：forest, desert, mountain等
    public Color regionColor = Color.white;
    
    [Header("解锁条件")]
    public int requiredPlayerLevel = 1;
    public List<string> requiredQuests;
    public List<string> requiredItems;
}

[System.Serializable]
public class DifficultyConfig
{
    [Header("难度信息")]
    public string difficultyId;
    public string displayName;
    public Color difficultyColor = Color.white;
    
    [Header("难度修正")]
    public float enemyHealthMultiplier = 1f;
    public float enemyDamageMultiplier = 1f;
    public float experienceMultiplier = 1f;
    public float goldMultiplier = 1f;
    
    [Header("特殊效果")]
    public List<string> specialEffects;
}

[System.Serializable]
public class LevelProgression
{
    [Header("进度信息")]
    public int levelId;
    public bool completed = false;
    public bool unlocked = false;
    public float bestTime = 0f;
    public int timesCompleted = 0;
    
    [Header("评分系统")]
    public int starRating = 0; // 0-3星评级
    public List<string> achievements; // 该关卡获得的成就
}

/// <summary>
/// 关卡配置管理器
/// 提供统一的关卡配置访问接口
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game Config/Level Config")]
public class LevelConfigSO : ScriptableObject
{
    [Header("关卡配置列表")]
    public List<LevelConfig> levelConfigs;
    
    [Header("世界区域配置")]
    public List<WorldRegion> worldRegions;
    
    [Header("难度配置")]
    public List<DifficultyConfig> difficultyConfigs;
    
    [Header("全局设置")]
    public int maxPlayerLevel = 100;
    public float defaultRespawnTime = 30f;
    
    /// <summary>
    /// 获取关卡配置
    /// </summary>
    /// <param name="levelId">关卡ID</param>
    /// <returns>关卡配置</returns>
    public LevelConfig GetLevelConfig(int levelId)
    {
        return levelConfigs?.Find(config => config.id == levelId);
    }
    
    /// <summary>
    /// 根据关卡键名获取配置
    /// </summary>
    /// <param name="levelKey">关卡键名</param>
    /// <returns>关卡配置</returns>
    public LevelConfig GetLevelConfigByKey(string levelKey)
    {
        return levelConfigs?.Find(config => 
            config.key.Equals(levelKey, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取指定难度的关卡列表
    /// </summary>
    /// <param name="difficulty">难度</param>
    /// <returns>关卡配置列表</returns>
    public List<LevelConfig> GetLevelsByDifficulty(string difficulty)
    {
        return levelConfigs?.FindAll(config => 
            config.difficulty.Equals(difficulty, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取玩家等级可访问的关卡列表
    /// </summary>
    /// <param name="playerLevel">玩家等级</param>
    /// <returns>关卡配置列表</returns>
    public List<LevelConfig> GetAvailableLevels(int playerLevel)
    {
        return levelConfigs?.FindAll(config => config.requiredLevel <= playerLevel);
    }
    
    /// <summary>
    /// 获取安全区域关卡列表
    /// </summary>
    /// <returns>安全区域关卡列表</returns>
    public List<LevelConfig> GetSafeZoneLevels()
    {
        return levelConfigs?.FindAll(config => config.safeZone);
    }
    
    /// <summary>
    /// 获取Boss关卡列表
    /// </summary>
    /// <returns>Boss关卡列表</returns>
    public List<LevelConfig> GetBossLevels()
    {
        return levelConfigs?.FindAll(config => config.isBossLevel);
    }
    
    /// <summary>
    /// 获取世界区域配置
    /// </summary>
    /// <param name="regionId">区域ID</param>
    /// <returns>区域配置</returns>
    public WorldRegion GetWorldRegion(string regionId)
    {
        return worldRegions?.Find(region => 
            region.regionId.Equals(regionId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取区域内的关卡列表
    /// </summary>
    /// <param name="regionId">区域ID</param>
    /// <returns>关卡配置列表</returns>
    public List<LevelConfig> GetLevelsInRegion(string regionId)
    {
        var region = GetWorldRegion(regionId);
        if (region?.levelIds != null)
        {
            var levels = new List<LevelConfig>();
            foreach (var levelId in region.levelIds)
            {
                var level = GetLevelConfig(levelId);
                if (level != null)
                {
                    levels.Add(level);
                }
            }
            return levels;
        }
        return new List<LevelConfig>();
    }
    
    /// <summary>
    /// 获取难度配置
    /// </summary>
    /// <param name="difficultyId">难度ID</param>
    /// <returns>难度配置</returns>
    public DifficultyConfig GetDifficultyConfig(string difficultyId)
    {
        return difficultyConfigs?.Find(config => 
            config.difficultyId.Equals(difficultyId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 检查关卡是否解锁
    /// </summary>
    /// <param name="levelId">关卡ID</param>
    /// <param name="playerLevel">玩家等级</param>
    /// <param name="completedQuests">已完成任务列表</param>
    /// <param name="playerItems">玩家物品列表</param>
    /// <returns>是否解锁</returns>
    public bool IsLevelUnlocked(int levelId, int playerLevel, List<string> completedQuests = null, List<string> playerItems = null)
    {
        var level = GetLevelConfig(levelId);
        if (level == null) return false;
        
        // 检查等级要求
        if (playerLevel < level.requiredLevel) return false;
        
        // 检查前置任务
        if (level.prerequisites != null && completedQuests != null)
        {
            foreach (var prerequisite in level.prerequisites)
            {
                if (!completedQuests.Contains(prerequisite))
                {
                    return false;
                }
            }
        }
        
        // 检查需要的物品
        if (level.requiredItems != null && playerItems != null)
        {
            foreach (var requiredItem in level.requiredItems)
            {
                if (!playerItems.Contains(requiredItem))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取下一个关卡
    /// </summary>
    /// <param name="currentLevelId">当前关卡ID</param>
    /// <returns>下一个关卡配置</returns>
    public LevelConfig GetNextLevel(int currentLevelId)
    {
        return levelConfigs?.Find(config => config.id == currentLevelId + 1);
    }
    
    /// <summary>
    /// 获取上一个关卡
    /// </summary>
    /// <param name="currentLevelId">当前关卡ID</param>
    /// <returns>上一个关卡配置</returns>
    public LevelConfig GetPreviousLevel(int currentLevelId)
    {
        return levelConfigs?.Find(config => config.id == currentLevelId - 1);
    }
    
    /// <summary>
    /// 验证关卡ID是否存在
    /// </summary>
    /// <param name="levelId">关卡ID</param>
    /// <returns>是否存在</returns>
    public bool IsValidLevelId(int levelId)
    {
        return GetLevelConfig(levelId) != null;
    }
    
    /// <summary>
    /// 获取所有关卡ID
    /// </summary>
    /// <returns>关卡ID列表</returns>
    public List<int> GetAllLevelIds()
    {
        var ids = new List<int>();
        if (levelConfigs != null)
        {
            foreach (var config in levelConfigs)
            {
                ids.Add(config.id);
            }
        }
        return ids;
    }
}

/// <summary>
/// 关卡配置常量
/// 定义常用的关卡配置值
/// </summary>
public static class LevelConfigConstants
{
    // 难度常量
    public const string DIFFICULTY_EASY = "easy";
    public const string DIFFICULTY_MEDIUM = "medium";
    public const string DIFFICULTY_HARD = "hard";
    public const string DIFFICULTY_EXTREME = "extreme";
    
    // 触发条件常量
    public const string TRIGGER_PLAYER_ENTER = "player_enter";
    public const string TRIGGER_TIME_BASED = "time_based";
    public const string TRIGGER_EVENT_BASED = "event_based";
    
    // 区域主题常量
    public const string THEME_FOREST = "forest";
    public const string THEME_DESERT = "desert";
    public const string THEME_MOUNTAIN = "mountain";
    public const string THEME_DUNGEON = "dungeon";
    public const string THEME_VILLAGE = "village";
    
    // 特殊机制常量
    public const string MECHANIC_TIME_LIMIT = "time_limit";
    public const string MECHANIC_SURVIVAL = "survival";
    public const string MECHANIC_ESCORT = "escort";
    public const string MECHANIC_PUZZLE = "puzzle";
}