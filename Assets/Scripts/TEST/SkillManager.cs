using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class SkillManager : MonoBehaviour
{
    [TabGroup("技能配置")]
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "skillName")]
    [LabelText("可用技能列表")]
    public List<SkillDataTest> availableSkills = new List<SkillDataTest>();
    
    [TabGroup("技能配置")]
    [Required]
    [LabelText("技能释放点")]
    public Transform skillSpawnPoint;
    
    [TabGroup("状态监控")]
    [ProgressBar(0, "maxMana", ColorMember = "GetManaBarColor")]
    [LabelText("当前法力值")]
    public float currentMana = 100f;
    
    [TabGroup("状态监控")]
    [Range(50f, 500f)]
    [LabelText("最大法力值")]
    public float maxMana = 100f;
    
    [TabGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [DictionaryDrawerSettings(KeyLabel = "技能", ValueLabel = "剩余冷却时间")]
    private Dictionary<SkillDataTest, float> skillCooldowns = new Dictionary<SkillDataTest, float>();
    
    private SimplePlayerController playerController;
    
    private Color GetManaBarColor()
    {
        float ratio = currentMana / maxMana;
        if (ratio > 0.6f) return Color.blue;
        if (ratio > 0.3f) return Color.yellow;
        return Color.red;
    }
    
    void Start()
    {
        playerController = GetComponent<SimplePlayerController>();
        
        // 初始化技能冷却时间
        foreach (var skill in availableSkills)
        {
            skillCooldowns[skill] = 0f;
        }
    }
    
    void Update()
    {
        // 更新技能冷却时间
        UpdateCooldowns();
        
        // 检测技能输入
        HandleSkillInput();
        
        // 恢复法力值
        RegenerateMana();
    }
    
    void HandleSkillInput()
    {
        // Q键 - 技能1
        if (Input.GetKeyDown(KeyCode.Q) && availableSkills.Count > 0)
        {
            UseSkill(availableSkills[0]);
        }
        
        // E键 - 技能2
        if (Input.GetKeyDown(KeyCode.E) && availableSkills.Count > 1)
        {
            UseSkill(availableSkills[1]);
        }
    }
    
    public bool UseSkill(SkillDataTest skill)
    {
        // 检查冷却时间和法力值
        if (skillCooldowns[skill] > 0 || currentMana < skill.manaCost)
        {
            Debug.Log($"技能 {skill.skillName} 无法使用：冷却中或法力不足");
            return false;
        }
        
        // 消耗法力值
        currentMana -= skill.manaCost;
        currentMana = Mathf.Max(0, currentMana);
        
        // 设置冷却时间
        skillCooldowns[skill] = skill.cooldown;
        
        // 执行技能效果
        ExecuteSkill(skill);
        
        Debug.Log($"使用技能: {skill.skillName}");
        return true;
    }
    
    void ExecuteSkill(SkillDataTest skill)
    {
        // 生成技能特效
        if (skill.skillEffect != null && skillSpawnPoint != null)
        {
            GameObject effect = Instantiate(skill.skillEffect, skillSpawnPoint.position, skillSpawnPoint.rotation);
            Destroy(effect, 2f); // 2秒后销毁特效
        }
        
        // 播放技能音效
        if (skill.skillSound != null && AudioManagerTest.Instance != null)
        {
            AudioManagerTest.Instance.PlaySound(skill.skillSound);
        }
        
        // 技能伤害检测
        DetectSkillTargets(skill);
    }
    
    void DetectSkillTargets(SkillDataTest skill)
    {
        // 检测技能范围内的敌人
        Collider2D[] targets = Physics2D.OverlapCircleAll(skillSpawnPoint.position, skill.range, LayerMask.GetMask("Enemy"));
        
        foreach (var target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                // 对敌人造成伤害
                var enemy = target.GetComponent<SimpleEnemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(skill.damage);
                    Debug.Log($"技能对 {target.name} 造成 {skill.damage} 点伤害");
                }
            }
        }
    }
    
    void UpdateCooldowns()
    {
        var keys = new List<SkillDataTest>(skillCooldowns.Keys);
        foreach (var skill in keys)
        {
            if (skillCooldowns[skill] > 0)
            {
                skillCooldowns[skill] -= Time.deltaTime;
                skillCooldowns[skill] = Mathf.Max(0, skillCooldowns[skill]);
            }
        }
    }
    
    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += 10f * Time.deltaTime; // 每秒恢复10点法力
            currentMana = Mathf.Min(maxMana, currentMana);
        }
    }
    
    public float GetSkillCooldown(SkillDataTest skill)
    {
        return skillCooldowns.ContainsKey(skill) ? skillCooldowns[skill] : 0f;
    }}
