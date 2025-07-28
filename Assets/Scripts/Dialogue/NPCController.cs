using UnityEngine;
using Fungus;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// NPC控制器 - 处理NPC与玩家的对话交互
/// 集成Fungus对话系统
/// </summary>
public class NPCController : MonoBehaviour
{
    [LabelText("NPC配置")]
    public NPCConfig npcConfig;
    [LabelText("NPC基础配置")]
    [ShowInInspector]
    [ReadOnly]
    private string npcName = "NPC";

    [LabelText("NPC角色类型")]
    [ShowInInspector]
    [ReadOnly]
    private NPCType npcType = NPCType.Villager;
    [TextArea(3, 5)]
    [LabelText("NPC描述")]
    [ShowInInspector]
    [ReadOnly]
    private string description = "";
    [LabelText("对话FLOWCHART")]
    [Required]
    public Flowchart dialogueFlowchart;

    private Transform playerTransform;
    [LabelText("交互偏移")]
    [ShowInInspector]
    [ReadOnly]
    private Vector3 promptOffset = new Vector3(0, 2, 0);
    [LabelText("对话块")]
    [ShowInInspector]
    [ReadOnly]
    private string startBlockName = "Start";
    [LabelText("重复对话块")]
    [ShowInInspector]
    [ReadOnly]
    private string repeatBlockName = "Repeat";

    [LabelText("交互设置")]
    [ShowInInspector]
    [ReadOnly]
    private float interactionRange = 2f;
    [LabelText("交互键")]
    [ShowInInspector]
    [ReadOnly]
    private KeyCode interactionKey = KeyCode.E;

    [LabelText("交互提示UI")]
    public GameObject interactionPrompt;

    [LabelText("交互提示文本")]
    [ShowInInspector]
    [ReadOnly]
    private string promptText = "按 E 键对话";

    [LabelText("是否已经对话过")]
    [ShowInInspector]
    [ReadOnly]
    private bool hasSpokenBefore = false;

    [LabelText("是否可以重复对话")]
    [ShowInInspector]
    [ReadOnly]
    private bool canRepeatDialogue = true;

    // 私有变量
    private bool playerInRange = false;
    // private PlayerController playerController;  
    private CircleCollider2D interactionCollider;
    private Enemy enemyComponent;
    // 事件
    void Awake()
    {
    }
    void Start()
    {
        // 实例化并激活

        initConfig();
        var flowchartInstance = Instantiate(dialogueFlowchart, transform);
        flowchartInstance.gameObject.SetActive(true);
        dialogueFlowchart = flowchartInstance;
        InitializeNPC();
        InitializeNpcTypeComponent();
    }
    private void InitializeNpcTypeComponent()
    {
        if (npcType == NPCType.Enemy)
        {
            //敌人触发器是子对象
            enemyComponent = GetComponentInParent<Enemy>();
            // enemyComponent.Animator.Play("Idle");
            enemyComponent.SetPaused(true);
            enemyComponent.setNPCPaused(true);
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
        }
        //获取通用配置
        var config = DialogueManager.Instance.dialogueConfig;
        if (config)
        {
            interactionKey = config.interactKey;
            promptText = config.interactPrompt;
        }
    }

    void Update()
    {
        HandleInteraction();
    }
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
        if (dialogueFlowchart != null)
        {
            Destroy(dialogueFlowchart.gameObject);
        }

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
    Other,       // 其他
    Enemy        // 敌人
}
