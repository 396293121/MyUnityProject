using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skills/Skill Data")]
public class SkillDataTest : ScriptableObject
{
    [TabGroup("基础信息")]
    [LabelText("技能名称")]
    public string skillName = "技能名称";
    
    [TabGroup("基础信息")]
    [PreviewField(100, ObjectFieldAlignment.Left)]
    [LabelText("技能图标")]
    public Sprite skillIcon;
    
    [TabGroup("基础信息")]
    [TextArea(2, 4)]
    [LabelText("技能描述")]
    public string description = "技能描述";
    
    [TabGroup("属性配置")]
    [Range(1f, 200f)]
    [LabelText("伤害值")]
    public float damage = 30f;
    
    [TabGroup("属性配置")]
    [Range(0.5f, 30f)]
    [LabelText("冷却时间(秒)")]
    public float cooldown = 3f;
    
    [TabGroup("属性配置")]
    [Range(0f, 100f)]
    [LabelText("法力消耗")]
    public float manaCost = 20f;
    
    [TabGroup("属性配置")]
    [Range(1f, 20f)]
    [LabelText("技能范围")]
    public float range = 8f;
    
    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能特效预制体")]
    public GameObject skillEffect;
    
    [TabGroup("特效配置")]
    [AssetsOnly]
    [LabelText("技能音效")]
    public AudioClip skillSound;
}