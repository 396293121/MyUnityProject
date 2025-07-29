using UnityEngine;

/// <summary>
/// 对话配置文件 - ScriptableObject
/// 用于配置对话系统的通用参数
/// </summary>
[CreateAssetMenu(fileName = "DialogueConfig", menuName = "Game/Dialogue Config")]
public class DialogueConfig : ScriptableObject
{
    [Header("交互设置")]
    [Tooltip("交互按键")]
    public KeyCode interactKey = KeyCode.E;
    
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
}