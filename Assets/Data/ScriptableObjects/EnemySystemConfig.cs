using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敌人系统配置 ScriptableObject
/// 基于Phaser项目中的敌人系统设计
/// </summary>
[CreateAssetMenu(fileName = "EnemySystemConfig", menuName = "Game/Enemy System Config")]
public class EnemySystemConfig : ScriptableObject
{
    [Header("敌人系统设置")]
    [Tooltip("最大敌人数量")]
    public int maxEnemyCount = 10;
    
    [Tooltip("敌人更新频率(秒)")]
    public float enemyUpdateInterval = 0.1f;
    
    [Tooltip("敌人AI更新频率(秒)")]
    public float aiUpdateInterval = 0.2f;
    
    [Header("野猪配置")]
    [Tooltip("野猪基础配置")]
    public WildBoarConfig wildBoarConfig;
    
    [Header("状态机配置")]
    [Tooltip("敌人状态配置")]
    public List<EnemyStateConfig> enemyStates = new List<EnemyStateConfig>();
    
    [Header("行为配置")]
    [Tooltip("巡逻行为配置")]
    public PatrolBehaviorConfig patrolBehavior;
    
    [Tooltip("追击行为配置")]
    public ChaseBehaviorConfig chaseBehavior;
    
    [Tooltip("攻击行为配置")]
    public AttackBehaviorConfig attackBehavior;
    
    [Tooltip("冲锋行为配置")]
    public ChargeBehaviorConfig chargeBehavior;
    
    [Header("碰撞检测配置")]
    [Tooltip("碰撞检测层")]
    public LayerMask collisionLayers = -1;
    
    [Tooltip("平台碰撞层")]
    public LayerMask platformLayers = -1;
    
    [Tooltip("玩家碰撞层")]
    public LayerMask playerLayers = -1;
}

/// <summary>
/// 野猪配置
/// </summary>
[System.Serializable]
public class WildBoarConfig
{
    [Header("基础属性")]
    [Tooltip("生命值")]
    public int health = 50;
    
    [Tooltip("移动速度")]
    public float moveSpeed = 2f;
    
    [Tooltip("攻击伤害")]
    public int attackDamage = 15;
    
    [Tooltip("攻击范围")]
    public float attackRange = 1.5f;
    
    [Tooltip("检测范围")]
    public float detectionRange = 5f;
    
    [Header("冲锋属性")]
    [Tooltip("冲锋速度")]
    public float chargeSpeed = 8f;
    
    [Tooltip("冲锋距离")]
    public float chargeDistance = 6f;
    
    [Tooltip("冲锋冷却时间")]
    public float chargeCooldown = 3f;
    
    [Tooltip("冲锋准备时间")]
    public float chargePreparationTime = 0.5f;
    
    [Header("巡逻属性")]
    [Tooltip("巡逻速度")]
    public float patrolSpeed = 1f;
    
    [Tooltip("巡逻等待时间")]
    public float patrolWaitTime = 2f;
    
    [Tooltip("巡逻半径")]
    public float patrolRadius = 3f;
}

/// <summary>
/// 敌人状态配置
/// </summary>
[System.Serializable]
public class EnemyStateConfig
{
    [Tooltip("状态名称")]
    public string stateName;
    
    [Tooltip("状态类型")]
    public EnemyStateType stateType;
    
    [Tooltip("状态持续时间(-1为无限)")]
    public float stateDuration = -1f;
    
    [Tooltip("状态优先级")]
    public int priority = 0;
    
    [Tooltip("可以转换到的状态")]
    public List<string> allowedTransitions = new List<string>();
    
    [Tooltip("状态动画名称")]
    public string animationName;
    
    [Tooltip("状态音效")]
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
public class PatrolBehaviorConfig
{
    [Tooltip("巡逻类型")]
    public PatrolType patrolType = PatrolType.BackAndForth;
    
    [Tooltip("巡逻速度倍数")]
    public float speedMultiplier = 0.5f;
    
    [Tooltip("到达巡逻点的距离阈值")]
    public float arrivalThreshold = 0.5f;
    
    [Tooltip("巡逻点等待时间")]
    public float waitTimeAtPoint = 1f;
    
    [Tooltip("随机等待时间范围")]
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
public class ChaseBehaviorConfig
{
    [Tooltip("追击速度倍数")]
    public float speedMultiplier = 1.5f;
    
    [Tooltip("追击距离阈值")]
    public float chaseThreshold = 0.8f;
    
    [Tooltip("失去目标的时间")]
    public float loseTargetTime = 3f;
    
    [Tooltip("追击时的加速度")]
    public float acceleration = 2f;
    
    [Tooltip("最大追击距离")]
    public float maxChaseDistance = 10f;
}

/// <summary>
/// 攻击行为配置
/// </summary>
[System.Serializable]
public class AttackBehaviorConfig
{
    [Tooltip("攻击间隔")]
    public float attackInterval = 1.5f;
    
    [Tooltip("攻击前摇时间")]
    public float attackWindupTime = 0.3f;
    
    [Tooltip("攻击后摇时间")]
    public float attackRecoveryTime = 0.5f;
    
    [Tooltip("攻击范围角度")]
    public float attackAngle = 45f;
    
    [Tooltip("连击次数")]
    public int comboCount = 1;
    
    [Tooltip("连击间隔")]
    public float comboInterval = 0.2f;
}

/// <summary>
/// 冲锋行为配置
/// </summary>
[System.Serializable]
public class ChargeBehaviorConfig
{
    [Tooltip("冲锋触发距离")]
    public float chargeTriggerDistance = 4f;
    
    [Tooltip("冲锋最小距离")]
    public float chargeMinDistance = 2f;
    
    [Tooltip("冲锋加速度")]
    public float chargeAcceleration = 10f;
    
    [Tooltip("冲锋减速度")]
    public float chargeDeceleration = 5f;
    
    [Tooltip("冲锋后眩晕时间")]
    public float chargeStunTime = 1f;
    
    [Tooltip("冲锋伤害倍数")]
    public float chargeDamageMultiplier = 1.5f;
    
    [Tooltip("冲锋特效")]
    public GameObject chargeEffect;
    
    [Tooltip("冲锋轨迹特效")]
    public GameObject chargeTrailEffect;
}

/// <summary>
/// 敌人配置
/// </summary>
[System.Serializable]
public class EnemyConfig
{
    [Header("基础属性")]
    [Tooltip("生命值")]
    public int health = 100;
    
    [Tooltip("攻击伤害")]
    public int damage = 10;
    
    [Tooltip("移动速度")]
    public float speed = 2f;
    
    [Tooltip("攻击范围")]
    public float attackRange = 1.5f;
    
    [Tooltip("检测范围")]
    public float detectionRange = 5f;
    
    [Header("战斗属性")]
    [Tooltip("攻击冷却时间")]
    public float attackCooldown = 1.5f;
    
    [Tooltip("防御力")]
    public int defense = 0;
    
    [Tooltip("经验奖励")]
    public int expReward = 10;
    
    [Header("AI属性")]
    [Tooltip("巡逻半径")]
    public float patrolRadius = 3f;
    
    [Tooltip("追击距离")]
    public float chaseDistance = 8f;
    
    [Tooltip("失去目标时间")]
    public float loseTargetTime = 3f;
}