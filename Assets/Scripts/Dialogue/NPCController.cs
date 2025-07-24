using UnityEngine;
using Fungus;
using Sirenix.OdinInspector;

/// <summary>
/// NPC控制器 - 处理NPC与玩家的对话交互
/// 集成Fungus对话系统
/// </summary>
public class NPCController : MonoBehaviour
{
    [Header("NPC配置")]
    [Tooltip("NPC配置文件")]
    public NPCConfig npcConfig;
    [Header("NPC基础配置")]
    [Tooltip("NPC显示名称")]
    [ShowInInspector] [ReadOnly]
    private string npcName = "NPC";
    
    [Tooltip("NPC角色类型")]
    private NPCType npcType = NPCType.Villager;
      [Tooltip("NPC描述")]
    [TextArea(3, 5)]
    [ShowInInspector] [ReadOnly]
    private string description = "";    
    [Header("对话配置")]
    [Tooltip("对话流程组件")]
    public Flowchart dialogueFlowchart;
       [Tooltip("交互提示偏移")]
    [ShowInInspector] [ReadOnly]
    private Vector3 promptOffset = new Vector3(0, 2, 0);
    [Tooltip("初始对话块名称")]
    [ShowInInspector] [ReadOnly]
    private string startBlockName = "Start";
    
    [Tooltip("重复对话块名称")]
    [ShowInInspector] [ReadOnly]
    private string repeatBlockName = "Repeat";
    
    [Header("交互设置")]
    [Tooltip("交互范围")]
    [ShowInInspector] [ReadOnly]
    private float interactionRange = 2f;
    
    [Tooltip("交互按键")]
    [ShowInInspector] [ReadOnly]
    private KeyCode interactionKey = KeyCode.E;
    
    [Header("UI组件")]
    [Tooltip("交互提示UI")]
    public GameObject interactionPrompt;
    
    [Tooltip("提示文本")]
    [ShowInInspector] [ReadOnly]
    private string promptText = "按 E 键对话";
    
    [Header("状态管理")]
    [Tooltip("是否已经对话过")]
    [ShowInInspector] [ReadOnly]
    private bool hasSpokenBefore = false;
    
    [Tooltip("是否可以重复对话")]
    [ShowInInspector] [ReadOnly]
    private bool canRepeatDialogue = true;
    
    // 私有变量
    private bool playerInRange = false;
    private PlayerController playerController;
    private FungusCharacterAdapter characterAdapter;
    private CircleCollider2D interactionCollider;
    
    // 事件
    void Awake()
    {
            if(npcConfig)
            {
                npcName = npcConfig.npcName;
                npcType = npcConfig.npcType;
                description = npcConfig.description;
                promptOffset = npcConfig.promptOffset;
                startBlockName = npcConfig.firstDialogueBlock;
                repeatBlockName = npcConfig.repeatDialogueBlock;
                interactionRange = npcConfig.interactionRange;
            }
            //获取通用配置
        var config = DialogueManager.Instance.dialogueConfig;
        if(config){
            interactionKey = config.interactKey;
            promptText = config.interactPrompt;
        }
    }
    void Start()
    {
        InitializeNPC();
    }
    
    void Update()
    {
        HandleInteraction();
    }
    
    /// <summary>
    /// 初始化NPC
    /// </summary>
    private void InitializeNPC()
    {
        // 设置交互提示
        SetupInteractionPrompt();
        
        // 获取角色适配器
        characterAdapter = GetComponent<FungusCharacterAdapter>();
        if (characterAdapter == null)
        {
            characterAdapter = gameObject.AddComponent<FungusCharacterAdapter>();
            characterAdapter.defaultFlowchart = dialogueFlowchart;
            characterAdapter.defaultStartBlock = startBlockName;
        }
        
        Debug.Log($"[NPCController] {npcName} 初始化完成");
    }
  
    
    /// <summary>
    /// 设置交互提示
    /// </summary>
    private void SetupInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            
            // 设置提示文本
            var promptTextComponent = interactionPrompt.GetComponentInChildren<UnityEngine.UI.Text>();
            if (promptTextComponent != null)
            {
                promptTextComponent.text = promptText;
            }
        }
    }
    
    
    /// <summary>
    /// 处理交互输入
    /// </summary>
    private void HandleInteraction()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            StartDialogue();
        }
    }
    
    /// <summary>
    /// 开始对话
    /// </summary>
    public void StartDialogue()
    {
        if (!playerInRange || dialogueFlowchart == null)
        {
            Debug.LogWarning("[NPCController] 玩家不在交互范围内或对话流程图未设置");
            return;
        }
        
        // 检查是否可以开始对话
        if (characterAdapter != null && !characterAdapter.CanStartDialogue())
        {
            Debug.LogWarning("[NPCController] 角色不允许开始对话");
            return;
        }
        
        // 确定要执行的对话块
        string blockToExecute = GetDialogueBlock();
        
       
        
        // 隐藏交互提示
        ShowInteractionPrompt(false);
        
        // 触发对话开始事件
        
        // 开始对话
        if (characterAdapter != null)
        {
            characterAdapter.StartDialogue(dialogueFlowchart, blockToExecute);
        }
        else
        {
            dialogueFlowchart.ExecuteBlock(blockToExecute);
        }
        // 标记已经对话过
        hasSpokenBefore = true;
        
        Debug.Log($"[NPCController] 开始与 {npcName} 的对话，执行块: {blockToExecute}");
    }
    
    /// <summary>
    /// 获取要执行的对话块
    /// </summary>
    private string GetDialogueBlock()
    {
        if (hasSpokenBefore && canRepeatDialogue && !string.IsNullOrEmpty(repeatBlockName))
        {
            return repeatBlockName;
        }
        return startBlockName;
    }
    
    /// <summary>
    /// 对话结束回调
    /// </summary>
    public void OnDialogueComplete()
    {
        // 恢复游戏
        ResumeGame();
        
        // 显示交互提示（如果玩家还在范围内）
        if (playerInRange)
        {
            ShowInteractionPrompt(true);
        }
        
        
        Debug.Log($"[NPCController] 与 {npcName} 的对话结束");
    }
    

    /// <summary>
    /// 恢复游戏
    /// </summary>
    private void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Playing);
        }
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// 显示/隐藏交互提示
    /// </summary>
    private void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(show);
        }
    }
    
    /// <summary>
    /// 玩家进入交互范围
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<PlayerController>();
            ShowInteractionPrompt(true);
            
            Debug.Log($"[NPCController] 玩家进入 {npcName} 的交互范围");
        }
    }
    
    /// <summary>
    /// 玩家离开交互范围
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerController = null;
            ShowInteractionPrompt(false);
            
            Debug.Log($"[NPCController] 玩家离开 {npcName} 的交互范围");
        }
    }
    
    /// <summary>
    /// 重置对话状态
    /// </summary>
    public void ResetDialogueState()
    {
        hasSpokenBefore = false;
        Debug.Log($"[NPCController] 重置 {npcName} 的对话状态");
    }
    

    
    /// <summary>
    /// 在Scene视图中绘制交互范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
       // 绘制线框圆形表示交互范围
       UnityEngine.Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

/// <summary>
/// NPC类型枚举
/// </summary>
public enum NPCType
{
    Villager,    // 村民
    Merchant,    // 商人
    Guard,       // 守卫
    QuestGiver,  // 任务发布者
    Trainer,     // 训练师
    Other        // 其他
}