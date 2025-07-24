using UnityEngine;
using System;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 敌人基础类 - 基于SimpleEnemy的优化设计
/// 采用高效的状态机系统和性能优化
/// 支持Odin Inspector可视化编辑
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public abstract class Enemy : MonoBehaviour, IDamageable, IPausable
{
    [LabelText("敌人基础配置文件")]
    [Required]
    [SerializeField]
    public EnemyConfig enemyConfig;
    
       [LabelText("敌人音频类型")]
    public   AudioCategory audioCategory;
    [LabelText("敌人技能系统")]
    [SerializeField]
    [ReadOnly]
    protected SkillComponent skillComponent = null;
    [LabelText("敌人状态系统")]
    [SerializeField]
    [ReadOnly]
    protected BuffManager buffManager;
    [LabelText("敌人配置/攻击点")]
    [Required]
    [SerializeField]
    protected Transform attackPoint;
    public Transform AttackPoint { get { return attackPoint; } }
    #region 性能优化配置
    [FoldoutGroup("敌人配置/性能优化", expanded: false)]
    [HorizontalGroup("敌人配置/性能优化/优化设置")]
    [VerticalGroup("敌人配置/性能优化/优化设置/更新频率")]
    [LabelText("屏幕外更新间隔")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("敌人在屏幕外时的更新间隔，减少性能消耗")]
    private float offScreenUpdateInterval = 0.5f;

    [VerticalGroup("敌人配置/性能优化/优化设置/更新频率")]
    [LabelText("屏幕内更新间隔")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("敌人在屏幕内时的更新间隔")]
    private float onScreenUpdateInterval = 0.05f;

    [VerticalGroup("敌人配置/性能优化/优化设置/检测设置")]
    [LabelText("屏幕边界扩展")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("屏幕边界的扩展范围，用于提前激活敌人")]
    private float screenBoundaryExtension = 5f;

    [VerticalGroup("敌人配置/性能优化/优化设置/状态监控")]
    [LabelText("在屏幕内")]
    [ReadOnly]
    [ShowInInspector]
    protected bool isOnScreen = true;

    public bool IsOnScreen {get{ return isOnScreen; }}
    [VerticalGroup("敌人配置/性能优化/优化设置/状态监控")]
    [LabelText("上次更新时间")]
    [ReadOnly]
    [ShowInInspector]
    private float lastUpdateTime = 0f;

    [VerticalGroup("敌人配置/性能优化/优化设置/状态监控")]
    [LabelText("状态计时器")]
    [ReadOnly]
    [ShowInInspector]
    protected float stateTimer = 0f;

    [VerticalGroup("敌人配置/性能优化/优化设置/状态监控")]
    [LabelText("上次状态改变时间")]
    [ReadOnly]
    [ShowInInspector]
    protected bool isSkill;
    public bool IsSkill
    {
        set
        {
            isSkill = value;
        }
    }
    // 新增技能打断事件
    public event Action OnSkillEnd;
    protected float lastStateChangeTime = 0f;
    protected float lastPatrolPointUpdateTime = 0f;
    #endregion

    #region 事件定义
    /// <summary>
    /// 敌人死亡事件
    /// </summary>
    public event Action<Enemy> OnDeath;

    /// <summary>
    /// 敌人攻击事件
    /// </summary>
    public event Action<Enemy> OnAttack;

    #endregion
    #region 敌人属性

    [TitleGroup("敌人配置")]
    [FoldoutGroup("敌人配置/基础属性", expanded: false)]
    [HorizontalGroup("敌人配置/基础属性/属性设置")]
    [VerticalGroup("敌人配置/基础属性/属性设置/身份信息")]
    [LabelText("敌人名称")]
    [SerializeField] private string enemyName = "Enemy";

    [VerticalGroup("敌人配置/基础属性/属性设置/身份信息")]
    [LabelText("等级")]
    [PropertyRange(1, 100)]
    [SerializeField] private int level = 1;

    [VerticalGroup("敌人配置/基础属性/属性设置/生命值")]
    [LabelText("最大生命值")]
    [ReadOnly]
    [ShowInInspector]
    public int maxHealth = 100;

    [VerticalGroup("敌人配置/基础属性/属性设置/生命值")]
    [LabelText("当前生命值")]
    [ReadOnly]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
    [SerializeField] public int currentHealth;

    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("攻击伤害")]
    [ReadOnly]
    [ShowInInspector]
    public float attackDamage = 20f;
    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("伤害类型")]
    [ReadOnly]
    [ShowInInspector]
    public DamageType damageType = DamageType.Physical;
    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("物理攻击力")]
    [ReadOnly]
    [ShowInInspector]
    public float physicalAttack = 15f;

    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("防御力")]
    [ReadOnly]
    [ShowInInspector]
    public float defense = 5f;

    [HorizontalGroup("敌人配置/基础属性/移动设置")]
    [VerticalGroup("敌人配置/基础属性/移动设置/移动属性")]
    [LabelText("移动速度")]
    [ReadOnly]
    [ShowInInspector]
    public float moveSpeed = 3f;
    [HorizontalGroup("敌人配置/基础属性/移动设置")]
    [VerticalGroup("敌人配置/基础属性/移动设置/移动属性")]
    [LabelText("追击速度倍率")]
    [ReadOnly]
    [ShowInInspector]
    public float chaseSpeedRate = 1.2f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int isHurtHash = Animator.StringToHash("isHurt");
    private static readonly int dieHash = Animator.StringToHash("Die");

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [VerticalGroup("敌人配置/基础属性/移动设置/检测范围")]
    [LabelText("攻击范围")]
    //[ReadOnly]
    [Range(0f, 5f)]
    [ShowInInspector]
    public float attackRange = 2f;

    [VerticalGroup("敌人配置/基础属性/移动设置/检测范围")]
    [LabelText("检测范围")]
    // [ReadOnly]
    [Range(0f, 20f)]
    [ShowInInspector]
    public float detectionRange = 8f;

    [VerticalGroup("敌人配置/基础属性/移动设置/检测范围")]
    [LabelText("失去目标范围")]
    //[ReadOnly]
    [Range(0f, 20f)]
    [ShowInInspector]
    public float loseTargetRange = 12f;

    [VerticalGroup("敌人配置/基础属性/移动设置/奖励")]
    [LabelText("经验奖励")]
    [ReadOnly]
    [ShowInInspector]
    public int expReward = 10;

    [FoldoutGroup("敌人配置/状态控制", expanded: false)]
    [HorizontalGroup("敌人配置/状态控制/状态设置")]
    [VerticalGroup("敌人配置/状态控制/状态设置/基础状态")]
    [LabelText("是否存活")]
    [ReadOnly]
    public bool isAlive = true;


    [VerticalGroup("敌人配置/状态控制/状态设置/行为控制")]
    [LabelText("可以移动")]
    public bool canMove = true;

    [VerticalGroup("敌人配置/状态控制/状态设置/行为控制")]
    [LabelText("可以攻击")]
    public bool canAttack = true;

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

    [HorizontalGroup("敌人配置/状态控制/状态信息")]
    [VerticalGroup("敌人配置/状态控制/状态信息/当前状态")]
    [LabelText("当前AI状态")]
    [ReadOnly]
    [EnumToggleButtons]
    public EnemyState currentState = EnemyState.Idle;

//暂停前状态
    private EnemyState prePauseState;
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("是否死亡")]
    [ReadOnly]
    [SerializeField] protected bool isDead = false;

    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("正在攻击")]
    [ReadOnly]
    [SerializeField] protected bool isAttacking = false;

    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("正在移动")]
    [ReadOnly]
    [SerializeField] protected bool isMoving = false;

    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("玩家在范围内")]
    [ReadOnly]
    [SerializeField] private bool isPlayerInRange = false;

    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("刚体组件")]
    [Required("需要Rigidbody2D组件")]
    protected Rigidbody2D rb2D;
    public Rigidbody2D rb { get { return rb2D; } }

    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("碰撞器组件")]
    [Required("需要Collider2D组件")]
    private Collider2D enemyCollider;
public Collider2D EnemyCollider { get { return enemyCollider; } }
    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("动画控制器")]
    public Animator animator;

    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("精灵渲染器")]
    [Required("需要SpriteRenderer组件")]
    public SpriteRenderer spriteRenderer;
    #endregion

    #region 巡逻系统
    [FoldoutGroup("敌人配置/巡逻系统", expanded: false)]
    [HorizontalGroup("敌人配置/巡逻系统/巡逻设置")]
    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻属性")]
    [LabelText("巡逻速度")]
    [ReadOnly]
    [ShowInInspector]
    public float patrolSpeed = 1f;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻属性")]
    [LabelText("巡逻范围")]
    // [ReadOnly]
    [Range(0f, 20f)]
    [ShowInInspector]
    public float patrolRange = 8f;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻属性")]
    [LabelText("巡逻等待时间")]
    [ReadOnly]
    [ShowInInspector]
    public float patrolWaitTime = 2f;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻状态")]
    [LabelText("初始位置")]
    [ReadOnly]
    [ShowInInspector]
    protected Vector3 initialPosition;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻状态")]
    [LabelText("左巡逻点")]
    [ReadOnly]
    [ShowInInspector]
    protected Vector3 leftPatrolPoint;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻状态")]
    [LabelText("右巡逻点")]
    [ReadOnly]
    [ShowInInspector]
    protected Vector3 rightPatrolPoint;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/巡逻状态")]
    [LabelText("当前巡逻目标")]
    [ReadOnly]
    [ShowInInspector]
    protected Vector3 currentPatrolTarget;

    [VerticalGroup("敌人配置/巡逻系统/巡逻设置/朝向控制")]
    [LabelText("面向右侧")]
    [ReadOnly]
    [ShowInInspector]
    public bool facingRight = true;
    #endregion

    #region 目标和导航
    [FoldoutGroup("敌人配置/导航系统", expanded: false)]
    [HorizontalGroup("敌人配置/导航系统/导航设置")]
    [VerticalGroup("敌人配置/导航系统/导航设置/目标设置")]
    [LabelText("玩家对象")]
    [ReadOnly]
    [SerializeField] protected Transform player;

    //玩家控制器，用于触发HURT
    protected PlayerController playerController;
    #endregion

    #region 攻击系统
    [FoldoutGroup("敌人配置/攻击系统", expanded: false)]
    [HorizontalGroup("敌人配置/攻击系统/攻击配置")]
    [VerticalGroup("敌人配置/攻击系统/攻击配置/攻击设置")]
    [LabelText("攻击冷却时间")]
    [ReadOnly]
    [ShowInInspector]
    public float attackCooldown = 2f;

    [VerticalGroup("敌人配置/攻击系统/攻击配置/攻击设置")]
    [LabelText("上次攻击时间")]
    [ReadOnly]
    [SerializeField] protected float lastAttackTime;
    #endregion

    #region 属性访问器
    [FoldoutGroup("敌人配置/调试信息", expanded: false)]
    [HorizontalGroup("敌人配置/调试信息/信息显示")]
    [VerticalGroup("敌人配置/调试信息/信息显示/属性访问器")]
    [ShowInInspector, ReadOnly, LabelText("敌人名称")]
    public string EnemyName => enemyName;
    public bool isPlayerInAttackRange;

    [ShowInInspector, ReadOnly, LabelText("等级")]
    public int Level => level;

    [ShowInInspector, ReadOnly, LabelText("最大生命值")]
    public int MaxHealth => maxHealth;

    [ShowInInspector, ReadOnly, LabelText("当前生命值")]
    public int CurrentHealth => currentHealth;

    [ShowInInspector, ReadOnly, LabelText("攻击伤害")]
    public float AttackDamage => attackDamage;

    [ShowInInspector, ReadOnly, LabelText("移动速度")]
    public float MoveSpeed => moveSpeed;

    [ShowInInspector, ReadOnly, LabelText("攻击范围")]
    public float AttackRange => attackRange;

    [ShowInInspector, ReadOnly, LabelText("检测范围")]
    public float DetectionRange => detectionRange;

    [ShowInInspector, ReadOnly, LabelText("经验奖励")]
    public int ExpReward => expReward;

    [ShowInInspector, ReadOnly, LabelText("是否死亡")]
    public bool IsDead => isDead;

    // IDamageable接口属性
    public bool IsAlive => !isDead;
    // public int CurrentHealth => currentHealth;
    // public int MaxHealth => maxHealth;

    [ShowInInspector, ReadOnly, LabelText("正在攻击")]
    public bool IsAttacking => isAttacking;

    [ShowInInspector, ReadOnly, LabelText("正在移动")]
    public bool IsMoving => isMoving;

    [ShowInInspector, ReadOnly, LabelText("玩家在范围内")]
    public bool IsPlayerInRange => isPlayerInRange;

    [ShowInInspector, ReadOnly, LabelText("当前位置")]
    public Vector3 Position => transform.position;

    private bool isPaused = false;

    /// <summary>
    /// 获取生命值进度条颜色
    /// </summary>
    private Color GetHealthBarColor()
    {
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
    #endregion

    #region Unity生命周期
    [HorizontalGroup("敌人配置/调试信息/控制面板")]
    [VerticalGroup("敌人配置/调试信息/控制面板/生命周期方法")]
    [Button("手动初始化组件", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void ManualAwake() => Awake();

    public virtual void Awake()
    {
        // 获取组件引用
        rb2D = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 从系统配置初始化参数
        InitializeFromSystemConfig();

        // 初始化技能组件
        InitializeSkillComponent();
        InitializeBuffManager();
        // 配置敌人属性
        //   ConfigureEnemy(this, enemyType);
        // 初始化属性
        currentHealth = maxHealth;

        // 记录初始位置
        initialPosition = transform.position;

        // 根据初始水平方向设置facingRight
        facingRight = transform.localScale.x > 0;
    }

    private void Start()
    {
        // 查找玩家
        FindPlayer();

        // 设置巡逻点
        //   SetupPatrolPoints();

        // 初始状态设置为Idle
        ChangeState(EnemyState.Idle);

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            //  Debug.Log($"[Enemy] {gameObject.name} 初始化完成 - 生命值: {currentHealth}/{maxHealth}, 位置: {initialPosition}");
        }
    }

    protected virtual void Update()
    {
            // 更新状态计时器
        stateTimer += Time.unscaledDeltaTime;
        if (!isAlive || !ShouldUpdate()) return;

        // 更新动画控制器参数
        UpdateAnimatorParameters();
        // 性能优化：检查是否需要更新
        // if (!ShouldUpdate()) return;

    



        // 根据当前状态执行对应的AI逻辑
        ExecuteCurrentState();

        // 检测玩家（降低频率）
        if (ShouldDetectPlayer())
        {
            DetectPlayer();
        }
        updatePatrolPoints();
        // 更新最后更新时间
        lastUpdateTime = Time.time;
    }
     public void SetPaused(bool paused)
    {
        isPaused = paused;
        if(paused)
        {
            prePauseState = currentState;
            animator.enabled = false;
        }
        else
        {
            currentState = prePauseState;
            animator.enabled = true;
        }
    }
    protected virtual void InitializeSkillComponent()
    {
        skillComponent = GetComponent<SkillComponent>();
        if (skillComponent != null)
        {
            // 配置技能组件引用
            skillComponent.SetEnemyReferences(this, null);
        }
    }
    protected virtual void InitializeBuffManager()
    {
        buffManager = GetComponent<BuffManager>();
        if (buffManager != null)
        {
            // 配置BUFF组件引用
            buffManager.SetEnemyController(this);
        }
    }
    private void updatePatrolPoints()
    {
        if (Time.time - lastPatrolPointUpdateTime > 5f)
        {
            SetupPatrolPoints();
            lastPatrolPointUpdateTime = Time.time;
        }
    }


    private bool ShouldDetectPlayer()
    {
        // 冲锋状态下禁用检测（WildBoar.cs第228-231行）
        if (currentState == EnemyState.Charge) return false;

        // 攻击状态下降低检测频率
        float baseInterval = isOnScreen ? 0.1f : 0.5f;
        if (currentState == EnemyState.Attack) baseInterval *= 2;

        return Time.time - lastUpdateTime >= baseInterval;
    }
    /// <summary>
    /// 性能优化：检查是否应该更新敌人
    /// </summary>
    private bool ShouldUpdate()
    {
        // 检查屏幕范围
        CheckScreenBounds();
        // 根据是否在屏幕内决定更新频率
        float updateInterval = isOnScreen ? onScreenUpdateInterval : offScreenUpdateInterval;

        return Time.time - lastUpdateTime >= updateInterval;
    }

    /// <summary>
    /// 检查敌人是否在屏幕范围内
    /// </summary>
    // 在CheckScreenBounds方法中添加状态同步
    private void CheckScreenBounds()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            isOnScreen = true; // 如果没有主摄像机，默认认为在屏幕内
            return;
        }

        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.position);

        // 考虑边界扩展
        float extension = screenBoundaryExtension / mainCamera.orthographicSize;

        isOnScreen = screenPoint.x >= -extension && screenPoint.x <= 1 + extension &&
                     screenPoint.y >= -extension && screenPoint.y <= 1 + extension &&
                     screenPoint.z > 0;
        if (!wasOnScreen && isOnScreen)
        {
            DetectPlayer();
        }
        wasOnScreen = isOnScreen;
    }

    /// <summary>
    /// 更新动画控制器的参数
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // 使用哈希值提升性能（参考PlayerController优化）
            animator.SetFloat(SpeedHash, Mathf.Abs(rb2D.velocity.x));
            // 仅在状态变化时更新（参考EnemyStateMachine_Optimization_Summary.md第13点）

        }
    }


    private void FixedUpdate()
    {
        if (isDead) return;

        // 移动逻辑现在由状态机处理
        // HandleMovement(); // 已移除，由状态机处理
    }
    #endregion

    /// <summary>
    /// 从系统配置初始化敌人基础属性
    /// </summary>
    protected virtual void InitializeFromSystemConfig()
    {
        //优先读取配置文件
        enemyConfig ??= FindObjectOfType<EnemyConfig>();
        if (enemyConfig != null)
        {
            var baseConfig = GetBaseConfig(enemyConfig);

            if (baseConfig != null)
            {
                // 初始化基础属性
                maxHealth = baseConfig.health;
                currentHealth = maxHealth;
                attackDamage = baseConfig.attackDamage;
                damageType = baseConfig.damageType;
                defense = baseConfig.defense;
                moveSpeed = baseConfig.moveSpeed;
                chaseSpeedRate = baseConfig.chaseSpeedRate;
                attackRange = baseConfig.attackRange;
                detectionRange = baseConfig.detectionRange;
                loseTargetRange = baseConfig.loseTargetRange;
                attackCooldown = baseConfig.attackCooldown;
                expReward = baseConfig.expReward;

                // 初始化巡逻属性
                patrolSpeed = baseConfig.patrolSpeed;
                patrolWaitTime = baseConfig.patrolWaitTime;
                //巡逻半径在场景配置文件中设置
               // patrolRange = baseConfig.patrolRadius;

                // 初始化性能优化参数
                offScreenUpdateInterval = baseConfig.offScreenUpdateInterval;
                onScreenUpdateInterval = baseConfig.onScreenUpdateInterval;
                screenBoundaryExtension = baseConfig.screenBoundaryExtension;

                if (GameManager.Instance != null && GameManager.Instance.debugMode)
                {
                    Debug.Log($"[Enemy] {gameObject.name} 从系统配置初始化完成");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Enemy] {gameObject.name} 未找到EnemySystemConfig，使用默认配置");
        }
    }

    /// <summary>
    /// 获取基础配置，子类可重写以返回特定配置
    /// </summary>
    protected virtual EnemyConfig GetBaseConfig(EnemyConfig enemyConfig)
    {
        return enemyConfig;
    }
    #region 初始化方法
    /// <summary>
    /// 查找玩家
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    /// <summary>
    /// 设置巡逻点 - 简化版本
    /// </summary>
    protected void SetupPatrolPoints()
    {
        initialPosition = transform.position;
        leftPatrolPoint = initialPosition + Vector3.left * (patrolRange * 0.5f);
        rightPatrolPoint = initialPosition + Vector3.right * (patrolRange * 0.5f);
        currentPatrolTarget = facingRight ? rightPatrolPoint : leftPatrolPoint;

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 巡逻点设置完成 - 左: {leftPatrolPoint}, 右: {rightPatrolPoint}");
        }
    }



    /// <summary>
    /// 设置敌人等级
    /// </summary>
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        // 根据等级调整属性
        float levelMultiplier = 1f + (newLevel - 1) * 0.1f;
        maxHealth *= (int)levelMultiplier;
        currentHealth = maxHealth;
        attackDamage *= levelMultiplier;
    }
    #endregion

    #region AI状态机系统

    /// <summary>
    /// 改变敌人AI状态
    /// </summary>
    /// <param name="newState">新的状态</param>
    protected void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        cachedPatrolDirection = null;
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 状态改变: {currentState} -> {newState}");
        }

        // 退出当前状态
        ExitCurrentState();

        // 设置新状态
        currentState = newState;
        stateTimer = 0f;
        lastStateChangeTime = Time.time;

        // 进入新状态
        EnterNewState();
    }

    /// <summary>
    /// 退出当前状态的清理工作
    /// </summary>
    protected virtual void ExitCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Chase:
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Hurt:
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
                break;

            case EnemyState.Patrol:
                SetRandomPatrolTarget();
                break;

            case EnemyState.Chase:
                break;

            case EnemyState.Attack:
                StopMovement();
                break;

            case EnemyState.Hurt:

                break;

            case EnemyState.Dead:
                isAlive = false;
                rb2D.velocity = Vector2.zero;
                rb2D.isKinematic = true;
                break;
        }
    }

    /// <summary>
    /// 执行当前状态的逻辑 - 优化版本
    /// </summary>
    protected virtual void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                ExecuteIdleState();
                CheckPlayerWithInterval(1.5f); // 空闲状态降低检测频率
                break;

            case EnemyState.Patrol:
                ExecutePatrolState();
                CheckPlayerWithInterval(0.8f); // 巡逻状态中等检测频率
                break;

            case EnemyState.Chase:
                ExecuteChaseState();
                CheckPlayerWithInterval(0.2f); // 追击状态高频检测
                break;

            case EnemyState.Attack:
                ExecuteAttackState(attackDamage);
                break;

            case EnemyState.Hurt:
                ExecuteHurtState();
                break;

            case EnemyState.Dead:
                // 死亡状态无需执行逻辑
                break;

            case EnemyState.Charge:
                // 子类可以重写此状态
                break;

            case EnemyState.Stun:
                // 子类可以重写此状态
                break;
        }
    }

    /// <summary>
    /// 分层玩家检测（带状态专属间隔）
    /// </summary>
    private void CheckPlayerWithInterval(float baseInterval)
    {
        // 动态调整检测间隔：屏幕外时延长检测间隔
        float adjustedInterval = isOnScreen ? baseInterval : baseInterval * 3f;

        if (Time.time - lastDetectionTime > adjustedInterval)
        {
            DetectPlayer();
            lastDetectionTime = Time.time;
        }
    }

    #endregion

    #region AI状态执行方法

    /// <summary>
    /// 执行空闲状态逻辑 - 优化版本，避免频繁切换
    /// </summary>
    private void ExecuteIdleState()
    {
        // 停止移动
        StopMovement();

        // 增加最小空闲时间，避免频繁切换到巡逻
        float minIdleTime = isOnScreen ? 1f : 2f; // 屏幕外更长的空闲时间
        if (stateTimer > minIdleTime)
        {
            // 随机决定是否开始巡逻，增加变化性
            if (UnityEngine.Random.Range(0f, 1f) < 0.7f) // 70%概率开始巡逻
            {
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    /// <summary>
    /// 执行巡逻状态逻辑 - 优化版本，采用SimpleEnemy的简化巡逻
    /// </summary>
    private void ExecutePatrolState()
    {
        if (!canMove) return;

        // 简化的左右巡逻逻辑，类似SimpleEnemy
        Vector2 patrolDirection = GetSimplePatrolDirection();

        if (patrolDirection != Vector2.zero)
        {
            rb2D.velocity = new Vector2(patrolDirection.x * patrolSpeed, rb2D.velocity.y);
        }
        // 增加最小巡逻时间，避免频繁切换回空闲
        float minPatrolTime = isOnScreen ? 3f : 5f; // 屏幕外更长的巡逻时间
        if (stateTimer > minPatrolTime)
        {
            // 随机决定是否回到空闲状态
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f) // 30%概率回到空闲
            {
                ChangeState(EnemyState.Idle);
            }
        }
    }
    private Vector2? cachedPatrolDirection;
    private float lastDetectionTime;
    private bool wasOnScreen;

    /// <summary>
    /// 获取简单的巡逻方向 - 采用SimpleEnemy的时间基础巡逻
    /// </summary>
    private Vector2 GetSimplePatrolDirection()
    {
        if (cachedPatrolDirection.HasValue)
        {
            return cachedPatrolDirection.Value;
        }
        else
        {
            float patrolCycle = patrolWaitTime * 4f; // 完整巡逻周期，增加稳定性
            float currentTime = (Time.time) % patrolCycle; // 加入实例ID避免同步
            bool shouldMoveRight = currentTime < patrolCycle * 0.5f;
         
            UpdateFacing(shouldMoveRight); // 新增方向同步
            Vector2 direction = shouldMoveRight ? Vector2.right : Vector2.left;
            cachedPatrolDirection = direction;
            return direction;
        }


    }

    /// <summary>
    /// 执行追击状态逻辑 - 优化版本
    /// </summary>
    protected virtual void ExecuteChaseState()
    {
        if (!canMove || player == null) return;
        // 修改为攻击点检测
        if (IsPlayerInAttackRange())
        {
            ChangeState(EnemyState.Attack);
            return;
        }
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        // 检查是否失去目标 - 增加缓冲时间避免频繁切换
        if (distanceToPlayer > loseTargetRange)
        {
            if (stateTimer > 2f) // 至少追击2秒后才能失去目标
            {
                ChangeState(EnemyState.Patrol);
            }
            return;
        }

        // 追击移动
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        rb2D.velocity = new Vector2(directionToPlayer.x * moveSpeed * chaseSpeedRate, rb2D.velocity.y);
        UpdateFacing(directionToPlayer.x > 0);
    }


    /// <summary>
    /// 执行受伤状态逻辑
    /// </summary>
    private void ExecuteHurtState()
    {
        // 停止移动
        StopMovement();

        // 受伤状态持续时间

    }
    public virtual void onHurtEnd()
    {
          // 防止重复调用
    if (currentState != EnemyState.Hurt) return;
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else
        {
            ChangeState(EnemyState.Patrol);
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

    /// <summary>
    /// 更新敌人朝向
    /// </summary>
    /// <param name="facingRight">是否面向右侧</param>
    protected virtual void UpdateFacing(bool facingRight)
    {
        if (facingRight && !this.facingRight)
        {
            Flip();
        }
        else if (!facingRight && this.facingRight)
        {
            Flip();
        }
    }

    /// <summary>
    /// 翻转敌人朝向
    /// </summary>
    private void Flip()
    {
        // 先更新方向状态再设置缩放
        facingRight = !facingRight;
        float scaleX = Mathf.Abs(transform.localScale.x) * (facingRight ? 1 : -1);
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }

    #endregion

    #region AI行为系统

    /// <summary>
    /// 执行攻击
    /// </summary>
    protected virtual void PerformAttack()
    {

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 执行攻击");
        }

        // 子类可以重写此方法实现具体的攻击逻辑
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public virtual void TakeDamage(int damage,DamageType damageType, Vector2 hitPoint=default, Character attacker=null)
    {
        if (!isAlive) 
        {
            Debug.Log($"[{gameObject.name}] 已死亡，忽略伤害");
            return;
        }

        // 计算实际伤害
        int actualDamage = damageType == DamageType.Magical ?
            Mathf.Max(1, damage - (int)defense) :
            Mathf.Max(1, damage - (int)defense);

        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);

            Vector3 damageNumberPosition = transform.position;
        
            if (enemyCollider != null)
            {
                damageNumberPosition += Vector3.up * enemyCollider.bounds.extents.y;
            }
DamagePopup popup = DamagePool.Instance.GetPopup();
        popup.Setup(damageNumberPosition, actualDamage, damageType);

        // 处理死亡
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        
        //技能可以被打断
        if (isSkill)
        {
            OnSkillEnd?.Invoke();
            Debug.Log("技能被打断");
            InterruptSkill();
        }

        // 触发受伤状态
        ChangeState(EnemyState.Hurt);
        if (animator != null)
        {
            animator.SetTrigger(isHurtHash);
        }
        PlayerAudioConfig.Instance.PlaySound("hurt",audioCategory);

        Debug.Log($"[{gameObject.name}] 受到 {actualDamage} 点{damageType}伤害，剩余生命值: {currentHealth}");
    }

    protected virtual void InterruptSkill()
    {
        // 供子类扩展的技能被打断方法
        if (!isSkill) return;

        // 核心打断流程
        StopAllCoroutines();
        rb2D.velocity = Vector2.zero; // 立即停止技能位移

        if (skillComponent != null)
        {
            skillComponent.InterruptCurrentSkill(); // 通知技能组件中断
        }

        // if (buffManager != null)
        // {
        //     buffManager.RemoveSkillBuffs(); // 移除技能相关BUFF
        // }

        // 动画控制
        if (animator != null)
        {
            animator.SetTrigger(isHurtHash); // 强制切换到受伤动画
        }

        // 状态机控制
        ChangeState(EnemyState.Hurt);
        isSkill = false;
    }
    /// <summary>
    /// 敌人死亡
    /// </summary>
    public virtual void Die()
    {
        if (!isAlive) return;

        isAlive = false;
        ChangeState(EnemyState.Dead);

        if (animator != null)
        {
            animator.SetTrigger(dieHash);
        }
        PlayerAudioConfig.Instance.PlaySound( "die",audioCategory);

        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 死亡");
        }

        // 启动死亡处理协程
        StartCoroutine(HandleDeath());
    }

    /// <summary>
    /// 处理死亡后的清理工作
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleDeath()
    {
        // 等待死亡动画播放完毕
        yield return new WaitForSeconds(2f);

        // 销毁敌人对象
        Destroy(gameObject);
    }

    #endregion

    #region 移动系统 (已优化)


    /// <summary>
    /// 向指定方向以指定速度移动 (保持Y轴速度)
    /// </summary>
    /// <param name="direction">移动方向</param>
    /// <param name="speed">移动速度</param>
    protected virtual void MoveInDirection(Vector2 direction, float speed)
    {
        if (rb2D != null)
        {
            // 保持Y轴速度，只改变X轴速度
            Vector2 targetVelocity = new Vector2(direction.x * speed, rb2D.velocity.y);
            rb2D.velocity = targetVelocity;

        isMoving = targetVelocity.magnitude > 0.1f;
        }
    }

    /// <summary>
    /// 停止移动 (保持Y轴速度)
    /// </summary>
    protected virtual void StopMovement()
    {
        if (rb2D != null)
        {
            // 保持Y轴速度，只停止X轴移动
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
        }
    }

    #endregion

    #region 伤害和死亡系统





    /// <summary>
    /// 死亡清理协程
    /// </summary>
    private IEnumerator DeathCleanup()
    {
        // 等待死亡动画播放完成
        yield return new WaitForSeconds(2f);

        // 淡出效果
        if (spriteRenderer != null)
        {
            float fadeTime = 1f;
            float elapsedTime = 0f;
            Color originalColor = spriteRenderer.color;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // 销毁游戏对象
        Destroy(gameObject);
    }
    #endregion


    #region 公共方法
    /// <summary>
    /// 强制死亡
    /// </summary>
    public void ForceDeath()
    {
        currentHealth = 0;
        Die();
    }

    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
    }





    /// <summary>
    /// 获取到玩家的距离
    /// </summary>
    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    /// <summary>
    /// 重置敌人状态
    /// </summary>
    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isDead = false;
        isAttacking = false;
        isMoving = false;
        isPlayerInRange = false;

        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }

        if (rb2D != null)
        {
            rb2D.isKinematic = false;
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
        }
    }


    /// <summary>
    /// 执行巡逻行为 - 抽象方法，由子类实现具体逻辑
    /// </summary>
    public abstract void ExecutePatrol();



    /// <summary>
    /// 执行攻击行为
    /// </summary>
    #region 攻击系统
    // 新增攻击检测方法
    protected bool IsPlayerInAttackRange()
    {
        if (attackPoint == null) return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    protected virtual void ExecuteAttackState(float damage)
    {
        if (!canAttack || isDead) return;

        // 新增攻击范围检测
        if (!IsPlayerInAttackRange())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 检查攻击冷却
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
            PlayerAudioConfig.Instance.PlaySound("attack",audioCategory);
        }

        // 新增攻击点检测
        if (playerController != null)
        {
            playerController.TakeDamage((int)damage,damageType);
        }

        lastAttackTime = Time.time;
        OnAttack?.Invoke(this);
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            //    Debug.Log($"[WildBoar] {gameObject.name} 攻击玩家，伤害: {damage}");
        }
    }


    // GetPatrolDirection方法已移至子类实现
    #endregion


    #region 调试可视化



    #endregion
}

/// <summary>
/// 玩家生命值组件（示例）
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }


    #endregion

}