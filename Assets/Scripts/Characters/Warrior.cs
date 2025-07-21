using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

/// <summary>
/// 战士职业类 - 高攻高物防但速度较慢的近战角色
/// 从原Phaser项目的Warrior.js迁移而来
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class Warrior : Character
{
    [TitleGroup("战士职业配置", "高攻高防的近战战士", TitleAlignments.Centered)]
    [FoldoutGroup("战士职业配置/职业设置", expanded: true)]
    [LabelText("战士配置文件")]
    [Required]
    [InfoBox("战士职业的所有数值配置，通过ScriptableObject管理")]
    public WarriorConfig config;
    

    

    
    [FoldoutGroup("战士职业配置/攻击冷却", expanded: false)]
    [LabelText("基础攻击冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    public float attackCooldown => config != null ? config.attackCooldown : 0.8f;
    
    [FoldoutGroup("战士职业配置/攻击冷却")]
    [LabelText("上次攻击时间")]
    [ReadOnly]
    [ShowInInspector]
    private float lastAttackTime;
    
    [FoldoutGroup("战士职业配置/攻击冷却")]
    [LabelText("攻击冷却剩余时间")]
    [ReadOnly]
    [ShowInInspector]
    private float AttackCooldownRemaining => Mathf.Max(0f, (lastAttackTime + attackCooldown) - Time.time);
    
    [FoldoutGroup("战士职业配置/攻击冷却")]
    [LabelText("可以攻击")]
    [ReadOnly]
    [ShowInInspector]
    private bool CanAttackNow => Time.time >= lastAttackTime + attackCooldown;
    
    [TitleGroup("战士实时状态", "当前技能状态和效果", TitleAlignments.Centered)]

    

    

    


    
    // 重写Character基类的攻击属性，提供战士特有的配置
    public override float AttackRange => config != null ? config.attackRange : 2.5f;        // 战士攻击范围稍大
    public override float AttackHeight => config != null ? config.attackHeight : 2.5f;       // 战士攻击高度稍大
    public override float KnockbackForce => config != null ? config.knockbackForce : 8f;       // 战士击退力度更强

    protected override void Awake()
    {
        base.Awake();
        playerType = "warrior";
        // 战士特有属性设置
        strength = config != null ? config.strength : 15;      // 高力量
        agility = config != null ? config.agility : 8;        // 较低敏捷
        stamina = config != null ? config.stamina : 12;       // 高体力
        intelligence = config != null ? config.intelligence : 5;   // 低智力
        maxHealth = config != null ? config.maxHealth : 100;
        maxMana = config != null ? config.maxMana : 100;
        // 重新计算衍生属性
        CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Warrior] 战士创建完成 - 攻击力: {physicalAttack}, 防御力: {defense}");
        }
    }
    
    public override void CalculateDerivedStats()
    {
        base.CalculateDerivedStats();
        
        // 战士额外加成
        physicalAttack += 5;  // 额外物理攻击力
        defense += 3;         // 额外防御力
        speed -= 0.5f;        // 速度略慢
    }
    
    protected override void Update()
    {
        base.Update();
        
    }

    
    /// <summary>
    /// 重写Character基类的DealDamageToTarget方法，战士造成更高伤害
    /// </summary>
    /// <param name="target">目标碰撞体</param>
    public override  void DealDamageToTarget(IDamageable target, Vector2 hitPoint)
    { if (target is Enemy enemy) 
    {
        int damage = Mathf.RoundToInt(physicalAttack * 1.1f);
        enemy.TakeDamage(damage);
        Debug.Log($"战士攻击造成伤害: {damage}");
    }
        // var enemy = target.GetComponent<Enemy>();
        // if (enemy != null)
        // {
        //     // 战士造成额外10%伤害
        //     int damage = Mathf.RoundToInt(physicalAttack * 1.1f);
        //     enemy.TakePlayerDamage(damage);
        //     Debug.Log($"战士攻击造成伤害: {damage} (基础伤害: {physicalAttack})");
        // }
    }
    
    /// <summary>
    /// 战士基础攻击方法
    /// </summary>
    /// <returns>是否成功执行攻击</returns>
    public bool PerformBasicAttack()
    {
        // 检查攻击冷却|| isUsingSkill
        if (!CanAttackNow || !isAlive )
        {
            return false;
        }
        
        // 更新上次攻击时间
        lastAttackTime = Time.time;
        
        
        return true;
    }
    
    /// <summary>
    /// 重写Character基类的ApplyKnockbackToTarget方法，战士击退效果更强
    /// </summary>
    /// <param name="target">被击退的目标</param>
    public void ApplyKnockbackToTarget(Collider2D target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // 计算击退方向（从角色指向目标）
            Vector2 knockbackDirection = (target.transform.position - transform.position).normalized;
            
            // 战士的击退力度更强，并且有向上的分量
            Vector2 knockbackForce = knockbackDirection * KnockbackForce + Vector2.up * 2f;
            targetRb.AddForce(knockbackForce, ForceMode2D.Impulse);
            
            Debug.Log($"战士对 {target.name} 应用强力击退: {KnockbackForce}");
        }
    }



    

    

    

    

    

    

    

    

    


    
 


    

    

    

    

    

}