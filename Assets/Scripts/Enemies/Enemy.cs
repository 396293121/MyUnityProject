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
public abstract class Enemy : MonoBehaviour, IDamageable
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
    [SerializeField] protected bool isDead = false;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("正在攻击")]
    [ReadOnly]
    [SerializeField] protected bool isAttacking = false;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/状态标志")]
    [LabelText("正在移动")]
    [ReadOnly]
    [SerializeField] private bool isMoving = false;
    
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
    
    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("动画控制器")]
    public Animator animator;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("精灵渲染器")]
    [Required("需要SpriteRenderer组件")]
    public SpriteRenderer spriteRenderer;
    
    [VerticalGroup("敌人配置/状态控制/状态信息/核心组件")]
    [LabelText("状态机")]
    [InfoBox("敌人状态机系统，负责状态转换和管理")]
    protected EnemyStateMachine stateMachine;
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
    
    // 巡逻相关字段已移至子类实现，基类不再包含具体巡逻逻辑
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
    [SerializeField] protected float lastAttackTime;
    
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
        
        // 获取或添加状态机组件
        stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<EnemyStateMachine>();
        }
        
        // 配置敌人属性
        ConfigureEnemy(this, enemyType);
        // 初始化属性
        currentHealth = maxHealth;
    }
    
    private void Start()
    {
        // 查找玩家
        FindPlayer();
        
        // 注意：AIBehavior协程已被EnemyStateMachine接管，不再启动
        // StartCoroutine(AIBehavior()); // 已移除，由状态机处理
    }
    
    protected virtual void Update()
    {
        if (!isAlive) return;
        
        // 基础的玩家检测（主要是设置目标）
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
                player = target;
            }
        }
        
        // 状态机会处理大部分逻辑，这里只做必要的更新
        // 移除了大部分状态检测逻辑，交给状态机处理
    }
    
    private void FixedUpdate()
    {
        if (isDead) return;
        
        // 注意：HandleMovement已被EnemyStateMachine接管，不再调用
        // HandleMovement(); // 已移除，由状态机处理
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
    // 巡逻点设置方法已移至子类实现
    
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
    // 注意：以下AI行为方法已被EnemyStateMachine接管，保留用于兼容性
    // 实际AI逻辑由EnemyStateMachine处理
    #endregion
    
    #region 伤害和死亡系统
    /// <summary>
    /// 受到伤害
    /// 优化：改进与状态机的健康状态通知
    /// </summary>
    public virtual void TakePlayerDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 通知状态机健康状态变化
        if (stateMachine != null)
        {
            stateMachine.NotifyHealthStateChanged();
        }
        
        // 触发受伤事件
        OnTakeDamage?.Invoke(this, damage);
        
        // 播放受伤动画 - 根据Unity动画控制器参数
        if (animator != null)
        {
            animator.SetTrigger("isHurt"); // 与动画控制器中的isHurt参数一致
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
        TakePlayerDamage(damage);
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
        
        // 播放死亡动画 - 根据Unity动画控制器参数
        if (animator != null)
        {
            animator.SetTrigger("Die"); // 保持原有触发器名称
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
        
        if (rb2D != null)
        {
            // 使用velocity进行移动，这样可以正确显示速度并与物理系统协作
        //    Vector2 targetVelocity = direction.normalized * moveSpeed;
            rb2D.velocity = new Vector2(direction.x * moveSpeed, rb2D.velocity.y);
        }
        else
        {
            // 如果没有Rigidbody2D，使用transform移动作为后备
            Vector3 movement = direction.normalized * moveSpeed * Time.fixedDeltaTime;
            transform.position += movement;
        }
        
        // 翻转精灵
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    /// <summary>
    /// 执行巡逻行为 - 抽象方法，由子类实现具体逻辑
    /// </summary>
    public abstract void ExecutePatrol();

    /// <summary>
    /// 执行追击行为
    /// </summary>
    public virtual void ExecuteChase()
    {
        if (!canMove || isDead) return;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        // 计算朝向玩家的方向
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        
        // 执行移动
        MoveInDirection(directionToPlayer);
    }

    /// <summary>
    /// 执行攻击行为
    /// </summary>
    public virtual void ExecuteAttack()
    {
        if (!canAttack || isDead) return;
        
        // 检查攻击冷却
        if (Time.time - lastAttackTime < attackCooldown) return;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 对玩家造成伤害
        var playerCharacter = player.GetComponent<Character>();
        if (playerCharacter != null)
        {
            int damage = (int)attackDamage;
            playerCharacter.TakeDamage(damage);
        }
        
        // 更新最后攻击时间
        lastAttackTime = Time.time;
        
        // 触发攻击事件
        OnAttack?.Invoke(this);
    }

    /// <summary>
    /// 停止移动 - 只停止X轴移动，保持Y轴速度（重力等）
    /// </summary>
    public virtual void StopMovement()
    {
        if (rb2D != null)
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
        }
    }

    // GetPatrolDirection方法已移至子类实现
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
        
        // 巡逻相关调试绘制已移至子类实现
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
    

    
    private void Die()
    {
        Debug.Log("[PlayerHealth] 敌人死亡");
        // 处理玩家死亡逻辑
    }
}