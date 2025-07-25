using UnityEngine;
using Fungus;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 改进的对话事件管理器
/// 解决 Block.OnBlockExecuted 过度触发的问题
/// 提供精确的对话开始/结束事件监听
/// </summary>
public class ImprovedDialogueEventManager : MonoBehaviour
{
    [Header("对话检测配置")]
    [Tooltip("是否启用调试日志")]
    public bool enableDebugLogs = true;
    
    [Tooltip("对话暂停过渡时间")]
    public float fadeDuration = 0.5f;
    
    [Tooltip("是否在对话期间播放待机动画")]
    public bool playIdleAnimationDuringDialogue = true;

    [Header("动画控制")]
    [Tooltip("对话期间的动画速度倍数")]
    [Range(0f, 1f)]
    public float dialogueAnimationSpeed = 0.3f;

    // 私有变量
    private bool isInDialogue = false;
    private HashSet<string> dialogueCommands = new HashSet<string>
    {
        "Say", "Menu", "SetSayDialog", "Portrait", "Stage"
    };
    
    // 缓存的组件引用
    private PlayerController playerController;
    private List<Enemy> enemies = new List<Enemy>();[Header("Continue Button修复")]
    [Tooltip("自动修复Continue Button")]
    // 原始动画速度缓存
    private Dictionary<Animator, float> originalAnimatorSpeeds = new Dictionary<Animator, float>();

    void Start()
    {
        InitializeComponents();
        RegisterEvents();
    }

    void OnDestroy()
    {
        UnregisterEvents();
    }

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 获取玩家控制器
        playerController = FindObjectOfType<PlayerController>();
        
        // 获取所有敌人
        Enemy[] enemyArray = FindObjectsOfType<Enemy>();
        enemies.AddRange(enemyArray);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[ImprovedDialogueEventManager] 初始化完成 - 玩家: {(playerController != null ? "找到" : "未找到")}, 敌人数量: {enemies.Count}");
        }
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEvents()
    {
        // 监听 Block 开始和结束事件
        BlockSignals.OnBlockStart += OnBlockStart;
        BlockSignals.OnBlockEnd += OnBlockEnd;
        BlockSignals.OnCommandExecute += OnCommandExecute;
        
        if (enableDebugLogs)
        {
            Debug.Log("[ImprovedDialogueEventManager] 事件监听已注册");
        }
    }

    /// <summary>
    /// 取消事件监听
    /// </summary>
    private void UnregisterEvents()
    {
        BlockSignals.OnBlockStart -= OnBlockStart;
        BlockSignals.OnBlockEnd -= OnBlockEnd;
        BlockSignals.OnCommandExecute -= OnCommandExecute;
    }

    /// <summary>
    /// Block 开始事件处理
    /// </summary>
    private void OnBlockStart(Block block)
    {
        // 检查是否包含对话命令
        if (ContainsDialogueCommands(block))
        {
            if (!isInDialogue)
            {
                StartDialogue();
            }
        }
    }

    /// <summary>
    /// Block 结束事件处理
    /// </summary>
    private void OnBlockEnd(Block block)
    {
        // 检查是否是对话 Block 结束
        if (ContainsDialogueCommands(block))
        {
            // 延迟检查是否还有其他对话 Block 在执行
            StartCoroutine(CheckDialogueEndDelayed());
        }
    }

    /// <summary>
    /// 命令执行事件处理
    /// </summary>
    private void OnCommandExecute(Block block, Command command, int commandIndex, int maxCommandIndex)
    {
        string commandType = command.GetType().Name;
        
        // 检测到对话命令开始
        if (dialogueCommands.Contains(commandType))
        {
            if (!isInDialogue)
            {
                StartDialogue();
            }
        }
    }

    /// <summary>
    /// 检查 Block 是否包含对话命令
    /// </summary>
    private bool ContainsDialogueCommands(Block block)
    {
        if (block == null || block.CommandList == null) return false;

        foreach (var command in block.CommandList)
        {
            if (command != null && dialogueCommands.Contains(command.GetType().Name))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 延迟检查对话是否结束
    /// </summary>
    private IEnumerator CheckDialogueEndDelayed()
    {
        yield return new WaitForSeconds(0.1f); // 等待一小段时间
        
        // 检查是否还有对话相关的 Block 在执行
        bool hasActiveDialogue = false;
        
        // 查找所有活动的 Flowchart
        Flowchart[] flowcharts = FindObjectsOfType<Flowchart>();
        foreach (var flowchart in flowcharts)
        {
            if (flowchart.HasExecutingBlocks())
            {
                // 检查执行中的 Block 是否包含对话命令
                foreach (var block in flowchart.GetComponents<Block>())
                {
                    if (block.IsExecuting() && ContainsDialogueCommands(block))
                    {
                        hasActiveDialogue = true;
                        break;
                    }
                }
            }
            if (hasActiveDialogue) break;
        }

        if (!hasActiveDialogue && isInDialogue)
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    private void StartDialogue()
    {
        if (isInDialogue) return;
        
        isInDialogue = true;
        
        if (enableDebugLogs)
        {
            Debug.Log("[ImprovedDialogueEventManager] 对话开始");
        }

        // 暂停游戏
        StartCoroutine(GamePauseManager.Instance.SmoothPause(true, fadeDuration));
        GamePauseManager.Instance.SetPaused(true);

        // 处理动画
        if (playIdleAnimationDuringDialogue)
        {
            SetDialogueAnimationState(true);
        }

        // 通知其他系统
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnDialogueStart();
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    private void EndDialogue()
    {
        if (!isInDialogue) return;
        
        isInDialogue = false;
        
        if (enableDebugLogs)
        {
            Debug.Log("[ImprovedDialogueEventManager] 对话结束");
        }

        // 恢复游戏
        StartCoroutine(GamePauseManager.Instance.SmoothPause(false, fadeDuration));
        GamePauseManager.Instance.SetPaused(false);

        // 恢复动画
        if (playIdleAnimationDuringDialogue)
        {
            SetDialogueAnimationState(false);
        }

        // 通知其他系统
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnDialogueEnd();
        }
    }

    /// <summary>
    /// 设置对话期间的动画状态
    /// </summary>
    private void SetDialogueAnimationState(bool inDialogue)
    {
        if (inDialogue)
        {
            // 保存原始动画速度并设置对话期间的速度
            SaveAndSetAnimationSpeeds();
        }
        else
        {
            // 恢复原始动画速度
            RestoreAnimationSpeeds();
        }
    }

    /// <summary>
    /// 保存并设置动画速度
    /// </summary>
    private void SaveAndSetAnimationSpeeds()
    {
        originalAnimatorSpeeds.Clear();

        // 处理玩家动画
        if (playerController != null)
        {
            var playerAnimator = playerController.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                originalAnimatorSpeeds[playerAnimator] = playerAnimator.speed;
                playerAnimator.speed = dialogueAnimationSpeed;
                
                // 不要强制设置Speed为0，让玩家保持当前动画状态
                // 如果玩家在移动，会自然切换到待机动画
                // playerAnimator.SetFloat("Speed", 0f); // 移除这行
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[ImprovedDialogueEventManager] 设置玩家动画速度: {dialogueAnimationSpeed}");
                }
            }
        }

        // 处理敌人动画
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                var enemyAnimator = enemy.GetComponent<Animator>();
                if (enemyAnimator != null)
                {
                    originalAnimatorSpeeds[enemyAnimator] = enemyAnimator.speed;
                    enemyAnimator.speed = dialogueAnimationSpeed;
                    
                    // 同样不强制设置敌人的Speed参数
                    // enemyAnimator.SetFloat("Speed", 0f); // 移除这行
                }
            }
        }
    }

    /// <summary>
    /// 恢复动画速度
    /// </summary>
    private void RestoreAnimationSpeeds()
    {
        foreach (var kvp in originalAnimatorSpeeds)
        {
            if (kvp.Key != null)
            {
                kvp.Key.speed = kvp.Value;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[ImprovedDialogueEventManager] 恢复动画速度: {kvp.Value}");
                }
            }
        }
        
        originalAnimatorSpeeds.Clear();
    }

    /// <summary>
    /// 手动开始对话（供外部调用）
    /// </summary>
    public void ManualStartDialogue()
    {
        StartDialogue();
    }

    /// <summary>
    /// 手动结束对话（供外部调用）
    /// </summary>
    public void ManualEndDialogue()
    {
        EndDialogue();
    }

    /// <summary>
    /// 获取当前对话状态
    /// </summary>
    public bool IsInDialogue => isInDialogue;

    /// <summary>
    /// 添加自定义对话命令类型
    /// </summary>
    public void AddDialogueCommandType(string commandType)
    {
        dialogueCommands.Add(commandType);
    }

    /// <summary>
    /// 移除对话命令类型
    /// </summary>
    public void RemoveDialogueCommandType(string commandType)
    {
        dialogueCommands.Remove(commandType);
    }
}