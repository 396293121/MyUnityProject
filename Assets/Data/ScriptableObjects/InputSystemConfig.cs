using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 输入系统配置 ScriptableObject
/// 基于Phaser项目中的InputManager设计
/// </summary>
[CreateAssetMenu(fileName = "InputSystemConfig", menuName = "Game/Input System Config")]
public class InputSystemConfig : ScriptableObject
{
    [Header("输入系统设置")]
    [Tooltip("输入更新频率")]
    public float inputUpdateRate = 60f;
    
    [Tooltip("启用输入缓冲")]
    public bool enableInputBuffer = true;
    
    [Tooltip("输入缓冲时间")]
    public float inputBufferTime = 0.2f;
    
    [Header("移动控制")]
    [Tooltip("移动输入配置")]
    public MovementInputConfig movementConfig;
    
    [Header("战斗控制")]
    [Tooltip("攻击输入配置")]
    public AttackInputConfig attackConfig;
    
    [Tooltip("技能输入配置")]
    public SkillInputConfig skillConfig;
    
    [Header("UI控制")]
    [Tooltip("UI输入配置")]
    public UIInputConfig uiConfig;
    
    [Header("系统控制")]
    [Tooltip("系统输入配置")]
    public SystemInputConfig systemConfig;
    
    [Header("手柄支持")]
    [Tooltip("手柄输入配置")]
    public GamepadInputConfig gamepadConfig;
    
    [Header("输入映射")]
    [Tooltip("键盘输入映射")]
    public List<KeyboardInputMapping> keyboardMappings = new List<KeyboardInputMapping>();
    
    [Tooltip("鼠标输入映射")]
    public List<MouseInputMapping> mouseMappings = new List<MouseInputMapping>();
    
    [Tooltip("手柄输入映射")]
    public List<GamepadInputMapping> gamepadMappings = new List<GamepadInputMapping>();
}

/// <summary>
/// 移动输入配置
/// </summary>
[System.Serializable]
public class MovementInputConfig
{
    [Header("移动设置")]
    [Tooltip("移动死区")]
    [Range(0f, 1f)]
    public float deadZone = 0.1f;
    
    [Tooltip("移动平滑度")]
    [Range(0f, 1f)]
    public float smoothing = 0.1f;
    
    [Tooltip("启用八方向移动")]
    public bool enableEightDirectional = true;
    
    [Tooltip("对角线移动速度修正")]
    public bool normalizeDiagonal = true;
    
    [Header("键盘移动")]
    [Tooltip("上移动键")]
    public KeyCode upKey = KeyCode.W;
    
    [Tooltip("下移动键")]
    public KeyCode downKey = KeyCode.S;
    
    [Tooltip("左移动键")]
    public KeyCode leftKey = KeyCode.A;
    
    [Tooltip("右移动键")]
    public KeyCode rightKey = KeyCode.D;
    
    [Header("替代移动键")]
    [Tooltip("上箭头键")]
    public KeyCode upArrowKey = KeyCode.UpArrow;
    
    [Tooltip("下箭头键")]
    public KeyCode downArrowKey = KeyCode.DownArrow;
    
    [Tooltip("左箭头键")]
    public KeyCode leftArrowKey = KeyCode.LeftArrow;
    
    [Tooltip("右箭头键")]
    public KeyCode rightArrowKey = KeyCode.RightArrow;
    
    [Header("特殊移动")]
    [Tooltip("冲刺键")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    
    [Tooltip("潜行键")]
    public KeyCode sneakKey = KeyCode.LeftControl;
    
    [Tooltip("跳跃键")]
    public KeyCode jumpKey = KeyCode.Space;
}

/// <summary>
/// 攻击输入配置
/// </summary>
[System.Serializable]
public class AttackInputConfig
{
    [Header("攻击设置")]
    [Tooltip("攻击按键")]
    public KeyCode attackKey = KeyCode.Space;
    
    [Tooltip("重攻击按键")]
    public KeyCode heavyAttackKey = KeyCode.LeftShift;
    
    [Tooltip("格挡按键")]
    public KeyCode blockKey = KeyCode.Mouse1;
    
    [Tooltip("闪避按键")]
    public KeyCode dodgeKey = KeyCode.LeftControl;
    
    [Header("连击设置")]
    [Tooltip("启用连击系统")]
    public bool enableComboSystem = true;
    
    [Tooltip("连击窗口时间")]
    public float comboWindow = 0.5f;
    
    [Tooltip("最大连击数")]
    public int maxComboCount = 3;
    
    [Header("鼠标攻击")]
    [Tooltip("鼠标左键攻击")]
    public bool leftClickAttack = true;
    
    [Tooltip("鼠标右键特殊攻击")]
    public bool rightClickSpecialAttack = true;
    
    [Tooltip("鼠标中键技能")]
    public bool middleClickSkill = false;
}

/// <summary>
/// 技能输入配置
/// </summary>
[System.Serializable]
public class SkillInputConfig
{
    [Header("技能快捷键")]
    [Tooltip("技能槽1")]
    public KeyCode skill1Key = KeyCode.Alpha1;
    
    [Tooltip("技能槽2")]
    public KeyCode skill2Key = KeyCode.Alpha2;
    
    [Tooltip("技能槽3")]
    public KeyCode skill3Key = KeyCode.Alpha3;
    
    [Tooltip("技能槽4")]
    public KeyCode skill4Key = KeyCode.Alpha4;
    
    [Tooltip("技能槽5")]
    public KeyCode skill5Key = KeyCode.Alpha5;
    
    [Tooltip("技能槽6")]
    public KeyCode skill6Key = KeyCode.Alpha6;
    
    [Tooltip("技能槽7")]
    public KeyCode skill7Key = KeyCode.Alpha7;
    
    [Tooltip("技能槽8")]
    public KeyCode skill8Key = KeyCode.Alpha8;
    
    [Header("特殊技能")]
    [Tooltip("终极技能")]
    public KeyCode ultimateSkillKey = KeyCode.R;
    
    [Tooltip("治疗技能")]
    public KeyCode healSkillKey = KeyCode.H;
    
    [Tooltip("传送技能")]
    public KeyCode teleportSkillKey = KeyCode.T;
}

/// <summary>
/// UI输入配置
/// </summary>
[System.Serializable]
public class UIInputConfig
{
    [Header("基础控制")]
    [Tooltip("菜单键")]
    public KeyCode menuKey = KeyCode.Escape;
    
    [Tooltip("交互键")]
    public KeyCode interactKey = KeyCode.E;
    
    [Header("界面控制")]
    [Tooltip("背包键")]
    public KeyCode inventoryKey = KeyCode.I;
    
    [Tooltip("角色面板键")]
    public KeyCode characterPanelKey = KeyCode.C;
    
    [Tooltip("技能树键")]
    public KeyCode skillTreeKey = KeyCode.K;
    
    [Tooltip("任务日志键")]
    public KeyCode questLogKey = KeyCode.J;
    
    [Tooltip("地图键")]
    public KeyCode mapKey = KeyCode.M;
    
    [Header("聊天控制")]
    [Tooltip("聊天键")]
    public KeyCode chatKey = KeyCode.Return;
    
    [Tooltip("全体聊天键")]
    public KeyCode allChatKey = KeyCode.T;
    
    [Tooltip("队伍聊天键")]
    public KeyCode teamChatKey = KeyCode.Y;
    
    [Header("快捷操作")]
    [Tooltip("快速保存键")]
    public KeyCode quickSaveKey = KeyCode.F5;
    
    [Tooltip("快速加载键")]
    public KeyCode quickLoadKey = KeyCode.F9;
    
    [Tooltip("截图键")]
    public KeyCode screenshotKey = KeyCode.F12;
}

/// <summary>
/// 系统输入配置
/// </summary>
[System.Serializable]
public class SystemInputConfig
{
    [Header("系统控制")]
    [Tooltip("暂停键")]
    public KeyCode pauseKey = KeyCode.Escape;
    
    [Tooltip("暂停/菜单键")]
    public KeyCode pauseMenuKey = KeyCode.Escape;
    
    [Tooltip("设置键")]
    public KeyCode settingsKey = KeyCode.F1;
    
    [Tooltip("帮助键")]
    public KeyCode helpKey = KeyCode.F1;
    
    [Header("调试控制")]
    [Tooltip("调试键")]
    public KeyCode debugKey = KeyCode.F3;
    
    [Tooltip("调试模式键")]
    public KeyCode debugModeKey = KeyCode.F3;
    
    [Tooltip("控制台键")]
    public KeyCode consoleKey = KeyCode.BackQuote;
    
    [Tooltip("性能监视器键")]
    public KeyCode performanceMonitorKey = KeyCode.F11;
    
    [Header("音频控制")]
    [Tooltip("静音键")]
    public KeyCode muteKey = KeyCode.F10;
    
    [Tooltip("音量增加键")]
    public KeyCode volumeUpKey = KeyCode.Plus;
    
    [Tooltip("音量减少键")]
    public KeyCode volumeDownKey = KeyCode.Minus;
}

/// <summary>
/// 手柄输入配置
/// </summary>
[System.Serializable]
public class GamepadInputConfig
{
    [Header("手柄设置")]
    [Tooltip("启用手柄支持")]
    public bool enableGamepadSupport = true;
    
    [Tooltip("手柄死区")]
    [Range(0f, 1f)]
    public float deadzone = 0.2f;
    
    [Tooltip("手柄死区")]
    [Range(0f, 1f)]
    public float gamepadDeadZone = 0.2f;
    
    [Tooltip("手柄灵敏度")]
    [Range(0.1f, 3f)]
    public float gamepadSensitivity = 1f;
    
    [Header("振动设置")]
    [Tooltip("启用振动")]
    public bool enableVibration = true;
    
    [Tooltip("振动强度")]
    [Range(0f, 1f)]
    public float vibrationIntensity = 0.5f;
    
    [Tooltip("振动持续时间")]
    public float vibrationDuration = 0.2f;
    
    [Header("手柄按键")]
    [Tooltip("攻击按钮")]
    public KeyCode attackButton = KeyCode.Joystick1Button0;
    
    [Tooltip("技能1按钮")]
    public KeyCode skill1Button = KeyCode.Joystick1Button1;
    
    [Tooltip("技能2按钮")]
    public KeyCode skill2Button = KeyCode.Joystick1Button2;
    
    [Tooltip("技能3按钮")]
    public KeyCode skill3Button = KeyCode.Joystick1Button3;
    
    [Tooltip("技能4按钮")]
    public KeyCode skill4Button = KeyCode.Joystick1Button4;
    
    [Tooltip("暂停按钮")]
    public KeyCode pauseButton = KeyCode.Joystick1Button7;
    
    [Tooltip("菜单按钮")]
    public KeyCode menuButton = KeyCode.Joystick1Button6;
}

/// <summary>
/// 键盘输入映射
/// </summary>
[System.Serializable]
public class KeyboardInputMapping
{
    [Tooltip("动作名称")]
    public string actionName;
    
    [Tooltip("按键")]
    public KeyCode keyCode;
    
    [Tooltip("输入类型")]
    public InputType inputType = InputType.KeyDown;
    
    [Tooltip("是否可重新映射")]
    public bool remappable = true;
    
    [Tooltip("描述")]
    public string description;
}

/// <summary>
/// 鼠标输入映射
/// </summary>
[System.Serializable]
public class MouseInputMapping
{
    [Tooltip("动作名称")]
    public string actionName;
    
    [Tooltip("鼠标按键")]
    public MouseButton mouseButton;
    
    [Tooltip("输入类型")]
    public InputType inputType = InputType.KeyDown;
    
    [Tooltip("是否可重新映射")]
    public bool remappable = true;
    
    [Tooltip("描述")]
    public string description;
}

/// <summary>
/// 手柄输入映射
/// </summary>
[System.Serializable]
public class GamepadInputMapping
{
    [Tooltip("动作名称")]
    public string actionName;
    
    [Tooltip("手柄按键")]
    public GamepadButton gamepadButton;
    
    [Tooltip("输入类型")]
    public InputType inputType = InputType.KeyDown;
    
    [Tooltip("是否可重新映射")]
    public bool remappable = true;
    
    [Tooltip("描述")]
    public string description;
}

/// <summary>
/// 输入类型枚举
/// </summary>
public enum InputType
{
    KeyDown,    // 按下
    KeyUp,      // 抬起
    KeyHold     // 持续按住
}

/// <summary>
/// 鼠标按键枚举
/// </summary>
public enum MouseButton
{
    Left = 0,
    Right = 1,
    Middle = 2,
    Button3 = 3,
    Button4 = 4
}

/// <summary>
/// 手柄按键枚举
/// </summary>
public enum GamepadButton
{
    A,
    B,
    X,
    Y,
    LeftBumper,
    RightBumper,
    LeftTrigger,
    RightTrigger,
    LeftStick,
    RightStick,
    DPadUp,
    DPadDown,
    DPadLeft,
    DPadRight,
    Start,
    Select
}