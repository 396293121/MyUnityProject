using UnityEngine;

/// <summary>
/// 任务ID选择器属性
/// 用于在Inspector中显示任务下拉框选择器
/// </summary>
public class QuestIdAttribute : PropertyAttribute
{
    public bool includeEmpty;
    public string emptyLabel;
    
    public QuestIdAttribute(bool includeEmpty = true, string emptyLabel = "无任务")
    {
        this.includeEmpty = includeEmpty;
        this.emptyLabel = emptyLabel;
    }
}