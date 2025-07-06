using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 技能系统 - 管理游戏中的技能学习和升级
/// 从原Phaser项目的技能系统迁移而来
/// </summary>
public class SkillSystem : MonoBehaviour
{
    [Header("技能配置")]
    public List<SkillData> availableSkills = new List<SkillData>();
    
    [Header("技能设置")]
    public int maxSkillLevel = 10;               // 技能最大等级
    public bool allowSkillReset = true;          // 允许技能重置
    public int skillResetCost = 100;             // 技能重置费用
    public bool showSkillEffects = true;         // 显示技能特效
    
    // 单例
    public static SkillSystem Instance { get; private set; }
    
    // 技能状态
    private Dictionary<string, SkillData> skillDatabase = new Dictionary<string, SkillData>();
    private Dictionary<string, PlayerSkill> learnedSkills = new Dictionary<string, PlayerSkill>();
    private Dictionary<string, SkillTree> skillTrees = new Dictionary<string, SkillTree>();
    
    // 事件
    public event Action<PlayerSkill> OnSkillLearned;
    public event Action<PlayerSkill> OnSkillUpgraded;
    public event Action<PlayerSkill> OnSkillUsed;
    public event Action OnSkillPointsChanged;
    public event Action OnSkillTreeUpdated;
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSkillSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化技能系统
    /// </summary>
    private void InitializeSkillSystem()
    {
        // 构建技能数据库
        foreach (SkillData skillData in availableSkills)
        {
            if (!string.IsNullOrEmpty(skillData.skillId))
            {
                skillDatabase[skillData.skillId] = skillData;
            }
        }
        
        // 创建技能树
        CreateSkillTrees();
        
        // 添加默认技能
        if (availableSkills.Count == 0)
        {
            CreateDefaultSkills();
        }
    }
    
    /// <summary>
    /// 创建技能树
    /// </summary>
    private void CreateSkillTrees()
    {
        // 战士技能树
        SkillTree warriorTree = new SkillTree
        {
            characterClass = "Warrior",
            skillNodes = new List<SkillNode>()
        };
        
        // 法师技能树
        SkillTree mageTree = new SkillTree
        {
            characterClass = "Mage",
            skillNodes = new List<SkillNode>()
        };
        
        // 射手技能树
        SkillTree archerTree = new SkillTree
        {
            characterClass = "Archer",
            skillNodes = new List<SkillNode>()
        };
        
        skillTrees["Warrior"] = warriorTree;
        skillTrees["Mage"] = mageTree;
        skillTrees["Archer"] = archerTree;
    }
    
    /// <summary>
    /// 创建默认技能
    /// </summary>
    private void CreateDefaultSkills()
    {
        // 战士技能
        List<SkillData> warriorSkills = new List<SkillData>
        {
            new SkillData
            {
                skillId = "warrior_slash",
                skillName = "重击",
                description = "造成额外伤害的强力攻击",
                skillType = SkillType.Active,
                characterClass = "Warrior",
                maxLevel = maxSkillLevel,
                manaCost = 10,
                cooldown = 3f,
                castTime = 0.5f,
                range = 2f,
                effects = new List<SkillEffect>
                {
                    new SkillEffect { type = EffectType.Damage, baseValue = 50, scalingPerLevel = 10 }
                }
            },
            new SkillData
            {
                skillId = "warrior_defense",
                skillName = "防御姿态",
                description = "提高防御力，减少受到的伤害",
                skillType = SkillType.Passive,
                characterClass = "Warrior",
                maxLevel = maxSkillLevel,
                effects = new List<SkillEffect>
                {
                    new SkillEffect { type = EffectType.DefenseBonus, baseValue = 5, scalingPerLevel = 2 }
                }
            }
        };
        
        // 法师技能
        List<SkillData> mageSkills = new List<SkillData>
        {
            new SkillData
            {
                skillId = "mage_fireball",
                skillName = "火球术",
                description = "发射火球攻击敌人",
                skillType = SkillType.Active,
                characterClass = "Mage",
                maxLevel = maxSkillLevel,
                manaCost = 15,
                cooldown = 2f,
                castTime = 1f,
                range = 8f,
                effects = new List<SkillEffect>
                {
                    new SkillEffect { type = EffectType.MagicDamage, baseValue = 40, scalingPerLevel = 8 }
                }
            },
            new SkillData
            {
                skillId = "mage_mana_shield",
                skillName = "魔法护盾",
                description = "消耗魔法值来吸收伤害",
                skillType = SkillType.Toggle,
                characterClass = "Mage",
                maxLevel = maxSkillLevel,
                manaCost = 5,
                effects = new List<SkillEffect>
                {
                    new SkillEffect { type = EffectType.ManaShield, baseValue = 20, scalingPerLevel = 5 }
                }
            }
        };
        
        // 射手技能
        List<SkillData> archerSkills = new List<SkillData>
        {
            new SkillData
            {
                skillId = "archer_multishot",
                skillName = "多重射击",
                description = "同时射出多支箭矢",
                skillType = SkillType.Active,
                characterClass = "Archer",
                maxLevel = maxSkillLevel,
                manaCost = 12,
                cooldown = 4f,
                castTime = 0.8f,
                range = 10f,
                effects = new List<SkillEffect>
                {
                    new SkillEffect { type = EffectType.ProjectileCount, baseValue = 3, scalingPerLevel = 1 },
                    new SkillEffect { type = EffectType.Damage, baseValue = 30, scalingPerLevel = 6 }
                }
            },
            new SkillData
            {
                skillId = "archer_speed",
                skillName = "疾行",
                description = "提高移动速度和攻击速度",
                skillType = SkillType.Passive,
                characterClass = "Archer",
                maxLevel = maxSkillLevel,
                effects = new List<SkillEffect>
                {
                    new SkillEffect { type = EffectType.SpeedBonus, baseValue = 10, scalingPerLevel = 3 },
                    new SkillEffect { type = EffectType.AttackSpeedBonus, baseValue = 5, scalingPerLevel = 2 }
                }
            }
        };
        
        availableSkills.AddRange(warriorSkills);
        availableSkills.AddRange(mageSkills);
        availableSkills.AddRange(archerSkills);
        
        // 更新数据库
        foreach (SkillData skillData in availableSkills)
        {
            skillDatabase[skillData.skillId] = skillData;
        }
        
        // 更新技能树
        UpdateSkillTrees();
    }
    
    /// <summary>
    /// 更新技能树
    /// </summary>
    private void UpdateSkillTrees()
    {
        foreach (SkillData skillData in availableSkills)
        {
            if (skillTrees.ContainsKey(skillData.characterClass))
            {
                SkillTree tree = skillTrees[skillData.characterClass];
                SkillNode node = new SkillNode
                {
                    skillId = skillData.skillId,
                    position = Vector2.zero, // 需要根据实际布局设置
                    prerequisites = skillData.prerequisites,
                    tier = CalculateSkillTier(skillData)
                };
                tree.skillNodes.Add(node);
            }
        }
    }
    
    /// <summary>
    /// 计算技能层级
    /// </summary>
    private int CalculateSkillTier(SkillData skillData)
    {
        if (skillData.prerequisites.Count == 0)
            return 1;
        
        int maxTier = 0;
        foreach (string prerequisite in skillData.prerequisites)
        {
            if (skillDatabase.ContainsKey(prerequisite))
            {
                int tier = CalculateSkillTier(skillDatabase[prerequisite]);
                maxTier = Mathf.Max(maxTier, tier);
            }
        }
        
        return maxTier + 1;
    }
    
    /// <summary>
    /// 学习技能
    /// </summary>
    public bool LearnSkill(string skillId)
    {
        if (!skillDatabase.ContainsKey(skillId))
        {
            Debug.LogWarning($"Skill '{skillId}' not found!");
            return false;
        }
        
        SkillData skillData = skillDatabase[skillId];
        
        // 检查是否可以学习
        if (!CanLearnSkill(skillData))
        {
            return false;
        }
        
        // 检查技能点
        if (!HasEnoughSkillPoints(skillData.requiredSkillPoints))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("技能点不足！");
            }
            return false;
        }
        
        // 学习技能
        PlayerSkill playerSkill = new PlayerSkill(skillData);
        learnedSkills[skillId] = playerSkill;
        
        // 消耗技能点
        ConsumeSkillPoints(skillData.requiredSkillPoints);
        
        // 触发事件
        OnSkillLearned?.Invoke(playerSkill);
        OnSkillPointsChanged?.Invoke();
        OnSkillTreeUpdated?.Invoke();
        
        // 显示消息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"学会技能: {skillData.skillName}");
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("skill_learn");
        }
        
        return true;
    }
    
    /// <summary>
    /// 升级技能
    /// </summary>
    public bool UpgradeSkill(string skillId)
    {
        if (!learnedSkills.ContainsKey(skillId))
        {
            Debug.LogWarning($"Skill '{skillId}' not learned!");
            return false;
        }
        
        PlayerSkill playerSkill = learnedSkills[skillId];
        
        // 检查是否可以升级
        if (!CanUpgradeSkill(playerSkill))
        {
            return false;
        }
        
        // 计算升级所需技能点
        int requiredPoints = CalculateUpgradeSkillPoints(playerSkill);
        
        // 检查技能点
        if (!HasEnoughSkillPoints(requiredPoints))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("技能点不足！");
            }
            return false;
        }
        
        // 升级技能
        playerSkill.currentLevel++;
        
        // 消耗技能点
        ConsumeSkillPoints(requiredPoints);
        
        // 触发事件
        OnSkillUpgraded?.Invoke(playerSkill);
        OnSkillPointsChanged?.Invoke();
        
        // 显示消息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"技能升级: {playerSkill.skillName} Lv.{playerSkill.currentLevel}");
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("skill_upgrade");
        }
        
        return true;
    }
    
    /// <summary>
    /// 使用技能
    /// </summary>
    public bool UseSkill(string skillId, Vector3 targetPosition = default, GameObject target = null)
    {
        if (!learnedSkills.ContainsKey(skillId))
        {
            return false;
        }
        
        PlayerSkill playerSkill = learnedSkills[skillId];
        
        // 检查是否可以使用
        if (!CanUseSkill(playerSkill))
        {
            return false;
        }
        
        // 检查魔法值
        if (!HasEnoughMana(playerSkill.GetManaCost()))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("魔法值不足！");
            }
            return false;
        }
        
        // 使用技能
        ExecuteSkill(playerSkill, targetPosition, target);
        
        // 消耗魔法值
        ConsumeMana(playerSkill.GetManaCost());
        
        // 设置冷却
        playerSkill.lastUsedTime = Time.time;
        
        // 触发事件
        OnSkillUsed?.Invoke(playerSkill);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX($"skill_{skillId}");
        }
        
        // 显示特效
        if (showSkillEffects)
        {
            ShowSkillEffect(playerSkill, targetPosition);
        }
        
        return true;
    }
    
    /// <summary>
    /// 执行技能效果
    /// </summary>
    private void ExecuteSkill(PlayerSkill skill, Vector3 targetPosition, GameObject target)
    {
        foreach (SkillEffect effect in skill.effects)
        {
            float effectValue = effect.GetValue(skill.currentLevel);
            
            switch (effect.type)
            {
                case EffectType.Damage:
                    ApplyDamage(target, effectValue);
                    break;
                case EffectType.MagicDamage:
                    ApplyMagicDamage(target, effectValue);
                    break;
                case EffectType.Heal:
                    ApplyHeal(target, effectValue);
                    break;
                case EffectType.DefenseBonus:
                    ApplyDefenseBonus(effectValue);
                    break;
                case EffectType.SpeedBonus:
                    ApplySpeedBonus(effectValue);
                    break;
                case EffectType.AttackSpeedBonus:
                    ApplyAttackSpeedBonus(effectValue);
                    break;
                case EffectType.ManaShield:
                    ApplyManaShield(effectValue);
                    break;
                case EffectType.ProjectileCount:
                    // 多重射击等特殊效果需要在具体技能中处理
                    break;
            }
        }
    }
    
    /// <summary>
    /// 应用伤害
    /// </summary>
    private void ApplyDamage(GameObject target, float damage)
    {
        if (target != null)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)damage);
            }
        }
    }
    
    /// <summary>
    /// 应用魔法伤害
    /// </summary>
    private void ApplyMagicDamage(GameObject target, float damage)
    {
        ApplyDamage(target, damage); // 简化处理，实际可能有不同的计算方式
    }
    
    /// <summary>
    /// 应用治疗
    /// </summary>
    private void ApplyHeal(GameObject target, float healAmount)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            // 这里需要与角色系统集成
            // player.Heal((int)healAmount);
        }
    }
    
    /// <summary>
    /// 应用防御加成
    /// </summary>
    private void ApplyDefenseBonus(float bonus)
    {
        // 这里需要与角色系统集成
    }
    
    /// <summary>
    /// 应用速度加成
    /// </summary>
    private void ApplySpeedBonus(float bonus)
    {
        // 这里需要与角色系统集成
    }
    
    /// <summary>
    /// 应用攻击速度加成
    /// </summary>
    private void ApplyAttackSpeedBonus(float bonus)
    {
        // 这里需要与角色系统集成
    }
    
    /// <summary>
    /// 应用魔法护盾
    /// </summary>
    private void ApplyManaShield(float shieldAmount)
    {
        // 这里需要与角色系统集成
    }
    
    /// <summary>
    /// 显示技能特效
    /// </summary>
    private void ShowSkillEffect(PlayerSkill skill, Vector3 position)
    {
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect($"skill_{skill.skillId}", position);
        }
    }
    
    /// <summary>
    /// 检查是否可以学习技能
    /// </summary>
    private bool CanLearnSkill(SkillData skillData)
    {
        // 检查是否已经学会
        if (learnedSkills.ContainsKey(skillData.skillId))
        {
            return false;
        }
        
        // 检查职业要求
        if (GameManager.Instance != null)
        {
            string playerClass = GameManager.Instance.GetPlayerClass();
            if (!string.IsNullOrEmpty(skillData.characterClass) && !skillData.characterClass.Equals(playerClass, System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        
        // 检查等级要求
        if (GameManager.Instance != null && GameManager.Instance.GetPlayerLevel() < skillData.requiredLevel)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage($"需要等级 {skillData.requiredLevel}");
            }
            return false;
        }
        
        // 检查前置技能
        foreach (string prerequisite in skillData.prerequisites)
        {
            if (!learnedSkills.ContainsKey(prerequisite))
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage("需要先学习前置技能！");
                }
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查是否可以升级技能
    /// </summary>
    private bool CanUpgradeSkill(PlayerSkill skill)
    {
        if (skill.currentLevel >= skill.maxLevel)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("技能已达最大等级！");
            }
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查是否可以使用技能
    /// </summary>
    private bool CanUseSkill(PlayerSkill skill)
    {
        // 检查技能类型
        if (skill.skillType == SkillType.Passive)
        {
            return false;
        }
        
        // 检查冷却时间
        if (Time.time - skill.lastUsedTime < skill.cooldown)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 计算升级所需技能点
    /// </summary>
    private int CalculateUpgradeSkillPoints(PlayerSkill skill)
    {
        return skill.currentLevel; // 简单的线性增长
    }
    
    /// <summary>
    /// 检查是否有足够技能点
    /// </summary>
    private bool HasEnoughSkillPoints(int required)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerSkillPoints() >= required;
        }
        return false;
    }
    
    /// <summary>
    /// 消耗技能点
    /// </summary>
    private void ConsumeSkillPoints(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerSkillPoints(-amount);
        }
    }
    
    /// <summary>
    /// 检查是否有足够魔法值
    /// </summary>
    private bool HasEnoughMana(int required)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerMana() >= required;
        }
        return false;
    }
    
    /// <summary>
    /// 消耗魔法值
    /// </summary>
    private void ConsumeMana(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerMana(-amount);
        }
    }
    
    /// <summary>
    /// 重置技能
    /// </summary>
    public bool ResetSkills()
    {
        if (!allowSkillReset)
        {
            return false;
        }
        
        // 检查重置费用
        if (GameManager.Instance != null && GameManager.Instance.GetPlayerGold() < skillResetCost)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("金币不足！");
            }
            return false;
        }
        
        // 计算返还的技能点
        int totalSkillPoints = 0;
        foreach (PlayerSkill skill in learnedSkills.Values)
        {
            totalSkillPoints += skill.currentLevel;
        }
        
        // 清空已学技能
        learnedSkills.Clear();
        
        // 扣除费用
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerGold(-skillResetCost);
            GameManager.Instance.AddPlayerSkillPoints(totalSkillPoints);
        }
        
        // 触发事件
        OnSkillPointsChanged?.Invoke();
        OnSkillTreeUpdated?.Invoke();
        
        // 显示消息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"技能重置完成，返还 {totalSkillPoints} 技能点");
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取已学技能
    /// </summary>
    public PlayerSkill GetLearnedSkill(string skillId)
    {
        return learnedSkills.ContainsKey(skillId) ? learnedSkills[skillId] : null;
    }
    
    /// <summary>
    /// 获取所有已学技能
    /// </summary>
    public List<PlayerSkill> GetAllLearnedSkills()
    {
        return learnedSkills.Values.ToList();
    }
    
    /// <summary>
    /// 获取技能树
    /// </summary>
    public SkillTree GetSkillTree(string characterClass)
    {
        return skillTrees.ContainsKey(characterClass) ? skillTrees[characterClass] : null;
    }
    
    /// <summary>
    /// 获取技能数据
    /// </summary>
    public SkillData GetSkillData(string skillId)
    {
        return skillDatabase.ContainsKey(skillId) ? skillDatabase[skillId] : null;
    }
    
    /// <summary>
    /// 保存技能数据
    /// </summary>
    public SkillSaveData GetSaveData()
    {
        SkillSaveData saveData = new SkillSaveData();
        
        foreach (var kvp in learnedSkills)
        {
            saveData.learnedSkills.Add(kvp.Value.GetSaveData());
        }
        
        return saveData;
    }
    
    /// <summary>
    /// 加载技能数据
    /// </summary>
    public void LoadSaveData(SkillSaveData saveData)
    {
        learnedSkills.Clear();
        
        foreach (SkillSaveData.PlayerSkillData skillData in saveData.learnedSkills)
        {
            if (skillDatabase.ContainsKey(skillData.skillId))
            {
                PlayerSkill skill = new PlayerSkill(skillDatabase[skillData.skillId]);
                skill.LoadSaveData(skillData);
                learnedSkills[skillData.skillId] = skill;
            }
        }
    }
}

/// <summary>
/// 技能数据
/// </summary>
[System.Serializable]
public class SkillData
{
    public string skillId;
    public string skillName;
    public string description;
    public SkillType skillType;
    public string characterClass; // Changed from CharacterClass to string
    public int maxLevel = 10;
    public int requiredLevel = 1;
    public int requiredSkillPoints = 1;
    public List<string> prerequisites = new List<string>();
    public int manaCost;
    public float cooldown;
    public float castTime;
    public float range;
    public List<SkillEffect> effects = new List<SkillEffect>();
    public string iconPath;
}

/// <summary>
/// 玩家技能
/// </summary>
[Serializable]
public class PlayerSkill
{
    public string skillId;
    public string skillName;
    public string description;
    public SkillType skillType;
    public string characterClass; // Changed from CharacterClass to string
    public int currentLevel;
    public int maxLevel;
    public int manaCost;
    public float cooldown;
    public float castTime;
    public float range;
    public List<SkillEffect> effects;
    public float lastUsedTime;
    
    public PlayerSkill(SkillData data)
    {
        skillId = data.skillId;
        skillName = data.skillName;
        description = data.description;
        skillType = data.skillType;
        characterClass = data.characterClass;
        currentLevel = 1;
        maxLevel = data.maxLevel;
        manaCost = data.manaCost;
        cooldown = data.cooldown;
        castTime = data.castTime;
        range = data.range;
        effects = new List<SkillEffect>(data.effects);
        lastUsedTime = 0f;
    }
    
    public int GetManaCost()
    {
        return Mathf.RoundToInt(manaCost * (1 + (currentLevel - 1) * 0.1f));
    }
    
    public float GetCooldown()
    {
        return cooldown * (1 - (currentLevel - 1) * 0.05f);
    }
    
    public bool IsOnCooldown()
    {
        return Time.time - lastUsedTime < GetCooldown();
    }
    
    public float GetCooldownRemaining()
    {
        float remaining = GetCooldown() - (Time.time - lastUsedTime);
        return Mathf.Max(0, remaining);
    }
    
    public SkillSaveData.PlayerSkillData GetSaveData()
    {
        return new SkillSaveData.PlayerSkillData
        {
            skillId = skillId,
            currentLevel = currentLevel,
            lastUsedTime = lastUsedTime
        };
    }
    
    public void LoadSaveData(SkillSaveData.PlayerSkillData saveData)
    {
        currentLevel = saveData.currentLevel;
        lastUsedTime = saveData.lastUsedTime;
    }
}

/// <summary>
/// 技能效果
/// </summary>
[Serializable]
public class SkillEffect
{
    public EffectType type;
    public float baseValue;
    public float scalingPerLevel;
    public float duration;
    
    public float GetValue(int level)
    {
        return baseValue + scalingPerLevel * (level - 1);
    }
}

/// <summary>
/// 技能树
/// </summary>
[Serializable]
public class SkillTree
{
    public string characterClass; // Changed from CharacterClass to string
    public List<SkillNode> skillNodes = new List<SkillNode>();
}

/// <summary>
/// 技能节点
/// </summary>
[Serializable]
public class SkillNode
{
    public string skillId;
    public Vector2 position;
    public List<string> prerequisites;
    public int tier;
}

/// <summary>
/// 技能保存数据
/// </summary>
[Serializable]
public class SkillSaveData
{
    public List<PlayerSkillData> learnedSkills = new List<PlayerSkillData>();
    
    [Serializable]
    public class PlayerSkillData
    {
        public string skillId;
        public int currentLevel;
        public float lastUsedTime;
    }
}

/// <summary>
/// 技能类型枚举
/// </summary>
public enum SkillType
{
    Active,      // 主动技能
    Passive,     // 被动技能
    Toggle       // 切换技能
}

/// <summary>
/// 效果类型枚举
/// </summary>
public enum EffectType
{
    Damage,              // 物理伤害
    MagicDamage,         // 魔法伤害
    Heal,                // 治疗
    DefenseBonus,        // 防御加成
    SpeedBonus,          // 速度加成
    AttackSpeedBonus,    // 攻击速度加成
    ManaShield,          // 魔法护盾
    ProjectileCount,     // 投射物数量
    CriticalChance,      // 暴击几率
    CriticalDamage       // 暴击伤害
}