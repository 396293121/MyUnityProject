using UnityEngine;
using System.Collections;

/// <summary>
/// 简单的燃烧特效组件
/// 在怪物身上播放燃烧和烟雾效果
/// </summary>
public class BurnEffect : MonoBehaviour
{
    [Header("燃烧特效设置")]
    public float burnDuration = 3f;          // 燃烧持续时间
    public Vector3 effectOffset = Vector3.zero; // 特效偏移位置
    
    [Header("粒子系统引用")]
    public ParticleSystem fireParticles;     // 火焰粒子系统
    public ParticleSystem smokeParticles;    // 烟雾粒子系统
    
    private bool isPlaying = false;
    private Coroutine burnCoroutine;
    
    private void Awake()
    {
        // 如果没有手动分配粒子系统，尝试自动创建
        if (fireParticles == null || smokeParticles == null)
        {
            CreateParticleSystems();
        }
    }
    
    /// <summary>
    /// 开始播放燃烧特效
    /// </summary>
    public void StartBurnEffect()
    {
        if (isPlaying) return;
        
        isPlaying = true;
        
        // 播放粒子效果
        if (fireParticles != null)
        {
            fireParticles.gameObject.SetActive(true);
            fireParticles.Play();
        }
        
        if (smokeParticles != null)
        {
            smokeParticles.gameObject.SetActive(true);
            smokeParticles.Play();
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("burn_sound");
        }
        
        // 启动燃烧协程
        burnCoroutine = StartCoroutine(BurnCoroutine());
    }
    
    /// <summary>
    /// 停止燃烧特效
    /// </summary>
    public void StopBurnEffect()
    {
        if (!isPlaying) return;
        
        isPlaying = false;
        
        // 停止粒子效果
        if (fireParticles != null)
        {
            fireParticles.Stop();
        }
        
        if (smokeParticles != null)
        {
            smokeParticles.Stop();
        }
        
        // 停止协程
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
            burnCoroutine = null;
        }
    }
    
    /// <summary>
    /// 燃烧效果协程
    /// </summary>
    private IEnumerator BurnCoroutine()
    {
        float elapsed = 0f;
        
        while (elapsed < burnDuration && isPlaying)
        {
            elapsed += Time.deltaTime;
            
            // 可以在这里添加额外的燃烧逻辑
            // 比如持续伤害、颜色变化等
            
            yield return null;
        }
        
        // 燃烧结束，停止特效
        StopBurnEffect();
        
        // 延迟一段时间后隐藏粒子系统
        yield return new WaitForSeconds(1f);
        
        if (fireParticles != null)
        {
            fireParticles.gameObject.SetActive(false);
        }
        
        if (smokeParticles != null)
        {
            smokeParticles.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 创建粒子系统
    /// </summary>
    private void CreateParticleSystems()
    {
        // 创建火焰粒子系统
        CreateFireParticles();
        
        // 创建烟雾粒子系统
        CreateSmokeParticles();
    }
    
    /// <summary>
    /// 创建火焰粒子系统
    /// </summary>
    private void CreateFireParticles()
    {
        GameObject fireObj = new GameObject("FireParticles");
        fireObj.transform.SetParent(transform);
        fireObj.transform.localPosition = effectOffset;
        
        fireParticles = fireObj.AddComponent<ParticleSystem>();
        
        // 配置火焰粒子
        var main = fireParticles.main;
        main.startLifetime = 1.0f;
        main.startSpeed = 2.0f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.5f, 0f, 1f); // 橙色
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 发射设置
        var emission = fireParticles.emission;
        emission.rateOverTime = 15f;
        
        // 形状设置
        var shape = fireParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
        
        // 速度设置
        var velocityOverLifetime = fireParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, 3f); // 向上飘动
        
        // 颜色渐变
        var colorOverLifetime = fireParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fireGradient = new Gradient();
        fireGradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(new Color(1f, 1f, 0f), 0f),    // 黄色开始
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f), // 橙色中间
                new GradientColorKey(new Color(1f, 0f, 0f), 1f)      // 红色结束
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(1f, 0f), 
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = fireGradient;
        
        // 大小变化
        var sizeOverLifetime = fireParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        fireObj.SetActive(false);
    }
    
    /// <summary>
    /// 创建烟雾粒子系统
    /// </summary>
    private void CreateSmokeParticles()
    {
        GameObject smokeObj = new GameObject("SmokeParticles");
        smokeObj.transform.SetParent(transform);
        smokeObj.transform.localPosition = effectOffset + Vector3.up * 0.5f;
        
        smokeParticles = smokeObj.AddComponent<ParticleSystem>();
        
        // 配置烟雾粒子
        var main = smokeParticles.main;
        main.startLifetime = 2.0f;
        main.startSpeed = 1.0f;
        main.startSize = 0.5f;
        main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 灰色
        main.maxParticles = 15;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 发射设置
        var emission = smokeParticles.emission;
        emission.rateOverTime = 8f;
        
        // 形状设置
        var shape = smokeParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        
        // 速度设置
        var velocityOverLifetime = smokeParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f); // 缓慢向上
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); // 左右摆动
        
        // 颜色渐变
        var colorOverLifetime = smokeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient smokeGradient = new Gradient();
        smokeGradient.SetKeys(
            new GradientColorKey[] 
            { 
                new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0f),  // 深灰开始
                new GradientColorKey(new Color(0.6f, 0.6f, 0.6f), 0.5f), // 中灰
                new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 1f)   // 浅灰结束
            },
            new GradientAlphaKey[] 
            { 
                new GradientAlphaKey(0.8f, 0f), 
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = smokeGradient;
        
        // 大小变化
        var sizeOverLifetime = smokeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve smokeSizeCurve = new AnimationCurve();
        smokeSizeCurve.AddKey(0f, 0.3f);
        smokeSizeCurve.AddKey(0.5f, 1f);
        smokeSizeCurve.AddKey(1f, 1.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, smokeSizeCurve);
        
        smokeObj.SetActive(false);
    }
    
    /// <summary>
    /// 检查是否正在播放
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    private void OnDestroy()
    {
        StopBurnEffect();
    }
}