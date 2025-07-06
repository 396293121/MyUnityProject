using UnityEngine; 
using UnityEngine.UI; 
using System.Collections.Generic; 

/// <summary>
/// 游戏HUD UI
/// 显示游戏中的HUD界面，包括生命条、魔法条、经验条、等级文本、职业文本和技能图标等
/// </summary>
public class GameHudUI : MonoBehaviour
{
    #region UI元素引用
    private Image healthBar;
    private Text healthText;
    private Image manaBar;
    private Text manaText;
    private Image expBar;
    private Text expText;
    private Text levelText;
    private Text classText;
    private Text coinText;
    private Text bloodText;
    private Text questDesc;
    private Text questObjective;

    private Dictionary<string, Image> skillIcons = new Dictionary<string, Image>();
    private Dictionary<string, Text> skillKeyTexts = new Dictionary<string, Text>();
    private Dictionary<string, Image> skillCooldownMasks = new Dictionary<string, Image>();
    private Dictionary<string, Text> skillCooldownTexts = new Dictionary<string, Text>();

    #endregion

    #region UI配置
    [Header("UI配置")]
    [SerializeField] private float barWidth = 200f;
    [SerializeField] private float barHeight = 20f;
    [SerializeField] private float barSpacing = 10f;
    [SerializeField] private float barPadding = 2f;
    [SerializeField] private float updateThreshold = 0.01f;
    [SerializeField] private float skillIconSize = 50f;
    [SerializeField] private float skillIconSpacing = 10f;
    [SerializeField] private float resourceIconSize = 24f;
    [SerializeField] private float resourceTextOffset = 30f;
    [SerializeField] private float questTrackerWidth = 230f;
    [SerializeField] private float questTrackerHeight = 100f;
    #endregion

    #region 私有字段
    private Dictionary<string, bool> dirtyFlags;
    private Dictionary<string, float> lastValues;
    private Dictionary<string, string> lastStringValues;
    private RectTransform canvasRect;
    #endregion

    #region Unity生命周期
    void Awake()
    {
        canvasRect = GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("[GameHudUI] 找不到Canvas的RectTransform组件！");
            enabled = false; // 禁用脚本
            return;
        }

        InitializeDirtyFlags();
        InitializeLastValues();
        CreateUIElements();
    }

    void Start()
    {
        ForceUpdate();
    }
    #endregion

    #region UI创建
    private void CreateUIElements()
    {
        // 创建状态条（生命、魔法、经验）
        CreateStatusBars();

        // 创建状态文本（等级、职业）
        CreateStatusTexts();

        // 创建技能图标
        CreateSkillIcons();

        // 创建资源显示（金币、精血）
        CreateResourceDisplay();

        // 创建任务追踪器
        CreateQuestTracker();
    }

    private void CreateStatusBars()
    {
        float startX = 20f;
        float startY = -20f; // 从顶部向下偏移

        // Health Bar
        GameObject healthBarBgObj = new GameObject("HealthBarBackground");
        healthBarBgObj.transform.SetParent(transform, false);
        Image healthBarBg = healthBarBgObj.AddComponent<Image>();
        healthBarBg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform healthBarBgRect = healthBarBg.rectTransform;
        healthBarBgRect.anchorMin = new Vector2(0, 1); // Top-left
        healthBarBgRect.anchorMax = new Vector2(0, 1); // Top-left
        healthBarBgRect.pivot = new Vector2(0, 1); // Top-left
        healthBarBgRect.anchoredPosition = new Vector2(startX, startY);
        healthBarBgRect.sizeDelta = new Vector2(barWidth, barHeight);

        GameObject healthBarObj = new GameObject("HealthBar");
        healthBarObj.transform.SetParent(healthBarBgObj.transform, false);
        healthBar = healthBarObj.AddComponent<Image>();
        healthBar.color = Color.red;
        RectTransform healthBarRect = healthBar.rectTransform;
        healthBarRect.anchorMin = new Vector2(0, 1); // Top-left
        healthBarRect.anchorMax = new Vector2(0, 1); // Top-left
        healthBarRect.pivot = new Vector2(0, 1); // Top-left
        healthBarRect.anchoredPosition = new Vector2(barPadding, -barPadding);
        healthBarRect.sizeDelta = new Vector2(barWidth - barPadding * 2, barHeight - barPadding * 2);

        // Mana Bar
        GameObject manaBarBgObj = new GameObject("ManaBarBackground");
        manaBarBgObj.transform.SetParent(transform, false);
        Image manaBarBg = manaBarBgObj.AddComponent<Image>();
        manaBarBg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform manaBarBgRect = manaBarBg.rectTransform;
        manaBarBgRect.anchorMin = new Vector2(0, 1); // Top-left
        manaBarBgRect.anchorMax = new Vector2(0, 1); // Top-left
        manaBarBgRect.pivot = new Vector2(0, 1); // Top-left
        manaBarBgRect.anchoredPosition = new Vector2(startX, startY - (barHeight + barSpacing));
        manaBarBgRect.sizeDelta = new Vector2(barWidth, barHeight);

        GameObject manaBarObj = new GameObject("ManaBar");
        manaBarObj.transform.SetParent(manaBarBgObj.transform, false);
        manaBar = manaBarObj.AddComponent<Image>();
        manaBar.color = Color.blue;
        RectTransform manaBarRect = manaBar.rectTransform;
        manaBarRect.anchorMin = new Vector2(0, 1); // Top-left
        manaBarRect.anchorMax = new Vector2(0, 1); // Top-left
        manaBarRect.pivot = new Vector2(0, 1); // Top-left
        manaBarRect.anchoredPosition = new Vector2(barPadding, -barPadding);
        manaBarRect.sizeDelta = new Vector2(barWidth - barPadding * 2, barHeight - barPadding * 2);

        // Exp Bar
        GameObject expBarBgObj = new GameObject("ExpBarBackground");
        expBarBgObj.transform.SetParent(transform, false);
        Image expBarBg = expBarBgObj.AddComponent<Image>();
        expBarBg.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform expBarBgRect = expBarBg.rectTransform;
        expBarBgRect.anchorMin = new Vector2(0, 1); // Top-left
        expBarBgRect.anchorMax = new Vector2(0, 1); // Top-left
        expBarBgRect.pivot = new Vector2(0, 1); // Top-left
        expBarBgRect.anchoredPosition = new Vector2(startX, startY - (barHeight + barSpacing) * 2);
        expBarBgRect.sizeDelta = new Vector2(barWidth, barHeight / 2);

        GameObject expBarObj = new GameObject("ExpBar");
        expBarObj.transform.SetParent(expBarBgObj.transform, false);
        expBar = expBarObj.AddComponent<Image>();
        expBar.color = Color.yellow;
        RectTransform expBarRect = expBar.rectTransform;
        expBarRect.anchorMin = new Vector2(0, 1); // Top-left
        expBarRect.anchorMax = new Vector2(0, 1); // Top-left
        expBarRect.pivot = new Vector2(0, 1); // Top-left
        expBarRect.anchoredPosition = new Vector2(barPadding, -barPadding);
        expBarRect.sizeDelta = new Vector2(barWidth - barPadding * 2, barHeight / 2 - barPadding * 2);

        // Status Texts (HP, MP, EXP)
        GameObject healthTextObj = new GameObject("HealthText");
        healthTextObj.transform.SetParent(transform, false);
        healthText = healthTextObj.AddComponent<Text>();
        healthText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleLeft;
        RectTransform healthTextRect = healthText.rectTransform;
        healthTextRect.anchorMin = new Vector2(0, 1); // Top-left
        healthTextRect.anchorMax = new Vector2(0, 1); // Top-left
        healthTextRect.pivot = new Vector2(0, 0.5f); // Middle-left
        healthTextRect.anchoredPosition = new Vector2(startX + barWidth + 10, startY - barHeight / 2);
        healthTextRect.sizeDelta = new Vector2(100, barHeight);

        GameObject manaTextObj = new GameObject("ManaText");
        manaTextObj.transform.SetParent(transform, false);
        manaText = manaTextObj.AddComponent<Text>();
        manaText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        manaText.color = Color.white;
        manaText.alignment = TextAnchor.MiddleLeft;
        RectTransform manaTextRect = manaText.rectTransform;
        manaTextRect.anchorMin = new Vector2(0, 1); // Top-left
        manaTextRect.anchorMax = new Vector2(0, 1); // Top-left
        manaTextRect.pivot = new Vector2(0, 0.5f); // Middle-left
        manaTextRect.anchoredPosition = new Vector2(startX + barWidth + 10, startY - (barHeight + barSpacing) - barHeight / 2);
        manaTextRect.sizeDelta = new Vector2(100, barHeight);

        GameObject expTextObj = new GameObject("ExpText");
        expTextObj.transform.SetParent(transform, false);
        expText = expTextObj.AddComponent<Text>();
        expText.font = Font.CreateDynamicFontFromOSFont("Arial", 12);
        expText.color = Color.white;
        expText.alignment = TextAnchor.MiddleLeft;
        RectTransform expTextRect = expText.rectTransform;
        expTextRect.anchorMin = new Vector2(0, 1); // Top-left
        expTextRect.anchorMax = new Vector2(0, 1); // Top-left
        expTextRect.pivot = new Vector2(0, 0.5f); // Middle-left
        expTextRect.anchoredPosition = new Vector2(startX + barWidth + 10, startY - (barHeight + barSpacing) * 2 - barHeight / 4);
        expTextRect.sizeDelta = new Vector2(100, barHeight / 2);
    }

    private void CreateStatusTexts()
    {
        float startX = 20f;
        float startY = - (20f + (barHeight + barSpacing) * 2 + barHeight / 2 + 20f); // 从顶部向下偏移

        // Level Text
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(transform, false);
        levelText = levelTextObj.AddComponent<Text>();
        levelText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        levelText.color = Color.white;
        levelText.alignment = TextAnchor.UpperLeft;
        RectTransform levelTextRect = levelText.rectTransform;
        levelTextRect.anchorMin = new Vector2(0, 1); // Top-left
        levelTextRect.anchorMax = new Vector2(0, 1); // Top-left
        levelTextRect.pivot = new Vector2(0, 1); // Top-left
        levelTextRect.anchoredPosition = new Vector2(startX, startY);
        levelTextRect.sizeDelta = new Vector2(80, 20);

        // Class Text
        GameObject classTextObj = new GameObject("ClassText");
        classTextObj.transform.SetParent(transform, false);
        classText = classTextObj.AddComponent<Text>();
        classText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        classText.color = Color.white;
        classText.alignment = TextAnchor.UpperLeft;
        RectTransform classTextRect = classText.rectTransform;
        classTextRect.anchorMin = new Vector2(0, 1); // Top-left
        classTextRect.anchorMax = new Vector2(0, 1); // Top-left
        classTextRect.pivot = new Vector2(0, 1); // Top-left
        classTextRect.anchoredPosition = new Vector2(startX + 80, startY);
        classTextRect.sizeDelta = new Vector2(100, 20);
    }

    private void CreateSkillIcons()
    {
        float startX = 20f;
        float startY = 20f; // 从底部向上偏移

        string[] skillKeys = { "skill_0", "skill_1", "skill_2", "skill_3" };
        string[] hotkeys = { "Q", "W", "E", "R" };

        for (int i = 0; i < 4; i++)
        {
            // Skill Background
            GameObject skillBgObj = new GameObject($"SkillBackground_{i}");
            skillBgObj.transform.SetParent(transform, false);
            Image skillBg = skillBgObj.AddComponent<Image>();
            skillBg.color = new Color(0f, 0f, 0f, 0.7f);
            RectTransform skillBgRect = skillBg.rectTransform;
            skillBgRect.anchorMin = new Vector2(0, 0); // Bottom-left
            skillBgRect.anchorMax = new Vector2(0, 0); // Bottom-left
            skillBgRect.pivot = new Vector2(0, 0); // Bottom-left
            skillBgRect.anchoredPosition = new Vector2(startX + (skillIconSize + skillIconSpacing) * i, startY);
            skillBgRect.sizeDelta = new Vector2(skillIconSize, skillIconSize);

            // Skill Icon (Placeholder)
            GameObject skillIconObj = new GameObject($"SkillIcon_{i}");
            skillIconObj.transform.SetParent(skillBgObj.transform, false);
            Image skillIcon = skillIconObj.AddComponent<Image>();
            skillIcon.color = Color.white; // Placeholder color, replace with actual sprite
            RectTransform skillIconRect = skillIcon.rectTransform;
            skillIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            skillIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            skillIconRect.pivot = new Vector2(0.5f, 0.5f);
            skillIconRect.anchoredPosition = Vector2.zero;
            skillIconRect.sizeDelta = new Vector2(skillIconSize * 0.6f, skillIconSize * 0.6f); // Scale
            skillIcons.Add(skillKeys[i], skillIcon);

            // Hotkey Text
            GameObject keyTextObj = new GameObject($"HotkeyText_{i}");
            keyTextObj.transform.SetParent(skillBgObj.transform, false);
            Text keyText = keyTextObj.AddComponent<Text>();
            keyText.font = Font.CreateDynamicFontFromOSFont("Arial", 12);
            keyText.color = Color.white;
            keyText.alignment = TextAnchor.UpperLeft;
            keyText.text = hotkeys[i];
            RectTransform keyTextRect = keyText.rectTransform;
            keyTextRect.anchorMin = new Vector2(0, 1); // Top-left
            keyTextRect.anchorMax = new Vector2(0, 1); // Top-left
            keyTextRect.pivot = new Vector2(0, 1); // Top-left
            keyTextRect.anchoredPosition = new Vector2(5, -5);
            keyTextRect.sizeDelta = new Vector2(20, 20);
            skillKeyTexts.Add(skillKeys[i], keyText);

            // Cooldown Mask
            GameObject cooldownMaskObj = new GameObject($"CooldownMask_{i}");
            cooldownMaskObj.transform.SetParent(skillBgObj.transform, false);
            Image cooldownMask = cooldownMaskObj.AddComponent<Image>();
            cooldownMask.color = new Color(0f, 0f, 0f, 0.7f);
            RectTransform cooldownMaskRect = cooldownMask.rectTransform;
            cooldownMaskRect.anchorMin = Vector2.zero;
            cooldownMaskRect.anchorMax = Vector2.one;
            cooldownMaskRect.sizeDelta = Vector2.zero;
            cooldownMask.gameObject.SetActive(false);
            skillCooldownMasks.Add(skillKeys[i], cooldownMask);

            // Cooldown Text
            GameObject cooldownTextObj = new GameObject($"CooldownText_{i}");
            cooldownTextObj.transform.SetParent(skillBgObj.transform, false);
            Text cooldownText = cooldownTextObj.AddComponent<Text>();
            cooldownText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            cooldownText.color = Color.white;
            cooldownText.alignment = TextAnchor.MiddleCenter;
            RectTransform cooldownTextRect = cooldownText.rectTransform;
            cooldownTextRect.anchorMin = Vector2.zero;
            cooldownTextRect.anchorMax = Vector2.one;
            cooldownTextRect.sizeDelta = Vector2.zero;
            cooldownText.gameObject.SetActive(false);
            skillCooldownTexts.Add(skillKeys[i], cooldownText);
        }
    }

    private void CreateResourceDisplay()
    {
        float startX = -20f; // 从右侧向左偏移
        float startY = -20f; // 从顶部向下偏移

        // Coin Icon (Placeholder)
        GameObject coinIconObj = new GameObject("CoinIcon");
        coinIconObj.transform.SetParent(transform, false);
        Image coinIcon = coinIconObj.AddComponent<Image>();
        coinIcon.color = Color.yellow; // Placeholder color, replace with actual sprite
        RectTransform coinIconRect = coinIcon.rectTransform;
        coinIconRect.anchorMin = new Vector2(1, 1); // Top-right
        coinIconRect.anchorMax = new Vector2(1, 1); // Top-right
        coinIconRect.pivot = new Vector2(1, 1); // Top-right
        coinIconRect.anchoredPosition = new Vector2(startX, startY);
        coinIconRect.sizeDelta = new Vector2(resourceIconSize, resourceIconSize);

        // Coin Text
        GameObject coinTextObj = new GameObject("CoinText");
        coinTextObj.transform.SetParent(transform, false);
        coinText = coinTextObj.AddComponent<Text>();
        coinText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        coinText.color = Color.yellow;
        coinText.alignment = TextAnchor.MiddleRight;
        RectTransform coinTextRect = coinText.rectTransform;
        coinTextRect.anchorMin = new Vector2(1, 1); // Top-right
        coinTextRect.anchorMax = new Vector2(1, 1); // Top-right
        coinTextRect.pivot = new Vector2(1, 0.5f); // Middle-right
        coinTextRect.anchoredPosition = new Vector2(startX - resourceTextOffset, startY - resourceIconSize / 2);
        coinTextRect.sizeDelta = new Vector2(100, resourceIconSize);

        // Blood Essence Icon (Placeholder)
        GameObject bloodIconObj = new GameObject("BloodIcon");
        bloodIconObj.transform.SetParent(transform, false);
        Image bloodIcon = bloodIconObj.AddComponent<Image>();
        bloodIcon.color = Color.red; // Placeholder color, replace with actual sprite
        RectTransform bloodIconRect = bloodIcon.rectTransform;
        bloodIconRect.anchorMin = new Vector2(1, 1); // Top-right
        bloodIconRect.anchorMax = new Vector2(1, 1); // Top-right
        bloodIconRect.pivot = new Vector2(1, 1); // Top-right
        bloodIconRect.anchoredPosition = new Vector2(startX, startY - resourceIconSize - 10);
        bloodIconRect.sizeDelta = new Vector2(resourceIconSize, resourceIconSize);

        // Blood Essence Text
        GameObject bloodTextObj = new GameObject("BloodText");
        bloodTextObj.transform.SetParent(transform, false);
        bloodText = bloodTextObj.AddComponent<Text>();
        bloodText.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        bloodText.color = Color.red;
        bloodText.alignment = TextAnchor.MiddleRight;
        RectTransform bloodTextRect = bloodText.rectTransform;
        bloodTextRect.anchorMin = new Vector2(1, 1); // Top-right
        bloodTextRect.anchorMax = new Vector2(1, 1); // Top-right
        bloodTextRect.pivot = new Vector2(1, 0.5f); // Middle-right
        bloodTextRect.anchoredPosition = new Vector2(startX - resourceTextOffset, startY - resourceIconSize - 10 - resourceIconSize / 2);
        bloodTextRect.sizeDelta = new Vector2(100, resourceIconSize);
    }

    private void CreateQuestTracker()
    {
        float startX = -20f; // 从右侧向左偏移
        float startY = -(80f); // 从顶部向下偏移

        // Quest Tracker Background
        GameObject questBgObj = new GameObject("QuestTrackerBackground");
        questBgObj.transform.SetParent(transform, false);
        Image questBg = questBgObj.AddComponent<Image>();
        questBg.color = new Color(0f, 0f, 0f, 0.5f);
        RectTransform questBgRect = questBg.rectTransform;
        questBgRect.anchorMin = new Vector2(1, 1); // Top-right
        questBgRect.anchorMax = new Vector2(1, 1); // Top-right
        questBgRect.pivot = new Vector2(1, 1); // Top-right
        questBgRect.anchoredPosition = new Vector2(startX, startY);
        questBgRect.sizeDelta = new Vector2(questTrackerWidth, questTrackerHeight);

        // Quest Title
        GameObject questTitleObj = new GameObject("QuestTitle");
        questTitleObj.transform.SetParent(questBgObj.transform, false);
        Text questTitle = questTitleObj.AddComponent<Text>();
        questTitle.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        questTitle.color = Color.yellow;
        questTitle.alignment = TextAnchor.UpperLeft;
        questTitle.text = "当前任务";
        RectTransform questTitleRect = questTitle.rectTransform;
        questTitleRect.anchorMin = new Vector2(0, 1); // Top-left
        questTitleRect.anchorMax = new Vector2(0, 1); // Top-left
        questTitleRect.pivot = new Vector2(0, 1); // Top-left
        questTitleRect.anchoredPosition = new Vector2(10, -10);
        questTitleRect.sizeDelta = new Vector2(questTrackerWidth - 20, 20);

        // Quest Description
        GameObject questDescObj = new GameObject("QuestDescription");
        questDescObj.transform.SetParent(questBgObj.transform, false);
        questDesc = questDescObj.AddComponent<Text>();
        questDesc.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        questDesc.color = Color.white;
        questDesc.alignment = TextAnchor.UpperLeft;
        questDesc.text = "无任务";
        RectTransform questDescRect = questDesc.rectTransform;
        questDescRect.anchorMin = new Vector2(0, 1); // Top-left
        questDescRect.anchorMax = new Vector2(0, 1); // Top-left
        questDescRect.pivot = new Vector2(0, 1); // Top-left
        questDescRect.anchoredPosition = new Vector2(10, -35);
        questDescRect.sizeDelta = new Vector2(questTrackerWidth - 20, 30);

        // Quest Objective
        GameObject questObjectiveObj = new GameObject("QuestObjective");
        questObjectiveObj.transform.SetParent(questBgObj.transform, false);
        questObjective = questObjectiveObj.AddComponent<Text>();
        questObjective.font = Font.CreateDynamicFontFromOSFont("Arial", 12);
        questObjective.color = Color.gray;
        questObjective.alignment = TextAnchor.UpperLeft;
        questObjective.text = "";
        RectTransform questObjectiveRect = questObjective.rectTransform;
        questObjectiveRect.anchorMin = new Vector2(0, 1); // Top-left
        questObjectiveRect.anchorMax = new Vector2(0, 1); // Top-left
        questObjectiveRect.pivot = new Vector2(0, 1); // Top-left
        questObjectiveRect.anchoredPosition = new Vector2(10, -70);
        questObjectiveRect.sizeDelta = new Vector2(questTrackerWidth - 20, 20);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 更新UI
    /// </summary>
    /// <param name="playerHealth">玩家当前生命值</param>
    /// <param name="playerMaxHealth">玩家最大生命值</param>
    /// <param name="playerMana">玩家当前魔法值</param>
    /// <param name="playerMaxMana">玩家最大魔法值</param>
    /// <param name="playerExperience">玩家当前经验值</param>
    /// <param name="playerExpToNextLevel">玩家升级所需经验值</param>
    /// <param name="playerLevel">玩家等级</param>
    /// <param name="playerClassName">玩家职业名称</param>
    /// <param name="gold">金币数量</param>
    /// <param name="bloodEssence">精血数量</param>
    /// <param name="questTitle">任务标题</param>
    /// <param name="questObjectiveText">任务目标文本</param>
    public void UpdateUI(
        float playerHealth, float playerMaxHealth,
        float playerMana, float playerMaxMana,
        float playerExperience, float playerExpToNextLevel,
        int playerLevel, string playerClassName,
        int gold, int bloodEssence,
        string questTitle, string questObjectiveText,
        List<float> skillCurrentCooldowns, List<float> skillMaxCooldowns)
    {
        float lastHealth = lastValues["health"];
        CheckAndSetDirtyFlag(ref lastHealth, playerHealth, "health");
        lastValues["health"] = lastHealth;

        float lastMaxHealth = lastValues["maxHealth"];
        CheckAndSetDirtyFlag(ref lastMaxHealth, playerMaxHealth, "health");
        lastValues["maxHealth"] = lastMaxHealth;

        float lastMana = lastValues["mana"];
        CheckAndSetDirtyFlag(ref lastMana, playerMana, "mana");
        lastValues["mana"] = lastMana;

        float lastMaxMana = lastValues["maxMana"];
        CheckAndSetDirtyFlag(ref lastMaxMana, playerMaxMana, "mana");
        lastValues["maxMana"] = lastMaxMana;

        float lastExperience = lastValues["experience"];
        CheckAndSetDirtyFlag(ref lastExperience, playerExperience, "experience");
        lastValues["experience"] = lastExperience;

        float lastExperienceToNextLevel = lastValues["experienceToNextLevel"];
        CheckAndSetDirtyFlag(ref lastExperienceToNextLevel, playerExpToNextLevel, "experience");
        lastValues["experienceToNextLevel"] = lastExperienceToNextLevel;

        float lastLevel = lastValues["level"];
        CheckAndSetDirtyFlag(ref lastLevel, playerLevel, "level");
        lastValues["level"] = lastLevel;

        string lastClassName = lastStringValues["className"];
        CheckAndSetDirtyFlag(ref lastClassName, playerClassName, "class");
        lastStringValues["className"] = lastClassName;

        float lastGold = lastValues["gold"];
        CheckAndSetDirtyFlag(ref lastGold, gold, "resources");
        lastValues["gold"] = lastGold;

        float lastBloodEssence = lastValues["bloodEssence"];
        CheckAndSetDirtyFlag(ref lastBloodEssence, bloodEssence, "resources");
        lastValues["bloodEssence"] = lastBloodEssence;

        string lastQuestTitle = lastStringValues["questTitle"];
        CheckAndSetDirtyFlag(ref lastQuestTitle, questTitle, "quest");
        lastStringValues["questTitle"] = lastQuestTitle;

        string lastQuestObjective = lastStringValues["questObjective"];
        CheckAndSetDirtyFlag(ref lastQuestObjective, questObjectiveText, "quest");
        lastStringValues["questObjective"] = lastQuestObjective;
        // 检查技能冷却时间是否发生变化
        bool skillCooldownsChanged = false;
        if (skillCurrentCooldowns.Count != skillIcons.Count)
        {
            skillCooldownsChanged = true;
        }
        else
        {
            for (int i = 0; i < skillCurrentCooldowns.Count; i++)
            {
                if (Mathf.Abs(skillCurrentCooldowns[i] - (lastSkillCooldowns.Count > i ? lastSkillCooldowns[i] : 0f)) > updateThreshold)
                {
                    skillCooldownsChanged = true;
                    break;
                }
            }
        }

        if (skillCooldownsChanged)
        {
            dirtyFlags["skills"] = true;
            lastSkillCooldowns = new List<float>(skillCurrentCooldowns);
        }

        UpdateDirtyElements(
            playerHealth, playerMaxHealth, playerMana, playerMaxMana,
            playerExperience, playerExpToNextLevel, playerLevel, playerClassName,
            gold, bloodEssence, questTitle, questObjectiveText,
            skillCurrentCooldowns, skillMaxCooldowns);
    }

    /// <summary>
    /// 强制更新所有UI元素
    /// </summary>
    public void ForceUpdate()
    {
        foreach (var key in new List<string>(dirtyFlags.Keys))
        {
            dirtyFlags[key] = true;
        }
        UpdateDirtyElements(
            lastValues["health"], lastValues["maxHealth"],
            lastValues["mana"], lastValues["maxMana"],
            lastValues["experience"], lastValues["experienceToNextLevel"],
            (int)lastValues["level"], lastStringValues["className"],
            (int)lastValues["gold"], (int)lastValues["bloodEssence"],
            lastStringValues["questTitle"], lastStringValues["questObjective"],
            new List<float>(), new List<float>()); // ForceUpdate时技能冷却数据为空，需要根据实际情况调整
    }

    /// <summary>
    /// 重置缓存值
    /// </summary>
    public void ResetCache()
    {
        InitializeLastValues();
        ForceUpdate();
    }
    #endregion

    #region 私有方法
    private void InitializeDirtyFlags()
    {
        dirtyFlags = new Dictionary<string, bool>
        {
            { "health", true },
            { "mana", true },
            { "experience", true },
            { "level", true },
            { "class", true },
            { "resources", true },
            { "quest", true },
            { "skills", true } // 技能冷却需要单独处理
        };
    }

    private List<float> lastSkillCooldowns; // 用于存储上一次的技能冷却时间

    private void InitializeLastValues()
    {
        lastValues = new Dictionary<string, float>
        {
            { "health", 0 }, { "maxHealth", 0 },
            { "mana", 0 }, { "maxMana", 0 },
            { "experience", 0 }, { "experienceToNextLevel", 0 },
            { "level", 0 },
            { "gold", 0 }, { "bloodEssence", 0 }
        };
        lastStringValues = new Dictionary<string, string>
        {
            { "className", "" },
            { "questTitle", "" }, { "questObjective", "" }
        };
        lastSkillCooldowns = new List<float>();
    }

    private void CheckAndSetDirtyFlag<T>(ref T lastValue, T currentValue, string flagName)
    {
        if (!EqualityComparer<T>.Default.Equals(lastValue, currentValue))
        {
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int))
            {
                if (System.Math.Abs(System.Convert.ToSingle(lastValue) - System.Convert.ToSingle(currentValue)) > updateThreshold)
                {
                    dirtyFlags[flagName] = true;
                }
            }
            else
            {
                dirtyFlags[flagName] = true;
            }
            lastValue = currentValue;
        }
    }

    private void UpdateDirtyElements(
        float playerHealth, float playerMaxHealth,
        float playerMana, float playerMaxMana,
        float playerExperience, float playerExpToNextLevel,
        int playerLevel, string playerClassName,
        int gold, int bloodEssence,
        string questTitle, string questObjectiveText,
        List<float> skillCurrentCooldowns, List<float> skillMaxCooldowns)
    {
        if (dirtyFlags["health"]) { UpdateHealthBar(playerHealth, playerMaxHealth); dirtyFlags["health"] = false; }
        if (dirtyFlags["mana"]) { UpdateManaBar(playerMana, playerMaxMana); dirtyFlags["mana"] = false; }
        if (dirtyFlags["experience"]) { UpdateExpBar(playerExperience, playerExpToNextLevel); dirtyFlags["experience"] = false; }
        if (dirtyFlags["level"]) { UpdateLevelText(playerLevel); dirtyFlags["level"] = false; }
        if (dirtyFlags["class"]) { UpdateClassText(playerClassName); dirtyFlags["class"] = false; }
        if (dirtyFlags["resources"]) { UpdateResourceDisplay(gold, bloodEssence); dirtyFlags["resources"] = false; }
        if (dirtyFlags["quest"]) { UpdateQuestTracker(questTitle, questObjectiveText); dirtyFlags["quest"] = false; }
        if (dirtyFlags["skills"]) { UpdateSkillCooldowns(skillCurrentCooldowns, skillMaxCooldowns); dirtyFlags["skills"] = false; }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar != null && healthText != null)
        {
            float healthPercent = Mathf.Max(0f, Mathf.Min(currentHealth / maxHealth, 1f));
            healthBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (barWidth - barPadding * 2) * healthPercent);
            healthText.text = $"HP: {Mathf.FloorToInt(currentHealth)}/{Mathf.FloorToInt(maxHealth)}";
        }
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        if (manaBar != null && manaText != null)
        {
            float manaPercent = Mathf.Max(0f, Mathf.Min(currentMana / maxMana, 1f));
            manaBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (barWidth - barPadding * 2) * manaPercent);
            manaText.text = $"MP: {Mathf.FloorToInt(currentMana)}/{Mathf.FloorToInt(maxMana)}";
        }
    }

    private void UpdateExpBar(float currentExp, float expToNextLevel)
    {
        if (expBar != null && expText != null)
        {
            float expPercent = Mathf.Max(0f, Mathf.Min(currentExp / expToNextLevel, 1f));
            expBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (barWidth - barPadding * 2) * expPercent);
            expText.text = $"EXP: {Mathf.FloorToInt(currentExp)}/{Mathf.FloorToInt(expToNextLevel)}";
        }
    }

    private void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"LV: {level}";
        }
    }

    private void UpdateClassText(string className)
    {
        if (classText != null)
        {
            classText.text = $"职业: {className}";
        }
    }

    private void UpdateResourceDisplay(int gold, int bloodEssence)
    {
        if (coinText != null)
        {
            coinText.text = gold.ToString();
        }
        if (bloodText != null)
        {
            bloodText.text = bloodEssence.ToString();
        }
    }

    private void UpdateQuestTracker(string questTitle, string questObjectiveText)
    {
        if (questDesc != null)
        {
            questDesc.text = questTitle;
        }
        if (questObjective != null)
        {
            questObjective.text = questObjectiveText;
        }
    }

    private void UpdateSkillCooldowns(List<float> skillCurrentCooldowns, List<float> skillMaxCooldowns)
    {
        for (int i = 0; i < skillCurrentCooldowns.Count; i++) // Loop based on the provided cooldown lists
        {
            string skillKey = $"SkillIcon_{i}"; // Construct the key

            if (skillCooldownMasks.ContainsKey(skillKey) && skillCooldownMasks[skillKey] != null)
            {
                Image cooldownMask = skillCooldownMasks[skillKey];
                Text cooldownText = skillCooldownTexts.ContainsKey(skillKey) ? skillCooldownTexts[skillKey] : null;

                float currentCooldown = skillCurrentCooldowns[i];
                float maxCooldown = skillMaxCooldowns[i];

                if (maxCooldown > 0 && currentCooldown < maxCooldown)
                {
                    cooldownMask.fillAmount = currentCooldown / maxCooldown;
                    cooldownMask.gameObject.SetActive(true);

                    if (cooldownText != null)
                    {
                        cooldownText.text = Mathf.CeilToInt(maxCooldown - currentCooldown).ToString(); // Convert float to int then to string
                        cooldownText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    cooldownMask.gameObject.SetActive(false);
                    if (cooldownText != null)
                    {
                        cooldownText.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
    #endregion
}