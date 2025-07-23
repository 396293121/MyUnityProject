
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
[CreateAssetMenu(fileName = "AudioDataConfig", menuName = "Audios/Audio Data Config")]
[ShowOdinSerializedPropertiesInInspector]
public class AudioDataConfig : ScriptableObject
{    [TabGroup("主配置", "分层音效")]
    [ListDrawerSettings(NumberOfItemsPerPage = 5, ShowFoldout = true)]
    public List<SoundCategory> soundCategories = new();


    // 修改原始字段为可序列化结构
    [TabGroup("旧配置", "职业音效")]
    [ListDrawerSettings(NumberOfItemsPerPage = 5, ShowFoldout = true)]
    public List<ClassSoundEntry> classSoundConfigs = new();
}
[System.Serializable]
public enum AudioCategory
{
    [LabelText("默认音效")]
    Default,
    [LabelText("战士音效")]
    Warrior,
    [LabelText("法师音效")]
    Mage,
    [LabelText("射手音效")]
    Archer,
    [LabelText("野猪音效")]
    WildBoar,
        [LabelText("大型野猪音效")]
    BigWildBoar,
    [LabelText("界面音效")]
    UI,
    [LabelText("环境音效")]
    Environment,

    [LabelText("技能音效")]
    Skill,
    [LabelText("投射物音效")]
    Projectiles,
}

[System.Serializable]
public class SoundCategory
{
    [HorizontalGroup("Category", width: 120)]
    [LabelText("音效分类")]
    [ValueDropdown("GetCategoryTypes")]
    public AudioCategory categoryType;

    [ListDrawerSettings(NumberOfItemsPerPage = 5, ShowFoldout = true)]
    [LabelText("音效条目")]
    public List<SoundEntry> soundEntries = new();

    #if UNITY_EDITOR
    private IEnumerable<ValueDropdownItem> GetCategoryTypes()
    {
        return new List<ValueDropdownItem>() // 修改泛型类型
        {
              new("默认", AudioCategory.Default), 
            new("战士", AudioCategory.Warrior), // 显式创建项
            new("法师", AudioCategory.Mage),
            new("射手", AudioCategory.Archer),
            new("野猪", AudioCategory.WildBoar),
             new("大型野猪", AudioCategory.BigWildBoar),
            new("界面", AudioCategory.UI),
            new("环境", AudioCategory.Environment),
            new("技能", AudioCategory.Skill),
            new("投射物", AudioCategory.Projectiles),
        };
    }
    #endif
}

[System.Serializable]
public class SoundEntry
{
    [HorizontalGroup("Entry", width: 150)]
    [LabelText("音效名称")]
    public string soundName;

    [HorizontalGroup("Entry")]
    [LabelText("音效配置")]
    public AudioClipConfig config = new();
}

[System.Serializable]public class ClassSoundEntry
{
    [HorizontalGroup("Group", width: 150)]
    [LabelText("音效名称")]
    public string className;

    [HorizontalGroup("Group")]
    [LabelText("音效")]
    public AudioClipConfig sound = new();

}


