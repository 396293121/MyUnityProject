
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
[CreateAssetMenu(fileName = "AudioDataConfig", menuName = "Audios/Audio Data Config")]
[ShowOdinSerializedPropertiesInInspector]
public class AudioDataConfig : ScriptableObject
{
    // 修改原始字段为可序列化结构
    [TabGroup("扩展配置", "职业音效")]
    [ListDrawerSettings(NumberOfItemsPerPage = 5, ShowFoldout = true)]
    public List<ClassSoundEntry> classSoundConfigs = new List<ClassSoundEntry>();
}
[System.Serializable]public class ClassSoundEntry
{
    [HorizontalGroup("Group", width: 300)]
    [LabelText("音效名称")]
    public string className;

    [HorizontalGroup("Group")]
    [LabelText("音效")]
    public AudioClipConfig sound = new AudioClipConfig();

}


