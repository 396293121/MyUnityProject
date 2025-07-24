using UnityEngine;

/// <summary>
/// NPC配置文件 - ScriptableObject
/// 用于配置NPC的通用参数
/// </summary>
[CreateAssetMenu(fileName = "NPCConfig", menuName = "Game/NPC Config")]
public class NPCConfig : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("NPC名称")]
    public string npcName = "NPC";
       [Tooltip("NPC角色类型")]
      public NPCType npcType = NPCType.Villager;
    [Tooltip("NPC描述")]
    [TextArea(3, 5)]
    public string description = "";
    
    [Header("交互设置")]
    [Tooltip("交互范围")]
    public float interactionRange = 2f;
         [Tooltip("交互提示偏移")]
    public Vector3 promptOffset = new Vector3(0, 2, 0);
    
    [Tooltip("初次对话Block名称")]
    public string firstDialogueBlock = "Start";
    
    [Tooltip("重复对话Block名称")]
    public string repeatDialogueBlock = "Repeat";
}