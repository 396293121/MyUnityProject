using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class HUDManager : MonoBehaviour
{
    [TabGroup("生命值UI")]
    [Required]
    [LabelText("生命值滑动条")]
    public Slider healthBar;
    
    [TabGroup("生命值UI")]
    [LabelText("生命值文本")]
    public Text healthText;
    
    [TabGroup("法力值UI")]
    [Required]
    [LabelText("法力值滑动条")]
    public Slider manaBar;
    
    [TabGroup("法力值UI")]
    [LabelText("法力值文本")]
    public Text manaText;
    
    [TabGroup("技能UI")]
    [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 6)]
    [LabelText("技能槽列表")]
    public List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();
    
    [TabGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("玩家控制器")]
    private SimplePlayerController playerController;
    
    [TabGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("技能管理器")]
    private SkillManager skillManager;
    void Start()
    {
        // 查找玩家组件
        playerController = FindObjectOfType<SimplePlayerController>();
        skillManager = FindObjectOfType<SkillManager>();
        
        // 初始化UI
        UpdateHealthUI();
        UpdateManaUI();
        InitializeSkillUI();
    }
    
    void Update()
    {
        UpdateHealthUI();
        UpdateManaUI();
        UpdateSkillUI();
    }
    
    void UpdateHealthUI()
    {
        if (playerController != null && healthBar != null)
        {
            float healthPercent = playerController.GetCurrentHealth() / playerController.GetMaxHealth();
            healthBar.value = healthPercent;
            
            if (healthText != null)
            {
                healthText.text = $"{playerController.GetCurrentHealth():F0}/{playerController.GetMaxHealth():F0}";
            }
        }
    }
    
    void UpdateManaUI()
    {
        if (skillManager != null && manaBar != null)
        {
            float manaPercent = skillManager.currentMana / skillManager.maxMana;
            manaBar.value = manaPercent;
            
            if (manaText != null)
            {
                manaText.text = $"{skillManager.currentMana:F0}/{skillManager.maxMana:F0}";
            }
        }
    }
    
    void InitializeSkillUI()
    {
        if (skillManager != null)
        {
            for (int i = 0; i < skillSlots.Count && i < skillManager.availableSkills.Count; i++)
            {
                skillSlots[i].SetSkill(skillManager.availableSkills[i]);
            }
        }
    }
    
    void UpdateSkillUI()
    {
        if (skillManager != null)
        {
            for (int i = 0; i < skillSlots.Count && i < skillManager.availableSkills.Count; i++)
            {
                var skill = skillManager.availableSkills[i];
                float cooldown = skillManager.GetSkillCooldown(skill);
                skillSlots[i].UpdateCooldown(cooldown, skill.cooldown);
            }
        }
    }
}