using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 目标项UI组件 - 显示任务目标的进度和状态
/// </summary>
public class ObjectiveItemUI : MonoBehaviour
{
    [BoxGroup("UI组件")]
    [LabelText("目标描述文本")]
    public TextMeshProUGUI objectiveText;
    
    [BoxGroup("UI组件")]
    [LabelText("进度文本")]
    public TextMeshProUGUI progressText;
    
    [BoxGroup("UI组件")]
    [LabelText("完成图标")]
    public Image checkIcon;
    
    [ShowInInspector, ReadOnly]
    [LabelText("目标数据")]
    private QuestObjective objectiveData;

    public void Initialize(QuestObjective objective)
    {
        this.objectiveData = objective;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (objectiveData == null) return;
        
        if (objectiveText != null)
        {
            objectiveText.text = objectiveData.description;
        }
        
        if (progressText != null)
        {
            progressText.text = objectiveData.GetProgressText();
        }
        
        if (checkIcon != null)
        {
            checkIcon.gameObject.SetActive(objectiveData.isCompleted);
        }
    }
}