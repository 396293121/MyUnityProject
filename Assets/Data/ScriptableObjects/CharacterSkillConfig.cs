using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 角色技能配置 - 独立的技能配置ScriptableObject
/// 将技能相关配置从职业配置中分离出来
/// </summary>
[CreateAssetMenu(fileName = "CharacterSkillConfig", menuName = "Character Config/Character Skill Config")]
[ShowOdinSerializedPropertiesInInspector]
public class CharacterSkillConfig : ScriptableObject
{
    [TitleGroup("角色技能配置", "基于skillDataConfig的技能系统配置", TitleAlignments.Centered)]
    [FoldoutGroup("角色技能配置/基础信息", expanded: true)]
    [LabelText("配置名称")]
    [InfoBox("该技能配置的名称，用于标识不同角色的技能设置")]
    public string configName = "默认技能配置";
    
    [FoldoutGroup("角色技能配置/基础信息")]
    [LabelText("适用角色类型")]
    [InfoBox("该配置适用的角色类型")]
    public CharacterType characterType = CharacterType.Warrior;
    
    [FoldoutGroup("角色技能配置/技能列表", expanded: true)]
    [LabelText("技能数据列表")]
    [Required]
    [InfoBox("角色可使用的技能列表，基于skillDataConfig配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "skillName")]
    public List<skillDataConfig> skillDataList = new List<skillDataConfig>();
    
    [FoldoutGroup("角色技能配置/输入映射", expanded: true)]
    [LabelText("技能按键映射")]
    [InfoBox("技能对应的输入按键配置，与新输入系统配合使用")]
    [TableList(ShowIndexLabels = true)]
    public List<SkillInputMapping> skillInputMappings = new List<SkillInputMapping>();
    
    [FoldoutGroup("角色技能配置/全局设置", expanded: false)]
    [LabelText("全局技能冷却倍率")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("倍")]
    [InfoBox("应用于所有技能的冷却时间倍率")]
    public float globalCooldownMultiplier = 1f;
    
    [FoldoutGroup("角色技能配置/全局设置")]
    [LabelText("全局法力消耗倍率")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("倍")]
    [InfoBox("应用于所有技能的法力消耗倍率")]
    public float globalManaCostMultiplier = 1f;
    
    [FoldoutGroup("角色技能配置/全局设置")]
    [LabelText("全局伤害倍率")]
    [PropertyRange(0.1f, 5f)]
    [SuffixLabel("倍")]
    [InfoBox("应用于所有技能的伤害倍率")]
    public float globalDamageMultiplier = 1f;
    
    [FoldoutGroup("角色技能配置/特殊效果", expanded: false)]
    [LabelText("技能连击系统")]
    [InfoBox("是否启用技能连击系统")]
    public bool enableComboSystem = false;
    
    [FoldoutGroup("角色技能配置/特殊效果")]
    [LabelText("连击窗口时间")]
    [PropertyRange(0.1f, 3f)]
    [SuffixLabel("秒")]
    [ShowIf("enableComboSystem")]
    [InfoBox("技能连击的时间窗口")]
    public float comboWindow = 1f;
    
    [FoldoutGroup("角色技能配置/特殊效果")]
    [LabelText("技能升级系统")]
    [InfoBox("是否启用技能升级系统")]
    public bool enableSkillUpgrade = false;
    
    [FoldoutGroup("角色技能配置/特殊效果")]
    [LabelText("最大技能等级")]
    [PropertyRange(1, 10)]
    [ShowIf("enableSkillUpgrade")]
    [InfoBox("技能的最大等级")]
    public int maxSkillLevel = 3;
    
    /// <summary>
    /// 获取指定索引的技能数据
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>技能数据</returns>
    public skillDataConfig GetSkillData(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillDataList.Count)
        {
            return skillDataList[skillIndex];
        }
        return null;
    }
    
    /// <summary>
    /// 获取技能数量
    /// </summary>
    /// <returns>技能数量</returns>
    public int GetSkillCount()
    {
        return skillDataList.Count;
    }
    
    /// <summary>
    /// 获取指定技能的输入映射
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>输入映射</returns>
    public SkillInputMapping GetSkillInputMapping(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillInputMappings.Count)
        {
            return skillInputMappings[skillIndex];
        }
        return null;
    }
    
    /// <summary>
    /// 应用全局倍率到技能数据
    /// </summary>
    /// <param name="skillData">原始技能数据</param>
    /// <returns>应用倍率后的技能数据</returns>
    public skillDataConfig ApplyGlobalMultipliers(skillDataConfig skillData)
    {
        if (skillData == null) return null;
        
        // 创建技能数据的副本
        skillDataConfig modifiedSkill = Instantiate(skillData);
        
        // 应用全局倍率
        modifiedSkill.cooldown *= globalCooldownMultiplier;
        modifiedSkill.manaCost = Mathf.RoundToInt(modifiedSkill.manaCost * globalManaCostMultiplier);
        modifiedSkill.damage *= globalDamageMultiplier;
        
        return modifiedSkill;
    }
    
    /// <summary>
    /// 验证配置的完整性
    /// </summary>
    /// <returns>验证结果</returns>
    [Button("验证配置")]
    [FoldoutGroup("角色技能配置/调试工具", expanded: false)]
    public bool ValidateConfiguration()
    {
        bool isValid = true;
        
        // 检查技能列表
        if (skillDataList == null || skillDataList.Count == 0)
        {
            Debug.LogWarning($"[{configName}] 技能列表为空");
            isValid = false;
        }
        
        // 检查输入映射
        if (skillInputMappings == null || skillInputMappings.Count != skillDataList.Count)
        {
            Debug.LogWarning($"[{configName}] 输入映射数量与技能数量不匹配");
            isValid = false;
        }
        
        // 检查每个技能数据
        for (int i = 0; i < skillDataList.Count; i++)
        {
            if (skillDataList[i] == null)
            {
                Debug.LogWarning($"[{configName}] 技能索引 {i} 的数据为空");
                isValid = false;
            }
        }
        
        // 检查输入映射的重复
        HashSet<string> usedInputs = new HashSet<string>();
        for (int i = 0; i < skillInputMappings.Count; i++)
        {
            var mapping = skillInputMappings[i];
            if (mapping != null && !string.IsNullOrEmpty(mapping.inputActionName))
            {
                if (usedInputs.Contains(mapping.inputActionName))
                {
                    Debug.LogWarning($"[{configName}] 输入动作 '{mapping.inputActionName}' 被重复使用");
                    isValid = false;
                }
                usedInputs.Add(mapping.inputActionName);
            }
        }
        
        if (isValid)
        {
            Debug.Log($"[{configName}] 配置验证通过");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 创建默认配置
    /// </summary>
    [Button("创建默认配置")]
    [FoldoutGroup("角色技能配置/调试工具")]
    public void CreateDefaultConfiguration()
    {
        skillInputMappings.Clear();
        
        // 为每个技能创建默认输入映射
        for (int i = 0; i < skillDataList.Count; i++)
        {
            skillInputMappings.Add(new SkillInputMapping
            {
                skillIndex = i,
                inputActionName = $"Skill{i + 1}",
                keyboardKey = GetDefaultKeyForSkill(i),
                description = skillDataList[i] != null ? skillDataList[i].skillName : $"技能{i + 1}"
            });
        }
        
        Debug.Log($"[{configName}] 已创建默认配置");
    }
    
    /// <summary>
    /// 获取技能的默认按键
    /// </summary>
    /// <param name="skillIndex">技能索引</param>
    /// <returns>默认按键</returns>
    private KeyCode GetDefaultKeyForSkill(int skillIndex)
    {
        switch (skillIndex)
        {
            case 0: return KeyCode.Q;
            case 1: return KeyCode.W;
            case 2: return KeyCode.E;
            case 3: return KeyCode.R;
            default: return KeyCode.None;
        }
    }
}

/// <summary>
/// 技能输入映射
/// </summary>
[System.Serializable]
public class SkillInputMapping
{
    [LabelText("技能索引")]
    [ReadOnly]
    public int skillIndex;
    
    [LabelText("输入动作名称")]
    [InfoBox("对应InputSystem中的Action名称")]
    public string inputActionName;
    
    [LabelText("键盘按键")]
    [InfoBox("对应的键盘按键（用于显示）")]
    public KeyCode keyboardKey;
    
    [LabelText("描述")]
    [InfoBox("技能的描述信息")]
    public string description;
}

/// <summary>
/// 角色类型枚举
/// </summary>
public enum CharacterType
{
    [LabelText("战士")]
    Warrior,
    
    [LabelText("法师")]
    Mage,
    
    [LabelText("射手")]
    Archer,
    
    [LabelText("通用")]
    Generic
}