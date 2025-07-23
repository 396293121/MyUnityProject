using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

/// <summary>
/// 射手职业类 - 高敏捷高速度的远程物理攻击角色
/// 从原Phaser项目的Archer.js迁移而来
/// </summary>
public class Archer : Character
{
    [TitleGroup("射手职业配置", "远程攻击的敏捷射手", TitleAlignments.Centered)]
    [FoldoutGroup("射手职业配置/职业设置", expanded: true)]
    [LabelText("射手配置文件")]
    [Required]
    [InfoBox("射手职业的所有数值配置，通过ScriptableObject管理")]
    public ArcherConfig config;
    
    // 射手特有属性（只读，从配置文件获取）
    [FoldoutGroup("射手职业配置/射击属性", expanded: false)]
    [HorizontalGroup("射手职业配置/射击属性/参数")]
    [VerticalGroup("射手职业配置/射击属性/参数/左列")]
    [LabelText("射击范围")]
    [ReadOnly]
    [ShowInInspector]
    public float shootRange => config != null ? config.shootRange : 10f;
    
    [VerticalGroup("射手职业配置/射击属性/参数/左列")]
    [LabelText("箭矢速度")]
    [ReadOnly]
    [ShowInInspector]
    public float arrowSpeed => config != null ? config.arrowSpeed : 15f;
    
    [VerticalGroup("射手职业配置/射击属性/参数/右列")]
    [LabelText("最大箭矢数")]
    [ReadOnly]
    [ShowInInspector]
    public int maxArrows => config != null ? config.maxArrows : 30;
    
    [VerticalGroup("射手职业配置/射击属性/参数/右列")]
    [LabelText("装填时间")]
    [ReadOnly]
    [ShowInInspector]
    public float reloadTime => config != null ? config.reloadTime : 2f;
    
    // 重写Character基类的攻击属性，提供射手特有的配置
    public override float AttackRange => config != null ? config.attackRange : 2f;
    public override float AttackHeight => config != null ? config.attackHeight : 2f;
    public override float KnockbackForce => config != null ? config.knockbackForce : 5f;
    

    
    public override void DealDamageToTarget(IDamageable target, Vector2 hitPoint)
    {
        // 射手造成物理伤害，基于敏捷度
        int damage = Mathf.RoundToInt(physicalAttack * 0.9f); // 使用90%的物理攻击力
        target.TakeDamage(damage, DamageType.Physical, hitPoint, this);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 射手精准攻击造成 {damage} 点物理伤害");
        }
    }
    
    protected override void ApplyKnockbackToTarget(Rigidbody2D targetRb, Vector2 direction)
    {
        // 射手的击退效果更加精准，主要是水平方向
        Vector2 knockback = new Vector2(direction.normalized.x * KnockbackForce, direction.normalized.y * KnockbackForce * 0.5f);
        targetRb.AddForce(knockback, ForceMode2D.Impulse);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 射手精准击退，力度: {KnockbackForce}");
        }
    }
    
    [FoldoutGroup("射手职业配置/攻击冷却", expanded: false)]
    [LabelText("基础攻击冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    public float attackCooldown => config != null ? config.attackCooldown : 0.4f;
    
    [FoldoutGroup("射手职业配置/攻击冷却")]
    [LabelText("上次攻击时间")]
    [ReadOnly]
    [ShowInInspector]
    private float lastAttackTime;
    
    [FoldoutGroup("射手职业配置/攻击冷却")]
    [LabelText("攻击冷却剩余时间")]
    [ReadOnly]
    [ShowInInspector]
    private float AttackCooldownRemaining => Mathf.Max(0f, (lastAttackTime + attackCooldown) - Time.time);
    
    [FoldoutGroup("射手职业配置/攻击冷却")]
    [LabelText("可以攻击")]
    [ReadOnly]
    [ShowInInspector]
    private bool CanAttackNow => Time.time >= lastAttackTime + attackCooldown;
    
    /// <summary>
    /// 射手基础攻击方法
    /// </summary>
    /// <returns>是否成功执行攻击</returns>
    public bool PerformBasicAttack()
    {
        // 检查攻击冷却和箭矢数量
        if (!CanAttackNow || !isAlive || currentArrows <= 0 || isReloading)
        {
            return false;
        }
        
        // 更新上次攻击时间
        lastAttackTime = Time.time;
        
        // 消耗箭矢
        currentArrows--;
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(nameof(ReloadArrows));
        }
        
        return true;
    }
    
    /// <summary>
    /// 装填箭矢
    /// </summary>
    private System.Collections.IEnumerator ReloadArrows()
    {
        isReloading = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 开始装填箭矢，装填时间: {reloadTime}秒");
        }
        
        yield return new WaitForSeconds(reloadTime);
        
        currentArrows = maxArrows;
        isReloading = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 箭矢装填完成，当前箭矢: {currentArrows}");
        }
    }
    
    [FoldoutGroup("射手职业配置/技能冷却", expanded: false)]
    [HorizontalGroup("射手职业配置/技能冷却/时间")]
    [VerticalGroup("射手职业配置/技能冷却/时间/左列")]
    [LabelText("多重射击冷却")]
    [ReadOnly]
    [ShowInInspector]
    public float multiShotCooldown => config != null ? config.multiShotCooldown : 6f;
    
    [VerticalGroup("射手职业配置/技能冷却/时间/左列")]
    [LabelText("穿透射击冷却")]
    [ReadOnly]
    [ShowInInspector]
    public float piercingShotCooldown => config != null ? config.piercingShotCooldown : 8f;
    
    [VerticalGroup("射手职业配置/技能冷却/时间/右列")]
    [LabelText("快速射击冷却")]
    [ReadOnly]
    [ShowInInspector]
    public float rapidFireCooldown => config != null ? config.rapidFireCooldown : 12f;
    
    [VerticalGroup("射手职业配置/技能冷却/时间/右列")]
    [LabelText("爆炸箭冷却")]
    [ReadOnly]
    [ShowInInspector]
    public float explosiveArrowCooldown => config != null ? config.explosiveArrowCooldown : 15f;
    
    [FoldoutGroup("射手职业配置/技能预制件", expanded: false)]
    [HorizontalGroup("射手职业配置/技能预制件/箭矢")]
    [VerticalGroup("射手职业配置/技能预制件/箭矢/左列")]
    [LabelText("普通箭矢预制件")]
    [Required]
    [InfoBox("普通射击使用的箭矢预制件")]
    public GameObject arrowPrefab;
    
    [VerticalGroup("射手职业配置/技能预制件/箭矢/右列")]
    [LabelText("穿透箭矢预制件")]
    [Required]
    [InfoBox("穿透射击使用的箭矢预制件")]
    public GameObject piercingArrowPrefab;
    
    [FoldoutGroup("射手职业配置/技能预制件")]
    [LabelText("爆炸箭矢预制件")]
    [Required]
    [InfoBox("爆炸箭技能使用的箭矢预制件")]
    public GameObject explosiveArrowPrefab;
    
    // 箭矢系统
    private int currentArrows;
    private bool isReloading = false;
    
    // 技能状态
    private bool canUseMultiShot = true;
    private bool canUsePiercingShot = true;
    private bool canUseRapidFire = true;
    private bool canUseExplosiveArrow = true;
    
    // 快速射击状态
    private bool rapidFireActive = false;
    private float originalAttackSpeed;
    
    protected override void Awake()
    {
        base.Awake();
        characterClass=CharacterClass.Archer;
        // 从配置文件设置射手特有属性
        if (config != null)
        {
            strength = config.strength;
            agility = config.agility;
            stamina = config.stamina;
            intelligence = config.intelligence;
        }
        else
        {
            // 默认值作为后备
            strength = 10;      // 中等力量
            agility = 15;       // 高敏捷
            stamina = 10;       // 中等体力
            intelligence = 8;   // 较低智力
        }
        
        // 初始化箭矢数量
        currentArrows = maxArrows;
        
        // 重新计算衍生属性
        CalculateDerivedStats();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 射手创建完成 - 攻击力: {physicalAttack}, 速度: {speed}, 箭矢: {currentArrows}/{maxArrows}");
        }
    }
    
    public override void InitializeWithConfig(string characterType)
    {
        // var config = ConfigManager.Instance?.GetCharacterConfig(characterType);
        // if (config != null)
        // {
        //     maxHealth = config.health;
        //     maxMana = config.mana;
        //     currentHealth = maxHealth;
        //     currentMana = maxMana;
        // }
        
        // 应用射手特有属性
        CalculateDerivedStats();
    }
    
    public override void CalculateDerivedStats()
    {
        base.CalculateDerivedStats();
        
        // 射手额外加成
        physicalAttack += 3;  // 额外物理攻击力
        speed += 1f;          // 额外速度
        defense -= 2;         // 防御力较低
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 处理射手特有的更新逻辑
        HandleArcherInput();
    }
    
    /// <summary>
    /// 处理射手特有输入
    /// </summary>
    private void HandleArcherInput()
    {
        if ( !isAlive) return;
        
        // 检查技能输入
        if (InputManager.Instance != null)
        {
            // 这里可以添加射手特有的输入处理
            // 例如：多重射击、穿透射击、快速射击、爆炸箭等技能的快捷键
        }
    }
    
    /// <summary>
    /// 普通射击
    /// </summary>
    public bool Shoot(Vector2 targetPosition)
    {
        if (  isReloading || currentArrows <= 0) return false;
        
        currentArrows--;
        
        // 播放射击动画
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        // 创建箭矢
        StartCoroutine(CreateArrow(targetPosition, arrowPrefab, physicalAttack));
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(nameof(ReloadArrows));
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 射击，剩余箭矢: {currentArrows}/{maxArrows}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 多重射击 - 同时射出3支箭
    /// </summary>
    public bool PerformMultiShot(Vector2 targetPosition)
    {
        if (!canUseMultiShot ||  !isAlive || isReloading || currentArrows < 3) return false;
        
        canUseMultiShot = false;
        currentArrows -= 3;
        
        // 播放多重射击动画
        if (animator != null)
        {
            animator.SetTrigger("MultiShot");
        }
        
        // 创建多支箭矢
        StartCoroutine(CreateMultipleArrows(targetPosition));
        
        // 开始冷却
        StartCoroutine(MultiShotCooldown());
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(nameof(ReloadArrows));
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 多重射击，剩余箭矢: {currentArrows}/{maxArrows}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 穿透射击 - 射出能穿透敌人的箭矢
    /// </summary>
    public bool PerformPiercingShot(Vector2 targetPosition)
    {
        if (!canUsePiercingShot || !isAlive || isReloading || currentArrows <= 0) return false;
        
        canUsePiercingShot = false;
        currentArrows--;
        
        // 播放穿透射击动画
        if (animator != null)
        {
            animator.SetTrigger("PiercingShot");
        }
        
        // 创建穿透箭矢
        float damageMultiplier = config != null ? config.piercingShotDamageMultiplier : 1.3f;
        int damage = Mathf.RoundToInt(physicalAttack * damageMultiplier);
        StartCoroutine(CreateArrow(targetPosition, piercingArrowPrefab, damage));
        
        // 开始冷却
        StartCoroutine(PiercingShotCooldown());
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(nameof(ReloadArrows));
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 穿透射击，伤害: {damage}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 快速射击 - 提高攻击速度，持续8秒
    /// </summary>
    public bool PerformRapidFire()
    {
        if (!canUseRapidFire || !isAlive || rapidFireActive) return false;
        
        canUseRapidFire = false;
        
        // 播放快速射击动画
        if (animator != null)
        {
            animator.SetTrigger("RapidFire");
        }
        
        // 应用快速射击效果
        StartCoroutine(ApplyRapidFireEffect());
        
        // 开始冷却
        StartCoroutine(RapidFireCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 激活快速射击模式");
        }
        
        return true;
    }
    
    /// <summary>
    /// 爆炸箭 - 射出造成范围伤害的箭矢
    /// </summary>
    public bool PerformExplosiveArrow(Vector2 targetPosition)
    {
        if (!canUseExplosiveArrow || !isAlive || isReloading || currentArrows <= 0) return false;
        
        canUseExplosiveArrow = false;
        currentArrows--;
        
        // 播放爆炸箭动画
        if (animator != null)
        {
            animator.SetTrigger("ExplosiveArrow");
        }
        
        // 创建爆炸箭矢
        float damageMultiplier = config != null ? config.explosiveArrowDamageMultiplier : 1.5f;
        int damage = Mathf.RoundToInt(physicalAttack * damageMultiplier);
        StartCoroutine(CreateArrow(targetPosition, explosiveArrowPrefab, damage));
        
        // 开始冷却
        StartCoroutine(ExplosiveArrowCooldown());
        
        // 检查是否需要装填
        if (currentArrows <= 0)
        {
            StartCoroutine(nameof(ReloadArrows));
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Archer] 爆炸箭，伤害: {damage}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 手动装填箭矢
    /// </summary>
    public bool ManualReload()
    {
        if (isReloading || currentArrows >= maxArrows) return false;
        
StartCoroutine(nameof(ReloadArrows));
        return true;
    }
    
    /// <summary>
    /// 创建箭矢
    /// </summary>
    private System.Collections.IEnumerator CreateArrow(Vector2 targetPosition, GameObject arrowPrefab, int damage)
    {
        // 等待射击动画
        float animationTime = config != null ? config.normalShootAnimationTime : 0.2f;
        yield return new WaitForSeconds(animationTime);
        
        if (arrowPrefab != null)
        {
            // 创建箭矢实例
            GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            
            // 设置箭矢属性（假设箭矢有Arrow组件）
        
        }
        
        // 恢复攻击能力（考虑快速射击效果）
        float rapidFireDelay = config != null ? config.rapidFireAttackDelay : 0.1f;
        float normalDelay = config != null ? config.normalAttackRecoveryTime : 0.3f;
        float attackDelay = rapidFireActive ? rapidFireDelay : normalDelay;
        yield return new WaitForSeconds(attackDelay);
    }
    
    /// <summary>
    /// 创建多支箭矢
    /// </summary>
    private System.Collections.IEnumerator CreateMultipleArrows(Vector2 targetPosition)
    {
        // 等待射击动画
        float animationTime = config != null ? config.multiShotAnimationTime : 0.3f;
        yield return new WaitForSeconds(animationTime);
        
        if (arrowPrefab != null)
        {
            // 计算方向和箭矢数量
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int arrowCount = config != null ? config.multiShotArrowCount : 3;
            float angleDiff = config != null ? config.multiShotAngleDiff : 15f;
            
            // 创建多支箭矢，角度稍有不同
            int halfCount = arrowCount / 2;
            for (int i = -halfCount; i <= halfCount; i++)
            {
                if (arrowCount % 2 == 0 && i == 0) continue; // 偶数箭矢时跳过中心
                
                float arrowAngle = angle + (i * angleDiff);
                Vector2 arrowDirection = new Vector2(Mathf.Cos(arrowAngle * Mathf.Deg2Rad), Mathf.Sin(arrowAngle * Mathf.Deg2Rad));
                Vector2 arrowTarget = (Vector2)transform.position + arrowDirection * shootRange;
                
                GameObject arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            
            }
        }
        
        // 恢复攻击能力
        float recoveryTime = config != null ? config.multiShotRecoveryTime : 0.4f;
        yield return new WaitForSeconds(recoveryTime);
    }
    
    /// <summary>
    /// 装填箭矢
    /// </summary>
    private System.Collections.IEnumerator DoReloadArrows()
    {
        isReloading = true;
        
        // 播放装填动画
        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 开始装填箭矢...");
        }
        
        // 等待装填时间
        yield return new WaitForSeconds(reloadTime);
        
        // 恢复箭矢
        currentArrows = maxArrows;
        isReloading = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 箭矢装填完成");
        }
    }
    
    /// <summary>
    /// 应用快速射击效果
    /// </summary>
    private System.Collections.IEnumerator ApplyRapidFireEffect()
    {
        rapidFireActive = true;
        
        // 提高动画播放速度
        if (animator != null)
        {
            animator.speed = 1.5f;
        }
        
        // 持续时间从配置获取
        float duration = config != null ? config.rapidFireDuration : 8f;
        yield return new WaitForSeconds(duration);
        
        // 恢复正常速度
        if (animator != null)
        {
            animator.speed = 1f;
        }
        
        rapidFireActive = false;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 快速射击效果结束");
        }
    }
    
    /// <summary>
    /// 多重射击冷却
    /// </summary>
    private System.Collections.IEnumerator MultiShotCooldown()
    {
        yield return new WaitForSeconds(multiShotCooldown);
        canUseMultiShot = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 多重射击冷却完成");
        }
    }
    
    /// <summary>
    /// 穿透射击冷却
    /// </summary>
    private System.Collections.IEnumerator PiercingShotCooldown()
    {
        yield return new WaitForSeconds(piercingShotCooldown);
        canUsePiercingShot = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 穿透射击冷却完成");
        }
    }
    
    /// <summary>
    /// 快速射击冷却
    /// </summary>
    private System.Collections.IEnumerator RapidFireCooldown()
    {
        yield return new WaitForSeconds(rapidFireCooldown);
        canUseRapidFire = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 快速射击冷却完成");
        }
    }
    
    /// <summary>
    /// 爆炸箭冷却
    /// </summary>
    private System.Collections.IEnumerator ExplosiveArrowCooldown()
    {
        yield return new WaitForSeconds(explosiveArrowCooldown);
        canUseExplosiveArrow = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[Archer] 爆炸箭冷却完成");
        }
    }
    
    /// <summary>
    /// 获取技能状态信息
    /// </summary>
    public ArcherSkillStatus GetSkillStatus()
    {
        return new ArcherSkillStatus
        {
            canUseMultiShot = this.canUseMultiShot,
            canUsePiercingShot = this.canUsePiercingShot,
            canUseRapidFire = this.canUseRapidFire,
            canUseExplosiveArrow = this.canUseExplosiveArrow,
            currentArrows = this.currentArrows,
            maxArrows = this.maxArrows,
            isReloading = this.isReloading,
            rapidFireActive = this.rapidFireActive
        };
    }
}

/// <summary>
/// 射手技能状态结构
/// </summary>
[System.Serializable]
public struct ArcherSkillStatus
{
    public bool canUseMultiShot;
    public bool canUsePiercingShot;
    public bool canUseRapidFire;
    public bool canUseExplosiveArrow;
    public int currentArrows;
    public int maxArrows;
    public bool isReloading;
    public bool rapidFireActive;
}