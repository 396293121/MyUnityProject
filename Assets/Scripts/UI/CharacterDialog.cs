using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// 角色详情对话框
/// 实现Phaser项目中的角色详情显示功能
/// </summary>
public class CharacterDialog : MonoBehaviour
{
    [Header("UI组件")]
    public Image dialogBackground;      // 半透明背景
    public Image dialogPanel;           // 对话框面板
    public Image characterPreview; // 角色预览对象（包含Image和Animator）
    public Text characterNameText;      // 角色名称
    public Text characterDetailsText;   // 角色详细信息
    public Button startButton;          // 开始游戏按钮
    public Button closeButton;          // 关闭按钮
    
    [Header("动画设置")]
    public float animationDuration = 0.3f;
    public Ease showEase = Ease.OutBack;
    public Ease hideEase = Ease.InBack;
    
    [Header("角色预览设置")]
    public float characterPreviewScale = 1.0f; // 角色预览缩放倍数
    public bool preserveAspect = true; // 保持宽高比
    
    [Header("角色资源配置")]
    [SerializeField] private CharacterSpriteConfig[] characterConfigs; // 角色精灵图配置数组
    
    [Header("音效设置")]
    public string showSoundKey = "dialog_open";
    public string hideSoundKey = "dialog_close";
    public string buttonClickSoundKey = "button_click";
    
    // 事件回调
    public System.Action OnStartGame;
    public System.Action OnClose;
    
    // 组件引用
    private CanvasGroup canvasGroup;
    private Animator characterAnimator;
    
    // 状态变量
    private bool isShowing = false;
    private string currentCharacterType;
    
    void Awake()
    {
        // 获取组件引用
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (characterPreview != null)
        {
            characterAnimator = characterPreview.GetComponent<Animator>();
        }
        
        // 设置按钮事件
        SetupButtonEvents();
        
        // 设置初始状态
        SetupInitialState();
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    void SetupButtonEvents()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(() => {
                PlaySound(buttonClickSoundKey);
                OnStartGame?.Invoke();
                HideDialog();
            });
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => {
                PlaySound(buttonClickSoundKey);
                OnClose?.Invoke();
                HideDialog();
            });
        }
        
        // 点击背景关闭对话框
        if (dialogBackground != null)
        {
            var backgroundButton = dialogBackground.GetComponent<Button>();
            if (backgroundButton == null)
            {
                backgroundButton = dialogBackground.gameObject.AddComponent<Button>();
            }
            
            backgroundButton.onClick.AddListener(() => {
                OnClose?.Invoke();
                HideDialog();
            });
        }
    }
    
    /// <summary>
    /// 设置初始状态
    /// </summary>
    void SetupInitialState()
    {
        // 初始隐藏
        gameObject.SetActive(false);
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        if (dialogPanel != null)
        {
            dialogPanel.transform.localScale = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 设置角色信息
    /// </summary>
    public void SetCharacterInfo(string characterType, string name, string details)
    {
        currentCharacterType = characterType;
        
        // 设置角色名称
        if (characterNameText != null)
        {
            characterNameText.text = name;
        }
        
        // 设置角色详细信息
        if (characterDetailsText != null)
        {
            characterDetailsText.text = details;
        }
        
        // 加载角色预览图片
        LoadCharacterPreview(characterType);
        
        Debug.Log($"[CharacterDialog] 设置角色信息: {name} ({characterType})");
    }
    
    /// <summary>
    /// 加载角色预览
    /// </summary>
    void LoadCharacterPreview(string characterType)
    {
        if (characterPreview == null) return;
        
        // 确保角色预览对象激活
        characterPreview.gameObject.SetActive(true);
        
        // 设置Image属性
        characterPreview.preserveAspect = preserveAspect;
        
        // 调整角色预览尺寸
        var rectTransform = characterPreview.rectTransform;
        if (rectTransform != null)
        {
            // 设置合适的缩放比例
            rectTransform.localScale = Vector3.one * characterPreviewScale;
            Debug.Log($"[CharacterDialog] 设置角色预览缩放: {rectTransform.localScale}");
        }
        
        // 获取角色配置
        var config = GetCharacterConfig(characterType);
        
        // 根据配置决定是否播放动画
        bool shouldPlayAnimation = (config != null && config.hasAnimation) || 
                                 (config == null && characterType == "warrior");
        
        if (shouldPlayAnimation && characterAnimator != null)
        {
            // 确保动画控制器处于正常状态
            characterAnimator.enabled = true;

            // 动态切换Animator Controller
            if (config != null && config.animatorController != null)
            {
                characterAnimator.runtimeAnimatorController = config.animatorController;
                Debug.Log($"[CharacterDialog] 动态切换Animator Controller到: {config.animatorController.name}");
            }
            else
            {
                // 如果没有配置特定的Animator Controller，则清空，使用默认的
                characterAnimator.runtimeAnimatorController = null;
                Debug.Log("[CharacterDialog] 清空Animator Controller，使用默认设置");
            }
            
            // 设置初始精灵图（动画会覆盖显示）
            var initialSprite = GetCharacterInitialSprite(characterType, config);
            if (initialSprite != null)
            {
                characterPreview.sprite = initialSprite;
            }
            
            // 播放动画
            string trigger = (config != null && !string.IsNullOrEmpty(config.animationTrigger)) 
                           ? config.animationTrigger : "Walk";
            characterAnimator.SetTrigger(trigger);
            Debug.Log($"[CharacterDialog] 播放{characterType}动画，触发器: {trigger}");
        }
        else
        {
            // 停止动画并显示静态精灵图
            if (characterAnimator != null)
            {
                characterAnimator.enabled = false;
                Debug.Log($"[CharacterDialog] 停止动画播放");
            }
            
            // 获取角色静态精灵图
            var sprite = GetCharacterStaticSprite(characterType);
            if (sprite != null)
            {
                characterPreview.sprite = sprite;
                Debug.Log($"[CharacterDialog] 加载静态精灵图: {sprite.name}");
            }
            else
            {
                Debug.LogWarning($"[CharacterDialog] 未找到角色 {characterType} 的精灵图");
            }
        }
        
        // 应用自定义缩放（如果配置了）
        if (config != null && config.customScale != 1.0f)
        {
            rectTransform.localScale = Vector3.one * config.customScale;
            Debug.Log($"[CharacterDialog] 应用自定义缩放: {config.customScale}");
        }
    }
    
    /// <summary>
    /// 显示对话框
    /// </summary>
    public void ShowDialog()
    {
        if (isShowing) return;
        
        isShowing = true;
        gameObject.SetActive(true);
        
        // 播放显示音效
        PlaySound(showSoundKey);
        
        // 设置初始状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }
        
        if (dialogPanel != null)
        {
            dialogPanel.transform.localScale = Vector3.zero;
        }
        
        // 创建显示动画序列
        var sequence = DOTween.Sequence();
        
        // 背景淡入
        if (canvasGroup != null)
        {
            sequence.Append(canvasGroup.DOFade(1f, animationDuration * 0.5f));
        }
        
        // 对话框弹出
        if (dialogPanel != null)
        {
            sequence.Append(dialogPanel.transform.DOScale(Vector3.one, animationDuration)
                .SetEase(showEase));
        }
        
        // 动画完成后启用交互
        sequence.OnComplete(() => {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
            }
            Debug.Log($"[CharacterDialog] 对话框显示完成");
        });
        
        Debug.Log($"[CharacterDialog] 开始显示对话框");
    }
    
    /// <summary>
    /// 隐藏对话框
    /// </summary>
    public void HideDialog()
    {
        if (!isShowing) return;
        
        isShowing = false;
        
        // 播放隐藏音效
        PlaySound(hideSoundKey);
        
        // 禁用交互
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
        }
        
        // 创建隐藏动画序列
        var sequence = DOTween.Sequence();
        
        // 对话框缩小
        if (dialogPanel != null)
        {
            sequence.Append(dialogPanel.transform.DOScale(Vector3.zero, animationDuration)
                .SetEase(hideEase));
        }
        
        // 背景淡出
        if (canvasGroup != null)
        {
            sequence.Append(canvasGroup.DOFade(0f, animationDuration * 0.5f));
        }
        
        // 动画完成后销毁对象
        sequence.OnComplete(() => {
            gameObject.SetActive(false);
            Destroy(gameObject);
            Debug.Log($"[CharacterDialog] 对话框隐藏完成并销毁");
        });
        
        Debug.Log($"[CharacterDialog] 开始隐藏对话框");
    }
    
    /// <summary>
    /// 更新角色对话框内容
    /// </summary>
    public void UpdateCharacterDialog(string characterType, string name, string details)
    {
        SetCharacterInfo(characterType, name, details);
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    void PlaySound(string soundKey)
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
                AudioManager.Instance.PlaySFX(soundKey, 0.6f);
            }
        }
    }
    
    /// <summary>
    /// 设置开始游戏回调
    /// </summary>
    public void SetStartGameCallback(System.Action callback)
    {
        OnStartGame = callback;
    }
    
    /// <summary>
    /// 设置关闭回调
    /// </summary>
    public void SetCloseCallback(System.Action callback)
    {
        OnClose = callback;
    }
    
    /// <summary>
    /// 检查是否正在显示
    /// </summary>
    public bool IsShowing()
    {
        return isShowing;
    }
    
    /// <summary>
    /// 获取当前角色类型
    /// </summary>
    public string GetCurrentCharacterType()
    {
        return currentCharacterType;
    }
    
    void OnDestroy()
    {
        // 清理DOTween动画
        DOTween.Kill(this);
        
        // 清理按钮事件
        if (startButton != null) startButton.onClick.RemoveAllListeners();
        if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        
        Debug.Log($"[CharacterDialog] 对话框组件销毁");
    }
    
    void OnDisable()
    {
        // 禁用时停止所有动画
        DOTween.Kill(this);
    }
    
    /// <summary>
    /// 获取角色初始精灵图（用于动画开始前显示）
    /// </summary>
    private Sprite GetCharacterInitialSprite(string characterType, CharacterSpriteConfig config)
    {
        // 优先使用配置中的动画第一帧
        if (config != null && config.animationFirstFrame != null)
        {
            return config.animationFirstFrame;
        }
        
        // 其次使用配置中的静态精灵图
        if (config != null && config.staticSprite != null)
        {
            return config.staticSprite;
        }
        
        // 备用：使用路径加载
        if (config != null && !string.IsNullOrEmpty(config.customAnimationPath))
        {
            var sprite = Resources.Load<Sprite>(config.customAnimationPath);
            if (sprite != null) return sprite;
        }
        
        
        return null;
    }
    
    /// <summary>
    /// 获取角色静态精灵图
    /// </summary>
    private Sprite GetCharacterStaticSprite(string characterType)
    {
        var config = GetCharacterConfig(characterType);
        
        // 优先使用编辑器配置的精灵图
        if (config != null && config.staticSprite != null)
        {
            return config.staticSprite;
        }
        
        // 其次使用配置中的自定义路径
        if (config != null && !string.IsNullOrEmpty(config.customStaticPath))
        {
            var sprite = Resources.Load<Sprite>(config.customStaticPath);
            if (sprite != null)
            {
                Debug.Log($"[CharacterDialog] 使用自定义路径加载: {config.customStaticPath}");
                return sprite;
            }
        }
        return null;
      
        
      
    }
    
    /// <summary>
    /// 获取角色配置
    /// </summary>
    private CharacterSpriteConfig GetCharacterConfig(string characterType)
    {
        if (characterConfigs == null) return null;
        
        foreach (var config in characterConfigs)
        {
            if (config != null && string.Equals(config.characterType, characterType, System.StringComparison.OrdinalIgnoreCase))
            {
                return config;
            }
        }
        
        return null;
    }
}

/// <summary>
/// 角色精灵图配置
/// </summary>
[System.Serializable]
public class CharacterSpriteConfig
{
    [Header("角色信息")]
    public string characterType;        // 角色类型（如：warrior, mage, archer, rogue）
    public string displayName;          // 显示名称
    
    [Header("精灵图资源")]
    public Sprite staticSprite;         // 静态精灵图
    public Sprite animationFirstFrame;  // 动画第一帧（可选）
    
    [Header("备用路径配置")]
    public string customStaticPath;     // 自定义静态图路径
    public string customAnimationPath;  // 自定义动画路径
    
    [Header("显示设置")]
    public bool hasAnimation = false;   // 是否有动画
    public string animationTrigger = "Walk"; // 动画触发器名称
    public float customScale = 1.0f;    // 自定义缩放（如果需要特殊缩放）
    public RuntimeAnimatorController animatorController; // 角色动画控制器（可选）
}