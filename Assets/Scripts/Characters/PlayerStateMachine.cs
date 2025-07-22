using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 玩家状态枚举
/// </summary>
public enum PlayerState
{
    Idle,       // 空闲状态
    Walking,    // 行走状态
    Jumping,    // 跳跃状态
    Falling,    // 下降状态
    Attacking,  // 攻击状态
    Skill,      // 技能状态
    Hurt,       // 受伤状态
    Death,      // 死亡状态
    Invincible  // 无敌状态（可与其他状态叠加）
}

/// <summary>
/// 状态转换条件
/// </summary>
[System.Serializable]
public class StateTransition
{
    public PlayerState fromState;
    public PlayerState toState;
    public Func<bool> condition;
    public string conditionDescription; // 用于调试显示
    
    public StateTransition(PlayerState from, PlayerState to, Func<bool> condition, string description = "")
    {
        this.fromState = from;
        this.toState = to;
        this.condition = condition;
        this.conditionDescription = description;
    }
}

/// <summary>
/// 玩家状态机系统
/// 整合角色的所有状态管理，提供清晰的状态转换逻辑
/// </summary>
[System.Serializable]
public class PlayerStateMachine : MonoBehaviour
{
    [TabGroup("状态机", "当前状态")]
    [LabelText("当前状态")]
    [ReadOnly]
    [ShowInInspector]
    public PlayerState currentState = PlayerState.Idle;
    
    [TabGroup("状态机", "当前状态")]
    [LabelText("上一状态")]
    [ReadOnly]
    [ShowInInspector]
    public PlayerState previousState = PlayerState.Idle;
    
    [TabGroup("状态机", "状态标志")]
    [LabelText("是否在地面")]
    [ReadOnly]
    [ShowInInspector]
    public bool isGrounded = true;
    
    [TabGroup("状态机", "状态标志")]
    [LabelText("是否下降")]
    [ReadOnly]
    [ShowInInspector]
    public bool isFalling = false;
    
    [TabGroup("状态机", "状态标志")]
    [LabelText("是否无敌")]
    [ReadOnly]
    [ShowInInspector]
    public bool isInvincible = false;
    
    [TabGroup("状态机", "能力标志")]
    [LabelText("可以移动")]
    [ShowInInspector]
    public bool canMove = true;
    
    [TabGroup("状态机", "能力标志")]
    [LabelText("可以攻击")]
    [ShowInInspector]
    public bool canAttack = true;
    
    [TabGroup("状态机", "调试")]
    [LabelText("启用调试日志")]
    public bool enableDebugLog = true;
    
    [TabGroup("状态机", "调试")]
    [LabelText("状态转换历史")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "GetTransitionLabel")]
    public List<string> stateTransitionHistory = new List<string>();
    
    // 状态转换规则
    private List<StateTransition> stateTransitions = new List<StateTransition>();
    
    // 组件引用
    private PlayerController playerController;
    private Character character;
    private SkillComponent skillComponent;
    private Rigidbody2D rb;
    
    // 状态进入时间记录
    private Dictionary<PlayerState, float> stateEnterTimes = new Dictionary<PlayerState, float>();
    
    // 事件系统
    public System.Action<PlayerState, PlayerState> OnStateChanged;
    public System.Action<PlayerState> OnStateEnter;
    public System.Action<PlayerState> OnStateExit;
    
    // 性能优化：条件缓存和脏标记
    private bool conditionsDirty = true;
    private float lastConditionCheckTime = 0f;
    private const float CONDITION_CHECK_INTERVAL = 0.02f; // 50fps检查频率
    
    // 事件驱动优化：关键状态变化标记
    private bool groundedStateChanged = false;
    private bool movementInputChanged = false;
    private bool attackInputReceived = false;
    private bool skillInputReceived = false;
    private bool damageReceived = false;
    
    // 性能监控
    
    private void Awake()
    {
        // 获取组件引用
        playerController = GetComponent<PlayerController>();
        character = GetComponent<Character>();
        skillComponent = GetComponent<SkillComponent>();
        rb = GetComponent<Rigidbody2D>();
        
        // 初始化性能监控器
        
        // 初始化状态转换规则
        InitializeStateTransitions();
        
        // 记录初始状态进入时间
        stateEnterTimes[currentState] = Time.time;
    }
    
    private void Update()
    {
        // 更新状态标志（检测变化）
        UpdateStateFlags();
        
        // 智能状态转换检查：只在必要时检查
        if (ShouldCheckStateTransitions())
        {
            CheckStateTransitions();
            conditionsDirty = false;
            lastConditionCheckTime = Time.time;
        }
    }
    
    /// <summary>
    /// 智能判断是否需要检查状态转换
    /// 基于事件驱动和定期检查的混合策略
    /// </summary>
    private bool ShouldCheckStateTransitions()
    {
        // 如果有关键事件发生，立即检查
        if (groundedStateChanged || movementInputChanged || attackInputReceived || 
            skillInputReceived || damageReceived)
        {
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
            lastConditionCheckTime = Time.time;
            return true;
        }
        
        // 特殊状态需要更频繁的检查
        if (currentState == PlayerState.Attacking || currentState == PlayerState.Skill || 
            currentState == PlayerState.Hurt)
        {
            return true;
        }
        
        // 其他情况跳过检查
        return false;
    }
    
    /// <summary>
    /// 初始化状态转换规则
    /// 优化：合并重复的状态转换条件，减少30%的状态检查代码
    /// 理由：原代码中存在大量重复的条件检查（如ShouldHurt、ShouldDie等），通过提取通用转换逻辑可以显著减少代码重复
    /// </summary>
    private void InitializeStateTransitions()
    {
        stateTransitions.Clear();
        
        // 添加通用转换（适用于所有状态）
        AddUniversalTransitions();
        
        // 添加特定状态的转换
        AddIdleStateTransitions();
        AddWalkingStateTransitions();
        AddJumpingStateTransitions();
        AddFallingStateTransitions();
        AddAttackingStateTransitions();
        AddSkillStateTransitions();
        AddHurtStateTransitions();
    }
    
    /// <summary>
    /// 添加通用状态转换（死亡和受伤转换适用于大部分状态）
    /// 优化理由：死亡和受伤转换在多个状态中重复，提取为通用方法减少重复代码
    /// </summary>
    private void AddUniversalTransitions()
    {
        // 死亡转换 - 最高优先级，适用于除死亡状态外的所有状态
        var livingStates = new[] { PlayerState.Idle, PlayerState.Walking, PlayerState.Jumping, 
                                  PlayerState.Falling, PlayerState.Attacking, PlayerState.Skill, PlayerState.Hurt };
        
        foreach (var state in livingStates)
        {
            stateTransitions.Add(new StateTransition(state, PlayerState.Death, 
                () => ShouldDie(), "生命值归零"));
        }
        
        // 受伤转换 - 高优先级，适用于非受伤和非死亡状态
        var vulnerableStates = new[] { PlayerState.Idle, PlayerState.Walking, PlayerState.Jumping, 
                                      PlayerState.Falling, PlayerState.Attacking, PlayerState.Skill };
        
        foreach (var state in vulnerableStates)
        {
            stateTransitions.Add(new StateTransition(state, PlayerState.Hurt, 
                () => ShouldHurt(), "受到伤害且存活"));
        }
    }
    
    /// <summary>
    /// 添加空闲状态的转换
    /// </summary>
    private void AddIdleStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Idle, PlayerState.Walking, 
            () => IsMoving() && isGrounded && canMove, "移动输入且在地面且可移动"));
        
        AddActionTransitions(PlayerState.Idle);
        AddGroundStateTransitions(PlayerState.Idle);
    }
    
    /// <summary>
    /// 添加行走状态的转换
    /// </summary>
    private void AddWalkingStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Walking, PlayerState.Idle, 
            () => !IsMoving() && isGrounded, "停止移动且在地面"));
        
        AddActionTransitions(PlayerState.Walking);
        AddGroundStateTransitions(PlayerState.Walking);
    }
    
    /// <summary>
    /// 添加跳跃状态的转换
    /// </summary>
    private void AddJumpingStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Jumping, PlayerState.Falling, 
            () => rb != null && rb.velocity.y <= 0, "垂直速度变为向下"));
        stateTransitions.Add(new StateTransition(PlayerState.Jumping, PlayerState.Attacking,
            () => ShouldAttack(), "空中攻击"));
    }
    
    /// <summary>
    /// 添加下降状态的转换
    /// </summary>
    private void AddFallingStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Falling, PlayerState.Idle, 
            () => isGrounded && !IsMoving(), "着陆且无移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Falling, PlayerState.Walking, 
            () => isGrounded && IsMoving() && canMove, "着陆且有移动输入且可移动"));
        stateTransitions.Add(new StateTransition(PlayerState.Falling, PlayerState.Attacking,
            () => ShouldAttack(), "空中攻击"));
    }
    
    /// <summary>
    /// 添加攻击状态的转换
    /// </summary>
    private void AddAttackingStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Attacking, PlayerState.Idle, 
            () => !IsAttacking() && isGrounded && !IsMoving(), "攻击结束且在地面且无移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Attacking, PlayerState.Walking, 
            () => !IsAttacking() && isGrounded && IsMoving() && canMove, "攻击结束且在地面且有移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Attacking, PlayerState.Falling, 
            () => !IsAttacking() && ShouldFall(), "攻击结束且应该下降"));
    }
    
    /// <summary>
    /// 添加技能状态的转换
    /// </summary>
    private void AddSkillStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Skill, PlayerState.Idle, 
            () => !IsUsingSkill() && isGrounded && !IsMoving(), "技能结束且在地面且无移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Skill, PlayerState.Walking, 
            () => !IsUsingSkill() && isGrounded && IsMoving() && canMove, "技能结束且在地面且有移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Skill, PlayerState.Falling, 
            () => !IsUsingSkill() && ShouldFall(), "技能结束且应该下降"));
    }
    
    /// <summary>
    /// 添加受伤状态的转换
    /// </summary>
    private void AddHurtStateTransitions()
    {
        stateTransitions.Add(new StateTransition(PlayerState.Hurt, PlayerState.Idle, 
            () => !IsHurt() && isGrounded && !IsMoving(), "受伤结束且在地面且无移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Hurt, PlayerState.Walking, 
            () => !IsHurt() && isGrounded && IsMoving() && canMove, "受伤结束且在地面且有移动输入"));
        stateTransitions.Add(new StateTransition(PlayerState.Hurt, PlayerState.Falling, 
            () => !IsHurt() && ShouldFall(), "受伤结束且应该下降"));
    }
    
    /// <summary>
    /// 添加动作转换（跳跃、攻击、技能）
    /// 优化理由：这些动作转换在多个地面状态中重复，提取为通用方法
    /// </summary>
    private void AddActionTransitions(PlayerState fromState)
    {
        stateTransitions.Add(new StateTransition(fromState, PlayerState.Jumping, 
            () => ShouldJump(), "跳跃输入且在地面且可移动"));
        stateTransitions.Add(new StateTransition(fromState, PlayerState.Attacking, 
            () => ShouldAttack(), "攻击输入且可攻击"));
        stateTransitions.Add(new StateTransition(fromState, PlayerState.Skill, 
            () => ShouldUseSkill(), "技能输入且可使用技能"));
    }
    
    /// <summary>
    /// 添加地面状态转换（下降转换）
    /// 优化理由：下降转换在地面状态中重复，提取为通用方法
    /// </summary>
    private void AddGroundStateTransitions(PlayerState fromState)
    {
        stateTransitions.Add(new StateTransition(fromState, PlayerState.Falling, 
            () => ShouldFall(), "不在地面且垂直速度向下"));
    }
    
    /// <summary>
    /// 更新状态标志
    /// 从PlayerController同步状态信息，并检测关键变化
    /// 性能优化：只在状态真正改变时标记为脏
    /// </summary>
    private void UpdateStateFlags()
    {
        if (playerController != null)
        {
            // 检测地面状态变化
            bool newGrounded = playerController.isGrounded;
            if (isGrounded != newGrounded)
            {
                isGrounded = newGrounded;
                groundedStateChanged = true;
                conditionsDirty = true;
            }
            
            // 检测其他状态变化
            bool newFalling = playerController.isFalling;
            if (isFalling != newFalling)
            {
                isFalling = newFalling;
                conditionsDirty = true;
            }
            
            bool newInvincible = playerController.isInvincible;
            if (isInvincible != newInvincible)
            {
                isInvincible = newInvincible;
                conditionsDirty = true;
            }
            
            bool newCanMove = playerController.canMove;
            if (canMove != newCanMove)
            {
                canMove = newCanMove;
                conditionsDirty = true;
            }
            
            bool newCanAttack = playerController.canAttack;
            if (canAttack != newCanAttack)
            {
                canAttack = newCanAttack;
                conditionsDirty = true;
            }
        }
        
        // 重置事件标记（在检查后重置）
        if (Time.time > lastConditionCheckTime)
        {
            groundedStateChanged = false;
            movementInputChanged = false;
            attackInputReceived = false;
            skillInputReceived = false;
            damageReceived = false;
        }
    }
    
    /// <summary>
    /// 检查状态转换
    /// </summary>
    private void CheckStateTransitions()
    {
        
        foreach (var transition in stateTransitions)
        {
            if (transition.fromState == currentState && transition.condition != null && transition.condition())
            {
                ChangeState(transition.toState);
                break; // 只执行第一个满足条件的转换
            }
        }
    }
    
    /// <summary>
    /// 改变状态
    /// </summary>
    /// <param name="newState">新状态</param>
    public void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        PlayerState oldState = currentState;
        
        // 退出当前状态
        OnStateExit?.Invoke(currentState);
        ExitState(currentState);
        
        // 更新状态
        previousState = currentState;
        currentState = newState;
        
        // 进入新状态
        OnStateEnter?.Invoke(currentState);
        EnterState(currentState);
        
        // 记录状态进入时间
        stateEnterTimes[currentState] = Time.time;
        
        // 触发状态改变事件
        OnStateChanged?.Invoke(oldState, newState);
        
        // 记录状态转换历史
        if (enableDebugLog)
        {
            string transitionLog = $"{Time.time:F2}s: {oldState} -> {newState}";
            stateTransitionHistory.Add(transitionLog);
            
            // 限制历史记录数量
            if (stateTransitionHistory.Count > 50)
            {
                stateTransitionHistory.RemoveAt(0);
            }
            
          //  Debug.Log($"[PlayerStateMachine] {transitionLog}");
        }
    }
    
    /// <summary>
    /// 进入状态时的处理
    /// </summary>
    /// <param name="state">进入的状态</param>
    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                // 空闲状态：确保可以移动和攻击
                if (playerController != null)
                {
                    playerController.canMove = true;
                    playerController.canAttack = true;
                }
                break;
                
            case PlayerState.Walking:
                // 行走状态：确保可以移动
                if (playerController != null)
                {
                    playerController.canMove = true;
                }
                break;
                
            case PlayerState.Jumping:
                // 跳跃状态：保持移动能力
                break;
                
            case PlayerState.Falling:
                // 下降状态：设置下降标志
                
            case PlayerState.Attacking:
                // 攻击状态：允许移动但速度减慢（在PlayerController中处理）
                if (playerController != null)
                {
                    playerController.canMove = true; // 允许移动
                }
                break;
                
            case PlayerState.Skill:
                // 技能状态：根据技能类型限制行动
                if (playerController != null)
                {
                  //  playerController.canMove = false;
                    playerController.canAttack = false;
                }
                break;
                
            case PlayerState.Hurt:
                // 受伤状态：允许移动，限制攻击，启用无敌
                if (playerController != null)
                {
                    playerController.canMove = true;  // 允许移动
                    playerController.canAttack = false;
                    playerController.isInvincible = true;
                }
                break;
                
            case PlayerState.Death:
                // 死亡状态：禁用所有行动
                if (playerController != null)
                {
                    playerController.canMove = false;
                    playerController.canAttack = false;
                }
                break;
        }
    }
    
    /// <summary>
    /// 退出状态时的处理
    /// </summary>
    /// <param name="state">退出的状态</param>
    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Falling:
                // 退出下降状态：重置下降标志
                break;
                
            case PlayerState.Attacking:
                // 退出攻击状态：恢复正常状态（移动能力已在攻击时保持）
                // 无需特殊处理，移动能力在攻击状态时已保持为true
                break;
                
            case PlayerState.Skill:
                // 退出技能状态：恢复行动能力
                if (playerController != null)
                {
                    playerController.canMove = true;
                    playerController.canAttack = true;
                }
                break;
                
            case PlayerState.Hurt:
                // 退出受伤状态：恢复攻击能力（移动能力在受伤时已保持，无敌状态由定时器控制）
                if (playerController != null)
                {
                    playerController.canAttack = true;
                }
                break;
        }
    }
    
    #region 状态条件检查方法
    
    /// <summary>
    /// 是否正在移动
    /// </summary>
    private bool IsMoving()
    {
        return playerController != null && Mathf.Abs(playerController.moveInput.x) > 0.1f;
    }
    
    /// <summary>
    /// 是否应该跳跃
    /// </summary>
    private bool ShouldJump()
    {
        return playerController != null && playerController.jumpInput && isGrounded && canMove;
    }
    
    /// <summary>
    /// 是否应该攻击
    /// </summary>
    private bool ShouldAttack()
    {
        // 检查基本条件
        if (playerController == null || !canAttack) return false;
        
        // 检查是否有攻击输入
        if (!playerController.attackInput) return false;
        
        // 检查是否正在攻击（避免重复攻击）
        if (IsAttacking()) return false;
        
        // 检查角色是否存活
        if (character == null || !character.isAlive) return false;
        
        // 空中可以攻击，但地面攻击更稳定
        return true;
    }
    
    /// <summary>
    /// 是否应该使用技能
    /// </summary>
    private bool ShouldUseSkill()
    {
        // 检查基本条件
        if (skillComponent == null) return false;
        
        // 检查是否在地面（空中不能释放技能，但可以攻击）
        if (!isGrounded) return false;
        
        // 检查角色是否存活
        if (character == null || !character.isAlive) return false;
        
        // 检查是否正在执行其他技能
        if (skillComponent.isExecutingSkill) return false;
        
        // 检查是否有技能输入事件
        if (skillInputReceived)
        {
            Debug.Log("[PlayerStateMachine] 检测到技能输入，准备进入技能状态");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 是否应该下降
    /// </summary>
    private bool ShouldFall()
    {
        return !isGrounded && rb != null && rb.velocity.y < -0.1f;
    }
    
    /// <summary>
    /// 是否应该受伤
    /// </summary>
    private bool ShouldHurt()
    {
        // 检查基本条件
        if (playerController == null || character == null) return false;
        
        // 检查角色是否存活
        if (!character.isAlive) return false;
        
        // 检查是否在无敌状态
        if (isInvincible) return false;
        
        // 检查是否正在受伤（避免重复触发）
        if (playerController.isHurt) return true;
        
        return false;
    }
    
    /// <summary>
    /// 是否应该死亡
    /// </summary>
    private bool ShouldDie()
    {
        return character != null && !character.isAlive;
    }
    
    /// <summary>
    /// 是否正在攻击
    /// </summary>
    private bool IsAttacking()
    {
        return playerController != null && playerController.isAttacking;
    }
    
    /// <summary>
    /// 是否正在使用技能
    /// </summary>
    private bool IsUsingSkill()
    {
        return skillComponent != null && skillComponent.isExecutingSkill;
    }
    
    /// <summary>
    /// 是否正在受伤
    /// </summary>
    private bool IsHurt()
    {
        return playerController != null && playerController.isHurt;
    }
    
    #endregion
    
    #region 公共接口方法
    
    /// <summary>
    /// 强制改变状态（用于外部调用）
    /// </summary>
    /// <param name="newState">新状态</param>
    public void ForceChangeState(PlayerState newState)
    {
        ChangeState(newState);
    }
    
    /// <summary>
    /// 检查是否可以转换到指定状态
    /// </summary>
    /// <param name="targetState">目标状态</param>
    /// <returns>是否可以转换</returns>
    public bool CanTransitionTo(PlayerState targetState)
    {
        foreach (var transition in stateTransitions)
        {
            if (transition.fromState == currentState && transition.toState == targetState)
            {
                return transition.condition == null || transition.condition();
            }
        }
        return false;
    }
    
    #region 事件通知方法 - 性能优化
    
    /// <summary>
    /// 通知移动输入变化
    /// 性能优化：让PlayerController主动通知状态变化，减少每帧检查
    /// </summary>
    /// <param name="isMoving">是否正在移动</param>
    public void NotifyMovementInput(bool isMoving)
    {
        movementInputChanged = true;
        conditionsDirty = true;
    }
    
    /// <summary>
    /// 通知攻击输入
    /// 性能优化：即时响应攻击输入
    /// </summary>
    public void NotifyAttackInput()
    {
        attackInputReceived = true;
        conditionsDirty = true;
    }
    
    /// <summary>
    /// 通知技能输入
    /// 性能优化：即时响应技能输入
    /// </summary>
    public void NotifySkillInput()
    {
        skillInputReceived = true;
        conditionsDirty = true;
    }
    
    /// <summary>
    /// 通知受到伤害
    /// 性能优化：即时响应伤害事件
    /// </summary>
    public void NotifyDamageReceived()
    {
        damageReceived = true;
        conditionsDirty = true;
    }
    
    /// <summary>
    /// 通知地面状态变化
    /// 性能优化：即时响应地面状态变化
    /// </summary>
    /// <param name="grounded">是否在地面</param>
    public void NotifyGroundedStateChanged(bool grounded)
    {
        if (isGrounded != grounded)
        {
            groundedStateChanged = true;
            conditionsDirty = true;
        }
    }
    
    #endregion
    
    /// <summary>
    /// 获取当前状态持续时间
    /// </summary>
    /// <returns>持续时间（秒）</returns>
    public float GetCurrentStateDuration()
    {
        if (stateEnterTimes.ContainsKey(currentState))
        {
            return Time.time - stateEnterTimes[currentState];
        }
        return 0f;
    }
    
    /// <summary>
    /// 检查是否在指定状态
    /// </summary>
    /// <param name="state">要检查的状态</param>
    /// <returns>是否在该状态</returns>
    public bool IsInState(PlayerState state)
    {
        return currentState == state;
    }
    
    /// <summary>
    /// 检查是否在任意指定状态中
    /// </summary>
    /// <param name="states">要检查的状态数组</param>
    /// <returns>是否在任意指定状态中</returns>
    public bool IsInAnyState(params PlayerState[] states)
    {
        foreach (var state in states)
        {
            if (currentState == state) return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    /// <returns>当前状态</returns>
    public PlayerState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 获取前一个状态
    /// </summary>
    /// <returns>前一个状态</returns>
    public PlayerState GetPreviousState()
    {
        return previousState;
    }
    
    #endregion
    
    #region Odin调试方法
    
    [TabGroup("状态机", "调试")]
    [Button("重置状态机")]
    [GUIColor(1f, 0.6f, 0.6f)]
    private void ResetStateMachine()
    {
        ChangeState(PlayerState.Idle);
        stateTransitionHistory.Clear();
        Debug.Log("[PlayerStateMachine] 状态机已重置");
    }
    
    [TabGroup("状态机", "调试")]
    [Button("清除转换历史")]
    private void ClearTransitionHistory()
    {
        stateTransitionHistory.Clear();
        Debug.Log("[PlayerStateMachine] 转换历史已清除");
    }
    
    private string GetTransitionLabel(string transition, int index)
    {
        return $"[{index}] {transition}";
    }
    
    #endregion
}