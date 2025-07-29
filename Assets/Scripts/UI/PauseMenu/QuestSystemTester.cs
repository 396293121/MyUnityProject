using UnityEngine;
using System.Collections;

/// <summary>
/// 任务系统测试脚本
/// 用于测试任务系统的各项功能
/// </summary>
public class QuestSystemTester : MonoBehaviour
{
    [Header("测试配置")]
    [Tooltip("是否在启动时自动运行测试")]
    public bool autoRunTestOnStart = false;
    
    [Tooltip("测试间隔时间")]
    public float testInterval = 2f;
    
    [Tooltip("要测试的任务ID")]
    public string testQuestId = "kill_wild_boar_001";
    
    [Header("调试信息")]
    [Tooltip("是否启用详细日志")]
    public bool enableVerboseLogging = true;
    
    void Start()
    {
        if (autoRunTestOnStart)
        {
            StartCoroutine(RunAutomaticTests());
        }
    }
    
    /// <summary>
    /// 运行自动测试
    /// </summary>
    private IEnumerator RunAutomaticTests()
    {
        Log("开始任务系统自动测试...");
        
        // 等待系统初始化
        yield return new WaitForSeconds(1f);
        
        // 测试1：系统初始化检查
        yield return StartCoroutine(TestSystemInitialization());
        yield return new WaitForSeconds(testInterval);
        
        // 测试2：任务启动
        yield return StartCoroutine(TestQuestStart());
        yield return new WaitForSeconds(testInterval);
        
        // 测试3：进度更新
        yield return StartCoroutine(TestProgressUpdate());
        yield return new WaitForSeconds(testInterval);
        
        // 测试4：任务完成
        yield return StartCoroutine(TestQuestCompletion());
        
        Log("任务系统自动测试完成！");
    }
    
    /// <summary>
    /// 测试系统初始化
    /// </summary>
    private IEnumerator TestSystemInitialization()
    {
        Log("=== 测试1：系统初始化检查 ===");
        
        // 检查QuestManager
        if (QuestManager.Instance != null)
        {
            Log("✓ QuestManager 初始化成功");
        }
        else
        {
            LogError("✗ QuestManager 初始化失败");
        }
        
        // 检查EnemyKillTracker
        if (EnemyKillTracker.Instance != null)
        {
            Log("✓ EnemyKillTracker 初始化成功");
        }
        else
        {
            LogError("✗ EnemyKillTracker 初始化失败");
        }
        
        // 检查DialogueManager
        if (DialogueManager.Instance != null)
        {
            Log("✓ DialogueManager 初始化成功");
        }
        else
        {
            LogWarning("⚠ DialogueManager 未找到（可选组件）");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 测试任务启动
    /// </summary>
    private IEnumerator TestQuestStart()
    {
        Log("=== 测试2：任务启动 ===");
        
        if (QuestManager.Instance == null)
        {
            LogError("QuestManager未初始化，跳过测试");
            yield break;
        }
        
        // 尝试启动测试任务
        bool startResult = QuestManager.Instance.StartQuest(testQuestId);
        
        if (startResult)
        {
            Log($"✓ 任务 {testQuestId} 启动成功");
            
            // 检查任务状态
            var quest = QuestManager.Instance.GetQuestById(testQuestId);
            if (quest != null && quest.questStatus == QuestStatus.InProgress)
            {
                Log("✓ 任务状态正确设置为InProgress");
            }
            else
            {
                LogError("✗ 任务状态设置错误");
            }
        }
        else
        {
            LogError($"✗ 任务 {testQuestId} 启动失败");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 测试进度更新
    /// </summary>
    private IEnumerator TestProgressUpdate()
    {
        Log("=== 测试3：进度更新 ===");
        
        if (QuestManager.Instance == null)
        {
            LogError("QuestManager未初始化，跳过测试");
            yield break;
        }
        
        var quest = QuestManager.Instance.GetQuestById(testQuestId);
        if (quest == null)
        {
            LogError("测试任务不存在，跳过测试");
            yield break;
        }
        
        // 记录初始进度
        float initialProgress = quest.GetProgressPercentage();
        Log($"初始进度: {initialProgress * 100:F1}%");
        
        // 检查任务目标类型
        if (quest.objectives != null && quest.objectives.Count > 0)
        {
            var firstObjective = quest.objectives[0];
            if (firstObjective.objectiveType == ObjectiveType.KillEnemy)
            {
                // 模拟击杀事件
                if (EnemyKillTracker.Instance != null)
                {
                    Log("模拟击杀敌人事件...");
                    
                    // 模拟敌人死亡
                    SimulateEnemyKill(firstObjective.targetId);
                    
                    yield return new WaitForSeconds(0.5f);
                    
                    // 检查进度是否更新
                    float newProgress = quest.GetProgressPercentage();
                    if (newProgress > initialProgress)
                    {
                        Log($"✓ 进度更新成功: {newProgress * 100:F1}%");
                    }
                    else
                    {
                        LogError("✗ 进度更新失败");
                    }
                }
            }
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 测试任务完成
    /// </summary>
    private IEnumerator TestQuestCompletion()
    {
        Log("=== 测试4：任务完成 ===");
        
        if (QuestManager.Instance == null)
        {
            LogError("QuestManager未初始化，跳过测试");
            yield break;
        }
        
        var quest = QuestManager.Instance.GetQuestById(testQuestId);
        if (quest == null)
        {
            LogError("测试任务不存在，跳过测试");
            yield break;
        }
        
        // 如果任务还没完成，模拟完成
        if (quest.questStatus != QuestStatus.Completed)
        {
            // 模拟完成所有目标
            if (quest.objectives != null && quest.objectives.Count > 0)
            {
                foreach (var objective in quest.objectives)
                {
                    if (objective.objectiveType == ObjectiveType.KillEnemy)
                    {
                        Log($"模拟完成击杀目标: {objective.targetId}");
                        
                        // 模拟击杀足够数量的敌人
                        int remainingKills = objective.targetProgress - objective.currentProgress;
                        for (int i = 0; i < remainingKills; i++)
                        {
                            SimulateEnemyKill(objective.targetId);
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
            }
        }
        
        // 检查任务是否完成
        if (quest.questStatus == QuestStatus.Completed)
        {
            Log("✓ 任务成功完成");
            Log($"✓ 最终进度: {quest.GetProgressPercentage() * 100:F1}%");
        }
        else
        {
            LogError("✗ 任务未能完成");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 模拟敌人击杀
    /// </summary>
    private void SimulateEnemyKill(string enemyType)
    {
        if (EnemyKillTracker.Instance != null)
        {
            // 直接调用击杀事件
            var killTracker = EnemyKillTracker.Instance;
            
            // 使用反射调用私有方法（仅用于测试）
            var method = killTracker.GetType().GetMethod("OnEnemyDeath", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                // 创建一个模拟的Enemy对象
                var mockEnemy = new GameObject("MockEnemy").AddComponent<MockEnemy>();
                mockEnemy.enemyType = enemyType;
                
                method.Invoke(killTracker, new object[] { mockEnemy });
                
                // 清理模拟对象
                DestroyImmediate(mockEnemy.gameObject);
            }
        }
    }
    
    #region 手动测试方法
    
    [ContextMenu("手动测试：启动任务")]
    public void ManualTestStartQuest()
    {
        if (QuestManager.Instance != null)
        {
            bool result = QuestManager.Instance.StartQuest(testQuestId);
            Log($"手动启动任务结果: {result}");
        }
        else
        {
            LogError("QuestManager未初始化");
        }
    }
    
    [ContextMenu("手动测试：模拟击杀")]
    public void ManualTestKillEnemy()
    {
        SimulateEnemyKill("WildBoar");
        Log("模拟击杀野猪");
    }
    
    [ContextMenu("手动测试：显示任务状态")]
    public void ManualTestShowQuestStatus()
    {
        if (QuestManager.Instance != null)
        {
            var quest = QuestManager.Instance.GetQuestById(testQuestId);
            if (quest != null)
            {
                Log($"任务状态: {quest.questStatus}");
                Log($"任务进度: {quest.GetProgressPercentage() * 100:F1}%");
            }
            else
            {
                LogError("任务不存在");
            }
        }
        else
        {
            LogError("QuestManager未初始化");
        }
    }
    
    [ContextMenu("手动测试：完成任务")]
    public void ManualTestCompleteQuest()
    {
        if (QuestManager.Instance != null)
        {
            bool result = QuestManager.Instance.CompleteQuest(testQuestId);
            Log($"手动完成任务结果: {result}");
        }
        else
        {
            LogError("QuestManager未初始化");
        }
    }
    
    #endregion
    
    #region 日志方法
    
    private void Log(string message)
    {
        if (enableVerboseLogging)
        {
            Debug.Log($"[QuestSystemTester] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        if (enableVerboseLogging)
        {
            Debug.LogWarning($"[QuestSystemTester] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[QuestSystemTester] {message}");
    }
    
    #endregion
}

/// <summary>
/// 模拟敌人类（仅用于测试）
/// </summary>
public class MockEnemy : MonoBehaviour
{
    public string enemyType = "WildBoar";
    
    public System.Type GetEnemyType()
    {
        return typeof(MockEnemy);
    }
}