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
    [LabelText("NPC碰撞体")]
    public BoxCollider2D c2d;
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
    [LabelText("有未完成对话任务")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasUncompletedTalkObjective = false;
    [BoxGroup("运行时状态")]
    [LabelText("有可完成任务")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasCompletableQuests = false;

    // 私有变量
    private Transform playerTransform;
    private bool playerInRange = false;
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
            interactionRange = npcConfig.interactionRange;
            availableQuests = npcConfig.availableQuests;
        
            // 初始化任务对话块配置
            if (npcType == NPCType.QuestGiver)
            {
                // 这些字段会在GetDialogueBlockWithQuestLogic中使用
                // 无需额外的字段存储，直接从npcConfig读取
            }
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
                interactionPrompt = config.interactionPrompt;
                if (npcType == NPCType.QuestGiver)
                {
                    questIndicator = config.questIndicator;
                    questActiveIndicator = config.questActiveIndicator;
                    questCompleteIndicator = config.questCompleteIndicator;
                }

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
    }
    void OnDestroy()
    {
        BlockSignals.OnBlockStart -= OnBlockStart;
        BlockSignals.OnBlockEnd -= OnBlockEnd;
           if (QuestManager.Instance != null)
        {
            // 取消订阅任务事件
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnQuestProgressUpdated -= OnQuestProgressUpdated;
        }
    }

    private void OnBlockStart(Block block)
    {
        if (block.GetFlowchart() != dialogueFlowchart) return;

        // NPC 对话时播放动画
        // if (TryGetComponent(out Animator animator))
        // {
        //     animator.Play("Talking");
        // }
    }

    private void OnBlockEnd(Block block)
    {
        if (block.GetFlowchart() != dialogueFlowchart) return;

        // 对话结束逻辑
        // if (TryGetComponent(out Animator animator))
        // {
        //     animator.Play("Idle");
        // }
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
        if (!playerInRange || dialogueFlowchart == null)
        {
            return;
        }

        if (dialogueFlowchart == null)
        {
            Debug.LogError("Flowchart 未分配给 NPC: " + gameObject.name);
            return;
        }

        // 恶魔城风格：面向玩家
        FacePlayer();
        // 隐藏交互提示
        ShowInteractionPrompt(false);

        // 处理任务相关对话逻辑
        string blockToExecute = GetDialogueBlockWithQuestLogic();
        
        // 记录NPC对话事件到任务系统
        RecordNPCTalkEvent();

        if (dialogueFlowchart.ExecuteBlock(blockToExecute))
        {
            GamePauseManager.Instance.SetPaused(true);
            // 标记已经对话过
            hasSpokenBefore = true;
        }
        else
        {
            Debug.LogWarning($"[NPCController] 对话块 {blockToExecute} 不存在，尝试使用默认对话块");
            // 回退到原有逻辑
            string fallbackBlock =  npcConfig.defaultQuestBlock;
            if (dialogueFlowchart.ExecuteBlock(fallbackBlock))
            {
                GamePauseManager.Instance.SetPaused(true);
                hasSpokenBefore = true;
            }
        }
    }

    /// <summary>
    /// 根据任务状态获取对话块
    /// </summary>
    private string GetDialogueBlockWithQuestLogic()
    {
        // 如果不是任务NPC，使用原有逻辑
        if (npcType != NPCType.QuestGiver || npcConfig == null)
        {
            return npcConfig.defaultQuestBlock;
        }

        // 更新任务状态（确保状态是最新的）
        UpdateQuestStatus();
        
        // 设置Fungus变量，供对话流程图使用
        SetFungusQuestVariables();

        // 根据任务状态优先级选择对话块
        string questBlock = DetermineQuestDialogueBlock();
        
        if (!string.IsNullOrEmpty(questBlock))
        {
            return questBlock;
        }
        return npcConfig.defaultQuestBlock;

    }

    /// <summary>
    /// 确定任务对话块（按优先级）
    /// </summary>
    private string DetermineQuestDialogueBlock()
    {
        // 优先级1: 可完成任务
        if (hasCompletableQuests && !string.IsNullOrEmpty(npcConfig.completeQuestBlock))
        {
            return npcConfig.completeQuestBlock;
        }
        
        // 优先级2: 未完成对话任务
        if (hasUncompletedTalkObjective && !string.IsNullOrEmpty(npcConfig.uncompletedTalkObjectiveBlock))
        {
            return npcConfig.uncompletedTalkObjectiveBlock;
        }
        // 优先级3: 可接取任务
        if (hasAvailableQuests && !string.IsNullOrEmpty(npcConfig.availableQuestBlock))
        {
            return npcConfig.availableQuestBlock;
        }
        // 优先级4: 进行中任务
        if (hasActiveQuests && !string.IsNullOrEmpty(npcConfig.activeQuestBlock))
        {
            return npcConfig.activeQuestBlock;
        }
        
        // 优先级4: 默认任务对话
        if (!string.IsNullOrEmpty(npcConfig.defaultQuestBlock))
        {
            return npcConfig.defaultQuestBlock;
        }

        return null;
    }

    /// <summary>
    /// 设置Fungus变量供对话流程图使用
    /// </summary>
    private void SetFungusQuestVariables()
    {
        if (dialogueFlowchart == null) return;

        // 设置任务状态变量
        dialogueFlowchart.SetBooleanVariable("HasAvailableQuests", hasAvailableQuests);
        dialogueFlowchart.SetBooleanVariable("HasActiveQuests", hasActiveQuests);
        dialogueFlowchart.SetBooleanVariable("HasCompletableQuests", hasCompletableQuests);
        
        // 设置NPC信息变量
        dialogueFlowchart.SetStringVariable("NPCId", npcConfig?.npcId ?? gameObject.name);
        dialogueFlowchart.SetStringVariable("NPCName", npcName);
        
        // 设置首次对话标记
        dialogueFlowchart.SetBooleanVariable("IsFirstTalk", !hasSpokenBefore);
    }

    /// <summary>
    /// 记录NPC对话事件到任务系统
    /// </summary>
    private void RecordNPCTalkEvent()
    {
        if (QuestManager.Instance == null || npcConfig == null) return;
        
        // 使用NPC ID记录对话事件
        string npcId = !string.IsNullOrEmpty(npcConfig.npcId) ? npcConfig.npcId : gameObject.name;
        if (QuestManager.Instance.HasUncompletedTalkObjective(npcId))
        {
                    QuestManager.Instance.HandleNPCTalk(npcId);

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
        InitIndicator();
        // 更新任务状态
        UpdateQuestStatus();
    }
    private void InitIndicator()
    {
        if (c2d == null)
        {   
            c2d=GetComponent<BoxCollider2D>();
            if(c2d==null)
            {
                c2d = gameObject.AddComponent<BoxCollider2D>();
            }
        }
        Vector3 position = c2d.bounds.max + Vector3.left * c2d.bounds.extents.x+Vector3.up*0.5f;
       questIndicator= Instantiate(questIndicator, position, Quaternion.identity,transform);
       questActiveIndicator= Instantiate(questActiveIndicator, position, Quaternion.identity,transform);
      questCompleteIndicator= Instantiate(questCompleteIndicator, position, Quaternion.identity,transform);
    }
    /// <summary>
    /// 更新任务状态
    /// </summary>
    public void UpdateQuestStatus()
    {
        if (QuestManager.Instance == null) return;

        hasAvailableQuests = false;
        hasCompletableQuests = false;
        hasActiveQuests = false;
        hasUncompletedTalkObjective = false;
        // 检查可分发的任务
        foreach (var quest in availableQuests)
        {
            if (quest == null) continue;

            // 检查是否有可以开始的任务
            if (!givenQuestIds.Contains(quest.questId) &&
                !QuestManager.Instance.IsQuestActive(quest.questId) &&
                !QuestManager.Instance.IsQuestCompleted(quest.questId) &&
                QuestManager.Instance.CanStartQuest(quest.questId))
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

            //检查是否有未完成的对话任务
            if (QuestManager.Instance.HasUncompletedTalkObjective(npcConfig.npcId))
            {
                hasUncompletedTalkObjective = true;
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
            questActiveIndicator.SetActive(hasActiveQuests||hasUncompletedTalkObjective);
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
