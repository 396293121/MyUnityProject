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
    
    [FoldoutGroup("野猪配置/状态监控/眩晕状态", expanded: true)]
    [LabelText("正在眩晕")]
    [ReadOnly]
    [ShowInInspector]
    private bool isStunned = false;
    
    // 公共属性访问器，供状态机使用
    public bool IsCharging => isCharging;
    public bool IsStunned => isStunned;
    public bool IsEnraged => isEnraged;
    public bool CanCharge => canCharge;
    public float ChargeTimer => chargeTimer;
    public float ChargeCooldownTime => chargeCooldown;
    public float LastChargeTime => lastChargeTime;
    
    public override void Awake()
    {
        this.enemyType = "WildBoar";
        base.Awake();
        
        // 初始化状态机引用
        // stateMachine = GetComponent<EnemyStateMachine>();
        // if (stateMachine == null)
        // {
        //     Debug.LogWarning($"[WildBoar] {gameObject.name} 未找到EnemyStateMachine组件");
        // }
        
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
        
        // 更新野猪特有的动画参数
        UpdateAnimation();
        
        // 状态机会处理大部分逻辑，这里只保留必要的野猪特有逻辑
        // 移除了重复的状态检测，交给状态机的PerformWildBoarStateCheck处理
        
        // 只在冲撞状态下执行冲撞移动逻辑
        if (isCharging)
        {
            ExecuteCharge();
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
        
        // 检查距离 - 在检测范围内但不在攻击范围内时触发冲撞
        float distanceToTarget = Vector2.Distance(transform.position, player.position);
        if (distanceToTarget > attackRange && distanceToTarget <= chargeDistance)
        {
            StartCharge();
        }
    }
    
    /// <summary>
    /// 开始冲撞
    /// 优化：改进与状态机的集成
    /// </summary>
    private void StartCharge()
    {
        if (!canAttack || player == null) return;
        
        isCharging = true;
        canCharge = false;
        chargeTimer = 0f;
        lastChargeTime = Time.time;
        
        // 通知状态机冲撞状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyChargeStateChanged();
        }
        
        // 计算冲撞方向
        chargeDirection = (player.position - transform.position).normalized;
        
        // 播放冲撞动画 - 根据Unity动画控制器参数
        if (animator != null)
        {
            animator.SetTrigger("Charge"); // 与动画控制器中的Charge参数一致
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
    // private void ExecuteCharge()
    // {
    //     chargeTimer += Time.deltaTime;
        
    //     if (chargeTimer >= chargeDuration)
    //     {
    //         // 冲撞结束
    //         EndCharge();
    //         return;
    //     }
        
    //      // 冲撞移动
    //     if (rb2D != null)
    //     {
    //         rb2D.velocity = chargeDirection * chargeSpeed;
    //     }
        
    //     // 翻转精灵
    //     if (spriteRenderer != null)
    //     {
    //         spriteRenderer.flipX = chargeDirection.x < 0;
    //     }
    // }
    
    /// <summary>
    /// 结束冲撞
    /// 优化：改进与状态机的集成
    /// </summary>
    private void EndCharge()
    {
        isCharging = false;
        
        // 通知状态机冲撞状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyChargeStateChanged();
        }
        
        // 停止移动
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
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
        
        // 播放狂暴动画 - 注意：动画控制器中没有Enrage触发器，使用布尔参数
        if (animator != null)
        {
            // animator.SetTrigger("Enrage"); // 动画控制器中没有此触发器，已移除
            animator.SetBool("IsEnraged", true); // 使用布尔参数表示狂暴状态
        }
        
        // 改变颜色表示狂暴状态
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 进入狂暴状态 - 速度: {moveSpeed}, 攻击力: {attackDamage}");
        }
    }
    
    /// <summary>
    /// 更新动画参数 - 野猪特有的动画更新
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 更新野猪特有的动画参数
        animator.SetBool("IsCharging", isCharging);
        animator.SetBool("IsStunned", isStunned);
        animator.SetBool("IsEnraged", isEnraged);
        
        // 检查狂暴状态
        CheckEnrageState();
    }
    

    
    #region AI状态机系统
    
    // 注意：以下状态机相关方法已被EnemyStateMachine接管，保留用于兼容性
    // 实际状态管理由EnemyStateMachine处理
    
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
    
    // 注意：DetectPlayer方法已被EnemyStateMachine接管，保留用于兼容性
    // 实际玩家检测由EnemyStateMachine处理
    
    #endregion
    
    /// <summary>
    /// 重写攻击方法
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (player == null) return;
        
        // 如果正在冲撞，使用冲撞伤害
        int damage = isCharging ? chargeDamage : (int)attackDamage;
        
        // 播放攻击动画 - 根据Unity动画控制器参数
        if (animator != null)
        {
            // 动画控制器中只有Attack触发器，没有ChargeAttack
            animator.SetTrigger("Attack"); // 统一使用Attack触发器
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
    /// 重写执行攻击行为 - 野猪版本
    /// </summary>
    public override void ExecuteAttack()
    {
        if (!canAttack || isDead) return;
        
        // 检查攻击冷却
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        // 调用野猪的攻击方法
        PerformAttack();
        
        // 更新最后攻击时间
        lastAttackTime = Time.time;

        // 触发攻击事件
     //   OnAttack?.Invoke(this);
    }

    /// <summary>
    /// 重写执行追击行为 - 野猪版本，包含冲撞逻辑
    /// </summary>
    public override void ExecuteChase()
    {
        if (!canMove || isDead) return;
        
        // 如果正在冲撞，让冲撞逻辑处理移动
        if (isCharging) return;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 检查是否应该开始冲撞
        if (canCharge && distanceToPlayer <= chargeDistance && distanceToPlayer > attackRange)
        {
            ExecuteCharge();
            return;
        }
        
        // 普通追击移动
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        MoveInDirection(directionToPlayer);
    }

    /// <summary>
    /// 重写执行巡逻行为 - 野猪版本
    /// </summary>
    public override void ExecutePatrol()
    {
        if (!canMove || isDead || isCharging) return;
        
        // 野猪的巡逻逻辑
        Vector2 patrolDirection = GetWildBoarPatrolDirection();
        if (patrolDirection != Vector2.zero)
        {
            // 使用巡逻速度进行移动
            MoveInDirectionWithSpeed(patrolDirection, patrolSpeed);
        }
    }

    /// <summary>
    /// 使用指定速度向指定方向移动
    /// </summary>
    private void MoveInDirectionWithSpeed(Vector2 direction, float speed)
    {
        if (!canMove || isDead) return;
        
        if (rb2D != null)
        {
            // 使用指定速度进行移动
            Vector2 targetVelocity = direction.normalized * speed;
            rb2D.velocity = targetVelocity;
        }
        else
        {
            // 如果没有Rigidbody2D，使用transform移动作为后备
            Vector3 movement = direction.normalized * speed * Time.fixedDeltaTime;
            transform.position += movement;
        }
        
        // 翻转精灵
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    /// <summary>
    /// 重写停止移动 - 野猪版本，处理冲撞状态
    /// </summary>
    public override void StopMovement()
    {
        // 如果正在冲撞，不要强制停止
        if (isCharging) return;
        
        base.StopMovement();
    }

    /// <summary>
    /// 执行冲撞
    /// </summary>
    public void ExecuteCharge()
    {
        if (!canCharge || isCharging || isDead) return;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        // 开始冲撞
        isCharging = true;
        canCharge = false;
        
        // 计算冲撞方向并设置为类成员变量
        chargeDirection = (player.position - transform.position).normalized;
        
        // 设置冲撞速度
        if (rb2D != null)
        {
            rb2D.velocity = chargeDirection * chargeSpeed;
        }
        
        // 播放冲撞动画
        if (animator != null)
        {
            animator.SetBool("IsCharging", true);
        }
        
        // 通知状态机冲撞状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyChargeStateChanged();
        }
        
        // 设置冲撞持续时间
        StartCoroutine(ChargeTimerCoroutine());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 开始冲撞，方向: {chargeDirection}");
        }
    }

    /// <summary>
    /// 冲撞计时器协程
    /// </summary>
    private IEnumerator ChargeTimerCoroutine()
    {
        yield return new WaitForSeconds(chargeDuration);
        
        // 冲撞时间结束，停止冲撞
        if (isCharging)
        {
            EndCharge();
        }
    }

    /// <summary>
    /// 获取野猪巡逻方向
    /// </summary>
    private Vector2 GetWildBoarPatrolDirection()
    {
        // 野猪的巡逻逻辑 - 更随机和自然
        if (Vector2.Distance(transform.position, currentPatrolTarget) < 0.5f)
        {
            SetRandomPatrolTarget();
        }
        
        Vector2 direction = (currentPatrolTarget - (Vector2)transform.position).normalized;
        return direction;
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
    public override void TakePlayerDamage(int damage)
    {
        base.TakePlayerDamage(damage);
        
        // 通知状态机健康状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyHealthStateChanged();
        }
        
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
    /// 获取状态机组件引用
    /// </summary>
    // private EnemyStateMachine stateMachine;
    
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
    /// 优化：改进与状态机的集成
    /// </summary>
    private IEnumerator StunAfterWallHit()
    {
        EndCharge();
        
        isStunned = true;
        
        // 通知状态机眩晕状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyStunStateChanged();
        }
        
        // 播放眩晕动画 - 根据Unity动画控制器参数
        if (animator != null)
        {
            // animator.SetTrigger("Stunned"); // 动画控制器中没有此触发器，已移除
            animator.SetBool("IsStunned", true); // 使用布尔参数表示眩晕状态
        }
        
        // 眩晕2秒
        yield return new WaitForSeconds(2f);
        
        isStunned = false;
        
        // 通知状态机眩晕状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyStunStateChanged();
        }
        
        // 更新动画
        if (animator != null)
        {
            animator.SetBool("IsStunned", false);
        }
        
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
    //     if (rb2D != null && rb2D.velocity.magnitude > 0.1f)
    //     {
    //         Gizmos.color = Color.magenta;
    //         Gizmos.DrawRay(transform.position, rb2D.velocity * 0.3f);
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