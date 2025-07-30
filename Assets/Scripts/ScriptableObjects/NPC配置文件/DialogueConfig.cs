using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 对话配置文件 - ScriptableObject
/// 用于配置对话系统的通用参数
/// </summary>
[CreateAssetMenu(fileName = "DialogueConfig", menuName = "Game/Dialogue Config")]
public class DialogueConfig : ScriptableObject
{
    [Header("交互设置")]
    
    [Tooltip("交互提示文本")]
    public string interactPrompt = "按 E 交互";
    
    [Header("音效设置")]
    [Tooltip("对话开始音效")]
    public AudioClip dialogueStartSound;
    
    [Tooltip("对话结束音效")]
    public AudioClip dialogueEndSound;
    
    [Tooltip("文字打字音效")]
    public AudioClip typingSound;
    
    [Header("调试设置")]
    [Tooltip("启用调试模式")]
    public bool debugMode = false;
        [BoxGroup("任务系统")]
    [LabelText("任务可接取指示器")]
    [InfoBox("显示NPC有任务可接取的UI指示器")]
    [SerializeField] public GameObject questIndicator;
        [BoxGroup("任务系统")]
    [LabelText("任务正在进行指示器")]
    [InfoBox("显示NPC有任务正在进行的UI指示器")]
    [SerializeField] public GameObject questActiveIndicator;
    
    [BoxGroup("任务系统")]
    [LabelText("任务可完成指示器")]
    [InfoBox("显示NPC有任务可完成的UI指示器")]
    [SerializeField] public GameObject questCompleteIndicator;

        [BoxGroup("UI组件")]
    [LabelText("交互提示UI")]
    [InfoBox("玩家靠近时显示的交互提示")]
    public GameObject interactionPrompt;

    [BoxGroup("UI组件")]
    [LabelText("交互提示文本")]
    [ShowInInspector]
    [ReadOnly]
    public string promptText = "按 E 键对话";
}