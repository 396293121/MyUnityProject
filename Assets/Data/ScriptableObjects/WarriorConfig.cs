using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 战士职业配置 - ScriptableObject
/// 包含战士职业的所有数值参数配置
/// </summary>
[CreateAssetMenu(fileName = "WarriorConfig", menuName = "Character Config/Warrior Config")]
[ShowOdinSerializedPropertiesInInspector]
public class WarriorConfig : ScriptableObject
{
    [TitleGroup("战士基础配置", "战士职业的核心属性设置", TitleAlignments.Centered)]
    [FoldoutGroup("战士基础配置/核心属性", expanded: true)]
    [HorizontalGroup("战士基础配置/核心属性/数值")]
    [VerticalGroup("战士基础配置/核心属性/数值/左列")]
    [LabelText("力量")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STR")]
    [InfoBox("战士的力量属性，影响物理攻击力和重击伤害")]
    public int strength = 15;
    
    [VerticalGroup("战士基础配置/核心属性/数值/左列")]
    [LabelText("敏捷")]
    [PropertyRange(1, 100)]
    [SuffixLabel("AGI")]
    [InfoBox("战士的敏捷属性，影响移动速度和攻击速度")]
    public int agility = 8;
    
    [VerticalGroup("战士基础配置/核心属性/数值/右列")]
    [LabelText("体力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STA")]
    [InfoBox("战士的体力属性，影响生命值和防御力")]
    public int stamina = 15;
    
    [VerticalGroup("战士基础配置/核心属性/数值/右列")]
    [LabelText("智力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("INT")]
    [InfoBox("战士的智力属性，影响魔法攻击力")]
    public int intelligence = 5;
    
    // 技能相关配置已移至CharacterSkillConfig
    
    [TitleGroup("攻击系统配置", "战士攻击相关参数设置", TitleAlignments.Centered)]
    [FoldoutGroup("攻击系统配置/基础攻击", expanded: true)]
    [HorizontalGroup("攻击系统配置/基础攻击/参数")]
    [VerticalGroup("攻击系统配置/基础攻击/参数/左列")]
    [LabelText("攻击范围")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("米")]
    [InfoBox("战士基础攻击范围")]
    public float attackRange = 2.5f;
    
    [VerticalGroup("攻击系统配置/基础攻击/参数/右列")]
    [LabelText("攻击高度")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("米")]
    [InfoBox("战士基础攻击高度")]
    public float attackHeight = 2.5f;
    
    [FoldoutGroup("攻击系统配置/基础攻击")]
    [HorizontalGroup("攻击系统配置/基础攻击/冷却")]
    [VerticalGroup("攻击系统配置/基础攻击/冷却/左列")]
    [LabelText("基础攻击冷却时间")]
    [PropertyRange(0.1f, 3f)]
    [SuffixLabel("秒")]
    [InfoBox("战士基础攻击冷却时间")]
    public float attackCooldown = 0.8f;
    
    [VerticalGroup("攻击系统配置/基础攻击/冷却/右列")]
    [LabelText("基础击退力度")]
    [PropertyRange(0f, 20f)]
    [SuffixLabel("力")]
    [InfoBox("战士基础攻击的击退力度")]
    public float knockbackForce = 8f;
    

 
}