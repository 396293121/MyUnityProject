using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 射手职业配置 - ScriptableObject
/// 包含射手职业的所有数值参数配置
/// </summary>
[CreateAssetMenu(fileName = "ArcherConfig", menuName = "Character Config/Archer Config")]
[ShowOdinSerializedPropertiesInInspector]
public class ArcherConfig : ScriptableObject
{
    [TitleGroup("基础属性")]
    [FoldoutGroup("基础属性/核心属性", expanded: true)]
    [LabelText("力量")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STR")]
    [InfoBox("射手的力量属性，影响物理攻击力")]
    public int strength = 10;
    
    [FoldoutGroup("基础属性/核心属性")]
    [LabelText("敏捷")]
    [PropertyRange(1, 100)]
    [SuffixLabel("AGI")]
    [InfoBox("射手的敏捷属性，影响移动速度和攻击速度")]
    public int agility = 15;
    
    [FoldoutGroup("基础属性/核心属性")]
    [LabelText("体力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("STA")]
    [InfoBox("射手的体力属性，影响生命值")]
    public int stamina = 10;
    
    [FoldoutGroup("基础属性/核心属性")]
    [LabelText("智力")]
    [PropertyRange(1, 100)]
    [SuffixLabel("INT")]
    [InfoBox("射手的智力属性，影响魔法攻击力")]
    public int intelligence = 8;
    
    [TitleGroup("射击属性")]
    [HorizontalGroup("射击属性/射击设置")]
    [VerticalGroup("射击属性/射击设置/基础射击")]
    [LabelText("射击范围")]
    [PropertyRange(5f, 20f)]
    [SuffixLabel("米")]
    [InfoBox("射手的射击攻击范围")]
    public float shootRange = 10f;
    
    [VerticalGroup("射击属性/射击设置/基础射击")]
    [LabelText("箭矢速度")]
    [PropertyRange(5f, 30f)]
    [SuffixLabel("m/s")]
    [InfoBox("箭矢的飞行速度")]
    public float arrowSpeed = 15f;
    
    [VerticalGroup("射击属性/射击设置/箭矢系统")]
    [LabelText("最大箭矢数")]
    [PropertyRange(10, 100)]
    [SuffixLabel("支")]
    [InfoBox("射手携带的最大箭矢数量")]
    public int maxArrows = 30;
    
    [VerticalGroup("射击属性/射击设置/箭矢系统")]
    [LabelText("装填时间")]
    [PropertyRange(0.5f, 5f)]
    [SuffixLabel("秒")]
    [InfoBox("箭矢用完后的装填时间")]
    public float reloadTime = 2f;
    
    [TitleGroup("攻击属性")]
    [FoldoutGroup("攻击属性/近战攻击", expanded: true)]
    [LabelText("攻击范围")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("米")]
    [InfoBox("射手近战攻击范围")]
    public float attackRange = 2f;
    
    [FoldoutGroup("攻击属性/近战攻击")]
    [LabelText("攻击高度")]
    [PropertyRange(1f, 5f)]
    [SuffixLabel("米")]
    [InfoBox("射手近战攻击高度")]
    public float attackHeight = 2f;
    
    [FoldoutGroup("攻击属性/近战攻击")]
    [HorizontalGroup("攻击属性/近战攻击/冷却")]
    [VerticalGroup("攻击属性/近战攻击/冷却/左列")]
    [LabelText("基础攻击冷却时间")]
    [PropertyRange(0.1f, 3f)]
    [SuffixLabel("秒")]
    [InfoBox("射手基础攻击的冷却时间")]
    public float attackCooldown = 0.4f;
    
    [VerticalGroup("攻击属性/近战攻击/冷却/右列")]
    [LabelText("击退力度")]
    [PropertyRange(0f, 20f)]
    [SuffixLabel("力")]
    [InfoBox("射手近战攻击的击退力度")]
    public float knockbackForce = 5f;
    
    [TitleGroup("技能冷却")]
    [FoldoutGroup("技能冷却/射击技能", expanded: true)]
    [LabelText("多重射击冷却")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("秒")]
    [InfoBox("多重射击技能的冷却时间")]
    public float multiShotCooldown = 6f;
    
    [FoldoutGroup("技能冷却/射击技能")]
    [LabelText("穿透射击冷却")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("秒")]
    [InfoBox("穿透射击技能的冷却时间")]
    public float piercingShotCooldown = 8f;
    
    [FoldoutGroup("技能冷却/射击技能")]
    [LabelText("快速射击冷却")]
    [PropertyRange(5f, 30f)]
    [SuffixLabel("秒")]
    [InfoBox("快速射击技能的冷却时间")]
    public float rapidFireCooldown = 12f;
    
    [FoldoutGroup("技能冷却/射击技能")]
    [LabelText("爆炸箭冷却")]
    [PropertyRange(5f, 30f)]
    [SuffixLabel("秒")]
    [InfoBox("爆炸箭技能的冷却时间")]
    public float explosiveArrowCooldown = 15f;
    
    [TitleGroup("技能效果")]
    [FoldoutGroup("技能效果/快速射击", expanded: true)]
    [LabelText("快速射击持续时间")]
    [PropertyRange(3f, 15f)]
    [SuffixLabel("秒")]
    [InfoBox("快速射击效果的持续时间")]
    public float rapidFireDuration = 8f;
    
    [FoldoutGroup("技能效果/快速射击")]
    [LabelText("快速射击攻击间隔")]
    [PropertyRange(0.05f, 0.5f)]
    [SuffixLabel("秒")]
    [InfoBox("快速射击模式下的攻击间隔")]
    public float rapidFireAttackDelay = 0.1f;
    
    [FoldoutGroup("技能效果/多重射击")]
    [LabelText("多重射击箭矢数")]
    [PropertyRange(2, 5)]
    [SuffixLabel("支")]
    [InfoBox("多重射击一次发射的箭矢数量")]
    public int multiShotArrowCount = 3;
    
    [FoldoutGroup("技能效果/多重射击")]
    [LabelText("多重射击角度差")]
    [PropertyRange(5f, 30f)]
    [SuffixLabel("度")]
    [InfoBox("多重射击箭矢之间的角度差")]
    public float multiShotAngleDiff = 15f;
    
    [FoldoutGroup("技能效果/伤害倍率")]
    [LabelText("穿透射击伤害倍率")]
    [PropertyRange(1f, 3f)]
    [SuffixLabel("倍")]
    [InfoBox("穿透射击相对于普通攻击的伤害倍率")]
    public float piercingShotDamageMultiplier = 1.3f;
    
    [FoldoutGroup("技能效果/伤害倍率")]
    [LabelText("爆炸箭伤害倍率")]
    [PropertyRange(1f, 3f)]
    [SuffixLabel("倍")]
    [InfoBox("爆炸箭相对于普通攻击的伤害倍率")]
    public float explosiveArrowDamageMultiplier = 1.5f;
    
    [TitleGroup("动画时间")]
    [FoldoutGroup("动画时间/射击动画", expanded: true)]
    [LabelText("普通射击动画时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("普通射击动画的等待时间")]
    public float normalShootAnimationTime = 0.2f;
    
    [FoldoutGroup("动画时间/射击动画")]
    [LabelText("多重射击动画时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("多重射击动画的等待时间")]
    public float multiShotAnimationTime = 0.3f;
    
    [FoldoutGroup("动画时间/攻击恢复")]
    [LabelText("普通攻击恢复时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("普通攻击后的恢复时间")]
    public float normalAttackRecoveryTime = 0.3f;
    
    [FoldoutGroup("动画时间/攻击恢复")]
    [LabelText("多重射击恢复时间")]
    [PropertyRange(0.1f, 1f)]
    [SuffixLabel("秒")]
    [InfoBox("多重射击后的恢复时间")]
    public float multiShotRecoveryTime = 0.4f;
}