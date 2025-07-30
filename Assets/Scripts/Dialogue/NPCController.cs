using UnityEngine;
using Fungus;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

/// <summary>
/// NPC控制器 - 处理NPC与玩家的对话交互
/// 集成Fungus对话系统
/// </summary>
public class NPCController : MonoBehaviour, IInputListener
{
    [BoxGroup("NPC基础配置")]
    [LabelText("NPC配置文件")]
    [InfoBox("包含NPC的基本信息和对话设置")]
    public NPCConfig npcConfig;
    
    [BoxGroup("NPC基础配置")]
    [LabelText("NPC名称")]
    [ShowInInspector]
    [ReadOnly]
    private string npcName = "NPC";

    [BoxGroup("NPC基础配置")]
    [LabelText("NPC角色类型")]
    [ShowInInspector]
    [ReadOnly]
    private NPCType npcType = NPCType.Villager;
    
    [BoxGroup("NPC基础配置")]
    [LabelText("NPC描述")]
    [TextArea(3, 5)]
    [ShowInInspector]
    [ReadOnly]
    private string description = "";
    
    [BoxGroup("对话系统")]
    [LabelText("对话流程图")]
    [InfoBox("Fungus对话系统的Flowchart组件")]
    [Required]
    public Flowchart dialogueFlowchart;

    [BoxGroup("对话系统")]
    [LabelText("开始对话块")]
    [ShowInInspector]
    [ReadOnly]
    private string startBlockName = "Start";
    
    [BoxGroup("对话系统")]
    [LabelText("重复对话块")]
    [ShowInInspector]
    [ReadOnly]
    private string repeatBlockName = "Repeat";

    [BoxGroup("交互设置")]
    [LabelText("交互范围")]
    [ShowInInspector]
    [ReadOnly]
    [MinValue(0.1f)]
    private float interactionRange = 2f;
    
    [BoxGroup("交互设置")]
    [LabelText("交互偏移")]
    [ShowInInspector]
    [ReadOnly]
    private Vector3 promptOffset = new Vector3(0, 2, 0);

        [BoxGroup("任务系统")]
    [LabelText("任务可接取指示器")]
    [InfoBox("显示NPC有任务可接取的UI指示器")]    [ReadOnly]
    [SerializeField] private GameObject questIndicator;
        [BoxGroup("任务系统")]
    [LabelText("任务正在进行指示器")]
    [InfoBox("显示NPC有任务正在进行的UI指示器")]    [ReadOnly]
    [SerializeField] private GameObject questActiveIndicator;
    
    [BoxGroup("任务系统")]
    [LabelText("任务可完成指示器")]
    [InfoBox("显示NPC有任务可完成的UI指示器")]
    [ReadOnly]
    [SerializeField] private GameObject questCompleteIndicator;

        [BoxGroup("UI组件")]
    [LabelText("交互提示UI")]
    [InfoBox("玩家靠近时显示的交互提示")]
    [ReadOnly]
    private GameObject interactionPrompt;

    [BoxGroup("UI组件")]
    [LabelText("交互提示文本")]
    [ShowInInspector]
    [ReadOnly]
    private string promptText = "按 E 键对话";

    [BoxGroup("对话状态")]
    [LabelText("是否已对话")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasSpokenBefore = false;

    [BoxGroup("对话状态")]
    [LabelText("可重复对话")]
    [ShowInInspector]
    [ReadOnly]
    private bool canRepeatDialogue = true;

    [BoxGroup("任务系统", VisibleIf = "@npcType == NPCType.QuestGiver")]
    [LabelText("可分发任务")]
    [InfoBox("此NPC可以分发的任务列表")]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    [SerializeField] private List<QuestData> availableQuests = new List<QuestData>();
    

    

    [BoxGroup("运行时状态")]
    [LabelText("已分发任务ID")]
    [ShowInInspector]
    [ReadOnly]
    [ListDrawerSettings(ShowIndexLabels = true)]
    private List<string> givenQuestIds = new List<string>();
    
    [BoxGroup("运行时状态")]
    [LabelText("有可接任务")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasAvailableQuests = false;
    [BoxGroup("运行时状态")]
    [LabelText("有正在进行任务")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasActiveQuests = false;
    [BoxGroup("运行时状态")]
    [LabelText("有可完成任务")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasCompletableQuests = false;

    // 私有变量
    private Transform playerTransform;
    private bool playerInRange = false;
    private CircleCollider2D interactionCollider;
    private Enemy enemyComponent;
    // 事件
    void Awake()
    {
    }
    void Start()
    {

        // 实例化并激活
        try
        {
            initConfig();
                    InputManager.RegisterListener(this);
   
            if (dialogueFlowchart != null)
            {
             dialogueFlowchart = Instantiate(dialogueFlowchart, transform);
              //  return;
            }
            
          //  dialogueFlowchart.gameObject.SetActive(true);
            
            InitializeNPC();
            InitializeNpcTypeComponent();
            
            // 初始化任务系统
            if (npcType == NPCType.QuestGiver)
            {
                InitializeQuestSystem();
            }
            
            Debug.Log($"[NPCController] {gameObject.name}: 初始化完成");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NPCController] {gameObject.name}: 初始化失败 - {e.Message}");
            Debug.LogError($"[NPCController] 堆栈跟踪: {e.StackTrace}");
        }
    }
    private void InitializeNpcTypeComponent()
    {
        if (npcType == NPCType.Enemy)
        {
            //敌人触发器是子对象
            enemyComponent = GetComponentInParent<Enemy>();
            // enemyComponent.Animator.Play("Idle");
            if (enemyComponent != null)
            {

                enemyComponent.SetPaused(true);
                enemyComponent.setNPCPaused(true);
            }
            else
            {
                Debug.LogError("无法获取敌人类型NPC的ENEMY组件，请检查");
            }
        }
    }
    private void initConfig()
    {
        if (npcConfig)
        {
            npcName = npcConfig.npcName;
            npcType = npcConfig.npcType;
            description = npcConfig.description;
            promptOffset = npcConfig.promptOffset;
            startBlockName = npcConfig.firstDialogueBlock;
            repeatBlockName = npcConfig.repeatDialogueBlock;
            interactionRange = npcConfig.interactionRange;
            availableQuests = npcConfig.availableQuests;
        }
        else
        {
            Debug.LogWarning($"[NPCController] {gameObject.name}: npcConfig 未分配，使用默认配置");
        }
        
        //获取通用配置
        if (DialogueManager.Instance != null)
        {
            var config = DialogueManager.Instance.dialogueConfig;
            if (config)
            {
                promptText = config.interactPrompt;
                questIndicator = config.questIndicator;
                questActiveIndicator = config.questActiveIndicator;
                questCompleteIndicator = config.questCompleteIndicator;
                interactionPrompt = config.interactionPrompt;

            }
            else
            {
                Debug.LogWarning($"[NPCController] {gameObject.name}: DialogueManager.dialogueConfig 为空，使用默认交互设置");
            }
        }
        else
        {
            Debug.LogWarning($"[NPCController] {gameObject.name}: DialogueManager.Instance 为空，使用默认交互设置");
        }
    }

    // void Update()
    // {
    //     HandleInteraction();
    // }
    // 在 NPCController 中添加
    private void OnEnable()
    {
        // 注册 Fungus 事件

        BlockSignals.OnBlockStart += OnBlockStart;
        BlockSignals.OnBlockEnd += OnBlockEnd;
    }

    private void OnDisable()
    {
        // 注销 Fungus 事件
        BlockSignals.OnBlockStart -= OnBlockStart;
        BlockSignals.OnBlockEnd -= OnBlockEnd;
        // 注销事件
        // EventHandler.UnregisterEvent<Flowchart, Block>("BlockStart", OnBlockStart);
        // EventHandler.UnregisterEvent<Flowchart>("FlowchartEnd", OnBlockEnd);
    }
    void OnDestroy()
    {
        BlockSignals.OnBlockStart -= OnBlockStart;
        BlockSignals.OnBlockEnd -= OnBlockEnd;
    }

    private void OnBlockStart(Block block)
    {
        if (block.GetFlowchart() != dialogueFlowchart) return;

        // NPC 对话时播放动画
        if (TryGetComponent(out Animator animator))
        {
            animator.Play("Talking");
        }
    }

    private void OnBlockEnd(Block block)
    {
        if (block.GetFlowchart() != dialogueFlowchart) return;

        // 对话结束逻辑
        if (TryGetComponent(out Animator animator))
        {
            animator.Play("Idle");
        }
        // 恢复玩家控制
        GamePauseManager.Instance.SetPaused(false);
        //如果是敌人NPC恢复敌人控制
        if (enemyComponent != null)
        {

            enemyComponent.setNPCPaused(false);
        }
        // GameStateManager.Instance.ExitDialogueState();
    }
    /// <summary>
    /// 初始化NPC
    /// </summary>
    private void InitializeNPC()
    {
        // 设置交互提示
        SetupInteractionPrompt();

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
    /// 开始对话
    /// </summary>
    public void StartDialogue()
    {
        Debug.Log("大安" + dialogueFlowchart);
        if (!playerInRange || dialogueFlowchart == null)
        {
            return;
        }
        // 确定要执行的对话块
        string blockToExecute = GetDialogueBlock();

        if (dialogueFlowchart == null)
        {
            Debug.LogError("Flowchart 未分配给 NPC: " + gameObject.name);
            return;
        }


        // 恶魔城风格：面向玩家
        FacePlayer();
        // 隐藏交互提示
        ShowInteractionPrompt(false);
        // 触发对话开始事件

        if (dialogueFlowchart.ExecuteBlock(blockToExecute))
        {
            GamePauseManager.Instance.SetPaused(true);
            // 标记已经对话过
            hasSpokenBefore = true;

        }
        else
        {
            Debug.LogWarning($"[NPCController] 对话块 {blockToExecute} 不存在");
        }


    }
    private void FacePlayer()
    {
        if ((playerTransform.position.x > transform.position.x && transform.localScale.x < 0) || (playerTransform.position.x < transform.position.x && transform.localScale.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
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

            playerTransform = other.transform;
            playerInRange = true;
            // playerController = other.GetComponent<PlayerController>();
            ShowInteractionPrompt(true);

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
            // playerController = null;
            ShowInteractionPrompt(false);

        }
    }

    /// <summary>
    /// 重置对话状态
    /// </summary>
    public void ResetDialogueState()
    {
        hasSpokenBefore = false;
    }
    
    #region 任务系统方法
    
    /// <summary>
    /// 初始化任务系统
    /// </summary>
    private void InitializeQuestSystem()
    {
        if (QuestManager.Instance != null)
        {
            // 订阅任务事件
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnQuestProgressUpdated += OnQuestProgressUpdated;
        }
        
        // 更新任务状态
        UpdateQuestStatus();
    }
    
    /// <summary>
    /// 更新任务状态
    /// </summary>
    private void UpdateQuestStatus()
    {
        if (QuestManager.Instance == null) return;
        
        hasAvailableQuests = false;
        hasCompletableQuests = false;
        hasActiveQuests = false;
        // 检查可分发的任务
        foreach (var quest in availableQuests)
        {
            if (quest == null) continue;
            
            // 检查是否有可以开始的任务
            if (!givenQuestIds.Contains(quest.questId) && 
                !QuestManager.Instance.IsQuestActive(quest.questId) &&
                !QuestManager.Instance.IsQuestCompleted(quest.questId))
            {
                hasAvailableQuests = true;
            }
            
            // 检查是否有正在进行的任务
            if (QuestManager.Instance.IsQuestActive(quest.questId))
            {
                hasActiveQuests = true;
            }
            // 检查是否有可以完成的任务
            if (QuestManager.Instance.IsQuestActive(quest.questId))
            {
                var activeQuest = QuestManager.Instance.GetActiveQuest(quest.questId);
                if (activeQuest != null && activeQuest.CanComplete())
                {
                    hasCompletableQuests = true;
                }
            }
        }
        
        // 更新指示器
        UpdateQuestIndicators();
    }
    
    /// <summary>
    /// 更新任务指示器
    /// </summary>
    private void UpdateQuestIndicators()
    {
        if (questIndicator != null)
        {
            questIndicator.SetActive(hasAvailableQuests);
        }
        if (questActiveIndicator != null)
        {
            questActiveIndicator.SetActive(hasActiveQuests);
        }
        if (questCompleteIndicator != null)
        {
            questCompleteIndicator.SetActive(hasCompletableQuests);
        }
    }
    
    /// <summary>
    /// 分发任务
    /// </summary>
    public bool GiveQuest(string questId)
    {
        if (QuestManager.Instance == null) return false;
        
        var quest = availableQuests.Find(q => q.questId == questId);
        if (quest == null)
        {
            Debug.LogWarning($"[NPCController] NPC {gameObject.name} 没有任务 {questId}");
            return false;
        }
        
        if (QuestManager.Instance.StartQuest(questId))
        {
            givenQuestIds.Add(questId);
            UpdateQuestStatus();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 完成任务
    /// </summary>
    public bool CompleteQuest(string questId)
    {
        if (QuestManager.Instance == null) return false;
        
        if (QuestManager.Instance.CompleteQuest(questId))
        {
            UpdateQuestStatus();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取可分发的任务列表
    /// </summary>
    public List<QuestData> GetAvailableQuests()
    {
        var result = new List<QuestData>();
        
        foreach (var quest in availableQuests)
        {
            if (quest == null) continue;
            
            if (!givenQuestIds.Contains(quest.questId) && 
                !QuestManager.Instance.IsQuestActive(quest.questId) &&
                !QuestManager.Instance.IsQuestCompleted(quest.questId))
            {
                result.Add(quest);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 获取可完成的任务列表
    /// </summary>
    public List<QuestData> GetCompletableQuests()
    {
        var result = new List<QuestData>();
        
        foreach (var quest in availableQuests)
        {
            if (quest == null) continue;
            
            if (QuestManager.Instance.IsQuestActive(quest.questId))
            {
                var activeQuest = QuestManager.Instance.GetActiveQuest(quest.questId);
                if (activeQuest != null && activeQuest.CanComplete())
                {
                    result.Add(activeQuest);
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 任务开始事件处理
    /// </summary>
    private void OnQuestStarted(QuestData quest)
    {
        UpdateQuestStatus();
    }
    
    /// <summary>
    /// 任务完成事件处理
    /// </summary>
    private void OnQuestCompleted(QuestData quest)
    {
        UpdateQuestStatus();
    }
    
    /// <summary>
    /// 任务进度更新事件处理
    /// </summary>
    private void OnQuestProgressUpdated(QuestData quest)
    {
        UpdateQuestStatus();
    }
    
    #endregion



    /// <summary>
    /// 在Scene视图中绘制交互范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 绘制线框圆形表示交互范围
        UnityEngine.Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
      #region 输入监听实现
    // 只实现需要的交互输入
    public void OnInteractInput()
    {
            if (playerInRange)
        {
            StartDialogue();
        }
    }
    // 其他输入方法显式实现为空
    void IInputListener.OnMoveInput(Vector2 moveInput) { }          // 空实现
    void IInputListener.OnJumpInput() { }                          // 空实现
    void IInputListener.OnJumpReleased() { }                       // 空实现
    void IInputListener.OnAttackInput() { }                       // 空实现
    void IInputListener.OnSkillInput() { }                        // 空实现
    // void IInputListener.OnInteractInput() { }                     // 空实现
    void IInputListener.OnPauseInput() { }                        // 空实现
    void IInputListener.OnClimbDownInput() { }                    // 空实现
    void IInputListener.OnClimbUpInput() { }                      // 空实现
    #endregion
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
    Other,       // 其他
    Enemy        // 敌人
}
