using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// UI管理器 - 统一管理游戏中的所有UI界面和交互
/// 从原Phaser项目的UI系统迁移而来
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI面板引用")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject inventoryPanel;
    public GameObject characterPanel;
    public GameObject saveLoadPanel;
    public GameObject gameOverPanel;
    
    [Header("游戏内UI组件")]
    public Slider healthBar;
    public Slider manaBar;
    public Slider experienceBar;
    public Text levelText;
    public Text healthText;
    public Text manaText;
    
    [Header("技能UI组件")]
    public Button[] skillButtons;
    public Image[] skillCooldownImages;
    public Text[] skillCooldownTexts;
    
    [Header("消息系统")]
    public GameObject messagePanel;
    public Text messageText;
    public float messageDuration = 3f;
    
    [Header("确认对话框")]
    public GameObject confirmDialog;
    public Text confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;
    
    // UI状态管理
    private Stack<GameObject> uiStack = new Stack<GameObject>();
    private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();
    
    // 当前显示的角色（用于更新UI）
    private Character currentCharacter;
    
    // 消息队列
    private Queue<string> messageQueue = new Queue<string>();
    private bool isShowingMessage = false;
    
    // 确认对话框回调
    private System.Action onConfirmYes;
    private System.Action onConfirmNo;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 设置按钮事件
        SetupButtonEvents();
        
        // 显示主菜单
        ShowPanel("MainMenu");
    }
    
    private void Update()
    {
        // 更新角色UI
        UpdateCharacterUI();
        
        // 更新技能UI
      //  UpdateSkillUI();
        
        // 处理输入
        HandleUIInput();
        
        // 处理消息队列
        ProcessMessageQueue();
    }
    
    /// <summary>
    /// 初始化UI系统
    /// </summary>
    private void InitializeUI()
    {
        // 将UI面板添加到字典中
        if (mainMenuPanel != null) uiPanels["MainMenu"] = mainMenuPanel;
        if (gameplayPanel != null) uiPanels["Gameplay"] = gameplayPanel;
        if (pauseMenuPanel != null) uiPanels["PauseMenu"] = pauseMenuPanel;
        if (settingsPanel != null) uiPanels["Settings"] = settingsPanel;
        if (inventoryPanel != null) uiPanels["Inventory"] = inventoryPanel;
        if (characterPanel != null) uiPanels["Character"] = characterPanel;
        if (saveLoadPanel != null) uiPanels["SaveLoad"] = saveLoadPanel;
        if (gameOverPanel != null) uiPanels["GameOver"] = gameOverPanel;
        
        // 初始时隐藏所有面板
        foreach (var panel in uiPanels.Values)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        
        // 隐藏消息面板和确认对话框
        if (messagePanel != null) messagePanel.SetActive(false);
        if (confirmDialog != null) confirmDialog.SetActive(false);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log("[UIManager] UI系统初始化完成");
        }
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtonEvents()
    {
        // 确认对话框按钮
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.AddListener(() => {
                onConfirmYes?.Invoke();
                HideConfirmDialog();
            });
        }
        
        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.AddListener(() => {
                onConfirmNo?.Invoke();
                HideConfirmDialog();
            });
        }
        
        // 技能按钮事件
   //     SetupSkillButtons();
    }
    
    /// <summary>
    /// 设置技能按钮
    /// </summary>
    // private void SetupSkillButtons()
    // {
    //     if (skillButtons == null) return;
        
    //     for (int i = 0; i < skillButtons.Length; i++)
    //     {
    //         int skillIndex = i; // 闭包变量
    //         if (skillButtons[i] != null)
    //         {
    //             skillButtons[i].onClick.AddListener(() => UseSkill(skillIndex));
    //         }
    //     }
    // }
    
    /// <summary>
    /// 显示UI面板
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (!uiPanels.ContainsKey(panelName))
        {
            Debug.LogWarning($"[UIManager] 找不到UI面板: {panelName}");
            return;
        }
        
        GameObject panel = uiPanels[panelName];
        if (panel == null) return;
        
        // 隐藏当前顶层面板
        if (uiStack.Count > 0)
        {
            GameObject currentPanel = uiStack.Peek();
            if (currentPanel != null)
            {
                currentPanel.SetActive(false);
            }
        }
        
        // 显示新面板
        panel.SetActive(true);
        uiStack.Push(panel);
    }
    
    // 重复的SetCurrentCharacter方法已删除
    
    /// <summary>
    /// 更新角色UI
    /// </summary>
    public void UpdateCharacterUI(Character character)
    {
        if (character == null) return;
        
        // 更新生命值
        if (healthBar != null)
        {
            healthBar.value = (float)character.currentHealth / character.maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{character.currentHealth}/{character.maxHealth}";
        }
        
        // 更新魔法值
        if (manaBar != null)
        {
            manaBar.value = (float)character.currentMana / character.maxMana;
        }
        
        if (manaText != null)
        {
            manaText.text = $"{character.currentMana}/{character.maxMana}";
        }
        
        // 更新等级
        if (levelText != null)
        {
            levelText.text = $"Lv.{character.level}";
        }
        
        // 更新经验值
        if (experienceBar != null && character.maxExperience > 0)
        {
            experienceBar.value = (float)character.currentExperience / character.maxExperience;
        }
    }
    
    /// <summary>
    /// 显示消息
    /// </summary>
    public void ShowMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        messageQueue.Enqueue(message);
    }
    
    /// <summary>
    /// 显示确认对话框
    /// </summary>
    public void ShowConfirmDialog(string message, System.Action onYes, System.Action onNo)
    {
        if (confirmDialog == null) return;
        
        if (confirmText != null)
        {
            confirmText.text = message;
        }
        
        onConfirmYes = onYes;
        onConfirmNo = onNo;
        
        confirmDialog.SetActive(true);
    }
    
    /// <summary>
    /// 隐藏确认对话框
    /// </summary>
    public void HideConfirmDialog()
    {
        if (confirmDialog != null)
        {
            confirmDialog.SetActive(false);
        }
        
        onConfirmYes = null;
        onConfirmNo = null;
    }
    
    /// <summary>
    /// 处理消息队列
    /// </summary>
    private void ProcessMessageQueue()
    {
        if (messageQueue.Count > 0 && !isShowingMessage)
        {
            string message = messageQueue.Dequeue();
            StartCoroutine(ShowMessageCoroutine(message));
        }
    }
    
    /// <summary>
    /// 显示消息协程
    /// </summary>
    private System.Collections.IEnumerator ShowMessageCoroutine(string message)
    {
        isShowingMessage = true;
        
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);
            
            yield return new WaitForSeconds(2f);
            
            messagePanel.SetActive(false);
        }
        
        isShowingMessage = false;
    }
    
    /// <summary>
    /// 隐藏当前面板
    /// </summary>
    public void HideCurrentPanel()
    {
        if (uiStack.Count == 0) return;
        
        GameObject currentPanel = uiStack.Pop();
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        
        // 显示上一个面板
        if (uiStack.Count > 0)
        {
            GameObject previousPanel = uiStack.Peek();
            if (previousPanel != null)
            {
                previousPanel.SetActive(true);
            }
        }
        
        // 播放UI音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ui_cancel");
        }
    }
    
    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    public void HideAllPanels()
    {
        while (uiStack.Count > 0)
        {
            GameObject panel = uiStack.Pop();
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 切换面板显示状态
    /// </summary>
    public void TogglePanel(string panelName)
    {
        if (!uiPanels.ContainsKey(panelName)) return;
        
        GameObject panel = uiPanels[panelName];
        if (panel == null) return;
        
        if (panel.activeInHierarchy)
        {
            HideCurrentPanel();
        }
        else
        {
            ShowPanel(panelName);
        }
    }
    
    /// <summary>
    /// 设置当前角色（用于UI更新）
    /// </summary>
    public void SetCurrentCharacter(Character character)
    {
        currentCharacter = character;
    }
    
    /// <summary>
    /// 更新角色UI
    /// </summary>
    private void UpdateCharacterUI()
    {
        if (currentCharacter == null) return;
        
        // 更新生命值条
        if (healthBar != null)
        {
            healthBar.value = (float)currentCharacter.currentHealth / currentCharacter.maxHealth;
        }
        
        // 更新魔法值条
        if (manaBar != null)
        {
            manaBar.value = (float)currentCharacter.currentMana / currentCharacter.maxMana;
        }
        
        // 更新经验值条
        if (experienceBar != null)
        {
            int expForNextLevel = currentCharacter.GetExperienceForNextLevel();
            int currentLevelExp = currentCharacter.experience - currentCharacter.GetExperienceForLevel(currentCharacter.level);
            int expNeeded = expForNextLevel - currentCharacter.GetExperienceForLevel(currentCharacter.level);
            
            if (expNeeded > 0)
            {
                experienceBar.value = (float)currentLevelExp / expNeeded;
            }
        }
        
        // 更新文本
        if (levelText != null)
        {
            levelText.text = $"等级 {currentCharacter.level}";
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentCharacter.currentHealth}/{currentCharacter.maxHealth}";
        }
        
        if (manaText != null)
        {
            manaText.text = $"{currentCharacter.currentMana}/{currentCharacter.maxMana}";
        }
    }
    
    /// <summary>
    /// 更新技能UI
    /// </summary>
    // private void UpdateSkillUI()
    // {
    //     if (currentCharacter == null || skillButtons == null) return;
        
    //     // 根据角色类型更新技能UI
    //     if (currentCharacter is Warrior warrior)
    //     {
    //         UpdateWarriorSkillUI(warrior);
    //     }
    //     else if (currentCharacter is Mage mage)
    //     {
    //         UpdateMageSkillUI(mage);
    //     }
    //     else if (currentCharacter is Archer archer)
    //     {
    //         UpdateArcherSkillUI(archer);
    //     }
    // }
    
    /// <summary>
    /// 更新战士技能UI
    /// </summary>
    // private void UpdateWarriorSkillUI(Warrior warrior)
    // {
    //     var skillStatus = warrior.GetSkillStatus();
        
    //     // 更新技能按钮状态
    //     if (skillButtons.Length > 0 && skillButtons[0] != null)
    //     {
    //         skillButtons[0].interactable = skillStatus.canUseHeavySlash;
    //     }
        
    //     if (skillButtons.Length > 1 && skillButtons[1] != null)
    //     {
    //         skillButtons[1].interactable = skillStatus.canUseWhirlwind;
    //     }
        
    //     if (skillButtons.Length > 2 && skillButtons[2] != null)
    //     {
    //         skillButtons[2].interactable = skillStatus.canUseBattleCry;
    //     }
    // }
    


    /// <summary>
    /// 使用技能
    /// </summary>
    // private void UseSkill(int skillIndex)
    // {
    //     if (currentCharacter == null) return;
        
    //     // 播放UI音效
    //     if (AudioManager.Instance != null)
    //     {
    //         AudioManager.Instance.PlaySFX("ui_click");
    //     }
        
    //     // 根据角色类型执行技能
    //     if (currentCharacter is Warrior warrior)
    //     {
    //         UseWarriorSkill(warrior, skillIndex);
    //     }
    //     else if (currentCharacter is Mage mage)
    //     {
    //         UseMageSkill(mage, skillIndex);
    //     }
    //     else if (currentCharacter is Archer archer)
    //     {
    //         UseArcherSkill(archer, skillIndex);
    //     }
    // }
    
    /// <summary>
    /// 使用战士技能
    /// </summary>
    // private void UseWarriorSkill(Warrior warrior, int skillIndex)
    // {
    //     switch (skillIndex)
    //     {
    //         case 0:
    //             warrior.PerformHeavySlash();
    //             break;
    //         case 1:
    //             warrior.PerformWhirlwind();
    //             break;
    //         case 2:
    //             warrior.PerformBattleCry();
    //             break;
    //     }
    // }
    



    // Duplicate methods removed - using earlier definitions
    
    /// <summary>
    /// 处理UI输入
    /// </summary>
    private void HandleUIInput()
    {
        // ESC键处理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (uiStack.Count > 1) // 有多个面板时，关闭当前面板
            {
                HideCurrentPanel();
            }
            else if (GameManager.Instance != null) // 只有一个面板时，暂停游戏
            {
                if (GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.PauseGame();
                    ShowPanel("PauseMenu");
                }
                else if (GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.ResumeGame();
                    HideCurrentPanel();
                }
            }
        }
        
        // 快捷键
        if (Input.GetKeyDown(KeyCode.I))
        {
            TogglePanel("Inventory");
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            TogglePanel("Character");
        }
    }
    
    /// <summary>
    /// 游戏开始时的UI设置
    /// </summary>
    public void OnGameStart()
    {
        HideAllPanels();
        ShowPanel("Gameplay");
    }
    
    /// <summary>
    /// 游戏结束时的UI设置
    /// </summary>
    public void OnGameOver()
    {
        HideAllPanels();
        ShowPanel("GameOver");
    }
    
    /// <summary>
    /// 游戏暂停时的UI设置
    /// </summary>
    public void OnGamePause()
    {
        ShowPanel("PauseMenu");
    }
    
    /// <summary>
    /// 游戏恢复时的UI设置
    /// </summary>
    public void OnGameResume()
    {
        // 如果当前显示的是暂停菜单，则隐藏它
        if (uiStack.Count > 0 && uiStack.Peek() == pauseMenuPanel)
        {
            HideCurrentPanel();
        }
    }
    
    /// <summary>
    /// 显示商店UI
    /// </summary>
    // public void ShowShopUI(Shop shop)
    // {
    //     // 这里应该显示商店界面
    //     // 由于没有具体的商店UI面板，暂时显示消息
    //     ShowMessage($"打开商店: {shop.shopName}");
    // }
    
    /// <summary>
    /// 隐藏商店UI
    /// </summary>
    public void HideShopUI()
    {
        // 这里应该隐藏商店界面
        // 由于没有具体的商店UI面板，暂时显示消息
        ShowMessage("关闭商店");
    }
}