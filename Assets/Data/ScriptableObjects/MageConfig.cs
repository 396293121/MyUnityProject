using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 法师职业配置 - ScriptableObject
/// 包含法师职业的所有数值参数配置
/// </summary>
[CreateAssetMenu(fileName = "MageConfig", menuName = "Character Config/Mage Config")]
[ShowOdinSerializedPropertiesInInspector]
public class MageConfig : ScriptableObject
{
    [TitleGroup("法师基础配置", "法师职业的核心属性设置", TitleAlignments.Centered)]
    [FoldoutGroup("法师基础配置/核心属性", expanded: true)]
    [HorizontalGroup("法师基础配置/核心属性/数值")]
    [VerticalGroup("法师基础配置/核心属性/数值/左列")]
    [LabelText("力量")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STR")]
    [InfoBox("法师的力量属性，影响物理攻击力和重击伤害")]
    public int strength = 15;
    
    [VerticalGroup("法师基础配置/核心属性/数值/左列")]
    [LabelText("敏捷")]
    [PropertyRange(1, 100)]
    [SuffixLabel("AGI")]
    [InfoBox("法师的敏捷属性，影响移动速度和攻击速度")]
    public int agility = 8;
    
    [VerticalGroup("法师基础配置/核心属性/数值/右列")]
    [LabelText("体力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STA")]
    [InfoBox("法师的体力属性，影响生命值和防御力")]
    public int stamina = 15;
    
    [VerticalGroup("法师基础配置/核心属性/数值/右列")]
    [LabelText("智力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("INT")]
    [InfoBox("法师的智力属性，影响魔法攻击力")]
    public int intelligence = 5;
    
    // 技能相关配置已移至CharacterSkillConfig
    [VerticalGroup("法师基础配置/核心属性/数值/右列")]
    [LabelText("生命值")]
    [PropertyRange(1, 200)]
    [SuffixLabel("HP")]
    [InfoBox("法师的生命值，影响生命值")]
    public int maxHealth = 100;
    
    [VerticalGroup("法师基础配置/核心属性/数值/右列")]
    [LabelText("魔法值")]
    [PropertyRange(1, 200)]
    [SuffixLabel("MP")]
    [InfoBox("法师的魔法值，影响魔法值")]
    public int maxMana = 100;
    

    [LabelText("魔法值回复速度")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("点/秒")]
    [InfoBox("法师魔法值的自动回复速度")]
    public float manaRegenRate = 2f;

}