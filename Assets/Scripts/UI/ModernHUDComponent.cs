using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 现代化HUD组件 - 集成玩家控制器、技能系统和HUD设计提示词
/// 基于银河战士恶魔城2D动作冒险游戏风格，金属与魔法结合
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class ModernHUDComponent : MonoBehaviour
{
    [TitleGroup("HUD系统配置", "现代化HUD界面组件", TitleAlignments.Centered)]
    
    [TabGroup("配置", "核心引用")]
    [BoxGroup("配置/核心引用/角色引用")]
    [LabelText("玩家控制器")]
    [Required("必须指定玩家控制器")]
    public PlayerController playerController;
    
    [BoxGroup("配置/核心引用/角色引用")]
    [LabelText("技能组件")]
    [Required("必须指定技能组件")]
    public SkillComponent skillComponent;
    
    [BoxGroup("配置/核心引用/角色引用")]
    [LabelText("角色引用")]
    [Required("必须指定角色引用")]
    public Character character;
    

    
    [TabGroup("配置", "UI元素")]
    [BoxGroup("配置/UI元素/生命值系统")]
    [LabelText("生命值条背景")]
    public Image healthBarBackground;
    
    [BoxGroup("配置/UI元素/生命值系统")]
    [LabelText("生命值条填充")]
    public Image healthBarFill;
    
    [BoxGroup("配置/UI元素/生命值系统")]
    [LabelText("生命值文本")]
    public Text healthText;
    
    [BoxGroup("配置/UI元素/生命值系统")]
    [LabelText("生命值低血量特效")]
    public ParticleSystem lowHealthEffect;
    
    [BoxGroup("配置/UI元素/魔力系统")]
    [LabelText("魔力值条背景")]
    public Image manaBarBackground;
    
    [BoxGroup("配置/UI元素/魔力系统")]
    [LabelText("魔力值条填充")]
    public Image manaBarFill;
    
    [BoxGroup("配置/UI元素/魔力系统")]
    [LabelText("魔力值文本")]
    public Text manaText;
    
    [BoxGroup("配置/UI元素/魔力系统")]
    [LabelText("魔力充能特效")]
    public ParticleSystem manaChargeEffect;
    
    [BoxGroup("配置/UI元素/经验系统")]
    [LabelText("经验值条背景")]
    public Image expBarBackground;
    
    [BoxGroup("配置/UI元素/经验系统")]
    [LabelText("经验值条填充")]
    public Image expBarFill;
    
    [BoxGroup("配置/UI元素/经验系统")]
    [LabelText("经验值文本")]
    public Text expText;
    
    [BoxGroup("配置/UI元素/经验系统")]
    [LabelText("等级文本")]
    public Text levelText;
    
    [BoxGroup("配置/UI元素/经验系统")]
    [LabelText("升级特效")]
    public ParticleSystem levelUpEffect;
    
    [BoxGroup("配置/UI元素/技能系统")]
    [LabelText("技能槽位容器")]
    public Transform skillSlotsContainer;
    
    [BoxGroup("配置/UI元素/技能系统")]
    [LabelText("技能槽位预制体")]
    public GameObject skillSlotPrefab;
    
    [BoxGroup("配置/UI元素/状态系统")]
    [LabelText("状态图标容器")]
    public Transform statusIconsContainer;
    
    // [BoxGroup("配置/UI元素/状态系统")]
    // [LabelText("状态图标预制体")]
    // public GameObject statusIconPrefab;
    
    // [BoxGroup("配置/UI元素/小地图系统")]
    // [LabelText("小地图容器")]
    // public RectTransform minimapContainer;
    
    [BoxGroup("配置/UI元素/小地图系统")]
    [LabelText("小地图相机")]
    public Camera minimapCamera;
    
    [TabGroup("状态", "运行时数据")]
    [FoldoutGroup("状态/运行时数据/技能槽位", expanded: true)]
    [LabelText("技能槽位列表")]
    [ReadOnly]
    [ShowInInspector]
    private List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();
    
    // [FoldoutGroup("状态/运行时数据/状态图标", expanded: false)]
    // [LabelText("状态图标列表")]
    // [ReadOnly]
    // [ShowInInspector]
    // private List<StatusIconUI> statusIcons = new List<StatusIconUI>();
    
    [FoldoutGroup("状态/运行时数据/动画状态", expanded: false)]
    [LabelText("生命值动画协程")]
    [ReadOnly]
    [ShowInInspector]
    private Coroutine healthAnimCoroutine;
    
    [FoldoutGroup("状态/运行时数据/动画状态")]
    [LabelText("魔力值动画协程")]
    [ReadOnly]
    [ShowInInspector]
    private Coroutine manaAnimCoroutine;
    
    [FoldoutGroup("状态/运行时数据/动画状态")]
    [LabelText("经验值动画协程")]
    [ReadOnly]
    [ShowInInspector]
    private Coroutine expAnimCoroutine;
    
    [TabGroup("状态", "缓存数据")]
    [FoldoutGroup("状态/缓存数据/数值缓存", expanded: false)]
    [LabelText("上次生命值")]
    [ReadOnly]
    [ShowInInspector]
    private float lastHealth = -1f;
    
    [FoldoutGroup("状态/缓存数据/数值缓存")]
    [LabelText("上次魔力值")]
    [ReadOnly]
    [ShowInInspector]
    private float lastMana = -1f;
    
    [FoldoutGroup("状态/缓存数据/数值缓存")]
    [LabelText("上次经验值")]
    [ReadOnly]
    [ShowInInspector]
    private float lastExp = -1f;
    
    [FoldoutGroup("状态/缓存数据/数值缓存")]
    [LabelText("上次等级")]
    [ReadOnly]
    [ShowInInspector]
    private int lastLevel = -1;
    private static ModernHUDComponent _instance;
public static ModernHUDComponent Instance => _instance;
    private void Awake()
    {
        // 单例初始化
    if (_instance != null && _instance != this)
    {
        Destroy(gameObject);
        return;
    }
    _instance = this;
    DontDestroyOnLoad(gameObject); // 跨场景保留

        // 自动获取组件引用
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        
        if (skillComponent == null)
            skillComponent = FindObjectOfType<SkillComponent>();
        
        if (character == null && playerController != null)
            character = playerController.playerCharacter;
        
        // 验证必要组件
        ValidateComponents();
    }
    
    private void Start()
    {
        // 初始化HUD
        InitializeHUD();
        
        // 注册事件监听
        RegisterEventListeners();
        
        // 初始化技能槽位
        InitializeSkillSlots();
        
        // 强制更新一次显示
        ForceUpdateDisplay();
    }
    
    private void Update()
    {
        
        // 更新技能槽位
        // UpdateSkillSlots();
        
        // 更新状态图标
      //  UpdateStatusIcons();
    }

    private void OnDestroy()
    {
        // 注销事件监听
        UnregisterEventListeners();
         character.OnHealthChanged -= UpdateHealthDisplay;
    character.OnManaChanged -= UpdateManaDisplay;
    }
    
    /// <summary>
    /// 验证必要组件
    /// </summary>
    private void ValidateComponents()
    {
        if (playerController == null)
        {
            Debug.LogError("[ModernHUDComponent] 未找到PlayerController组件！");
            enabled = false;
            return;
        }
        
        if (skillComponent == null)
        {
            Debug.LogError("[ModernHUDComponent] 未找到SkillComponent组件！");
            enabled = false;
            return;
        }
        
        if (character == null)
        {
            Debug.LogError("[ModernHUDComponent] 未找到Character组件！");
            enabled = false;
            return;
        }
        
        Debug.Log("[ModernHUDComponent] 组件验证通过");
    }
    
    /// <summary>
    /// 初始化HUD
    /// </summary>
    private void InitializeHUD()
    {
        // 应用HUD配置
        
        // 初始化生命值条
        InitializeHealthBar();
        
        // 初始化魔力值条
        InitializeManaBar();
        
        // 初始化经验值条
        InitializeExpBar();
        
        // 初始化小地图
        InitializeMinimap();
 
        Debug.Log("[ModernHUDComponent] HUD初始化完成");
    }
    

    
    /// <summary>
    /// 初始化生命值条
    /// </summary>
    private void InitializeHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
            healthBarFill.type = Image.Type.Filled;
            healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        }
        
    }
    
    /// <summary>
    /// 初始化魔力值条
    /// </summary>
    private void InitializeManaBar()
    {
        if (manaBarFill != null)
        {
            manaBarFill.fillAmount = 1f;
            manaBarFill.type = Image.Type.Filled;
            manaBarFill.fillMethod = Image.FillMethod.Horizontal;
        }
    }
    
    /// <summary>
    /// 初始化经验值条
    /// </summary>
    private void InitializeExpBar()
    {
        if (expBarFill != null)
        {
            expBarFill.fillAmount = 0f;
            expBarFill.type = Image.Type.Filled;
            expBarFill.fillMethod = Image.FillMethod.Horizontal;
        }
        
    }
    
    /// <summary>
    /// 初始化小地图
    /// </summary>
    private void InitializeMinimap()
    {
        if (minimapCamera != null)
        {
            // 设置小地图相机
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 10f;
            minimapCamera.cullingMask = LayerMask.GetMask("Minimap");
        }
    }
    
    /// <summary>
    /// 初始化技能槽位
    /// </summary>
    private void InitializeSkillSlots()
    {
        if (skillSlotsContainer == null || skillSlotPrefab == null || skillComponent == null)
            return;
        
        // 清除现有槽位
        foreach (Transform child in skillSlotsContainer)
        {
            DestroyImmediate(child.gameObject);
        }
        skillSlots.Clear();
        
        // 创建技能槽位
        int skillCount = skillComponent.GetSkillCount();
        for (int i = 0; i < skillCount; i++)
        {
            GameObject slotObj = Instantiate(skillSlotPrefab, skillSlotsContainer);
            SkillSlotUI slotUI = slotObj.GetComponent<SkillSlotUI>();
            
            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<SkillSlotUI>();
            }
            
            slotUI.Initialize(i, skillComponent);
            skillSlots.Add(slotUI);
        }
        
        Debug.Log($"[ModernHUDComponent] 创建了 {skillCount} 个技能槽位");
    }
    
    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEventListeners()
    {
        if (character != null)
        {
                      // 添加事件订阅
         character.OnHealthChanged += UpdateHealthDisplay;
    character.OnManaChanged += UpdateManaDisplay;
    // character.OnLevelUp += TriggerLevelUpEffect;
    // skillComponent.OnSkillPerformed += UpdateSkillSlots;
            character.OnLevelUp += OnLevelUp;
            character.OnDeath += OnPlayerDeath;
        }
    }
    
    /// <summary>
    /// 注销事件监听
    /// </summary>
    private void UnregisterEventListeners()
    {
        if (character != null)
        {
            character.OnLevelUp -= OnLevelUp;
            character.OnDeath -= OnPlayerDeath;
        }
    }
    
 

    /// <summary>
    /// 更新生命值显示
    /// </summary>
    private void UpdateHealthDisplay(int oldHealth, int currentHealth)
    {
        if (character == null) return;
        float healthPercent = (float)currentHealth / character.maxHealth;

        // 动画更新生命值条
        if (healthAnimCoroutine != null)
        {
            StopCoroutine(healthAnimCoroutine);
        }
        healthAnimCoroutine = StartCoroutine(AnimateBar(healthBarFill, healthPercent, 0.3f));

        // 更新生命值文本
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{character.maxHealth}";
        }

        // 低血量特效
        if (healthPercent <= 0.3f && lowHealthEffect != null && !lowHealthEffect.isPlaying)
        {
            lowHealthEffect.Play();
        }
        else if (healthPercent > 0.3f && lowHealthEffect != null && lowHealthEffect.isPlaying)
        {
            lowHealthEffect.Stop();
        }
        OnHealthChanged(oldHealth,currentHealth);
    }

    /// <summary>
    /// 更新魔力值显示
    /// </summary>
    private void UpdateManaDisplay(int oldMana, int currentMana)
    {
        if (character == null) return;

        float manaPercent = (float)currentMana / character.maxMana;

        // 动画更新魔力值条
        if (manaAnimCoroutine != null)
        {
            StopCoroutine(manaAnimCoroutine);
        }
        manaAnimCoroutine = StartCoroutine(AnimateBar(manaBarFill, manaPercent, 0.3f));

        // 更新魔力值文本
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{character.maxMana}";
        }

        // 魔力充能特效
        if (manaPercent >= 1f && manaChargeEffect != null && !manaChargeEffect.isPlaying)
        {
            manaChargeEffect.Play();
        }
        else if (manaPercent < 1f && manaChargeEffect != null && manaChargeEffect.isPlaying)
        {
            manaChargeEffect.Stop();
        }
        OnManaChanged(oldMana,currentMana);
    }

    /// <summary>
    /// 更新经验值显示
    /// </summary>
    private void UpdateExpDisplay(int nextExp, int currentExp)
    {
        if (character == null) return;

        int expForNextLevel = nextExp;
        int currentLevelExp = currentExp;
        float expPercent = expForNextLevel > 0 ? (float)currentLevelExp / expForNextLevel : 0f;

        // 动画更新经验值条
        if (expAnimCoroutine != null)
        {
            StopCoroutine(expAnimCoroutine);
        }
        expAnimCoroutine = StartCoroutine(AnimateBar(expBarFill, expPercent, 0.5f));

        // 更新经验值文本
        if (expText != null)
        {
            expText.text = $"{currentLevelExp}/{expForNextLevel}";
        }
    }
    

    /// <summary>
    /// 更新技能槽位
    /// </summary>
    private void UpdateSkillSlots()
    {
        foreach (var slot in skillSlots)
        {
            if (slot != null)
            {
                slot.UpdateDisplay();
            }
        }
    }
    
    /// <summary>
    /// 更新状态图标
    /// </summary>
    private void UpdateStatusIcons()
    {
        // TODO: 实现状态图标更新逻辑
        // 这里可以根据角色的状态效果来显示相应的图标
    }

    /// <summary>
    /// 强制更新显示
    /// </summary>
    private void ForceUpdateDisplay()
    {
        UpdateHealthDisplay(character.currentHealth,character.maxHealth);
        UpdateManaDisplay(character.currentMana,character.maxMana);
        UpdateExpDisplay(character.experienceToNext,character.experience);
    }
    
    /// <summary>
    /// 动画更新进度条
    /// </summary>
    private IEnumerator AnimateBar(Image bar, float targetValue, float duration)
    {
        if (bar == null) yield break;
        
        float startValue = bar.fillAmount;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            bar.fillAmount = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }
        
        bar.fillAmount = targetValue;
    }
    
    #region 事件处理
    
    /// <summary>
    /// 生命值变化事件处理
    /// </summary>
    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        // 生命值变化时的特殊效果
        if (newHealth < oldHealth)
        {
            // 受伤效果
            StartCoroutine(FlashEffect(healthBarBackground, Color.red, 0.2f));
        }
        else if (newHealth > oldHealth)
        {
            // 治疗效果
            StartCoroutine(FlashEffect(healthBarBackground, Color.green, 0.2f));
        }
    }
    
    /// <summary>
    /// 魔力值变化事件处理
    /// </summary>
    private void OnManaChanged(int oldMana, int newMana)
    {
        // 魔力值变化时的特殊效果
        if (newMana > oldMana)
        {
            // 魔力恢复效果
            StartCoroutine(FlashEffect(manaBarBackground, Color.cyan, 0.2f));
        }
    }
    
    /// <summary>
    /// 升级事件处理
    /// </summary>
    private void OnLevelUp(int newLevel)
    {
        // 升级特效
        if (levelUpEffect != null)
        {
            levelUpEffect.Play();
        }
          if (levelText != null)
        {
            levelText.text = $"Lv.{character.level}";
        }
        // 经验条闪烁效果
        StartCoroutine(FlashEffect(expBarBackground, Color.yellow, 0.5f));
    }
    
    /// <summary>
    /// 玩家死亡事件处理
    /// </summary>
    private void OnPlayerDeath()
    {
        // 死亡时的HUD效果
        StartCoroutine(FadeEffect(0.5f, 1f));
    }
    
    #endregion
    
    #region 特效方法
    
    /// <summary>
    /// 闪烁特效
    /// </summary>
    private IEnumerator FlashEffect(Image target, Color flashColor, float duration)
    {
        if (target == null) yield break;
        
        Color originalColor = target.color;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f);
            target.color = Color.Lerp(originalColor, flashColor, t);
            yield return null;
        }
        
        target.color = originalColor;
    }
    
    /// <summary>
    /// 淡入淡出特效
    /// </summary>
    private IEnumerator FadeEffect(float targetAlpha, float duration)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    #endregion
    
    #region 公共接口
    
    /// <summary>
    /// 设置HUD可见性
    /// </summary>
    public void SetHUDVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// 切换HUD可见性
    /// </summary>
    public void ToggleHUD()
    {
        SetHUDVisible(!gameObject.activeSelf);
    }
    
    /// <summary>
    /// 获取技能槽位
    /// </summary>
    public SkillSlotUI GetSkillSlot(int index)
    {
        if (index >= 0 && index < skillSlots.Count)
        {
            return skillSlots[index];
        }
        return null;
    }
    
    #endregion
}