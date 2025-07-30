using Fungus;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC配置文件 - ScriptableObject
/// 用于配置NPC的通用参数
/// </summary>
[CreateAssetMenu(fileName = "NPCConfig", menuName = "Game/NPC Config")]
public class NPCConfig : ScriptableObject
{
    [BoxGroup("标识信息", Order = 0)]
    [LabelText("NPC ID")]
    [InfoBox("唯一标识符，用于任务系统和游戏逻辑识别")]
    public string npcId = "";

    [BoxGroup("标识信息")]
    [LabelText("NPC名称")]
    public string npcName = "NPC";

    [BoxGroup("标识信息")]
    [LabelText("NPC角色类型")]
    public NPCType npcType = NPCType.Villager;

    [BoxGroup("任务相关配置", VisibleIf = "@npcType == NPCType.QuestGiver")]
    [LabelText("可分发任务")]
    [InfoBox("此NPC可以分发的任务列表")]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    [SerializeField] public List<QuestData> availableQuests = new List<QuestData>();
    [BoxGroup("标识信息")]
    [LabelText("NPC描述")]
    [TextArea(3, 5)]
    public string description = "";

    [BoxGroup("交互设置")]
    [LabelText("交互范围")]
    public float interactionRange = 2f;

    [BoxGroup("交互设置")]
    [LabelText("交互提示偏移")]
    public Vector3 promptOffset = new Vector3(0, 2, 0);

    [BoxGroup("对话设置")]
    [LabelText("初次对话Block名称")]
    public string firstDialogueBlock = "Start";

    [BoxGroup("对话设置")]
    [LabelText("重复对话Block名称")]
    public string repeatDialogueBlock = "Repeat";





    [BoxGroup("功能设置")]
    [LabelText("可以交易")]
    public bool canTrade = false;

    [BoxGroup("功能设置")]
    [LabelText("可分发任务")]
    public bool canGiveQuests = false;

    [BoxGroup("功能设置")]
    [LabelText("可完成任务")]
    public bool canCompleteQuests = false;

    [BoxGroup("功能设置")]
    [LabelText("提供服务")]
    [ShowIf("@canTrade || canGiveQuests || canCompleteQuests")]
    public List<NPCService> services = new List<NPCService>();

    [BoxGroup("任务相关配置")]
    [LabelText("可用于对话任务")]
    public bool canBeTalkTarget = true;



    /// <summary>
    /// 验证配置
    /// </summary>
    public bool ValidateConfig()
    {
        if (string.IsNullOrEmpty(npcId))
        {
            Debug.LogError($"NPC配置缺少ID: {name}");
            return false;
        }

        if (string.IsNullOrEmpty(npcName))
        {
            Debug.LogError($"NPC配置缺少名称: {npcId}");
            return false;
        }

        if (interactionRange <= 0)
        {
            Debug.LogError($"NPC配置交互范围无效: {npcId}");
            return false;
        }

        return true;
    }

    /// <summary>



    /// <summary>
    /// 检查是否可以交易指定物品
    /// </summary>
    public bool CanTradeItem(string itemId)
    {
        return canTrade && services.Contains(NPCService.ItemTrading);
    }

    /// <summary>
    /// 检查是否提供指定服务
    /// </summary>
    public bool ProvidesService(NPCService service)
    {
        return services.Contains(service);
    }
}


/// <summary>
/// NPC服务类型枚举
/// </summary>
public enum NPCService
{
    [LabelText("物品交易")]
    ItemTrading,
    [LabelText("装备修理")]
    EquipmentRepair,
    [LabelText("技能训练")]
    SkillTraining,
    [LabelText("任务发布")]
    QuestGiving,
    [LabelText("任务完成")]
    QuestCompletion,
    [LabelText("信息提供")]
    InformationProvider,
    [LabelText("传送服务")]
    Teleportation,
    [LabelText("银行服务")]
    Banking,
    [LabelText("治疗服务")]
    Healing
}

