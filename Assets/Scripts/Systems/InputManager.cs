using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 输入管理器 - 处理玩家输入
/// 使用Unity新输入系统替代原Phaser的键盘输入
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("输入设置")]
    public bool debugInput = false;
    
    // 输入动作
    private PlayerInputActions inputActions;
    
    // 移动输入
    public Vector2 MoveInput { get; private set; }
    
    // 动作输入事件
    public System.Action OnJumpPressed;
    public System.Action OnJumpReleased;
    public System.Action OnAttackPressed;
    public System.Action OnSkillPressed;
    public System.Action OnInteractPressed;
    public System.Action OnPausePressed;
    
    // 单例模式
    public static InputManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInput();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeInput()
    {
        inputActions = new PlayerInputActions();
        
        // 绑定移动输入
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        
        // 绑定动作输入
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Jump.canceled += OnJumpCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Skill.performed += OnSkillPerformed;
        inputActions.Player.Interact.performed += OnInteractPerformed;
        inputActions.Player.Pause.performed += OnPausePerformed;
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 输入系统初始化完成");
        }
    }
    
    void OnEnable()
    {
        inputActions?.Enable();
    }
    
    void OnDisable()
    {
        inputActions?.Disable();
    }
    
    void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Dispose();
        }
    }
    
    // 移动输入处理
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        
        if (debugInput)
        {
            Debug.Log($"[InputManager] 移动输入: {MoveInput}");
        }
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }
    
    // 跳跃输入处理
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 跳跃按下");
        }
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        OnJumpReleased?.Invoke();
    }
    
    // 攻击输入处理
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        OnAttackPressed?.Invoke();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 攻击按下");
        }
    }
    
    // 技能输入处理
    private void OnSkillPerformed(InputAction.CallbackContext context)
    {
        OnSkillPressed?.Invoke();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 技能按下");
        }
    }
    
    // 交互输入处理
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 交互按下");
        }
    }
    
    // 暂停输入处理
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 暂停按下");
        }
    }
    
    /// <summary>
    /// 启用输入
    /// </summary>
    public void EnableInput()
    {
        inputActions?.Enable();
    }
    
    /// <summary>
    /// 禁用输入
    /// </summary>
    public void DisableInput()
    {
        inputActions?.Disable();
    }
}