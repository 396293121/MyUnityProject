using UnityEngine;
using System.Collections;

/// <summary>
/// 敌人基类 - 所有敌人的基础类
/// 从原Phaser项目的Enemy.js迁移而来
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [Header("基础属性")]
    public int level = 1;
    public int maxHealth = 100;
    public int currentHealth;
    public int physicalAttack = 10;
    public int magicAttack = 5;
    public int defense = 5;
    public int magicDefense = 3;
    public float speed = 3f;
    
    [Header("AI行为")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float patrolRange = 5f;
    public float attackCooldown = 2f;
    
    [Header("掉落物品")]
    public int expReward = 50;
    public GameObject[] dropItems;
    public float[] dropChances;
    
    [Header("状态")]
    public bool isAlive = true;
    public bool canMove = true;
    public bool canAttack = true;
    public bool isAggressive = false;
    
    // 组件引用
    protected Rigidbody2D rb;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D enemyCollider;
    
    // AI状态
    protected Transform target;
    protected Vector2 originalPosition;
    protected Vector2 patrolTarget;
    protected bool isPatrolling = false;
    protected bool isChasing = false;
    protected bool isAttacking = false;
    protected float lastAttackTime = 0f;
    
    // 状态机
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }
    
    protected EnemyState currentState = EnemyState.Idle;
    
    // 事件
    public System.Action<EnemyBase> OnEnemyDeath;
    public System.Action<EnemyBase, int> OnEnemyTakeDamage;
    public System.Action<EnemyBase, Transform> OnEnemyDetectPlayer;
    public System.Action OnDeath;
    public System.Action OnAttack;
    
    // 兼容性属性
    public int damage => physicalAttack;
    
    protected virtual void Awake()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        
        // 初始化属性
        currentHealth = maxHealth;
        originalPosition = transform.position;
        
        // 设置随机巡逻目标
        SetRandomPatrolTarget();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 创建完成 - 生命值: {currentHealth}/{maxHealth}");
        }
    }
    
    protected virtual void Start()
    {
        // 开始AI行为
        StartCoroutine(AIBehavior());
    }
    
    protected virtual void Update()
    {
        if (!isAlive) return;
        
        // 更新AI状态
        UpdateAIState();
        
        // 更新动画
        UpdateAnimations();
    }
    
    /// <summary>
    /// AI行为主循环
    /// </summary>
    protected virtual IEnumerator AIBehavior()
    {
        while (isAlive)
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    HandleIdleState();
                    break;
                case EnemyState.Patrol:
                    HandlePatrolState();
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Attack:
                    HandleAttackState();
                    break;
                case EnemyState.Dead:
                    HandleDeathState();
                    break;
            }
            
            yield return new WaitForSeconds(0.1f); // AI更新频率
        }
    }
    
    /// <summary>
    /// 更新AI状态
    /// </summary>
    protected virtual void UpdateAIState()
    {
        if (!isAlive) return;
        
        // 检测玩家
        DetectPlayer();
        
        // 根据当前状态执行相应逻辑
        switch (currentState)
        {
            case EnemyState.Idle:
                if (ShouldStartPatrol())
                {
                    ChangeState(EnemyState.Patrol);
                }
                break;
                
            case EnemyState.Patrol:
                if (target != null)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
                
            case EnemyState.Chase:
                if (target == null || Vector2.Distance(transform.position, target.position) > detectionRange * 1.5f)
                {
                    ChangeState(EnemyState.Patrol);
                }
                else if (Vector2.Distance(transform.position, target.position) <= attackRange)
                {
                    ChangeState(EnemyState.Attack);
                }
                break;
                
            case EnemyState.Attack:
                if (target == null || Vector2.Distance(transform.position, target.position) > attackRange * 1.2f)
                {
                    ChangeState(EnemyState.Chase);
                }
                break;
        }
    }
    
    /// <summary>
    /// 检测玩家
    /// </summary>
    protected virtual void DetectPlayer()
    {
        if (!isAlive || target != null) return;
        
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= detectionRange)
            {
                target = player.transform;
                OnEnemyDetectPlayer?.Invoke(this, target);
                
                if (GameManager.Instance != null && GameManager.Instance.debugMode)
                {
                    Debug.Log($"[Enemy] {gameObject.name} 发现玩家，距离: {distance:F1}");
                }
            }
        }
    }
    
    /// <summary>
    /// 处理空闲状态
    /// </summary>
    protected virtual void HandleIdleState()
    {
        // 停止移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    /// <summary>
    /// 处理巡逻状态
    /// </summary>
    protected virtual void HandlePatrolState()
    {
        if (!canMove) return;
        
        // 移动到巡逻目标
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        MoveInDirection(direction);
        
        // 检查是否到达巡逻目标
        if (Vector2.Distance(transform.position, patrolTarget) < 0.5f)
        {
            SetRandomPatrolTarget();
        }
    }
    
    /// <summary>
    /// 处理追击状态
    /// </summary>
    protected virtual void HandleChaseState()
    {
        if (!canMove || target == null) return;
        
        // 移动向玩家
        Vector2 direction = (target.position - transform.position).normalized;
        MoveInDirection(direction);
    }
    
    /// <summary>
    /// 处理攻击状态
    /// </summary>
    protected virtual void HandleAttackState()
    {
        if (!canAttack || target == null) return;
        
        // 检查攻击冷却
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }
    
    /// <summary>
    /// 处理死亡状态
    /// </summary>
    protected virtual void HandleDeathState()
    {
        // 停止所有移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        // 禁用碰撞器
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }
    
    /// <summary>
    /// 改变状态
    /// </summary>
    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        EnemyState oldState = currentState;
        currentState = newState;
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 状态改变: {oldState} -> {newState}");
        }
    }
    
    /// <summary>
    /// 在指定方向移动
    /// </summary>
    protected virtual void MoveInDirection(Vector2 direction)
    {
        if (!canMove || rb == null) return;
        
        rb.velocity = direction * speed;
        
        // 翻转精灵
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    /// <summary>
    /// 设置随机巡逻目标
    /// </summary>
    protected virtual void SetRandomPatrolTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRange;
        patrolTarget = originalPosition + randomOffset;
    }
    
    /// <summary>
    /// 是否应该开始巡逻
    /// </summary>
    protected virtual bool ShouldStartPatrol()
    {
        return Random.Range(0f, 1f) < 0.3f; // 30%概率开始巡逻
    }
    
    /// <summary>
    /// 执行攻击
    /// </summary>
    protected virtual void PerformAttack()
    {
        if (!canAttack || target == null) return;
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 对玩家造成伤害
        var playerCharacter = target.GetComponent<Character>();
        if (playerCharacter != null)
        {
            playerCharacter.TakeDamage(physicalAttack);
        }
        
        // 触发攻击事件
        OnAttack?.Invoke();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 攻击玩家，伤害: {physicalAttack}");
        }
    }
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        // 计算实际伤害（考虑防御力）
        int actualDamage = Mathf.Max(1, damage - defense);
        currentHealth -= actualDamage;
        
        // 触发受伤事件
        OnEnemyTakeDamage?.Invoke(this, actualDamage);
        
        // 播放受伤动画
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 受到伤害: {actualDamage}, 剩余生命值: {currentHealth}/{maxHealth}");
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 死亡
    /// </summary>
    protected virtual void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        canMove = false;
        canAttack = false;
        
        ChangeState(EnemyState.Dead);
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 给予玩家经验值
        GiveExperienceToPlayer();
        
        // 掉落物品
        DropItems();
        
        // 触发死亡事件
        OnEnemyDeath?.Invoke(this);
        OnDeath?.Invoke();
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Enemy] {gameObject.name} 死亡");
        }
        
        // 延迟销毁
        StartCoroutine(DestroyAfterDelay(3f));
    }
    
    /// <summary>
    /// 给予玩家经验值
    /// </summary>
    protected virtual void GiveExperienceToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerCharacter = player.GetComponent<Character>();
            if (playerCharacter != null)
            {
                playerCharacter.GainExperience(expReward);
            }
        }
    }
    
    /// <summary>
    /// 掉落物品
    /// </summary>
    protected virtual void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;
        
        for (int i = 0; i < dropItems.Length; i++)
        {
            if (dropItems[i] != null && i < dropChances.Length)
            {
                if (Random.Range(0f, 1f) <= dropChances[i])
                {
                    // 在敌人位置生成掉落物品
                    Vector2 dropPosition = (Vector2)transform.position + Random.insideUnitCircle * 0.5f;
                    Instantiate(dropItems[i], dropPosition, Quaternion.identity);
                    
                    if (GameManager.Instance != null && GameManager.Instance.debugMode)
                    {
                        Debug.Log($"[Enemy] {gameObject.name} 掉落物品: {dropItems[i].name}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 延迟销毁
    /// </summary>
    protected virtual IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 更新动画
    /// </summary>
    protected virtual void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 设置动画参数
        animator.SetBool("IsMoving", rb != null && rb.velocity.magnitude > 0.1f);
        animator.SetBool("IsAlive", isAlive);
        animator.SetBool("IsChasing", currentState == EnemyState.Chase);
        animator.SetBool("IsAttacking", currentState == EnemyState.Attack);
    }
    
    /// <summary>
    /// 设置敌人等级
    /// </summary>
    public virtual void SetLevel(int newLevel)
    {
        level = newLevel;
        // 根据等级调整属性
        maxHealth = 100 + (level - 1) * 20;
        currentHealth = maxHealth;
        physicalAttack = 10 + (level - 1) * 3;
        defense = 5 + (level - 1) * 2;
    }
    
    /// <summary>
    /// 设置目标
    /// </summary>
    public virtual void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // 可写的攻击伤害属性
    public int attackDamage
    {
        get { return physicalAttack; }
        set { physicalAttack = value; }
    }
    
    // 可写的移动速度属性
    public float moveSpeed
    {
        get { return speed; }
        set { speed = value; }
    }
    
    /// <summary>
    /// 在Scene视图中绘制检测范围
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 绘制巡逻范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originalPosition, patrolRange);
    }
}