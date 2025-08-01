using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;

/// <summary>
/// 任务项UI组件 - 用于在任务列表中显示单个任务的信息
/// </summary>
public class QuestItemUI : MonoBehaviour
{
    [BoxGroup("UI组件")]
    [LabelText("任务名称文本")]
    public TextMeshProUGUI questNameText;
    
    [BoxGroup("UI组件")]
    [LabelText("任务类型文本")]
    public TextMeshProUGUI questTypeText;
    
    [BoxGroup("UI组件")]
    [LabelText("进度滑动条")]
    public Slider progressSlider;
    
    [BoxGroup("UI组件")]
    [LabelText("进度文本")]
    public TextMeshProUGUI progressText;
    
    [BoxGroup("UI组件")]
    [LabelText("任务图标")]
    public Image questIcon;
    
    [BoxGroup("UI组件")]
    [LabelText("选择按钮")]
    public Button selectButton;
    
    [BoxGroup("状态颜色")]
    [LabelText("进行中颜色")]
    [ColorUsage(false)]
    public Color activeColor = Color.yellow;
    
    [BoxGroup("状态颜色")]
    [LabelText("已完成颜色")]
    [ColorUsage(false)]
    public Color completedColor = Color.green;
    
    [BoxGroup("状态颜色")]
    [LabelText("失败颜色")]
    [ColorUsage(false)]
    public Color failedColor = Color.red;
    
    // 任务数据
    public QuestData QuestData { get; private set; }
    
    // 选中事件
    public event Action<QuestData> OnQuestSelected;
    
    private System.Action<QuestData> onClickCallback;
    
    void Start()
    {
        // 设置按钮事件
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
    }
    
    /// <summary>
    /// 初始化任务项
    /// </summary>
    public void Initialize(QuestData quest, System.Action<QuestData> onClickCallback = null)
    {
        QuestData = quest;
        this.onClickCallback = onClickCallback;
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// 更新显示
    /// </summary>
    public void UpdateDisplay()
    {
        if (QuestData == null) return;
        
        // 更新任务名称
        if (questNameText != null)
        {
            questNameText.text = QuestData.questName;
        }
        
        // 更新任务类型
        if (questTypeText != null)
        {
            questTypeText.text = GetQuestTypeDisplayName(QuestData.questType);
        }
        
        // 更新进度
        UpdateProgress();
        
        // 更新状态颜色
        UpdateStatusColor();
        
        // 更新图标
        UpdateQuestIcon();
    }
    
    /// <summary>
    /// 更新进度显示
    /// </summary>
    public void UpdateProgress()
    {
        if (QuestData == null)
            return;
        
        float progress = QuestData.GetProgressPercentage();
        
        // 更新进度条
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }
        
        // 更新进度文本
        if (progressText != null)
        {
            if (QuestData.questStatus == QuestStatus.Completed)
            {
                progressText.text = "已完成";
            }
            else if (QuestData.questStatus == QuestStatus.Failed)
            {
                progressText.text = "已失败";
            }
            else
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }
    }
    
    /// <summary>
    /// 更新状态颜色
    /// </summary>
    private void UpdateStatusColor()
    {
        Color statusColor = activeColor;
        
        switch (QuestData.questStatus)
        {
            case QuestStatus.InProgress:
                statusColor = activeColor;
                break;
            case QuestStatus.Completed:
                statusColor = completedColor;
                break;
            case QuestStatus.Failed:
                statusColor = failedColor;
                break;
        }
        
        // 应用颜色到进度条
        if (progressSlider != null && progressSlider.fillRect != null)
        {
            var fillImage = progressSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = statusColor;
            }
        }
        
        // 应用颜色到任务名称文本
        if (questNameText != null)
        {
            questNameText.color = statusColor;
        }
    }
    
    /// <summary>
    /// 更新任务图标
    /// </summary>
    private void UpdateQuestIcon()
    {
        if (questIcon == null || QuestData == null)
            return;
        
        // 如果任务数据中有图标，使用它
        if (QuestData.questIcon != null)
        {
            questIcon.sprite = QuestData.questIcon;
        }
        
        // 根据任务类型设置默认图标（如果没有自定义图标）
        // 这里可以根据需要加载不同的图标资源
    }
    
    /// <summary>
    /// 获取任务类型显示名称
    /// </summary>
    private string GetQuestTypeDisplayName(QuestType questType)
    {
        switch (questType)
        {
            case QuestType.MainQuest:
                return "主线任务";
            case QuestType.SideQuest:
                return "支线任务";
            default:
                return "未知任务";
        }
    }
    
    /// <summary>
    /// 选择按钮点击事件
    /// </summary>
    private void OnSelectButtonClicked()
    {
        OnQuestSelected?.Invoke(QuestData);
        onClickCallback?.Invoke(QuestData);
    }
    
    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        // 可以在这里添加选中状态的视觉效果
        if (selectButton != null)
        {
            var colors = selectButton.colors;
            if (selected)
            {
                colors.normalColor = Color.cyan;
            }
            else
            {
                colors.normalColor = Color.white;
            }
            selectButton.colors = colors;
        }
    }
}
