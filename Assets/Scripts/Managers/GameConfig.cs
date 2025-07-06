using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 游戏配置管理器 - 管理游戏的各种配置数据
/// 从原Phaser项目的配置系统迁移而来
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("游戏基础设置")]
    public string gameVersion = "1.0.0";
    public string gameName = "RPG Game";
    public bool debugMode = false;
    
    [Header("角色配置")]
    public CharacterConfig characterConfig;
    
    [Header("战斗配置")]
    public CombatConfig combatConfig;
    
    [Header("经验配置")]
    public ExperienceConfig experienceConfig;
    
    [Header("物品配置")]
    public ItemConfig itemConfig;
    
    [Header("音频配置")]
    public AudioConfig audioConfig;
    
    [Header("UI配置")]
    public UIConfig uiConfig;
    
    [Header("场景配置")]
    public SceneConfig sceneConfig;
    
    // 单例实例
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                {
                    Debug.LogError("GameConfig not found in Resources folder!");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 获取角色基础属性
    /// </summary>
    public CharacterBaseStats GetCharacterBaseStats(CharacterType type)
    {
        return characterConfig.GetBaseStats(type);
    }
    
    /// <summary>
    /// 获取技能配置
    /// </summary>
    public SkillData GetSkillData(string skillId)
    {
        return combatConfig.GetSkillData(skillId);
    }
    
    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetRequiredExperience(int level)
    {
        return experienceConfig.GetRequiredExperience(level);
    }
    
    /// <summary>
    /// 获取物品数据
    /// </summary>
    public ItemData GetItemData(string itemId)
    {
        return itemConfig.GetItemData(itemId);
    }
}

/// <summary>
/// 角色类型枚举
/// </summary>
public enum CharacterType
{
    Warrior,
    Mage,
    Archer
}

// CharacterConfig moved to Assets/Scripts/Config/CharacterConfig.cs

/// <summary>
/// 角色基础属性
/// </summary>
[Serializable]
public class CharacterBaseStats
{
    [Header("基础属性")]
    public int baseHealth = 100;
    public int baseMana = 50;
    public int baseStrength = 10;
    public int baseAgility = 10;
    public int baseVitality = 10;
    public int baseIntelligence = 10;
    
    [Header("成长属性")]
    public float healthGrowth = 10f;
    public float manaGrowth = 5f;
    public float strengthGrowth = 2f;
    public float agilityGrowth = 2f;
    public float vitalityGrowth = 2f;
    public float intelligenceGrowth = 2f;
}

/// <summary>
/// 战斗配置
/// </summary>
[Serializable]
public class CombatConfig
{
    [Header("基础战斗设置")]
    public float globalDamageMultiplier = 1f;
    public float criticalChanceBase = 0.05f;
    public float criticalDamageMultiplier = 2f;
    public float dodgeChanceBase = 0.05f;
    
    [Header("技能数据")]
    public List<SkillData> skillDataList = new List<SkillData>();
    
    [Header("状态效果持续时间")]
    public float poisonDuration = 5f;
    public float burnDuration = 3f;
    public float freezeDuration = 2f;
    public float stunDuration = 1f;
    
    public SkillData GetSkillData(string skillId)
    {
        return skillDataList.Find(skill => skill.skillId == skillId);
    }
}

// SkillData and SkillType moved to Assets/Scripts/Systems/SkillSystem.cs

/// <summary>
/// 状态效果
/// </summary>
[Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float value;
    public float duration;
}

/// <summary>
/// 状态效果类型
/// </summary>
public enum StatusEffectType
{
    Poison,
    Burn,
    Freeze,
    Stun,
    Heal,
    SpeedBoost,
    DamageBoost,
    DefenseBoost
}

/// <summary>
/// 经验配置
/// </summary>
[Serializable]
public class ExperienceConfig
{
    [Header("经验设置")]
    public int baseExperience = 100;
    public float experienceGrowthRate = 1.2f;
    public int maxLevel = 50;
    
    [Header("经验来源")]
    public int enemyKillBaseExp = 10;
    public int questCompleteBaseExp = 50;
    public int itemUseBaseExp = 5;
    
    public int GetRequiredExperience(int level)
    {
        if (level <= 1) return 0;
        return Mathf.RoundToInt(baseExperience * Mathf.Pow(experienceGrowthRate, level - 1));
    }
}

// ItemConfig moved to separate ItemConfig.cs file

/// <summary>
/// 物品数据
/// </summary>
[Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity;
    public int maxStackSize = 1;
    public int sellPrice;
    public int buyPrice;
    public bool canDrop = true;
    public bool canTrade = true;
    public bool canUse = false;
    public float cooldown = 0f;
    public bool consumeOnUse = true;
    public List<ItemEffect> effects = new List<ItemEffect>();
}

// ItemEffect moved to separate ItemConfig.cs file

/// <summary>
/// 物品效果类型
/// </summary>
public enum ItemEffectType
{
    InstantHeal,
    InstantMana,
    HealthOverTime,
    ManaOverTime,
    StrengthBoost,
    AgilityBoost,
    VitalityBoost,
    IntelligenceBoost,
    SpeedBoost,
    DamageBoost,
    DefenseBoost,
    Poison,
    Invisibility,
    Invincibility
}

// AudioConfig moved to Assets/Scripts/Systems/AudioManager.cs

/// <summary>
/// UI配置
/// </summary>
[Serializable]
public class UIConfig
{
    [Header("UI动画设置")]
    public float panelFadeTime = 0.3f;
    public float buttonScaleTime = 0.1f;
    public float messageDisplayTime = 3f;
    
    [Header("血条设置")]
    public float healthBarUpdateSpeed = 2f;
    public float manaBarUpdateSpeed = 2f;
    public Color lowHealthColor = Color.red;
    public Color normalHealthColor = Color.green;
    public Color lowManaColor = Color.blue;
    public Color normalManaColor = Color.cyan;
    
    [Header("伤害数字设置")]
    public float damageNumberDuration = 1f;
    public float damageNumberMoveSpeed = 2f;
    public Color playerDamageColor = Color.red;
    public Color enemyDamageColor = Color.yellow;
    public Color healColor = Color.green;
    public Color criticalColor = new Color(1f, 0.5f, 0f, 1f); // 橙色
}

/// <summary>
/// 场景配置
/// </summary>
[Serializable]
public class SceneConfig
{
    [Header("场景信息")]
    public List<SceneInfo> sceneInfoList = new List<SceneInfo>();
    
    [Header("加载设置")]
    public float minLoadingTime = 1f;
    public float maxLoadingTime = 5f;
    public bool showLoadingTips = true;
    public List<string> loadingTips = new List<string>();
    
    public SceneInfo GetSceneInfo(string sceneName)
    {
        return sceneInfoList.Find(scene => scene.sceneName == sceneName);
    }
    
    public string GetRandomLoadingTip()
    {
        if (loadingTips.Count == 0) return "Loading...";
        return loadingTips[UnityEngine.Random.Range(0, loadingTips.Count)];
    }
}

// SceneInfo and SceneType moved to Assets/Scripts/Systems/SceneController.cs