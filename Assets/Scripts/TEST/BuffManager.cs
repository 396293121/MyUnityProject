using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;

[System.Serializable]
public class BuffEffect
{
    [LabelText("BUFF名称")]
    public string buffName;
    
    [LabelText("攻击力加成")]
    public float attackBonus;
    
    [LabelText("移动速度加成")]
    public float speedBonus;
    
    [LabelText("剩余时间")]
    public float remainingTime;
    
    [LabelText("总持续时间")]
    public float totalDuration;
    
    public BuffEffect(string name, float attack, float speed, float duration)
    {
        buffName = name;
        attackBonus = attack;
        speedBonus = speed;
        remainingTime = duration;
        totalDuration = duration;
    }
}

public class BuffManager : MonoBehaviour
{
    [TabGroup("BUFF状态")]
    [ShowInInspector, ReadOnly]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "buffName")]
    [LabelText("当前BUFF列表")]
    private List<BuffEffect> activeBuffs = new List<BuffEffect>();
    
    [TabGroup("BUFF状态")]
    [ShowInInspector, ReadOnly]
    [LabelText("总攻击力加成")]
    public float TotalAttackBonus { get; private set; }
    
    [TabGroup("BUFF状态")]
    [ShowInInspector, ReadOnly]
    [LabelText("总移动速度加成")]
    public float TotalSpeedBonus { get; private set; }
    
    private SimplePlayerController playerController;
    
    void Start()
    {
        playerController = GetComponent<SimplePlayerController>();
    }
    
    void Update()
    {
        UpdateBuffs();
    }
    
    /// <summary>
    /// 应用BUFF效果
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <param name="attackBonus">攻击力加成</param>
    /// <param name="speedBonus">移动速度加成</param>
    /// <param name="duration">持续时间</param>
    public void ApplyBuff(string buffName, float attackBonus, float speedBonus, float duration)
    {
        // 检查是否已存在同名BUFF
        BuffEffect existingBuff = activeBuffs.Find(buff => buff.buffName == buffName);
        if (existingBuff != null)
        {
            // 刷新现有BUFF的持续时间
            existingBuff.remainingTime = duration;
            existingBuff.totalDuration = duration;
            existingBuff.attackBonus = attackBonus;
            existingBuff.speedBonus = speedBonus;
            Debug.Log($"刷新BUFF: {buffName}");
        }
        else
        {
            // 添加新BUFF
            BuffEffect newBuff = new BuffEffect(buffName, attackBonus, speedBonus, duration);
            activeBuffs.Add(newBuff);
            Debug.Log($"应用新BUFF: {buffName}");
        }
        
        RecalculateBuffs();
    }
    
    /// <summary>
    /// 移除指定BUFF
    /// </summary>
    /// <param name="buffName">要移除的BUFF名称</param>
    public void RemoveBuff(string buffName)
    {
        BuffEffect buffToRemove = activeBuffs.Find(buff => buff.buffName == buffName);
        if (buffToRemove != null)
        {
            activeBuffs.Remove(buffToRemove);
            RecalculateBuffs();
            Debug.Log($"移除BUFF: {buffName}");
        }
    }
    
    /// <summary>
    /// 清除所有BUFF
    /// </summary>
    public void ClearAllBuffs()
    {
        activeBuffs.Clear();
        RecalculateBuffs();
        Debug.Log("清除所有BUFF");
    }
    
    /// <summary>
    /// 检查是否拥有指定BUFF
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <returns>是否拥有该BUFF</returns>
    public bool HasBuff(string buffName)
    {
        return activeBuffs.Exists(buff => buff.buffName == buffName);
    }
    
    /// <summary>
    /// 获取指定BUFF的剩余时间
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <returns>剩余时间，如果不存在返回0</returns>
    public float GetBuffRemainingTime(string buffName)
    {
        BuffEffect buff = activeBuffs.Find(b => b.buffName == buffName);
        return buff != null ? buff.remainingTime : 0f;
    }
    
    private void UpdateBuffs()
    {
        bool buffsChanged = false;
        
        // 更新所有BUFF的剩余时间
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].remainingTime -= Time.deltaTime;
            
            // 移除过期的BUFF
            if (activeBuffs[i].remainingTime <= 0)
            {
                Debug.Log($"BUFF过期: {activeBuffs[i].buffName}");
                activeBuffs.RemoveAt(i);
                buffsChanged = true;
            }
        }
        
        // 如果BUFF发生变化，重新计算总加成
        if (buffsChanged)
        {
            RecalculateBuffs();
        }
    }
    
    private void RecalculateBuffs()
    {
        TotalAttackBonus = 0f;
        TotalSpeedBonus = 0f;
        
        // 计算所有BUFF的总加成
        foreach (var buff in activeBuffs)
        {
            TotalAttackBonus += buff.attackBonus;
            TotalSpeedBonus += buff.speedBonus;
        }
        
        // 应用速度加成到玩家控制器
        if (playerController != null)
        {
            Debug.Log($"BUFF:{TotalAttackBonus},,,{TotalSpeedBonus}");
            // 假设SimplePlayerController有设置速度加成的方法
             playerController.SetSpeedMultiplier(TotalSpeedBonus/100);
            playerController.SetAttackMultiplier(TotalAttackBonus/100);
        }
    }
    
    /// <summary>
    /// 获取当前所有BUFF的信息（用于UI显示）
    /// </summary>
    /// <returns>BUFF信息列表</returns>
    public List<BuffEffect> GetActiveBuffs()
    {
        return new List<BuffEffect>(activeBuffs);
    }
    
    [TabGroup("调试功能")]
    [Button("测试攻击BUFF")]
    private void TestAttackBuff()
    {
        ApplyBuff("测试攻击BUFF", 0.2f, 0f, 10f); // 假设 0.2f 代表 20% 攻击力加成
    }
    
    [TabGroup("调试功能")]
    [Button("测试速度BUFF")]
    private void TestSpeedBuff()
    {
        ApplyBuff("测试速度BUFF", 0f, 0.5f, 15f); // 0.5f 代表 50% 速度加成
    }
    
    [TabGroup("调试功能")]
    [Button("清除所有BUFF")]
    private void DebugClearAllBuffs()
    {
        ClearAllBuffs();
    }
}