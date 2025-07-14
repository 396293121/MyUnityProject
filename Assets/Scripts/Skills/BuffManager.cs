using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// BUFF管理器 - 处理角色的增益和减益效果
/// 与技能系统配合使用
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class BuffManager : MonoBehaviour
{
    [TitleGroup("BUFF系统", "角色增益减益效果管理", TitleAlignments.Centered)]
    [FoldoutGroup("BUFF系统/当前BUFF列表", expanded: true)]
    [LabelText("激活的BUFF")]
    [ReadOnly]
    [ShowInInspector]
    private List<BuffEffect> activeBuffs = new List<BuffEffect>();
    
    [FoldoutGroup("BUFF系统/组件引用", expanded: false)]
    [LabelText("角色控制器")]
    [Required]
    [InfoBox("需要应用BUFF效果的角色控制器")]
    public Character characterController;
    
    [FoldoutGroup("BUFF系统/调试设置", expanded: false)]
    [LabelText("显示调试信息")]
    [InfoBox("在控制台显示BUFF相关的调试信息")]
    public bool showDebugInfo = true;
    
    private void Awake()
    {
        // 自动获取角色控制器引用
        if (characterController == null)
        {
            characterController = GetComponent<Character>();
        }
    }
    
    private void Update()
    {
        // 更新所有BUFF的持续时间
        UpdateBuffDurations();
    }
    
    /// <summary>
    /// 应用BUFF效果
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <param name="attackBonus">攻击力加成</param>
    /// <param name="speedBonus">速度加成</param>
    /// <param name="duration">持续时间</param>
    public void ApplyBuff(string buffName, float attackBonus, float speedBonus, float duration)
    {
        // 检查是否已存在同名BUFF
        BuffEffect existingBuff = activeBuffs.Find(buff => buff.buffName == buffName);
        if (existingBuff != null)
        {
            // 刷新现有BUFF的持续时间
            existingBuff.remainingDuration = duration;
            if (showDebugInfo)
            {
                Debug.Log($"刷新BUFF: {buffName}，持续时间重置为 {duration} 秒");
            }
            return;
        }
        
        // 创建新的BUFF效果
        BuffEffect newBuff = new BuffEffect
        {
            buffName = buffName,
            attackBonus = attackBonus,
            speedBonus = speedBonus,
            duration = duration,
            remainingDuration = duration,
            isActive = true
        };
        
        // 添加到激活列表
        activeBuffs.Add(newBuff);
        
        // 应用BUFF效果到角色
        ApplyBuffToCharacter(newBuff);
        
        if (showDebugInfo)
        {
            Debug.Log($"应用BUFF: {buffName}，攻击力+{attackBonus}，速度+{speedBonus}，持续 {duration} 秒");
        }
    }
    
    /// <summary>
    /// 应用治疗BUFF
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <param name="healAmount">治疗量</param>
    /// <param name="duration">持续时间</param>
    /// <param name="healInterval">治疗间隔</param>
    public void ApplyHealBuff(string buffName, float healAmount, float duration, float healInterval = 1f)
    {
        // 检查是否已存在同名BUFF
        BuffEffect existingBuff = activeBuffs.Find(buff => buff.buffName == buffName);
        if (existingBuff != null)
        {
            existingBuff.remainingDuration = duration;
            return;
        }
        
        // 创建治疗BUFF
        BuffEffect healBuff = new BuffEffect
        {
            buffName = buffName,
            healAmount = healAmount,
            duration = duration,
            remainingDuration = duration,
            healInterval = healInterval,
            lastHealTime = Time.time,
            isHealBuff = true,
            isActive = true
        };
        
        activeBuffs.Add(healBuff);
        
        if (showDebugInfo)
        {
            Debug.Log($"应用治疗BUFF: {buffName}，每 {healInterval} 秒治疗 {healAmount} 点生命值，持续 {duration} 秒");
        }
    }
    
    /// <summary>
    /// 移除指定BUFF
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    public void RemoveBuff(string buffName)
    {
        BuffEffect buffToRemove = activeBuffs.Find(buff => buff.buffName == buffName);
        if (buffToRemove != null)
        {
            RemoveBuffFromCharacter(buffToRemove);
            activeBuffs.Remove(buffToRemove);
            
            if (showDebugInfo)
            {
                Debug.Log($"移除BUFF: {buffName}");
            }
        }
    }
    
    /// <summary>
    /// 清除所有BUFF
    /// </summary>
    public void ClearAllBuffs()
    {
        foreach (BuffEffect buff in activeBuffs)
        {
            RemoveBuffFromCharacter(buff);
        }
        activeBuffs.Clear();
        
        if (showDebugInfo)
        {
            Debug.Log("清除所有BUFF");
        }
    }
    
    /// <summary>
    /// 检查是否有指定BUFF
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <returns>是否存在该BUFF</returns>
    public bool HasBuff(string buffName)
    {
        return activeBuffs.Exists(buff => buff.buffName == buffName && buff.isActive);
    }
    
    /// <summary>
    /// 获取BUFF剩余时间
    /// </summary>
    /// <param name="buffName">BUFF名称</param>
    /// <returns>剩余时间</returns>
    public float GetBuffRemainingTime(string buffName)
    {
        BuffEffect buff = activeBuffs.Find(b => b.buffName == buffName);
        return buff != null ? buff.remainingDuration : 0f;
    }
    
    /// <summary>
    /// 更新BUFF持续时间
    /// </summary>
    private void UpdateBuffDurations()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            BuffEffect buff = activeBuffs[i];
            
            // 更新剩余时间
            buff.remainingDuration -= Time.deltaTime;
            
            // 处理治疗BUFF
            if (buff.isHealBuff && Time.time >= buff.lastHealTime + buff.healInterval)
            {
                ProcessHealBuff(buff);
                buff.lastHealTime = Time.time;
            }
            
            // 检查BUFF是否过期
            if (buff.remainingDuration <= 0)
            {
                RemoveBuffFromCharacter(buff);
                activeBuffs.RemoveAt(i);
                
                if (showDebugInfo)
                {
                    Debug.Log($"BUFF过期: {buff.buffName}");
                }
            }
        }
    }
    
    /// <summary>
    /// 处理治疗BUFF
    /// </summary>
    /// <param name="healBuff">治疗BUFF</param>
    private void ProcessHealBuff(BuffEffect healBuff)
    {
        if (characterController != null)
        {
            int healAmount = Mathf.RoundToInt(healBuff.healAmount);
            characterController.currentHealth = Mathf.Min(
                characterController.currentHealth + healAmount,
                characterController.maxHealth
            );
            
            if (showDebugInfo)
            {
                Debug.Log($"治疗BUFF生效: {healBuff.buffName}，恢复 {healAmount} 点生命值");
            }
        }
    }
    
    /// <summary>
    /// 将BUFF效果应用到角色
    /// </summary>
    /// <param name="buff">BUFF效果</param>
    private void ApplyBuffToCharacter(BuffEffect buff)
    {
        if (characterController == null) return;
        
        // 应用攻击力加成
        if (buff.attackBonus != 0)
        {
            characterController.physicalAttack += Mathf.RoundToInt(buff.attackBonus);
        }
        
        // 应用速度加成
        if (buff.speedBonus != 0)
        {
            characterController.speed += buff.speedBonus;
        }
    }
    
    /// <summary>
    /// 从角色身上移除BUFF效果
    /// </summary>
    /// <param name="buff">BUFF效果</param>
    private void RemoveBuffFromCharacter(BuffEffect buff)
    {
        if (characterController == null) return;
        
        // 移除攻击力加成
        if (buff.attackBonus != 0)
        {
            characterController.physicalAttack -= Mathf.RoundToInt(buff.attackBonus);
        }
        
        // 移除速度加成
        if (buff.speedBonus != 0)
        {
            characterController.speed -= buff.speedBonus;
        }
    }
    
    /// <summary>
    /// 设置角色控制器引用
    /// </summary>
    /// <param name="character">角色控制器</param>
    public void SetCharacterController(Character character)
    {
        this.characterController = character;
        Debug.Log("已设置BuffManager的角色控制器引用");
    }
    
    /// <summary>
    /// 获取所有激活的BUFF信息
    /// </summary>
    /// <returns>BUFF信息列表</returns>
    public List<BuffInfo> GetActiveBuffsInfo()
    {
        List<BuffInfo> buffInfos = new List<BuffInfo>();
        
        foreach (BuffEffect buff in activeBuffs)
        {
            if (buff.isActive)
            {
                // 确定BUFF类型
                string buffType = "Unknown";
                float buffValue = 0f;
                
                if (buff.isHealBuff)
                {
                    buffType = "Heal";
                    buffValue = buff.healAmount;
                }
                else if (buff.attackBonus != 0)
                {
                    buffType = "Attack";
                    buffValue = buff.attackBonus;
                }
                else if (buff.speedBonus != 0)
                {
                    buffType = "Speed";
                    buffValue = buff.speedBonus;
                }
                
                buffInfos.Add(new BuffInfo
                {
                    name = buff.buffName,
                    buffName = buff.buffName,
                    type = buffType,
                    value = buffValue,
                    attackBonus = buff.attackBonus,
                    speedBonus = buff.speedBonus,
                    healAmount = buff.healAmount,
                    remainingTime = buff.remainingDuration,
                    remainingDuration = buff.remainingDuration,
                    isHealBuff = buff.isHealBuff
                });
            }
        }
        
        return buffInfos;
    }
    
    /// <summary>
    /// 获取所有激活的BUFF（兼容性方法）
    /// </summary>
    /// <returns>BUFF信息列表</returns>
    public List<BuffInfo> GetAllActiveBuffs()
    {
        return GetActiveBuffsInfo();
    }
}

/// <summary>
/// BUFF效果数据结构
/// </summary>
[System.Serializable]
public class BuffEffect
{
    [LabelText("BUFF名称")]
    public string buffName;
    
    [LabelText("攻击力加成")]
    public float attackBonus;
    
    [LabelText("速度加成")]
    public float speedBonus;
    
    [LabelText("治疗量")]
    public float healAmount;
    
    [LabelText("持续时间")]
    public float duration;
    
    [LabelText("剩余时间")]
    public float remainingDuration;
    
    [LabelText("治疗间隔")]
    public float healInterval = 1f;
    
    [LabelText("上次治疗时间")]
    public float lastHealTime;
    
    [LabelText("是否为治疗BUFF")]
    public bool isHealBuff;
    
    [LabelText("是否激活")]
    public bool isActive;
}

/// <summary>
/// BUFF信息结构（用于UI显示）
/// </summary>
[System.Serializable]
public class BuffInfo
{
    public string name;           // BUFF名称（兼容性字段）
    public string buffName;       // BUFF名称
    public string type;           // BUFF类型
    public float value;           // BUFF效果值
    public float attackBonus;     // 攻击力加成
    public float speedBonus;      // 速度加成
    public float healAmount;      // 治疗量
    public float remainingTime;   // 剩余时间（兼容性字段）
    public float remainingDuration; // 剩余持续时间
    public bool isHealBuff;       // 是否为治疗BUFF
}