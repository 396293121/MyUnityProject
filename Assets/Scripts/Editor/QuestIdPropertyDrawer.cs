#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 任务ID选择器的自定义属性绘制器
/// 在Inspector中显示任务下拉框
/// </summary>
[CustomPropertyDrawer(typeof(QuestIdAttribute))]
public class QuestIdPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        QuestIdAttribute questIdAttribute = (QuestIdAttribute)attribute;
        
        // 确保属性是字符串类型
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "QuestId属性只能用于字符串字段");
            return;
        }
        
        // 获取所有可用的任务
        List<QuestData> allQuests = GetAllQuests();
        List<string> questOptions = new List<string>();
        List<string> questIds = new List<string>();
        
        // 添加空选项
        if (questIdAttribute.includeEmpty)
        {
            questOptions.Add(questIdAttribute.emptyLabel);
            questIds.Add("");
        }
        
        // 添加所有任务选项
        foreach (var quest in allQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.questId))
            {
                string displayName = $"{quest.questName} ({quest.questId})";
                questOptions.Add(displayName);
                questIds.Add(quest.questId);
            }
        }
        
        // 如果没有找到任务，显示警告
        if (questOptions.Count == (questIdAttribute.includeEmpty ? 1 : 0))
        {
            questOptions.Add("未找到任务数据");
            questIds.Add("");
        }
        
        // 找到当前选中的索引
        int selectedIndex = 0;
        string currentValue = property.stringValue;
        for (int i = 0; i < questIds.Count; i++)
        {
            if (questIds[i] == currentValue)
            {
                selectedIndex = i;
                break;
            }
        }
        
        // 绘制下拉框
        EditorGUI.BeginProperty(position, label, property);
        
        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, questOptions.ToArray());
        
        // 更新属性值
        if (newIndex >= 0 && newIndex < questIds.Count)
        {
            property.stringValue = questIds[newIndex];
        }
        
        EditorGUI.EndProperty();
    }
    
    /// <summary>
    /// 获取所有可用的任务数据
    /// </summary>
    private List<QuestData> GetAllQuests()
    {
        List<QuestData> allQuests = new List<QuestData>();
        
        // 方法1: 从QuestManager实例获取
        if (Application.isPlaying && QuestManager.Instance != null)
        {
            // 运行时从QuestManager获取
            var questManagerType = typeof(QuestManager);
            var allQuestsField = questManagerType.GetField("allQuests", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (allQuestsField != null)
            {
                var questsList = allQuestsField.GetValue(QuestManager.Instance) as List<QuestData>;
                if (questsList != null)
                {
                    allQuests.AddRange(questsList);
                }
            }
        }
        
        // 方法2: 从场景中的QuestManager组件获取
        if (allQuests.Count == 0)
        {
            QuestManager questManager = Object.FindObjectOfType<QuestManager>();
            if (questManager != null)
            {
                SerializedObject serializedQuestManager = new SerializedObject(questManager);
                SerializedProperty allQuestsProperty = serializedQuestManager.FindProperty("allQuests");
                
                if (allQuestsProperty != null && allQuestsProperty.isArray)
                {
                    for (int i = 0; i < allQuestsProperty.arraySize; i++)
                    {
                        SerializedProperty questProperty = allQuestsProperty.GetArrayElementAtIndex(i);
                        QuestData quest = questProperty.objectReferenceValue as QuestData;
                        if (quest != null)
                        {
                            allQuests.Add(quest);
                        }
                    }
                }
            }
        }
        
        // 方法3: 从项目资源中查找所有QuestData
        if (allQuests.Count == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:QuestData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(path);
                if (quest != null)
                {
                    allQuests.Add(quest);
                }
            }
        }
        
        // 按任务名称排序
        return allQuests.OrderBy(q => q.questName).ToList();
    }
}
#endif