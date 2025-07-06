using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 测试场景输入管理器
/// 基于Phaser项目中的输入系统，处理键盘、鼠标和手柄输入
/// </summary>
public class TestSceneInputManager : MonoBehaviour
{
    #region 配置引用
    [Header("配置")]
    [SerializeField] private InputSystemConfig inputConfig;
    #endregion
    
    #region 输入状态
    [Header("输入状态")]
    [SerializeField] private Vector2 movementInput = Vector2.zero;
    [SerializeField] private bool isAttackPressed = false;
    [SerializeField] private bool isAttackHeld = false;
    [SerializeField] private bool[] skillInputs = new bool[6]; // 支持6个技能
    [SerializeField] private bool isPausePressed = false;
    [SerializeField] private bool isMenuPressed = false;
    [SerializeField] private bool isInteractPressed = false;
    [SerializeField] private bool isDebugPressed = false;
    #endregion
    
    #region 私有字段
    private TestSceneEventBus eventBus;
    private Dictionary<KeyCode, float> keyHoldTimes = new Dictionary<KeyCode, float>();
    private Dictionary<string, float> lastInputTimes = new Dictionary<string, float>();
    
    // 输入缓冲
    private Queue<InputAction> inputBuffer = new Queue<InputAction>();
    private float inputBufferTime = 0.2f;
    
    // 组合键状态
    private bool isShiftHeld = false;
    private bool isCtrlHeld = false;
    private bool isAltHeld = false;
    
    // 鼠标状态
    private Vector3 lastMousePosition;
    private Vector3 mouseWorldPosition;
    private bool isMouseOverUI = false;
    
    // 手柄支持
    private bool isGamepadConnected = false;
    private string[] gamepadNames;
    
    // 输入锁定
    private bool isInputLocked = false;
    private Dictionary<string, bool> inputLocks = new Dictionary<string, bool>();
    #endregion
    
    #region 输入事件
    public event Action<Vector2> OnMovementInput;
    public event Action OnAttackPressed;
    public event Action OnAttackReleased;
    public event Action<int> OnSkillPressed;
    public event Action OnPausePressed;
    public event Action OnMenuPressed;
    public event Action OnInteractPressed;
    public event Action OnDebugPressed;
    public event Action<Vector3> OnMouseClick;
    public event Action<Vector3> OnMouseMove;
    #endregion
    
    #region Unity生命周期
    private void Awake()
    {
        // 初始化输入系统
        InitializeInputSystem();
    }
    
    private void Start()
    {
        // 检测手柄
        DetectGamepads();
    }
    
    private void Update()
    {
        if (isInputLocked) return;
        
        // 处理键盘输入
        HandleKeyboardInput();
        
        // 处理鼠标输入
        HandleMouseInput();
        
        // 处理手柄输入
        HandleGamepadInput();
        
        // 处理组合键
        HandleModifierKeys();
        
        // 处理输入缓冲
        ProcessInputBuffer();
        
        // 更新按键持续时间
        UpdateKeyHoldTimes();
    }
    
    private void OnDestroy()
    {
        // 清理资源
        CleanupInputSystem();
    }
    #endregion
    
    #region 初始化方法
    /// <summary>
    /// 初始化输入管理器
    /// </summary>
    public void Initialize(InputSystemConfig config, TestSceneEventBus eventSystem)
    {
        inputConfig = config;
        eventBus = eventSystem;
        
        // 注册事件监听器
        RegisterEventListeners();
        
        // 应用输入配置
        ApplyInputConfiguration();
        
        Debug.Log("[TestSceneInputManager] 输入管理器初始化完成");
    }
    
    /// <summary>
    /// 初始化输入系统
    /// </summary>
    private void InitializeInputSystem()
    {
        // 初始化输入状态
        movementInput = Vector2.zero;
        isAttackPressed = false;
        isAttackHeld = false;
        
        for (int i = 0; i < skillInputs.Length; i++)
        {
            skillInputs[i] = false;
        }
        
        // 初始化鼠标位置
        lastMousePosition = Input.mousePosition;
        
        // 设置光标状态
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// 应用输入配置
    /// </summary>
    private void ApplyInputConfiguration()
    {
        if (inputConfig == null) return;
        
        // 设置输入缓冲时间
        inputBufferTime = inputConfig.inputBufferTime;
        
        // 应用其他配置...
    }
    
    /// <summary>
    /// 检测手柄
    /// </summary>
    private void DetectGamepads()
    {
        gamepadNames = Input.GetJoystickNames();
        isGamepadConnected = gamepadNames.Length > 0 && !string.IsNullOrEmpty(gamepadNames[0]);
        
        if (isGamepadConnected)
        {
            Debug.Log($"[TestSceneInputManager] 检测到手柄: {gamepadNames[0]}");
        }
    }
    #endregion
    
    #region 输入处理方法
    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (inputConfig?.movementConfig == null) return;
        
        // 移动输入
        HandleMovementInput();
        
        // 攻击输入
        HandleAttackInput();
        
        // 技能输入
        HandleSkillInput();
        
        // 系统输入
        HandleSystemInput();
        
        // UI输入
        HandleUIInput();
    }
    
    /// <summary>
    /// 处理移动输入
    /// </summary>
    private void HandleMovementInput()
    {
        Vector2 newMovementInput = Vector2.zero;
        
        // 获取移动输入
        if (IsKeyPressed(inputConfig.movementConfig.upKey))
            newMovementInput.y += 1f;
        if (IsKeyPressed(inputConfig.movementConfig.downKey))
            newMovementInput.y -= 1f;
        if (IsKeyPressed(inputConfig.movementConfig.leftKey))
            newMovementInput.x -= 1f;
        if (IsKeyPressed(inputConfig.movementConfig.rightKey))
            newMovementInput.x += 1f;
        
        // 标准化移动向量
        if (newMovementInput.magnitude > 1f)
        {
            newMovementInput.Normalize();
        }
        
        // 检查移动输入是否改变
        if (newMovementInput != movementInput)
        {
            movementInput = newMovementInput;
            
            // 触发移动事件
            OnMovementInput?.Invoke(movementInput);
            eventBus?.TriggerEvent("MovementInput", movementInput);
        }
    }
    
    /// <summary>
    /// 处理攻击输入
    /// </summary>
    private void HandleAttackInput()
    {
        bool attackPressed = IsKeyDown(inputConfig.attackConfig.attackKey);
        bool attackReleased = IsKeyUp(inputConfig.attackConfig.attackKey);
        bool attackHeld = IsKeyPressed(inputConfig.attackConfig.attackKey);
        
        // 攻击按下
        if (attackPressed && !isAttackPressed)
        {
            isAttackPressed = true;
            OnAttackPressed?.Invoke();
            eventBus?.TriggerEvent("AttackInput", null);
            
            // 添加到输入缓冲
            AddToInputBuffer(new InputAction { actionType = "Attack", timestamp = Time.time });
        }
        
        // 攻击释放
        if (attackReleased && isAttackPressed)
        {
            isAttackPressed = false;
            OnAttackReleased?.Invoke();
        }
        
        // 更新攻击持续状态
        isAttackHeld = attackHeld;
    }
    
    /// <summary>
    /// 处理技能输入
    /// </summary>
    private void HandleSkillInput()
    {
        var skillKeys = new KeyCode[]
        {
            inputConfig.skillConfig.skill1Key,
            inputConfig.skillConfig.skill2Key,
            inputConfig.skillConfig.skill3Key,
            inputConfig.skillConfig.skill4Key,
            inputConfig.skillConfig.skill5Key,
            inputConfig.skillConfig.skill6Key
        };
        
        for (int i = 0; i < skillKeys.Length && i < skillInputs.Length; i++)
        {
            if (IsKeyDown(skillKeys[i]))
            {
                skillInputs[i] = true;
                OnSkillPressed?.Invoke(i);
                eventBus?.TriggerEvent("SkillInput", i);
                
                // 添加到输入缓冲
                AddToInputBuffer(new InputAction { actionType = $"Skill{i}", timestamp = Time.time });
            }
            else if (IsKeyUp(skillKeys[i]))
            {
                skillInputs[i] = false;
            }
        }
    }
    
    /// <summary>
    /// 处理系统输入
    /// </summary>
    private void HandleSystemInput()
    {
        // 暂停输入
        if (IsKeyDown(inputConfig.systemConfig.pauseKey))
        {
            isPausePressed = true;
            OnPausePressed?.Invoke();
            eventBus?.TriggerEvent("PauseInput", null);
        }
        else
        {
            isPausePressed = false;
        }
        
        // 菜单输入
        if (IsKeyDown(inputConfig.uiConfig.menuKey))
        {
            isMenuPressed = true;
            OnMenuPressed?.Invoke();
            eventBus?.TriggerEvent("MenuInput", null);
        }
        else
        {
            isMenuPressed = false;
        }
        
        // 交互输入
        if (IsKeyDown(inputConfig.uiConfig.interactKey))
        {
            isInteractPressed = true;
            OnInteractPressed?.Invoke();
            eventBus?.TriggerEvent("InteractInput", null);
        }
        else
        {
            isInteractPressed = false;
        }
        
        // 调试输入
        if (IsKeyDown(inputConfig.systemConfig.debugKey))
        {
            isDebugPressed = true;
            OnDebugPressed?.Invoke();
            eventBus?.TriggerEvent("DebugInput", null);
        }
        else
        {
            isDebugPressed = false;
        }
    }
    
    /// <summary>
    /// 处理UI输入
    /// </summary>
    private void HandleUIInput()
    {
        // 检查鼠标是否在UI上
        isMouseOverUI = UnityEngine.EventSystems.EventSystem.current?.IsPointerOverGameObject() ?? false;
        
        // 其他UI相关输入处理...
    }
    
    /// <summary>
    /// 处理鼠标输入
    /// </summary>
    private void HandleMouseInput()
    {
        if (inputConfig == null) return;
        
        // 鼠标移动
        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != lastMousePosition)
        {
            lastMousePosition = currentMousePosition;
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, Camera.main.nearClipPlane));
            
            OnMouseMove?.Invoke(mouseWorldPosition);
        }
        
        // 鼠标点击
        if (Input.GetMouseButtonDown(0) && !isMouseOverUI)
        {
            OnMouseClick?.Invoke(mouseWorldPosition);
            eventBus?.TriggerEvent("MouseClick", mouseWorldPosition);
        }
        
        // 右键点击
        if (Input.GetMouseButtonDown(1) && !isMouseOverUI)
        {
            eventBus?.TriggerEvent("MouseRightClick", mouseWorldPosition);
        }
        
        // 鼠标滚轮
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            eventBus?.TriggerEvent("MouseScroll", scroll);
        }
    }
    
    /// <summary>
    /// 处理手柄输入
    /// </summary>
    private void HandleGamepadInput()
    {
        if (!isGamepadConnected || inputConfig?.gamepadConfig == null) return;
        
        // 手柄移动输入
        Vector2 gamepadMovement = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        
        // 应用死区
        if (gamepadMovement.magnitude < inputConfig.gamepadConfig.deadzone)
        {
            gamepadMovement = Vector2.zero;
        }
        
        // 如果手柄有输入，覆盖键盘输入
        if (gamepadMovement.magnitude > 0.1f)
        {
            movementInput = gamepadMovement;
            OnMovementInput?.Invoke(movementInput);
            eventBus?.TriggerEvent("MovementInput", movementInput);
        }
        
        // 手柄按钮输入
        HandleGamepadButtons();
    }
    
    /// <summary>
    /// 处理手柄按钮
    /// </summary>
    private void HandleGamepadButtons()
    {
        // 攻击按钮
        if (Input.GetKeyDown(inputConfig.gamepadConfig.attackButton))
        {
            OnAttackPressed?.Invoke();
            eventBus?.TriggerEvent("AttackInput", null);
        }
        
        // 技能按钮
        var skillButtons = new KeyCode[]
        {
            inputConfig.gamepadConfig.skill1Button,
            inputConfig.gamepadConfig.skill2Button,
            inputConfig.gamepadConfig.skill3Button,
            inputConfig.gamepadConfig.skill4Button
        };
        
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (Input.GetKeyDown(skillButtons[i]))
            {
                OnSkillPressed?.Invoke(i);
                eventBus?.TriggerEvent("SkillInput", i);
            }
        }
        
        // 系统按钮
        if (Input.GetKeyDown(inputConfig.gamepadConfig.pauseButton))
        {
            OnPausePressed?.Invoke();
            eventBus?.TriggerEvent("PauseInput", null);
        }
        
        if (Input.GetKeyDown(inputConfig.gamepadConfig.menuButton))
        {
            OnMenuPressed?.Invoke();
            eventBus?.TriggerEvent("MenuInput", null);
        }
    }
    
    /// <summary>
    /// 处理组合键
    /// </summary>
    private void HandleModifierKeys()
    {
        isShiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        isAltHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        
        // 处理组合键功能
        if (isCtrlHeld && Input.GetKeyDown(KeyCode.D))
        {
            // Ctrl+D: 切换调试模式
            eventBus?.TriggerEvent("ToggleDebug", null);
        }
        
        if (isCtrlHeld && Input.GetKeyDown(KeyCode.R))
        {
            // Ctrl+R: 重启场景
            eventBus?.TriggerEvent("RestartScene", null);
        }
    }
    #endregion
    
    #region 输入缓冲系统
    /// <summary>
    /// 输入动作结构
    /// </summary>
    private struct InputAction
    {
        public string actionType;
        public float timestamp;
    }
    
    /// <summary>
    /// 添加到输入缓冲
    /// </summary>
    private void AddToInputBuffer(InputAction action)
    {
        inputBuffer.Enqueue(action);
        
        // 限制缓冲区大小
        while (inputBuffer.Count > 10)
        {
            inputBuffer.Dequeue();
        }
    }
    
    /// <summary>
    /// 处理输入缓冲
    /// </summary>
    private void ProcessInputBuffer()
    {
        while (inputBuffer.Count > 0)
        {
            var action = inputBuffer.Peek();
            
            // 检查输入是否过期
            if (Time.time - action.timestamp > inputBufferTime)
            {
                inputBuffer.Dequeue();
                continue;
            }
            
            // 处理缓冲的输入
            ProcessBufferedInput(action);
            inputBuffer.Dequeue();
        }
    }
    
    /// <summary>
    /// 处理缓冲的输入
    /// </summary>
    private void ProcessBufferedInput(InputAction action)
    {
        // 这里可以实现输入缓冲的具体逻辑
        // 例如：连击系统、技能组合等
        Debug.Log($"[TestSceneInputManager] 处理缓冲输入: {action.actionType}");
    }
    #endregion
    
    #region 输入锁定系统
    /// <summary>
    /// 更新输入（供外部调用）
    /// </summary>
    public void UpdateInput()
    {
        // 这个方法可以为空，因为Update()已经处理了所有输入
        // 或者可以用于强制更新输入状态
    }
    
    /// <summary>
    /// 设置输入锁定状态
    /// </summary>
    public void SetInputLocked(bool locked)
    {
        isInputLocked = locked;
    }
    
    /// <summary>
    /// 锁定所有输入
    /// </summary>
    public void LockAllInput(bool locked)
    {
        isInputLocked = locked;
    }
    
    /// <summary>
    /// 锁定特定输入类型
    /// </summary>
    public void LockInput(string inputType, bool locked)
    {
        inputLocks[inputType] = locked;
    }
    
    /// <summary>
    /// 检查输入是否被锁定
    /// </summary>
    private bool IsInputLocked(string inputType)
    {
        return inputLocks.ContainsKey(inputType) && inputLocks[inputType];
    }
    #endregion
    
    #region 辅助方法
    /// <summary>
    /// 检查按键是否按下
    /// </summary>
    private bool IsKeyDown(KeyCode key)
    {
        return Input.GetKeyDown(key);
    }
    
    /// <summary>
    /// 检查按键是否释放
    /// </summary>
    private bool IsKeyUp(KeyCode key)
    {
        return Input.GetKeyUp(key);
    }
    
    /// <summary>
    /// 检查按键是否持续按下
    /// </summary>
    private bool IsKeyPressed(KeyCode key)
    {
        return Input.GetKey(key);
    }
    
    /// <summary>
    /// 更新按键持续时间
    /// </summary>
    private void UpdateKeyHoldTimes()
    {
        var keysToRemove = new List<KeyCode>();
        
        foreach (var kvp in new Dictionary<KeyCode, float>(keyHoldTimes))
        {
            if (Input.GetKey(kvp.Key))
            {
                keyHoldTimes[kvp.Key] += Time.deltaTime;
            }
            else
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            keyHoldTimes.Remove(key);
        }
        
        // 添加新按下的键
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key) && !keyHoldTimes.ContainsKey(key))
            {
                keyHoldTimes[key] = 0f;
            }
        }
    }
    
    /// <summary>
    /// 获取按键持续时间
    /// </summary>
    public float GetKeyHoldTime(KeyCode key)
    {
        return keyHoldTimes.ContainsKey(key) ? keyHoldTimes[key] : 0f;
    }
    #endregion
    
    #region 事件系统
    /// <summary>
    /// 注册事件监听器
    /// </summary>
    private void RegisterEventListeners()
    {
        if (eventBus == null) return;
        
        // 注册输入相关事件监听器
        eventBus.RegisterListener("LockInput", (data) => {
            if (data is bool locked)
                LockAllInput(locked);
        });
        
        eventBus.RegisterListener("LockSpecificInput", (data) => {
            // 处理特定输入锁定
        });
    }
    
    /// <summary>
    /// 注销事件监听器
    /// </summary>
    private void UnregisterEventListeners()
    {
        if (eventBus == null) return;
        
        // 注销事件监听器
        eventBus.ClearListeners("LockInput");
        eventBus.ClearListeners("LockSpecificInput");
    }
    #endregion
    
    #region 公共方法
    /// <summary>
    /// 获取当前移动输入
    /// </summary>
    public Vector2 GetMovementInput()
    {
        return movementInput;
    }
    
    /// <summary>
    /// 获取鼠标世界位置
    /// </summary>
    public Vector3 GetMouseWorldPosition()
    {
        return mouseWorldPosition;
    }
    
    /// <summary>
    /// 检查是否有手柄连接
    /// </summary>
    public bool IsGamepadConnected()
    {
        return isGamepadConnected;
    }
    
    /// <summary>
    /// 强制刷新手柄检测
    /// </summary>
    public void RefreshGamepadDetection()
    {
        DetectGamepads();
    }
    
    /// <summary>
    /// 获取输入统计信息
    /// </summary>
    public string GetInputStats()
    {
        string stats = "输入系统统计:\n";
        stats += $"移动输入: {movementInput}\n";
        stats += $"攻击状态: {(isAttackPressed ? "按下" : "释放")}\n";
        stats += $"手柄连接: {(isGamepadConnected ? "是" : "否")}\n";
        stats += $"输入锁定: {(isInputLocked ? "是" : "否")}\n";
        stats += $"输入缓冲数量: {inputBuffer.Count}\n";
        stats += $"鼠标在UI上: {(isMouseOverUI ? "是" : "否")}\n";
        
        return stats;
    }
    #endregion
    
    #region 清理方法
    /// <summary>
    /// 清理输入系统
    /// </summary>
    private void CleanupInputSystem()
    {
        // 注销事件监听器
        UnregisterEventListeners();
        
        // 清理输入缓冲
        inputBuffer.Clear();
        
        // 清理按键持续时间
        keyHoldTimes.Clear();
        
        // 清理输入锁定
        inputLocks.Clear();
        
        // 重置光标状态
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("[TestSceneInputManager] 输入系统清理完成");
    }
    #endregion
}