using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 狼配置
/// </summary>
[System.Serializable]
[InlineProperty]
[CreateAssetMenu(fileName = "CaptainBanditsConfig", menuName = "Enemy/CaptainBanditsConfig")]
public class CaptainBanditsConfig : EnemyConfig
{
    [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("后撤步触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发后撤步技能。建议设置为保持距离的1.5-2倍，避免与反向移动冲突")]
    public float backChargeDistance = 6f;
    
   [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("飞刀触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发飞刀技能。建议设置为保持距离的1.5-2倍，避免与反向移动冲突")]
    public float knifeDinstance = 12f;




#if UNITY_EDITOR
    private Color GetHealthBarColor()
    {
        if (health <= 25) return Color.red;
        if (health <= 50) return Color.yellow;
        return Color.green;
    }
#endif
}

