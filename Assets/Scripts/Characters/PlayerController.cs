using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 玩家控制器 - 整合角色系统和输入管理，控制玩家角色
/// 从原Phaser项目的Player.js迁移而来
/// 实现IInputListener接口，支持自动输入监听
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class PlayerController : MonoBehaviour, IInputListener
{
    [TabGroup("配置", "角色设置")]
    [BoxGroup("配置/角色设置/角色引用")]
    [LabelText("玩家角色")]
    [Required("必须指定玩家角色")]
    public Character playerCharacter;        // 玩家角色引用
    
    [BoxGroup("配置/角色设置/音频系统")]
    [LabelText("音频配置组件")]
    [InfoBox("可配置的音效系统，支持拖入音频文件、设置音量等参数")]
    public PlayerAudioConfig audioConfig;    // 音频配置组件
    
    [TabGroup("配置", "移动设置")]
    [BoxGroup("配置/移动设置/移动参数")]
    [LabelText("已废弃")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("已废弃")]
    public float aamoveSpeed = 0;             // 移动速度
    
    [BoxGroup("配置/移动设置/移动参数")]
    [LabelText("跳跃力度")]
    [PropertyRange(5f, 30f)]
    [SuffixLabel("力度")]
    public float jumpForce = 10f;            // 跳跃力度
    
    [BoxGroup("配置/移动设置/地面检测")]
    [LabelText("地面检测距离")]
    [PropertyRange(0.05f, 1f)]
    [SuffixLabel("米")]
    public float groundCheckDistance = 0.1f; // 地面检测距离
    
    [BoxGroup("配置/移动设置/地面检测")]
    [LabelText("地面图层")]
    public LayerMask groundLayerMask = 1;    // 地面图层
    
    [BoxGroup("配置/移动设置/地面检测")]
    [LabelText("地面检测点")]
    public GameObject groundCheck;
    // 攻击相关参数已移动到Character基类中
    // 这里保留UI显示用的属性引用
 

    
    [TabGroup("配置", "交互设置")]
    [VerticalGroup("配置/交互设置/交互参数")]
    [LabelText("交互范围")]
    [PropertyRange(0.5f, 5f)]
    [SuffixLabel("米")]
    public float interactRange = 1.5f;       // 交互范围
    
    [VerticalGroup("配置/交互设置/交互参数")]
    [LabelText("可交互物体图层")]
    public LayerMask interactableLayerMask = 1; // 可交互物体图层
    
    
    [TabGroup("状态", "组件引用")]
    [FoldoutGroup("状态/组件引用/核心组件", expanded: false)]
    [LabelText("刚体组件")]
    [ReadOnly]
    [ShowInInspector]
    private Rigidbody2D rb;
    
    [FoldoutGroup("状态/组件引用/核心组件")]
    [LabelText("动画控制器")]
    [ReadOnly]
    [ShowInInspector]
    private Animator animator;
    
    [FoldoutGroup("状态/组件引用/核心组件")]
    [LabelText("精灵渲染器")]
    [ReadOnly]
    [ShowInInspector]
    private SpriteRenderer spriteRenderer;
    
    [FoldoutGroup("状态/组件引用/核心组件")]
    [LabelText("背包系统")]
    [ReadOnly]
    [ShowInInspector]
    private Inventory inventory;
    
    [TabGroup("状态", "输入状态")]
    [HorizontalGroup("状态/输入状态/输入设置")]
    [VerticalGroup("状态/输入状态/输入设置/移动输入")]
    [LabelText("移动输入")]
    [ReadOnly]
    [ShowInInspector]
    public Vector2 moveInput;
    
    [VerticalGroup("状态/输入状态/输入设置/按键输入")]
    [LabelText("跳跃输入")]
    [ReadOnly]
    [ShowInInspector]
    public bool jumpInput;
    
    [VerticalGroup("状态/输入状态/输入设置/按键输入")]
    [LabelText("攻击输入")]
    [ReadOnly]
    [ShowInInspector]
    public bool attackInput;
    
    [VerticalGroup("状态/输入状态/输入设置/按键输入")]
    [LabelText("交互输入")]
    [ReadOnly]
    [ShowInInspector]
    private bool interactInput;
    
    [VerticalGroup("状态/输入状态/输入设置/技能输入")]
    [LabelText("技能1输入")]
    [ReadOnly]
    [ShowInInspector]
    private bool skill1Input;
    
    [VerticalGroup("状态/输入状态/输入设置/技能输入")]
    [LabelText("技能2输入")]
    [ReadOnly]
    [ShowInInspector]
    private bool skill2Input;
    
    [VerticalGroup("状态/输入状态/输入设置/技能输入")]
    [LabelText("技能3输入")]
    [ReadOnly]
    [ShowInInspector]
    private bool skill3Input;
    
    [VerticalGroup("状态/输入状态/输入设置/技能输入")]
    [LabelText("技能4输入")]
    [ReadOnly]
    [ShowInInspector]
    private bool skill4Input;
    
    [TabGroup("状态", "状态机系统")]
    [FoldoutGroup("状态/状态机系统/状态机引用", expanded: true)]
    [LabelText("玩家状态机")]
    [ReadOnly]
    [ShowInInspector]
    private PlayerStateMachine stateMachine;
    
    [TabGroup("状态", "角色状态")]
    [HorizontalGroup("状态/角色状态/状态设置")]
    [VerticalGroup("状态/角色状态/状态设置/基础状态")]
    [LabelText("是否在地面")]
    [ReadOnly]
    [ShowInInspector]
    public bool isGrounded;
    [VerticalGroup("状态/角色状态/状态设置/基础状态")]
    [LabelText("是否正在掉落")]
    [ReadOnly]
    [ShowInInspector]
    public bool isFalling;
    [VerticalGroup("状态/角色状态/状态设置/基础状态")]
    [LabelText("面向右侧")]
    [ReadOnly]
    [ShowInInspector]
    private bool facingRight = true;
    
    [VerticalGroup("状态/角色状态/状态设置/能力状态")]
    [LabelText("可以移动")]
    [ReadOnly]
    [ShowInInspector]
    public bool canMove = true;
    
    [VerticalGroup("状态/角色状态/状态设置/能力状态")]
    [LabelText("可以攻击")]
    [ReadOnly]
    [ShowInInspector]
    public bool canAttack = true;
    
    [VerticalGroup("状态/角色状态/状态设置/能力状态")]
    [LabelText("正在攻击")]
    [ReadOnly]
    [ShowInInspector]
    public bool isAttacking = false;
    
    [VerticalGroup("状态/角色状态/状态设置/受伤状态")]
    [LabelText("正在受伤")]
    [ReadOnly]
    [ShowInInspector]
    public bool isHurt = false;
    
    [VerticalGroup("状态/角色状态/状态设置/受伤状态")]
    [LabelText("无敌状态")]
    [ReadOnly]
    [ShowInInspector]
    public bool isInvincible = false;
    
    [VerticalGroup("状态/角色状态/状态设置/受伤状态")]
    [LabelText("无敌时间")]
    [PropertyRange(0.5f, 3f)]
    [SuffixLabel("秒")]
    [ShowInInspector]
    private float invincibilityTime = 1f;
    
    [TabGroup("状态", "攻击系统")]
    [HorizontalGroup("状态/攻击系统/攻击设置")]
    [VerticalGroup("状态/攻击系统/攻击设置/攻击状态")]
    [LabelText("伤害已触发")]
    [ReadOnly]
    [ShowInInspector]
    private bool damageTriggered = false;
    
    // 攻击冷却参数已移动到各职业脚本中
    // 攻击参数已移动到Character基类中
    
    [TabGroup("状态", "动画系统")]
    [FoldoutGroup("状态/动画系统/动画哈希", expanded: false)]
    [LabelText("移动速度哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animMoveSpeed;
    
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("地面状态哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animIsGrounded;
    
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("攻击哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animAttack;
    
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("跳跃哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animJump;
    
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("掉落哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animFalling;
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("受伤哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animHurt;
    
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("死亡哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animDeath;
    
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("正在攻击哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int animIsAttacking;
    [FoldoutGroup("状态/动画系统/动画哈希")]
    [LabelText("垂直速度哈希")]
    [ReadOnly]
    [ShowInInspector]
    private int verticalVelocityHash;
    
    private void Awake()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inventory = GetComponent<Inventory>();
        
        // 如果没有指定角色，尝试获取当前对象的角色组件
        if (playerCharacter == null)
        {
            playerCharacter = GetComponent<Character>();
           
        } 
        
        // 如果没有指定音频配置，尝试获取当前对象的音频配置组件
        if (audioConfig == null)
        {
            audioConfig = GetComponent<PlayerAudioConfig>();
        }
        
        // 获取或添加状态机组件
        stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<PlayerStateMachine>();
        }
        
        // 初始化动画哈希
        InitializeAnimationHashes();
    }
    
    private void Start()
    {
        // 设置UI管理器的当前角色
        if (UIManager.Instance != null && playerCharacter != null)
        {
            UIManager.Instance.SetCurrentCharacter(playerCharacter);
        }
       
        // 注册为输入监听器
        InputManager.RegisterListener(this);
        
        // 监听角色事件
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged += OnHealthChanged;
            playerCharacter.OnManaChanged += OnManaChanged;
            playerCharacter.OnLevelUp += OnLevelUp;
            playerCharacter.OnDeath += OnPlayerDeath;
            playerCharacter.OnTakeDamage += OnTakeDamage;
        }
        
        // 状态机会在自己的Awake中自动初始化
    }
    
    private void Update()
    {
        // 检查地面
        CheckGrounded();
        
        // 处理移动
        HandleMovement();
        
        // 处理攻击
        HandleAttack();
        
        // 处理交互
        HandleInteraction();
        
        // 状态机会在自己的Update中自动更新
        
        // 更新动画
        UpdateAnimations();
    }
    
    private void FixedUpdate()
    {
        // 物理移动 - 使用状态机判断
        if (stateMachine != null)
        {
            // 根据状态机状态判断是否应该移动
            PlayerState currentState = stateMachine.GetCurrentState();
            bool shouldMove = currentState == PlayerState.Idle || 
                            currentState == PlayerState.Walking || 
                            currentState == PlayerState.Jumping || 
                            currentState == PlayerState.Falling ||
                            currentState == PlayerState.Attacking || // 攻击时允许移动但速度减慢
                            currentState == PlayerState.Skill; // 技能时允许移动但速度减慢
            
            if (shouldMove)
            {
                ApplyMovement();
            }
        }
    }
    
    /// <summary>
    /// 初始化动画哈希
    /// </summary>
    private void InitializeAnimationHashes()
    {
      
        // animAttack = Animator.StringToHash("Attack");
        // animJump = Animator.StringToHash("Jump");
        // animHurt = Animator.StringToHash("Hurt");
        // animDeath = Animator.StringToHash("Death");

            animMoveSpeed = Animator.StringToHash("isWalking");      // 使用 bool 类型
    animIsGrounded = Animator.StringToHash("IsGrounded");
    animAttack = Animator.StringToHash("attackTrigger");
    animJump = Animator.StringToHash("isJumping");
    animFalling = Animator.StringToHash("isFalling");
    animHurt = Animator.StringToHash("hurtTrigger");
    animDeath = Animator.StringToHash("deathTrigger");
     // 添加技能动画哈希
    animIsAttacking = Animator.StringToHash("isAttacking");
    verticalVelocityHash = Animator.StringToHash("verticalVelocity");
    }
    
    /// <summary>
    /// 检查是否在地面
    /// </summary>
    private void CheckGrounded()
    {
        Vector2 raycastOrigin = transform.position;
        if (groundCheck != null)
        {
            raycastOrigin = groundCheck.transform.position;
        }
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, groundCheckDistance, groundLayerMask);
        bool newGrounded = hit.collider != null;
        
        // 性能优化：只在地面状态真正改变时通知状态机
        if (isGrounded != newGrounded)
        {
            isGrounded = newGrounded;
            if (stateMachine != null)
            {
                stateMachine.NotifyGroundedStateChanged(newGrounded);
            }
        }
    }
    
    /// <summary>
    /// 处理移动输入
    /// </summary>
    private void HandleMovement()
    {
        // 移动输入现在通过IInputListener接口的OnMoveInput方法接收
        
        // 性能优化：通知状态机移动输入变化
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        if (stateMachine != null)
        {
            stateMachine.NotifyMovementInput(isMoving);
        }
        
        // 处理转向
        if (moveInput.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && facingRight)
        {
            Flip();
        }
        
        // 处理跳跃 - 使用状态机判断
        if (jumpInput)
        {
            if (stateMachine != null)
            {
                // 状态机会自动检查是否可以跳跃
                if (stateMachine.CanTransitionTo(PlayerState.Jumping))
                {
                    Jump();
                }
            }
            
            // 每次处理完跳跃输入后重置，防止连续跳跃
            jumpInput = false;
        }
    }
    
    /// <summary>
    /// 应用物理移动
    /// </summary>
    private void ApplyMovement()
    {
        if (rb != null && stateMachine != null)
        {
            Vector2 velocity = rb.velocity;
            
            // 使用状态机的canMove标志判断是否可以移动
            if (stateMachine.canMove)
            {
                float speedMultiplier = 1f;
                
                // 根据状态机状态调整移动速度
                PlayerState currentState = stateMachine.GetCurrentState();
                switch (currentState)
                {
                    case PlayerState.Attacking:
                        speedMultiplier = 0.5f; // 攻击时减慢移动速度
                        break;
                    case PlayerState.Skill:
                        speedMultiplier = 0.3f; // 技能时减慢移动速度
                        break;
                    case PlayerState.Hurt:
                        speedMultiplier = 0.7f; // 受伤时减慢移动速度
                        break;
                    default:
                        speedMultiplier = 1f; // 正常移动速度
                        break;
                }
                
                velocity.x = moveInput.x * playerCharacter.Speed * speedMultiplier;
            }
            else
            {
                // 状态机不允许移动时，停止水平移动
                velocity.x = 0f;
            }
            
            rb.velocity = velocity;
        }
    }
    
    /// <summary>
    /// 跳跃
    /// </summary>
    private void Jump()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            
            // 通知状态机进入跳跃状态
            if (stateMachine != null)
            {
                stateMachine.ForceChangeState(PlayerState.Jumping);
            }
            
            // 播放跳跃音效 - 优先使用配置化音频系统
            audioConfig.PlaySound("jump");
            // 播放跳跃动画
            if (animator != null)
            {
                animator.SetTrigger(animJump);
            }
        }
    }
    
    /// <summary>
    /// 翻转角色
    /// </summary>
    private void Flip()
    {
        facingRight = !facingRight;
        if (transform != null)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    
    /// <summary>
    /// 处理攻击
    /// </summary>
    private void HandleAttack()
    {
        // 检查攻击输入
        if (attackInput)
        {
            // 性能优化：通知状态机攻击输入
            if (stateMachine != null)
            {
                stateMachine.NotifyAttackInput();
                
                // 使用状态机判断是否可以攻击
                if (stateMachine.CanTransitionTo(PlayerState.Attacking))
                {
                    // 根据角色类型调用对应的攻击方法
                    bool attackExecuted = false;
                    
                    if (playerCharacter is Warrior warrior)
                    {
                        attackExecuted = warrior.PerformBasicAttack();
                    }
                    else if (playerCharacter is Mage mage)
                    {
                        attackExecuted = mage.PerformBasicAttack();
                    }
                    else if (playerCharacter is Archer archer)
                    {
                        attackExecuted = archer.PerformBasicAttack();
                    }
                    
                    // 如果攻击成功执行，播放攻击动画和音效
                    if (attackExecuted)
                    {
                        StartAttackAnimation();
                    }
                }
            }
            
            attackInput = false;
        }
    }
    
    /// <summary>
    /// 开始攻击动画和音效
    /// </summary>
    private void StartAttackAnimation()
    {
        isAttacking = true;
        animator.SetBool(animIsAttacking, true);
        damageTriggered = false;
        
        // 通知状态机进入攻击状态
        if (stateMachine != null)
        {
            stateMachine.ForceChangeState(PlayerState.Attacking);
        }
        
        // 播放攻击动画
        if (animator != null)
        {
            animator.SetTrigger(animAttack);
        }
        
        // 播放攻击音效 - 优先使用配置化音频系统
        audioConfig.PlaySound("attack");
        
        Debug.Log("玩家开始攻击动画");
    }
    
    /// <summary>
    /// Animation Event调用：在攻击动画的伤害帧触发
    /// 注意：伤害检测现在由各职业的PerformBasicAttack方法处理
    /// 这里主要用于播放命中音效和其他视觉效果
    /// </summary>
    public void OnAttackDamageFrame()
    {
        if (!damageTriggered && isAttacking)
        {
            damageTriggered = true;
             playerCharacter.DetectAndDamageEnemies(()=>{
                audioConfig.PlaySound("attackHit");
             });
            Debug.Log("攻击动画伤害帧触发 - Animation Event");
        }
    }
    
    /// <summary>
    /// Animation Event调用：在攻击动画结束时触发
    /// </summary>
    public void OnAttackEnd()
    {
        EndAttack();
        Debug.Log("攻击动画结束 - Animation Event");
    }
    
    /// <summary>
    /// 结束攻击状态，重置相关标志
    /// </summary>
    private void EndAttack()
    {
        // 防止重复调用
        if (!isAttacking)
        {
            Debug.Log("EndAttack被重复调用，攻击状态已为false");
            return;
        }
        
        isAttacking = false;
        animator.SetBool(animIsAttacking, false);
        damageTriggered = false;
        
        // 通知状态机攻击结束，让状态机自动转换到合适的状态
        // 状态机会根据当前条件自动选择下一个状态（如Idle或Walking）
        Debug.Log("攻击状态已重置，状态机将自动转换状态");
    }
    
    /// <summary>
    /// 角色受到伤害的处理方法
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    public void TakeDamage(int damage)
    {
        // 性能优化：通知状态机伤害事件
        if (stateMachine != null)
        {
            stateMachine.NotifyDamageReceived();
            
            PlayerState currentState = stateMachine.GetCurrentState();
            bool canTakeDamage = playerCharacter.isAlive && 
                               !isInvincible && 
                               currentState != PlayerState.Death && 
                               currentState != PlayerState.Invincible;
            
            if (!canTakeDamage)
            {
                Debug.Log($"伤害被忽略 - 死亡状态: {!playerCharacter.isAlive}, 无敌状态: {isInvincible}, 当前状态: {currentState}");
                return;
            }
            
            // 让角色承受伤害
            playerCharacter.TakeDamage(damage);
            
            // 检查是否在攻击或技能状态 - 这些状态下受伤不触发受伤动画
            bool shouldPlayHurtAnimation = currentState != PlayerState.Attacking && 
                                         currentState != PlayerState.Skill;
            
            if (shouldPlayHurtAnimation)
            {
                // 触发受伤状态和动画
                HurtState();
            }
            else
            {
                // 攻击和技能时受伤：只扣血，不播放受伤动画，不打断当前动作
                Debug.Log($"攻击/技能状态下受伤：扣血但不触发受伤动画 - 当前状态: {currentState}");
                
                // 启动无敌时间但不改变状态
                isInvincible = true;
                StartCoroutine(EndInvincibilityAfterDelay(invincibilityTime));
            }
            
            Debug.Log($"玩家受到伤害: {damage}, 剩余生命值: {playerCharacter.currentHealth}/{playerCharacter.maxHealth}");
            
            // 根据生命值状态决定后续行为
            if (playerCharacter.currentHealth <= 0)
            {
                Die(); // 生命值归零，执行死亡逻辑
            }
            // 注意：受伤状态的处理已经在上面的shouldPlayHurtAnimation逻辑中完成
        }
    }
    
    /// <summary>
    /// 进入受伤状态，播放动画和音效
    /// </summary>
    private void HurtState()
    {
        isHurt = true;
        
        // 通知状态机进入受伤状态
        if (stateMachine != null)
        {
            stateMachine.ForceChangeState(PlayerState.Hurt);
        }
        
        // 触发受伤动画
        if (animator != null)
        {
            animator.SetTrigger(animHurt);
        }
        
        // 播放受伤音效 - 优先使用配置化音频系统
        audioConfig.PlaySound("hurt");
        Debug.Log("角色进入受伤状态");
    }
    
    /// <summary>
    /// Animation Event调用：受伤动画结束时触发
    /// </summary>
    public void OnHurtEnd()
    {
        Debug.Log("受伤动画结束");
        
        // 结束受伤状态
        isHurt = false;
        
        // 通知状态机受伤结束，让状态机自动转换到合适的状态
        // 状态机会根据当前条件自动选择下一个状态（如Idle或Walking）
        Debug.Log("受伤状态结束，状态机将自动转换状态");
        
        // 启动无敌时间倒计时
        StartCoroutine(EndInvincibilityAfterDelay(invincibilityTime));
    }
    
    /// <summary>
    /// 延迟结束无敌状态的协程
    /// </summary>
    /// <param name="delay">无敌持续时间</param>
    /// <returns>协程迭代器</returns>
    private IEnumerator EndInvincibilityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 状态机会自动管理无敌状态
        
        // 通知状态机无敌状态结束，让状态机自动转换到合适的状态
        // 状态机会根据当前条件自动选择下一个状态（如Idle或Walking）
        Debug.Log($"无敌状态结束，持续时间: {delay:F1}秒，状态机将自动转换状态");
    }
    
    /// <summary>
    /// 角色死亡处理
    /// </summary>
    private void Die()
    {
        // 通知状态机进入死亡状态
        if (stateMachine != null)
        {
            stateMachine.ForceChangeState(PlayerState.Death);
        }
        
        // 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger(animDeath);
        }
        
        // 播放死亡音效 - 优先使用配置化音频系统
        audioConfig.PlaySound("death");
        Debug.Log("玩家死亡");
    }
    

    

    private void HandleInteraction()
    {
        // 使用状态机判断是否可以交互
        if (stateMachine != null)
        {
            PlayerState currentState = stateMachine.GetCurrentState();
            bool canInteract = interactInput && 
                             isGrounded && 
                             Mathf.Abs(moveInput.x) < 0.1f &&
                             (currentState == PlayerState.Idle || currentState == PlayerState.Walking);
            
            if (canInteract)
            {
                PerformInteraction();
                interactInput = false;
            }
        }
    }
    
    /// <summary>
    /// 执行交互
    /// </summary>
    private void PerformInteraction()
    {
        Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableLayerMask);
        
        foreach (Collider2D interactable in interactables)
        {
            // 检查是否为物品
            ItemPickup itemPickup = interactable.GetComponent<ItemPickup>();
            if (itemPickup != null && inventory != null)
            {
                if (inventory.AddItem(itemPickup.item, itemPickup.quantity))
                {
                    itemPickup.OnPickedUp();
                    Destroy(interactable.gameObject);
                }
                continue;
            }
            
            // 检查其他可交互对象
            IInteractable interactableComponent = interactable.GetComponent<IInteractable>();
            if (interactableComponent != null)
            {
                interactableComponent.Interact(this);
            }
        }
    }
    
    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null || stateMachine == null) return;
        
        // 根据状态机状态更新动画参数
        PlayerState currentState = stateMachine.GetCurrentState();
        
        // 根据状态机状态设置动画参数
        animator.SetBool(animMoveSpeed, currentState == PlayerState.Walking);
        animator.SetBool(animIsGrounded, isGrounded);
        animator.SetBool(animIsAttacking, currentState == PlayerState.Attacking);
        
        // 设置垂直速度动画参数（始终需要）
        animator.SetFloat(verticalVelocityHash, rb.velocity.y);
            if(rb.velocity.y<-0.1&&!isFalling)
    {
        animator.SetTrigger(animFalling);
        isFalling=true;
    }
    if(isGrounded)
    {
        isFalling=false;
    }
    }
    
    
    // 旧的输入处理方法已移除，现在通过IInputListener接口实现
    
    /// <summary>
    /// 触发技能动画（由SkillComponent调用）
    /// </summary>
    /// <param name="skillName">技能名称</param>
    public void TriggerSkillAnimation(string skillName)
    {
        if (animator != null && !string.IsNullOrEmpty(skillName))
        {
            animator.SetTrigger(skillName);
        }
    }
    
    /// <summary>
    /// 查找最近的敌人
    /// </summary>
    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// 角色事件处理
    /// </summary>
    private void OnHealthChanged(int currentHealth)
    {
        // 生命值变化时的处理
        if (playerCharacter != null && currentHealth <= playerCharacter.maxHealth * 0.2f) // 生命值低于20%
        {
            // 播放低血量音效或显示警告 - 优先使用配置化音频系统
                   audioConfig.PlaySound("lowHealth");
        }
    }
    
    private void OnManaChanged(int currentMana)
    {
        // 魔法值变化时的处理
    }
    
    private void OnLevelUp(int newLevel)
    {
        // 升级时的处理
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"恭喜！升级到 {newLevel} 级！");
        }
        
        // 播放升级音效 - 优先使用配置化音频系统
               audioConfig.PlaySound("levelUp");
    }
    
    private void OnPlayerDeath()
    {
        // 玩家死亡时的处理
        canMove = false;
        canAttack = false;
        
        if (animator != null)
        {
            animator.SetTrigger(animDeath);
        }
        
        // 播放死亡音效 - 优先使用配置化音频系统
        audioConfig.PlaySound("death");
        // 显示游戏结束界面
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGameOver();
        }
        
        // 通知游戏管理器
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
            if (TestSceneController.Instance != null)
    {
        TestSceneController.Instance.OnPlayerDied();
    }
    }
    
    /// <summary>
    /// 受到伤害时的处理
    /// </summary>
    public void OnTakeDamage(int damage)
    {
        if (animator != null)
        {
            animator.SetTrigger(animHurt);
        }
        
        // 播放受伤音效 - 优先使用配置化音频系统
               audioConfig.PlaySound("hurt");
        // 屏幕震动效果（如果有相机震动组件）
        CameraShake cameraShake = Camera.main?.GetComponent<CameraShake>();
        if (cameraShake != null)
        {
            cameraShake.Shake(0.2f, 0.1f);
        }
    }

    
    [TabGroup("控制面板", "状态信息")]
    [BoxGroup("控制面板/状态信息/实时状态")]
    [LabelText("当前速度")]
    [ShowInInspector]
    [ReadOnly]
    private Vector2 CurrentVelocity => rb != null ? rb.velocity : Vector2.zero;
    
    [BoxGroup("控制面板/状态信息/角色信息")]
    [LabelText("角色类型")]
    [ShowInInspector]
    [ReadOnly]
    private string CharacterType => playerCharacter != null ? playerCharacter.GetType().Name : "无";
    
    [BoxGroup("控制面板/状态信息/角色信息")]
    [LabelText("当前生命值")]
    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0, "MaxHealth", ColorGetter = "GetHealthBarColor")]
    private int CurrentHealth => playerCharacter != null ? playerCharacter.currentHealth : 0;
    
    [BoxGroup("控制面板/状态信息/角色信息")]
    [LabelText("最大生命值")]
    [ShowInInspector]
    [ReadOnly]
    private int MaxHealth => playerCharacter != null ? playerCharacter.maxHealth : 0;
    
    [BoxGroup("控制面板/状态信息/角色信息")]
    [LabelText("当前魔法值")]
    [ShowInInspector]
    [ReadOnly]
    [ProgressBar(0, "MaxMana", 0.2f, 0.8f, 1f)]
    private int CurrentMana => playerCharacter != null ? playerCharacter.currentMana : 0;
    
    [BoxGroup("控制面板/状态信息/角色信息")]
    [LabelText("最大魔法值")]
    [ShowInInspector]
    [ReadOnly]
    private int MaxMana => playerCharacter != null ? playerCharacter.maxMana : 0;
    
    /// <summary>
    /// 获取生命值进度条颜色
    /// </summary>
    private Color GetHealthBarColor()
    {
        if (playerCharacter == null) return Color.gray;
        
        float healthPercent = (float)playerCharacter.currentHealth / playerCharacter.maxHealth;
        if (healthPercent > 0.6f) return Color.green;
        if (healthPercent > 0.3f) return Color.yellow;
        return Color.red;
    }
    
 
    /// <summary>
    /// 获取玩家角色
    /// </summary>
    public Character GetPlayerCharacter()
    {
        return playerCharacter;
    }
    
    /// <summary>
    /// 获取背包
    /// </summary>
    public Inventory GetInventory()
    {
        return inventory;
    }
    
    #region 音频系统
    

    /// <summary>
    /// 获取传统音效名称映射
    /// </summary>
    /// <param name="effectType">音效类型</param>
    /// <returns>传统音效名称</returns>
    private string GetLegacySoundName(string effectType)
    {
        switch (effectType)
        {
            case "jump": return "jump_sound";
            case "attack": return "player_attack";
            case "hurt": return "player_hurt";
            case "death": return "player_death";
            case "lowHealth": return "low_health_warning";
            case "levelUp": return "level_up";
            case "criticalHit": return "critical_hit";
            case "whirlwind": return "whirlwind_attack";
            case "battleCry": return "battle_cry";
            default: return null;
        }
    }
    
    #endregion
    private void OnDestroy()
    {
        // 注销输入监听器
        InputManager.UnregisterListener(this);
        
        // 取消监听角色事件
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged -= OnHealthChanged;
            playerCharacter.OnManaChanged -= OnManaChanged;
            playerCharacter.OnLevelUp -= OnLevelUp;
            playerCharacter.OnDeath -= OnPlayerDeath;
        }
    }
    
    #region IInputListener接口实现
    
    /// <summary>
    /// 移动输入事件处理
    /// </summary>
    /// <param name="moveInput">移动输入向量</param>
    public void OnMoveInput(Vector2 moveInput)
    {
        this.moveInput = moveInput;
    }
    
    /// <summary>
    /// 跳跃输入事件处理
    /// </summary>
    public void OnJumpInput()
    {
        jumpInput = true;
    }
    
    /// <summary>
    /// 跳跃释放事件处理
    /// </summary>
    public void OnJumpReleased()
    {
        // 可以在这里处理跳跃释放逻辑，比如可变跳跃高度
    }
    
    /// <summary>
    /// 攻击输入事件处理
    /// </summary>
    public void OnAttackInput()
    {
        attackInput = true;
    }
    
    /// <summary>
    /// 技能输入事件处理
    /// </summary>
    public void OnSkillInput()
    {
        // 获取技能组件
        SkillComponent skillComponent = GetComponent<SkillComponent>();
        if (skillComponent != null)
        {
            // 默认使用第一个技能（索引0），可以根据需要扩展为多技能选择
            skillComponent.TryUseSkill(0);
            Debug.Log("[PlayerController] 技能输入处理：尝试使用技能0");
        }
        else
        {
            Debug.LogWarning("[PlayerController] 未找到SkillComponent组件");
        }
    }
    
    /// <summary>
    /// 交互输入事件处理
    /// </summary>
    public void OnInteractInput()
    {
        interactInput = true;
    }
    
    /// <summary>
    /// 暂停输入事件处理
    /// </summary>
    public void OnPauseInput()
    {
        // 可以在这里处理暂停逻辑
        if (UIManager.Instance != null)
        {
            // UIManager.Instance.TogglePauseMenu();
        }
    }
    
    #endregion
}

/// <summary>
/// 可交互接口
/// </summary>
public interface IInteractable
{
    void Interact(PlayerController player);
}

/// <summary>
/// 物品拾取组件
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public Item item;
    public int quantity = 1;
    
    public void OnPickedUp()
    {
        if (item != null)
        {
            item.OnPickedUp();
        }
    }
}