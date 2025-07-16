using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 战士技能数据配置
/// 基于skillDataConfig结构，为战士职业定制的技能数据
/// </summary>
[CreateAssetMenu(fileName = "WarriorSkillData", menuName = "Skills/Warrior Skill Data")]
public class WarriorSkillData : skillDataConfig
{
    [TitleGroup("战士技能配置", "战士职业专用技能数据", TitleAlignments.Centered)]
    [InfoBox("此配置继承自skillDataConfig，包含战士职业的所有技能设置")]

    [FoldoutGroup("战士技能配置/技能预设", expanded: true)]
    [Button("设置为重斩技能")]
    [GUIColor(1f, 0.4f, 0.4f)]
    private void SetupHeavySlash()
    {
        skillName = "重斩";
        description = "造成150%攻击力的强力一击，并击退敌人";
        damage = 0; // 将通过伤害倍率计算
        cooldown = 5f;
        manaCost = 20;
        range = 3f;

        skillType = SkillTypeTest.SingleTargetBox;
        targetType = TargetType.Enemy;

        // 矩形攻击参数
        // boxWidth = 3f;
        // boxLength = 2.5f;

        // 动画配置
        animationTrigger = "HeavySlash";
        animationDuration = 1.2f;

        Debug.Log("已设置为重斩技能配置");
    }

    [FoldoutGroup("战士技能配置/技能预设")]
    [Button("设置为旋风斩技能")]
    [GUIColor(0.4f, 1f, 0.4f)]
    private void SetupWhirlwind()
    {
        skillName = "旋风斩";
        description = "对周围敌人造成120%攻击力的范围伤害";
        damage = 0; // 将通过伤害倍率计算
        cooldown = 8f;
        manaCost = 30;
        range = 2.5f;

        skillType = SkillTypeTest.AreaOfEffect;
        targetType = TargetType.Enemy;

        // AOE参数
        isCircularAOE = true;
        range = 2.5f; // 使用range作为AOE半径

        // 动画配置
        animationTrigger = "Whirlwind";
        animationDuration = 1.5f;

        Debug.Log("已设置为旋风斩技能配置");
    }

    [FoldoutGroup("战士技能配置/技能预设")]
    [Button("设置为战吼技能")]
    [GUIColor(1f, 1f, 0.4f)]
    private void SetupBattleCry()
    {
        skillName = "战吼";
        description = "提高攻击力和防御力，持续10秒";
        damage = 0;
        cooldown = 15f;
        manaCost = 25;
        range = 0f; // 自身BUFF

        skillType = SkillTypeTest.Buff;
        targetType = TargetType.Self;

        // BUFF配置
        buffDuration = 10f;
        attackBonus = 0.5f; // 50%攻击力提升
        speedBonus = 0f; // 不影响速度

        // 动画配置
        animationTrigger = "BattleCry";
        animationDuration = 1f;

        Debug.Log("已设置为战吼技能配置");
    }

}