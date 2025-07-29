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
    
    [Header("系统配置")]
    [Tooltip("是否启用调试日志")]
    [SerializeField] private bool enableDebugLogs = true;
    
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
            
            if (enableDebugLogs)
            {
                Debug.Log("[QuestManager] 开始初始化任务系统...");
            }
            
            InitializeQuests();
            InitializeKillQuestSystem();
            
            if (enableDebugLogs)
            {
                Debug.Log("[QuestManager] 任务系统初始化完成！");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化击杀任务系统
    /// </summary>
    private void InitializeKillQuestSystem()
    {
        // 获取敌人击杀追踪器
        var killTracker = FindObjectOfType<EnemyKillTracker>();
        if (killTracker == null)
        {
            // 如果场景中没有EnemyKillTracker，创建一个
            GameObject trackerObj = new GameObject("EnemyKillTracker");
            killTracker = trackerObj.AddComponent<EnemyKillTracker>();
            DontDestroyOnLoad(trackerObj);
            
            if (enableDebugLogs)
            {
                Debug.Log("[QuestManager] 创建了EnemyKillTracker实例");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.Log("[QuestManager] EnemyKillTracker已存在");
            }
        }
        
        // 订阅敌人死亡事件
        killTracker.OnEnemyKilled += HandleEnemyKilled;
    }
    
    /// <summary>
    /// 处理敌人死亡事件
    /// </summary>
    private void HandleEnemyKilled(string enemyType, int totalKills)
    {
        // 更新所有相关的击杀任务
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if ((objective.objectiveType == ObjectiveType.KillEnemy || 
                     objective.objectiveType == ObjectiveType.KillBoss) && 
                    objective.targetId == enemyType)
                {
                    UpdateObjectiveProgress(quest.questId, objective.targetId, totalKills);
                }
            }
        }
    }
    
    /// <summary>
    /// 处理物品收集事件
    /// </summary>
    public void HandleItemCollected(string itemId, int quantity)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == ObjectiveType.CollectItem && 
                    objective.targetId == itemId)
                {
                    AddObjectiveProgress(quest.questId, objective.targetId, quantity);
                }
            }
        }
    }
    
    /// <summary>
    /// 处理NPC对话事件
    /// </summary>
    public void HandleNPCTalk(string npcId)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == ObjectiveType.TalkToNPC && 
                    objective.targetId == npcId)
                {
                    UpdateObjectiveProgress(quest.questId, objective.targetId, 1);
                }
            }
        }
    }
    
    /// <summary>
    /// 处理到达地点事件
    /// </summary>
    public void HandleLocationReached(string locationId, Vector3 playerPosition)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == ObjectiveType.ReachLocation && 
                    objective.targetId == locationId)
                {
                    // 检查距离
                    float distance = Vector3.Distance(playerPosition, objective.parameters.targetPosition);
                    if (distance <= objective.parameters.targetRange)
                    {
                        UpdateObjectiveProgress(quest.questId, objective.targetId, 1);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 处理物品使用事件
    /// </summary>
    public void HandleItemUsed(string itemId, int quantity)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == ObjectiveType.UseItem && 
                    objective.targetId == itemId)
                {
                    AddObjectiveProgress(quest.questId, objective.targetId, quantity);
                }
            }
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
                
                if (enableDebugLogs)
                {
                    Debug.Log($"[QuestManager] 预加载任务: {quest.questName}");
                }
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 预加载了 {allQuests.Count} 个任务");
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
        if (quest.rewards == null || quest.rewards.Count == 0) return;
        
        foreach (var reward in quest.rewards)
        {
            // 给予经验值
            if (reward.experienceReward > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.AddPlayerExperience(reward.experienceReward);
            }
            
            // 给予金币
            if (reward.goldReward > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.AddPlayerGold(reward.goldReward);
            }
            
            // 给予物品奖励
            foreach (var itemReward in reward.itemRewards)
            {
                // 这里需要根据实际的物品系统来实现
                Debug.Log($"[QuestManager] 获得物品: {itemReward.itemId} x{itemReward.quantity}");
            }
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
    /// 获取指定状态的任务
    /// </summary>
    public List<QuestData> GetQuestsByStatus(QuestStatus status)
    {
        switch (status)
        {
            case QuestStatus.InProgress:
                return new List<QuestData>(activeQuests);
            case QuestStatus.Completed:
                return new List<QuestData>(completedQuests);
            case QuestStatus.Failed:
                return new List<QuestData>(failedQuests);
            case QuestStatus.NotStarted:
                return questDatabase.Values.Where(q => q.questStatus == QuestStatus.NotStarted).ToList();
            case QuestStatus.Abandoned:
                return questDatabase.Values.Where(q => q.questStatus == QuestStatus.Abandoned).ToList();
            default:
                return new List<QuestData>();
        }
    }
    
    /// <summary>
    /// 根据ID获取任务
    /// </summary>
    public QuestData GetQuestById(string questId)
    {
        return questDatabase.ContainsKey(questId) ? questDatabase[questId] : null;
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