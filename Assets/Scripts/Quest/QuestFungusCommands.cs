using UnityEngine;
using Fungus;

/// <summary>
/// Fungus自定义命令 - 分发任务
/// 用于在对话中分发任务给玩家
/// </summary>
[CommandInfo("Quest", 
             "Give Quest", 
             "分发任务给玩家")]
public class GiveQuestCommand : Command
{
    [Tooltip("要分发的任务ID")]
    [SerializeField] protected StringData questId = new StringData();
    
    [Tooltip("分发成功时的消息")]
    [SerializeField] protected StringData successMessage = new StringData("任务已接受！");
    
    [Tooltip("分发失败时的消息")]
    [SerializeField] protected StringData failureMessage = new StringData("无法接受此任务。");
    
    [Tooltip("是否显示结果消息")]
    [SerializeField] protected bool showResultMessage = true;

    public override void OnEnter()
    {
        var questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("[GiveQuestCommand] QuestManager 实例未找到");
            Continue();
            return;
        }

        string questIdValue = questId.Value;
        if (string.IsNullOrEmpty(questIdValue))
        {
            Debug.LogError("[GiveQuestCommand] 任务ID不能为空");
            Continue();
            return;
        }

        bool success = questManager.StartQuest(questIdValue);
        
        if (showResultMessage)
        {
            string message = success ? successMessage.Value : failureMessage.Value;
            
            // 如果当前Flowchart有Say命令，可以直接显示消息
            var flowchart = GetFlowchart();
            if (flowchart != null)
            {
                // 创建临时Say命令来显示消息
                var sayCommand = flowchart.gameObject.AddComponent<Say>();
                sayCommand.SetStandardText(message);
                sayCommand.OnEnter();
            }
            else
            {
                Debug.Log($"[GiveQuestCommand] {message}");
            }
        }

        Continue();
    }

    public override string GetSummary()
    {
        if (questId.Value == "")
        {
            return "错误: 未设置任务ID";
        }
        return "分发任务: " + questId.Value;
    }
}

/// <summary>
/// Fungus自定义命令 - 完成任务
/// 用于在对话中完成任务
/// </summary>
[CommandInfo("Quest", 
             "Complete Quest", 
             "完成指定任务")]
public class CompleteQuestCommand : Command
{
    [Tooltip("要完成的任务ID")]
    [SerializeField] protected StringData questId = new StringData();
    
    [Tooltip("完成成功时的消息")]
    [SerializeField] protected StringData successMessage = new StringData("任务已完成！");
    
    [Tooltip("完成失败时的消息")]
    [SerializeField] protected StringData failureMessage = new StringData("任务无法完成。");
    
    [Tooltip("是否显示结果消息")]
    [SerializeField] protected bool showResultMessage = true;

    public override void OnEnter()
    {
        var questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("[CompleteQuestCommand] QuestManager 实例未找到");
            Continue();
            return;
        }

        string questIdValue = questId.Value;
        if (string.IsNullOrEmpty(questIdValue))
        {
            Debug.LogError("[CompleteQuestCommand] 任务ID不能为空");
            Continue();
            return;
        }

        bool success = questManager.CompleteQuest(questIdValue);
        
        if (showResultMessage)
        {
            string message = success ? successMessage.Value : failureMessage.Value;
            
            var flowchart = GetFlowchart();
            if (flowchart != null)
            {
                var sayCommand = flowchart.gameObject.AddComponent<Say>();
                sayCommand.SetStandardText(message);
                sayCommand.OnEnter();
            }
            else
            {
                Debug.Log($"[CompleteQuestCommand] {message}");
            }
        }

        Continue();
    }

    public override string GetSummary()
    {
        if (questId.Value == "")
        {
            return "错误: 未设置任务ID";
        }
        return "完成任务: " + questId.Value;
    }
}

/// <summary>
/// Fungus自定义命令 - 检查任务状态
/// 用于在对话中检查任务状态并根据结果执行不同的对话分支
/// </summary>
[CommandInfo("Quest", 
             "Check Quest Status", 
             "检查任务状态")]
public class CheckQuestStatusCommand : Command
{
    [Tooltip("要检查的任务ID")]
    [SerializeField] protected StringData questId = new StringData();
    
    [Tooltip("要检查的任务状态")]
    [SerializeField] protected QuestStatus targetStatus = QuestStatus.InProgress;
    
    [Tooltip("状态匹配时跳转到的Block")]
    [SerializeField] protected Block targetBlock;
    
    [Tooltip("状态不匹配时跳转到的Block（可选）")]
    [SerializeField] protected Block elseBlock;

    public override void OnEnter()
    {
        var questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("[CheckQuestStatusCommand] QuestManager 实例未找到");
            Continue();
            return;
        }

        string questIdValue = questId.Value;
        if (string.IsNullOrEmpty(questIdValue))
        {
            Debug.LogError("[CheckQuestStatusCommand] 任务ID不能为空");
            Continue();
            return;
        }

        var quest = questManager.GetQuest(questIdValue);
        bool statusMatches = false;
        
        if (quest != null)
        {
            statusMatches = quest.questStatus == targetStatus;
        }
        else
        {
            // 如果任务不存在，检查是否要查找NotStarted状态
            statusMatches = targetStatus == QuestStatus.NotStarted;
        }

        if (statusMatches && targetBlock != null)
        {
            // 跳转到目标Block
            GetFlowchart().ExecuteBlock(targetBlock);
        }
        else if (!statusMatches && elseBlock != null)
        {
            // 跳转到else Block
            GetFlowchart().ExecuteBlock(elseBlock);
        }
        else
        {
            Continue();
        }
    }

    public override string GetSummary()
    {
        if (questId.Value == "")
        {
            return "错误: 未设置任务ID";
        }
        return $"检查任务 {questId.Value} 状态是否为 {targetStatus}";
    }
}

/// <summary>
/// Fungus自定义命令 - 检查任务是否可完成
/// 用于检查任务的所有目标是否都已完成
/// </summary>
[CommandInfo("Quest", 
             "Check Quest Completable", 
             "检查任务是否可完成")]
public class CheckQuestCompletableCommand : Command
{
    [Tooltip("要检查的任务ID")]
    [SerializeField] protected StringData questId = new StringData();
    
    [Tooltip("任务可完成时跳转到的Block")]
    [SerializeField] protected Block completableBlock;
    
    [Tooltip("任务不可完成时跳转到的Block（可选）")]
    [SerializeField] protected Block notCompletableBlock;

    public override void OnEnter()
    {
        var questManager = QuestManager.Instance;
        if (questManager == null)
        {
            Debug.LogError("[CheckQuestCompletableCommand] QuestManager 实例未找到");
            Continue();
            return;
        }

        string questIdValue = questId.Value;
        if (string.IsNullOrEmpty(questIdValue))
        {
            Debug.LogError("[CheckQuestCompletableCommand] 任务ID不能为空");
            Continue();
            return;
        }

        var quest = questManager.GetActiveQuest(questIdValue);
        bool canComplete = quest != null && quest.CanComplete();

        if (canComplete && completableBlock != null)
        {
            GetFlowchart().ExecuteBlock(completableBlock);
        }
        else if (!canComplete && notCompletableBlock != null)
        {
            GetFlowchart().ExecuteBlock(notCompletableBlock);
        }
        else
        {
            Continue();
        }
    }

    public override string GetSummary()
    {
        if (questId.Value == "")
        {
            return "错误: 未设置任务ID";
        }
        return $"检查任务 {questId.Value} 是否可完成";
    }
}