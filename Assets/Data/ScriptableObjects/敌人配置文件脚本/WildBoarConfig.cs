using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 野猪配置
/// </summary>
[System.Serializable]
[InlineProperty]
[CreateAssetMenu(fileName = "WildBoarConfig", menuName = "Enemy/Wild Boar Config")]
public class WildBoarConfig : EnemyConfig
{
    [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("冲锋触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发冲锋技能")]
    public float chargeDistance = 6f;
    
    [FoldoutGroup("技能触发条件")]
    [LabelText("狂暴血量阈值")]
    [PropertyRange(0.1f, 0.8f)]
    [SuffixLabel("%")]
    [PropertyOrder(7)]
    [InfoBox("血量低于此百分比时触发狂暴技能")]
    public float enrageHealthThreshold = 0.3f;
    
    [FoldoutGroup("技能触发条件")]
    [LabelText("眩晕持续时间")]
    [PropertyRange(0.5f, 4f)]
    [SuffixLabel("秒")]
    [PropertyOrder(8)]
    [InfoBox("眩晕持续时间")]
    public float stunDuration = 2f;




#if UNITY_EDITOR
    private Color GetHealthBarColor()
    {
        if (health <= 25) return Color.red;
        if (health <= 50) return Color.yellow;
        return Color.green;
    }
#endif
}

