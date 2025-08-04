using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 敌人系统配置 ScriptableObject
/// 基于Phaser项目中的敌人系统设计
/// 整合了掉落、动画和物理配置，避免重复
/// </summary>
[CreateAssetMenu(fileName = "EnemySystemConfig", menuName = "Game/Enemy System Config")]
[ShowOdinSerializedPropertiesInInspector]
/// <summary>
/// 敌人基类配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class EnemyConfig: ScriptableObject
{
    [BoxGroup("标识信息", Order = 0)]
    [LabelText("敌人ID")]
    [InfoBox("唯一标识符，用于任务系统和游戏逻辑识别")]
    [PropertyOrder(0)]
    public string enemyId = "";
    
    [BoxGroup("标识信息")]
    [LabelText("显示名称")]
    [PropertyOrder(0.1f)]
    public string displayName = "";
    
    [BoxGroup("标识信息")]
    [LabelText("敌人描述")]
    [TextArea(2, 4)]
    [PropertyOrder(0.2f)]
    public string description = "";
    
    [BoxGroup("标识信息")]
    [LabelText("敌人图标")]
    [PropertyOrder(0.3f)]
    public Sprite enemyIcon;
    
    [BoxGroup("标识信息")]
    [LabelText("敌人类型")]
    [PropertyOrder(0.4f)]
    public EnemyCategory category = EnemyCategory.Normal;
    
    [BoxGroup("标识信息")]
    [LabelText("稀有度")]
    [PropertyOrder(0.5f)]
    public EnemyRarity rarity = EnemyRarity.Common;
    [BoxGroup("标识信息")]
    [LabelText("敌人UI预制体")]
    [PropertyOrder(0.6f)]
    public EnemyUI enemyUI;
    [FoldoutGroup("基础属性", expanded: true)]
    [LabelText("生命值")]
    [PropertyRange(1, 1000)]
    [ProgressBar(0, 1000, ColorGetter = "GetHealthBarColor")]
    [PropertyOrder(1)]
    public int health = 50;
    
    [FoldoutGroup("基础属性")]
    [LabelText("移动速度")]
    [PropertyRange(0.1f, 10f)]
    [SuffixLabel("单位/秒")]
    [PropertyOrder(2)]
    public float moveSpeed = 2f;
        
    [FoldoutGroup("基础属性")]
    [LabelText("追击速度倍率")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("倍")]
    public float chaseSpeedRate = 1.2f;
    [FoldoutGroup("基础属性")]
    [LabelText("是否可以攻击")]
    public bool canAttack = true;
    [FoldoutGroup("基础属性")]
    [LabelText("攻击伤害")]
    [ShowIf("canAttack")]
    [PropertyRange(1, 100)]
    [PropertyOrder(3)]
    public int attackDamage = 15;
    [FoldoutGroup("基础属性")]
    [LabelText("伤害类型")]
      [ShowIf("canAttack")]
    public DamageType damageType = DamageType.Physical;
    
    [FoldoutGroup("基础属性")]
    [LabelText("攻击冷却时间")]
    [PropertyRange(0.5f, 10f)]
      [ShowIf("canAttack")]
    [SuffixLabel("秒")]
    [PropertyOrder(7)]
    public float attackCooldown = 2f;
    [FoldoutGroup("基础属性")]
    [LabelText("防御力")]
    [PropertyRange(0, 50)]
    [PropertyOrder(3.6f)]
    public float defense = 5f;
    
    [FoldoutGroup("基础属性")]
    [LabelText("检测范围")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("米")]
    [PropertyOrder(5)]
    public float detectionRange = 5f;
    
    [FoldoutGroup("基础属性")]
    [LabelText("失去目标范围")]
    [PropertyRange(5f, 30f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    public float loseTargetRange = 12f;
    [FoldoutGroup("基础属性")]
    [LabelText("保持距离")]
    [Tooltip("敌人与玩家保持的最小距离，小于此距离时会反向移动")]
    [Range(0f, 15f)]
    [PropertyOrder(7)]
    public float keepDistance = 0f;
    
    [FoldoutGroup("巡逻属性", expanded: true)]
    [LabelText("巡逻速度")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("单位/秒")]
    [PropertyOrder(12)]
    public float patrolSpeed = 1f;
    
    [FoldoutGroup("巡逻属性")]
    [LabelText("巡逻等待时间")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("秒")]
    [PropertyOrder(13)]
    public float patrolWaitTime = 2f;
    
    [FoldoutGroup("性能优化", expanded: false)]
    [LabelText("屏幕外更新间隔")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(16)]
    public float offScreenUpdateInterval = 0.5f;
    
    [FoldoutGroup("性能优化")]
    [LabelText("屏幕内更新间隔")]
    [PropertyRange(0.02f, 0.2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(17)]
    public float onScreenUpdateInterval = 0.05f;
    
    [FoldoutGroup("性能优化")]
    [LabelText("屏幕边界扩展")]
    [PropertyRange(1f, 10f)]
    [SuffixLabel("米")]
    [PropertyOrder(18)]
    public float screenBoundaryExtension = 5f;
    
    [FoldoutGroup("基础属性")]
    [LabelText("经验奖励")]
    [PropertyRange(1, 1000)]
    [PropertyOrder(15)]
    public int expReward = 10;
    
    [BoxGroup("任务相关配置", Order = 20)]
    [LabelText("可用于击杀任务")]
    [PropertyOrder(20)]
    public bool canBeKillTarget = true;
    
    [BoxGroup("任务相关配置")]
    [LabelText("可用于BOSS任务")]
    [PropertyOrder(21)]
    public bool canBeBossTarget = false;


    [BoxGroup("任务相关配置")]


    /// <summary>
    /// 验证配置
    /// </summary>
    public bool ValidateConfig()
    {
        if (string.IsNullOrEmpty(enemyId))
        {
            Debug.LogError($"敌人配置缺少ID: {name}");
            return false;
        }
        
        if (string.IsNullOrEmpty(displayName))
        {
            Debug.LogError($"敌人配置缺少显示名称: {enemyId}");
            return false;
        }
        
        if (health <= 0)
        {
            Debug.LogError($"敌人配置生命值无效: {enemyId}");
            return false;
        }
        
        return true;
    }
    
 
    


#if UNITY_EDITOR
    private Color GetHealthBarColor()
    {
        if (health <= 50) return Color.red;
        if (health <= 200) return Color.yellow;
        return Color.green;
    }
#endif
}


/// <summary>
/// 敌人掉落物品配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class EnemyDropItem
{
    [LabelText("物品ID")]
    [PropertyOrder(1)]
    public string itemId;
    
    [LabelText("掉落概率")]
    [PropertyRange(0f, 1f)]
    [ProgressBar(0, 1, ColorGetter = "GetDropChanceColor")]
    [PropertyOrder(2)]
    public float chance = 0.5f;
    
    [LabelText("最小数量")]
    [PropertyRange(1, 99)]
    [PropertyOrder(3)]
    public int minQuantity = 1;
    
    [LabelText("最大数量")]
    [PropertyRange(1, 99)]
    [PropertyOrder(4)]
    public int maxQuantity = 1;
    
#if UNITY_EDITOR
    private Color GetDropChanceColor()
    {
        if (chance <= 0.1f) return Color.red;
        if (chance <= 0.5f) return Color.yellow;
        return Color.green;
    }
#endif
}

/// <summary>
/// 敌人金币掉落配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class EnemyGoldDrop
{
    [LabelText("最小金币")]
    [PropertyRange(0, 1000)]
    [PropertyOrder(1)]
    public int min = 1;
    
    [LabelText("最大金币")]
    [PropertyRange(0, 1000)]
    [PropertyOrder(2)]
    public int max = 5;
    
    [LabelText("掉落概率")]
    [PropertyRange(0f, 1f)]
    [ProgressBar(0, 1)]
    [PropertyOrder(3)]
    public float dropChance = 0.8f;
}




/// <summary>
/// 敌人状态类型枚举
/// </summary>
public enum EnemyStateType
{
    Idle,       // 待机
    Patrol,     // 巡逻
    Chase,      // 追击
    Attack,     // 攻击
    Death       // 死亡
}

/// <summary>
/// 敌人类型枚举
/// </summary>
public enum EnemyCategory
{
    [LabelText("普通敌人")]
    Normal,
    [LabelText("精英敌人")]
    Elite,
    [LabelText("BOSS")]
    Boss,
    [LabelText("小BOSS")]
    MiniBoss,
    [LabelText("野兽")]
    Beast,
    [LabelText("不死族")]
    Undead,
    [LabelText("魔法生物")]
    Magical,
    [LabelText("机械")]
    Mechanical
}

/// <summary>
/// 敌人稀有度枚举
/// </summary>
public enum EnemyRarity
{
    [LabelText("普通")]
    Common,
    [LabelText("稀有")]
    Rare,
    [LabelText("史诗")]
    Epic,
    [LabelText("传说")]
    Legendary,
    [LabelText("神话")]
    Mythic
}

// 移除旧的EnemyConfig类，现在使用WildBoarConfig等具体配置