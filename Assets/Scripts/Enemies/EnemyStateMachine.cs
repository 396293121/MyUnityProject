using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 敌人状态枚举
/// </summary>
public enum EnemyState
{
    Idle,       // 空闲状态
    Patrol,     // 巡逻状态
    Chase,      // 追击状态
    Attack,     // 攻击状态
    Charge,     // 冲撞状态（野猪专用）
    Stun,       // 眩晕状态
    Hurt,       // 受伤状态
    Dead        // 死亡状态
}

/// <summary>
/// 敌人状态转换条件
/// </summary>
[System.Serializable]
public class EnemyStateTransition
{
    public EnemyState fromState;
    public EnemyState toState;
    public Func<bool> condition;
    public string conditionDescription; // 用于调试显示
    
    public EnemyStateTransition(EnemyState from, EnemyState to, Func<bool> condition, string description = "")
    {
        this.fromState = from;
        this.toState = to;
        this.condition = condition;
        this.conditionDescription = description;
    }
}

/// <summary>
    /// 敌人状态机系统
    /// 参照玩家状态机设计，提供清晰的状态转换逻辑和性能优化
    /// 优化目标：
    /// 1. 事件驱动的状态转换，减少不必要的Update检查
    /// 2. 智能条件缓存，避免重复计算
    /// 3. 清晰的状态管理和解耦合设计
    /// 4. 将状态判断逻辑集中到状态机中
    /// </summary>
    [System.Serializable]
    public class EnemyStateMachine : MonoBehaviour
    {
        [Header("状态机核心")]
        [SerializeField, ReadOnly] private EnemyState currentState = EnemyState.Idle;
        [SerializeField, ReadOnly] private EnemyState previousState = EnemyState.Idle;
        
        [Header("状态标志 - 运行时更新")]
        [SerializeField, ReadOnly] private bool isPlayerInRange = false;
        [SerializeField, ReadOnly] private bool isPlayerInAttackRange = false;
        [SerializeField, ReadOnly] private bool isHealthLow = false;
        [SerializeField, ReadOnly] private bool isDead = false;
        [SerializeField, ReadOnly] private bool isStunned = false;
        [SerializeField, ReadOnly] private bool isCharging = false;
        [SerializeField, ReadOnly] private bool isAttacking = false;
        [SerializeField, ReadOnly] private bool isHurt = false;
        
        [Header("能力标志")]
        [SerializeField, ReadOnly] private bool canAttack = true;
        [SerializeField, ReadOnly] private bool canCharge = true;
        [SerializeField, ReadOnly] private bool canMove = true;
        
        [Header("调试选项")]
        [SerializeField] private bool enableDebugLogs = false;
        [SerializeField] private bool showStateTransitions = true;
        
        // 状态转换规则
        private List<EnemyStateTransition> stateTransitions = new List<EnemyStateTransition>();
        
        // 组件引用
        private Enemy enemyController;
        private WildBoar wildBoarController;
        private Animator animator;
        private Rigidbody2D rb;
        
        // 事件系统 - 减少Update调用
        public event Action<EnemyState, EnemyState> OnStateChanged;
        public event Action<bool> OnPlayerRangeChanged;
        public event Action<bool> OnAttackRangeChanged;
        public event Action<bool> OnHealthChanged;
        
        // 性能优化 - 条件缓存和事件驱动
        private Dictionary<string, bool> cachedConditions = new Dictionary<string, bool>();
        private bool conditionsDirty = true;
        private float lastConditionCheckTime = 0f;
        private const float CONDITION_CHECK_INTERVAL = 0.05f; // 20fps检查频率，比玩家状态机稍低
        private bool needsConditionUpdate = false;
        
        // 事件驱动优化：关键状态变化标记
        private bool playerRangeChanged = false;
        private bool attackRangeChanged = false;
        private bool healthStateChanged = false;
        private bool attackStateChanged = false;
        private bool chargeStateChanged = false;
        private bool stunStateChanged = false;
        
        // 状态进入时间记录
        private Dictionary<EnemyState, float> stateEnterTimes = new Dictionary<EnemyState, float>();
    
    // 公共属性访问器
    public EnemyState CurrentState => currentState;
    public EnemyState PreviousState => previousState;
    public bool IsPlayerInRange => isPlayerInRange;
    public bool IsPlayerInAttackRange => isPlayerInAttackRange;
    public bool IsHealthLow => isHealthLow;
    public bool IsDead => isDead;
    public bool IsStunned => isStunned;
    public bool IsCharging => isCharging;
    public bool IsAttacking => isAttacking;
    public bool IsHurt => isHurt;
    public bool CanAttack => canAttack;
    public bool CanCharge => canCharge;
    public bool CanMove => canMove;

    private void Awake()
    {
        // 获取组件引用
        enemyController = GetComponent<Enemy>();
        wildBoarController = GetComponent<WildBoar>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        // 初始化状态转换规则
        InitializeStateTransitions();
        
        if (enableDebugLogs)
            Debug.Log($"[EnemyStateMachine] 初始化完成 - {gameObject.name}");
    }

    private void Start()
    {
        // 设置初始状态
        ChangeState(EnemyState.Idle);
        
        // 订阅事件
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// 订阅相关事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (enemyController != null)
        {
            // 这里可以订阅Enemy的事件，如果有的话
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // 清理事件订阅
    }

    /// <summary>
    /// 初始化状态转换规则
    /// 参照玩家状态机的设计，将所有状态转换逻辑集中管理
    /// </summary>
    private void InitializeStateTransitions()
    {
        stateTransitions.Clear();
        
        // 从空闲状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Idle, EnemyState.Patrol, () => ShouldStartPatrol(), "开始巡逻"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Idle, EnemyState.Chase, () => ShouldChase(), "发现玩家"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Idle, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        // 从巡逻状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Patrol, EnemyState.Idle, () => ShouldReturnToIdle(), "返回空闲"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Patrol, EnemyState.Chase, () => ShouldChase(), "发现玩家"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Patrol, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        // 从追击状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Chase, EnemyState.Attack, () => ShouldAttack(), "进入攻击"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Chase, EnemyState.Charge, () => ShouldCharge(), "开始冲撞"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Chase, EnemyState.Patrol, () => ShouldReturnToPatrol(), "失去目标"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Chase, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        // 从攻击状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Attack, EnemyState.Chase, () => ShouldChase(), "继续追击"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Attack, EnemyState.Idle, () => ShouldReturnToIdle(), "攻击结束"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Attack, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        // 从冲撞状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Charge, EnemyState.Stun, () => ShouldStun(), "冲撞眩晕"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Charge, EnemyState.Chase, () => ShouldChase(), "冲撞结束"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Charge, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        // 从眩晕状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Stun, EnemyState.Chase, () => ShouldChase(), "眩晕恢复"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Stun, EnemyState.Idle, () => ShouldReturnToIdle(), "眩晕恢复"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Stun, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        // 从受伤状态的转换
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Hurt, EnemyState.Chase, () => ShouldChase(), "受伤后追击"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Hurt, EnemyState.Idle, () => ShouldReturnToIdle(), "受伤恢复"));
        stateTransitions.Add(new EnemyStateTransition(EnemyState.Hurt, EnemyState.Dead, () => ShouldDie(), "死亡"));
        
        if (enableDebugLogs)
            Debug.Log($"[EnemyStateMachine] 初始化了 {stateTransitions.Count} 个状态转换规则");
    }

    /// <summary>
    /// 优化的Update方法 - 事件驱动，减少不必要的检查
    /// </summary>
    private void Update()
    {
        if (isDead) return; // 死亡状态不需要更新
        
        // 处理攻击逻辑
        HandleAttackLogic();
        
        // 智能状态转换检查：只在必要时检查
        if (ShouldCheckStateTransitions())
        {
            // 更新状态标志（检测变化）
            UpdateStateFlags();
            
            // 检查状态转换
            CheckStateTransitions();
            
            // 更新动画参数
            UpdateAnimatorParameters();
            
            // 重置标记
            conditionsDirty = false;
            lastConditionCheckTime = Time.time;
            ResetEventFlags();
        }
    }

    /// <summary>
    /// FixedUpdate - 处理物理相关的移动逻辑
    /// </summary>
    private void FixedUpdate()
    {
        if (isDead || !canMove) return;
        
        // 根据当前状态执行相应的行为逻辑
        ExecuteStateMovement();
    }

    /// <summary>
    /// Update中处理攻击逻辑
    /// </summary>
    private void HandleAttackLogic()
    {
        if (currentState == EnemyState.Attack && canAttack)
        {
            // 直接调用Enemy组件的攻击方法
            if (enemyController != null)
            {
                enemyController.ExecuteAttack();
            }
        }
    }

    /// <summary>
    /// 根据当前状态执行移动逻辑
    /// </summary>
    private void ExecuteStateMovement()
    {
        if (enemyController == null) return;
        
        switch (currentState)
        {
            case EnemyState.Patrol:
                enemyController.ExecutePatrol();
                break;
                
            case EnemyState.Chase:
                enemyController.ExecuteChase();
                break;
                
            case EnemyState.Idle:
            case EnemyState.Attack:
            case EnemyState.Stun:
            case EnemyState.Dead:
                // 这些状态下停止移动
                enemyController.StopMovement();
                break;
                
            case EnemyState.Charge:
                // 冲撞状态由野猪自己处理，状态机不干预
                break;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyStateMachine] {gameObject.name} 执行状态: {currentState}");
        }
    }

    /// <summary>
    /// 智能判断是否需要检查状态转换
    /// 基于事件驱动和定期检查的混合策略
    /// </summary>
    private bool ShouldCheckStateTransitions()
    {
        // 如果有关键事件发生，立即检查
        if (playerRangeChanged || attackRangeChanged || healthStateChanged || 
            attackStateChanged || chargeStateChanged || stunStateChanged)
        {
            if (enableDebugLogs)
                Debug.Log($"[EnemyStateMachine] 事件驱动触发状态检查 - {gameObject.name}");
            return true;
        }
        
        // 如果条件缓存已脏，需要检查
        if (conditionsDirty)
        {
            return true;
        }
        
        // 定期检查（降低频率）
        if (Time.time - lastConditionCheckTime >= CONDITION_CHECK_INTERVAL)
        {
            return true;
        }
        
        // 特殊状态需要更频繁的检查
        if (currentState == EnemyState.Attack || currentState == EnemyState.Charge || 
            currentState == EnemyState.Hurt || currentState == EnemyState.Stun)
        {
            return true;
        }
        
        // 其他情况跳过检查
        return false;
    }

    /// <summary>
    /// 重置事件标记
    /// </summary>
    private void ResetEventFlags()
    {
        playerRangeChanged = false;
        attackRangeChanged = false;
        healthStateChanged = false;
        attackStateChanged = false;
        chargeStateChanged = false;
        stunStateChanged = false;
    }

    /// <summary>
    /// 更新状态标志 - 集中调用各种状态检测
    /// </summary>
    private void UpdateStateFlags()
    {
        // 执行集中的状态检测
        PerformPlayerDetection();
        PerformHealthCheck();
        PerformAttackStateCheck();
        
        // 如果是野猪，执行野猪特殊状态检测
        if (wildBoarController != null)
        {
            PerformWildBoarStateCheck();
        }
    }

    /// <summary>
    /// 检查状态转换
    /// </summary>
    private void CheckStateTransitions()
    {
        foreach (var transition in stateTransitions)
        {
            if (transition.fromState == currentState && transition.condition())
            {
                ChangeState(transition.toState);
                
                if (enableDebugLogs)
                    Debug.Log($"[EnemyStateMachine] 状态转换: {transition.fromState} -> {transition.toState} ({transition.conditionDescription})");
                
                break; // 只执行第一个满足条件的转换
            }
        }
    }

    /// <summary>
    /// 改变状态
    /// </summary>
    public void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        EnemyState oldState = currentState;
        
        // 退出当前状态
        ExitState(currentState);
        
        // 更新状态
        previousState = currentState;
        currentState = newState;
        
        // 进入新状态
        EnterState(newState);
        
        // 触发状态改变事件
        OnStateChanged?.Invoke(oldState, newState);
        
        // if (showStateTransitions)
        //     Debug.Log($"[EnemyStateMachine] {gameObject.name}: {oldState} -> {newState}");
    }

    /// <summary>
    /// 进入状态
    /// </summary>
    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                canMove = true;
                canAttack = true;
                break;
                
            case EnemyState.Patrol:
                canMove = true;
                canAttack = true;
                break;
                
            case EnemyState.Chase:
                canMove = true;
                canAttack = true;
                break;
                
            case EnemyState.Attack:
                isAttacking = true;
                canMove = false;
                break;
                
            case EnemyState.Charge:
                isCharging = true;
                canMove = false;
                canAttack = false;
                break;
                
            case EnemyState.Stun:
                isStunned = true;
                canMove = false;
                canAttack = false;
                break;
                
            case EnemyState.Hurt:
                isHurt = true;
                break;
                
            case EnemyState.Dead:
                isDead = true;
                canMove = false;
                canAttack = false;
                canCharge = false;
                break;
        }
    }

    /// <summary>
    /// 退出状态
    /// </summary>
    private void ExitState(EnemyState state)
    {
        switch (state)
        {
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
    /// 更新动画参数 - 根据Unity动画控制器参数
    /// </summary>
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        
        // 计算速度值，确保能够触发BlendTree中的run动画（阈值为0.2）
        float currentSpeed = 0f;
        if (rb != null)
        {
            currentSpeed = rb.velocity.magnitude;
        }
        
        // 根据状态调整速度值，确保动画正确播放
        if (currentState == EnemyState.Chase || currentState == EnemyState.Patrol)
        {
            // 如果在移动状态但速度很小，使用移动速度作为参考
            if (currentSpeed < 0.1f && canMove)
            {
                if (enemyController != null)
                {
                    currentSpeed = enemyController.moveSpeed * 0.1f; // 调整比例以匹配BlendTree阈值
                }
            }
        }
        
        // 根据WildBoarController.controller中的参数设置动画
        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("IsAlive", !isDead);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsCharging", isCharging);
        animator.SetBool("IsStunned", isStunned);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[EnemyStateMachine] 动画参数更新 - Speed: {currentSpeed}, State: {currentState}");
        }
    }

    #region 状态转换条件方法
    
    /// <summary>
    /// 是否应该开始巡逻
    /// </summary>
    private bool ShouldStartPatrol()
    {
        return currentState == EnemyState.Idle && 
               !isPlayerInRange && 
               canMove && 
               !isDead;
    }

    /// <summary>
    /// 是否应该追击玩家
    /// </summary>
    private bool ShouldChase()
    {
        return isPlayerInRange && 
               !isPlayerInAttackRange && 
               canMove && 
               !isDead && 
               !isStunned;
    }

    /// <summary>
    /// 是否应该攻击
    /// </summary>
    private bool ShouldAttack()
    {
        return isPlayerInAttackRange && 
               canAttack && 
               !isDead && 
               !isStunned && 
               !isCharging;
    }

    /// <summary>
    /// 是否应该冲撞（野猪专用）
    /// </summary>
    private bool ShouldCharge()
    {
        return wildBoarController != null && 
               isPlayerInRange && 
               !isPlayerInAttackRange && // 不在攻击范围内但在检测范围内时才冲撞
               canCharge && 
               wildBoarController.CanCharge && 
               !isDead && 
               !isStunned &&
               !isAttacking; // 不在攻击状态时才能冲撞
    }

    /// <summary>
    /// 是否应该眩晕
    /// </summary>
    private bool ShouldStun()
    {
        return isStunned || 
               (wildBoarController != null && wildBoarController.IsStunned);
    }

    /// <summary>
    /// 是否应该死亡
    /// </summary>
    private bool ShouldDie()
    {
        return isDead || (enemyController != null && enemyController.CurrentHealth <= 0);
    }

    /// <summary>
    /// 是否应该返回空闲状态
    /// </summary>
    private bool ShouldReturnToIdle()
    {
        return !isPlayerInRange && 
               !isAttacking && 
               !isCharging && 
               !isStunned && 
               !isDead;
    }

    /// <summary>
    /// 是否应该返回巡逻状态
    /// </summary>
    private bool ShouldReturnToPatrol()
    {
        return !isPlayerInRange && 
               !isAttacking && 
               !isCharging && 
               !isStunned && 
               !isDead && 
               canMove;
    }

    #endregion

    #region 外部通知方法 - 事件驱动优化

    /// <summary>
    /// 集中的玩家检测逻辑 - 从Enemy类移到状态机中
    /// </summary>
    public void PerformPlayerDetection()
    {
        if (enemyController == null) return;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // 检测范围变化
        bool newPlayerInRange = distanceToPlayer <= enemyController.detectionRange;
        if (newPlayerInRange != isPlayerInRange)
        {
            NotifyPlayerRangeChanged(newPlayerInRange);
        }
        
        // 攻击范围变化
        bool newPlayerInAttackRange = distanceToPlayer <= enemyController.attackRange;
        if (newPlayerInAttackRange != isPlayerInAttackRange)
        {
            NotifyAttackRangeChanged(newPlayerInAttackRange);
        }
    }

    /// <summary>
    /// 集中的健康状态检测逻辑 - 从Enemy类移到状态机中
    /// </summary>
    public void PerformHealthCheck()
    {
        if (enemyController == null) return;
        
        bool newIsDead = enemyController.currentHealth <= 0;
        bool newIsHealthLow = enemyController.currentHealth <= (enemyController.maxHealth * 0.3f);
        
        if (newIsDead != isDead || newIsHealthLow != isHealthLow)
        {
            isDead = newIsDead;
            isHealthLow = newIsHealthLow;
            NotifyHealthStateChanged();
        }
    }

    /// <summary>
    /// 集中的野猪特殊状态检测逻辑 - 从WildBoar类移到状态机中
    /// </summary>
    public void PerformWildBoarStateCheck()
    {
        if (wildBoarController == null) return;
        
        bool newIsCharging = wildBoarController.IsCharging;
        bool newIsStunned = wildBoarController.IsStunned;
        bool newIsEnraged = wildBoarController.IsEnraged;
        
        if (newIsCharging != isCharging)
        {
            isCharging = newIsCharging;
            NotifyChargeStateChanged();
        }
        
        if (newIsStunned != isStunned)
        {
            isStunned = newIsStunned;
            NotifyStunStateChanged();
        }
        
        // 检查狂暴状态变化
        if (newIsEnraged && !isHealthLow)
        {
            // 如果野猪进入狂暴状态，更新健康状态标记
            isHealthLow = true;
            NotifyHealthStateChanged();
        }
    }

    /// <summary>
    /// 集中的攻击状态检测逻辑
    /// </summary>
    public void PerformAttackStateCheck()
    {
        if (enemyController == null) return;
        
        // 这里可以添加攻击状态的检测逻辑
        // 例如检查动画状态、攻击冷却等
        bool newIsAttacking = false;
        
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            newIsAttacking = stateInfo.IsName("Attack") || stateInfo.IsName("Charge");
        }
        
        if (newIsAttacking != isAttacking)
        {
            isAttacking = newIsAttacking;
            NotifyAttackStateChanged();
        }
    }

    /// <summary>
    /// 通知玩家范围状态变化
    /// </summary>
    public void NotifyPlayerRangeChanged(bool inRange)
    {
        if (isPlayerInRange != inRange)
        {
            isPlayerInRange = inRange;
            playerRangeChanged = true;
            needsConditionUpdate = true;
            OnPlayerRangeChanged?.Invoke(inRange);
        }
    }

    /// <summary>
    /// 通知攻击范围状态变化
    /// </summary>
    public void NotifyAttackRangeChanged(bool inAttackRange)
    {
        if (isPlayerInAttackRange != inAttackRange)
        {
            isPlayerInAttackRange = inAttackRange;
            attackRangeChanged = true;
            needsConditionUpdate = true;
            OnAttackRangeChanged?.Invoke(inAttackRange);
        }
    }

    /// <summary>
    /// 通知健康状态变化
    /// </summary>
    public void NotifyHealthStateChanged()
    {
        healthStateChanged = true;
        needsConditionUpdate = true;
        OnHealthChanged?.Invoke(isHealthLow);
    }

    /// <summary>
    /// 通知攻击状态变化
    /// </summary>
    public void NotifyAttackStateChanged()
    {
        attackStateChanged = true;
        needsConditionUpdate = true;
    }

    /// <summary>
    /// 通知冲撞状态变化
    /// </summary>
    public void NotifyChargeStateChanged()
    {
        chargeStateChanged = true;
        needsConditionUpdate = true;
    }

    /// <summary>
    /// 通知眩晕状态变化
    /// </summary>
    public void NotifyStunStateChanged()
    {
        stunStateChanged = true;
        needsConditionUpdate = true;
    }

    #endregion
}