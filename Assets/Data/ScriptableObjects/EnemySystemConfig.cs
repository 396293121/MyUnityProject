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
public class EnemySystemConfig : ScriptableObject
{
    [TitleGroup("系统配置")]
    [FoldoutGroup("系统配置/基础设置", expanded: true)]
    [HorizontalGroup("系统配置/基础设置/设置选项")]
    [VerticalGroup("系统配置/基础设置/设置选项/性能设置")]
    [LabelText("最大敌人数量")]
    [PropertyRange(1, 100)]
    [InfoBox("设置场景中同时存在的最大敌人数量", InfoMessageType.Info)]
    public int maxEnemyCount = 10;
    
    [VerticalGroup("系统配置/基础设置/设置选项/性能设置")]
    [LabelText("敌人AI更新频率")]
    [PropertyRange(0.01f, 1f)]
    [SuffixLabel("秒")]
    public float aiUpdateInterval = 0.2f;
    
    [FoldoutGroup("系统配置/敌人类型", expanded: false)]
    [HorizontalGroup("系统配置/敌人类型/类型配置")]
    [VerticalGroup("系统配置/敌人类型/类型配置/野猪配置")]
    [LabelText("野猪基础配置")]
    [InlineEditor(InlineEditorModes.GUIAndHeader)]
    [InfoBox("配置野猪敌人的各项属性和行为", InfoMessageType.Info)]
    public WildBoarConfig wildBoarConfig;
}
/// <summary>
/// 敌人基类配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class enmeyConfig{
    [FoldoutGroup("基础属性", expanded: true)]
    [LabelText("生命值")]
    [PropertyRange(1, 1000)]
    [ProgressBar(0, 200, ColorGetter = "GetHealthBarColor")]
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
    [LabelText("攻击伤害")]
    [PropertyRange(1, 100)]
    [PropertyOrder(3)]
    public int attackDamage = 15;
    
    [FoldoutGroup("基础属性")]
    [LabelText("物理攻击力")]
    [PropertyRange(1, 100)]
    [PropertyOrder(3.5f)]
    public float physicalAttack = 15f;
    
    [FoldoutGroup("基础属性")]
    [LabelText("防御力")]
    [PropertyRange(0, 50)]
    [PropertyOrder(3.6f)]
    public float defense = 5f;
    
    [FoldoutGroup("基础属性")]
    [LabelText("攻击范围")]
    [PropertyRange(0.1f, 10f)]
    [SuffixLabel("米")]
    [PropertyOrder(4)]
    public float attackRange = 1.5f;
    
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
    [LabelText("攻击冷却时间")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("秒")]
    [PropertyOrder(7)]
    public float attackCooldown = 2f;
    
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
    
    [FoldoutGroup("巡逻属性")]
    [LabelText("巡逻半径")]
    [PropertyRange(1f, 10f)]
    [SuffixLabel("米")]
    [PropertyOrder(14)]
    public float patrolRadius = 3f;
    
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
}
/// <summary>
/// 野猪配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class WildBoarConfig : enmeyConfig
{
    [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("冲锋触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发冲锋技能")]
    public float chargeDistance = 6f;
    
    [FoldoutGroup("技能触发条件")]
    [LabelText("狂暴血量阈值")]
    [PropertyRange(0.1f, 0.8f)]
    [SuffixLabel("%")]
    [PropertyOrder(7)]
    [InfoBox("血量低于此百分比时触发狂暴技能")]
    public float enrageHealthThreshold = 0.3f;
    
    [FoldoutGroup("技能触发条件")]
    [LabelText("眩晕持续时间")]
    [PropertyRange(0.5f, 4f)]
    [SuffixLabel("秒")]
    [PropertyOrder(8)]
    [InfoBox("眩晕持续时间")]
    public float stunDuration = 2f;




#if UNITY_EDITOR
    private Color GetHealthBarColor()
    {
        if (health <= 25) return Color.red;
        if (health <= 50) return Color.yellow;
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




// 移除旧的EnemyConfig类，现在使用WildBoarConfig等具体配置