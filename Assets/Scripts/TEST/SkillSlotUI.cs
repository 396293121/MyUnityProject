using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SkillSlotUI : MonoBehaviour
{
    [BoxGroup("UI组件配置")]
    [LabelText("技能图标")]
    public Image skillIcon;
    
    [BoxGroup("UI组件配置")]
    [Required]
    [LabelText("冷却遮罩")]
    public Image cooldownOverlay;
    
    [BoxGroup("UI组件配置")]
    [LabelText("冷却时间文本")]
    public Text cooldownText;
    
    [BoxGroup("UI组件配置")]
    [LabelText("按键绑定文本")]
    public Text keyBindText;
    
    [BoxGroup("状态信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("当前技能")]
    private SkillDataTest currentSkill;
    
    public void SetSkill(SkillDataTest skill)
    {
        currentSkill = skill;
        
        if (skill.skillIcon != null)
        {
            skillIcon.sprite = skill.skillIcon;
        }
    }
    
    public void UpdateCooldown(float currentCooldown, float maxCooldown)
    {
        if (cooldownOverlay != null)
        {
            if (currentCooldown > 0)
            {
                cooldownOverlay.gameObject.SetActive(true);
                cooldownOverlay.fillAmount = currentCooldown / maxCooldown;
                
                if (cooldownText != null)
                {
                    cooldownText.text = currentCooldown.ToString("F1");
                }
            }
            else
            {
                cooldownOverlay.gameObject.SetActive(false);
                
                if (cooldownText != null)
                {
                    cooldownText.text = "";
                }
            }
        }
    }
}