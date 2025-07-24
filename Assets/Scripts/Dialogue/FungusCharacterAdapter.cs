using UnityEngine;
using Fungus;

/// <summary>
/// Fungus角色适配器 - 连接项目角色系统与Fungus对话系统
/// </summary>
public class FungusCharacterAdapter : MonoBehaviour
{
    [Header("Fungus角色配置")]
    [Tooltip("Fungus对话系统中的角色组件")]
    public Fungus.Character fungusCharacter;
    
    [Header("项目角色引用")]
    [Tooltip("项目中的角色组件")]
    public Character projectCharacter;
    
    [Header("对话配置")]
    [Tooltip("默认对话流程图名称")]
    public Flowchart defaultFlowchart;
    
    [Tooltip("默认开始对话块名称")]
    public string defaultStartBlock = "Start";
    
    void Start()
    {
        InitializeCharacterSync();
    }
    
    /// <summary>
    /// 初始化角色信息同步
    /// </summary>
    private void InitializeCharacterSync()
    {
        if (fungusCharacter != null && projectCharacter != null)
        {
            // 同步角色名称
            if (string.IsNullOrEmpty(fungusCharacter.NameText))
            {
                // 如果Fungus角色没有设置名称，使用项目角色的名称
                // 注意：这需要修改Fungus.Character的nameText字段，或者通过反射设置
                Debug.Log($"同步角色名称: {projectCharacter.name}");
            }
        }
    }
    
    /// <summary>
    /// 开始对话
    /// </summary>
    /// <param name="flowchartName">流程图名称，为空则使用默认值</param>
    /// <param name="blockName">对话块名称，为空则使用默认值</param>
    public void StartDialogue(Flowchart flowchart = null, string blockName = null)
    {
        string targetBlock = string.IsNullOrEmpty(blockName) ? defaultStartBlock : blockName;

        if (flowchart ?? defaultFlowchart != null)
        {
            // 设置当前说话角色
            if (fungusCharacter != null)
            {
                // 这里可以设置当前对话角色的上下文
                Debug.Log($"开始与 {fungusCharacter.NameText} 的对话");
            }
            
            flowchart.ExecuteBlock(targetBlock);
        }
        else
        {
        }
    }
    
    /// <summary>
    /// 查找指定名称的Flowchart
    /// </summary>
    private Flowchart FindFlowchart(string flowchartName)
    {
        // 首先尝试在当前GameObject上查找
        var flowchart = GetComponent<Flowchart>();
        if (flowchart != null && flowchart.name == flowchartName)
        {
            return flowchart;
        }
        
        // 在场景中查找指定名称的Flowchart
        var flowchartObject = GameObject.Find(flowchartName);
        if (flowchartObject != null)
        {
            return flowchartObject.GetComponent<Flowchart>();
        }
        
        // 查找所有Flowchart组件
        var allFlowcharts = FindObjectsOfType<Flowchart>();
        foreach (var fc in allFlowcharts)
        {
            if (fc.name == flowchartName || fc.gameObject.name == flowchartName)
            {
                return fc;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 检查是否可以开始对话
    /// </summary>
    public bool CanStartDialogue()
    {
        // 检查游戏状态
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.CurrentState == GameState.Playing;
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取角色信息用于对话显示
    /// </summary>
    public CharacterInfo GetCharacterInfo()
    {
        var info = new CharacterInfo();
        
        if (fungusCharacter != null)
        {
            info.name = fungusCharacter.NameText;
            info.nameColor = fungusCharacter.NameColor;
            info.soundEffect = fungusCharacter.SoundEffect;
        }
        
        if (projectCharacter != null)
        {
            info.level = projectCharacter.level;
            info.characterClass = projectCharacter.CHARACTERCLASS.ToString();
        }
        
        return info;
    }
}

/// <summary>
/// 角色信息结构体
/// </summary>
[System.Serializable]
public struct CharacterInfo
{
    public string name;
    public Color nameColor;
    public AudioClip soundEffect;
    public int level;
    public string characterClass;
}