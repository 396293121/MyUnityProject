using UnityEngine;
using System;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 敌人基础类
/// 基于Phaser项目中的敌人系统设计
/// 支持Odin Inspector可视化编辑
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class Enemy : MonoBehaviour, IDamageable
{
    #region 事件定义
    /// <summary>
    /// 敌人死亡事件
    /// </summary>
    public event Action<Enemy> OnDeath;
    
    /// <summary>
    /// 敌人攻击事件
    /// </summary>
    public event Action<Enemy> OnAttack;
    
    /// <summary>
    /// 敌人受伤事件
    /// </summary>
    public event Action<Enemy, float> OnTakeDamage;
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
    [PropertyRange(1, 1000)]
    [SerializeField] public int maxHealth = 100;
    
    [VerticalGroup("敌人配置/基础属性/属性设置/生命值")]
    [LabelText("当前生命值")]
    [ReadOnly]
    [ProgressBar(0, "maxHealth", ColorGetter = "GetHealthBarColor")]
    [SerializeField] public int currentHealth;
    
    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("攻击伤害")]
    [PropertyRange(1, 100)]
    [SerializeField] public float attackDamage = 20f;
    
    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("物理攻击力")]
    [PropertyRange(1, 100)]
    [SerializeField] public float physicalAttack = 15f;
    
    [VerticalGroup("敌人配置/基础属性/属性设置/战斗属性")]
    [LabelText("防御力")]
    [PropertyRange(0, 50)]
    [SerializeField] public float defense = 5f;
    
    [HorizontalGroup("敌人配置/基础属性/移动设置")]
    [VerticalGroup("敌人配置/基础属性/移动设置/移动属性")]
    [LabelText("移动速度")]
    [PropertyRange(0.1f, 10f)]
    [SerializeField] public float moveSpeed = 3f;
    
    [VerticalGroup("敌人配置/基础属性/移动设置/移动属性")]
    [LabelText("速度")]
    [PropertyRange(0.1f, 10f)]
    [SerializeField] public float speed = 3f;
    
    [VerticalGroup("敌人配置/基础属性/移动设置/检测范围")]
    [LabelText("攻击范围")]
    [PropertyRange(0.5f, 10f)]
    [SerializeField] public float attackRange = 2f;
    
    [VerticalGroup("敌人配置/基础属性/移动设置/检测范围")]
    [LabelText("检测范围")]
    [PropertyRange(1f, 20f)]
    [SerializeField] public float detectionRange = 8f;
    
    [VerticalGroup("敌人配置/基础属性/移动设置/奖励")]
    [LabelText("经验奖励")]
    [PropertyRange(1, 1000)]
    [SerializeField] public int expReward = 10;
    
    [FoldoutGroup("敌人配置/状态控制", expanded: false)]
    [HorizontalGroup("敌人配置/状态控制/状态设置")]
    [VerticalGroup("敌人配置/状态控制/状态设置/基础状态")]
    [LabelText("是否存活")]
    [ReadOnly]
    public bool isAlive = true;
    
    [VerticalGroup("敌人配置/状态控制/状态设置/基础状态")]
    [LabelText("目标对象")]
    [ReadOnly]
    public Transform target;
    
    [VerticalGroup("敌人配置/状态控制/状态设置/行为控制")]
    [LabelText("可以移动")]
    public bool canMove = true;
    
    [VerticalGroup("敌人配置/状态控制/状态设置/行为控制")]
    [LabelText("可以攻击")]
    public bool canAttack = true;
    
    public enum EnemyState
    {
        [LabelText("待机")] Idle,
        [LabelText("巡逻")] Patrol,
        [LabelText("追击")] Chase,
        [LabelText("攻击")] Attack,
        [LabelText("死亡")] Dead,
        [LabelText("眩晕")] Stun
    }
    
    [HorizontalGroup("敌人配置/状态控制/状态信息")]
    [VerticalGroup("敌人配置/状态控制/状态信息/当前状态")]
    [LabelText("当前AI状态")]
    [ReadOnly]
    [EnumToggleButtons]
    public EnemyState currentState = EnemyState.Idle;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("是否死亡")]
    [ReadOnly]
    [SerializeField] private bool isDead = false;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("正在攻击")]
    [ReadOnly]
    [SerializeField] private bool isAttacking = false;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("正在移动")]
    [ReadOnly]
    [SerializeField] private bool isMoving = false;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("玩家在范围内")]
    [ReadOnly]
    [SerializeField] private bool isPlayerInRange = false;
    #endregion
    
    #region 组件引用
    [HorizontalGroup("敌人配置/敌人组件引用/组件设置")]
    [VerticalGroup("敌人配置/敌人组件引用/组件设置/核心组件")]
    [LabelText("刚体组件")]
    [Required("需要Rigidbody2D组件")]
    protected Rigidbody2D rb2D;
    public Rigidbody2D rb { get { return rb2D; } }
    
    [VerticalGroup("敌人配置/敌人组件引用/组件设置/核心组件")]
    [LabelText("碰撞器组件")]
    [Required("需要Collider2D组件")]
    private Collider2D enemyCollider;
    
    [VerticalGroup("敌人配置/敌人组件引用/组件设置/核心组件")]
    [LabelText("动画控制器")]
    public Animator animator;
    
    [VerticalGroup("敌人配置/敌人组件引用/组件设置/核心组件")]
    [LabelText("精灵渲染器")]
    [Required("需要SpriteRenderer组件")]
    public SpriteRenderer spriteRenderer;
    #endregion
    
    #region 目标和导航
    [FoldoutGroup("敌人配置/导航系统", expanded: false)]
    [HorizontalGroup("敌人配置/导航系统/导航设置")]
    [VerticalGroup("敌人配置/导航系统/导航设置/目标设置")]
    [LabelText("玩家对象")]
    [ReadOnly]
    [SerializeField] protected Transform player;
    
    [VerticalGroup("敌人配置/导航系统/导航设置/目标设置")]
    [LabelText("目标位置")]
    [ReadOnly]
    [SerializeField] private Vector3 targetPosition;
    
    [VerticalGroup("敌人配置/导航系统/导航设置/巡逻设置")]
    [LabelText("巡逻起始点")]
    [ReadOnly]
    [SerializeField] private Vector3 patrolStartPoint;
    
    [VerticalGroup("敌人配置/导航系统/导航设置/巡逻设置")]
    [LabelText("巡逻结束点")]
    [ReadOnly]
    [SerializeField] private Vector3 patrolEndPoint;
    
    [VerticalGroup("敌人配置/导航系统/导航设置/巡逻设置")]
    [LabelText("正在巡逻")]
    [ReadOnly]
    [SerializeField] private bool isPatrolling = true;
    
    [VerticalGroup("敌人配置/导航系统/导航设置/巡逻设置")]
    [LabelText("向终点移动")]
    [ReadOnly]
    [SerializeField] private bool movingToEnd = true;
    #endregion
    
    #region 攻击系统
    [FoldoutGroup("敌人配置/攻击系统", expanded: false)]
    [HorizontalGroup("敌人配置/攻击系统/攻击配置")]
    [VerticalGroup("敌人配置/攻击系统/攻击配置/攻击设置")]
    [LabelText("攻击冷却时间")]
    [PropertyRange(0.1f, 10f)]
    [SuffixLabel("秒")]
    [SerializeField] public float attackCooldown = 2f;
    
    [VerticalGroup("敌人配置/攻击系统/攻击配置/攻击设置")]
    [LabelText("上次攻击时间")]
    [ReadOnly]
    [SerializeField] private float lastAttackTime;
    
    [VerticalGroup("敌人配置/攻击系统/攻击配置/攻击设置")]
    [LabelText("玩家图层")]
    [SerializeField] private LayerMask playerLayer = 1;
    #endregion
    
    #region 属性访问器
    [FoldoutGroup("敌人配置/调试信息", expanded: false)]
    [HorizontalGroup("敌人配置/调试信息/信息显示")]
    [VerticalGroup("敌人配置/调试信息/信息显示/属性访问器")]
    [ShowInInspector, ReadOnly, LabelText("敌人名称")]
    public string EnemyName => enemyName;
    [ShowInInspector, ReadOnly, LabelText("敌人类型")]
    protected string enemyType;
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
        // 配置敌人属性
        ConfigureEnemy(this, enemyType);
        // 初始化属性
        currentHealth = maxHealth;
        patrolStartPoint = transform.position;
        patrolEndPoint = transform.position + Vector3.right * 5f; // 默认巡逻距离
    }
    
    private void Start()
    {
        // 查找玩家
        FindPlayer();
        
        // 开始AI行为
        StartCoroutine(AIBehavior());
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        // 更新状态标志
        isAlive = !isDead;
        
        // 更新目标引用
        if (target == null && player != null)
        {
            target = player;
        }
        
        // 检测玩家
        DetectPlayer();
        
        // 更新当前状态
        UpdateCurrentState();
        
        // 更新动画
        UpdateAnimation();
    }
    
    private void FixedUpdate()
    {
        if (isDead) return;
        
        // 处理移动
        HandleMovement();
    }
    #endregion
    
    void ConfigureEnemy(Enemy enemyController, string enemyType)
    {
        if (enemyController == null) return;
        
        // 从EnemySystemConfig获取配置
        var systemConfig = ConfigManager.Instance?.GetEnemySystemConfig();
        if (systemConfig?.wildBoarConfig != null && enemyType == "WildBoar")
        {
            var wildBoarConfig = systemConfig.wildBoarConfig;
            enemyController.maxHealth = wildBoarConfig.health;
            enemyController.currentHealth = wildBoarConfig.health;
            enemyController.attackDamage = wildBoarConfig.attackDamage;
            enemyController.moveSpeed = wildBoarConfig.moveSpeed;
            enemyController.attackRange = wildBoarConfig.attackRange;
            enemyController.detectionRange = wildBoarConfig.detectionRange;
        }
        else
        {
            // 使用默认值
            Debug.LogWarning($"[Enemy] 未找到敌人配置: {enemyType}，使用默认值");
        }
    }
    
    #region 初始化方法
    /// <summary>
    /// 查找玩家
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    /// <summary>
    /// 设置巡逻点
    /// </summary>
    public void SetPatrolPoints(Vector3 startPoint, Vector3 endPoint)
    {
        patrolStartPoint = startPoint;
        patrolEndPoint = endPoint;
        targetPosition = movingToEnd ? patrolEndPoint : patrolStartPoint;
    }
    
    /// <summary>
    /// 设置敌人属性
    /// </summary>
    public void SetEnemyStats(int health, int damage, float speed, float range)
    {
        maxHealth = health;
        currentHealth = health;
        attackDamage = damage;
        moveSpeed = speed;
        attackRange = range;
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
    
    #region AI行为系统
    /// <summary>
    /// AI行为协程
    /// </summary>
    private IEnumerator AIBehavior()
    {
        while (!isDead)
        {
            if (isPlayerInRange && player != null)
            {
                // 追击玩家
                ChasePlayer();
                
                // 检查是否可以攻击
                if (CanAttackPlayer())
                {
                    yield return StartCoroutine(AttackPlayer());
                }
            }
            else
            {
                // 巡逻行为
                Patrol();
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// 检测玩家
    /// </summary>
    private void DetectPlayer()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRange;
    }
    
    /// <summary>
    /// 追击玩家
    /// </summary>
    private void ChasePlayer()
    {
        if (player == null) return;
        
        targetPosition = player.position;
        isPatrolling = false;
        isMoving = true;
    }
    
    /// <summary>
    /// 巡逻行为
    /// </summary>
    private void Patrol()
    {
        isPatrolling = true;
        
        // 检查是否到达目标点
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            // 切换巡逻方向
            movingToEnd = !movingToEnd;
            targetPosition = movingToEnd ? patrolEndPoint : patrolStartPoint;
        }
        
        isMoving = true;
    }
    
    /// <summary>
    /// 处理移动
    /// </summary>
    private void HandleMovement()
    {
        if (!isMoving || isAttacking) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 movement = direction * moveSpeed * Time.fixedDeltaTime;
        
        // 使用Rigidbody2D移动
        if (rb2D != null)
        {
            rb2D.MovePosition(transform.position + movement);
        }
        else
        {
            transform.position += movement;
        }
        
        // 翻转精灵
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    /// <summary>
    /// 检查是否可以攻击玩家
    /// </summary>
    private bool CanAttackPlayer()
    {
        if (player == null || isAttacking) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool inAttackRange = distanceToPlayer <= attackRange;
        bool cooldownReady = Time.time - lastAttackTime >= attackCooldown;
        
        return inAttackRange && cooldownReady;
    }
    
    /// <summary>
    /// 攻击玩家协程
    /// </summary>
    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        isMoving = false;
        
        // 触发攻击事件
        OnAttack?.Invoke(this);
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 等待攻击动画
        yield return new WaitForSeconds(0.5f);
        
        // 造成伤害
        DealDamageToPlayer();
        
        // 记录攻击时间
        lastAttackTime = Time.time;
        
        // 攻击结束
        isAttacking = false;
        
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 对玩家造成伤害
    /// </summary>
    private void DealDamageToPlayer()
    {
        if (player == null) return;
        
        // 检查玩家是否在攻击范围内
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);
        
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // 获取玩家组件并造成伤害
                var playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
                break;
            }
        }
    }
    #endregion
    
    #region 伤害和死亡系统
    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 触发受伤事件
        OnTakeDamage?.Invoke(this, damage);
        
        // 播放受伤动画
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// IDamageable接口实现 - 受到伤害（带击中点和攻击者）
    /// </summary>
    public void TakeDamage(int damage, Vector2 hitPoint, Character attacker)
    {
        // 调用原有的TakeDamage方法保持兼容性
        TakeDamage(damage);
    }
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    public virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isMoving = false;
        isAttacking = false;
        
        // 触发死亡事件
        OnDeath?.Invoke(this);
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 禁用碰撞器
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
        
        // 停止物理
        if (rb2D != null)
        {
            rb2D.velocity = Vector2.zero;
            rb2D.isKinematic = true;
        }
        
        // 开始死亡清理
        StartCoroutine(DeathCleanup());
    }
    
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
    
    #region 动画更新
    /// <summary>
    /// 更新动画状态
    /// </summary>
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 设置动画参数
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
        animator.SetBool("IsPlayerInRange", isPlayerInRange);
        
        // 设置移动速度
        if (rb2D != null)
        {
            animator.SetFloat("MoveSpeed", rb2D.velocity.magnitude);
        }
    }
    
    /// <summary>
    /// 更新动画（公共方法）
    /// </summary>
    public virtual void UpdateAnimations()
    {
        UpdateAnimation();
    }
    
    /// <summary>
    /// 更新当前状态（由Update方法调用）
    /// </summary>
    private void UpdateCurrentState()
    {
        if (isDead)
        {
            currentState = EnemyState.Dead;
        }
        else if (isAttacking)
        {
            currentState = EnemyState.Attack;
        }
        else if (isPlayerInRange)
        {
            currentState = EnemyState.Chase;
        }
        else if (isPatrolling)
        {
            currentState = EnemyState.Patrol;
        }
        else
        {
            currentState = EnemyState.Idle;
        }
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
    /// 设置玩家引用
    /// </summary>
    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
    
    /// <summary>
    /// 设置目标
    /// </summary>
    public void SetTarget(Transform target)
    {
        player = target;
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
    /// 向指定方向移动
    /// </summary>
    public void MoveInDirection(Vector2 direction)
    {
        if (!canMove || isDead) return;
        
        Vector3 movement = direction.normalized * moveSpeed * Time.fixedDeltaTime;
        
        if (rb2D != null)
        {
            rb2D.MovePosition(transform.position + movement);
        }
        else
        {
            transform.position += movement;
        }
        
        // 翻转精灵
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    #endregion
    
    #region 调试方法
    private void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制巡逻路径
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(patrolStartPoint, patrolEndPoint);
        Gizmos.DrawWireSphere(patrolStartPoint, 0.5f);
        Gizmos.DrawWireSphere(patrolEndPoint, 0.5f);
        
        // 绘制目标位置
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
    }
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
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"[PlayerHealth] 玩家受到 {damage} 点伤害，当前生命值: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log("[PlayerHealth] 玩家死亡");
        // 处理玩家死亡逻辑
    }
}