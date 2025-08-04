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
[CreateAssetMenu(fileName = "WolfConfig", menuName = "Enemy/wolf Config")]
public class WolfConfig : EnemyConfig
{
    [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("冲锋触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发冲锋技能。建议设置为保持距离的1.5-2倍，避免与反向移动冲突")]
    public float chargeDistance = 10f;
    
    [FoldoutGroup("技能触发条件")]
    [LabelText("召唤血量阈值")]
    [PropertyRange(0.1f, 0.8f)]
    [SuffixLabel("%")]
    [PropertyOrder(7)]
    [InfoBox("血量低于此百分比时触发召唤技能")]
    public float enrageHealthThreshold = 0.5f;




#if UNITY_EDITOR
    private Color GetHealthBarColor()
    {
        if (health <= 25) return Color.red;
        if (health <= 50) return Color.yellow;
        return Color.green;
    }
#endif
}

