using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 任务数据 - 表示游戏中的一个任务
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [BoxGroup("基本信息")]
    [LabelText("任务ID")]
    [InfoBox("唯一标识符，用于区分不同任务")]
    public string questId;
    
    [BoxGroup("基本信息")]
    [LabelText("任务名称")]
    public string questName;
    
    [BoxGroup("基本信息")]
    [LabelText("任务描述")]
    [TextArea(3, 5)]
    public string description;
    
    [BoxGroup("基本信息")]
    [LabelText("任务类型")]
    public QuestType questType = QuestType.SideQuest;
    
    [BoxGroup("基本信息")]
    [LabelText("任务图标")]
    public Sprite questIcon;
    
    [BoxGroup("状态与要求")]
    [LabelText("任务状态")]
    [ReadOnly]
    public QuestStatus questStatus = QuestStatus.NotStarted;
    
    [BoxGroup("状态与要求")]
    [LabelText("任务等级要求")]
    [MinValue(1)]
    public int requiredLevel = 1;
    
    [BoxGroup("状态与要求")]
    [LabelText("前置任务")]
    [InfoBox("完成这些任务后才能接取此任务")]
    public List<string> prerequisiteQuests = new List<string>();
    
    [BoxGroup("任务内容")]
    [LabelText("任务目标")]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    public List<QuestObjective> objectives = new List<QuestObjective>();
    
    [BoxGroup("任务内容")]
    [LabelText("任务奖励")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<QuestReward> rewards = new List<QuestReward>();
    
    // 运行时数据
    [System.NonSerialized]
    public System.DateTime startTime;
    [System.NonSerialized]
    public System.DateTime completeTime;
    
    /// <summary>
    /// 初始化任务数据
    /// </summary>
    public virtual void Initialize()
    {
        questStatus = QuestStatus.NotStarted;
        foreach (var objective in objectives)
        {
            objective.currentProgress = 0;
            objective.isCompleted = false;
        }
    }
    
    /// <summary>
    /// 验证任务配置
    /// </summary>
    public virtual bool ValidateQuest()
    {
        if (string.IsNullOrEmpty(questId) || string.IsNullOrEmpty(questName))
            return false;
        
        if (objectives.Count == 0)
            return false;
        
        foreach (var objective in objectives)
        {
            if (objective.targetProgress <= 0)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取任务进度百分比
    /// </summary>
    public float GetProgressPercentage()
    {
        if (objectives.Count == 0) return 0f;
        
        float totalProgress = 0f;
        foreach (var objective in objectives)
        {
            totalProgress += (float)objective.currentProgress / objective.targetProgress;
        }
        
        return totalProgress / objectives.Count;
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
    
    /// <summary>
    /// 获取任务描述（支持动态内容）
    /// </summary>
    public virtual string GetDescription()
    {
        return description;
    }
    
    /// <summary>
    /// 获取总奖励经验值
    /// </summary>
    public int GetTotalExperienceReward()
    {
        int total = 0;
        foreach (var reward in rewards)
        {
            total += reward.experienceReward;
        }
        return total;
    }
    
    /// <summary>
    /// 获取总奖励金币
    /// </summary>
    public int GetTotalGoldReward()
    {
        int total = 0;
        foreach (var reward in rewards)
        {
            total += reward.goldReward;
        }
        return total;
    }
}

/// <summary>
/// 任务目标
/// </summary>
[System.Serializable]
public class QuestObjective
{
    [BoxGroup("目标配置")]
    [LabelText("目标描述")]
    [InfoBox("向玩家显示的目标说明")]
    public string description;
    
    [BoxGroup("目标配置")]
    [LabelText("目标类型")]
    public ObjectiveType objectiveType;
    
    [BoxGroup("目标配置")]
    [LabelText("目标ID")]
    [InfoBox("击杀任务填写敌人类型（如：WildBoar），收集任务填写物品ID")]
    public string targetId;
    
    [BoxGroup("进度跟踪")]
    [LabelText("当前进度")]
    [ReadOnly]
    public int currentProgress = 0;
    
    [BoxGroup("进度跟踪")]
    [LabelText("目标进度")]
    [InfoBox("需要击杀的数量或需要收集的数量")]
    [MinValue(1)]
    public int targetProgress = 1;
    
    [BoxGroup("进度跟踪")]
    [LabelText("是否完成")]
    [ReadOnly]
    public bool isCompleted = false;
    
    [BoxGroup("高级设置")]
    [LabelText("额外参数")]
    [InfoBox("用于存储特定目标类型的额外配置，如敌人等级、物品品质等")]
    public ObjectiveParameters parameters = new ObjectiveParameters();
    
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
    
    /// <summary>
    /// 检查是否匹配目标
    /// </summary>
    public bool MatchesTarget(string id, ObjectiveParameters checkParams = null)
    {
        if (targetId != id) return false;
        
        // 如果有额外参数需要检查
        if (checkParams != null && parameters != null)
        {
            return parameters.Matches(checkParams);
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取格式化的目标描述
    /// </summary>
    public string GetFormattedDescription()
    {
        switch (objectiveType)
        {
            case ObjectiveType.KillEnemy:
                return $"击杀 {GetEnemyDisplayName(targetId)} {GetProgressText()}";
            case ObjectiveType.CollectItem:
                return $"收集 {GetItemDisplayName(targetId)} {GetProgressText()}";
            case ObjectiveType.KillBoss:
                return $"击败BOSS {GetEnemyDisplayName(targetId)} {GetProgressText()}";
            case ObjectiveType.TalkToNPC:
                return $"与 {GetNPCDisplayName(targetId)} 对话";
            default:
                return $"{description} {GetProgressText()}";
        }
    }
    
    private string GetEnemyDisplayName(string enemyType)
    {
        switch (enemyType)
        {
            case "WildBoar": return "野猪";
            case "Wolf": return "狼";
            case "Goblin": return "哥布林";
            case "Orc": return "兽人";
            default: return enemyType;
        }
    }
    
    private string GetItemDisplayName(string itemId)
    {
        // 这里可以从物品数据库获取物品名称
        // 暂时返回ID
        return itemId;
    }
    
    private string GetNPCDisplayName(string npcId)
    {
        // 这里可以从NPC数据库获取NPC名称
        // 暂时返回ID
        return npcId;
    }
}

/// <summary>
/// 目标参数 - 用于存储特定目标类型的额外配置
/// </summary>
[System.Serializable]
public class ObjectiveParameters
{
    [BoxGroup("敌人相关")]
    [LabelText("敌人等级要求")]
    [ShowIf("@objectiveType == ObjectiveType.KillEnemy || objectiveType == ObjectiveType.KillBoss")]
    [MinValue(0)]
    public int minEnemyLevel = 0;
    
    [BoxGroup("敌人相关")]
    [LabelText("敌人最大等级")]
    [ShowIf("@objectiveType == ObjectiveType.KillEnemy || objectiveType == ObjectiveType.KillBoss")]
    [MinValue(0)]
    public int maxEnemyLevel = 999;
    
    [BoxGroup("物品相关")]
    [LabelText("物品品质要求")]
    [ShowIf("@objectiveType == ObjectiveType.CollectItem")]
    public ItemQuality requiredQuality = ItemQuality.Any;
    
    [BoxGroup("位置相关")]
    [LabelText("地点坐标")]
    [ShowIf("@objectiveType == ObjectiveType.ReachLocation")]
    public Vector3 targetPosition;
    
    [BoxGroup("位置相关")]
    [LabelText("地点范围")]
    [ShowIf("@objectiveType == ObjectiveType.ReachLocation")]
    [MinValue(0.1f)]
    public float targetRange = 5f;
    
    [BoxGroup("自定义")]
    [LabelText("自定义参数")]
    [InfoBox("格式：key1=value1;key2=value2")]
    [TextArea(2, 3)]
    public string customParameters = "";
    
    // 隐藏的引用，用于ShowIf条件
    [HideInInspector]
    public ObjectiveType objectiveType;
    
    /// <summary>
    /// 检查参数是否匹配
    /// </summary>
    public bool Matches(ObjectiveParameters other)
    {
        if (other == null) return true;
        
        // 检查敌人等级
        if (minEnemyLevel > 0 && other.minEnemyLevel < minEnemyLevel) return false;
        if (maxEnemyLevel < 999 && other.maxEnemyLevel > maxEnemyLevel) return false;
        
        // 检查物品品质
        if (requiredQuality != ItemQuality.Any && other.requiredQuality != requiredQuality) return false;
        
        return true;
    }
    
    /// <summary>
    /// 从字符串解析自定义参数
    /// </summary>
    public Dictionary<string, string> GetCustomParametersDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(customParameters)) return dict;
        
        var pairs = customParameters.Split(';');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                dict[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }
        
        return dict;
    }
}

/// <summary>
/// 物品品质枚举
/// </summary>
public enum ItemQuality
{
    [LabelText("任意品质")]
    Any,
    [LabelText("普通")]
    Common,
    [LabelText("稀有")]
    Rare,
    [LabelText("史诗")]
    Epic,
    [LabelText("传说")]
    Legendary
}

/// <summary>
/// 任务奖励
/// </summary>
[System.Serializable]
public class QuestReward
{
    [BoxGroup("基础奖励")]
    [LabelText("经验值奖励")]
    [MinValue(0)]
    public int experienceReward = 0;
    
    [BoxGroup("基础奖励")]
    [LabelText("金币奖励")]
    [MinValue(0)]
    public int goldReward = 0;
    
    [BoxGroup("物品奖励")]
    [LabelText("物品奖励")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<ItemReward> itemRewards = new List<ItemReward>();
}

/// <summary>
/// 物品奖励
/// </summary>
[System.Serializable]
public class ItemReward
{
    [HorizontalGroup("Item")]
    [LabelText("物品ID")]
    [HorizontalGroup("Item", Width = 0.7f)]
    public string itemId;
    
    [HorizontalGroup("Item")]
    [LabelText("数量")]
    [HorizontalGroup("Item", Width = 0.3f)]
    [MinValue(1)]
    public int quantity = 1;
}

/// <summary>
/// 任务类型枚举
/// </summary>
public enum QuestType
{
    [LabelText("主线任务")]
    MainQuest,
    [LabelText("支线任务")]
    SideQuest,
    [LabelText("日常任务")]
    DailyQuest,
    [LabelText("成就任务")]
    Achievement,
    [LabelText("击杀任务")]
    KillQuest,
    [LabelText("收集任务")]
    CollectQuest
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
    [LabelText("击杀BOSS")]
    KillBoss,
    [LabelText("与NPC对话")]
    TalkToNPC,
    [LabelText("寻找物品")]
    FindItem,
    [LabelText("到达地点")]
    ReachLocation,
    [LabelText("使用物品")]
    UseItem,
    [LabelText("完成任务")]
    CompleteQuest
}