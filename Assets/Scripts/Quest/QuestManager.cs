using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 任务管理器 - 管理游戏中的所有任务
/// 提供任务的创建、更新、完成等功能
/// </summary>
public class QuestManager : MonoBehaviour
{
    [Header("任务配置")]
    [Tooltip("任务配置文件夹路径（相对于Assets文件夹）")]
    [SerializeField] private string questConfigPath = "Data/Configs/任务配置";

    [Tooltip("是否自动加载任务配置文件")]
    [SerializeField] private bool autoLoadQuestConfigs = true;

    [Tooltip("手动配置的任务列表（当自动加载关闭时使用）")]
    [SerializeField] private List<QuestData> manualQuests = new List<QuestData>();

    [Header("运行时任务数据")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("自动加载的任务列表")]
    private List<QuestData> allQuests = new List<QuestData>();

    [Header("系统配置")]
    [Tooltip("是否启用调试日志")]
    [SerializeField] public bool enableDebugLogs = true;

    // 运行时任务数据
    private Dictionary<string, QuestData> questDatabase = new Dictionary<string, QuestData>();
    private List<QuestData> activeQuests = new List<QuestData>();
    private List<QuestData> completedQuests = new List<QuestData>();
    private List<QuestData> failedQuests = new List<QuestData>();

    // 优化查找的索引字典 - 避免双重FOR循环
    private Dictionary<string, List<QuestObjectiveRef>> enemyKillObjectives = new Dictionary<string, List<QuestObjectiveRef>>();
    private Dictionary<string, List<QuestObjectiveRef>> itemCollectObjectives = new Dictionary<string, List<QuestObjectiveRef>>();
    private Dictionary<string, List<QuestObjectiveRef>> npcTalkObjectives = new Dictionary<string, List<QuestObjectiveRef>>();
    private Dictionary<string, List<QuestObjectiveRef>> locationObjectives = new Dictionary<string, List<QuestObjectiveRef>>();
    private Dictionary<string, List<QuestObjectiveRef>> itemUseObjectives = new Dictionary<string, List<QuestObjectiveRef>>();

    /// <summary>
    /// 任务目标引用结构 - 用于快速查找
    /// </summary>
    private struct QuestObjectiveRef
    {
        public QuestData quest;
        public QuestObjective objective;

        public QuestObjectiveRef(QuestData quest, QuestObjective objective)
        {
            this.quest = quest;
            this.objective = objective;
        }
    }

    // 单例模式
    public static QuestManager Instance { get; private set; }

    // 事件
    public System.Action<QuestData> OnQuestStarted;
    public System.Action<QuestData> OnQuestCompleted;
    public System.Action<QuestData> OnQuestFailed;
    public System.Action<QuestData> OnQuestAbandoned;
    public System.Action<QuestData, QuestObjective> OnObjectiveCompleted;
    public System.Action<QuestData> OnQuestProgressUpdated;
// 添加批量模式状态
private bool isBatchProcessing = false;
private List<Enemy> pendingEnemies = new List<Enemy>();

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
    void Start()
    {

        // 初始化事件监听器
        InitializeEventListeners();
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

    public void SetBatchMode(bool isBatching)
    {
        isBatchProcessing = isBatching;
        if (!isBatching && pendingEnemies.Count > 0)
        {
            foreach (var enemy in pendingEnemies)
            {
                RegisterSingleEnemy(enemy);
            }
            pendingEnemies.Clear();
        }
    }
// 修改敌人注册方法
private void RegisterSingleEnemy(Enemy enemy)
{
    if (enemy == null) return;

    string enemyId = enemy.enemyConfig.enemyId;
    if (enemyKillObjectives.ContainsKey(enemyId))
    {
        EnemyKillTracker.Instance.RegisterEnemy(enemy);
    }
}

    /// <summary>
    /// 处理敌人死亡事件 - 优化版本，使用字典查找
    /// </summary>
    private void HandleEnemyKilled(string enemyType, int totalKills, Enemy enemyComponent)
    {
        // 直接从字典中获取相关的任务目标，避免双重FOR循环
        if (enemyKillObjectives.TryGetValue(enemyType, out var objectives))
        {
            foreach (var objRef in objectives)
            {
                // 确保任务仍然是活跃状态
                if (objRef.quest.questStatus == QuestStatus.InProgress)
                {
                    UpdateObjectiveProgress(objRef.quest.questId, objRef.objective.targetId, totalKills);
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 处理敌人击杀事件: {enemyType}, 总击杀数: {totalKills}");
        }
    }

    /// <summary>
    /// 处理物品收集事件 - 优化版本，使用字典查找
    /// </summary>
    public void HandleItemCollected(string itemId, int quantity)
    {
        // 直接从字典中获取相关的任务目标，避免双重FOR循环
        if (itemCollectObjectives.TryGetValue(itemId, out var objectives))
        {
            foreach (var objRef in objectives)
            {
                // 确保任务仍然是活跃状态
                if (objRef.quest.questStatus == QuestStatus.InProgress)
                {
                    AddObjectiveProgress(objRef.quest.questId, objRef.objective.targetId, quantity);
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 处理物品收集事件: {itemId}, 数量: {quantity}");
        }
    }

    /// <summary>
    /// 处理NPC对话事件 - 优化版本，使用字典查找
    /// </summary>
    public void HandleNPCTalk(string npcId)
    {
        // 直接从字典中获取相关的任务目标，避免双重FOR循环
        if (npcTalkObjectives.TryGetValue(npcId, out var objectives))
        {
            foreach (var objRef in objectives)
            {
                // 确保任务仍然是活跃状态
                if (objRef.quest.questStatus == QuestStatus.InProgress)
                {
                    UpdateObjectiveProgress(objRef.quest.questId, objRef.objective.targetId, 1);
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 处理NPC对话事件: {npcId}");
        }
    }
    //
    public bool HasUncompletedTalkObjective(string npcId)
    {
        if (npcTalkObjectives.TryGetValue(npcId, out var objectives))
        {
            foreach (var objRef in objectives)
            {
                Debug.Log(objRef.quest.questStatus);
                if (objRef.quest.questStatus == QuestStatus.InProgress && !objRef.objective.isCompleted)
                {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// 处理到达地点事件 - 优化版本，使用字典查找
    /// </summary>
    public void HandleLocationReached(string locationId, Vector3 playerPosition)
    {
        // 直接从字典中获取相关的任务目标，避免双重FOR循环
        if (locationObjectives.TryGetValue(locationId, out var objectives))
        {
            foreach (var objRef in objectives)
            {
                // 确保任务仍然是活跃状态
                if (objRef.quest.questStatus == QuestStatus.InProgress)
                {
                    // 检查距离
                    float distance = Vector3.Distance(playerPosition, objRef.objective.parameters.targetPosition);
                    if (distance <= objRef.objective.parameters.targetRange)
                    {
                        UpdateObjectiveProgress(objRef.quest.questId, objRef.objective.targetId, 1);
                    }
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 处理到达地点事件: {locationId}");
        }
    }

    /// <summary>
    /// 处理物品使用事件 - 优化版本，使用字典查找
    /// </summary>
    public void HandleItemUsed(string itemId, int quantity)
    {
        // 直接从字典中获取相关的任务目标，避免双重FOR循环
        if (itemUseObjectives.TryGetValue(itemId, out var objectives))
        {
            foreach (var objRef in objectives)
            {
                // 确保任务仍然是活跃状态
                if (objRef.quest.questStatus == QuestStatus.InProgress)
                {
                    AddObjectiveProgress(objRef.quest.questId, objRef.objective.targetId, quantity);
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 处理物品使用事件: {itemId}, 数量: {quantity}");
        }
    }

    /// <summary>
    /// 初始化任务系统
    /// </summary>
    private void InitializeQuests()
    {
        // 清空现有任务列表
        allQuests.Clear();

        if (autoLoadQuestConfigs)
        {
            LoadQuestConfigsFromFolder();
        }
        else
        {
            // 使用手动配置的任务列表
            allQuests.AddRange(manualQuests);
            if (enableDebugLogs)
            {
                Debug.Log($"[QuestManager] 使用手动配置的任务列表，共 {manualQuests.Count} 个任务");
            }
        }

        // 将所有任务添加到数据库
        foreach (var quest in allQuests)
        {
            if (quest == null)
            {
                Debug.LogWarning("[QuestManager] 发现空的任务配置，跳过");
                continue;
            }

            if (!questDatabase.ContainsKey(quest.questId))
            {
                // 初始化任务数据
                quest.Initialize();
                questDatabase.Add(quest.questId, quest);

                if (enableDebugLogs)
                {
                    Debug.Log($"[QuestManager] 预加载任务: {quest.questName} (ID: {quest.questId})");
                }
            }
            else
            {
                Debug.LogWarning($"[QuestManager] 发现重复的任务ID: {quest.questId}，跳过任务: {quest.questName}");
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 预加载了 {questDatabase.Count} 个任务");
        }

        // 加载保存的任务数据
        LoadQuestData();
    }

    /// <summary>
    /// 从指定文件夹加载所有任务配置文件
    /// </summary>
    private void LoadQuestConfigsFromFolder()
    {
        try
        {
#if UNITY_EDITOR
            // 编辑器模式下使用AssetDatabase
            LoadQuestConfigsInEditor();
#else
            // 运行时模式下使用Resources
            LoadQuestConfigsAtRuntime();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] 加载任务配置文件时发生错误: {e.Message}");

            // 如果自动加载失败，回退到手动配置
            if (manualQuests.Count > 0)
            {
                allQuests.AddRange(manualQuests);
                Debug.LogWarning($"[QuestManager] 自动加载失败，使用手动配置的 {manualQuests.Count} 个任务");
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下加载任务配置
    /// </summary>
    private void LoadQuestConfigsInEditor()
    {
        string fullPath = System.IO.Path.Combine(Application.dataPath, questConfigPath);

        if (!System.IO.Directory.Exists(fullPath))
        {
            Debug.LogWarning($"[QuestManager] 任务配置文件夹不存在: {fullPath}");
            return;
        }

        // 获取文件夹中所有的.asset文件
        string[] assetPaths = UnityEditor.AssetDatabase.FindAssets("t:QuestData", new[] { "Assets/" + questConfigPath });

        foreach (string guid in assetPaths)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            QuestData questData = UnityEditor.AssetDatabase.LoadAssetAtPath<QuestData>(assetPath);

            if (questData != null)
            {
                allQuests.Add(questData);
                if (enableDebugLogs)
                {
                    Debug.Log($"[QuestManager] 从编辑器加载任务配置: {questData.questName} ({assetPath})");
                }
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 编辑器模式下从文件夹加载了 {allQuests.Count} 个任务配置");
        }
    }
#endif

    /// <summary>
    /// 运行时模式下加载任务配置
    /// </summary>
    private void LoadQuestConfigsAtRuntime()
    {
        // 将路径转换为Resources相对路径
        string resourcesPath = questConfigPath;
        if (resourcesPath.StartsWith("Assets/"))
        {
            resourcesPath = resourcesPath.Substring(7); // 移除"Assets/"前缀
        }
        if (resourcesPath.StartsWith("Resources/"))
        {
            resourcesPath = resourcesPath.Substring(10); // 移除"Resources/"前缀
        }

        // 尝试从Resources文件夹加载
        QuestData[] questConfigs = Resources.LoadAll<QuestData>(resourcesPath);

        if (questConfigs.Length > 0)
        {
            allQuests.AddRange(questConfigs);
            if (enableDebugLogs)
            {
                Debug.Log($"[QuestManager] 运行时从Resources加载了 {questConfigs.Length} 个任务配置");
            }
        }
        else
        {
            Debug.LogWarning($"[QuestManager] 在Resources路径中未找到任务配置: {resourcesPath}");
            Debug.LogWarning($"[QuestManager] 提示: 如果要在运行时自动加载，请将任务配置文件放在Resources文件夹下");
        }
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

        // 构建任务目标索引，优化后续查找性能
        BuildObjectiveIndexes(quest);

        OnQuestStarted?.Invoke(quest);

        Debug.Log($"[QuestManager] 开始任务: {quest.questName}");
        return true;
    }
    public bool CanComplete(string questId)
    {
        return GetQuestById(questId).CanComplete();
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

        // 清理任务目标索引
        RemoveObjectiveIndexes(quest);

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

        // 清理任务目标索引
        RemoveObjectiveIndexes(quest);

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
        
        // 清理任务目标索引
        RemoveObjectiveIndexes(quest);

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
        Debug.Log($"[QuestManager] 检查任务是否可以开始: {quest.questName}");
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
    public bool CanStartQuest(string questId)
{
    var quest = GetQuest(questId);
    if (quest == null) return false;
    
    return CanStartQuest(quest);
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
        
        // 加载完成后重建索引
        RebuildAllObjectiveIndexes();
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
    ///设置敌人目标
    /// </summary>
    private void BuildObjectiveIndexes(QuestData quest)
    {
        foreach (var objective in quest.objectives)
        {
            var objRef = new QuestObjectiveRef(quest, objective);
            switch (objective.objectiveType)
            {
                case ObjectiveType.KillEnemy:
                case ObjectiveType.KillBoss:
                    AddToObjectiveIndex(enemyKillObjectives, objective.targetId, objRef);
                    
                    RegisterEnemyTargets(objective.targetId);
                    break;
                    
                case ObjectiveType.CollectItem:
                    AddToObjectiveIndex(itemCollectObjectives, objective.targetId, objRef);
                    RegisterItemTargets(objective.targetId);
                    break;
                    
                case ObjectiveType.TalkToNPC:
                    AddToObjectiveIndex(npcTalkObjectives, objective.targetId, objRef);
                    RegisterNPCTargets(objective.targetId);
                    break;
                    
                case ObjectiveType.ReachLocation:
                    AddToObjectiveIndex(locationObjectives, objective.targetId, objRef);
                    RegisterLocationTargets(objective.targetId);
                    break;
                    
                case ObjectiveType.UseItem:
                    AddToObjectiveIndex(itemUseObjectives, objective.targetId, objRef);
                    break;
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 为任务 {quest.questName} 构建了 {quest.objectives.Count} 个目标索引");
        }
    }
    
    /// <summary>
    /// 注册敌人目标
    /// </summary>
    private void RegisterEnemyTargets(string enemyId)
    {
        // 敌人目标注册逻辑
          if (EnemyKillTracker.Instance == null) return;
        List<Enemy> enemies = SceneController.Instance.EnemyControllers.FindAll(e => e.enemyConfig.enemyId == enemyId);
        if(enemies?.Count > 0)
        {
              foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.enemyConfig != null)
            {
                // 仅注册任务目标敌人
                if (IsEnemyTarget(enemy.enemyConfig.enemyId))
                {
                    EnemyKillTracker.Instance.RegisterEnemy(enemy);
                }
            }
        }
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 注册敌人目标: {enemyId}");
        }
        }
      
    }
    
    /// <summary>
    /// 注册物品目标
    /// </summary>
    private void RegisterItemTargets(string itemId)
    {
        // 物品目标注册逻辑
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 注册物品目标: {itemId}");
        }
    }
    
    /// <summary>
    /// 注册NPC目标
    /// </summary>
    private void RegisterNPCTargets(string npcId)
    {
        // 敌人目标注册逻辑
        List<NPCController> npcs = SceneController.Instance.NPCControllers.FindAll(e => e.npcConfig.npcId == npcId);
        if (npcs?.Count > 0)
        {
            foreach (var npc in npcs)
            {
                if (npc != null && npc.npcConfig != null)
                {
                    Debug.Log(npc.npcConfig.npcId+","+HasUncompletedTalkObjective(npc.npcConfig.npcId));
                    // 仅注册任务目标敌人
                    if (HasUncompletedTalkObjective(npc.npcConfig.npcId))
                    {

                        npc.UpdateQuestStatus();
                    }
                }
            }
        }
        if (enableDebugLogs)
            {
                Debug.Log($"[QuestManager] 注册NPC目标: {npcId}");
            }
    }
    
    /// <summary>
    /// 注册地点目标
    /// </summary>
    private void RegisterLocationTargets(string locationId)
    {
        // 地点目标注册逻辑
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 注册地点目标: {locationId}");
        }
    }
      
    /// <summary>
    /// 清理任务目标索引
    /// </summary>
    private void RemoveObjectiveIndexes(QuestData quest)
    {
        foreach (var objective in quest.objectives)
        {
            switch (objective.objectiveType)
            {
                case ObjectiveType.KillEnemy:
                case ObjectiveType.KillBoss:
                    RemoveFromObjectiveIndex(enemyKillObjectives, objective.targetId, quest);
                    break;
                    
                case ObjectiveType.CollectItem:
                    RemoveFromObjectiveIndex(itemCollectObjectives, objective.targetId, quest);
                    break;
                    
                case ObjectiveType.TalkToNPC:
                    RemoveFromObjectiveIndex(npcTalkObjectives, objective.targetId, quest);
                    break;
                    
                case ObjectiveType.ReachLocation:
                    RemoveFromObjectiveIndex(locationObjectives, objective.targetId, quest);
                    break;
                    
                case ObjectiveType.UseItem:
                    RemoveFromObjectiveIndex(itemUseObjectives, objective.targetId, quest);
                    break;
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 清理了任务 {quest.questName} 的目标索引");
        }
    }
    
    /// <summary>
    /// 添加到目标索引字典
    /// </summary>
    private void AddToObjectiveIndex(Dictionary<string, List<QuestObjectiveRef>> indexDict, string targetId, QuestObjectiveRef objRef)
    {
        if (!indexDict.ContainsKey(targetId))
        {
            indexDict[targetId] = new List<QuestObjectiveRef>();
        }
        indexDict[targetId].Add(objRef);
    }
    
    /// <summary>
    /// 从目标索引字典中移除
    /// </summary>
    private void RemoveFromObjectiveIndex(Dictionary<string, List<QuestObjectiveRef>> indexDict, string targetId, QuestData quest)
    {
        if (indexDict.TryGetValue(targetId, out var objectives))
        {
            objectives.RemoveAll(objRef => objRef.quest == quest);
            
            // 如果列表为空，移除整个键
            if (objectives.Count == 0)
            {
                indexDict.Remove(targetId);
            }
        }
    }
    
    /// <summary>
    /// 重建所有活跃任务的索引 - 用于系统初始化或重置后
    /// </summary>
    private void RebuildAllObjectiveIndexes()
    {
        // 清空所有索引
        ClearAllObjectiveIndexes();
        
        // 重建所有活跃任务的索引
        foreach (var quest in activeQuests)
        {
            BuildObjectiveIndexes(quest);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 重建了 {activeQuests.Count} 个活跃任务的索引");
        }
    }
    
    /// <summary>
    /// 清空所有目标索引
    /// </summary>
    private void ClearAllObjectiveIndexes()
    {
        enemyKillObjectives.Clear();
        itemCollectObjectives.Clear();
        npcTalkObjectives.Clear();
        locationObjectives.Clear();
        itemUseObjectives.Clear();
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
        
        // 清空所有索引
        ClearAllObjectiveIndexes();

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
    
    #region 调试和监控方法
    
    /// <summary>
    /// 显示索引字典状态（调试用）
    /// </summary>
    [Button("显示索引状态")]
    public void ShowIndexStatus()
    {
        Debug.Log($"[QuestManager] 索引状态统计:");
        Debug.Log($"  敌人击杀目标: {enemyKillObjectives.Count} 个不同目标");
        Debug.Log($"  物品收集目标: {itemCollectObjectives.Count} 个不同目标");
        Debug.Log($"  NPC对话目标: {npcTalkObjectives.Count} 个不同目标");
        Debug.Log($"  地点到达目标: {locationObjectives.Count} 个不同目标");
        Debug.Log($"  物品使用目标: {itemUseObjectives.Count} 个不同目标");
        
        int totalObjectives = 0;
        totalObjectives += enemyKillObjectives.Values.Sum(list => list.Count);
        totalObjectives += itemCollectObjectives.Values.Sum(list => list.Count);
        totalObjectives += npcTalkObjectives.Values.Sum(list => list.Count);
        totalObjectives += locationObjectives.Values.Sum(list => list.Count);
        totalObjectives += itemUseObjectives.Values.Sum(list => list.Count);
        
        Debug.Log($"  总索引目标数: {totalObjectives}");
    }
    
    /// <summary>
    /// 重建索引（调试用）
    /// </summary>
    [Button("重建索引")]
    public void DebugRebuildIndexes()
    {
        RebuildAllObjectiveIndexes();
        ShowIndexStatus();
    }
    
    /// <summary>
    /// 获取性能统计信息
    /// </summary>
    public string GetPerformanceStats()
    {
        int totalActiveObjectives = activeQuests.Sum(q => q.objectives.Count);
        int totalIndexedObjectives = 0;
        totalIndexedObjectives += enemyKillObjectives.Values.Sum(list => list.Count);
        totalIndexedObjectives += itemCollectObjectives.Values.Sum(list => list.Count);
        totalIndexedObjectives += npcTalkObjectives.Values.Sum(list => list.Count);
        totalIndexedObjectives += locationObjectives.Values.Sum(list => list.Count);
        totalIndexedObjectives += itemUseObjectives.Values.Sum(list => list.Count);
        
        return $"活跃任务: {activeQuests.Count}, 活跃目标: {totalActiveObjectives}, 索引目标: {totalIndexedObjectives}";
    }

    public bool IsEnemyTarget(string enemyId)
    {
        return enemyKillObjectives.ContainsKey(enemyId);
    }

    #endregion

    #region 地图切换事件处理
    /// <summary>
    /// 初始化事件监听器
    /// </summary>
    private void InitializeEventListeners()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete += OnMapTransitionComplete;
            Debug.Log("[QuestManager] 已订阅地图切换完成事件");
        }
        else
        {
            Debug.LogWarning("[QuestManager] MapManager实例未找到，无法订阅地图切换事件");
        }
        
        // 订阅场景内容重新加载完成事件
        SceneController.OnSceneContentReloaded += OnSceneContentReloaded;
        SceneController.OnEnemySpawned += OnEnemySpawned;
        Debug.Log("[QuestManager] 已订阅场景内容重新加载完成事件");
    }

    /// <summary>
    /// 清理事件监听器
    /// </summary>
    private void CleanupEventListeners()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete -= OnMapTransitionComplete;
            Debug.Log("[QuestManager] 已取消订阅地图切换完成事件");
        }

        // 取消订阅场景内容重新加载完成事件
    
        SceneController.OnSceneContentReloaded -= OnSceneContentReloaded;
        SceneController.OnEnemySpawned -= OnEnemySpawned;
        Debug.Log("[QuestManager] 已取消订阅场景内容重新加载完成事件");
    }

    /// <summary>
    /// 地图切换完成后重建任务索引
    /// </summary>
    /// <param name="targetAreaId">目标区域ID</param>
    private void OnMapTransitionComplete(string targetAreaId)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 地图切换完成，目标区域: {targetAreaId}");
        }
        
        // 不在这里立即重建索引，而是等待场景内容完全加载后再重建
        // 这样可以确保敌人和NPC已经完全生成
    }

    /// <summary>
    /// 场景内容重新加载完成后重建任务索引
    /// </summary>
    /// <param name="targetAreaId">目标区域ID</param>
    private void OnSceneContentReloaded(string targetAreaId)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[QuestManager] 场景内容重新加载完成，开始重建任务索引: {targetAreaId}");
        }
        
        StartCoroutine(RebuildQuestIndexesAfterTransition());
    }
    private void OnEnemySpawned(Enemy enemy)
    {
          if (isBatchProcessing)
    {
        pendingEnemies.Add(enemy);
    }
    else
    {
        RegisterSingleEnemy(enemy);
    }
    }
    /// <summary>
    /// 地图切换后重建任务索引的协程
    /// </summary>
    private System.Collections.IEnumerator RebuildQuestIndexesAfterTransition()
    {
        // 等待一帧，确保场景内容已完全加载
        yield return null;
        
        try
        {
            // 重建所有活跃任务的目标索引
            RebuildAllObjectiveIndexes();
            
            // 更新所有NPC的任务状态显示
            UpdateAllNPCQuestStatus();
            
            Debug.Log($"[QuestManager] 地图切换后任务索引重建完成，活跃任务数: {activeQuests.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QuestManager] 地图切换后重建任务索引时出错: {e.Message}");
        }
    }

    /// <summary>
    /// 更新所有NPC的任务状态
    /// </summary>
    private void UpdateAllNPCQuestStatus()
    {
        if (SceneController.Instance?.NPCControllers != null)
        {
            foreach (var npc in SceneController.Instance.NPCControllers)
            {
                if (npc != null)
                {
                    npc.UpdateQuestStatus();
                }
            }
            
            Debug.Log($"[QuestManager] 已更新 {SceneController.Instance.NPCControllers.Count} 个NPC的任务状态");
        }
    }

    /// <summary>
    /// 销毁时清理事件监听器
    /// </summary>
    private void OnDestroy()
    {
        CleanupEventListeners();
    }
    #endregion
}