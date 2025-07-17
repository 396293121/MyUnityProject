using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// 状态效果UI组件 - 显示玩家身上的各种状态效果
/// 支持增益、减益、持续时间显示等功能
/// </summary>
[ShowOdinSerializedPropertiesInInspector]
public class StatusEffectUI : MonoBehaviour
{
    [TitleGroup("状态效果UI", "显示玩家状态效果", TitleAlignments.Centered)]
    
    [TabGroup("UI元素")]
    [BoxGroup("UI元素/基础组件")]
    [LabelText("状态图标")]
    [Required]
    public Image statusIcon;
    
    [BoxGroup("UI元素/基础组件")]
    [LabelText("边框图像")]
    public Image borderImage;
    
    [BoxGroup("UI元素/基础组件")]
    [LabelText("持续时间文本")]
    public TextMeshProUGUI durationText;
    
    [BoxGroup("UI元素/基础组件")]
    [LabelText("层数文本")]
    public TextMeshProUGUI stackText;
    
    [BoxGroup("UI元素/特效")]
    [LabelText("增益特效")]
    public ParticleSystem buffEffect;
    
    [BoxGroup("UI元素/特效")]
    [LabelText("减益特效")]
    public ParticleSystem debuffEffect;
    
    [TabGroup("配置")]
    [BoxGroup("配置/颜色设置")]
    [LabelText("增益边框色")]
    public Color buffBorderColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    
    [BoxGroup("配置/颜色设置")]
    [LabelText("减益边框色")]
    public Color debuffBorderColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    
    [BoxGroup("配置/颜色设置")]
    [LabelText("中性边框色")]
    public Color neutralBorderColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    
    [BoxGroup("配置/动画设置")]
    [LabelText("闪烁速度")]
    [PropertyRange(0.5f, 5f)]
    public float blinkSpeed = 2f;
    
    [TabGroup("状态")]
    [FoldoutGroup("状态/运行时数据", expanded: true)]
    [LabelText("状态效果数据")]
    [ReadOnly]
    [ShowInInspector]
    private StatusEffect statusEffect;
    
    [FoldoutGroup("状态/运行时数据")]
    [LabelText("剩余时间")]
    [ReadOnly]
    [ShowInInspector]
    private float remainingTime;
    
    private void Update()
    {
        if (statusEffect != null)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// 设置状态效果
    /// </summary>
    public void SetStatusEffect(StatusEffect effect)
    {
        statusEffect = effect;
        
        if (statusEffect != null)
        {
            // 设置图标
            if (statusIcon != null)
            {
                statusIcon.sprite = statusEffect.icon;
            }
            
            // 设置边框颜色
            UpdateBorderColor();
            
            // 播放对应特效
            PlayEffect();
            
            Debug.Log($"[StatusEffectUI] 设置状态效果: {statusEffect.name}");
        }
    }
    
    /// <summary>
    /// 更新显示
    /// </summary>
    private void UpdateDisplay()
    {
        if (statusEffect == null) return;
        
        // 更新持续时间
        if (statusEffect.duration > 0)
        {
            remainingTime = statusEffect.remainingTime;
            
            if (durationText != null)
            {
                if (remainingTime > 0)
                {
                    durationText.text = Mathf.Ceil(remainingTime).ToString();
                    durationText.gameObject.SetActive(true);
                }
                else
                {
                    durationText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 永久效果不显示时间
            if (durationText != null)
            {
                durationText.gameObject.SetActive(false);
            }
        }
        
        // 更新层数
        if (stackText != null)
        {
            if (statusEffect.stackCount > 1)
            {
                stackText.text = statusEffect.stackCount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }
        
        // 时间快结束时闪烁
        if (remainingTime > 0 && remainingTime <= 3f)
        {
            StartBlinking();
        }
        else
        {
            StopBlinking();
        }
    }
    
    /// <summary>
    /// 更新边框颜色
    /// </summary>
    private void UpdateBorderColor()
    {
        if (borderImage == null || statusEffect == null) return;
        
        Color targetColor;
        
        switch (statusEffect.type)
        {
            case StatusEffectType.Buff:
                targetColor = buffBorderColor;
                break;
            case StatusEffectType.Debuff:
                targetColor = debuffBorderColor;
                break;
            default:
                targetColor = neutralBorderColor;
                break;
        }
        
        borderImage.color = targetColor;
    }
    
    /// <summary>
    /// 播放特效
    /// </summary>
    private void PlayEffect()
    {
        if (statusEffect == null) return;
        
        switch (statusEffect.type)
        {
            case StatusEffectType.Buff:
                if (buffEffect != null)
                {
                    buffEffect.Play();
                }
                if (debuffEffect != null && debuffEffect.isPlaying)
                {
                    debuffEffect.Stop();
                }
                break;
                
            case StatusEffectType.Debuff:
                if (debuffEffect != null)
                {
                    debuffEffect.Play();
                }
                if (buffEffect != null && buffEffect.isPlaying)
                {
                    buffEffect.Stop();
                }
                break;
                
            default:
                if (buffEffect != null && buffEffect.isPlaying)
                {
                    buffEffect.Stop();
                }
                if (debuffEffect != null && debuffEffect.isPlaying)
                {
                    debuffEffect.Stop();
                }
                break;
        }
    }
    
    /// <summary>
    /// 开始闪烁
    /// </summary>
    private void StartBlinking()
    {
        if (statusIcon != null)
        {
            StartCoroutine(BlinkCoroutine());
        }
    }
    
    /// <summary>
    /// 停止闪烁
    /// </summary>
    private void StopBlinking()
    {
        StopAllCoroutines();
        if (statusIcon != null)
        {
            Color color = statusIcon.color;
            color.a = 1f;
            statusIcon.color = color;
        }
    }
    
    /// <summary>
    /// 闪烁协程
    /// </summary>
    private System.Collections.IEnumerator BlinkCoroutine()
    {
        while (remainingTime > 0 && remainingTime <= 3f)
        {
            float time = 0f;
            while (time < 1f / blinkSpeed)
            {
                time += Time.deltaTime;
                float alpha = Mathf.PingPong(time * blinkSpeed * 2f, 1f);
                Color color = statusIcon.color;
                color.a = Mathf.Lerp(0.3f, 1f, alpha);
                if (statusIcon != null)
                {
                    statusIcon.color = color;
                }
                yield return null;
            }
        }
        
        // 恢复完全不透明
        if (statusIcon != null)
        {
            Color color = statusIcon.color;
            color.a = 1f;
            statusIcon.color = color;
        }
    }
    
    /// <summary>
    /// 清除状态效果
    /// </summary>
    public void ClearStatusEffect()
    {
        statusEffect = null;
        
        // 停止所有特效
        if (buffEffect != null && buffEffect.isPlaying)
        {
            buffEffect.Stop();
        }
        if (debuffEffect != null && debuffEffect.isPlaying)
        {
            debuffEffect.Stop();
        }
        
        // 停止闪烁
        StopBlinking();
        
        // 隐藏UI
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 获取状态效果
    /// </summary>
    public StatusEffect GetStatusEffect()
    {
        return statusEffect;
    }
}

/// <summary>
/// 状态效果类型
/// </summary>
public enum StatusEffectType
{
    Buff,       // 增益
    Debuff,     // 减益
    Neutral     // 中性
}

/// <summary>
/// 状态效果数据结构
/// </summary>
[System.Serializable]
public class StatusEffect
{
    public string name;
    public string description;
    public Sprite icon;
    public StatusEffectType type;
    public float duration;
    public float remainingTime;
    public int stackCount = 1;
    public int maxStacks = 1;
    
    public StatusEffect(string name, StatusEffectType type, float duration, Sprite icon = null)
    {
        this.name = name;
        this.type = type;
        this.duration = duration;
        this.remainingTime = duration;
        this.icon = icon;
    }
}