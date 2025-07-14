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
    [LabelText("敌人更新频率")]
    [PropertyRange(0.01f, 1f)]
    [SuffixLabel("秒")]
    public float enemyUpdateInterval = 0.1f;
    
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
    
    [TitleGroup("AI配置")]
    [FoldoutGroup("AI配置/状态机", expanded: false)]
    [HorizontalGroup("AI配置/状态机/状态设置")]
    [VerticalGroup("AI配置/状态机/状态设置/状态列表")]
    [LabelText("敌人状态配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "stateName")]
    [InfoBox("配置敌人的各种状态和状态转换规则", InfoMessageType.Info)]
    public List<EnemyStateConfig> enemyStates = new List<EnemyStateConfig>();
    
    [FoldoutGroup("AI配置/行为设置", expanded: false)]
    [HorizontalGroup("AI配置/行为设置/行为配置")]
    [VerticalGroup("AI配置/行为设置/行为配置/移动行为")]
    [LabelText("巡逻行为配置")]
    [InlineEditor(InlineEditorModes.GUIOnly)]
    public PatrolBehaviorConfig patrolBehavior;
    
    [VerticalGroup("AI配置/行为设置/行为配置/移动行为")]
    [LabelText("追击行为配置")]
    [InlineEditor(InlineEditorModes.GUIOnly)]
    public ChaseBehaviorConfig chaseBehavior;
    
    [VerticalGroup("AI配置/行为设置/行为配置/战斗行为")]
    [LabelText("攻击行为配置")]
    [InlineEditor(InlineEditorModes.GUIOnly)]
    public AttackBehaviorConfig attackBehavior;
    
    [VerticalGroup("AI配置/行为设置/行为配置/战斗行为")]
    [LabelText("冲锋行为配置")]
    [InlineEditor(InlineEditorModes.GUIOnly)]
    public ChargeBehaviorConfig chargeBehavior;
    
    [FoldoutGroup("系统配置/碰撞检测", expanded: false)]
    [HorizontalGroup("系统配置/碰撞检测/检测设置")]
    [VerticalGroup("系统配置/碰撞检测/检测设置/层级设置")]
    [LabelText("碰撞检测层")]
    [InfoBox("设置敌人与环境的碰撞检测层级", InfoMessageType.Info)]
    public LayerMask collisionLayers = -1;
    
    [VerticalGroup("系统配置/碰撞检测/检测设置/层级设置")]
    [LabelText("平台碰撞层")]
    public LayerMask platformLayers = -1;
    
    [VerticalGroup("系统配置/碰撞检测/检测设置/层级设置")]
    [LabelText("玩家碰撞层")]
    public LayerMask playerLayers = -1;
    
    [FoldoutGroup("系统配置/掉落配置", expanded: false)]
    [HorizontalGroup("系统配置/掉落配置/掉落设置")]
    [VerticalGroup("系统配置/掉落配置/掉落设置/全局掉落")]
    [LabelText("全局掉落配置")]
    [InfoBox("配置敌人死亡后的掉落物品和金币", InfoMessageType.Info)]
    public GlobalDropConfig globalDropConfig;
    
    [FoldoutGroup("系统配置/动画配置", expanded: false)]
    [HorizontalGroup("系统配置/动画配置/动画设置")]
    [VerticalGroup("系统配置/动画配置/动画设置/全局动画")]
    [LabelText("全局动画配置")]
    [InfoBox("配置敌人的动画参数和效果", InfoMessageType.Info)]
    public GlobalAnimationConfig globalAnimationConfig;
    
    [FoldoutGroup("系统配置/物理配置", expanded: false)]
    [HorizontalGroup("系统配置/物理配置/物理设置")]
    [VerticalGroup("系统配置/物理配置/物理设置/碰撞体")]
    [LabelText("全局物理配置")]
    [InfoBox("配置敌人的物理体和碰撞参数", InfoMessageType.Info)]
    public GlobalPhysicsConfig globalPhysicsConfig;
}

/// <summary>
/// 野猪配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class WildBoarConfig
{
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
    [LabelText("攻击伤害")]
    [PropertyRange(1, 100)]
    [PropertyOrder(3)]
    public int attackDamage = 15;
    
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
    
    [FoldoutGroup("冲锋属性", expanded: true)]
    [LabelText("冲锋速度")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("单位/秒")]
    [PropertyOrder(6)]
    public float chargeSpeed = 8f;
    
    [FoldoutGroup("冲锋属性")]
    [LabelText("冲锋距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(7)]
    public float chargeDistance = 6f;
    
    [FoldoutGroup("冲锋属性")]
    [LabelText("冲锋冷却时间")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("秒")]
    [PropertyOrder(8)]
    public float chargeCooldown = 3f;
    
    [FoldoutGroup("冲锋属性")]
    [LabelText("冲锋持续时间")]
    [PropertyRange(0.5f, 5f)]
    [SuffixLabel("秒")]
    [PropertyOrder(9)]
    public float chargeDuration = 2f;
    
    [FoldoutGroup("冲锋属性")]
    [LabelText("冲锋伤害")]
    [PropertyRange(10, 100)]
    [PropertyOrder(10)]
    public int chargeDamage = 25;
    
    [FoldoutGroup("冲锋属性")]
    [LabelText("冲锋准备时间")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [PropertyOrder(11)]
    public float chargePreparationTime = 0.5f;
    
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
/// 全局掉落配置
/// 从EnemyConfig迁移，避免重复配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class GlobalDropConfig
{
    [TitleGroup("掉落配置")]
    [FoldoutGroup("掉落配置/物品掉落", expanded: true)]
    [LabelText("默认掉落物品")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "itemId")]
    public List<EnemyDropItem> defaultDrops = new List<EnemyDropItem>();
    
    [FoldoutGroup("掉落配置/金币掉落", expanded: true)]
    [LabelText("默认金币掉落")]
    [InlineProperty]
    public EnemyGoldDrop defaultGoldDrop;
    
    [FoldoutGroup("掉落配置/掉落规则", expanded: true)]
    [LabelText("掉落概率加成")]
    [PropertyRange(0f, 2f)]
    [SuffixLabel("倍数")]
    public float dropRateMultiplier = 1f;
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
/// 全局动画配置
/// 从EnemyConfig迁移，避免重复配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class GlobalAnimationConfig
{
    [TitleGroup("动画配置")]
    [FoldoutGroup("动画配置/基础动画", expanded: true)]
    [LabelText("默认动画控制器")]
    public RuntimeAnimatorController defaultAnimatorController;
    
    [FoldoutGroup("动画配置/基础动画", expanded: true)]
    [LabelText("动画过渡时间")]
    [PropertyRange(0f, 1f)]
    [SuffixLabel("秒")]
    public float animationTransitionTime = 0.1f;
    
    [FoldoutGroup("动画配置/攻击动画", expanded: true)]
    [LabelText("攻击关键帧")]
    [PropertyRange(1, 20)]
    public int attackKeyFrame = 6;
    
    [FoldoutGroup("动画配置/攻击动画", expanded: true)]
    [LabelText("攻击击退力度")]
    [PropertyRange(0f, 500f)]
    [SuffixLabel("力度")]
    public float attackKnockbackForce = 150f;
    
    [FoldoutGroup("动画配置/攻击动画", expanded: true)]
    [LabelText("攻击击退持续时间")]
    [PropertyRange(0f, 1f)]
    [SuffixLabel("秒")]
    public float attackKnockbackDuration = 0.3f;
    
    [FoldoutGroup("动画配置/受伤动画", expanded: true)]
    [LabelText("受伤无敌时间")]
    [PropertyRange(0f, 2f)]
    [SuffixLabel("秒")]
    public float hurtInvulnerabilityDuration = 0.5f;
    
    [FoldoutGroup("动画配置/受伤动画", expanded: true)]
    [LabelText("受伤闪烁时间")]
    [PropertyRange(0f, 1f)]
    [SuffixLabel("秒")]
    public float hurtFlashDuration = 0.1f;
    
    [FoldoutGroup("动画配置/受伤动画", expanded: true)]
    [LabelText("击退抗性")]
    [PropertyRange(0f, 1f)]
    [SuffixLabel("抗性")]
    public float knockbackResistance = 0.7f;
    
    [FoldoutGroup("动画配置/死亡动画", expanded: true)]
    [LabelText("死亡淡出时间")]
    [PropertyRange(0f, 5f)]
    [SuffixLabel("秒")]
    public float deathFadeOutDuration = 1f;
    
    [FoldoutGroup("动画配置/死亡动画", expanded: true)]
    [LabelText("尸体消失延迟")]
    [PropertyRange(0f, 3f)]
    [SuffixLabel("秒")]
    public float bodyDisableDelay = 0.5f;
    
    [FoldoutGroup("动画配置/冲锋动画", expanded: true)]
    [LabelText("冲锋速度倍数")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("倍数")]
    public float chargeSpeedMultiplier = 2f;
    
    [FoldoutGroup("动画配置/冲锋动画", expanded: true)]
    [LabelText("冲锋帧率倍数")]
    [PropertyRange(1f, 3f)]
    [SuffixLabel("倍数")]
    public float chargeFrameRateMultiplier = 1.5f;
    
    [FoldoutGroup("动画配置/特效动画", expanded: true)]
    [LabelText("死亡特效预制体")]
    public GameObject deathEffectPrefab;
    
    [FoldoutGroup("动画配置/特效动画", expanded: true)]
    [LabelText("受击特效预制体")]
    public GameObject hitEffectPrefab;
}

/// <summary>
/// 全局物理配置
/// 从EnemyConfig迁移，避免重复配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class GlobalPhysicsConfig
{
    [TitleGroup("物理配置")]
    [FoldoutGroup("物理配置/碰撞体配置", expanded: true)]
    [LabelText("默认碰撞体大小")]
    public Vector2 defaultColliderSize = Vector2.one;
    
    [FoldoutGroup("物理配置/碰撞体配置", expanded: true)]
    [LabelText("默认碰撞体偏移")]
    public Vector2 defaultColliderOffset = Vector2.zero;
    
    [FoldoutGroup("物理配置/碰撞体配置", expanded: true)]
    [LabelText("默认触发器模式")]
    public bool defaultIsTrigger = false;
    
    [FoldoutGroup("物理配置/攻击框配置", expanded: true)]
    [LabelText("默认攻击框大小")]
    public Vector2 defaultHitboxSize = Vector2.one;
    
    [FoldoutGroup("物理配置/攻击框配置", expanded: true)]
    [LabelText("默认攻击框偏移")]
    public Vector2 defaultHitboxOffset = Vector2.zero;
    
    [FoldoutGroup("物理配置/攻击框配置", expanded: true)]
    [LabelText("默认攻击框持续时间")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    public float defaultHitboxDuration = 0.2f;
    
    [FoldoutGroup("物理配置/物理参数", expanded: true)]
    [LabelText("弹性系数")]
    [PropertyRange(0f, 1f)]
    public float bounce = 0.1f;
    
    [FoldoutGroup("物理配置/物理参数", expanded: true)]
    [LabelText("距离阈值")]
    [PropertyRange(1f, 50f)]
    [SuffixLabel("米")]
    public float distanceThreshold = 10f;
    
    [FoldoutGroup("物理配置/物理参数", expanded: true)]
    [LabelText("移动阈值")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("米")]
    public float movementThreshold = 5f;
    
    [FoldoutGroup("物理配置/物理参数", expanded: true)]
    [LabelText("冲锋伤害倍数")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("倍数")]
    public float chargeDamageMultiplier = 1.5f;
}

/// <summary>
/// 敌人状态配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class EnemyStateConfig
{
    [TitleGroup("状态配置")]
    [FoldoutGroup("状态配置/基础信息", expanded: true)]
    [LabelText("状态名称")]
    [PropertyOrder(1)]
    public string stateName;
    
    [FoldoutGroup("状态配置/基础信息", expanded: true)]
    [LabelText("状态描述")]
    [TextArea(2, 4)]
    [PropertyOrder(2)]
    public string stateDescription;
    
    [FoldoutGroup("状态配置/基础信息", expanded: true)]
    [LabelText("状态类型")]
    [EnumToggleButtons]
    [PropertyOrder(3)]
    public EnemyStateType stateType;
    
    [FoldoutGroup("状态配置/基础信息", expanded: true)]
    [LabelText("状态持续时间")]
    [InfoBox("设置为-1表示无限持续", InfoMessageType.Info, "@stateDuration == -1")]
    [PropertyRange(0f, 10f)]
    [SuffixLabel("秒")]
    [PropertyOrder(4)]
    public float stateDuration = 1f;
    
    [FoldoutGroup("状态配置/状态效果", expanded: true)]
    [LabelText("移动速度倍数")]
    [PropertyRange(0f, 3f)]
    [SuffixLabel("倍数")]
    [PropertyOrder(5)]
    public float moveSpeedMultiplier = 1f;
    
    [FoldoutGroup("状态配置/状态效果", expanded: true)]
    [LabelText("攻击力倍数")]
    [PropertyRange(0f, 3f)]
    [SuffixLabel("倍数")]
    [PropertyOrder(6)]
    public float attackDamageMultiplier = 1f;
    
    [FoldoutGroup("状态配置/状态效果", expanded: true)]
    [LabelText("防御力倍数")]
    [PropertyRange(0f, 3f)]
    [SuffixLabel("倍数")]
    [PropertyOrder(7)]
    public float defenseMultiplier = 1f;
    
    [FoldoutGroup("状态配置/状态转换", expanded: true)]
    [LabelText("状态优先级")]
    [PropertyRange(0, 10)]
    [InfoBox("优先级越高，状态切换时越容易被选中", InfoMessageType.Info)]
    [PropertyOrder(8)]
    public int priority = 0;
    
    [FoldoutGroup("状态配置/状态转换", expanded: true)]
    [LabelText("可以转换到的状态")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    [PropertyOrder(9)]
    public List<string> allowedTransitions = new List<string>();
    
    [FoldoutGroup("状态配置/视听效果", expanded: true)]
    [LabelText("状态动画名称")]
    [PropertyOrder(10)]
    public string animationName;
    
    [FoldoutGroup("状态配置/视听效果", expanded: true)]
    [LabelText("状态音效")]
    [PropertyOrder(11)]
    public string soundEffect;
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
    Charge,     // 冲锋
    Stunned,    // 眩晕
    Death       // 死亡
}

/// <summary>
/// 巡逻行为配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class PatrolBehaviorConfig
{
    [TitleGroup("巡逻配置")]
    [FoldoutGroup("巡逻配置/基础设置", expanded: true)]
    [LabelText("巡逻类型")]
    [EnumToggleButtons]
    [PropertyOrder(1)]
    public PatrolType patrolType = PatrolType.BackAndForth;
    
    [FoldoutGroup("巡逻配置/基础设置", expanded: true)]
    [LabelText("巡逻速度倍数")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("倍数")]
    [PropertyOrder(2)]
    public float speedMultiplier = 0.5f;
    
    [FoldoutGroup("巡逻配置/基础设置", expanded: true)]
    [LabelText("到达巡逻点的距离阈值")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("米")]
    [PropertyOrder(3)]
    public float arrivalThreshold = 0.5f;
    
    [FoldoutGroup("巡逻配置/高级设置", expanded: false)]
    [LabelText("巡逻点等待时间")]
    [PropertyRange(0f, 10f)]
    [SuffixLabel("秒")]
    [PropertyOrder(4)]
    public float waitTimeAtPoint = 1f;
    
    [FoldoutGroup("巡逻配置/高级设置", expanded: false)]
    [LabelText("随机等待时间范围")]
    [InfoBox("X为最小值，Y为最大值", InfoMessageType.Info)]
    [PropertyOrder(5)]
    public Vector2 randomWaitRange = new Vector2(0.5f, 2f);
}

public enum PatrolType
{
    BackAndForth,   // 来回巡逻
    Circular,       // 循环巡逻
    Random          // 随机巡逻
}

/// <summary>
/// 追击行为配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class ChaseBehaviorConfig
{
    [TitleGroup("追击配置")]
    [FoldoutGroup("追击配置/基础设置", expanded: true)]
    [LabelText("追击速度倍数")]
    [PropertyRange(0.5f, 3f)]
    [SuffixLabel("倍数")]
    [PropertyOrder(1)]
    public float speedMultiplier = 1.5f;
    
    [FoldoutGroup("追击配置/基础设置", expanded: true)]
    [LabelText("追击距离阈值")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("米")]
    [InfoBox("敌人与目标的最小追击距离", InfoMessageType.Info)]
    [PropertyOrder(2)]
    public float chaseThreshold = 0.8f;
    
    [FoldoutGroup("追击配置/基础设置", expanded: true)]
    [LabelText("失去目标的时间")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("秒")]
    [PropertyOrder(3)]
    public float loseTargetTime = 3f;
    
    [FoldoutGroup("追击配置/高级设置", expanded: false)]
    [LabelText("追击时的加速度")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("单位/秒²")]
    [PropertyOrder(4)]
    public float acceleration = 2f;
    
    [FoldoutGroup("追击配置/高级设置", expanded: false)]
    [LabelText("最大追击距离")]
    [PropertyRange(5f, 50f)]
    [SuffixLabel("米")]
    [InfoBox("超过此距离将放弃追击", InfoMessageType.Warning)]
    [PropertyOrder(5)]
    public float maxChaseDistance = 10f;
}

/// <summary>
/// 攻击行为配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class AttackBehaviorConfig
{
    [TitleGroup("攻击配置")]
    [FoldoutGroup("攻击配置/基础设置", expanded: true)]
    [LabelText("攻击间隔")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("秒")]
    [PropertyOrder(1)]
    public float attackInterval = 1.5f;
    
    [FoldoutGroup("攻击配置/基础设置", expanded: true)]
    [LabelText("攻击前摇时间")]
    [PropertyRange(0f, 2f)]
    [SuffixLabel("秒")]
    [InfoBox("攻击动画开始到造成伤害的时间", InfoMessageType.Info)]
    [PropertyOrder(2)]
    public float attackWindupTime = 0.3f;
    
    [FoldoutGroup("攻击配置/基础设置", expanded: true)]
    [LabelText("攻击后摇时间")]
    [PropertyRange(0f, 2f)]
    [SuffixLabel("秒")]
    [InfoBox("攻击后的恢复时间", InfoMessageType.Info)]
    [PropertyOrder(3)]
    public float attackRecoveryTime = 0.5f;
    
    [FoldoutGroup("攻击配置/基础设置", expanded: true)]
    [LabelText("攻击范围角度")]
    [PropertyRange(0f, 360f)]
    [SuffixLabel("度")]
    [PropertyOrder(4)]
    public float attackAngle = 45f;
    
    [FoldoutGroup("攻击配置/连击设置", expanded: false)]
    [LabelText("连击次数")]
    [PropertyRange(1, 10)]
    [PropertyOrder(5)]
    public int comboCount = 1;
    
    [FoldoutGroup("攻击配置/连击设置", expanded: false)]
    [LabelText("连击间隔")]
    [PropertyRange(0.1f, 2f)]
    [SuffixLabel("秒")]
    [ShowIf("@comboCount > 1")]
    [PropertyOrder(6)]
    public float comboInterval = 0.2f;
}

/// <summary>
/// 冲锋行为配置
/// </summary>
[System.Serializable]
[InlineProperty]
public class ChargeBehaviorConfig
{
    [TitleGroup("冲锋配置")]
    [FoldoutGroup("冲锋配置/基础设置", expanded: true)]
    [LabelText("冲锋触发距离")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("米")]
    [PropertyOrder(1)]
    public float chargeTriggerDistance = 4f;
    
    [FoldoutGroup("冲锋配置/基础设置", expanded: true)]
    [LabelText("冲锋最小距离")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("米")]
    [PropertyOrder(2)]
    public float chargeMinDistance = 2f;
    
    [FoldoutGroup("冲锋配置/物理设置", expanded: false)]
    [LabelText("冲锋加速度")]
    [PropertyRange(1f, 50f)]
    [SuffixLabel("单位/秒²")]
    [PropertyOrder(3)]
    public float chargeAcceleration = 10f;
    
    [FoldoutGroup("冲锋配置/物理设置", expanded: false)]
    [LabelText("冲锋减速度")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("单位/秒²")]
    [PropertyOrder(4)]
    public float chargeDeceleration = 5f;
    
    [FoldoutGroup("冲锋配置/物理设置", expanded: false)]
    [LabelText("冲锋后眩晕时间")]
    [PropertyRange(0f, 5f)]
    [SuffixLabel("秒")]
    [InfoBox("冲锋结束后敌人的眩晕时间", InfoMessageType.Info)]
    [PropertyOrder(5)]
    public float chargeStunTime = 1f;
    
    [FoldoutGroup("冲锋配置/伤害设置", expanded: false)]
    [LabelText("冲锋伤害倍数")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("倍数")]
    [PropertyOrder(6)]
    public float chargeDamageMultiplier = 1.5f;
    
    [FoldoutGroup("冲锋配置/视觉效果", expanded: false)]
    [LabelText("冲锋特效")]
    [AssetsOnly]
    [PropertyOrder(7)]
    public GameObject chargeEffect;
    
    [FoldoutGroup("冲锋配置/视觉效果", expanded: false)]
    [LabelText("冲锋轨迹特效")]
    [AssetsOnly]
    [PropertyOrder(8)]
    public GameObject chargeTrailEffect;
}

// 移除旧的EnemyConfig类，现在使用WildBoarConfig等具体配置