using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// 任务管理器 - 管理游戏中的所有任务
/// 提供任务的创建、更新、完成等功能
/// </summary>
public class QuestManager : MonoBehaviour
{
    [Header("任务配置")]
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();
    
    // 运行时任务数据
    private Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();
    private List<QuestData> activeQuests = new List<QuestData>();
    private List<QuestData> completedQuests = new List<QuestData>();
    private List<QuestData> failedQuests = new List<QuestData>();
    
    // 单例模式
    public static QuestManager Instance { get; private set; }
    
    // 事件
    public System.Action<QuestData> OnQuestStarted;
    public System.Action<QuestData> OnQuestCompleted;
    public System.Action<QuestData> OnQuestFailed;
    public System.Action<QuestData> OnQuestAbandoned;
    public System.Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public System.Action<QuestData> OnQuestProgressUpdated;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeQuests();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化任务系统
    /// </summary>
    private void InitializeQuests()
    {
        // 将所有任务添加到数据库
        foreach (var quest in allQuests)
        {
            if (!questDatabase.ContainsKey(quest.questId))
            {
                questDatabase.Add(quest.questId, quest);
            }
        }
        
        // 加载保存的任务数据
        LoadQuestData();
    }
    
    /// <summary>
    /// 开始任务
    /// </summary>
    public bool StartQuest(string questId)
    {
        if (!questDatabase.ContainsKey(questId))
        {
            Debug.LogWarning($"[QuestManager] 找不到任务: {questId}");
            return false;
        }
        
        var quest = questDatabase[questId];
        
        // 检查前置条件
        if (!CanStartQuest(quest))
        {
            Debug.LogWarning($"[QuestManager] 不满足任务开始条件: {questId}");
            return false;
        }
        
        // 开始任务
        quest.StartQuest();
        activeQuests.Add(quest);
        
        OnQuestStarted?.Invoke(quest);
        
        Debug.Log($"[QuestManager] 开始任务: {quest.questName}");
        return true;
    }
    
    /// <summary>
    /// 完成任务
    /// </summary>
    public bool CompleteQuest(string questId)
    {
        var quest = GetActiveQuest(questId);
        if (quest == null)
        {
            Debug.LogWarning($"[QuestManager] 找不到活跃任务: {questId}");
            return false;
        }
        
        if (!quest.CanComplete())
        {
            Debug.LogWarning($"[QuestManager] 任务目标未完成: {questId}");
            return false;
        }
        
        // 完成任务
        quest.CompleteQuest();
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        
        // 给予奖励
        GiveQuestReward(quest);
        
        OnQuestCompleted?.Invoke(quest);
        
        Debug.Log($"[QuestManager] 完成任务: {quest.questName}");
        return true;
    }
    
    /// <summary>
    /// 失败任务
    /// </summary>
    public bool FailQuest(string questId)
    {
        var quest = GetActiveQuest(questId);
        if (quest == null) return false;
        
        quest.FailQuest();
        activeQuests.Remove(quest);
        failedQuests.Add(quest);
        
        OnQuestFailed?.Invoke(quest);
        
        Debug.Log($"[QuestManager] 任务失败: {quest.questName}");
        return true;
    }
    
    /// <summary>
    /// 放弃任务
    /// </summary>
    public bool AbandonQuest(string questId)
    {
        var quest = GetActiveQuest(questId);
        if (quest == null) return false;
        
        quest.questStatus = QuestStatus.Abandoned;
        activeQuests.Remove(quest);
        
        OnQuestAbandoned?.Invoke(quest);
        
        Debug.Log($"[QuestManager] 放弃任务: {quest.questName}");
        return true;
    }
    
    /// <summary>
    /// 更新任务目标进度
    /// </summary>
    public void UpdateObjectiveProgress(string questId, string targetId, int progress)
    {
        var quest = GetActiveQuest(questId);
        if (quest == null) return;
        
        bool questUpdated = false;
        foreach (var objective in quest.objectives)
        {
            if (objective.targetId == targetId)
            {
                int oldProgress = objective.currentProgress;
                objective.UpdateProgress(progress);
                
                if (objective.currentProgress != oldProgress)
                {
                    questUpdated = true;
                    
                    if (objective.isCompleted)
                    {
                        OnObjectiveCompleted?.Invoke(quest, objective);
                    }
                }
            }
        }
        
        if (questUpdated)
        {
            OnQuestProgressUpdated?.Invoke(quest);
            
            // 检查任务是否可以完成
            if (quest.CanComplete())
            {
                // 可以选择自动完成或者提示玩家
                Debug.Log($"[QuestManager] 任务可以完成: {quest.questName}");
            }
        }
    }
    
    /// <summary>
    /// 增加任务目标进度
    /// </summary>
    public void AddObjectiveProgress(string questId, string targetId, int amount = 1)
    {
        var quest = GetActiveQuest(questId);
        if (quest == null) return;
        
        foreach (var objective in quest.objectives)
        {
            if (objective.targetId == targetId)
            {
                UpdateObjectiveProgress(questId, targetId, objective.currentProgress + amount);
                break;
            }
        }
    }
    
    /// <summary>
    /// 检查是否可以开始任务
    /// </summary>
    private bool CanStartQuest(QuestData quest)
    {
        // 检查等级要求
        if (GameManager.Instance != null)
        {
            int playerLevel = GameManager.Instance.GetPlayerLevel();
            if (playerLevel < quest.requiredLevel)
            {
                return false;
            }
        }
        
        // 检查前置任务
        foreach (var prerequisiteId in quest.prerequisiteQuests)
        {
            if (!IsQuestCompleted(prerequisiteId))
            {
                return false;
            }
        }
        
        // 检查任务是否已经在进行中或已完成
        if (quest.questStatus != QuestStatus.NotStarted)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 给予任务奖励
    /// </summary>
    private void GiveQuestReward(QuestData quest)
    {
        if (quest.reward == null) return;
        
        // 给予经验值
        if (quest.reward.experienceReward > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerExperience(quest.reward.experienceReward);
        }
        
        // 给予金币
        if (quest.reward.goldReward > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerGold(quest.reward.goldReward);
        }
        
        // 给予物品奖励
        foreach (var itemReward in quest.reward.itemRewards)
        {
            // 这里需要根据实际的物品系统来实现
            Debug.Log($"[QuestManager] 获得物品: {itemReward.itemId} x{itemReward.quantity}");
        }
    }
    
    /// <summary>
    /// 获取活跃任务
    /// </summary>
    public QuestData GetActiveQuest(string questId)
    {
        return activeQuests.FirstOrDefault(q => q.questId == questId);
    }
    
    /// <summary>
    /// 获取任务
    /// </summary>
    public QuestData GetQuest(string questId)
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
    /// 检查任务是否活跃
    /// </summary>
    public bool IsQuestActive(string questId)
    {
        return activeQuests.Any(q => q.questId == questId);
    }
    
    /// <summary>
    /// 获取所有活跃任务
    /// </summary>
    public List<QuestData> GetActiveQuests()
    {
        return new List<QuestData>(activeQuests);
    }
    
    /// <summary>
    /// 获取所有已完成任务
    /// </summary>
    public List<QuestData> GetCompletedQuests()
    {
        return new List<QuestData>(completedQuests);
    }
    
    /// <summary>
    /// 获取指定类型的任务
    /// </summary>
    public List<QuestData> GetQuestsByType(QuestType questType)
    {
        return activeQuests.Where(q => q.questType == questType).ToList();
    }
    
    /// <summary>
    /// 保存任务数据
    /// </summary>
    public void SaveQuestData()
    {
        // 这里应该实现实际的保存逻辑
        // 可以使用PlayerPrefs、JSON文件或其他保存方式
        Debug.Log("[QuestManager] 保存任务数据");
    }
    
    /// <summary>
    /// 加载任务数据
    /// </summary>
    private void LoadQuestData()
    {
        // 这里应该实现实际的加载逻辑
        // 可以从PlayerPrefs、JSON文件或其他地方加载
        Debug.Log("[QuestManager] 加载任务数据");
    }
    
    /// <summary>
    /// 添加新任务到数据库
    /// </summary>
    public void AddQuestToDatabase(QuestData quest)
    {
        if (!questDatabase.ContainsKey(quest.questId))
        {
            questDatabase.Add(quest.questId, quest);
            allQuests.Add(quest);
        }
    }
    
    /// <summary>
    /// 重置所有任务（用于测试）
    /// </summary>
    [Button("重置所有任务")]
    public void ResetAllQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        failedQuests.Clear();
        
        foreach (var quest in questDatabase.Values)
        {
            quest.questStatus = QuestStatus.NotStarted;
            foreach (var objective in quest.objectives)
            {
                objective.currentProgress = 0;
                objective.isCompleted = false;
            }
        }
        
        Debug.Log("[QuestManager] 所有任务已重置");
    }
}