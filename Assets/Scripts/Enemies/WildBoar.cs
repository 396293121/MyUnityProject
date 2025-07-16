using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 野猪敌人类 - 具有冲撞攻击能力的近战敌人
/// 继承自重构后的Enemy基类，采用SimpleEnemy的状态机设计
/// 特色功能：冲锋攻击、狂暴状态、撞墙眩晕
/// 性能优化：屏幕外减少更新频率
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class WildBoar : Enemy
{
    [TabGroup("野猪配置", "冲锋系统")]
    [FoldoutGroup("野猪配置/冲锋系统/冲锋属性", expanded: true)]
    [LabelText("冲锋速度")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("冲锋时的移动速度")]
    public float chargeSpeed = 8f;
    
    [FoldoutGroup("野猪配置/冲锋系统/冲锋属性")]
    [LabelText("冲锋持续时间")]
    [ReadOnly]
    [ShowInInspector]
    public float chargeDuration = 2f;
    
    [FoldoutGroup("野猪配置/冲锋系统/冲锋属性")]
    [LabelText("冲锋触发距离")]
    [ReadOnly]
    [ShowInInspector]
    public float chargeDistance = 6f;
    
    [FoldoutGroup("野猪配置/冲锋系统/冲锋属性")]
    [LabelText("冲锋冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    public float chargeCooldown = 5f;
    
    [FoldoutGroup("野猪配置/冲锋系统/冲锋属性")]
    [LabelText("冲锋伤害")]
    [ReadOnly]
    [ShowInInspector]
    public int chargeDamage = 25;
    
    [TabGroup("野猪配置", "狂暴系统")]
    [FoldoutGroup("野猪配置/狂暴系统/狂暴条件", expanded: true)]
    [LabelText("狂暴血量阈值")]
    [ReadOnly]
    [ShowInInspector]
    [InfoBox("血量低于此百分比时进入狂暴状态")]
    public float enrageHealthThreshold = 0.3f;
    
    [FoldoutGroup("野猪配置/狂暴系统/狂暴效果", expanded: true)]
    [LabelText("狂暴速度倍数")]
    [ReadOnly]
    [ShowInInspector]
    public float enrageSpeedMultiplier = 1.5f;
    
    [FoldoutGroup("野猪配置/狂暴系统/狂暴效果")]
    [LabelText("狂暴伤害倍数")]
    [ReadOnly]
    [ShowInInspector]
    public float enrageDamageMultiplier = 1.3f;
    
    [TabGroup("野猪配置", "眩晕系统")]
    [FoldoutGroup("野猪配置/眩晕系统/眩晕设置", expanded: true)]
    [LabelText("眩晕持续时间")]
    [ReadOnly]
    [ShowInInspector]
    public float stunDuration = 2f;
    
    [TabGroup("野猪配置", "状态监控")]
    [FoldoutGroup("野猪配置/状态监控/冲锋状态", expanded: true)]
    [LabelText("正在冲锋")]
    [ReadOnly]
    [ShowInInspector]
    private bool isCharging = false;
    
    [FoldoutGroup("野猪配置/状态监控/冲锋状态")]
    [LabelText("可以冲锋")]
    [ReadOnly]
    [ShowInInspector]
    private bool canCharge = true;
    
    [FoldoutGroup("野猪配置/状态监控/冲锋状态")]
    [LabelText("冲锋方向")]
    [ReadOnly]
    [ShowInInspector]
    private Vector2 chargeDirection;
    
    [FoldoutGroup("野猪配置/状态监控/冲锋状态")]
    [LabelText("冲锋计时器")]
    [ReadOnly]
    [ProgressBar(0, "chargeDuration")]
    [ShowInInspector]
    private float chargeTimer = 0f;
    
    [FoldoutGroup("野猪配置/状态监控/冲锋状态")]
    [LabelText("上次冲锋时间")]
    [ReadOnly]
    [ShowInInspector]
    private float lastChargeTime = 0f;
    
    [FoldoutGroup("野猪配置/状态监控/狂暴状态", expanded: true)]
    [LabelText("是否狂暴")]
    [ReadOnly]
    [ShowInInspector]
    private bool isEnraged = false;
    
    [FoldoutGroup("野猪配置/状态监控/眩晕状态", expanded: true)]
    [LabelText("正在眩晕")]
    [ReadOnly]
    [ShowInInspector]
    private bool isStunned = false;
    
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
    
    // 系统配置引用
    public bool IsCharging => isCharging;
    public bool IsStunned => isStunned;
    public bool IsEnraged => isEnraged;
    public bool CanCharge => canCharge;
    public float ChargeTimer => chargeTimer;
    public float ChargeCooldownTime => chargeCooldown;
    public float LastChargeTime => lastChargeTime;
    private static readonly int IsChargingHash = Animator.StringToHash("IsCharging");
    private static readonly int IsStunnedHash = Animator.StringToHash("IsStunned"); 
    private static readonly int ChargeHash = Animator.StringToHash("Charge");
    public override void Awake()
    {
        // 设置敌人类型
        enemyType = "WildBoar";
        
        // 调用基类初始化
        base.Awake();
        
        // 保存原始属性
        originalSpeed = moveSpeed;
        originalAttack = (int)attackDamage;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] 野猪创建完成 - 生命值: {currentHealth}/{maxHealth}, 攻击力: {attackDamage}");
        }
    }
          /// <summary>
    /// 调试可视化 - 野猪专用
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
   // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.transform.position, attackRange);
        
        // 绘制失去目标范围
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        
        // 绘制巡逻范围
        if (initialPosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
            Gizmos.DrawWireSphere(leftPatrolPoint, 0.3f);
            Gizmos.DrawWireSphere(rightPatrolPoint, 0.3f);
            
            // 绘制当前巡逻目标
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPatrolTarget, 0.2f);
        }
        
        // 绘制屏幕边界检测范围
        if (Camera.main != null)
        {
            Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
            Gizmos.color = isOnScreen ? Color.green : Color.gray;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
                Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 野猪状态指示器
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.4f);
        
        // 角色朝向
        Vector3 facingDirection = spriteRenderer != null && spriteRenderer.flipX ? Vector3.left : Vector3.right;
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, facingDirection * 1.5f);
        
        // 速度向量
        if (rb2D != null && rb2D.velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, rb2D.velocity * 0.5f);
        }
        
        // 冲撞方向
        if (isCharging && chargeDirection != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, chargeDirection * chargeSpeed * 0.3f);
        }
    }
    /// <summary>
    /// 重写获取基础配置方法，返回野猪特定配置
    /// </summary>
    protected override enmeyConfig GetBaseConfig(EnemySystemConfig systemConfig)
    {
        return systemConfig.wildBoarConfig;
    }
    
    /// <summary>
    /// 从系统配置初始化野猪特定属性
    /// </summary>
    protected override void InitializeFromSystemConfig()
    {
        // 先调用基类初始化
        base.InitializeFromSystemConfig();
        
        enemySystem??=FindObjectOfType<EnemySystemConfig>();
        if (enemySystem != null )
        {
            var wildBoarConfig = enemySystem.wildBoarConfig;
            
            // 初始化野猪特定属性
            chargeSpeed = wildBoarConfig.chargeSpeed;
            chargeDuration = wildBoarConfig.chargeDuration;
            chargeDistance = wildBoarConfig.chargeDistance;
            chargeCooldown = wildBoarConfig.chargeCooldown;
            chargeDamage = wildBoarConfig.chargeDamage;
            
            // 初始化狂暴系统属性
            enrageHealthThreshold = wildBoarConfig.enrageHealthThreshold;
            enrageSpeedMultiplier = wildBoarConfig.enrageSpeedMultiplier;
            enrageDamageMultiplier = wildBoarConfig.enrageDamageMultiplier;
            
            // 初始化眩晕系统属性
            stunDuration = wildBoarConfig.stunDuration;
            
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[WildBoar] 野猪特定配置初始化完成 - 冲锋速度: {chargeSpeed}, 狂暴阈值: {enrageHealthThreshold}");
            }
        }
    }
    
    protected override void Update()
    {
        // 调用基类的优化更新逻辑
        base.Update();
        
        if (IsDead) return;
        
        // 只处理野猪特有的逻辑
        UpdateWildBoarSpecificLogic();
    }
    
    /// <summary>
    /// 更新野猪特有逻辑
    /// </summary>
    private void UpdateWildBoarSpecificLogic()
    {
        // 更新野猪特有的动画参数
        UpdateWildBoarAnimationParameters();
        
        // 检查狂暴状态
        CheckEnrageState();
        
    }

    /// <summary>
    /// 更新野猪特有的动画参数
    /// </summary>
    private void UpdateWildBoarAnimationParameters()
    {
        if (animator == null) return;
        
       // animator.SetBool("IsEnraged", isEnraged);
    }
    
    /// <summary>
    /// 重写状态执行方法，添加冲锋和眩晕状态支持
    /// </summary>
    protected override void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Charge:
                ExecuteChargeState();
                break;
            case EnemyState.Stun:
                ExecuteStunState();
                break;
            default:
                base.ExecuteCurrentState();
                break;
        }
    }

    /// <summary>
    /// 执行冲锋状态 - 新增状态
    /// </summary>
    private void ExecuteChargeState()
    {
        if (!isCharging)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 更新冲锋计时器
        chargeTimer += Time.deltaTime;

        // 冲锋时间结束
        if (chargeTimer >= chargeDuration)
        {
            EndCharge();
            return;
        }

        // 执行冲锋移动
        if (rb2D != null)
        {
            Vector2 chargeVelocity = chargeDirection * chargeSpeed;
            rb2D.velocity = new Vector2(chargeVelocity.x, rb2D.velocity.y);
        }

        // 更新朝向
        if (chargeDirection.x != 0)
        {
            UpdateFacing(chargeDirection.x > 0);
        }
    }
    
    /// <summary>
    /// 执行眩晕状态 - 新增状态
    /// </summary>
    private void ExecuteStunState()
    {
        // 眩晕时停止移动
        if (rb2D != null)
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
        }
        
        // 眩晕时间结束后回到空闲状态
        if (Time.time - lastStateChangeTime >= stunDuration)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
   
    

    
    /// <summary>
    /// 开始冲锋 - 优化版本，采用SimpleEnemy的设计
    /// </summary>
    private void StartCharge()
    {
        if (!canCharge || player == null || isDead) return;
        
        isCharging = true;

        canCharge = false;
        chargeTimer = 0f;
        lastChargeTime = Time.time;
        
        // 计算冲锋方向
        chargeDirection = (player.position - transform.position).normalized;
        
        // 播放冲锋动画
        if (animator != null)
        {
            animator.SetTrigger(ChargeHash);
            animator.SetBool(IsChargingHash, true);
        }
        
        // 播放音效
        // if (AudioManagerTest.Instance != null)
        // {
        //     AudioManagerTest.Instance.PlaySound(AudioManagerTest.Instance.enemyChargeSound);
        // }
 PlayerAudioConfig.Instance.PlaySound("WildBoar_charge");

        // 切换到冲锋状态
        ChangeState(EnemyState.Charge);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 开始冲锋攻击，方向: {chargeDirection}");
        }
    }
    

    
    /// <summary>
    /// 结束冲锋 - 优化版本
    /// </summary>
    private void EndCharge()
    {
        isCharging = false;
        chargeTimer = 0f;
        
        // 停止移动
        if (rb2D != null)
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
        }
        
        // 更新动画
        if (animator != null)
        {
            animator.SetBool(IsChargingHash, false);
        }
        
        // 切换到眩晕状态
        ChangeState(EnemyState.Stun);
        
        // 开始冷却
        StartCoroutine(ChargeCooldown());
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[WildBoar] {gameObject.name} 冲锋结束");
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
        
        float healthPercentage = (float)currentHealth / maxHealth;
        
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
           // animator.SetBool("IsEnraged", true); // 使用布尔参数表示狂暴状态
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
    
  

    
    #region AI状态机系统
    
    // 清理过时的注释和代码
    // 状态管理现在由基类Enemy的状态机处理
    
    // SetRandomPatrolTarget方法已移除，使用简化的巡逻逻辑
    
    /// <summary>
    /// 更新角色朝向
    /// </summary>
    /// <param name="facingRight">是否面向右侧</param>
    private void UpdateFacing(bool facingRight)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }
        this.facingRight = facingRight;
    }
    
    // 清理过时的注释和代码
    // 玩家检测现在由基类Enemy处理
    
    #endregion
    
    /// <summary>
    /// 重写攻击执行方法 - 移除重复的PerformAttack
    /// </summary>
    protected override void ExecuteAttackState(float damage)
    {
                // 如果正在冲锋，使用冲锋伤害
        damage = isCharging ? chargeDamage : damage;
         base.ExecuteAttackState(damage);
     
    }

    /// <summary>
    /// 重写执行追击行为 - 野猪版本，采用SimpleEnemy的优化逻辑
    /// </summary>
    public override void ExecuteChase()
    {
        if (!canMove || isDead) return;
        
        // 如果正在冲锋，让冲锋逻辑处理移动
        if (isCharging) return;
        
        if (player == null) return;
        
      // 集成冲锋条件检查（原CheckChargeCondition逻辑）
    if (canCharge 
        && currentState == EnemyState.Chase 
        && Time.time >= lastChargeTime + chargeCooldown)
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange * 1.5f 
            && distanceToPlayer <= chargeDistance 
            && stateTimer > 1f)
        {
            // 30%概率触发冲锋，避免过于频繁
            if (Random.Range(0f, 1f) < 0.3f)
            {
                StartCharge();
                return;
            }
        }
    }
        
        // 普通追击移动
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        MoveInDirection(directionToPlayer);
        UpdateFacing(directionToPlayer.x > 0);
    }
    
    /// <summary>
    /// 重写执行巡逻行为 - 野猪版本，采用SimpleEnemy的简化巡逻
    /// </summary>
    public override void ExecutePatrol()
    {
        if (!canMove || isDead || isCharging) return;
        
        // 采用SimpleEnemy的时间基础巡逻逻辑
        Vector2 patrolDirection = GetWildBoarPatrolDirection();
        if (patrolDirection != Vector2.zero)
        {
            MoveInDirection(patrolDirection, patrolSpeed);
            UpdateFacing(patrolDirection.x > 0);
        }
    }
    
    /// <summary>
    /// 获取野猪巡逻方向 - 采用SimpleEnemy的稳定巡逻机制
    /// </summary>
    private Vector2 GetWildBoarPatrolDirection()
    {
        // 使用更长的巡逻周期，减少频繁切换
        float patrolCycle = patrolWaitTime * 6f; // 增加周期长度
        float currentTime = (Time.time + GetInstanceID()) % patrolCycle; // 加入实例ID避免同步
        
        if (currentTime < patrolCycle * 0.5f)
        {
            return facingRight ? Vector2.right : Vector2.left;
        }
        else
        {
            return facingRight ? Vector2.left : Vector2.right;
        }
    }

    /// <summary>
    /// 使用指定速度向指定方向移动 - 重载方法
    /// </summary>
    private void MoveInDirection(Vector2 direction, float speed)
    {
        if (!canMove || isDead || rb2D == null) return;
        
        Vector2 targetVelocity = new Vector2(direction.x * speed, rb2D.velocity.y);
        rb2D.velocity = targetVelocity;
        
        isMoving = targetVelocity.magnitude > 0.1f;
        
        // 更新朝向
        UpdateFacing(direction.x > 0);
    }

    /// <summary>
    /// 重写停止移动 - 野猪版本，处理冲撞状态
    /// </summary>
    protected override void StopMovement()
    {
        // 如果正在冲撞，不要强制停止
        if (isCharging) return;
        
        base.StopMovement();
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

    // 复杂的巡逻系统已简化，移除相关字段和方法
    
    /// <summary>
    /// 受到玩家伤害 - 优化版本
    /// </summary>
    public override void TakeDamage(float damage, Vector2 knockbackForce = default)
    {
        base.TakeDamage(damage, knockbackForce);
        
        // 有概率中断冲锋
        if (isCharging && Random.Range(0f, 1f) < 0.3f)
        {
            EndCharge();
        }
        
        // 检查是否进入狂暴状态
        CheckEnrageState();
    }
    
    /// <summary>
    /// 重写死亡方法 - 停止冲锋
    /// </summary>
    public override void Die()
    {
        // 如果正在冲锋，立即停止
        if (isCharging)
        {
            isCharging = false;
            if (rb2D != null)
            {
                rb2D.velocity = new Vector2(0, rb2D.velocity.y);
            }
        }
        
        base.Die();
    }
    
    /// <summary>
    /// 获取状态机组件引用
    /// </summary>
    // 状态机已集成到基类Enemy中
    
    /// <summary>
    /// 碰撞检测 - 处理冲撞时的碰撞
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCharging) return;
        
        // 如果冲撞时碰到玩家
        if (other.CompareTag("Player"))
        {
            if (playerController != null)
            {
                playerController.TakeDamage(chargeDamage);
                
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
        
        // 切换到眩晕状态
        ChangeState(EnemyState.Stun);
        
        // 播放眩晕动画 - 根据Unity动画控制器参数
        if (animator != null)
        {
            // animator.SetTrigger("Stunned"); // 动画控制器中没有此触发器，已移除
            animator.SetBool(IsStunnedHash, true); // 使用布尔参数表示眩晕状态
        }
        
        // 眩晕2秒
        yield return new WaitForSeconds(2f);
        
        isStunned = false;
        
        // 更新动画
        if (animator != null)
        {
            animator.SetBool(IsStunnedHash, false);
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
    public float HealthPercentage => (float)currentHealth / maxHealth;
    
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
            healthPercentage = (float)currentHealth / maxHealth
        };
    }
    
  
    
    /// <summary>
    /// 根据AI状态返回对应颜色
    /// </summary>
    private Color GetStateColor()
    {
        switch (currentState)
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