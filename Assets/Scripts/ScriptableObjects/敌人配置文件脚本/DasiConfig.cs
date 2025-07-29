using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 神秘人配置
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "DasiConfig", menuName = "Enemy/Dasi Config")]
[ShowOdinSerializedPropertiesInInspector]
public class DasiConfig : EnemyConfig
{
    [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("二段斩触发距离")]
    [PropertyRange(1f, 15f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发二段斩技能")]
    public float twoSwingSkillTriggerDistance = 8f;
    
   [FoldoutGroup("技能触发条件", expanded: true)]
    [LabelText("能量球触发距离")]
    [PropertyRange(1f, 20f)]
    [SuffixLabel("米")]
    [PropertyOrder(6)]
    [InfoBox("当玩家在此距离内时可能触发能量球技能")]
    public float energyBallTriggerDistance = 14f;




#if UNITY_EDITOR
    private Color GetHealthBarColor()
    {
        if (health <= 25) return Color.red;
        if (health <= 50) return Color.yellow;
        return Color.green;
    }
#endif
}

