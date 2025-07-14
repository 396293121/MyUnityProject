using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 技能示例配置类 - 展示如何创建不同类型的技能
/// 这个类仅用于演示，实际技能应该通过ScriptableObject资产创建
/// </summary>
public class SkillExamples : MonoBehaviour
{
    [TabGroup("技能示例")]
    [InfoBox("以下是不同类型技能的配置示例，可以参考这些设置来创建技能资产")]
    [Button("创建战士基础攻击技能")]
    private void CreateWarriorBasicAttack()
    {
        Debug.Log("=== 战士基础攻击技能配置示例 ===");
        Debug.Log("技能类型: 单体攻击");
        Debug.Log("目标类型: 敌人");
        Debug.Log("伤害值: 50");
        Debug.Log("冷却时间: 2秒");
        Debug.Log("法力消耗: 10");
        Debug.Log("技能范围: 3");
        Debug.Log("动画触发器: warrior_attack1");
        Debug.Log("动画持续时间: 0.8秒");
    }
    
    [TabGroup("技能示例")]
    [Button("创建战士旋风斩技能")]
    private void CreateWarriorWhirlwind()
    {
        Debug.Log("=== 战士旋风斩技能配置示例 ===");
        Debug.Log("技能类型: 范围伤害");
        Debug.Log("目标类型: 敌人");
        Debug.Log("伤害值: 80");
        Debug.Log("冷却时间: 8秒");
        Debug.Log("法力消耗: 40");
        Debug.Log("技能范围: 5");
        Debug.Log("AOE形状: 圆形");
        Debug.Log("动画触发器: warrior_whirlwind");
        Debug.Log("动画持续时间: 1.5秒");
    }
    
    [TabGroup("技能示例")]
    [Button("创建战士狂暴BUFF技能")]
    private void CreateWarriorBerserk()
    {
        Debug.Log("=== 战士狂暴BUFF技能配置示例 ===");
        Debug.Log("技能类型: 增益BUFF");
        Debug.Log("目标类型: 自己");
        Debug.Log("冷却时间: 30秒");
        Debug.Log("法力消耗: 50");
        Debug.Log("BUFF持续时间: 15秒");
        Debug.Log("攻击力加成: 30");
        Debug.Log("移动速度加成: 0.3");
        Debug.Log("动画触发器: warrior_berserk");
        Debug.Log("动画持续时间: 1.0秒");
    }
    
    [TabGroup("技能示例")]
    [Button("创建治疗药水技能")]
    private void CreateHealingPotion()
    {
        Debug.Log("=== 治疗药水技能配置示例 ===");
        Debug.Log("技能类型: 治疗");
        Debug.Log("目标类型: 自己");
        Debug.Log("治疗量: 100");
        Debug.Log("冷却时间: 10秒");
        Debug.Log("法力消耗: 20");
        Debug.Log("动画触发器: use_potion");
        Debug.Log("动画持续时间: 0.5秒");
    }
    
    [TabGroup("技能示例")]
    [Button("创建召唤守卫技能")]
    private void CreateSummonGuard()
    {
        Debug.Log("=== 召唤守卫技能配置示例 ===");
        Debug.Log("技能类型: 召唤");
        Debug.Log("冷却时间: 60秒");
        Debug.Log("法力消耗: 80");
        Debug.Log("召唤物存在时间: 45秒");
        Debug.Log("需要设置: 召唤物预制体");
        Debug.Log("动画触发器: summon_guard");
        Debug.Log("动画持续时间: 2.0秒");
    }
    
    [TabGroup("技能示例")]
    [Button("创建矩形AOE技能")]
    private void CreateRectangleAOE()
    {
        Debug.Log("=== 矩形AOE技能配置示例 ===");
        Debug.Log("技能类型: 范围伤害");
        Debug.Log("目标类型: 敌人");
        Debug.Log("伤害值: 60");
        Debug.Log("冷却时间: 6秒");
        Debug.Log("法力消耗: 35");
        Debug.Log("AOE形状: 矩形");
        Debug.Log("矩形AOE宽度: 8");
        Debug.Log("矩形AOE高度: 3");
        Debug.Log("动画触发器: warrior_slash_wave");
        Debug.Log("动画持续时间: 1.2秒");
    }
    
    [TabGroup("使用说明")]
    [InfoBox("技能配置步骤:\n" +
             "1. 在Project窗口右键 -> Create -> Skills -> Skill Data\n" +
             "2. 根据上述示例配置技能参数\n" +
             "3. 将技能添加到SkillManager的可用技能列表中\n" +
             "4. 在Animator Controller中添加对应的动画状态和触发器\n" +
             "5. 测试技能效果", InfoMessageType.Info)]
    [Button("打印技能系统架构说明")]
    private void PrintSkillSystemArchitecture()
    {
        Debug.Log("=== 技能系统架构说明 ===");
        Debug.Log("1. skillDataConfig: 技能数据配置，包含所有技能参数和效果逻辑");
        Debug.Log("2. SkillManager: 技能管理器，处理技能输入、冷却、法力消耗等");
        Debug.Log("3. BuffManager: BUFF管理器，处理增益效果的应用和移除");
        Debug.Log("4. 技能类型支持: 单体攻击、范围伤害、BUFF、治疗、召唤");
        Debug.Log("5. AOE形状支持: 圆形、矩形");
        Debug.Log("6. 目标类型支持: 敌人、自己、友军、所有单位");
        Debug.Log("7. 动画集成: 支持技能动画播放和时机控制");
        Debug.Log("8. 特效和音效: 支持技能特效和音效播放");
    }
    
    [TabGroup("调试工具")]
    [InfoBox("调试功能可以帮助测试技能效果")]
    [Button("测试所有技能类型")]
    private void TestAllSkillTypes()
    {
        var skillManager = FindObjectOfType<SkillManager>();
        if (skillManager == null)
        {
            Debug.LogError("未找到SkillManager组件！");
            return;
        }
        
        Debug.Log("=== 技能系统测试 ===");
        Debug.Log($"当前可用技能数量: {skillManager.availableSkills.Count}");
        Debug.Log($"当前法力值: {skillManager.currentMana}/{skillManager.maxMana}");
        
        for (int i = 0; i < skillManager.availableSkills.Count; i++)
        {
            var skill = skillManager.availableSkills[i];
            Debug.Log($"技能{i + 1}: {skill.skillName} - 类型: {skill.skillType} - 冷却: {skillManager.GetSkillCooldown(skill):F1}秒");
        }
    }
}