using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 输入管理器 - 处理玩家输入
/// 使用Unity新输入系统替代原Phaser的键盘输入
/// 支持自动初始化和监听器注册系统
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("输入设置")]
    public bool debugInput = false;
    
    // 输入动作
    private @PlayInputActions inputActions;
    
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
    
    // 输入监听器列表
    private List<IInputListener> listeners = new List<IInputListener>();
    
    /// <summary>
    /// 自动初始化InputManager - 在场景加载前自动创建
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject inputManagerGO = new GameObject("[InputManager]");
            inputManagerGO.AddComponent<InputManager>();
            DontDestroyOnLoad(inputManagerGO);
            Debug.Log("[InputManager] 自动初始化完成");
        }
    }
    
    void Awake()
    {
        Debug.Log($"InputManager Awake,{Instance}");
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
        inputActions = new @PlayInputActions();
        
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
        Debug.Log($"@jump,{inputActions.Player.Jump},@attack,{inputActions.Player.Attack}");
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
        
        // 广播移动输入给所有监听器
        BroadcastMoveInput(MoveInput);
        
        if (debugInput)
        {
            Debug.Log($"[InputManager] 移动输入: {MoveInput}");
        }
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
        
        // 广播移动输入给所有监听器
        BroadcastMoveInput(MoveInput);
    }
    
    // 跳跃输入处理
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
        BroadcastJumpInput();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 跳跃按下");
        }
    }
    
    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        OnJumpReleased?.Invoke();
        BroadcastJumpReleased();
    }
    
    // 攻击输入处理
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        OnAttackPressed?.Invoke();
        BroadcastAttackInput();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 攻击按下");
        }
    }
    
    // 技能输入处理
    private void OnSkillPerformed(InputAction.CallbackContext context)
    {
        OnSkillPressed?.Invoke();
        BroadcastSkillInput();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 技能按下");
        }
    }
    
    // 交互输入处理
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
        BroadcastInteractInput();
        
        if (debugInput)
        {
            Debug.Log("[InputManager] 交互按下");
        }
    }
    
    // 暂停输入处理
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
        BroadcastPauseInput();
        
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
    
    #region 监听器注册系统
    
    /// <summary>
    /// 注册输入监听器
    /// </summary>
    /// <param name="listener">要注册的监听器</param>
    public static void RegisterListener(IInputListener listener)
    {
        if (Instance != null && listener != null && !Instance.listeners.Contains(listener))
        {
            Instance.listeners.Add(listener);
            
            if (Instance.debugInput)
            {
                Debug.Log($"[InputManager] 注册监听器: {listener.GetType().Name}");
            }
        }
    }
    
    /// <summary>
    /// 注销输入监听器
    /// </summary>
    /// <param name="listener">要注销的监听器</param>
    public static void UnregisterListener(IInputListener listener)
    {
        if (Instance != null && listener != null)
        {
            Instance.listeners.Remove(listener);
            
            if (Instance.debugInput)
            {
                Debug.Log($"[InputManager] 注销监听器: {listener.GetType().Name}");
            }
        }
    }
    
    #endregion
    
    #region 输入广播方法
    
    /// <summary>
    /// 广播移动输入
    /// </summary>
    /// <param name="moveInput">移动输入向量</param>
    private void BroadcastMoveInput(Vector2 moveInput)
    {
        foreach (var listener in listeners)
        {
            listener?.OnMoveInput(moveInput);
        }
    }
    
    /// <summary>
    /// 广播跳跃输入
    /// </summary>
    private void BroadcastJumpInput()
    {
        foreach (var listener in listeners)
        {
            listener?.OnJumpInput();
        }
    }
    
    /// <summary>
    /// 广播跳跃释放
    /// </summary>
    private void BroadcastJumpReleased()
    {
        foreach (var listener in listeners)
        {
            listener?.OnJumpReleased();
        }
    }
    
    /// <summary>
    /// 广播攻击输入
    /// </summary>
    private void BroadcastAttackInput()
    {
        foreach (var listener in listeners)
        {
            listener?.OnAttackInput();
        }
    }
    
    /// <summary>
    /// 广播技能输入
    /// </summary>
    private void BroadcastSkillInput()
    {
        foreach (var listener in listeners)
        {
            listener?.OnSkillInput();
        }
    }
    
    /// <summary>
    /// 广播交互输入
    /// </summary>
    private void BroadcastInteractInput()
    {
        foreach (var listener in listeners)
        {
            listener?.OnInteractInput();
        }
    }
    
    /// <summary>
    /// 广播暂停输入
    /// </summary>
    private void BroadcastPauseInput()
    {
        foreach (var listener in listeners)
        {
            listener?.OnPauseInput();
        }
    }
    
    #endregion
}