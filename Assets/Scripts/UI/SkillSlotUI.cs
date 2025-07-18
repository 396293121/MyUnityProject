using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 技能槽位UI组件 - 基于HUD设计提示词的菱形金属框设计
/// 支持技能图标、冷却显示、状态指示等功能
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class SkillSlotUI : MonoBehaviour
{
    [TitleGroup("技能槽位配置", "菱形金属框技能槽位", TitleAlignments.Centered)]
    
    // [TabGroup("UI元素")]
    // [BoxGroup("UI元素/基础组件")]
    // [LabelText("槽位背景")]
    // [Required]
    // public Image slotBackground;
    [BoxGroup("UI元素")]
    [LabelText("技能图标")]
    [Required]
    public Image skillIcon;
    
    [BoxGroup("UI元素")]
    [LabelText("边框图像")]
    public Image borderImage;
    [BoxGroup("UI元素")]
    [LabelText("冷却遮罩")]
    public Image cooldownMask;
    [BoxGroup("UI元素")]
    [LabelText("技能名称")]
    public Text skillName;
    [BoxGroup("UI元素")]
    [LabelText("冷却文本")]
    public TextMeshProUGUI cooldownText;
    
    [BoxGroup("UI元素")]
    [LabelText("快捷键文本")]
    public TextMeshProUGUI hotkeyText;




    // [TabGroup("配置")]
    // [BoxGroup("配置/颜色设置")]
    // [LabelText("常态边框色")]
    // public Color normalBorderColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [BoxGroup("配置")]
    [LabelText("悬停缩放")]
    [PropertyRange(1f, 1.5f)]
    public float hoverScale = 1.1f;
    
    [FoldoutGroup("状态", expanded: true)]
    [LabelText("技能索引")]
    [ReadOnly]
    [ShowInInspector]
    private int skillIndex = -1;
    
    [FoldoutGroup("状态")]
    [LabelText("技能组件引用")]
    [ReadOnly]
    [ShowInInspector]
    private SkillComponent skillComponent;
    
    [FoldoutGroup("状态")]
    [LabelText("是否悬停")]
    [ReadOnly]
    [ShowInInspector]
    private bool isHovered = false;
    
    [FoldoutGroup("状态")]
    [LabelText("是否选中")]
    [ReadOnly]
    [ShowInInspector]
    private bool isSelected = false;
    
    [FoldoutGroup("状态")]
    [LabelText("原始缩放")]
    [ReadOnly]
    [ShowInInspector]
    private Vector3 originalScale;
    
    private RectTransform rectTransform;
    private Button button;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        
        originalScale = transform.localScale;
        
        // 设置按钮事件
        button.onClick.AddListener(OnSkillSlotClicked);
        
        // 添加悬停事件
        var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // 鼠标进入事件
        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((data) => OnPointerEnter());
        eventTrigger.triggers.Add(pointerEnter);
        
        // 鼠标离开事件
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => OnPointerExit());
        eventTrigger.triggers.Add(pointerExit);
    }
    
    /// <summary>
    /// 初始化技能槽位
    /// </summary>
    /// <param name="index">技能索引</param>
    /// <param name="component">技能组件</param>
    public void Initialize(int index, SkillComponent component)
    {
        skillIndex = index;
        skillComponent = component;
        
        // 设置快捷键文本
        if (hotkeyText != null)
        {
            hotkeyText.text = (index + 1).ToString();
        }
        
        // 获取技能信息并设置图标
        var skillInfo = skillComponent.GetSkillInfo(index);
        if (skillInfo != null && skillIcon != null)
        {
            skillIcon.sprite = skillInfo.skillIcon;
        }
        skillName.text = skillInfo.skillName;
         // 注册事件监听
        component.OnSkillCooldownUpdated += OnSkillUpdated;
        component.OnSkillAvailabilityChanged += OnSkillUpdated;
        Debug.Log($"[SkillSlotUI] 初始化技能槽位 {index}");
    }
      private void OnDestroy()
    {
        if (skillComponent != null)
        {
            skillComponent.OnSkillCooldownUpdated -= OnSkillUpdated;
            skillComponent.OnSkillAvailabilityChanged -= OnSkillUpdated;
        }
    }
    
    private void OnSkillUpdated(int updatedIndex)
    {
        if (skillIndex == updatedIndex)
        {
            UpdateDisplay();
        }
    }
    /// <summary>
    /// 更新显示
    /// </summary>
    public void UpdateDisplay()
    {
        if (skillComponent == null || skillIndex < 0) return;

        var skillInfo = skillComponent.GetSkillInfo(skillIndex);
        if (skillInfo == null) return;

        // 更新冷却显示
        UpdateCooldownDisplay(skillInfo);

        // 更新状态显示
        UpdateStatusDisplay(skillInfo);

        // 更新边框颜色
        // UpdateBorderColor(skillInfo);

        // 更新特效
        // UpdateEffects(skillInfo);
    }
    
    /// <summary>
    /// 更新冷却显示
    /// </summary>
    private void UpdateCooldownDisplay(SkillInfo skillInfo)
    {
        if (cooldownMask != null)
        {
            if (skillInfo.remainingCooldown > 0)
            {
                float cooldownPercent = skillInfo.remainingCooldown / skillInfo.cooldown;
                cooldownMask.fillAmount = cooldownPercent;
                cooldownMask.gameObject.SetActive(true);
            }
            else
            {
                cooldownMask.gameObject.SetActive(false);
            }
        }
        
        if (cooldownText != null)
        {
            if (skillInfo.remainingCooldown > 0)
            {
                cooldownText.text = Mathf.Ceil(skillInfo.remainingCooldown).ToString();
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 更新状态显示
    /// </summary>
    private void UpdateStatusDisplay(SkillInfo skillInfo)
    {
        // 更新按钮交互状态
        if (button != null)
        {
            button.interactable = skillInfo.canUse;
        }
        
     
    }
    



    
 

    /// <summary>
    /// 鼠标进入事件
    /// </summary>
    private void OnPointerEnter()
    {
        isHovered = true;
        
        // 悬停缩放效果
        transform.localScale = originalScale * hoverScale;
        
        // 悬停时升高5像素
        if (rectTransform != null)
        {
            Vector3 pos = rectTransform.anchoredPosition;
            pos.y += 5f;
            rectTransform.anchoredPosition = pos;
        }
        
        // 播放悬停音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ui_hover", 0.5f);
        }
    }
    
    /// <summary>
    /// 鼠标离开事件
    /// </summary>
    private void OnPointerExit()
    {
        isHovered = false;
        
        // 恢复原始缩放
        transform.localScale = originalScale;
        
        // 恢复原始位置
        if (rectTransform != null)
        {
            Vector3 pos = rectTransform.anchoredPosition;
            pos.y -= 5f;
            rectTransform.anchoredPosition = pos;
        }
    }
    
    /// <summary>
    /// 技能槽位点击事件
    /// </summary>
    private void OnSkillSlotClicked()
    {
        if (skillComponent != null && skillIndex >= 0)
        {
            // 尝试使用技能
            skillComponent.TryUseSkill(skillIndex);
            
            // 播放点击音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ui_click", 0.7f);
            }
            
    
        }
    }
    
    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            // 选中时放大110%并轻微浮动
            transform.localScale = originalScale * 1.1f;
            StartCoroutine(FloatAnimation());
        }
        else
        {
            // 取消选中时恢复原始状态
            transform.localScale = originalScale;
            StopCoroutine(FloatAnimation());
        }
    }
    
    /// <summary>
    /// 浮动动画
    /// </summary>
    private System.Collections.IEnumerator FloatAnimation()
    {
        Vector3 originalPos = rectTransform.anchoredPosition;
        
        while (isSelected)
        {
            float time = Time.time * 2f;
            Vector3 pos = originalPos;
            pos.y += Mathf.Sin(time) * 2f;
            rectTransform.anchoredPosition = pos;
            yield return null;
        }
        
        rectTransform.anchoredPosition = originalPos;
    }
    
    /// <summary>
    /// 获取技能信息
    /// </summary>
    public SkillInfo GetSkillInfo()
    {
        if (skillComponent != null && skillIndex >= 0)
        {
            return skillComponent.GetSkillInfo(skillIndex);
        }
        return null;
    }
}

