using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 测试野猪敌人类 - 基于ODIN优化界面和Unity规范的敌人AI系统
/// 参考SimplePlayerController的设计模式
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class SimpleEnemy : MonoBehaviour
{
    #region 敌人属性配置
    
    [TabGroup("敌人配置", "基础属性")]
    [BoxGroup("敌人配置/基础属性/生命值")]
    [LabelText("最大生命值")]
    [Range(50f, 500f)]
    [Tooltip("敌人的最大生命值")]
    public float maxHealth = 100f;
    
    [BoxGroup("敌人配置/基础属性/移动属性")]
    [LabelText("移动速度")]
    [Range(0.5f, 10f)]
    [Tooltip("敌人的基础移动速度")]
    public float moveSpeed = 2f;
    
    [BoxGroup("敌人配置/基础属性/移动属性")]
    [LabelText("巡逻速度")]
    [Range(0.5f, 5f)]
    [Tooltip("敌人巡逻时的移动速度")]
    public float patrolSpeed = 1f;
    
    [TabGroup("敌人配置", "AI行为")]
    [BoxGroup("敌人配置/AI行为/检测范围")]
    [LabelText("检测范围")]
    [Range(1f, 15f)]
    [Tooltip("敌人检测玩家的范围")]
    public float detectionRange = 10f;
    
    [BoxGroup("敌人配置/AI行为/检测范围")]
    [LabelText("失去目标范围")]
    [Range(5f, 20f)]
    [Tooltip("超过此范围后敌人会失去目标")]
    public float loseTargetRange = 12f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("攻击范围")]
    [Range(0.5f, 10f)]
    [Tooltip("敌人攻击玩家的范围")]
    public float attackRange = 5f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("攻击伤害")]
    [Range(5f, 50f)]
    [Tooltip("敌人每次攻击造成的伤害")]
    public float attackDamage = 15f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("攻击冷却时间")]
    [Range(0.5f, 5f)]
    [Tooltip("两次攻击之间的间隔时间")]
    public float attackCooldown = 2f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("冲锋冷却时间")]
    [Range(1f, 10f)]
    [Tooltip("两次冲锋之间的间隔时间")]
    public float chargeCooldown = 5f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("冲锋速度倍数")]
    [Range(1.5f, 3f)]
    [Tooltip("冲锋时的速度倍数")]
    public float chargeSpeedMultiplier = 2f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("冲锋持续时间")]
    [Range(0.5f, 3f)]
    [Tooltip("冲锋状态持续时间")]
    public float chargeDuration = 1.5f;
    
    [BoxGroup("敌人配置/AI行为/攻击属性")]
    [LabelText("眩晕持续时间")]
    [Range(1f, 5f)]
    [Tooltip("冲锋后眩晕状态持续时间")]
    public float stunDuration = 2f;
    
    [BoxGroup("敌人配置/AI行为/巡逻属性")]
    [LabelText("巡逻范围")]
    [Range(2f, 10f)]
    [Tooltip("敌人巡逻的范围")]
    public float patrolRange = 8f;
    
    [BoxGroup("敌人配置/AI行为/巡逻属性")]
    [LabelText("巡逻等待时间")]
    [Range(1f, 5f)]
    [Tooltip("到达巡逻点后的等待时间")]
    public float patrolWaitTime = 2f;
    
    #endregion
    
    #region 敌人状态枚举
    
    /// <summary>
    /// 敌人AI状态枚举
    /// </summary>
    public enum EnemyState
    {
        [LabelText("空闲状态")] Idle,
        [LabelText("巡逻状态")] Patrol,
        [LabelText("追击状态")] Chase,
        [LabelText("攻击状态")] Attack,
        [LabelText("冲锋状态")] Charge,
        [LabelText("眩晕状态")] Stun,
        [LabelText("受伤状态")] Hurt,
        [LabelText("死亡状态")] Dead
    }
    
    #endregion
    
    #region 组件引用
    
    [TabGroup("敌人配置", "组件引用")]
    [BoxGroup("敌人配置/组件引用/核心组件")]
    [LabelText("刚体组件")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人的2D刚体组件")]
    private Rigidbody2D rb;
    
    [BoxGroup("敌人配置/组件引用/核心组件")]
    [LabelText("动画控制器")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人的动画控制器组件")]
    private Animator animator;
    
    [BoxGroup("敌人配置/组件引用/核心组件")]
    [LabelText("精灵渲染器")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人的精灵渲染器组件")]
    private SpriteRenderer spriteRenderer;
    
    [BoxGroup("敌人配置/组件引用/目标引用")]
    [LabelText("玩家目标")]
    [ShowInInspector, ReadOnly]
    [Tooltip("当前追击的玩家目标")]
    private Transform player;
    
    #endregion
    
    #region 运行时状态
    
    [TabGroup("运行状态", "生命状态")]
    [BoxGroup("运行状态/生命状态/当前状态")]
    [LabelText("当前生命值")]
    [ShowInInspector, ReadOnly]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
    [Tooltip("敌人当前的生命值")]
    private float currentHealth;
    
    [TabGroup("运行状态", "AI状态")]
    [BoxGroup("运行状态/AI状态/当前状态")]
    [LabelText("当前AI状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人当前的AI行为状态")]
    private EnemyState currentState = EnemyState.Idle;
    
    [BoxGroup("运行状态/AI状态/行为状态")]
    [LabelText("是否存活")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人是否还活着")]
    private bool isAlive = true;
    
    [BoxGroup("运行状态/AI状态/行为状态")]
    [LabelText("正在攻击")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人是否正在执行攻击")]
    private bool isAttacking = false;
    
    [BoxGroup("运行状态/AI状态/行为状态")]
    [LabelText("受伤状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人是否处于受伤状态")]
    private bool isHurt = false;
    
    [BoxGroup("运行状态/AI状态/行为状态")]
    [LabelText("面向右侧")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人是否面向右侧")]
    private bool facingRight = true;
    
    [BoxGroup("运行状态/AI状态/行为状态")]
    [LabelText("正在冲锋")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人是否正在执行冲锋攻击")]
    private bool isCharging = false;
    
    [BoxGroup("运行状态/AI状态/行为状态")]
    [LabelText("眩晕状态")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人是否处于眩晕状态")]
    private bool isStunned = false;
    
    [BoxGroup("运行状态/AI状态/计时器")]
    [LabelText("状态计时器")]
    [ShowInInspector, ReadOnly]
    [Tooltip("当前状态持续时间")]
    private float stateTimer = 0f;
    
    [BoxGroup("运行状态/AI状态/计时器")]
    [LabelText("上次攻击时间")]
    [ShowInInspector, ReadOnly]
    [Tooltip("上次执行攻击的时间戳")]
    private float lastAttackTime = 0f;
    
    [BoxGroup("运行状态/AI状态/计时器")]
    [LabelText("上次冲锋时间")]
    [ShowInInspector, ReadOnly]
    [Tooltip("上次执行冲锋的时间戳")]
    private float lastChargeTime = 0f;
    
    #endregion
    
    #region 巡逻系统
    
    [TabGroup("运行状态", "巡逻状态")]
    [BoxGroup("运行状态/巡逻状态/巡逻点")]
    [LabelText("初始位置")]
    [ShowInInspector, ReadOnly]
    [Tooltip("敌人的初始生成位置")]
    private Vector3 initialPosition;
    
    [BoxGroup("运行状态/巡逻状态/巡逻点")]
    [LabelText("左巡逻点")]
    [ShowInInspector, ReadOnly]
    [Tooltip("左侧巡逻边界点")]
    private Vector3 leftPatrolPoint;
    
    [BoxGroup("运行状态/巡逻状态/巡逻点")]
    [LabelText("右巡逻点")]
    [ShowInInspector, ReadOnly]
    [Tooltip("右侧巡逻边界点")]
    private Vector3 rightPatrolPoint;
    
    [BoxGroup("运行状态/巡逻状态/巡逻点")]
    [LabelText("当前巡逻目标")]
    [ShowInInspector, ReadOnly]
    [Tooltip("当前要前往的巡逻目标点")]
    private Vector3 currentPatrolTarget;
    
    #endregion
    
    #region 调试和工具
    
    [TabGroup("调试工具", "状态控制")]
    [BoxGroup("调试工具/状态控制/生命值操作")]
    [Button("完全治疗", ButtonSizes.Medium)]
    [Tooltip("将敌人生命值恢复到最大值")]
    private void DebugFullHeal()
    {
        if (Application.isPlaying)
        {
            currentHealth = maxHealth;
            isAlive = true;
            ChangeState(EnemyState.Idle);
            Debug.Log("敌人已完全治疗！");
        }
    }
    
    [BoxGroup("调试工具/状态控制/生命值操作")]
    [Button("受到伤害 (20点)", ButtonSizes.Medium)]
    [Tooltip("让敌人受到20点伤害用于测试")]
    private void DebugTakeDamage()
    {
        if (Application.isPlaying)
        {
            TakeDamage(20f);
        }
    }
    
    [BoxGroup("调试工具/状态控制/状态切换")]
    [Button("切换到巡逻状态", ButtonSizes.Medium)]
    [Tooltip("强制切换敌人到巡逻状态")]
    private void DebugSetPatrolState()
    {
        if (Application.isPlaying)
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    [BoxGroup("调试工具/状态控制/状态切换")]
    [Button("切换到追击状态", ButtonSizes.Medium)]
    [Tooltip("强制切换敌人到追击状态")]
    private void DebugSetChaseState()
    {
        if (Application.isPlaying)
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    [TabGroup("调试工具", "状态重置")]
    [BoxGroup("调试工具/状态重置/敌人重置")]
    [Button("重置敌人状态", ButtonSizes.Large)]
    [Tooltip("重置敌人到初始状态")]
    private void DebugResetEnemy()
    {
        if (Application.isPlaying)
        {
            currentHealth = maxHealth;
            isAlive = true;
            isAttacking = false;
            isHurt = false;
            rb.velocity = Vector2.zero;
            transform.position = initialPosition;
            ChangeState(EnemyState.Idle);
            Debug.Log("敌人状态已重置！");
        }
    }
    
    [TabGroup("调试工具", "信息显示")]
    [BoxGroup("调试工具/信息显示/当前状态")]
    [ShowInInspector, ReadOnly]
    [LabelText("生命值百分比")]
    [ProgressBar(0, 100, ColorGetter = "GetHealthBarColor")]
    private float HealthPercentage => !isAlive ? 0 : (currentHealth / maxHealth) * 100f;
    
    [BoxGroup("调试工具/信息显示/当前状态")]
    [ShowInInspector, ReadOnly]
    [LabelText("敌人状态摘要")]
    private string EnemyStatusSummary => $"生命: {currentHealth:F1}/{maxHealth} | 状态: {currentState} | 存活: {isAlive} | 攻击: {isAttacking} | 受伤: {isHurt}";
    
    [BoxGroup("调试工具/信息显示/距离信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("与玩家距离")]
    private float DistanceToPlayer => player != null ? Vector2.Distance(transform.position, player.position) : -1f;
    
    /// <summary>
    /// 获取生命值进度条的颜色
    /// </summary>
    private Color GetHealthBarColor()
    {
        if (!isAlive) return Color.black;
        
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
    
    #endregion
    
    #region Unity生命周期方法
    
    /// <summary>
    /// 初始化敌人组件和状态
    /// </summary>
    void Start()
    {
        // 获取必要的组件引用
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 查找玩家目标
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 初始化生命值
        currentHealth = maxHealth;

        // 根据初始水平方向设置facingRight
        // 如果初始缩放的x值为负，则表示初始面向左侧
        facingRight = transform.localScale.x > 0;

        // 设置巡逻点
        SetupPatrolPoints();

        // 初始状态设置为Idle
        ChangeState(EnemyState.Idle);
        
        Debug.Log($"敌人初始化完成 - 生命值: {currentHealth}/{maxHealth}, 位置: {initialPosition}");
    }
    
    /// <summary>
    /// 每帧更新敌人状态和AI行为
    /// </summary>
    void Update()
    {
        // 死亡状态下停止所有操作
        if (!isAlive) 
        {
            return;
        }
        
        // 更新状态计时器
        stateTimer += Time.deltaTime;
        
        // 更新动画控制器参数
        UpdateAnimatorParameters();
        
        // 根据当前状态执行对应的AI逻辑
        ExecuteCurrentState();
        
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
            animator.SetBool("IsAttacking", isAttacking);
            animator.SetBool("IsAlive", isAlive);
            animator.SetBool("IsCharging", isCharging);
            animator.SetBool("IsStunned", isStunned);
        }
    }
    
    #endregion
    
    #region AI状态机系统
    
    /// <summary>
    /// 改变敌人AI状态
    /// </summary>
    /// <param name="newState">新的状态</param>
    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"敌人状态改变: {currentState} -> {newState}");
        
        // 退出当前状态
        ExitCurrentState();
        
        // 设置新状态
        currentState = newState;
        stateTimer = 0f;
        
        // 进入新状态
        EnterNewState();
    }
    
    /// <summary>
    /// 退出当前状态的清理工作
    /// </summary>
    private void ExitCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Chase:
                break;
            case EnemyState.Attack:
                isAttacking = false;
                break;
            case EnemyState.Charge:
                isCharging = false;
                break;
            case EnemyState.Stun:
                isStunned = false;
                break;
            case EnemyState.Hurt:
                isHurt = false;
                break;
        }
    }
    
    /// <summary>
    /// 进入新状态的初始化工作
    /// </summary>
    private void EnterNewState()
    {
        switch (currentState)
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
                isAttacking = true;
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
                
            case EnemyState.Charge:
                isCharging = true;
                if (animator != null)
                {
                    animator.SetTrigger("Charge");
                }
                AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.enemyChargeSound);
                break;
                
            case EnemyState.Stun:
                isStunned = true;
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
                
            case EnemyState.Hurt:
                isHurt = true;
                if (animator != null)
                {
                    animator.SetTrigger("isHurt");
                }
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
                
            case EnemyState.Dead:
                isAlive = false;
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
                break;
        }
    }
    
    /// <summary>
    /// 执行当前状态的逻辑
    /// </summary>
    private void ExecuteCurrentState()
    {
        switch (currentState)
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
                
            case EnemyState.Charge:
                ExecuteChargeState();
                break;
                
            case EnemyState.Stun:
                ExecuteStunState();
                break;
                
            case EnemyState.Hurt:
                ExecuteHurtState();
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
            SetRandomPatrolTarget();
            ChangeState(EnemyState.Patrol);
        }
    }
    
    /// <summary>
    /// 执行巡逻状态逻辑
    /// </summary>
    private void ExecutePatrolState()
    {
        // 向巡逻目标移动
        Vector2 direction = (currentPatrolTarget - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
        
        // 更新朝向
        UpdateFacing(direction.x);
        
        // 到达巡逻目标后设置新目标
        if (Vector2.Distance(transform.position, currentPatrolTarget) < 0.5f)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    /// <summary>
    /// 执行追击状态逻辑
    /// </summary>
    private void ExecuteChaseState()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 检查是否应该进入冲锋状态（距离适中且有一定追击时间，并且冷却时间已到）
        if (distanceToPlayer > attackRange * 1.5f && distanceToPlayer <= detectionRange * 0.8f && stateTimer > 1f && Time.time >= lastChargeTime + chargeCooldown)
        {
            // 有30%概率触发冲锋
            if (Random.Range(0f, 1f) < 0.3f)
            {
                lastChargeTime = Time.time; // 更新上次冲锋时间
                ChangeState(EnemyState.Charge);
                return;
            }
        }
        
        // 普通追击逻辑
        Vector2 normalDirection = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(normalDirection.x * moveSpeed, rb.velocity.y);
        UpdateFacing(normalDirection.x);
        
        // 检查是否进入攻击范围
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        // 检查是否失去目标
        else if (distanceToPlayer > loseTargetRange)
        {
            ChangeState(EnemyState.Patrol);
        }
    }
    
    /// <summary>
    /// 执行冲锋状态逻辑
    /// </summary>
    private void ExecuteChargeState()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Stun);
            return;
        }
        
        // 冲锋持续时间检查
        if (stateTimer >= chargeDuration)
        {
            ChangeState(EnemyState.Stun);
            return;
        }
        
        // 冲锋移动逻辑
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * chargeSpeedMultiplier, rb.velocity.y);
        UpdateFacing(direction.x);
        
        // 检查是否撞到玩家
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // 对玩家造成冲锋伤害
            var playerController = player.GetComponent<SimplePlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage * 1.5f); // 冲锋伤害更高
            }
          //  ChangeState(EnemyState.Stun);
        }
    }
    
    /// <summary>
    /// 执行眩晕状态逻辑
    /// </summary>
    private void ExecuteStunState()
    {
        // 停止移动
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 眩晕时间结束后返回追击或巡逻状态
        if (stateTimer >= stunDuration)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }
    
    /// <summary>
    /// 执行攻击状态逻辑
    /// </summary>
    private void ExecuteAttackState()
    {
        // 停止移动
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 检查攻击冷却
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
        
        // 攻击动画播放完毕后检查玩家位置
        if (stateTimer > 1f)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange)
                {
                    // 继续攻击
                    ChangeState(EnemyState.Attack);
                }
                else if (distanceToPlayer <= detectionRange)
                {
                    // 继续追击
                    ChangeState(EnemyState.Chase);
                }
                else
                {
                    // 返回巡逻
                    ChangeState(EnemyState.Patrol);
                }
            }
            else
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }
    
    /// <summary>
    /// 执行受伤状态逻辑
    /// </summary>
    private void ExecuteHurtState()
    {
        // 停止移动
        rb.velocity = new Vector2(0, rb.velocity.y);
        
        // 受伤动画播放完毕后返回之前的状态
        if (stateTimer > 0.5f)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }
    
    #endregion
    
    #region 玩家检测系统
    
    /// <summary>
    /// 检测玩家并决定是否追击
    /// </summary>
    private void DetectPlayer()
    {
        if (player == null || !isAlive) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 根据当前状态和距离决定行为
        switch (currentState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrol:
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
                
            case EnemyState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                else if (distanceToPlayer > loseTargetRange)
                {
                    ChangeState(EnemyState.Patrol);
                }
                break;
        }
    }
    
    #endregion
    
    #region 巡逻系统方法
    
    /// <summary>
    /// 设置巡逻点
    /// </summary>
    private void SetupPatrolPoints()
    {
        leftPatrolPoint = initialPosition + Vector3.left * patrolRange;
        rightPatrolPoint = initialPosition + Vector3.right * patrolRange;
        currentPatrolTarget = rightPatrolPoint;
    }
    
    /// <summary>
    /// 设置随机巡逻目标
    /// </summary>
    private void SetRandomPatrolTarget()
    {
        // 简单的左右巡逻逻辑
        if (Vector2.Distance(transform.position, rightPatrolPoint) < Vector2.Distance(transform.position, leftPatrolPoint))
        {
            currentPatrolTarget = leftPatrolPoint;
        }
        else
        {
            currentPatrolTarget = rightPatrolPoint;
        }
    }
    
    #endregion
    
    #region 战斗系统
    
    /// <summary>
    /// 执行攻击
    /// </summary>
    private void PerformAttack()
    {
        if (player == null) return;
        
        Debug.Log($"敌人攻击玩家，造成 {attackDamage} 点伤害");
        
        // 触发攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.enemyAttackSound);
        // 检测攻击范围内的玩家
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // 尝试对玩家造成伤害
            var playerController = player.GetComponent<SimplePlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(attackDamage);
            }
        }
    }
    
    /// <summary>
    /// 敌人受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        // 计算实际伤害
        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(0, currentHealth);


        Debug.Log($"敌人受到伤害: {actualDamage:F1}, 剩余生命值: {currentHealth:F1}/{maxHealth:F1}");
        
        // 根据生命值决定后续行为
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
          AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.enemyHurtSound);

            ChangeState(EnemyState.Hurt);
        }
    }
    
    /// <summary>
    /// 敌人死亡
    /// </summary>
    private void Die()
    {
        Debug.Log("敌人死亡!");
        ChangeState(EnemyState.Dead);
                AudioManagerTest.Instance?.PlaySound(AudioManagerTest.Instance.enemyDeathSound);

        // 触发死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 开始死亡处理
        StartCoroutine(HandleDeath());
    }
    
    /// <summary>
    /// 死亡后续处理协程
    /// </summary>
    /// <returns>协程迭代器</returns>
    IEnumerator HandleDeath()
    {
        // 等待死亡动画播放完成
        yield return new WaitForSeconds(2f);
        
        Debug.Log("敌人死亡处理完成");
        
        // 这里可以添加掉落物品、经验值奖励等逻辑
        // 或者销毁敌人对象
        Destroy(gameObject);
    }
    
    #endregion
    
    #region 工具方法
    
    /// <summary>
    /// 更新敌人朝向
    /// </summary>
    /// <param name="directionX">移动方向X轴</param>
    private void UpdateFacing(float directionX)
    {
        if (directionX > 0 && !facingRight)
        {
            Flip();
        }
        else if (directionX < 0 && facingRight)
        {
            Flip();
        }
    }
    
    /// <summary>
    /// 翻转敌人朝向
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }
    
    #endregion
    
    #region 可视化调试
    
    /// <summary>
    /// 在Scene视图中绘制调试信息
    /// </summary>
    [TabGroup("调试工具", "可视化调试")]
    [FoldoutGroup("调试工具/可视化调试/范围显示")]
    [PropertyOrder(100)]
    [Button("显示调试范围说明", ButtonSizes.Medium)]
    [InfoBox("黄色：玩家检测范围\n红色：攻击范围\n蓝色：失去目标范围\n绿色：巡逻路径\n白色：当前巡逻目标", InfoMessageType.Info)]
    void OnDrawGizmosSelected()
    {
        // 绘制玩家检测范围 (黄色) - 敌人检测玩家的圆形范围，玩家进入此范围后敌人开始追击
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制攻击范围 (红色) - 敌人可以对玩家造成伤害的圆形范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制失去目标范围 (蓝色) - 玩家超出此范围后，敌人将停止追击并返回巡逻状态
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        
        // 绘制巡逻范围和路径
        if (Application.isPlaying)
        {
            // 运行时显示实际巡逻点 - 巡逻路径 (绿色) - 敌人巡逻的水平路径，连接左右巡逻边界点
            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftPatrolPoint, rightPatrolPoint);
            Gizmos.DrawWireSphere(leftPatrolPoint, 0.3f);
            Gizmos.DrawWireSphere(rightPatrolPoint, 0.3f);
            
            // 绘制当前巡逻目标 (白色) - 敌人当前正在前往的巡逻目标点
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(currentPatrolTarget, 0.2f);
            Gizmos.DrawLine(transform.position, currentPatrolTarget);
        }
        else
        {
            // 编辑器模式下显示预估的巡逻范围 - 预估巡逻路径 (绿色) - 基于当前位置和巡逻范围计算
            Vector3 pos = transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos + Vector3.left * patrolRange, pos + Vector3.right * patrolRange);
            Gizmos.DrawWireSphere(pos + Vector3.left * patrolRange, 0.3f);
            Gizmos.DrawWireSphere(pos + Vector3.right * patrolRange, 0.3f);
        }
    }
    
    #endregion
}