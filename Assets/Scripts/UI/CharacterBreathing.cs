using UnityEngine;
using DG.Tweening;

/// <summary>
/// 角色呼吸动画效果
/// 实现Phaser项目中的角色呼吸效果
/// </summary>
public class CharacterBreathing : MonoBehaviour
{
    [Header("呼吸动画设置")]
    [SerializeField] private float breathingScale = 0.05f;     // 呼吸幅度 (0.8 ↔ 0.83)
    [SerializeField] private float breathingDuration = 1.5f;   // 呼吸周期
    [SerializeField] private AnimationCurve breathingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);    // 呼吸曲线
    [SerializeField] private bool autoStart = true;           // 自动开始
    
    private Vector3 originalScale;
    private Tween breathingTween;
    private bool isBreathing = false;
    
    void Start()
    {
        originalScale = transform.localScale;
        
        if (autoStart)
        {
            StartBreathing();
        }
    }
    
    /// <summary>
    /// 开始呼吸动画
    /// </summary>
    public void StartBreathing()
    {
        if (isBreathing) return;
        
        StopBreathing();
        
        // 设置初始缩放
        transform.localScale = originalScale;
        
        // 创建呼吸动画
        breathingTween = transform.DOScale(
            originalScale * (1 + breathingScale), 
            breathingDuration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
        
        isBreathing = true;
        
        Debug.Log($"[CharacterBreathing] {gameObject.name} 开始呼吸动画");
    }
    
    /// <summary>
    /// 停止呼吸动画
    /// </summary>
    public void StopBreathing()
    {
        if (breathingTween != null)
        {
            breathingTween.Kill();
            breathingTween = null;
        }
        
        // 恢复原始大小
        transform.localScale = originalScale;
        isBreathing = false;
        
        Debug.Log($"[CharacterBreathing] {gameObject.name} 停止呼吸动画");
    }
    
    /// <summary>
    /// 暂停呼吸动画
    /// </summary>
    public void PauseBreathing()
    {
        if (breathingTween != null && breathingTween.IsActive())
        {
            breathingTween.Pause();
            Debug.Log($"[CharacterBreathing] {gameObject.name} 暂停呼吸动画");
        }
    }
    
    /// <summary>
    /// 恢复呼吸动画
    /// </summary>
    public void ResumeBreathing()
    {
        if (breathingTween != null && breathingTween.IsActive())
        {
            breathingTween.Play();
            Debug.Log($"[CharacterBreathing] {gameObject.name} 恢复呼吸动画");
        }
    }
    
    /// <summary>
    /// 设置呼吸参数
    /// </summary>
    public void SetBreathingParams(float scale, float duration)
    {
        breathingScale = scale;
        breathingDuration = duration;
        
        // 如果正在呼吸，重新开始以应用新参数
        if (isBreathing)
        {
            StartBreathing();
        }
    }
    
    /// <summary>
    /// 检查是否正在呼吸
    /// </summary>
    public bool IsBreathing()
    {
        return isBreathing && breathingTween != null && breathingTween.IsActive();
    }
    
    void OnDestroy()
    {
        // 清理动画
        if (breathingTween != null)
        {
            breathingTween.Kill();
        }
    }
    
    void OnDisable()
    {
        // 禁用时暂停动画
        PauseBreathing();
    }
    
    void OnEnable()
    {
        // 启用时恢复动画
        if (isBreathing)
        {
            ResumeBreathing();
        }
    }
}