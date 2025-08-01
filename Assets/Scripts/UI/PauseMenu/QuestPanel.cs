using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

/// <summary>
/// 任务面板 - 显示任务列表和详细信息
/// 实现IPauseMenuContent接口，可作为暂停菜单的一个标签页
/// </summary>
public class QuestPanel : MonoBehaviour, IPauseMenuContent
{
    [BoxGroup("主要面板")]
    [LabelText("任务面板")]
    public GameObject questPanel;
    
    [BoxGroup("任务列表")]
    [LabelText("任务列表容器")]
    public Transform questListContainer;
    
    [BoxGroup("任务列表")]
    [LabelText("任务项预制体")]
    public GameObject questItemPrefab;
    
    [BoxGroup("任务列表")]
    [LabelText("滚动视图")]
    public ScrollRect questScrollRect;
    
    [BoxGroup("分类按钮")]
    [LabelText("全部任务")]
    public Button allQuestsButton;
    
    [BoxGroup("分类按钮")]
    [LabelText("主线任务")]
    public Button mainQuestsButton;
    
    [BoxGroup("分类按钮")]
    [LabelText("支线任务")]
    public Button sideQuestsButton;
    
    [BoxGroup("分类按钮")]
    [LabelText("已完成任务")]
    public Button completedQuestsButton;
    
    [BoxGroup("任务详情")]
    [LabelText("详情面板")]
    public GameObject questDetailPanel;
    
    [BoxGroup("任务详情")]
    [LabelText("任务标题")]
    public TextMeshProUGUI questTitleText;
    
    [BoxGroup("任务详情")]
    [LabelText("任务描述")]
    public TextMeshProUGUI questDescriptionText;
    
    [BoxGroup("任务详情")]
    [LabelText("任务类型")]
    public TextMeshProUGUI questTypeText;
    
    [BoxGroup("任务详情")]
    [LabelText("任务图标")]
    public Image questIconImage;
    
    [BoxGroup("任务详情")]
    [LabelText("进度滑动条")]
    public Slider questProgressSlider;
    
    [BoxGroup("任务详情")]
    [LabelText("进度文本")]
    public TextMeshProUGUI questProgressText;
    
    [BoxGroup("任务详情")]
    [LabelText("目标列表容器")]
    public Transform objectiveListContainer;
    
    [BoxGroup("任务详情")]
    [LabelText("目标项预制体")]
    public GameObject objectiveItemPrefab;
    
    [BoxGroup("操作按钮")]
    [LabelText("放弃任务")]
    public Button abandonQuestButton;
    
    [BoxGroup("操作按钮")]
    [LabelText("追踪任务")]
    public Button trackQuestButton;
    
    // 当前显示的任务类型
    private QuestDisplayType currentDisplayType = QuestDisplayType.All;
    
    // 当前选中的任务
    private QuestData selectedQuest;
    
    // 任务项UI缓存
    private List<QuestItemUI> questItemUIs = new List<QuestItemUI>();
    
    // 目标项UI缓存
    private List<ObjectiveItemUI> objectiveItemUIs = new List<ObjectiveItemUI>();
    
    private void Awake()
    {
        RegisterToPauseMenu();
        InitializeButtons();
    }
    
    private void Start()
    {
        // 默认隐藏面板
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
        
        // 隐藏详情面板
        if (questDetailPanel != null)
        {
            questDetailPanel.SetActive(false);
        }
        
        // 注册任务管理器事件
        RegisterQuestEvents();
    }
    
    private void OnDestroy()
    {
        UnregisterQuestEvents();
    }
    
    /// <summary>
    /// 注册到暂停菜单管理器
    /// </summary>
    private void RegisterToPauseMenu()
    {
        var pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        if (pauseMenuManager != null)
        {
            pauseMenuManager.RegisterTabContent("Quests", this);
        }
    }
    
    /// <summary>
    /// 初始化按钮事件
    /// </summary>
    private void InitializeButtons()
    {
        if (allQuestsButton != null)
        {
            allQuestsButton.onClick.AddListener(() => SetDisplayType(QuestDisplayType.All));
        }
        
        if (mainQuestsButton != null)
        {
            mainQuestsButton.onClick.AddListener(() => SetDisplayType(QuestDisplayType.Main));
        }
        
        if (sideQuestsButton != null)
        {
            sideQuestsButton.onClick.AddListener(() => SetDisplayType(QuestDisplayType.Side));
        }
        
        if (completedQuestsButton != null)
        {
            completedQuestsButton.onClick.AddListener(() => SetDisplayType(QuestDisplayType.Completed));
        }
        
        if (abandonQuestButton != null)
        {
            abandonQuestButton.onClick.AddListener(AbandonSelectedQuest);
        }
        
        if (trackQuestButton != null)
        {
            trackQuestButton.onClick.AddListener(ToggleQuestTracking);
        }
    }
    
    /// <summary>
    /// 注册任务管理器事件
    /// </summary>
    private void RegisterQuestEvents()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted += OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
            QuestManager.Instance.OnQuestProgressUpdated += OnQuestProgressUpdated;
        }
    }
    
    /// <summary>
    /// 取消注册任务管理器事件
    /// </summary>
    private void UnregisterQuestEvents()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStarted -= OnQuestStarted;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
            QuestManager.Instance.OnQuestProgressUpdated -= OnQuestProgressUpdated;
        }
    }
    
    /// <summary>
    /// 显示内容
    /// </summary>
    public void ShowContent()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(true);
        }
        RefreshContent();
    }
    
    /// <summary>
    /// 隐藏内容
    /// </summary>
    public void HideContent()
    {
        if (questPanel != null)
        {
            questPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 刷新内容
    /// </summary>
    public void RefreshContent()
    {
        RefreshQuestList();
        RefreshQuestDetail();
        UpdateCategoryButtons();
    }
    
    /// <summary>
    /// 设置显示类型
    /// </summary>
    public void SetDisplayType(QuestDisplayType displayType)
    {
        currentDisplayType = displayType;
        RefreshQuestList();
        UpdateCategoryButtons();
    }
    
    /// <summary>
    /// 刷新任务列表
    /// </summary>
    private void RefreshQuestList()
    {
        if (QuestManager.Instance == null) return;
        
        // 获取要显示的任务列表
        List<QuestData> questsToShow = GetQuestsToShow();
        
        // 清理现有的UI项
        ClearQuestItems();
        
        // 创建新的UI项
        foreach (var quest in questsToShow)
        {
            CreateQuestItem(quest);
        }
    }
    
    /// <summary>
    /// 获取要显示的任务列表
    /// </summary>
    private List<QuestData> GetQuestsToShow()
    {
        if (QuestManager.Instance == null) return new List<QuestData>();
        
        switch (currentDisplayType)
        {
            case QuestDisplayType.All:
                return QuestManager.Instance.GetActiveQuests();
            
            case QuestDisplayType.Main:
                return QuestManager.Instance.GetQuestsByType(QuestType.MainQuest);
            
            case QuestDisplayType.Side:
                return QuestManager.Instance.GetQuestsByType(QuestType.SideQuest);
            
            case QuestDisplayType.Completed:
                return QuestManager.Instance.GetCompletedQuests();
            
            default:
                return new List<QuestData>();
        }
    }
    
    /// <summary>
    /// 清理任务项UI
    /// </summary>
    private void ClearQuestItems()
    {
        foreach (var questItem in questItemUIs)
        {
            if (questItem != null && questItem.gameObject != null)
            {
                Destroy(questItem.gameObject);
            }
        }
        questItemUIs.Clear();
    }
    
    /// <summary>
    /// 创建任务项UI
    /// </summary>
    private void CreateQuestItem(QuestData quest)
    {
        if (questItemPrefab == null || questListContainer == null) return;
        
        GameObject questItemObj = Instantiate(questItemPrefab, questListContainer);
        QuestItemUI questItemUI = questItemObj.GetComponent<QuestItemUI>();
        
        if (questItemUI == null)
        {
            questItemUI = questItemObj.AddComponent<QuestItemUI>();
        }
        
        questItemUI.Initialize(quest, OnQuestItemClicked);
        questItemUIs.Add(questItemUI);
    }
    
    /// <summary>
    /// 任务项点击回调
    /// </summary>
    private void OnQuestItemClicked(QuestData quest)
    {
        selectedQuest = quest;
        RefreshQuestDetail();
    }
    
    /// <summary>
    /// 刷新任务详情
    /// </summary>
    private void RefreshQuestDetail()
    {
        if (selectedQuest == null)
        {
            if (questDetailPanel != null)
            {
                questDetailPanel.SetActive(false);
            }
            return;
        }
        
        if (questDetailPanel != null)
        {
            questDetailPanel.SetActive(true);
        }
        
        // 更新任务基本信息
        if (questTitleText != null)
        {
            questTitleText.text = selectedQuest.questName;
        }
        
        if (questDescriptionText != null)
        {
            questDescriptionText.text = selectedQuest.description;
        }
        
        if (questTypeText != null)
        {
            questTypeText.text = GetQuestTypeText(selectedQuest.questType);
        }
        
        if (questIconImage != null && selectedQuest.questIcon != null)
        {
            questIconImage.sprite = selectedQuest.questIcon;
        }
        
        // 更新进度
        if (questProgressSlider != null)
        {
            float progress = selectedQuest.GetProgressPercentage();
            questProgressSlider.value = progress;
        }
        
        if (questProgressText != null)
        {
            float progress = selectedQuest.GetProgressPercentage();
            questProgressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
        
        // 更新目标列表
        RefreshObjectiveList();
        
        // 更新操作按钮
        UpdateActionButtons();
    }
    
    /// <summary>
    /// 刷新目标列表
    /// </summary>
    private void RefreshObjectiveList()
    {
        if (selectedQuest == null || objectiveListContainer == null) return;
        
        // 清理现有目标UI
        ClearObjectiveItems();
        
        // 创建新的目标UI
        foreach (var objective in selectedQuest.objectives)
        {
            CreateObjectiveItem(objective);
        }
    }
    
    /// <summary>
    /// 清理目标项UI
    /// </summary>
    private void ClearObjectiveItems()
    {
        foreach (var objectiveItem in objectiveItemUIs)
        {
            if (objectiveItem != null && objectiveItem.gameObject != null)
            {
                Destroy(objectiveItem.gameObject);
            }
        }
        objectiveItemUIs.Clear();
    }
    
    /// <summary>
    /// 创建目标项UI
    /// </summary>
    private void CreateObjectiveItem(QuestObjective objective)
    {
        if (objectiveItemPrefab == null || objectiveListContainer == null) return;
        
        GameObject objectiveItemObj = Instantiate(objectiveItemPrefab, objectiveListContainer);
        ObjectiveItemUI objectiveItemUI = objectiveItemObj.GetComponent<ObjectiveItemUI>();
        
        if (objectiveItemUI == null)
        {
            objectiveItemUI = objectiveItemObj.AddComponent<ObjectiveItemUI>();
        }
        
        objectiveItemUI.Initialize(objective);
        objectiveItemUIs.Add(objectiveItemUI);
    }
    
    /// <summary>
    /// 更新操作按钮
    /// </summary>
    private void UpdateActionButtons()
    {
        if (selectedQuest == null) return;
        
        // 放弃任务按钮
        if (abandonQuestButton != null)
        {
            abandonQuestButton.gameObject.SetActive(selectedQuest.questStatus == QuestStatus.InProgress);
        }
        
        // 追踪任务按钮
        if (trackQuestButton != null)
        {
            trackQuestButton.gameObject.SetActive(selectedQuest.questStatus == QuestStatus.InProgress);
        }
    }
    
    /// <summary>
    /// 更新分类按钮状态
    /// </summary>
    private void UpdateCategoryButtons()
    {
        // 更新按钮的视觉状态
        UpdateButtonState(allQuestsButton, currentDisplayType == QuestDisplayType.All);
        UpdateButtonState(mainQuestsButton, currentDisplayType == QuestDisplayType.Main);
        UpdateButtonState(sideQuestsButton, currentDisplayType == QuestDisplayType.Side);
        UpdateButtonState(completedQuestsButton, currentDisplayType == QuestDisplayType.Completed);
    }
    
    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonState(Button button, bool isActive)
    {
        if (button == null) return;
        
        var colors = button.colors;
        if (isActive)
        {
            colors.normalColor = Color.yellow;
        }
        else
        {
            colors.normalColor = Color.white;
        }
        button.colors = colors;
    }
    
    /// <summary>
    /// 放弃选中的任务
    /// </summary>
    private void AbandonSelectedQuest()
    {
        if (selectedQuest == null) return;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowConfirmDialog(
                $"确定要放弃任务 \"{selectedQuest.questName}\" 吗？",
                () => {
                    QuestManager.Instance?.AbandonQuest(selectedQuest.questId);
                    selectedQuest = null;
                    RefreshContent();
                },
                () => {
                    // 取消操作
                }
            );
        }
        else
        {
            QuestManager.Instance?.AbandonQuest(selectedQuest.questId);
            selectedQuest = null;
            RefreshContent();
        }
    }
    
    /// <summary>
    /// 切换任务追踪状态
    /// </summary>
    private void ToggleQuestTracking()
    {
        if (selectedQuest == null) return;
        
        // 这里可以实现任务追踪功能
        Debug.Log($"切换任务追踪: {selectedQuest.questName}");
    }
    
    /// <summary>
    /// 获取任务类型文本
    /// </summary>
    private string GetQuestTypeText(QuestType questType)
    {
        switch (questType)
        {
            case QuestType.MainQuest: return "主线任务";
            case QuestType.SideQuest: return "支线任务";
            default: return "未知任务";
        }
    }
    
    // 任务管理器事件回调
    private void OnQuestStarted(QuestData quest)
    {
        RefreshQuestList();
    }
    
    private void OnQuestCompleted(QuestData quest)
    {
        RefreshContent();
    }
    
    private void OnQuestProgressUpdated(QuestData quest)
    {
        if (selectedQuest != null && selectedQuest.questId == quest.questId)
        {
            RefreshQuestDetail();
        }
    }
}

/// <summary>
/// 任务显示类型
/// </summary>
public enum QuestDisplayType
{
    All,        // 所有活跃任务
    Main,       // 主线任务
    Side,       // 支线任务
    Completed   // 已完成任务
}


