using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// 状态机性能监控器
/// 用于监控和展示状态机优化的效果
/// </summary>
public class StateMachinePerformanceMonitor : MonoBehaviour
{
    [TabGroup("性能监控", "实时数据")]
    [LabelText("状态检查频率 (次/秒)")]
    [ReadOnly]
    [ShowInInspector]
    public float stateCheckFrequency = 0f;
    
    [TabGroup("性能监控", "实时数据")]
    [LabelText("事件驱动触发次数")]
    [ReadOnly]
    [ShowInInspector]
    public int eventDrivenTriggers = 0;
    
    [TabGroup("性能监控", "实时数据")]
    [LabelText("跳过的状态检查次数")]
    [ReadOnly]
    [ShowInInspector]
    public int skippedStateChecks = 0;
    
    [TabGroup("性能监控", "实时数据")]
    [LabelText("CPU使用率节省 (%)")]
    [ReadOnly]
    [ShowInInspector]
    public float cpuSavingPercentage = 0f;
    
    [TabGroup("性能监控", "历史数据")]
    [LabelText("状态检查历史")]
    [ReadOnly]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "GetHistoryLabel")]
    public List<string> stateCheckHistory = new List<string>();
    
    [TabGroup("性能监控", "配置")]
    [LabelText("监控间隔 (秒)")]
    [Range(0.1f, 2f)]
    public float monitorInterval = 1f;
    
    [TabGroup("性能监控", "配置")]
    [LabelText("历史记录最大数量")]
    [Range(10, 100)]
    public int maxHistoryCount = 50;
    
    // 私有变量
    private PlayerStateMachine playerStateMachine;
    private float lastMonitorTime;
    private int lastFrameStateChecks = 0;
    private int currentFrameStateChecks = 0;
    private int totalStateChecks = 0;
    private int totalPossibleChecks = 0;
    
    private void Start()
    {
        // 查找玩家状态机
        playerStateMachine = FindObjectOfType<PlayerStateMachine>();
        if (playerStateMachine == null)
        {
            Debug.LogWarning("[StateMachinePerformanceMonitor] 未找到PlayerStateMachine组件");
            enabled = false;
            return;
        }
        
        lastMonitorTime = Time.time;
        Debug.Log("[StateMachinePerformanceMonitor] 性能监控已启动");
    }
    
    private void Update()
    {
        // 计算状态检查频率
        currentFrameStateChecks++;
        totalPossibleChecks++;
        
        // 定期更新监控数据
        if (Time.time - lastMonitorTime >= monitorInterval)
        {
            UpdateMonitoringData();
            lastMonitorTime = Time.time;
        }
    }
    
    /// <summary>
    /// 更新监控数据
    /// </summary>
    private void UpdateMonitoringData()
    {
        // 计算状态检查频率
        float deltaTime = Time.time - (lastMonitorTime - monitorInterval);
        stateCheckFrequency = currentFrameStateChecks / deltaTime;
        
        // 计算CPU节省百分比
        if (totalPossibleChecks > 0)
        {
            cpuSavingPercentage = ((float)skippedStateChecks / totalPossibleChecks) * 100f;
        }
        
        // 添加历史记录
        string historyEntry = $"{Time.time:F1}s - 检查频率: {stateCheckFrequency:F1}/s, " +
                             $"事件触发: {eventDrivenTriggers}, " +
                             $"跳过检查: {skippedStateChecks}, " +
                             $"CPU节省: {cpuSavingPercentage:F1}%";
        
        stateCheckHistory.Add(historyEntry);
        
        // 限制历史记录数量
        if (stateCheckHistory.Count > maxHistoryCount)
        {
            stateCheckHistory.RemoveAt(0);
        }
        
        // 重置计数器
        lastFrameStateChecks = currentFrameStateChecks;
        currentFrameStateChecks = 0;
    }
    
    /// <summary>
    /// 记录状态检查被执行
    /// </summary>
    public void RecordStateCheck()
    {
        totalStateChecks++;
    }
    
    /// <summary>
    /// 记录状态检查被跳过
    /// </summary>
    public void RecordSkippedStateCheck()
    {
        skippedStateChecks++;
    }
    
    /// <summary>
    /// 记录事件驱动触发
    /// </summary>
    public void RecordEventDrivenTrigger()
    {
        eventDrivenTriggers++;
    }
    
    /// <summary>
    /// 获取历史记录标签
    /// </summary>
    private string GetHistoryLabel(string entry, int index)
    {
        return $"[{index}] {entry}";
    }
    
    #region Odin调试方法
    
    [TabGroup("性能监控", "调试工具")]
    [Button("重置监控数据")]
    [GUIColor(1f, 0.6f, 0.6f)]
    private void ResetMonitoringData()
    {
        stateCheckFrequency = 0f;
        eventDrivenTriggers = 0;
        skippedStateChecks = 0;
        cpuSavingPercentage = 0f;
        stateCheckHistory.Clear();
        totalStateChecks = 0;
        totalPossibleChecks = 0;
        currentFrameStateChecks = 0;
        lastFrameStateChecks = 0;
        
        Debug.Log("[StateMachinePerformanceMonitor] 监控数据已重置");
    }
    
    [TabGroup("性能监控", "调试工具")]
    [Button("导出性能报告")]
    private void ExportPerformanceReport()
    {
        string report = "=== 状态机性能报告 ===\n";
        report += $"当前状态检查频率: {stateCheckFrequency:F2} 次/秒\n";
        report += $"事件驱动触发次数: {eventDrivenTriggers}\n";
        report += $"跳过的状态检查: {skippedStateChecks}\n";
        report += $"CPU使用率节省: {cpuSavingPercentage:F2}%\n";
        report += $"总状态检查次数: {totalStateChecks}\n";
        report += $"总可能检查次数: {totalPossibleChecks}\n";
        report += "\n=== 历史记录 ===\n";
        
        foreach (string entry in stateCheckHistory)
        {
            report += entry + "\n";
        }
        
        Debug.Log(report);
    }
    
    #endregion
}