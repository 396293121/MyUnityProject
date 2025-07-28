using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

/// <summary>
/// 玩家攀爬系统 - 处理梯子攀爬逻辑
/// 与PlayerController状态机集成，提供流畅的攀爬体验
/// </summary>
public class PlayerClimbing : MonoBehaviour, IInputListener
{
    [TabGroup("攀爬设置", "基础参数")]
    [LabelText("攀爬速度")]
    [PropertyRange(1f, 8f)]
    [SuffixLabel("单位/秒")]
    public float climbSpeed = 3f;

    [TabGroup("攀爬设置", "检测设置")]
    [LabelText("梯子检测范围")]
    [PropertyRange(0.5f, 2f)]
    [SuffixLabel("单位")]
    public float ladderDetectionRange = 1f;

    [TabGroup("状态显示", "当前状态")]
    [LabelText("是否在攀爬")]
    [ReadOnly]
    [ShowInInspector]
    private bool isClimbing = false;

    [TabGroup("状态显示", "当前状态")]
    [LabelText("是否在梯子区域")]
    [ReadOnly]
    [ShowInInspector]
    private bool isInLadderArea = false;

    [TabGroup("状态显示", "当前状态")]
    [LabelText("当前梯子")]
    [ReadOnly]
    [ShowInInspector]
    private Collider2D currentLadder = null;

    private float currentLadderBottom = 0f;
    private float currentLadderTop = 0f;

    private float topOffset = 0f;
    [TabGroup("状态显示", "当前状态")]
    [LabelText("梯子中心X坐标")]
    [ReadOnly]
    [ShowInInspector]
    private float ladderCenterX = 0f;
    private bool showDown=false;
    [TabGroup("状态显示", "当前状态")]
    [LabelText("是否按下攀爬输入")]
    [ReadOnly]
    [ShowInInspector]
    private bool climbInputPressed = false;
    // 组件引用
    public PlayerController playerController;
    private PlayerStateMachine stateMachine;
    public Rigidbody2D rb;
    public Animator animator;
    public Collider2D c2d;
    // 动画哈希
    private int animClimbing;
    private int animClimbSpeed;
    private int animClimbTrigger;
    // 输入缓存
    private float verticalInput = 0f;

    private void Awake()
    {

        // 初始化动画哈希
        InitializeAnimationHashes();
    }
    void OnDestroy()
    {
        InputManager.UnregisterListener(this);
    }
    private void Start()
    {
        // 获取组件引用
        playerController = playerController ?? GetComponent<PlayerController>();

        rb = rb ?? GetComponent<Rigidbody2D>();
        c2d = c2d ?? GetComponent<Collider2D>();
        animator = animator ?? GetComponent<Animator>();
        // 验证组件
        if (playerController == null)
        {
            Debug.LogError("[PlayerClimbing] 未找到PlayerController组件！");
            enabled = false;
            return;
        }

        InputManager.RegisterListener(this);
        if ((stateMachine = playerController.StateMachine) == null)
        {
            Debug.LogError("[PlayerClimbing] 未找到PlayerStateMachine组件！");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // 检查是否在梯子区域 且 按下攀爬输入
        if (!IsInLadderArea() || !climbInputPressed) return;

        // 获取输入
        // UpdateInput();

        verticalInput = Input.GetAxis("Vertical");
        // 处理攀爬逻辑
        HandleClimbingLogic();

        // 更新动画
        UpdateClimbingAnimation();
    }

    private void FixedUpdate()
    {
        // 处理攀爬移动
        if (isClimbing)
        {
            HandleClimbingMovement();
        }
    }
    #region 输入监听实现
    // 只实现需要的攀爬输入
    public void OnClimbUpInput()
    {

        if (!IsInLadderArea()) return;
    
        GetInput();
    }
    public void OnClimbDownInput()
    {

        if (!IsInLadderArea()) return;
        GetInputDown();
    }

    // 其他输入方法显式实现为空
    void IInputListener.OnMoveInput(Vector2 moveInput) { }          // 空实现
    void IInputListener.OnJumpInput() { }                          // 空实现
    void IInputListener.OnJumpReleased() { }                       // 空实现
    void IInputListener.OnAttackInput() { }                       // 空实现
    void IInputListener.OnSkillInput() { }                        // 空实现
    void IInputListener.OnInteractInput() { }                     // 空实现
    void IInputListener.OnPauseInput() { }                        // 空实现
    #endregion
    /// <summary>
    /// 初始化动画哈希
    /// </summary>
    private void InitializeAnimationHashes()
    {
        animClimbing = Animator.StringToHash("isClimbing");
        animClimbSpeed = Animator.StringToHash("climbSpeed");
        animClimbTrigger = Animator.StringToHash("ClimbTrigger");
    }

    /// <summary>
    /// 更新输入
    /// </summary>
    private void GetInput()
    {
        if (!isClimbing) {
    float hand =c2d.bounds.max.y;
        if (hand > currentLadderTop)
        {
            return;
        }
            climbInputPressed = true;}

    }
    private void GetInputDown()
    {
        if (!isClimbing)
        {
            float bottom =c2d.bounds.min.y;
            if (bottom < currentLadderTop * 0.5)
            {

                return;
            }
            showDown = true;
          climbInputPressed = true;
        }
    }
    /// <summary>
    /// 处理攀爬逻辑
    /// </summary>
    private void HandleClimbingLogic()
    {
        if (!isInLadderArea)
        {
            // 不在梯子区域，退出攀爬
            if (isClimbing)
            {
                ExitClimbing();
            }
            return;
        }

        // 在梯子区域内的逻辑
        if (!isClimbing && climbInputPressed && CanStartClimbing())
        {
            StartClimbing();
        }
        else if (isClimbing && ShouldExitClimbing())
        {
            ExitClimbing();
        }

        // 重置输入标志
        //   climbInputPressed = false;
    }

    /// <summary>
    /// 检查是否可以开始攀爬
    /// </summary>
    private bool CanStartClimbing()
    {

        if (stateMachine == null) return false;

        PlayerState currentState = stateMachine.GetCurrentState();

        // 只允许从特定状态转换到攀爬状态
        return currentState == PlayerState.Idle ||
               currentState == PlayerState.Walking ||
               currentState == PlayerState.Jumping ||
               currentState == PlayerState.Falling;
    }

    /// <summary>
    /// 开始攀爬
    /// </summary>
    private void StartClimbing()
    {
        isClimbing = true;
        animator.SetBool(animClimbing, true);
        animator.SetTrigger(animClimbTrigger);
        HandleSnapToLadder();
        if (showDown)
        {
            
        HandleClimbInput();
        }
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), true);

        // 通知状态机进入攀爬状态
        if (stateMachine != null)
        {
            stateMachine.ForceChangeState(PlayerState.Climbing);
        }

        // 禁用重力
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
        }

        // 禁用PlayerController的移动
        if (playerController != null)
        {
            playerController.canMove = false;
        }

    }

    /// <summary>
    /// 检查是否应该退出攀爬
    /// </summary>
    private bool ShouldExitClimbing()
    {
        // 如果不在梯子区域，退出攀爬
        if (!isInLadderArea)
            return true;

        // 如果按下跳跃键，退出攀爬
        if (Input.GetKeyDown(KeyCode.Space))
            return true;
            // 角色受伤时，退出攀爬
        if (playerController.isHurt)
        {
            return true;
        }
        //角色底部或顶部离开梯子
        if (c2d?.bounds.min.y < currentLadderBottom)
            return true;
        if ( c2d?.bounds.max.y> currentLadderTop)
        {
            //直接移动到地面,防止掉落
            playerController.transform.position = new Vector3(playerController.transform.position.x,  c2d?.bounds.max.y+ c2d?.bounds.extents.y * 2 ?? 0, 0);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 退出攀爬
    /// </summary>
    private void ExitClimbing()
    {
        isClimbing = false;
        animator.SetBool(animClimbing, false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), false);

        // 恢复重力
        if (rb != null)
        {
            rb.gravityScale = 3f;
        }

        // 恢复PlayerController的移动
        if (playerController != null)
        {
            playerController.canMove = true;
        }

        // 通知状态机退出攀爬状态
        if (stateMachine != null)
        {
            // // 根据当前情况选择合适的状态
            // if (playerController.isGrounded)
            // {
            //     if (Mathf.Abs(playerController.moveInput.x) > 0.1f)
            //     {
            //         stateMachine.ForceChangeState(PlayerState.Walking);
            //     }
            //     else
            //     {
            //         stateMachine.ForceChangeState(PlayerState.Idle);
            //     }
            // }
            // else
            // {
            //     stateMachine.ForceChangeState(PlayerState.Falling);
            // }
        }
        verticalInput = 0f;
        climbInputPressed = false;
    }

    /// <summary>
    /// 处理自动定位到梯子中心
    /// </summary>
    private void HandleSnapToLadder()
    {
        if (currentLadder == null)
        {
            return;
        }

        transform.position = new Vector3(ladderCenterX, transform.position.y, transform.position.z);

    }
    private void HandleClimbInput()
    {

        //直接移动到梯子,防止掉落
        Debug.Log((float)( currentLadderTop - c2d.bounds.extents.y*2));
        transform.position = new Vector3(transform.position.x, (float)( currentLadderTop - c2d.bounds.extents.y*2), 0);
        showDown = false;
     }
    /// <summary>
    /// 处理攀爬移动
    /// </summary>
    private void HandleClimbingMovement()
    {
        if (rb == null) return;

        // 垂直移动
        Vector2 velocity = rb.velocity;
        velocity.y = verticalInput * climbSpeed;
        velocity.x = 0f; // 攀爬时锁定水平移动
        rb.velocity = velocity;
    }

    /// <summary>
    /// 更新攀爬动画
    /// </summary>
    private void UpdateClimbingAnimation()
    {
        if (animator == null) return;

        // 设置攀爬状态
        // animator.SetBool(animClimbing, isClimbing);

        // 设置攀爬速度（用于控制动画播放速度）
        if (isClimbing)
        {
            float climbAnimSpeed = Mathf.Abs(verticalInput);
            animator.SetFloat(animClimbSpeed, climbAnimSpeed);
        }
        else
        {
            animator.SetFloat(animClimbSpeed, 0f);
        }
    }

    /// <summary>
    /// 进入梯子区域
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("梯子"))
        {
            isInLadderArea = true;
            currentLadder = other;
            currentLadderBottom =other.bounds.min.y;
            currentLadderTop =other.bounds.max.y;
            topOffset=other.GetComponent<LadderController>().topOffset;
            // 计算梯子中心X坐标
            ladderCenterX = other.bounds.center.x;

        }
    }

    /// <summary>
    /// 离开梯子区域
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("梯子") && other == currentLadder)
        {
            isInLadderArea = false;
            currentLadder = null;

            // 如果正在攀爬，退出攀爬状态
            if (isClimbing)
            {
                ExitClimbing();
            }

        }
    }

    /// <summary>
    /// 获取当前攀爬状态（供外部查询）
    /// </summary>
    public bool IsClimbing()
    {
        return isClimbing;
    }

    /// <summary>
    /// 获取是否在梯子区域（供外部查询）
    /// </summary>
    public bool IsInLadderArea()
    {
        return isInLadderArea;
    }

    /// <summary>
    /// 强制退出攀爬（供外部调用）
    /// </summary>
    public void ForceExitClimbing()
    {
        if (isClimbing)
        {
            ExitClimbing();
        }
    }
}