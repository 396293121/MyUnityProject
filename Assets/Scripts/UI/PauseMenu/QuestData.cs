using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 任务数据 - 表示游戏中的一个任务
/// </summary>
[System.Serializable]
public class QuestData
{
    [LabelText("任务ID")]
    public string questId;
    
    [LabelText("任务名称")]
    public string questName;
    
    [LabelText("任务描述")]
    [TextArea(3, 5)]
    public string questDescription;
    
    [LabelText("任务类型")]
    public QuestType questType = QuestType.Main;
    
    [LabelText("任务状态")]
    public QuestStatus questStatus = QuestStatus.NotStarted;
    
    [LabelText("任务目标")]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [LabelText("任务奖励")]
    public QuestReward reward;
    
    [LabelText("前置任务")]
    public List<string> prerequisiteQuests = new List<string>();
    
    [LabelText("任务等级要求")]
    public int requiredLevel = 1;
    
    [LabelText("任务图标")]
    public Sprite questIcon;
    
    // 运行时数据
    [System.NonSerialized]
    public System.DateTime startTime;
    [System.NonSerialized]
    public System.DateTime completeTime;
    
    /// <summary>
    /// 获取任务进度百分比
    /// </summary>
    public float GetProgressPercentage()
    {
        if (objectives.Count == 0) return 0f;
        
        int completedObjectives = 0;
        foreach (var objective in objectives)
        {
            if (objective.isCompleted)
            {
                completedObjectives++;
            }
        }
        
        return (float)completedObjectives / objectives.Count;
    }
    
    /// <summary>
    /// 检查任务是否可以完成
    /// </summary>
    public bool CanComplete()
    {
        foreach (var objective in objectives)
        {
            if (!objective.isCompleted)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// 开始任务
    /// </summary>
    public void StartQuest()
    {
        questStatus = QuestStatus.InProgress;
        startTime = System.DateTime.Now;
    }
    
    /// <summary>
    /// 完成任务
    /// </summary>
    public void CompleteQuest()
    {
        questStatus = QuestStatus.Completed;
        completeTime = System.DateTime.Now;
    }
    
    /// <summary>
    /// 失败任务
    /// </summary>
    public void FailQuest()
    {
        questStatus = QuestStatus.Failed;
    }
}

/// <summary>
/// 任务目标
/// </summary>
[System.Serializable]
public class QuestObjective
{
    [LabelText("目标描述")]
    public string description;
    
    [LabelText("目标类型")]
    public ObjectiveType objectiveType;
    
    [LabelText("目标ID")]
    public string targetId;
    
    [LabelText("当前进度")]
    public int currentProgress = 0;
    
    [LabelText("目标进度")]
    public int targetProgress = 1;
    
    [LabelText("是否完成")]
    [ReadOnly]
    public bool isCompleted = false;
    
    /// <summary>
    /// 更新进度
    /// </summary>
    public void UpdateProgress(int progress)
    {
        currentProgress = Mathf.Min(progress, targetProgress);
        isCompleted = currentProgress >= targetProgress;
    }
    
    /// <summary>
    /// 增加进度
    /// </summary>
    public void AddProgress(int amount = 1)
    {
        UpdateProgress(currentProgress + amount);
    }
    
    /// <summary>
    /// 获取进度文本
    /// </summary>
    public string GetProgressText()
    {
        return $"{currentProgress}/{targetProgress}";
    }
}

/// <summary>
/// 任务奖励
/// </summary>
[System.Serializable]
public class QuestReward
{
    [LabelText("经验值奖励")]
    public int experienceReward = 0;
    
    [LabelText("金币奖励")]
    public int goldReward = 0;
    
    [LabelText("物品奖励")]
    public List<ItemReward> itemRewards = new List<ItemReward>();
}

/// <summary>
/// 物品奖励
/// </summary>
[System.Serializable]
public class ItemReward
{
    [LabelText("物品ID")]
    public string itemId;
    
    [LabelText("物品数量")]
    public int quantity = 1;
}

/// <summary>
/// 任务类型枚举
/// </summary>
public enum QuestType
{
    [LabelText("主线任务")]
    Main,
    [LabelText("支线任务")]
    Side,
    [LabelText("日常任务")]
    Daily,
    [LabelText("成就任务")]
    Achievement
}

/// <summary>
/// 任务状态枚举
/// </summary>
public enum QuestStatus
{
    [LabelText("未开始")]
    NotStarted,
    [LabelText("进行中")]
    InProgress,
    [LabelText("已完成")]
    Completed,
    [LabelText("已失败")]
    Failed,
    [LabelText("已放弃")]
    Abandoned
}

/// <summary>
/// 目标类型枚举
/// </summary>
public enum ObjectiveType
{
    [LabelText("击杀敌人")]
    KillEnemy,
    [LabelText("收集物品")]
    CollectItem,
    [LabelText("到达地点")]
    ReachLocation,
    [LabelText("与NPC对话")]
    TalkToNPC,
    [LabelText("使用物品")]
    UseItem,
    [LabelText("完成副本")]
    CompleteDungeon,
    [LabelText("自定义")]
    Custom
}