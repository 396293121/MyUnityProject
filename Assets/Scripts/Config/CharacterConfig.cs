using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色配置文件
/// 定义所有角色的基础属性和职业特性
/// 从原Phaser项目的CharacterConfig.js迁移而来
/// 避免在类构造方法中硬编码参数
/// </summary>
[System.Serializable]
public class CharacterAttributes
{
    [Header("基础属性")]
    [Range(1, 20)]
    public int strength = 5;      // 力量
    [Range(1, 20)]
    public int agility = 5;       // 敏捷
    [Range(1, 20)]
    public int vitality = 5;      // 体力
    [Range(1, 20)]
    public int intelligence = 5;  // 智力
}

[System.Serializable]
public class SkillConfig
{
    [Header("技能基础信息")]
    public string skillKey;
    public string name;
    [TextArea(2, 4)]
    public string description;
    
    [Header("技能参数")]
    public float damage = 1.0f;           // 伤害倍数
    public float cooldown = 5000f;        // 冷却时间（毫秒）
    public float range = 80f;             // 技能范围
    public string attackType = "single";  // 攻击类型：single, aoe, buff
    
    [Header("特殊效果")]
    public float buffMultiplier = 1.0f;   // Buff倍数
    public float duration = 0f;           // 持续时间（毫秒）
    public AttackAreaConfig attackArea;   // 攻击区域配置
    public StunConfig stunConfig;         // 眩晕配置
}

[System.Serializable]
public class AttackAreaConfig
{
    [Header("攻击区域")]
    public float width = 100f;
    public float height = 100f;
    public float offsetX = 0f;
    public float offsetY = 0f;
}

[System.Serializable]
public class StunConfig
{
    [Header("眩晕配置")]
    public float stunRange = 200f;
    public float stunDuration = 2000f;    // 眩晕持续时间（毫秒）
}

[System.Serializable]
public class CharacterConfig
{
    [Header("角色基础信息")]
    public string characterType;
    public string displayName;
    
    [Header("基础属性配置")]
    public CharacterAttributes attributes;
    
    [Header("职业特性")]
    public string weaponType = "sword";
    public string attackRange = "melee"; // melee, ranged
    
    [Header("职业技能")]
    public List<SkillConfig> classSkills;
    
    /// <summary>
    /// 获取技能配置
    /// </summary>
    /// <param name="skillKey">技能键名</param>
    /// <returns>技能配置</returns>
    public SkillConfig GetSkillConfig(string skillKey)
    {
        return classSkills?.Find(skill => skill.skillKey == skillKey);
    }
    
    /// <summary>
    /// 获取角色基础属性
    /// </summary>
    /// <param name="type">角色类型</param>
    /// <returns>基础属性</returns>
    public CharacterBaseStats GetBaseStats(CharacterType type)
    {
        // 根据角色类型返回对应的基础属性
        var baseStats = new CharacterBaseStats();
        
        if (attributes != null)
        {
            baseStats.baseStrength = attributes.strength;
            baseStats.baseAgility = attributes.agility;
            baseStats.baseVitality = attributes.vitality;
            baseStats.baseIntelligence = attributes.intelligence;
        }
        
        return baseStats;
    }
}

/// <summary>
/// 角色配置管理器
/// 提供统一的角色配置访问接口
/// </summary>
[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Game Config/Character Config")]
public class CharacterConfigSO : ScriptableObject
{
    [Header("角色配置列表")]
    public List<CharacterConfig> characterConfigs;
    
    /// <summary>
    /// 获取角色配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>角色配置</returns>
    public CharacterConfig GetCharacterConfig(string characterType)
    {
        return characterConfigs?.Find(config => 
            config.characterType.Equals(characterType, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取角色属性配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>属性配置</returns>
    public CharacterAttributes GetCharacterAttributes(string characterType)
    {
        var config = GetCharacterConfig(characterType);
        return config?.attributes;
    }
    
    /// <summary>
    /// 获取角色技能配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>技能配置列表</returns>
    public List<SkillConfig> GetCharacterSkills(string characterType)
    {
        var config = GetCharacterConfig(characterType);
        return config?.classSkills;
    }
    
    /// <summary>
    /// 获取特定技能配置
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <param name="skillKey">技能键名</param>
    /// <returns>技能配置</returns>
    public SkillConfig GetSkillConfig(string characterType, string skillKey)
    {
        var config = GetCharacterConfig(characterType);
        return config?.GetSkillConfig(skillKey);
    }
    
    /// <summary>
    /// 获取所有可用角色类型
    /// </summary>
    /// <returns>角色类型列表</returns>
    public List<string> GetAvailableCharacterTypes()
    {
        var types = new List<string>();
        if (characterConfigs != null)
        {
            foreach (var config in characterConfigs)
            {
                if (!string.IsNullOrEmpty(config.characterType))
                {
                    types.Add(config.characterType);
                }
            }
        }
        return types;
    }
    
    /// <summary>
    /// 验证角色类型是否存在
    /// </summary>
    /// <param name="characterType">角色类型</param>
    /// <returns>是否存在</returns>
    public bool IsValidCharacterType(string characterType)
    {
        return GetCharacterConfig(characterType) != null;
    }
}

/// <summary>
/// 角色配置常量
/// 定义默认的角色配置值
/// </summary>
public static class CharacterConfigConstants
{
    // 武器类型常量
    public const string WEAPON_SWORD = "sword";
    public const string WEAPON_STAFF = "staff";
    public const string WEAPON_BOW = "bow";
    
    // 攻击范围常量
    public const string ATTACK_MELEE = "melee";
    public const string ATTACK_RANGED = "ranged";
    
    // 攻击类型常量
    public const string ATTACK_TYPE_SINGLE = "single";
    public const string ATTACK_TYPE_AOE = "aoe";
    public const string ATTACK_TYPE_BUFF = "buff";
    
    // 技能键名常量
    public const string SKILL_HEAVY_SLASH = "HEAVY_SLASH";
    public const string SKILL_WHIRLWIND = "WHIRLWIND";
    public const string SKILL_BATTLE_CRY = "BATTLE_CRY";
    
    // 角色类型常量
    public const string CHARACTER_WARRIOR = "warrior";
    public const string CHARACTER_MAGE = "mage";
    public const string CHARACTER_ARCHER = "archer";
}