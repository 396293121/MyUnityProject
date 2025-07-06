using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 战士职业类 - 高攻高物防但速度较慢的近战角色
/// 从原Phaser项目的Warrior.js迁移而来
/// </summary>
public class Warrior : Character
{
    [Header("战士特有属性")]
    public float chargeSpeed = 10f;
    public float chargeDuration = 1f;
    public float chargeDistance = 5f;
    
    [Header("战士技能冷却")]
    public float heavySlashCooldown = 5f;
    public float whirlwindCooldown = 8f;
    public float battleCryCooldown = 15f;
    
    // 技能状态
    private bool canUseHeavySlash = true;
    private bool canUseWhirlwind = true;
    private bool canUseBattleCry = true;
    
    // 战吼效果
    private bool battleCryActive = false;
    private float originalAttack;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 战士特有属性设置
        strength = 15;      // 高力量
        agility = 8;        // 较低敏捷
        stamina = 12;       // 高体力
        intelligence = 5;   // 低智力
        
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
        
        // 处理战士特有的输入
        HandleWarriorInput();
    }
    
    /// <summary>
    /// 处理战士特有输入
    /// </summary>
    private void HandleWarriorInput()
    {
        if (!canMove || !isAlive) return;
        
        // 检查技能输入
        if (InputManager.Instance != null)
        {
            // 这里可以添加战士特有的输入处理
            // 例如：重斩、旋风斩、战吼等技能的快捷键
        }
    }
    
    /// <summary>
    /// 重斩技能 - 造成150%攻击力的强力一击
    /// </summary>
    public bool PerformHeavySlash()
    {
        if (!canUseHeavySlash || !canAttack || !isAlive) return false;
        
        canUseHeavySlash = false;
        canAttack = false;
        
        // 播放重斩动画
        if (animator != null)
        {
            animator.SetTrigger("HeavySlash");
        }
        
        // 计算伤害
        int damage = Mathf.RoundToInt(physicalAttack * 1.5f);
        
        // 执行攻击判定（需要在动画的关键帧调用）
        StartCoroutine(ExecuteHeavySlashAttack(damage));
        
        // 开始冷却
        StartCoroutine(HeavySlashCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Warrior] 使用重斩技能，伤害: {damage}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 旋风斩技能 - 对周围敌人造成120%攻击力的范围伤害
    /// </summary>
    public bool PerformWhirlwind()
    {
        if (!canUseWhirlwind || !canAttack || !isAlive) return false;
        
        canUseWhirlwind = false;
        canAttack = false;
        
        // 播放旋风斩动画
        if (animator != null)
        {
            animator.SetTrigger("Whirlwind");
        }
        
        // 计算伤害
        int damage = Mathf.RoundToInt(physicalAttack * 1.2f);
        
        // 执行范围攻击判定
        StartCoroutine(ExecuteWhirlwindAttack(damage));
        
        // 开始冷却
        StartCoroutine(WhirlwindCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Warrior] 使用旋风斩技能，范围伤害: {damage}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 战吼技能 - 提高20%攻击力，持续10秒
    /// </summary>
    public bool PerformBattleCry()
    {
        if (!canUseBattleCry || !isAlive || battleCryActive) return false;
        
        canUseBattleCry = false;
        
        // 播放战吼动画
        if (animator != null)
        {
            animator.SetTrigger("BattleCry");
        }
        
        // 应用战吼效果
        StartCoroutine(ApplyBattleCryEffect());
        
        // 开始冷却
        StartCoroutine(BattleCryCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Warrior] 使用战吼技能，攻击力提升20%");
        }
        
        return true;
    }
    
    /// <summary>
    /// 执行重斩攻击判定
    /// </summary>
    private System.Collections.IEnumerator ExecuteHeavySlashAttack(int damage)
    {
        // 等待动画到达关键帧（通常是动画的50%处）
        yield return new WaitForSeconds(0.3f);
        
        // 创建攻击范围检测
        Vector2 attackPosition = (Vector2)transform.position + (spriteRenderer.flipX ? Vector2.left : Vector2.right) * 1.5f;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPosition, 1.2f, LayerMask.GetMask("Enemy"));
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // 对敌人造成伤害
            var enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(damage);
            }
        }
        
        // 恢复攻击能力
        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }
    
    /// <summary>
    /// 执行旋风斩攻击判定
    /// </summary>
    private System.Collections.IEnumerator ExecuteWhirlwindAttack(int damage)
    {
        // 等待动画到达关键帧
        yield return new WaitForSeconds(0.4f);
        
        // 创建范围攻击检测
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.GetMask("Enemy"));
        
        foreach (Collider2D enemy in hitEnemies)
        {
            // 对敌人造成伤害
            var enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(damage);
            }
        }
        
        // 恢复攻击能力
        yield return new WaitForSeconds(0.3f);
        canAttack = true;
    }
    
    /// <summary>
    /// 应用战吼效果
    /// </summary>
    private System.Collections.IEnumerator ApplyBattleCryEffect()
    {
        battleCryActive = true;
        originalAttack = physicalAttack;
        
        // 提升攻击力20%
        physicalAttack = Mathf.RoundToInt(originalAttack * 1.2f);
        
        // 持续10秒
        yield return new WaitForSeconds(10f);
        
        // 恢复原始攻击力
        physicalAttack = Mathf.RoundToInt(originalAttack);
        battleCryActive = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Warrior] 战吼效果结束");
        }
    }
    
    /// <summary>
    /// 重斩技能冷却
    /// </summary>
    private System.Collections.IEnumerator HeavySlashCooldown()
    {
        yield return new WaitForSeconds(heavySlashCooldown);
        canUseHeavySlash = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Warrior] 重斩技能冷却完成");
        }
    }
    
    /// <summary>
    /// 旋风斩技能冷却
    /// </summary>
    private System.Collections.IEnumerator WhirlwindCooldown()
    {
        yield return new WaitForSeconds(whirlwindCooldown);
        canUseWhirlwind = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Warrior] 旋风斩技能冷却完成");
        }
    }
    
    /// <summary>
    /// 战吼技能冷却
    /// </summary>
    private System.Collections.IEnumerator BattleCryCooldown()
    {
        yield return new WaitForSeconds(battleCryCooldown);
        canUseBattleCry = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Warrior] 战吼技能冷却完成");
        }
    }
    
    /// <summary>
    /// 获取技能状态信息
    /// </summary>
    public WarriorSkillStatus GetSkillStatus()
    {
        return new WarriorSkillStatus
        {
            canUseHeavySlash = this.canUseHeavySlash,
            canUseWhirlwind = this.canUseWhirlwind,
            canUseBattleCry = this.canUseBattleCry,
            battleCryActive = this.battleCryActive
        };
    }
}

/// <summary>
/// 战士技能状态结构
/// </summary>
[System.Serializable]
public struct WarriorSkillStatus
{
    public bool canUseHeavySlash;
    public bool canUseWhirlwind;
    public bool canUseBattleCry;
    public bool battleCryActive;
}