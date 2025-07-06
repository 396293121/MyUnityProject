using UnityEngine;
using System.Collections;

/// <summary>
/// 野猪敌人类 - 具有冲撞攻击能力的近战敌人
/// 从原Phaser项目的WildBoar.js迁移而来
/// </summary>
public class WildBoar : Enemy
{
    [Header("野猪特有属性")]
    public float chargeSpeed = 8f;
    public float chargeDuration = 2f;
    public float chargeDistance = 6f;
    public float chargeCooldown = 5f;
    public int chargeDamage = 25;
    
    [Header("野猪行为")]
    public float enrageHealthThreshold = 0.3f; // 30%血量以下进入狂暴状态
    public float enrageSpeedMultiplier = 1.5f;
    public float enrageDamageMultiplier = 1.3f;
    
    // 冲撞状态
    private bool isCharging = false;
    private bool canCharge = true;
    private bool isEnraged = false;
    private Vector2 chargeDirection;
    private float chargeTimer = 0f;
    private float lastChargeTime = 0f;
    
    // 原始属性（用于狂暴状态恢复）
    private float originalSpeed;
    private int originalAttack;
    
    public override void Awake()
    {
        base.Awake();
        
        // 野猪特有属性设置
        maxHealth = 120;
        currentHealth = maxHealth;
        attackDamage = 15;
        moveSpeed = 2.5f;
        
        // 保存原始属性
        originalSpeed = moveSpeed;
        originalAttack = (int)attackDamage;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] 野猪创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {physicalAttack}");
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 处理冲撞逻辑
        HandleChargeLogic();
        
        // 检查狂暴状态
        CheckEnrageState();
    }
    
    /// <summary>
    /// 处理冲撞逻辑
    /// </summary>
    private void HandleChargeLogic()
    {
        if (IsDead) return;
        
        if (isCharging)
        {
            // 执行冲撞移动
            ExecuteCharge();
        }
        else
        {
            // 检查是否可以开始冲撞
            CheckChargeCondition();
        }
    }
    
    /// <summary>
    /// 检查冲撞条件
    /// </summary>
    private void CheckChargeCondition()
    {
        if (!canCharge || player == null || currentState != EnemyState.Chase) return;
        
        // 检查冷却时间
        if (Time.time - lastChargeTime < chargeCooldown) return;
        
        // 检查距离
        float distanceToTarget = Vector2.Distance(transform.position, player.position);
        if (distanceToTarget >= 3f && distanceToTarget <= chargeDistance)
        {
            StartCharge();
        }
    }
    
    /// <summary>
    /// 开始冲撞
    /// </summary>
    private void StartCharge()
    {
        if (!canAttack || player == null) return;
        
        isCharging = true;
        canCharge = false;
        chargeTimer = 0f;
        lastChargeTime = Time.time;
        
        // 计算冲撞方向
        chargeDirection = (player.position - transform.position).normalized;
        
        // 播放冲撞动画
        if (animator != null)
        {
            animator.SetTrigger("Charge");
            animator.SetBool("IsCharging", true);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 开始冲撞攻击");
        }
    }
    
    /// <summary>
    /// 执行冲撞
    /// </summary>
    private void ExecuteCharge()
    {
        chargeTimer += Time.deltaTime;
        
        if (chargeTimer >= chargeDuration)
        {
            // 冲撞结束
            EndCharge();
            return;
        }
        
        // 冲撞移动
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = chargeDirection * chargeSpeed;
        }
        
        // 翻转精灵
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = chargeDirection.x < 0;
        }
    }
    
    /// <summary>
    /// 结束冲撞
    /// </summary>
    private void EndCharge()
    {
        isCharging = false;
        
        // 停止移动
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 更新动画
        if (animator != null)
        {
            animator.SetBool("IsCharging", false);
        }
        
        // 开始冷却
        StartCoroutine(ChargeCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 冲撞结束");
        }
    }
    
    /// <summary>
    /// 冲撞冷却
    /// </summary>
    private IEnumerator ChargeCooldown()
    {
        yield return new WaitForSeconds(chargeCooldown);
        canCharge = true;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 冲撞冷却完成");
        }
    }
    
    /// <summary>
    /// 检查狂暴状态
    /// </summary>
    private void CheckEnrageState()
    {
        if (IsDead) return;
        
        float healthPercentage = (float)CurrentHealth / MaxHealth;
        
        if (!isEnraged && healthPercentage <= enrageHealthThreshold)
        {
            EnterEnrageState();
        }
    }
    
    /// <summary>
    /// 进入狂暴状态
    /// </summary>
    private void EnterEnrageState()
    {
        isEnraged = true;
        
        // 提升属性
        moveSpeed = originalSpeed * enrageSpeedMultiplier;
        attackDamage = Mathf.RoundToInt(originalAttack * enrageDamageMultiplier);
        
        // 播放狂暴动画
        if (animator != null)
        {
            animator.SetTrigger("Enrage");
            animator.SetBool("IsEnraged", true);
        }
        
        // 改变颜色表示狂暴状态
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 进入狂暴状态 - 速度: {speed}, 攻击力: {physicalAttack}");
        }
    }
    

    
    /// <summary>
    /// 重写攻击方法
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (player == null) return;
        
        // 如果正在冲撞，使用冲撞伤害
        int damage = isCharging ? chargeDamage : (int)attackDamage;
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger(isCharging ? "ChargeAttack" : "Attack");
        }
        
        // 对玩家造成伤害
        var playerCharacter = player.GetComponent<Character>();
        if (playerCharacter != null)
        {
            playerCharacter.TakeDamage(damage);
            
            // 如果是冲撞攻击，可能造成击退效果
            if (isCharging)
            {
                ApplyKnockback(playerCharacter);
            }
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 攻击玩家，伤害: {damage} (冲撞: {isCharging})");
        }
    }
    
    /// <summary>
    /// 应用击退效果
    /// </summary>
    private void ApplyKnockback(Character playerCharacter)
    {
        var playerRb = playerCharacter.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockbackDirection = (playerCharacter.transform.position - transform.position).normalized;
            float knockbackForce = 10f;
            playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] 对玩家施加击退效果");
            }
        }
    }
    
    /// <summary>
    /// 重写受伤方法
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        
        // 受伤时有概率打断冲撞
        if (isCharging && Random.Range(0f, 1f) < 0.3f) // 30%概率
        {
            EndCharge();
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] {gameObject.name} 冲撞被打断");
            }
        }
    }
    
    /// <summary>
    /// 重写死亡方法
    /// </summary>
    public override void Die()
    {
        // 如果正在冲撞，立即停止
        if (isCharging)
        {
            EndCharge();
        }
        
        base.Die();
    }
    
    /// <summary>
    /// 重写动画更新
    /// </summary>
    public override void UpdateAnimations()
    {
        base.UpdateAnimations();
        
        if (animator == null) return;
        
        // 设置野猪特有的动画参数
        animator.SetBool("IsCharging", isCharging);
        animator.SetBool("IsEnraged", isEnraged);
        animator.SetFloat("ChargeTimer", chargeTimer);
    }
    
    /// <summary>
    /// 碰撞检测 - 处理冲撞时的碰撞
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCharging) return;
        
        // 如果冲撞时碰到玩家
        if (other.CompareTag("Player"))
        {
            var playerCharacter = other.GetComponent<Character>();
            if (playerCharacter != null)
            {
                playerCharacter.TakeDamage(chargeDamage);
                ApplyKnockback(playerCharacter);
                
                if (GameManager.Instance != null && GameManager.Instance.debugMode)
                {
                    Debug.Log($"[WildBoar] 冲撞命中玩家，伤害: {chargeDamage}");
                }
            }
        }
        // 如果冲撞时碰到障碍物
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            // 撞墙后眩晕一段时间
            StartCoroutine(StunAfterWallHit());
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] 冲撞撞墙，进入眩晕状态");
            }
        }
    }
    
    /// <summary>
    /// 撞墙后的眩晕效果
    /// </summary>
    private IEnumerator StunAfterWallHit()
    {
        EndCharge();
        
        // 播放眩晕动画
        if (animator != null)
        {
            animator.SetTrigger("Stunned");
        }
        
        // 眩晕2秒
        yield return new WaitForSeconds(2f);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 眩晕结束");
        }
    }
    
    /// <summary>
    /// 获取野猪状态信息
    /// </summary>
    public WildBoarStatus GetStatus()
    {
        return new WildBoarStatus
        {
            isCharging = this.isCharging,
            canCharge = this.canCharge,
            isEnraged = this.isEnraged,
            chargeTimer = this.chargeTimer,
            healthPercentage = (float)CurrentHealth / MaxHealth
        };
    }
}

/// <summary>
/// 野猪状态结构
/// </summary>
[System.Serializable]
public struct WildBoarStatus
{
    public bool isCharging;
    public bool canCharge;
    public bool isEnraged;
    public float chargeTimer;
    public float healthPercentage;
}