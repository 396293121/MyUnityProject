using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 野猪敌人类 - 具有冲撞攻击能力的近战敌人
/// 从原Phaser项目的WildBoar.js迁移而来
/// 支持Odin Inspector可视化编辑
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class WildBoar : Enemy
{
    [TabGroup("野猪配置", "配置引用")]
    [FoldoutGroup("野猪配置/配置引用/系统配置", expanded: true)]
    [LabelText("敌人系统配置")]
    [InfoBox("从EnemySystemConfig获取配置，避免重复定义", InfoMessageType.Info)]
    [ReadOnly]
    [ShowInInspector]
    private EnemySystemConfig systemConfig;
    
    [TabGroup("野猪配置", "冲撞系统")]
    [FoldoutGroup("野猪配置/冲撞系统/冲撞属性", expanded: true)]
    [LabelText("冲撞速度")]
    [InfoBox("从系统配置获取，如未配置则使用默认值", InfoMessageType.Warning)]
    [ReadOnly]
    [ShowInInspector]
    private float chargeSpeed = 8f;
    
    [FoldoutGroup("野猪配置/冲撞系统/冲撞属性")]
    [LabelText("冲撞持续时间")]
    [ReadOnly]
    [ShowInInspector]
    private float chargeDuration = 2f;
    
    [FoldoutGroup("野猪配置/冲撞系统/冲撞属性")]
    [LabelText("冲撞触发距离")]
    [ReadOnly]
    [ShowInInspector]
    private float chargeDistance = 6f;
    
    [FoldoutGroup("野猪配置/冲撞系统/冲撞属性")]
    [LabelText("冲撞冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    private float chargeCooldown = 5f;
    
    [FoldoutGroup("野猪配置/冲撞系统/冲撞属性")]
    [LabelText("冲撞伤害")]
    [ReadOnly]
    [ShowInInspector]
    private int chargeDamage = 25;
    
    [TabGroup("野猪配置", "狂暴系统")]
    [FoldoutGroup("野猪配置/狂暴系统/狂暴条件", expanded: true)]
    [LabelText("狂暴血量阈值")]
    [PropertyRange(0.1f, 0.8f)]
    [SuffixLabel("%")]
    [InfoBox("血量低于此百分比时进入狂暴状态")]
    public float enrageHealthThreshold = 0.3f;
    
    [FoldoutGroup("野猪配置/狂暴系统/狂暴效果", expanded: true)]
    [LabelText("狂暴速度倍数")]
    [PropertyRange(1f, 3f)]
    public float enrageSpeedMultiplier = 1.5f;
    
    [FoldoutGroup("野猪配置/狂暴系统/狂暴效果")]
    [LabelText("狂暴伤害倍数")]
    [PropertyRange(1f, 3f)]
    public float enrageDamageMultiplier = 1.3f;
    
    [TabGroup("野猪配置", "状态监控")]
    [FoldoutGroup("野猪配置/状态监控/冲撞状态", expanded: true)]
    [LabelText("正在冲撞")]
    [ReadOnly]
    [ShowInInspector]
    private bool isCharging = false;
    
    [FoldoutGroup("野猪配置/状态监控/冲撞状态")]
    [LabelText("可以冲撞")]
    [ReadOnly]
    [ShowInInspector]
    private bool canCharge = true;
    
    [FoldoutGroup("野猪配置/状态监控/冲撞状态")]
    [LabelText("冲撞方向")]
    [ReadOnly]
    [ShowInInspector]
    private Vector2 chargeDirection;
    
    [FoldoutGroup("野猪配置/状态监控/冲撞状态")]
    [LabelText("冲撞计时器")]
    [ReadOnly]
    [ProgressBar(0, "chargeDuration")]
    [ShowInInspector]
    private float chargeTimer = 0f;
    
    [FoldoutGroup("野猪配置/状态监控/冲撞状态")]
    [LabelText("上次冲撞时间")]
    [ReadOnly]
    [ShowInInspector]
    private float lastChargeTime = 0f;
    
    [FoldoutGroup("野猪配置/状态监控/狂暴状态", expanded: true)]
    [LabelText("是否狂暴")]
    [ReadOnly]
    [ShowInInspector]
    private bool isEnraged = false;
    
    [FoldoutGroup("野猪配置/状态监控/原始属性", expanded: true)]
    [LabelText("原始速度")]
    [ReadOnly]
    [ShowInInspector]
    private float originalSpeed;
    
    [FoldoutGroup("野猪配置/状态监控/原始属性")]
    [LabelText("原始攻击力")]
    [ReadOnly]
    [ShowInInspector]
    private int originalAttack;
    
    [TabGroup("野猪配置", "AI状态机")]
    [FoldoutGroup("野猪配置/AI状态机/状态控制", expanded: true)]
    [LabelText("当前AI状态")]
    [ReadOnly]
    [ShowInInspector]
    private EnemyState currentAIState = EnemyState.Idle;
    
    [FoldoutGroup("野猪配置/AI状态机/状态控制")]
    [LabelText("状态计时器")]
    [ReadOnly]
    [ProgressBar(0, 10)]
    [ShowInInspector]
    private float stateTimer = 0f;
    
    [FoldoutGroup("野猪配置/AI状态机/巡逻设置", expanded: true)]
    [LabelText("巡逻速度")]
    [PropertyRange(0.5f, 3f)]
    [SuffixLabel("米/秒")]
    public float patrolSpeed = 1f;
    
    [FoldoutGroup("野猪配置/AI状态机/巡逻设置")]
    [LabelText("巡逻范围")]
    [PropertyRange(2f, 10f)]
    [SuffixLabel("米")]
    public float patrolRange = 5f;
    
    [FoldoutGroup("野猪配置/AI状态机/巡逻设置")]
    [LabelText("巡逻等待时间")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("秒")]
    public float patrolWaitTime = 2f;
    
    [FoldoutGroup("野猪配置/AI状态机/巡逻状态", expanded: true)]
    [LabelText("当前巡逻目标")]
    [ReadOnly]
    [ShowInInspector]
    private Vector2 currentPatrolTarget;
    
    [FoldoutGroup("野猪配置/AI状态机/巡逻状态")]
    [LabelText("初始位置")]
    [ReadOnly]
    [ShowInInspector]
    private Vector2 initialPosition;
    
    [FoldoutGroup("野猪配置/AI状态机/眩晕设置", expanded: true)]
    [LabelText("眩晕持续时间")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("秒")]
    public float stunDuration = 2f;
    
    [FoldoutGroup("野猪配置/AI状态机/眩晕状态", expanded: true)]
    [LabelText("正在眩晕")]
    [ReadOnly]
    [ShowInInspector]
    private bool isStunned = false;
    
    public override void Awake()
    {
        this.enemyType = "WildBoar";
        base.Awake();
        
        // 从系统配置初始化属性
        InitializeFromSystemConfig();
        
        // 保存原始属性
        originalSpeed = moveSpeed;
        originalAttack = (int)attackDamage;
        
        // 记录初始位置
        initialPosition = transform.position;
        
        // 设置初始巡逻目标
        SetRandomPatrolTarget();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] 野猪创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {attackDamage}");
        }
    }
    
    /// <summary>
    /// 从系统配置初始化野猪属性
    /// </summary>
    private void InitializeFromSystemConfig()
    {
        // 尝试从TestSceneEnemySystem获取配置
        var enemySystem = FindObjectOfType<TestSceneEnemySystem>();
        if (enemySystem != null && enemySystem.config != null)
        {
            systemConfig = enemySystem.config;
            var wildBoarConfig = systemConfig.wildBoarConfig;
            
            // 使用系统配置初始化属性
            maxHealth = wildBoarConfig.health;
            currentHealth = maxHealth;
            attackDamage = wildBoarConfig.attackDamage;
            moveSpeed = wildBoarConfig.moveSpeed;
            
            // 初始化冲撞属性
            chargeSpeed = wildBoarConfig.chargeSpeed;
            chargeDuration = wildBoarConfig.chargeDuration;
            chargeDistance = wildBoarConfig.chargeDistance;
            chargeCooldown = wildBoarConfig.chargeCooldown;
            chargeDamage = wildBoarConfig.chargeDamage;
            
            // 初始化其他属性
            detectionRange = wildBoarConfig.detectionRange;
            attackRange = wildBoarConfig.attackRange;
            patrolSpeed = wildBoarConfig.patrolSpeed;
            patrolWaitTime = wildBoarConfig.patrolWaitTime;
            patrolRange = wildBoarConfig.patrolRadius;
        }
        else
        {
            // 如果没有找到系统配置，使用默认值
            Debug.LogWarning($"[WildBoar] 未找到EnemySystemConfig，使用默认配置");
            maxHealth = 120;
            currentHealth = maxHealth;
            attackDamage = 15;
            moveSpeed = 2.5f;
        }
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (IsDead) return;
        
        // 更新状态计时器
        stateTimer += Time.deltaTime;
        
        // 更新动画参数
        UpdateAnimatorParameters();
        
        // 执行当前状态逻辑
        ExecuteCurrentState();
        
        // 处理冲撞逻辑
        HandleChargeLogic();
        
        // 检查狂暴状态
        CheckEnrageState();
        
        // 检测玩家
        DetectPlayer();
    }
    
    /// <summary>
    /// 更新动画控制器的参数
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("IsAttacking", currentState == EnemyState.Attack);
            animator.SetBool("IsAlive", !IsDead);
            animator.SetBool("IsCharging", isCharging);
            animator.SetBool("IsStunned", isStunned);
        }
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
    

    
    #region AI状态机系统
    
    /// <summary>
    /// 改变敌人AI状态
    /// </summary>
    /// <param name="newState">新的状态</param>
    private void ChangeState(EnemyState newState)
    {
        if (currentAIState == newState) return;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] 状态改变: {currentAIState} -> {newState}");
        }
        
        // 退出当前状态
        ExitCurrentState();
        
        // 设置新状态
        currentAIState = newState;
        currentState = newState; // 同步基类状态
        stateTimer = 0f;
        
        // 进入新状态
        EnterNewState();
    }
    
    /// <summary>
    /// 退出当前状态的清理工作
    /// </summary>
    private void ExitCurrentState()
    {
        switch (currentAIState)
        {
            case EnemyState.Chase:
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Stun:
                isStunned = false;
                break;
        }
    }
    
    /// <summary>
    /// 进入新状态的初始化工作
    /// </summary>
    private void EnterNewState()
    {
        switch (currentAIState)
        {
            case EnemyState.Idle:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
                
            case EnemyState.Patrol:
                SetRandomPatrolTarget();
                break;
                
            case EnemyState.Chase:
                break;
                
            case EnemyState.Attack:
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (animator != null)
                {
                    animator.SetTrigger("Attack");
                }
                break;
                
            case EnemyState.Stun:
                isStunned = true;
                rb.velocity = new Vector2(0, rb.velocity.y);
                if (animator != null)
                {
                    animator.SetTrigger("Stun");
                }
                break;
                
            case EnemyState.Dead:
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                if (animator != null)
                {
                    animator.SetTrigger("Death");
                }
                break;
        }
    }
    
    /// <summary>
    /// 执行当前状态的逻辑
    /// </summary>
    private void ExecuteCurrentState()
    {
        switch (currentAIState)
        {
            case EnemyState.Idle:
                ExecuteIdleState();
                break;
                
            case EnemyState.Patrol:
                ExecutePatrolState();
                break;
                
            case EnemyState.Chase:
                ExecuteChaseState();
                break;
                
            case EnemyState.Attack:
                ExecuteAttackState();
                break;
                
            case EnemyState.Stun:
                ExecuteStunState();
                break;
                
            case EnemyState.Dead:
                // 死亡状态无需执行逻辑
                break;
        }
    }
    
    #endregion
    
    #region AI状态执行方法
    
    /// <summary>
    /// 执行空闲状态逻辑
    /// </summary>
    private void ExecuteIdleState()
    {
        // 停止移动
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 空闲一段时间后开始巡逻
        if (stateTimer > patrolWaitTime)
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    /// <summary>
    /// 执行巡逻状态逻辑
    /// </summary>
    private void ExecutePatrolState()
    {
        // 向巡逻目标移动
        Vector2 direction = (currentPatrolTarget - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
        
        // 更新朝向
        UpdateFacing(direction.x);
        
        // 到达巡逻目标后设置新目标
        if (Vector2.Distance(transform.position, currentPatrolTarget) < 0.5f)
        {
            SetRandomPatrolTarget();
            ChangeState(EnemyState.Idle);
        }
    }
    
    /// <summary>
    /// 执行追击状态逻辑
    /// </summary>
    private void ExecuteChaseState()
    {
        if (player == null) 
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
        
        // 计算到玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 如果玩家太远，回到巡逻状态
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
        
        // 如果足够接近，尝试攻击
        if (distanceToPlayer <= attackRange && canAttack)
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        
        // 向玩家移动
        Vector2 direction = (player.position - transform.position).normalized;
        float currentMoveSpeed = isEnraged ? moveSpeed * enrageSpeedMultiplier : moveSpeed;
        rb.velocity = new Vector2(direction.x * currentMoveSpeed, rb.velocity.y);
        
        // 更新朝向
        UpdateFacing(direction.x);
    }
    
    /// <summary>
    /// 执行攻击状态逻辑
    /// </summary>
    private void ExecuteAttackState()
    {
        // 执行攻击
        if (stateTimer < 0.1f) // 攻击开始时执行一次
        {
            PerformAttack();
        }
        
        // 攻击状态持续一定时间后结束
        if (stateTimer > 1f)
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    /// <summary>
    /// 执行眩晕状态逻辑
    /// </summary>
    private void ExecuteStunState()
    {
        // 眩晕状态下无法移动
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 眩晕时间结束
        if (stateTimer > stunDuration)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    /// <summary>
    /// 设置随机巡逻目标
    /// </summary>
    private void SetRandomPatrolTarget()
    {
        float randomX = Random.Range(-patrolRange, patrolRange);
        currentPatrolTarget = initialPosition + new Vector2(randomX, 0);
    }
    
    /// <summary>
    /// 更新角色朝向
    /// </summary>
    /// <param name="directionX">移动方向X分量</param>
    private void UpdateFacing(float directionX)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = directionX < 0;
        }
    }
    
    /// <summary>
    /// 检测玩家
    /// </summary>
    private void DetectPlayer()
    {
        if (player == null || currentAIState == EnemyState.Dead) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 如果检测到玩家且当前不在追击状态
        if (distanceToPlayer <= detectionRange && currentAIState != EnemyState.Chase && currentAIState != EnemyState.Attack)
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    #endregion
    
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
    
    [TabGroup("野猪配置", "控制面板")]
    [BoxGroup("野猪配置/控制面板/冲撞控制")]
    [Button("强制开始冲撞", ButtonSizes.Medium)]
    [GUIColor(1f, 0.6f, 0.4f)]
    [EnableIf("@!isCharging && canCharge && Application.isPlaying")]
    private void ForceStartCharge()
    {
        if (player != null) StartCharge();
    }
    
    [BoxGroup("野猪配置/控制面板/冲撞控制")]
    [Button("停止冲撞", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    [EnableIf("@isCharging && Application.isPlaying")]
    private void ForceEndCharge() => EndCharge();
    
    [BoxGroup("野猪配置/控制面板/狂暴控制")]
    [Button("强制进入狂暴", ButtonSizes.Medium)]
    [GUIColor(1f, 0.4f, 0.4f)]
    [EnableIf("@!isEnraged && Application.isPlaying")]
    private void ForceEnrage() => EnterEnrageState();
    
    [BoxGroup("野猪配置/控制面板/状态信息")]
    [ShowInInspector, ReadOnly, LabelText("当前血量百分比")]
    [ProgressBar(0, 1, ColorGetter = "GetHealthBarColor")]
    [PropertyOrder(100)]
    public float HealthPercentage => CurrentHealth / MaxHealth;
    
    [BoxGroup("野猪配置/控制面板/状态信息")]
    [ShowInInspector, ReadOnly, LabelText("冲撞冷却剩余时间")]
    [PropertyOrder(101)]
    public float ChargeCooldownRemaining => Mathf.Max(0, chargeCooldown - (Time.time - lastChargeTime));
    
    [BoxGroup("野猪配置/控制面板/状态信息")]
    [ShowInInspector, ReadOnly, LabelText("距离玩家距离")]
    [PropertyOrder(102)]
    public float DistanceToPlayer => player != null ? Vector2.Distance(transform.position, player.position) : 0f;
    
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
    
    /// <summary>
    /// 调试可视化
    /// </summary>
    // void OnDrawGizmosSelected()
    // {
    //     // AI状态显示
    //     Gizmos.color = GetStateColor();
    //     Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.3f);
        
    //     // 巡逻范围
    //     if (initialPosition != Vector3.zero)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawWireSphere(initialPosition, patrolRange);
            
    //         // 巡逻目标点
    //         if (currentPatrolTarget != Vector3.zero)
    //         {
    //             Gizmos.color = Color.cyan;
    //             Gizmos.DrawWireSphere(currentPatrolTarget, 0.2f);
    //             Gizmos.DrawLine(transform.position, currentPatrolTarget);
    //         }
    //     }
        
    //     // 检测范围
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawWireSphere(transform.position, detectionRange);
        
    //     // 攻击范围
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, attackRange);
        
    //     // 冲撞触发距离
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, chargeDistance);
        
    //     // 角色朝向
    //     Vector3 facingDirection = spriteRenderer != null && spriteRenderer.flipX ? Vector3.left : Vector3.right;
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawRay(transform.position, facingDirection * 1f);
        
    //     // 速度向量
    //     if (rb != null && rb.velocity.magnitude > 0.1f)
    //     {
    //         Gizmos.color = Color.magenta;
    //         Gizmos.DrawRay(transform.position, rb.velocity * 0.3f);
    //     }
        
    //     // 冲撞方向
    //     if (isCharging)
    //     {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawRay(transform.position, chargeDirection * chargeSpeed * 0.2f);
    //     }
    // }
    
    /// <summary>
    /// 根据AI状态返回对应颜色
    /// </summary>
    private Color GetStateColor()
    {
        switch (currentAIState)
        {
            case EnemyState.Idle: return Color.gray;
            case EnemyState.Patrol: return Color.green;
            case EnemyState.Chase: return Color.cyan;
            case EnemyState.Attack: return Color.red;
            case EnemyState.Stun: return Color.blue;
            case EnemyState.Dead: return Color.black;
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// 获取血量条颜色
    /// </summary>
    private Color GetHealthBarColor()
    {
        float healthPercent = HealthPercentage;
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
}

/// <summary>
/// 野猪状态结构
/// </summary>
[System.Serializable]
[InlineProperty]
public struct WildBoarStatus
{
    [LabelText("正在冲撞")]
    public bool isCharging;
    
    [LabelText("可以冲撞")]
    public bool canCharge;
    
    [LabelText("是否狂暴")]
    public bool isEnraged;
    
    [LabelText("冲撞计时器")]
    [Range(0f, 5f)]
    public float chargeTimer;
    
    [LabelText("血量百分比")]
    [Range(0f, 1f)]
    public float healthPercentage;
}