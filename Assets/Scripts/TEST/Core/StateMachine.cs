using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 通用状态机基类 - 用于管理角色和敌人的状态切换
/// 优化目标：减少代码重复，提高状态管理的一致性和可维护性
/// </summary>
/// <typeparam name="T">状态枚举类型</typeparam>
public abstract class StateMachine<T> : MonoBehaviour where T : System.Enum
{
    [TabGroup("状态机")]
    [ShowInInspector, ReadOnly]
    [LabelText("当前状态")]
    public T CurrentState { get; private set; }
    
    [TabGroup("状态机")]
    [ShowInInspector, ReadOnly]
    [LabelText("上一个状态")]
    public T PreviousState { get; private set; }
    
    [TabGroup("状态机")]
    [ShowInInspector, ReadOnly]
    [LabelText("状态持续时间")]
    public float StateTime { get; private set; }
    
    [TabGroup("状态机")]
    [ShowInInspector, ReadOnly]
    [LabelText("状态切换次数")]
    public int StateChangeCount { get; private set; }
    
    /// <summary>
    /// 状态切换事件
    /// </summary>
    public System.Action<T, T> OnStateChanged;
    
    protected virtual void Start()
    {
        // 子类需要调用 InitializeState() 来设置初始状态
    }
    
    protected virtual void Update()
    {
        StateTime += Time.deltaTime;
        UpdateCurrentState();
    }
    
    /// <summary>
    /// 初始化状态机
    /// </summary>
    /// <param name="initialState">初始状态</param>
    protected void InitializeState(T initialState)
    {
        CurrentState = initialState;
        PreviousState = initialState;
        StateTime = 0f;
        StateChangeCount = 0;
        
        OnEnterState(initialState);
        Debug.Log($"[{gameObject.name}] 状态机初始化 - 初始状态: {initialState}");
    }
    
    /// <summary>
    /// 切换到新状态
    /// </summary>
    /// <param name="newState">新状态</param>
    public virtual void ChangeState(T newState)
    {
        // 如果是相同状态，不进行切换
        if (CurrentState.Equals(newState))
            return;
            
        T oldState = CurrentState;
        
        // 退出当前状态
        OnExitState(CurrentState);
        
        // 更新状态信息
        PreviousState = CurrentState;
        CurrentState = newState;
        StateTime = 0f;
        StateChangeCount++;
        
        // 进入新状态
        OnEnterState(newState);
        
        // 触发状态切换事件
        OnStateChanged?.Invoke(oldState, newState);
        
        Debug.Log($"[{gameObject.name}] 状态切换: {oldState} -> {newState} (第{StateChangeCount}次切换)");
    }
    
    /// <summary>
    /// 检查是否处于指定状态
    /// </summary>
    /// <param name="state">要检查的状态</param>
    /// <returns>是否处于该状态</returns>
    public bool IsInState(T state)
    {
        return CurrentState.Equals(state);
    }
    
    /// <summary>
    /// 检查是否处于任意指定状态
    /// </summary>
    /// <param name="states">状态数组</param>
    /// <returns>是否处于任意一个指定状态</returns>
    public bool IsInAnyState(params T[] states)
    {
        foreach (var state in states)
        {
            if (CurrentState.Equals(state))
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取在当前状态的持续时间
    /// </summary>
    /// <returns>状态持续时间</returns>
    public float GetStateTime()
    {
        return StateTime;
    }
    
    /// <summary>
    /// 检查状态是否持续了指定时间
    /// </summary>
    /// <param name="duration">持续时间</param>
    /// <returns>是否达到指定时间</returns>
    public bool HasBeenInStateFor(float duration)
    {
        return StateTime >= duration;
    }
    
    /// <summary>
    /// 强制重置状态时间
    /// </summary>
    public void ResetStateTime()
    {
        StateTime = 0f;
    }
    
    #region 抽象方法 - 子类必须实现
    
    /// <summary>
    /// 更新当前状态的逻辑 - 每帧调用
    /// </summary>
    protected abstract void UpdateCurrentState();
    
    /// <summary>
    /// 进入状态时的处理
    /// </summary>
    /// <param name="state">进入的状态</param>
    protected abstract void OnEnterState(T state);
    
    /// <summary>
    /// 退出状态时的处理
    /// </summary>
    /// <param name="state">退出的状态</param>
    protected abstract void OnExitState(T state);
    
    #endregion
    
    #region 调试功能
    
    [TabGroup("调试")]
    [Button("打印状态信息")]
    private void DebugPrintStateInfo()
    {
        Debug.Log($"[{gameObject.name}] 状态信息:\n" +
                 $"当前状态: {CurrentState}\n" +
                 $"上一状态: {PreviousState}\n" +
                 $"状态时间: {StateTime:F2}秒\n" +
                 $"切换次数: {StateChangeCount}");
    }
    
    [TabGroup("调试")]
    [ShowInInspector, ReadOnly]
    [LabelText("状态历史")]
    private string StateHistory => $"{PreviousState} -> {CurrentState}";
    
    #endregion
}