using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 角色选择项组件
/// 实现Phaser项目中的角色选择交互效果
/// </summary>
public class CharacterOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("角色配置")]
    public string characterType = "warrior";
    public string characterName = "战士";
    [TextArea(2, 4)]
    public string shortDescription = "近战职业，拥有强大的攻击力和防御力";
    [TextArea(3, 6)]
    public string detailedDescription = "战士是游戏中的近战职业，拥有强大的攻击力和防御力。擅长使用各种武器进行近距离战斗，是团队中的主要输出和肉盾。";
    
    [Header("UI组件")]
    public Image characterImage;        // 角色图片
    public Image characterText;         // 角色文字图片
    public Text descriptionText;        // 描述文字
    public CanvasGroup canvasGroup;     // 用于透明度控制
    
    [Header("动画设置")]
    public float hoverScale = 1.1f;     // 悬停时的缩放
    public float normalScale = 1f;    // 正常状态的缩放
    public float selectedScale = 1.1f;  // 选中状态的缩放
    public float animationDuration = 0.2f; // 动画持续时间
    
    [Header("音效设置")]
    public string hoverSoundKey = "button_select";
    public string clickSoundKey = "button_click";
    
    // 组件引用
    private CharacterBreathing breathing;
    private CharacterSelectUIEnhanced parentUI;
    private Button button;
    
    // 状态变量
    private bool isSelected = false;
    private bool isHovering = false;
    private Vector3 originalScale;
    
    void Awake()
    {
        // 获取组件引用
        breathing = GetComponent<CharacterBreathing>();
        button = GetComponent<Button>();
        parentUI = GetComponentInParent<CharacterSelectUIEnhanced>();
        // 如果没有CanvasGroup，自动添加
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    
    void Start()
    {
        // 记录原始缩放
        originalScale = transform.localScale;
        // 设置初始状态
        SetupInitialState();
        
        Debug.Log($"[CharacterOption] {characterName} 初始化完成");
    }
    
    /// <summary>
    /// 设置初始状态
    /// </summary>
    void SetupInitialState()
    {
        // 设置初始缩放
        transform.localScale = originalScale * normalScale;
        
        // 设置描述文字初始透明度
        if (descriptionText != null)
        {
            var color = descriptionText.color;
            color.a = 0f;
            descriptionText.color = color;
            descriptionText.text = shortDescription;
        }
        
        // 确保呼吸动画开始
        if (breathing != null)
        {
            breathing.StartBreathing();
        }
    }
    
    #region 鼠标事件处理
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelected) return;
        
        isHovering = true;
        
        // 暂停呼吸动画
        if (breathing != null)
        {
            breathing.PauseBreathing();
        }
        
        // 放大角色
        transform.DOScale(originalScale * hoverScale, animationDuration)
            .SetEase(Ease.OutQuart);
        
        Debug.Log($"[CharacterOption] {originalScale} 鼠标悬停前 {hoverScale}");
        // 显示描述文字
        if (descriptionText != null)
        {
            descriptionText.DOFade(1f, animationDuration);
        }
        
        // 播放悬停音效
        PlaySound(hoverSoundKey);
        
        Debug.Log($"[CharacterOption] {characterName} 鼠标悬停");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected) return;
        
        isHovering = false;
        
        // 恢复原始大小
        transform.DOScale(originalScale * normalScale, animationDuration)
            .SetEase(Ease.OutQuart);
        
        // 隐藏描述文字
        if (descriptionText != null)
        {
            descriptionText.DOFade(0f, animationDuration);
        }
        
        // 恢复呼吸动画
        if (breathing != null)
        {
            breathing.ResumeBreathing();
        }
        
        Debug.Log($"[CharacterOption] {characterName} 鼠标离开");
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 播放点击音效
        PlaySound(clickSoundKey);
        
        // 通知父UI选择了这个角色
        if (parentUI != null)
        {
            parentUI.SelectCharacter(this);
        }
        
        Debug.Log($"[CharacterOption] {characterName} 被点击选择");
    }
    
    #endregion
    
    #region 状态控制
    
    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selected)
        {
            // 选中状态：停止呼吸动画，保持放大
            if (breathing != null)
            {
                breathing.StopBreathing();
            }
            transform.DOScale(originalScale * selectedScale, animationDuration)
                .SetEase(Ease.OutQuart);
            
            // 隐藏简短描述（因为会显示详细对话框）
            if (descriptionText != null)
            {
                descriptionText.DOFade(0f, animationDuration);
            }
            
            Debug.Log($"[CharacterOption] {characterName} 被选中");
        }
        else
        {
            // 未选中状态：恢复呼吸动画
            transform.DOScale(originalScale * normalScale, animationDuration)
                .SetEase(Ease.OutQuart);
            
            if (breathing != null)
            {
                breathing.StartBreathing();
            }
            
            // 如果鼠标仍在悬停，显示描述
            if (isHovering && descriptionText != null)
            {
                descriptionText.DOFade(1f, animationDuration);
            }
            
            Debug.Log($"[CharacterOption] {characterName} 取消选中");
        }
    }
    
    /// <summary>
    /// 设置交互状态
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = interactable ? 1f : 0.5f;
        }
    }
    
    /// <summary>
    /// 重置到初始状态
    /// </summary>
    public void ResetToInitialState()
    {
        isSelected = false;
        isHovering = false;
        
        // 重置缩放
        transform.DOScale(originalScale * normalScale, animationDuration);
        
        // 隐藏描述文字
        if (descriptionText != null)
        {
            descriptionText.DOFade(0f, animationDuration);
        }
        
        // 重新开始呼吸动画
        if (breathing != null)
        {
            breathing.StartBreathing();
        }
        
        Debug.Log($"[CharacterOption] {characterName} 重置到初始状态");
    }
    
    #endregion
    
    #region 音效播放
    
    /// <summary>
    /// 播放音效
    /// </summary>
    private void PlaySound(string soundKey)
    {
        if (string.IsNullOrEmpty(soundKey)) return;
        
        if (AudioManager.Instance != null)
        {
            var audioConfig = ConfigManager.Instance?.GetAudioConfig(soundKey);
            if (audioConfig != null)
            {
                AudioManager.Instance.PlaySFX(soundKey, audioConfig.volume);
            }
            else
            {
                AudioManager.Instance.PlaySFX(soundKey, 0.5f);
            }
        }
    }
    
    #endregion
    
    #region 公共接口
    
    /// <summary>
    /// 获取角色类型
    /// </summary>
    public string GetCharacterType()
    {
        return characterType;
    }
    
    /// <summary>
    /// 获取角色名称
    /// </summary>
    public string GetCharacterName()
    {
        return characterName;
    }
    
    /// <summary>
    /// 获取详细描述
    /// </summary>
    public string GetDetailedDescription()
    {
        return detailedDescription;
    }
    
    /// <summary>
    /// 设置角色信息
    /// </summary>
    public void SetCharacterInfo(string type, string name, string shortDesc, string detailedDesc)
    {
        characterType = type;
        characterName = name;
        shortDescription = shortDesc;
        detailedDescription = detailedDesc;
        
        if (descriptionText != null)
        {
            descriptionText.text = shortDescription;
        }
    }
    
    #endregion
    
    void OnDestroy()
    {
        // 清理DOTween动画
        transform.DOKill();
        if (descriptionText != null)
        {
            descriptionText.DOKill();
        }
    }
}