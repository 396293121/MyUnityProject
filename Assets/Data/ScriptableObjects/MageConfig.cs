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
    [InfoBox("法师的力量属性，影响物理攻击力")]
    public int strength = 8;
    
    [VerticalGroup("法师基础配置/核心属性/数值/左列")]
    [LabelText("敏捷")]
    [PropertyRange(1, 100)]
    [SuffixLabel("AGI")]
    [InfoBox("法师的敏捷属性，影响移动速度和攻击速度")]
    public int agility = 10;
    
    [VerticalGroup("法师基础配置/核心属性/数值/右列")]
    [LabelText("体力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STA")]
    [InfoBox("法师的体力属性，影响生命值")]
    public int stamina = 8;
    
    [VerticalGroup("法师基础配置/核心属性/数值/右列")]
    [LabelText("智力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("INT")]
    [InfoBox("法师的智力属性，影响魔法攻击力和魔法值")]
    public int intelligence = 18;
    
    [TitleGroup("法师专属技能", "法师职业独有的魔法配置", TitleAlignments.Centered)]
    [FoldoutGroup("法师专属技能/施法属性", expanded: true)]
    [HorizontalGroup("法师专属技能/施法属性/参数")]
    [VerticalGroup("法师专属技能/施法属性/参数/左列")]
    [LabelText("法术施放范围")]
    [PropertyRange(5f, 20f)]
    [SuffixLabel("米")]
    [InfoBox("法师法术的施放范围")]
    public float spellCastRange = 12f;
    
    [VerticalGroup("法师专属技能/施法属性/参数/右列")]
    [LabelText("魔法值回复速度")]
    [PropertyRange(0.5f, 10f)]
    [SuffixLabel("点/秒")]
    [InfoBox("法师魔法值的自动回复速度")]
    public float manaRegenRate = 2f;
    
    [TitleGroup("攻击系统配置", "法师攻击相关参数设置", TitleAlignments.Centered)]
    [FoldoutGroup("攻击系统配置/近战攻击", expanded: true)]
    [HorizontalGroup("攻击系统配置/近战攻击/参数")]
    [VerticalGroup("攻击系统配置/近战攻击/参数/左列")]
    [LabelText("攻击范围")]
    [PropertyRange(1f, 4f)]
    [SuffixLabel("米")]
    [InfoBox("法师近战攻击范围")]
    public float attackRange = 1.5f;
    
    [VerticalGroup("攻击系统配置/近战攻击/参数/右列")]
    [LabelText("攻击高度")]
    [PropertyRange(1f, 4f)]
    [SuffixLabel("米")]
    [InfoBox("法师近战攻击高度")]
    public float attackHeight = 1.5f;
    
    [FoldoutGroup("攻击系统配置/近战攻击")]
    [HorizontalGroup("攻击系统配置/近战攻击/冷却")]
    [VerticalGroup("攻击系统配置/近战攻击/冷却/左列")]
    [LabelText("基础攻击冷却时间")]
    [PropertyRange(0.1f, 3f)]
    [SuffixLabel("秒")]
    [InfoBox("法师基础攻击的冷却时间")]
    public float attackCooldown = 0.3f;
    
    [VerticalGroup("攻击系统配置/近战攻击/冷却/右列")]
    [LabelText("击退力度")]
    [PropertyRange(0f, 15f)]
    [SuffixLabel("力")]
    [InfoBox("法师近战攻击的击退力度")]
    public float knockbackForce = 3f;
    
    [TitleGroup("技能系统配置", "技能冷却和效果设置", TitleAlignments.Centered)]
    [FoldoutGroup("技能系统配置/技能冷却", expanded: true)]
    [HorizontalGroup("技能系统配置/技能冷却/法术技能")]
    [VerticalGroup("技能系统配置/技能冷却/法术技能/左列")]
    [LabelText("火球术冷却时间")]
    [PropertyRange(1f, 10f)]
    [SuffixLabel("秒")]
    [InfoBox("火球术技能的冷却时间")]
    public float fireballCooldown = 3f;
    
    [VerticalGroup("技能系统配置/技能冷却/法术技能/左列")]
    [LabelText("闪电箭冷却时间")]
    [PropertyRange(2f, 15f)]
    [SuffixLabel("秒")]
    [InfoBox("闪电箭技能的冷却时间")]
    public float lightningBoltCooldown = 5f;
    
    [VerticalGroup("技能系统配置/技能冷却/法术技能/左列")]
    [LabelText("冰霜新星冷却时间")]
    [PropertyRange(5f, 25f)]
    [SuffixLabel("秒")]
    [InfoBox("冰霜新星技能的冷却时间")]
    public float frostNovaCooldown = 12f;
    
    [VerticalGroup("技能系统配置/技能冷却/法术技能/右列")]
    [LabelText("治疗术冷却时间")]
    [PropertyRange(5f, 20f)]
    [SuffixLabel("秒")]
    [InfoBox("治疗术技能的冷却时间")]
    public float healCooldown = 8f;
    
    [VerticalGroup("技能系统配置/技能冷却/法术技能/右列")]
    [LabelText("传送术冷却时间")]
    [PropertyRange(8f, 30f)]
    [SuffixLabel("秒")]
    [InfoBox("传送术技能的冷却时间")]
    public float teleportCooldown = 15f;
    
    [FoldoutGroup("技能系统配置/魔法消耗", expanded: true)]
    [HorizontalGroup("技能系统配置/魔法消耗/法术消耗")]
    [VerticalGroup("技能系统配置/魔法消耗/法术消耗/左列")]
    [LabelText("火球术魔法消耗")]
    [PropertyRange(5, 50)]
    [SuffixLabel("MP")]
    [InfoBox("释放火球术消耗的魔法值")]
    public int fireballManaCost = 15;
    
    [VerticalGroup("技能系统配置/魔法消耗/法术消耗/左列")]
    [LabelText("闪电箭魔法消耗")]
    [PropertyRange(8, 60)]
    [SuffixLabel("MP")]
    [InfoBox("释放闪电箭消耗的魔法值")]
    public int lightningBoltManaCost = 20;
    
    [VerticalGroup("技能系统配置/魔法消耗/法术消耗/左列")]
    [LabelText("冰霜新星魔法消耗")]
    [PropertyRange(15, 80)]
    [SuffixLabel("MP")]
    [InfoBox("释放冰霜新星消耗的魔法值")]
    public int frostNovaManaCost = 30;
    
    [VerticalGroup("技能系统配置/魔法消耗/法术消耗/右列")]
    [LabelText("治疗术魔法消耗")]
    [PropertyRange(10, 50)]
    [SuffixLabel("MP")]
    [InfoBox("释放治疗术消耗的魔法值")]
    public int healManaCost = 20;
    
    [VerticalGroup("技能系统配置/魔法消耗/法术消耗/右列")]
    [LabelText("传送术魔法消耗")]
    [PropertyRange(10, 60)]
    [SuffixLabel("MP")]
    [InfoBox("释放传送术消耗的魔法值")]
    public int teleportManaCost = 25;
    
    [FoldoutGroup("技能系统配置/伤害倍率", expanded: true)]
    [HorizontalGroup("技能系统配置/伤害倍率/法术伤害")]
    [VerticalGroup("技能系统配置/伤害倍率/法术伤害/左列")]
    [LabelText("火球术伤害倍率")]
    [PropertyRange(0.8f, 3f)]
    [SuffixLabel("倍")]
    [InfoBox("火球术相对于魔法攻击力的伤害倍率")]
    public float fireballDamageMultiplier = 1.2f;
    
    [VerticalGroup("技能系统配置/伤害倍率/法术伤害/右列")]
    [LabelText("闪电箭伤害倍率")]
    [PropertyRange(1f, 4f)]
    [SuffixLabel("倍")]
    [InfoBox("闪电箭相对于魔法攻击力的伤害倍率")]
    public float lightningBoltDamageMultiplier = 1.5f;
    
    [VerticalGroup("技能系统配置/伤害倍率/法术伤害/左列")]
    [LabelText("冰霜新星伤害倍率")]
    [PropertyRange(0.5f, 2.5f)]
    [SuffixLabel("倍")]
    [InfoBox("冰霜新星相对于魔法攻击力的伤害倍率")]
    public float frostNovaDamageMultiplier = 1f;
    
    [TitleGroup("技能效果配置", "法师技能特殊效果设置", TitleAlignments.Centered)]
    [FoldoutGroup("技能效果配置/火球术效果", expanded: true)]
    [HorizontalGroup("技能效果配置/火球术效果/参数")]
    [VerticalGroup("技能效果配置/火球术效果/参数/左列")]
    [LabelText("火球术爆炸半径")]
    [PropertyRange(1f, 8f)]
    [SuffixLabel("米")]
    [InfoBox("火球术爆炸时的伤害半径")]
    public float fireballExplosionRadius = 2.5f;
    
    [VerticalGroup("技能效果配置/火球术效果/参数/右列")]
    [LabelText("火球术飞行速度")]
    [PropertyRange(5f, 25f)]
    [SuffixLabel("m/s")]
    [InfoBox("火球术的飞行速度")]
    public float fireballSpeed = 12f;
    
    [FoldoutGroup("技能效果配置/闪电箭效果", expanded: true)]
    [HorizontalGroup("技能效果配置/闪电箭效果/参数")]
    [VerticalGroup("技能效果配置/闪电箭效果/参数/左列")]
    [LabelText("闪电箭链式目标数")]
    [PropertyRange(1, 8)]
    [SuffixLabel("个")]
    [InfoBox("闪电箭可以链式攻击的目标数量")]
    public int lightningBoltChainTargets = 3;
    
    [VerticalGroup("技能效果配置/闪电箭效果/参数/右列")]
    [LabelText("闪电箭链式范围")]
    [PropertyRange(2f, 10f)]
    [SuffixLabel("米")]
    [InfoBox("闪电箭链式攻击的搜索范围")]
    public float lightningBoltChainRange = 5f;
    
    [FoldoutGroup("技能效果配置/冰霜新星效果", expanded: true)]
    [HorizontalGroup("技能效果配置/冰霜新星效果/参数")]
    [VerticalGroup("技能效果配置/冰霜新星效果/参数/左列")]
    [LabelText("冰霜新星半径")]
    [PropertyRange(3f, 12f)]
    [SuffixLabel("米")]
    [InfoBox("冰霜新星的攻击半径")]
    public float frostNovaRadius = 6f;
    
    [VerticalGroup("技能效果配置/冰霜新星效果/参数/右列")]
    [LabelText("冰霜新星减速效果")]
    [PropertyRange(0.1f, 0.8f)]
    [SuffixLabel("倍")]
    [InfoBox("冰霜新星造成的移动速度减少倍率")]
    public float frostNovaSlowEffect = 0.5f;
    
    [FoldoutGroup("技能效果配置/冰霜新星效果")]
    [LabelText("冰霜新星减速持续时间")]
    [PropertyRange(2f, 10f)]
    [SuffixLabel("秒")]
    [InfoBox("冰霜新星减速效果的持续时间")]
    public float frostNovaSlowDuration = 4f;
    
    [FoldoutGroup("技能效果配置/治疗术效果", expanded: true)]
    [HorizontalGroup("技能效果配置/治疗术效果/参数")]
    [VerticalGroup("技能效果配置/治疗术效果/参数/左列")]
    [LabelText("治疗术倍率")]
    [PropertyRange(0.5f, 2f)]
    [SuffixLabel("倍")]
    [InfoBox("治疗术相对于魔法攻击力的治疗倍率")]
    public float healMultiplier = 0.8f;
    
    [VerticalGroup("技能效果配置/治疗术效果/参数/右列")]
    [LabelText("智力治疗加成")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("倍")]
    [InfoBox("智力属性对治疗量的加成倍率")]
    public float intelligenceHealBonus = 2f;
    
    [FoldoutGroup("技能效果配置/治疗术效果")]
    [LabelText("治疗效果持续时间")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("秒")]
    [InfoBox("治疗特效的显示持续时间")]
    public float healEffectDuration = 2f;
    
    [FoldoutGroup("技能效果配置/传送术效果", expanded: true)]
    [LabelText("传送最大距离")]
    [PropertyRange(3f, 15f)]
    [SuffixLabel("米")]
    [InfoBox("传送术的最大传送距离")]
    public float teleportMaxDistance = 8f;
    
    [TitleGroup("动画系统配置", "法师动画时间参数设置", TitleAlignments.Centered)]
    [FoldoutGroup("动画系统配置/施法动画", expanded: true)]
    [HorizontalGroup("动画系统配置/施法动画/时间")]
    [VerticalGroup("动画系统配置/施法动画/时间/左列")]
    [LabelText("火球术施法时间")]
    [PropertyRange(0.2f, 1.5f)]
    [SuffixLabel("秒")]
    [InfoBox("火球术的施法动画时间")]
    public float fireballCastTime = 0.5f;
    
    [VerticalGroup("动画系统配置/施法动画/时间/左列")]
    [LabelText("闪电箭施法时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("闪电箭的施法动画时间")]
    public float lightningBoltCastTime = 0.3f;
    
    [VerticalGroup("动画系统配置/施法动画/时间/左列")]
    [LabelText("冰霜新星施法时间")]
    [PropertyRange(0.3f, 2f)]
    [SuffixLabel("秒")]
    [InfoBox("冰霜新星的施法动画时间")]
    public float frostNovaCastTime = 0.8f;
    
    [VerticalGroup("动画系统配置/施法动画/时间/右列")]
    [LabelText("治疗术施法时间")]
    [PropertyRange(0.2f, 1.5f)]
    [SuffixLabel("秒")]
    [InfoBox("治疗术的施法动画时间")]
    public float healCastTime = 0.4f;
    
    [VerticalGroup("动画系统配置/施法动画/时间/右列")]
    [LabelText("传送术施法时间")]
    [PropertyRange(0.2f, 1.5f)]
    [SuffixLabel("秒")]
    [InfoBox("传送术的施法动画时间")]
    public float teleportCastTime = 0.6f;
    
    [FoldoutGroup("动画系统配置/攻击恢复", expanded: true)]
    [HorizontalGroup("动画系统配置/攻击恢复/时间")]
    [VerticalGroup("动画系统配置/攻击恢复/时间/左列")]
    [LabelText("普通攻击恢复时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("普通攻击后的恢复时间")]
    public float normalAttackRecoveryTime = 0.3f;
    
    [VerticalGroup("动画系统配置/攻击恢复/时间/右列")]
    [LabelText("法术恢复时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("释放法术后的恢复时间")]
    public float spellRecoveryTime = 0.4f;
}