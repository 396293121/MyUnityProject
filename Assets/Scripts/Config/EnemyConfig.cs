using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人配置文件
/// 统一管理所有敌人的配置信息
/// 从原Phaser项目的EnemyConfig.js迁移而来
/// </summary>
[System.Serializable]
public class EnemyStats
{
    [Header("基础属性")]
    public int health = 40;
    public int attack = 18;
    public int defense = 15;
    public float speed = 80f;
    public int exp = 15;
}

[System.Serializable]
public class EnemyDropItem
{
    [Header("掉落物品")]
    public string itemId;
    [Range(0f, 1f)]
    public float chance = 0.5f;
    public int minQuantity = 1;
    public int maxQuantity = 1;
}

[System.Serializable]
public class EnemyGoldDrop
{
    [Header("金币掉落")]
    public int min = 1;
    public int max = 5;
}

[System.Serializable]
public class EnemyBehaviorConfig
{
    [Header("行为类型")]
    public string type = "territorial";
    
    [Header("检测范围")]
    public float detectionRadius = 150f;
    public float aggroRadius = 180f;
    public float attackRange = 40f;
    public float patrolRadius = 100f;
    
    [Header("移动速度")]
    public float chaseSpeed = 80f;
    public float chargeSpeed = 160f;
    
    [Header("时间配置")]
    public float returnToPatrolDelay = 3000f;
    public float attackCooldown = 1500f;
    public float chargeCooldown = 5000f;
    public float chargeStunDuration = 1500f;
    
    [Header("冲锋配置")]
    public float chargeTriggerDistance = 80f;
    public float chargeMinDistance = 40f;
}

[System.Serializable]
public class EnemyTimingConfig
{
    [Header("受伤配置")]
    public float hurtFlashDuration = 100f;
    public float invulnerabilityDuration = 500f;
    
    [Header("攻击配置")]
    public float attackHitDelay = 300f;
    
    [Header("冲锋配置")]
    public float chargeDuration = 1000f;
    public float chargeStunDuration = 1500f;
    
    [Header("死亡配置")]
    public float deathFadeDuration = 2000f;
}

[System.Serializable]
public class EnemyEnhancedAnimationConfig
{
    [Header("冲锋动画")]
    public ChargeAnimationConfig charge;
    
    [Header("攻击动画")]
    public EnemyAttackAnimationConfig attack;
    
    [Header("受伤动画")]
    public HurtAnimationConfig hurt;
    
    [Header("死亡动画")]
    public DeathAnimationConfig death;
}

[System.Serializable]
public class ChargeAnimationConfig
{
    [Header("冲锋参数")]
    public float speedMultiplier = 2.0f;
    public float duration = 1500f;
    public float cooldown = 5000f;
    public float stunDuration = 1500f;
    public float frameRateMultiplier = 1.5f;
}

[System.Serializable]
public class EnemyAttackAnimationConfig
{
    [Header("关键帧")]
    public int keyFrameNumber = 6;
    
    [Header("攻击框")]
    public EnemyHitboxConfig hitbox;
    
    [Header("击退效果")]
    public KnockbackConfig knockback;
}

[System.Serializable]
public class KnockbackConfig
{
    [Header("击退参数")]
    public float force = 150f;
    public float duration = 300f;
}

[System.Serializable]
public class HurtAnimationConfig
{
    [Header("受伤参数")]
    public float invulnerabilityDuration = 500f;
    public float flashDuration = 100f;
    public float knockbackResistance = 0.7f;
}

[System.Serializable]
public class DeathAnimationConfig
{
    [Header("死亡参数")]
    public float fadeOutDuration = 1000f;
    public float bodyDisableDelay = 500f;
}

[System.Serializable]
public class EnemyStateMachine
{
    [Header("状态定义")]
    public List<string> availableStates;
    
    [Header("状态转换")]
    public List<StateTransition> transitions;
}

[System.Serializable]
public class StateTransition
{
    [Header("状态转换")]
    public string fromState;
    public List<string> toStates;
}

[System.Serializable]
public class EnemyPhysicsBodyConfig
{
    [Header("物理体配置")]
    public Vector2 size = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public bool isTrigger = false;
}

[System.Serializable]
public class EnemyHitboxConfig
{
    [Header("攻击框配置")]
    public Vector2 size = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public float duration = 0.2f;
}

[System.Serializable]
public class EnemyConfigData
{
    [Header("基础信息")]
    public string id;
    public string name;
    [TextArea(2, 4)]
    public string description;
    public string type = "normal";
    public int level = 1;
    
    [Header("属性配置")]
    public EnemyStats stats;
    
    [Header("能力配置")]
    public List<string> abilities;
    
    [Header("掉落配置")]
    public List<EnemyDropItem> drops;
    public EnemyGoldDrop goldDrop;
    
    [Header("精灵配置")]
    public string spriteKey;
    public string textureKey;
    
    [Header("尺寸配置")]
    public Vector2 standardSize;
    public Vector2 anchorPoint;
    public EnemyPhysicsBodyConfig physicsBody;
    
    [Header("行为配置")]
    public EnemyBehaviorConfig behavior;
    
    [Header("时间配置")]
    public EnemyTimingConfig timing;
    
    [Header("增强动画配置")]
    public EnemyEnhancedAnimationConfig enhancedAnimation;
    
    [Header("状态机配置")]
    public EnemyStateMachine stateMachine;
}

/// <summary>
/// 通用配置常量
/// </summary>
[System.Serializable]
public class GlobalTimingConfig
{
    [Header("通用时间配置")]
    public float hurtFlashDuration = 100f;
    public float attackHitDelay = 300f;
    public float patrolRandomChance = 0.005f;
    public float patrolWaitTime = 2000f;
    public float baseAttackCooldown = 1500f;
    public float deathFadeDuration = 2000f;
    public float bodyDisableDelay = 500f;
    public float debugDisplayDuration = 200f;
    public float stateTransitionDelay = 50f;
    public float hurtRecoveryDuration = 500f;
    public float deathAnimationDuration = 1000f;
    public float collisionDamageCooldown = 2000f;
    public float playerInvulnerabilityDuration = 500f;
}

[System.Serializable]
public class GlobalPhysicsConfig
{
    [Header("通用物理配置")]
    public float bounce = 0.1f;
    public float distanceThreshold = 10f;
    public float movementThreshold = 5f;
    public float chargeDamageMultiplier = 1.5f;
}

[System.Serializable]
public class StuckDetectionConfig
{
    [Header("卡住检测配置")]
    public float checkInterval = 2000f;
    public float stuckTimeThreshold = 5000f;
    public float minMovementDistance = 20f;
}

/// <summary>
/// 敌人配置管理器
/// 提供统一的敌人配置访问接口
/// </summary>
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game Config/Enemy Config")]
public class EnemyConfigSO : ScriptableObject
{
    [Header("全局配置")]
    public GlobalTimingConfig globalTiming;
    public GlobalPhysicsConfig globalPhysics;
    public StuckDetectionConfig stuckDetection;
    
    [Header("敌人配置列表")]
    public List<EnemyConfigData> enemyConfigs;
    
    /// <summary>
    /// 获取敌人配置
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>敌人配置</returns>
    public EnemyConfigData GetEnemyConfig(string enemyType)
    {
        return enemyConfigs?.Find(config => 
            config.id.Equals(enemyType, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取敌人行为配置
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>行为配置</returns>
    public EnemyBehaviorConfig GetBehaviorConfig(string enemyType)
    {
        var config = GetEnemyConfig(enemyType);
        return config?.behavior;
    }
    
    /// <summary>
    /// 获取敌人时间配置
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>时间配置</returns>
    public EnemyTimingConfig GetTimingConfig(string enemyType)
    {
        var config = GetEnemyConfig(enemyType);
        return config?.timing;
    }
    
    /// <summary>
    /// 获取敌人增强动画配置
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>增强动画配置</returns>
    public EnemyEnhancedAnimationConfig GetEnhancedAnimationConfig(string enemyType)
    {
        var config = GetEnemyConfig(enemyType);
        return config?.enhancedAnimation;
    }
    
    /// <summary>
    /// 获取敌人属性配置
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>属性配置</returns>
    public EnemyStats GetEnemyStats(string enemyType)
    {
        var config = GetEnemyConfig(enemyType);
        return config?.stats;
    }
    
    /// <summary>
    /// 获取敌人掉落配置
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>掉落配置</returns>
    public List<EnemyDropItem> GetDropConfig(string enemyType)
    {
        var config = GetEnemyConfig(enemyType);
        return config?.drops;
    }
    
    /// <summary>
    /// 获取所有可用敌人类型
    /// </summary>
    /// <returns>敌人类型列表</returns>
    public List<string> GetAvailableEnemyTypes()
    {
        var types = new List<string>();
        if (enemyConfigs != null)
        {
            foreach (var config in enemyConfigs)
            {
                if (!string.IsNullOrEmpty(config.id))
                {
                    types.Add(config.id);
                }
            }
        }
        return types;
    }
    
    /// <summary>
    /// 验证敌人类型是否存在
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>是否存在</returns>
    public bool IsValidEnemyType(string enemyType)
    {
        return GetEnemyConfig(enemyType) != null;
    }
}