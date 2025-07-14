using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

/// <summary>
/// 法师职业类 - 高魔攻高魔防但生命值较低的远程魔法角色
/// 从原Phaser项目的Mage.js迁移而来
/// </summary>
public class Mage : Character
{
    [Header("法师配置")]
    [SerializeField, LabelText("法师配置文件")]
    [InfoBox("配置法师的所有属性和技能参数")]
    private MageConfig config;
    
    [Header("法师特有属性")]
    public float spellCastRange = 8f;
    public float manaRegenRate = 2f;
    
    // 重写Character基类的攻击属性，提供法师特有的配置
    public override float AttackRange => config != null ? config.attackRange : 1.5f;
    public override float AttackHeight => config != null ? config.attackHeight : 1.8f;
    public override float KnockbackForce => config != null ? config.knockbackForce : 3f;

    
    protected override void DealDamageToTarget(IDamageable target, Vector2 hitPoint)
    {
        // 法师造成魔法伤害，基于魔法攻击力
        int damage = Mathf.RoundToInt(magicalAttack * 0.8f); // 使用80%的魔法攻击力
        target.TakeDamage(damage, hitPoint, this);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 法师魔法攻击造成 {damage} 点魔法伤害");
        }
    }
    
    protected override void ApplyKnockbackToTarget(Rigidbody2D targetRb, Vector2 direction)
    {
        // 法师的击退效果带有魔法特效
        Vector2 knockback = direction.normalized * KnockbackForce;
        targetRb.AddForce(knockback, ForceMode2D.Impulse);
        
        // 可以在这里添加魔法击退特效
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 法师魔法击退，力度: {KnockbackForce}");
        }
    }
    
    /// <summary>
    /// 法师基础攻击方法
    /// </summary>
    /// <returns>是否成功执行攻击</returns>
    public bool PerformBasicAttack()
    {
        // 检查攻击冷却
        if (!CanAttackNow || !isAlive)
        {
            return false;
        }
        
        // 更新上次攻击时间
        lastAttackTime = Time.time;
        
        // 执行攻击逻辑
        DetectAndDamageEnemies(() => {
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[Mage] 基础魔法攻击命中，伤害: {magicalAttack}");
            }
        });
        
        return true;
    }
    
    // 法师攻击冷却
    [LabelText("基础攻击冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    public float attackCooldown => config != null ? config.attackCooldown : 0.3f;
    
    [LabelText("上次攻击时间")]
    [ReadOnly]
    [ShowInInspector]
    private float lastAttackTime;
    
    [LabelText("攻击冷却剩余时间")]
    [ReadOnly]
    [ShowInInspector]
    private float AttackCooldownRemaining => Mathf.Max(0f, (lastAttackTime + attackCooldown) - Time.time);
    
    [LabelText("可以攻击")]
    [ReadOnly]
    [ShowInInspector]
    private bool CanAttackNow => Time.time >= lastAttackTime + attackCooldown;
    
    [Header("法师技能冷却")]
    public float fireballCooldown = 3f;
    public float lightningBoltCooldown = 5f;
    public float healCooldown = 8f;
    public float teleportCooldown = 12f;
    
    [Header("技能预制件")]
    public GameObject fireballPrefab;
    public GameObject lightningBoltPrefab;
    public GameObject healEffectPrefab;
    public GameObject teleportEffectPrefab;
    
    // 技能状态
    private bool canUseFireball = true;
    private bool canUseLightningBolt = true;
    private bool canUseHeal = true;
    private bool canUseTeleport = true;
    
    // 魔法值自动回复
    private float manaRegenTimer = 0f;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 法师特有属性设置
        strength = config != null ? config.strength : 5;
        agility = config != null ? config.agility : 10;
        stamina = config != null ? config.stamina : 8;
        intelligence = config != null ? config.intelligence : 15;
        
        // 更新法师特有属性
        spellCastRange = config != null ? config.spellCastRange : 8f;
        manaRegenRate = config != null ? config.manaRegenRate : 2f;
        
        // 重新计算衍生属性
        CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 法师创建完成 - 魔法攻击力: {magicalAttack}, 魔法防御力: {magicDefense}");
        }
    }
    
    public override void InitializeWithConfig(string characterType)
    {
        var config = ConfigManager.Instance?.GetCharacterConfig(characterType);
        if (config != null)
        {
            maxHealth = config.health;
            maxMana = config.mana;
            currentHealth = maxHealth;
            currentMana = maxMana;
        }
        
        // 应用法师特有属性
        CalculateDerivedStats();
    }
    
    public override void CalculateDerivedStats()
    {
        base.CalculateDerivedStats();
        
        // 法师额外加成
        magicalAttack += 8;     // 额外魔法攻击力
        magicDefense += 5;    // 额外魔法防御力
        maxHealth -= 10;      // 生命值较低
        maxMana += 20;        // 额外魔法值
        
        // 确保当前生命值和魔法值不超过最大值
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentMana = Mathf.Min(currentMana, maxMana);
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 处理法师特有的更新逻辑
        HandleManaRegeneration();
        HandleMageInput();
    }
    
    /// <summary>
    /// 处理魔法值自动回复
    /// </summary>
    private void HandleManaRegeneration()
    {
        if (!isAlive || currentMana >= maxMana) return;
        
        manaRegenTimer += Time.deltaTime;
        if (manaRegenTimer >= 1f) // 每秒回复
        {
            float regenRate = config != null ? config.manaRegenRate : manaRegenRate;
            RestoreMana(Mathf.RoundToInt(regenRate));
            manaRegenTimer = 0f;
        }
    }
    
    /// <summary>
    /// 处理法师特有输入
    /// </summary>
    private void HandleMageInput()
    {
        if (!canMove || !isAlive) return;
        
        // 检查技能输入
        if (InputManager.Instance != null)
        {
            // 这里可以添加法师特有的输入处理
            // 例如：火球术、闪电术、治疗术、传送术等技能的快捷键
        }
    }
    
    /// <summary>
    /// 火球术 - 发射火球造成魔法伤害
    /// </summary>
    public bool CastFireball(Vector2 targetPosition)
    {
        if (!canUseFireball || !canAttack || !isAlive) return false;
        
        int manaCost = config != null ? config.fireballManaCost : 15;
        if (currentMana < manaCost) return false;
        
        canUseFireball = false;
        canAttack = false;
        
        // 消耗魔法值
        ConsumeMana(manaCost);
        
        // 播放施法动画
        if (animator != null)
        {
            animator.SetTrigger("CastFireball");
        }
        
        // 创建火球
        StartCoroutine(CreateFireball(targetPosition));
        
        // 开始冷却
        StartCoroutine(FireballCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 施放火球术，目标位置: {targetPosition}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 闪电术 - 瞬间攻击目标造成高魔法伤害
    /// </summary>
    public bool CastLightningBolt(Transform target)
    {
        if (!canUseLightningBolt || !canAttack || !isAlive || target == null) return false;
        
        int manaCost = config != null ? config.lightningBoltManaCost : 25;
        if (currentMana < manaCost) return false;
        
        // 检查距离
        float distance = Vector2.Distance(transform.position, target.position);
        float castRange = config != null ? config.spellCastRange : spellCastRange;
        if (distance > castRange) return false;
        
        canUseLightningBolt = false;
        canAttack = false;
        
        // 消耗魔法值
        ConsumeMana(manaCost);
        
        // 播放施法动画
        if (animator != null)
        {
            animator.SetTrigger("CastLightning");
        }
        
        // 执行闪电攻击
        StartCoroutine(ExecuteLightningBolt(target));
        
        // 开始冷却
        StartCoroutine(LightningBoltCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 施放闪电术，目标: {target.name}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 治疗术 - 恢复生命值
    /// </summary>
    public bool CastHeal()
    {
        if (!canUseHeal || !isAlive) return false;
        
        int manaCost = config != null ? config.healManaCost : 20;
        if (currentMana < manaCost) return false;
        
        canUseHeal = false;
        
        // 消耗魔法值
        ConsumeMana(manaCost);
        
        // 播放施法动画
        if (animator != null)
        {
            animator.SetTrigger("CastHeal");
        }
        
        // 执行治疗
        StartCoroutine(ExecuteHeal());
        
        // 开始冷却
        StartCoroutine(HealCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Mage] 施放治疗术");
        }
        
        return true;
    }
    
    /// <summary>
    /// 传送术 - 瞬间移动到指定位置
    /// </summary>
    public bool CastTeleport(Vector2 targetPosition)
    {
        if (!canUseTeleport || !isAlive) return false;
        
        int manaCost = config != null ? config.teleportManaCost : 30;
        if (currentMana < manaCost) return false;
        
        // 检查传送距离
        float distance = Vector2.Distance(transform.position, targetPosition);
        float maxDistance = config != null ? config.teleportMaxDistance : spellCastRange;
        if (distance > maxDistance) return false;
        
        canUseTeleport = false;
        
        // 消耗魔法值
        ConsumeMana(manaCost);
        
        // 播放施法动画
        if (animator != null)
        {
            animator.SetTrigger("CastTeleport");
        }
        
        // 执行传送
        StartCoroutine(ExecuteTeleport(targetPosition));
        
        // 开始冷却
        StartCoroutine(TeleportCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 施放传送术，目标位置: {targetPosition}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 创建火球
    /// </summary>
    private System.Collections.IEnumerator CreateFireball(Vector2 targetPosition)
    {
        // 等待施法动画
        float castTime = config != null ? config.fireballCastTime : 0.5f;
        yield return new WaitForSeconds(castTime);
        
        if (fireballPrefab != null)
        {
            // 创建火球实例
            GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            
            // 设置火球属性（假设火球有Fireball组件）
            var fireballComponent = fireball.GetComponent<Fireball>();
            if (fireballComponent != null)
            {
                float damageMultiplier = config != null ? config.fireballDamageMultiplier : 1.2f;
                int damage = Mathf.RoundToInt(magicalAttack * damageMultiplier);
                fireballComponent.Initialize(targetPosition, damage, this);
            }
        }
        
        // 恢复攻击能力
        float recoveryTime = config != null ? config.normalAttackRecoveryTime : 0.2f;
        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
    }
    
    /// <summary>
    /// 执行闪电攻击
    /// </summary>
    private System.Collections.IEnumerator ExecuteLightningBolt(Transform target)
    {
        // 等待施法动画
        float castTime = config != null ? config.lightningBoltCastTime : 0.3f;
        yield return new WaitForSeconds(castTime);
        
        if (target != null)
        {
            // 创建闪电效果
            if (lightningBoltPrefab != null)
            {
                GameObject lightning = Instantiate(lightningBoltPrefab, target.position, Quaternion.identity);
                Destroy(lightning, 1f); // 1秒后销毁效果
            }
            
            // 计算伤害并应用
            float damageMultiplier = config != null ? config.lightningBoltDamageMultiplier : 1.5f;
            int damage = Mathf.RoundToInt(magicalAttack * damageMultiplier);
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        // 恢复攻击能力
        float recoveryTime = config != null ? config.normalAttackRecoveryTime : 0.2f;
        yield return new WaitForSeconds(recoveryTime);
        canAttack = true;
    }
    
    /// <summary>
    /// 执行治疗
    /// </summary>
    private System.Collections.IEnumerator ExecuteHeal()
    {
        // 等待施法动画
        float castTime = config != null ? config.healCastTime : 0.4f;
        yield return new WaitForSeconds(castTime);
        
        // 创建治疗效果
        if (healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            float effectDuration = config != null ? config.healEffectDuration : 2f;
            Destroy(healEffect, effectDuration);
        }
        
        // 计算治疗量并恢复生命值
        float healMultiplier = config != null ? config.healMultiplier : 0.8f;
        float intelligenceBonus = config != null ? config.intelligenceHealBonus : 2f;
        int healAmount = Mathf.RoundToInt(magicalAttack * healMultiplier + intelligence * intelligenceBonus);
        Heal(healAmount);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 治疗术恢复生命值: {healAmount}");
        }
    }
    
    /// <summary>
    /// 执行传送
    /// </summary>
    private System.Collections.IEnumerator ExecuteTeleport(Vector2 targetPosition)
    {
        // 播放传送离开效果
        if (teleportEffectPrefab != null)
        {
            GameObject teleportOut = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(teleportOut, 1f);
        }
        
        // 等待传送动画
        float castTime = config != null ? config.teleportCastTime : 0.5f;
        yield return new WaitForSeconds(castTime);
        
        // 执行传送
        transform.position = targetPosition;
        
        // 播放传送到达效果
        if (teleportEffectPrefab != null)
        {
            GameObject teleportIn = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(teleportIn, 1f);
        }
    }
    
    /// <summary>
    /// 火球术冷却
    /// </summary>
    private System.Collections.IEnumerator FireballCooldown()
    {
        float cooldown = config != null ? config.fireballCooldown : fireballCooldown;
        yield return new WaitForSeconds(cooldown);
        canUseFireball = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Mage] 火球术冷却完成");
        }
    }
    
    /// <summary>
    /// 闪电术冷却
    /// </summary>
    private System.Collections.IEnumerator LightningBoltCooldown()
    {
        float cooldown = config != null ? config.lightningBoltCooldown : lightningBoltCooldown;
        yield return new WaitForSeconds(cooldown);
        canUseLightningBolt = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Mage] 闪电术冷却完成");
        }
    }
    
    /// <summary>
    /// 治疗术冷却
    /// </summary>
    private System.Collections.IEnumerator HealCooldown()
    {
        float cooldown = config != null ? config.healCooldown : healCooldown;
        yield return new WaitForSeconds(cooldown);
        canUseHeal = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Mage] 治疗术冷却完成");
        }
    }
    
    /// <summary>
    /// 传送术冷却
    /// </summary>
    private System.Collections.IEnumerator TeleportCooldown()
    {
        float cooldown = config != null ? config.teleportCooldown : teleportCooldown;
        yield return new WaitForSeconds(cooldown);
        canUseTeleport = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Mage] 传送术冷却完成");
        }
    }
    
    /// <summary>
    /// 获取技能状态信息
    /// </summary>
    public MageSkillStatus GetSkillStatus()
    {
        return new MageSkillStatus
        {
            canUseFireball = this.canUseFireball,
            canUseLightningBolt = this.canUseLightningBolt,
            canUseHeal = this.canUseHeal,
            canUseTeleport = this.canUseTeleport,
            currentMana = this.currentMana,
            maxMana = this.maxMana
        };
    }
}

/// <summary>
/// 法师技能状态结构
/// </summary>
[System.Serializable]
public struct MageSkillStatus
{
    public bool canUseFireball;
    public bool canUseLightningBolt;
    public bool canUseHeal;
    public bool canUseTeleport;
    public int currentMana;
    public int maxMana;
}