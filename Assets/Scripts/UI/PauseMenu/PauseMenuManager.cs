using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

/// <summary>
/// 暂停菜单管理器 - 管理暂停菜单的显示、隐藏和标签页切换
/// 设计为高扩展性、低耦合的模块化系统
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Transform tabButtonContainer;
    [SerializeField] private Transform contentContainer;
    
    [Header("标签页配置")]
    [SerializeField] private List<PauseMenuTab> tabs = new List<PauseMenuTab>();
    
    [Header("按钮引用")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button exitButton;
    
    // 当前激活的标签页
    private PauseMenuTab currentTab;
    private int currentTabIndex = 0;
    
    // 标签页内容管理
    private Dictionary<string, IPauseMenuContent> contentPanels = new Dictionary<string, IPauseMenuContent>();
    
    // 事件
    public System.Action OnPauseMenuOpened;
    public System.Action OnPauseMenuClosed;
    public System.Action<string> OnTabChanged;
    
    private void Awake()
    {
        InitializeButtons();
        InitializeTabs();
        RegisterInputHandlers();
        
   
    }
    
    private void Start()
    {
        // 默认隐藏暂停菜单
        pauseMenuPanel.SetActive(false);
    }
    
    private void Update()
    {
        HandleInput();
    }
 
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // if (Input.GetKeyDown(KeyCode.Escape))
        // {
        //     Debug.Log("[PauseMenuManager] 检测到ESC键按下");
        //     TogglePauseMenu();
        // }
        
        // 标签页快捷键切换（1-7数字键）
        for (int i = 0; i < tabs.Count && i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (pauseMenuPanel.activeInHierarchy)
                {
                    SwitchToTab(i);
                }
            }
        }
    }
    
    /// <summary>
    /// 切换暂停菜单显示状态
    /// </summary>
    public void TogglePauseMenu()
    {
        Debug.Log($"[PauseMenuManager] 切换暂停菜单状态，当前状态: {pauseMenuPanel.activeInHierarchy}");
        
        if (pauseMenuPanel.activeInHierarchy)
        {
            Debug.Log("[PauseMenuManager] 关闭暂停菜单");
            ClosePauseMenu();
        }
        else
        {
            Debug.Log("[PauseMenuManager] 打开暂停菜单");
            OpenPauseMenu();
        }
    }
    
    /// <summary>
    /// 打开暂停菜单
    /// </summary>
    public void OpenPauseMenu()
    {
        Debug.Log("[PauseMenuManager] 尝试打开暂停菜单");
        
        if (pauseMenuPanel == null)
        {
            Debug.LogError("[PauseMenuManager] pauseMenuPanel为null，无法打开暂停菜单！");
            return;
        }
        
        pauseMenuPanel.SetActive(true);
        Debug.Log("[PauseMenuManager] 暂停菜单面板已激活");
        
        // 暂停游戏
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPaused(true);
            Debug.Log("[PauseMenuManager] 游戏已暂停");
        }
        else
        {
            Debug.LogWarning("[PauseMenuManager] GameManager.Instance为null，无法暂停游戏");
            // 即使GameManager不存在，也直接设置timeScale
            Time.timeScale = 0f;
        }
        
        // 切换到默认标签页
        if (tabs.Count > 0)
        {
            SwitchToTab(0);
        }
        
        // 刷新所有内容
        RefreshAllContent();
        
        OnPauseMenuOpened?.Invoke();
        Debug.Log("[PauseMenuManager] 暂停菜单打开完成");
    }
    
    /// <summary>
    /// 关闭暂停菜单
    /// </summary>
    public void ClosePauseMenu()
    {
        pauseMenuPanel.SetActive(false);
        
        // 恢复游戏
        if (GamePauseManager.Instance != null)
        {
            GamePauseManager.Instance.SetPaused(false);
        }
        
        OnPauseMenuClosed?.Invoke();
    }
    
    /// <summary>
    /// 切换到指定标签页
    /// </summary>
    public void SwitchToTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex >= tabs.Count) return;
        
        // 隐藏当前标签页内容
        if (currentTab != null)
        {
            HideTabContent(currentTab.tabId);
        }
        
        // 更新标签页状态
        UpdateTabButtons(tabIndex);
        
        // 显示新标签页内容
        currentTab = tabs[tabIndex];
        currentTabIndex = tabIndex;
        ShowTabContent(currentTab.tabId);
        
        OnTabChanged?.Invoke(currentTab.tabId);
    }
    
    /// <summary>
    /// 切换到指定标签页（通过ID）
    /// </summary>
    public void SwitchToTab(string tabId)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].tabId == tabId)
            {
                SwitchToTab(i);
                break;
            }
        }
    }
    
    /// <summary>
    /// 注册标签页内容
    /// </summary>
    public void RegisterTabContent(string tabId, IPauseMenuContent content)
    {
        if (!contentPanels.ContainsKey(tabId))
        {
            contentPanels.Add(tabId, content);
        }
        else
        {
            contentPanels[tabId] = content;
        }
    }
    
    /// <summary>
    /// 刷新所有内容
    /// </summary>
    private void RefreshAllContent()
    {
        foreach (var content in contentPanels.Values)
        {
            content.RefreshContent();
        }
    }
    
    /// <summary>
    /// 显示标签页内容
    /// </summary>
    private void ShowTabContent(string tabId)
    {
        if (contentPanels.ContainsKey(tabId))
        {
            contentPanels[tabId].ShowContent();
        }
    }
    
    /// <summary>
    /// 隐藏标签页内容
    /// </summary>
    private void HideTabContent(string tabId)
    {
        if (contentPanels.ContainsKey(tabId))
        {
            contentPanels[tabId].HideContent();
        }
    }
    
    /// <summary>
    /// 更新标签页按钮状态
    /// </summary>
    private void UpdateTabButtons(int activeIndex)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (tabs[i].tabButton != null)
            {
                // 更新按钮视觉状态
                tabs[i].tabButton.interactable = (i != activeIndex);
                
                // 可以在这里添加更多的视觉效果，如颜色变化等
                var colors = tabs[i].tabButton.colors;
                if (i == activeIndex)
                {
                    colors.normalColor = Color.yellow; // 激活状态颜色
                }
                else
                {
                    colors.normalColor = Color.white; // 普通状态颜色
                }
                tabs[i].tabButton.colors = colors;
            }
        }
    }
    
    /// <summary>
    /// 初始化按钮事件
    /// </summary>
    private void InitializeButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ClosePauseMenu);
        }
        
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveGame);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }
    
    /// <summary>
    /// 初始化标签页
    /// </summary>
    private void InitializeTabs()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int tabIndex = i; // 闭包变量
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => SwitchToTab(tabIndex));
                //初始化图片和文字
                tabs[i].tabButton.GetComponent<Image>().sprite = tabs[i].tabIcon;
                 tabs[i].tabButton.GetComponentInChildren<TextMeshProUGUI>().text = tabs[i].tabName;
            }
        }
    }
    
    /// <summary>
    /// 注册输入处理器
    /// </summary>
    private void RegisterInputHandlers()
    {
        // 如果有InputManager，可以在这里注册输入事件
        if (InputManager.Instance != null)
        {
            // InputManager.Instance.RegisterInputListener(this);
        }
    }
    
    /// <summary>
    /// 保存游戏
    /// </summary>
    private void SaveGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
        }
        
        // 显示保存成功提示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("游戏已保存");
        }
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    private void ExitGame()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowConfirmDialog(
                "确定要退出游戏吗？",
                () => {
                    // 确认退出
                    if (GameManager.Instance != null)
                    {
                      //  GameManager.Instance.ExitGame();
                    }
                    else
                    {
                        Application.Quit();
                    }
                },
                () => {
                    // 取消退出
                }
            );
        }
    }
    
    /// <summary>
    /// 获取当前标签页ID
    /// </summary>
    public string GetCurrentTabId()
    {
        return currentTab?.tabId ?? "";
    }
    
    /// <summary>
    /// 获取当前标签页索引
    /// </summary>
    public int GetCurrentTabIndex()
    {
        return currentTabIndex;
    }
}

/// <summary>
/// 暂停菜单标签页配置
/// </summary>
[System.Serializable]
public class PauseMenuTab
{
    [LabelText("标签页ID")]
    public string tabId;
    
    [LabelText("标签页名称")]
    public string tabName;
    
    [LabelText("标签页按钮")]
    public Button tabButton;
    
    [LabelText("标签页图标")]
    public Sprite tabIcon;
    
    [LabelText("是否启用")]
    public bool isEnabled = true;
}

/// <summary>
/// 暂停菜单内容接口
/// </summary>
public interface IPauseMenuContent
{
    void ShowContent();
    void HideContent();
    void RefreshContent();
}