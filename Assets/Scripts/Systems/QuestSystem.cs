using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 任务系统 - 管理游戏中的任务
/// 从原Phaser项目的任务系统迁移而来
/// </summary>
public class QuestSystem : MonoBehaviour
{
    [Header("任务配置")]
    public List<QuestData> availableQuests = new List<QuestData>();
    
    [Header("任务设置")]
    public int maxActiveQuests = 5;          // 最大同时进行任务数
    public bool autoAcceptQuests = false;    // 自动接受任务
    public bool showQuestNotifications = true; // 显示任务通知
    
    // 单例
    public static QuestSystem Instance { get; private set; }
    
    // 任务状态
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();
    private Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();
    
    // 事件
    public event Action<Quest> OnQuestStarted;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest, QuestObjective> OnObjectiveCompleted;
    public event Action<Quest> OnQuestProgressUpdated;
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeQuestSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化任务系统
    /// </summary>
    private void InitializeQuestSystem()
    {
        // 构建任务数据库
        foreach (QuestData questData in availableQuests)
        {
            if (!string.IsNullOrEmpty(questData.questId))
            {
                questDatabase[questData.questId] = questData;
            }
        }
        
        // 添加默认任务
        if (availableQuests.Count == 0)
        {
            CreateDefaultQuests();
        }
    }
    
    /// <summary>
    /// 创建默认任务
    /// </summary>
    private void CreateDefaultQuests()
    {
        // 新手任务
        QuestData tutorialQuest = new QuestData
        {
            questId = "tutorial_001",
            questName = "新手训练",
            description = "完成基础训练，熟悉游戏操作",
            questType = QuestType.Tutorial,
            isRepeatable = false,
            requiredLevel = 1,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveId = "move",
                    description = "移动角色",
                    type = ObjectiveType.Custom,
                    targetCount = 1
                },
                new QuestObjective
                {
                    objectiveId = "attack",
                    description = "攻击敌人",
                    type = ObjectiveType.Custom,
                    targetCount = 1
                }
            },
            rewards = new List<QuestReward>
            {
                new QuestReward { type = RewardType.Experience, amount = 50 },
                new QuestReward { type = RewardType.Gold, amount = 10 }
            }
        };
        
        // 击杀任务
        QuestData killQuest = new QuestData
        {
            questId = "kill_001",
            questName = "清理野猪",
            description = "击败5只野猪",
            questType = QuestType.Kill,
            isRepeatable = true,
            requiredLevel = 1,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveId = "kill_wildboar",
                    description = "击败野猪",
                    type = ObjectiveType.Kill,
                    targetId = "WildBoar",
                    targetCount = 5
                }
            },
            rewards = new List<QuestReward>
            {
                new QuestReward { type = RewardType.Experience, amount = 100 },
                new QuestReward { type = RewardType.Gold, amount = 25 }
            }
        };
        
        // 收集任务
        QuestData collectQuest = new QuestData
        {
            questId = "collect_001",
            questName = "收集草药",
            description = "收集10个治疗草药",
            questType = QuestType.Collect,
            isRepeatable = true,
            requiredLevel = 1,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    objectiveId = "collect_herb",
                    description = "收集治疗草药",
                    type = ObjectiveType.Collect,
                    targetId = "healing_herb",
                    targetCount = 10
                }
            },
            rewards = new List<QuestReward>
            {
                new QuestReward { type = RewardType.Experience, amount = 75 },
                new QuestReward { type = RewardType.Item, itemId = "health_potion", amount = 3 }
            }
        };
        
        availableQuests.AddRange(new[] { tutorialQuest, killQuest, collectQuest });
        
        // 更新数据库
        foreach (QuestData questData in availableQuests)
        {
            questDatabase[questData.questId] = questData;
        }
    }
    
    /// <summary>
    /// 开始任务
    /// </summary>
    public bool StartQuest(string questId)
    {
        if (!questDatabase.ContainsKey(questId))
        {
            Debug.LogWarning($"Quest '{questId}' not found!");
            return false;
        }
        
        QuestData questData = questDatabase[questId];
        
        // 检查是否可以接受任务
        if (!CanAcceptQuest(questData))
        {
            return false;
        }
        
        // 创建任务实例
        Quest quest = new Quest(questData);
        activeQuests.Add(quest);
        
        // 触发事件
        OnQuestStarted?.Invoke(quest);
        
        // 显示通知
        if (showQuestNotifications && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"任务开始: {quest.questName}");
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("quest_start");
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查是否可以接受任务
    /// </summary>
    private bool CanAcceptQuest(QuestData questData)
    {
        // 检查任务数量限制
        if (activeQuests.Count >= maxActiveQuests)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("任务数量已达上限！");
            }
            return false;
        }
        
        // 检查是否已经在进行中
        if (activeQuests.Any(q => q.questId == questData.questId))
        {
            return false;
        }
        
        // 检查是否已完成且不可重复
        if (!questData.isRepeatable && completedQuests.Any(q => q.questId == questData.questId))
        {
            return false;
        }
        
        // 检查等级要求
        if (GameManager.Instance != null && GameManager.Instance.GetPlayerLevel() < questData.requiredLevel)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage($"需要等级 {questData.requiredLevel}");
            }
            return false;
        }
        
        // 检查前置任务
        foreach (string prerequisite in questData.prerequisites)
        {
            if (!completedQuests.Any(q => q.questId == prerequisite))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 更新任务进度
    /// </summary>
    public void UpdateQuestProgress(ObjectiveType type, string targetId, int amount = 1)
    {
        foreach (Quest quest in activeQuests.ToList())
        {
            bool questUpdated = false;
            
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type == type && 
                    (string.IsNullOrEmpty(objective.targetId) || objective.targetId == targetId) &&
                    !objective.isCompleted)
                {
                    objective.currentCount += amount;
                    
                    if (objective.currentCount >= objective.targetCount)
                    {
                        objective.currentCount = objective.targetCount;
                        objective.isCompleted = true;
                        
                        OnObjectiveCompleted?.Invoke(quest, objective);
                        
                        if (showQuestNotifications && UIManager.Instance != null)
                        {
                            UIManager.Instance.ShowMessage($"目标完成: {objective.description}");
                        }
                    }
                    
                    questUpdated = true;
                }
            }
            
            if (questUpdated)
            {
                OnQuestProgressUpdated?.Invoke(quest);
                
                // 检查任务是否完成
                if (quest.IsCompleted())
                {
                    CompleteQuest(quest);
                }
            }
        }
    }
    
    /// <summary>
    /// 完成任务
    /// </summary>
    private void CompleteQuest(Quest quest)
    {
        // 从活跃任务中移除
        activeQuests.Remove(quest);
        
        // 添加到已完成任务
        quest.completedTime = DateTime.Now;
        completedQuests.Add(quest);
        
        // 发放奖励
        GiveRewards(quest);
        
        // 触发事件
        OnQuestCompleted?.Invoke(quest);
        
        // 显示通知
        if (showQuestNotifications && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"任务完成: {quest.questName}");
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("quest_complete");
        }
    }
    
    /// <summary>
    /// 发放奖励
    /// </summary>
    private void GiveRewards(Quest quest)
    {
        foreach (QuestReward reward in quest.rewards)
        {
            switch (reward.type)
            {
                case RewardType.Experience:
                    GiveExperience(reward.amount);
                    break;
                case RewardType.Gold:
                    GiveGold(reward.amount);
                    break;
                case RewardType.Item:
                    GiveItem(reward.itemId, reward.amount);
                    break;
            }
        }
    }
    
    /// <summary>
    /// 给予经验
    /// </summary>
    private void GiveExperience(int amount)
    {
        // 这里需要与角色系统集成
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerExperience(amount);
        }
        
        if (showQuestNotifications && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"获得经验: {amount}");
        }
    }
    
    /// <summary>
    /// 给予金币
    /// </summary>
    private void GiveGold(int amount)
    {
        // 这里需要与货币系统集成
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerGold(amount);
        }
        
        if (showQuestNotifications && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"获得金币: {amount}");
        }
    }
    
    /// <summary>
    /// 给予物品
    /// </summary>
    private void GiveItem(string itemId, int amount)
    {
        // 这里需要与背包系统集成
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Inventory inventory = player.GetInventory();
            if (inventory != null && GameConfig.Instance != null)
            {
                ItemData itemData = GameConfig.Instance.GetItemData(itemId);
                if (itemData != null)
                {
                    // 创建物品实例并添加到背包
                    // 这里需要根据实际的物品系统实现
                }
            }
        }
        
        if (showQuestNotifications && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"获得物品: {itemId} x{amount}");
        }
    }
    
    /// <summary>
    /// 放弃任务
    /// </summary>
    public bool AbandonQuest(string questId)
    {
        Quest quest = activeQuests.FirstOrDefault(q => q.questId == questId);
        if (quest == null) return false;
        
        activeQuests.Remove(quest);
        
        if (showQuestNotifications && UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"任务已放弃: {quest.questName}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取活跃任务列表
    /// </summary>
    public List<Quest> GetActiveQuests()
    {
        return new List<Quest>(activeQuests);
    }
    
    /// <summary>
    /// 获取已完成任务列表
    /// </summary>
    public List<Quest> GetCompletedQuests()
    {
        return new List<Quest>(completedQuests);
    }
    
    /// <summary>
    /// 获取可接受的任务列表
    /// </summary>
    public List<QuestData> GetAvailableQuests()
    {
        return availableQuests.Where(q => CanAcceptQuest(q)).ToList();
    }
    
    /// <summary>
    /// 获取任务数据
    /// </summary>
    public QuestData GetQuestData(string questId)
    {
        return questDatabase.ContainsKey(questId) ? questDatabase[questId] : null;
    }
    
    /// <summary>
    /// 检查任务是否已完成
    /// </summary>
    public bool IsQuestCompleted(string questId)
    {
        return completedQuests.Any(q => q.questId == questId);
    }
    
    /// <summary>
    /// 检查任务是否正在进行
    /// </summary>
    public bool IsQuestActive(string questId)
    {
        return activeQuests.Any(q => q.questId == questId);
    }
    
    /// <summary>
    /// 保存任务数据
    /// </summary>
    public QuestSaveData GetSaveData()
    {
        return new QuestSaveData
        {
            activeQuests = activeQuests.Select(q => q.GetSaveData()).ToList(),
            completedQuestIds = completedQuests.Select(q => q.questId).ToList()
        };
    }
    
    /// <summary>
    /// 加载任务数据
    /// </summary>
    public void LoadSaveData(QuestSaveData saveData)
    {
        activeQuests.Clear();
        completedQuests.Clear();
        
        // 加载活跃任务
        foreach (QuestSaveData.QuestInstanceData questData in saveData.activeQuests)
        {
            if (questDatabase.ContainsKey(questData.questId))
            {
                Quest quest = new Quest(questDatabase[questData.questId]);
                quest.LoadSaveData(questData);
                activeQuests.Add(quest);
            }
        }
        
        // 加载已完成任务
        foreach (string questId in saveData.completedQuestIds)
        {
            if (questDatabase.ContainsKey(questId))
            {
                Quest quest = new Quest(questDatabase[questId]);
                quest.completedTime = DateTime.Now; // 简化处理
                completedQuests.Add(quest);
            }
        }
    }
}

/// <summary>
/// 任务数据
/// </summary>
[Serializable]
public class QuestData
{
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public bool isRepeatable = false;
    public int requiredLevel = 1;
    public List<string> prerequisites = new List<string>();
    public List<QuestObjective> objectives = new List<QuestObjective>();
    public List<QuestReward> rewards = new List<QuestReward>();
    public float timeLimit = 0f; // 0表示无时间限制
}

/// <summary>
/// 任务实例
/// </summary>
[Serializable]
public class Quest
{
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public List<QuestObjective> objectives;
    public List<QuestReward> rewards;
    public DateTime startTime;
    public DateTime completedTime;
    public float timeLimit;
    
    public Quest(QuestData data)
    {
        questId = data.questId;
        questName = data.questName;
        description = data.description;
        questType = data.questType;
        objectives = new List<QuestObjective>(data.objectives);
        rewards = new List<QuestReward>(data.rewards);
        startTime = DateTime.Now;
        timeLimit = data.timeLimit;
    }
    
    public bool IsCompleted()
    {
        return objectives.All(obj => obj.isCompleted);
    }
    
    public float GetProgress()
    {
        if (objectives.Count == 0) return 0f;
        
        float totalProgress = objectives.Sum(obj => (float)obj.currentCount / obj.targetCount);
        return totalProgress / objectives.Count;
    }
    
    public bool IsExpired()
    {
        if (timeLimit <= 0) return false;
        return (DateTime.Now - startTime).TotalSeconds > timeLimit;
    }
    
    public QuestSaveData.QuestInstanceData GetSaveData()
    {
        return new QuestSaveData.QuestInstanceData
        {
            questId = questId,
            startTime = startTime.ToBinary(),
            objectives = objectives.ToList()
        };
    }
    
    public void LoadSaveData(QuestSaveData.QuestInstanceData saveData)
    {
        startTime = DateTime.FromBinary(saveData.startTime);
        objectives = saveData.objectives;
    }
}

/// <summary>
/// 任务目标
/// </summary>
[Serializable]
public class QuestObjective
{
    public string objectiveId;
    public string description;
    public ObjectiveType type;
    public string targetId;
    public int targetCount = 1;
    public int currentCount = 0;
    public bool isCompleted = false;
}

/// <summary>
/// 任务奖励
/// </summary>
[Serializable]
public class QuestReward
{
    public RewardType type;
    public int amount;
    public string itemId;
}

/// <summary>
/// 任务保存数据
/// </summary>
[Serializable]
public class QuestSaveData
{
    public List<QuestInstanceData> activeQuests = new List<QuestInstanceData>();
    public List<string> completedQuestIds = new List<string>();
    
    [Serializable]
    public class QuestInstanceData
    {
        public string questId;
        public long startTime;
        public List<QuestObjective> objectives;
    }
}

/// <summary>
/// 任务类型枚举
/// </summary>
public enum QuestType
{
    Tutorial,    // 教程任务
    Kill,        // 击杀任务
    Collect,     // 收集任务
    Deliver,     // 运送任务
    Escort,      // 护送任务
    Explore,     // 探索任务
    Custom       // 自定义任务
}

/// <summary>
/// 目标类型枚举
/// </summary>
public enum ObjectiveType
{
    Kill,        // 击杀
    Collect,     // 收集
    Deliver,     // 运送
    Interact,    // 交互
    Reach,       // 到达
    Survive,     // 生存
    Custom       // 自定义
}

/// <summary>
/// 奖励类型枚举
/// </summary>
public enum RewardType
{
    Experience,  // 经验
    Gold,        // 金币
    Item         // 物品
}