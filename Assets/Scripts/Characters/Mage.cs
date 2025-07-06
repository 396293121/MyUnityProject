using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 法师职业类 - 高魔攻高魔防但生命值较低的远程魔法角色
/// 从原Phaser项目的Mage.js迁移而来
/// </summary>
public class Mage : Character
{
    [Header("法师特有属性")]
    public float spellCastRange = 8f;
    public float manaRegenRate = 2f;
    
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
        strength = 5;       // 低力量
        agility = 10;       // 中等敏捷
        stamina = 8;        // 较低体力
        intelligence = 15;  // 高智力
        
        // 重新计算衍生属性
        CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Mage] 法师创建完成 - 魔法攻击力: {magicalAttack}, 魔法防御力: {magicDefense}");
        }
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
            RestoreMana(Mathf.RoundToInt(manaRegenRate));
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
        
        int manaCost = 15;
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
        
        int manaCost = 25;
        if (currentMana < manaCost) return false;
        
        // 检查距离
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > spellCastRange) return false;
        
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
        
        int manaCost = 20;
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
        
        int manaCost = 30;
        if (currentMana < manaCost) return false;
        
        // 检查传送距离
        float distance = Vector2.Distance(transform.position, targetPosition);
        if (distance > spellCastRange) return false;
        
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
        yield return new WaitForSeconds(0.5f);
        
        if (fireballPrefab != null)
        {
            // 创建火球实例
            GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            
            // 设置火球属性（假设火球有Fireball组件）
            var fireballComponent = fireball.GetComponent<Fireball>();
            if (fireballComponent != null)
            {
                int damage = Mathf.RoundToInt(magicalAttack * 1.2f);
                fireballComponent.Initialize(targetPosition, damage, this);
            }
        }
        
        // 恢复攻击能力
        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }
    
    /// <summary>
    /// 执行闪电攻击
    /// </summary>
    private System.Collections.IEnumerator ExecuteLightningBolt(Transform target)
    {
        // 等待施法动画
        yield return new WaitForSeconds(0.3f);
        
        if (target != null)
        {
            // 创建闪电效果
            if (lightningBoltPrefab != null)
            {
                GameObject lightning = Instantiate(lightningBoltPrefab, target.position, Quaternion.identity);
                Destroy(lightning, 1f); // 1秒后销毁效果
            }
            
            // 计算伤害并应用
            int damage = Mathf.RoundToInt(magicalAttack * 1.5f);
            var enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        // 恢复攻击能力
        yield return new WaitForSeconds(0.2f);
        canAttack = true;
    }
    
    /// <summary>
    /// 执行治疗
    /// </summary>
    private System.Collections.IEnumerator ExecuteHeal()
    {
        // 等待施法动画
        yield return new WaitForSeconds(0.4f);
        
        // 创建治疗效果
        if (healEffectPrefab != null)
        {
            GameObject healEffect = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            Destroy(healEffect, 2f); // 2秒后销毁效果
        }
        
        // 计算治疗量并恢复生命值
        int healAmount = Mathf.RoundToInt(magicalAttack * 0.8f + intelligence * 2);
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
        yield return new WaitForSeconds(0.5f);
        
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
        yield return new WaitForSeconds(fireballCooldown);
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
        yield return new WaitForSeconds(lightningBoltCooldown);
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
        yield return new WaitForSeconds(healCooldown);
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
        yield return new WaitForSeconds(teleportCooldown);
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