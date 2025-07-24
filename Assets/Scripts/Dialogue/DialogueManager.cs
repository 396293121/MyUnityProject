using UnityEngine;
using Fungus;

/// <summary>
/// 对话管理器 - 统一管理游戏中的对话系统
/// 集成Fungus对话系统与项目现有系统
/// </summary>
public class DialogueManager : MonoBehaviour
{   
    [Header("对话系统配置")]
    public DialogueConfig dialogueConfig;
    [Tooltip("对话音效")]
    public AudioClip dialogueStartSound;
    public AudioClip dialogueEndSound;
    [Header("游戏集成")]
    [Tooltip("对话时是否暂停游戏")]
    public bool pauseGameDuringDialogue = true;
    
    [Tooltip("对话时是否隐藏游戏UI")]
    public bool hideGameUIDuringDialogue = true;
    
    // 单例模式
    public static DialogueManager Instance { get; private set; }
    
    // 当前对话状态
    public bool IsInDialogue { get; private set; }
    
    // 当前活动的Flowchart
    private Flowchart currentFlowchart;
    
    // 对话事件
    public System.Action OnDialogueStart;
    public System.Action OnDialogueEnd;
    public System.Action<string> OnDialogueTextChanged;
    
    [SerializeField] private SayDialog sayDialog;
    [SerializeField] private Flowchart flowchart;
    // 组件引用
    private AudioSource audioSource;
    
    void Awake()
    {
        
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDialogueManager();
        }
        else
        {
            Destroy(gameObject);
        }
        if(dialogueConfig)
        {
            dialogueStartSound = dialogueConfig.dialogueStartSound;
            dialogueEndSound = dialogueConfig.dialogueEndSound;
        }
    }
       void Start()
    {
        // 确保对话框在开始时是激活的
        // if (sayDialog != null)
        // {
        //     sayDialog.gameObject.SetActive(true);
            
        //     // 如果有SetActive方法，也调用它
        //     if (sayDialog.GetComponent<CanvasGroup>() != null)
        //     {
        //         sayDialog.GetComponent<CanvasGroup>().alpha = 1f;
        //         sayDialog.GetComponent<CanvasGroup>().interactable = true;
        //         sayDialog.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //     }
        // }
        
        // 延迟执行对话，确保所有组件都已初始化
        Invoke("StartDialogue", 0.5f);
    }
    
    void StartDialogue()
    {
        if (flowchart != null)
        {
            flowchart.ExecuteBlock("Start");
        }
    }
    /// <summary>
    /// 初始化对话管理器
    /// </summary>
    private void InitializeDialogueManager()
    {
        // 获取或添加音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 注册Fungus事件
        RegisterFungusEvents();
        
        Debug.Log("[DialogueManager] 对话管理器初始化完成");
    }
    
    /// <summary>
    /// 注册Fungus事件
    /// </summary>
    private void RegisterFungusEvents()
    {
        // 这里可以注册Fungus的全局事件
        // 例如监听所有Say命令的开始和结束
    }
    
    /// <summary>
    /// 开始对话
    /// </summary>
    /// <param name="flowchartName">流程图名称</param>
    /// <param name="blockName">对话块名称</param>
    /// <param name="character">说话角色</param>
    public void StartDialogue(string flowchartName, string blockName, Fungus.Character character = null)
    {
        if (IsInDialogue)
        {
            Debug.LogWarning("[DialogueManager] 已经在对话中，无法开始新对话");
            return;
        }
        
        // 查找Flowchart
        var flowchart = FindFlowchart(flowchartName);
        if (flowchart == null)
        {
            Debug.LogError($"[DialogueManager] 找不到名为 '{flowchartName}' 的Flowchart");
            return;
        }
        
        // 设置对话状态
        IsInDialogue = true;
        currentFlowchart = flowchart;
        
        // 处理游戏状态
        HandleGameStateForDialogue(true);
        
        // 播放开始音效
        PlayDialogueSound(dialogueStartSound);
        
        // 触发对话开始事件
        OnDialogueStart?.Invoke();
        
        // 执行对话块
        flowchart.ExecuteBlock(blockName);
        
        Debug.Log($"[DialogueManager] 开始对话: {flowchartName}.{blockName}");
    }
    
    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        if (!IsInDialogue)
        {
            return;
        }
        
        // 重置对话状态
        IsInDialogue = false;
        currentFlowchart = null;
        
        // 恢复游戏状态
        HandleGameStateForDialogue(false);
        
        // 播放结束音效
        PlayDialogueSound(dialogueEndSound);
        
        // 触发对话结束事件
        OnDialogueEnd?.Invoke();
        
        Debug.Log("[DialogueManager] 对话结束");
    }
    
    /// <summary>
    /// 处理对话期间的游戏状态
    /// </summary>
    private void HandleGameStateForDialogue(bool startDialogue)
    {
        if (startDialogue)
        {
            // 开始对话时的处理
            if (pauseGameDuringDialogue)
            {
                Time.timeScale = 0f;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeGameState(GameState.Paused);
                }
            }
            
            if (hideGameUIDuringDialogue && UIManager.Instance != null)
            {
            //    UIManager.Instance.HidePanel("Gameplay");
            }
        }
        else
        {
            // 结束对话时的处理
            if (pauseGameDuringDialogue)
            {
                Time.timeScale = 1f;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeGameState(GameState.Playing);
                }
            }
            
            if (hideGameUIDuringDialogue && UIManager.Instance != null)
            {
                UIManager.Instance.ShowPanel("Gameplay");
            }
        }
    }
    
    /// <summary>
    /// 播放对话音效
    /// </summary>
    private void PlayDialogueSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// 查找Flowchart
    /// </summary>
    private Flowchart FindFlowchart(string flowchartName)
    {
        // 在场景中查找指定名称的Flowchart
        var flowchartObject = GameObject.Find(flowchartName);
        if (flowchartObject != null)
        {
            var flowchart = flowchartObject.GetComponent<Flowchart>();
            if (flowchart != null)
            {
                return flowchart;
            }
        }
        
        // 查找所有Flowchart组件
        var allFlowcharts = FindObjectsOfType<Flowchart>();
        foreach (var fc in allFlowcharts)
        {
            if (fc.name == flowchartName || fc.gameObject.name == flowchartName)
            {
                return fc;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 获取当前对话文本
    /// </summary>
    public string GetCurrentDialogueText()
    {
        if (!IsInDialogue || currentFlowchart == null)
        {
            return "";
        }
        
        // 这里可以实现获取当前显示文本的逻辑
        // 需要根据具体的SayDialog实现
        var sayDialog = SayDialog.GetSayDialog();
        if (sayDialog != null)
        {
            return sayDialog.StoryText;
        }
        
        return "";
    }
    
    /// <summary>
    /// 跳过当前对话
    /// </summary>
    public void SkipCurrentDialogue()
    {
        if (!IsInDialogue)
        {
            return;
        }
        
        var sayDialog = SayDialog.GetSayDialog();
        if (sayDialog != null)
        {
            // 触发继续对话
            var writer = sayDialog.GetComponent<Writer>();
            if (writer != null && writer.IsWriting)
            {
                writer.Stop();
            }
        }
    }
    
    /// <summary>
    /// 设置对话变量
    /// </summary>
    public void SetDialogueVariable(string variableName, object value)
    {
        if (currentFlowchart == null)
        {
            Debug.LogWarning("[DialogueManager] 没有活动的Flowchart，无法设置变量");
            return;
        }
        
        // 根据值类型设置变量
        if (value is bool boolValue)
        {
            currentFlowchart.SetBooleanVariable(variableName, boolValue);
        }
        else if (value is int intValue)
        {
            currentFlowchart.SetIntegerVariable(variableName, intValue);
        }
        else if (value is float floatValue)
        {
            currentFlowchart.SetFloatVariable(variableName, floatValue);
        }
        else if (value is string stringValue)
        {
            currentFlowchart.SetStringVariable(variableName, stringValue);
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] 不支持的变量类型: {value.GetType()}");
        }
    }
    
    /// <summary>
    /// 获取对话变量
    /// </summary>
    public T GetDialogueVariable<T>(string variableName)
    {
        if (currentFlowchart == null)
        {
            Debug.LogWarning("[DialogueManager] 没有活动的Flowchart，无法获取变量");
            return default(T);
        }
        
        var variable = currentFlowchart.GetVariable(variableName);
        if (variable != null)
        {
            if (variable is BooleanVariable boolVar && typeof(T) == typeof(bool))
            {
                return (T)(object)boolVar.Value;
            }
            else if (variable is IntegerVariable intVar && typeof(T) == typeof(int))
            {
                return (T)(object)intVar.Value;
            }
            else if (variable is FloatVariable floatVar && typeof(T) == typeof(float))
            {
                return (T)(object)floatVar.Value;
            }
            else if (variable is StringVariable stringVar && typeof(T) == typeof(string))
            {
                return (T)(object)stringVar.Value;
            }
        }
        
        return default(T);
    }
    
    /// <summary>
    /// 检查是否有指定的对话块
    /// </summary>
    public bool HasDialogueBlock(string flowchartName, string blockName)
    {
        var flowchart = FindFlowchart(flowchartName);
        if (flowchart != null)
        {
            return flowchart.HasBlock(blockName);
        }
        return false;
    }
    
    /// <summary>
    /// 获取所有可用的Flowchart名称
    /// </summary>
    public string[] GetAvailableFlowcharts()
    {
        var allFlowcharts = FindObjectsOfType<Flowchart>();
        var names = new string[allFlowcharts.Length];
        for (int i = 0; i < allFlowcharts.Length; i++)
        {
            names[i] = allFlowcharts[i].name;
        }
        return names;
    }
}