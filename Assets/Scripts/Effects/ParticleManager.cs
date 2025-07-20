using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 粒子效果管理器 - 管理游戏中的各种粒子特效
/// 从原Phaser项目的特效系统迁移而来
/// </summary>
public class ParticleManager : MonoBehaviour
{
    [Header("粒子效果配置")]
    public List<ParticleEffectData> particleEffects = new List<ParticleEffectData>();
    
    [Header("对象池设置")]
    public int poolSize = 50;                // 对象池大小
    public Transform poolParent;             // 对象池父对象
    
    [Header("性能设置")]
    public int maxActiveEffects = 20;        // 最大同时活跃特效数量
    public float autoCleanupInterval = 5f;   // 自动清理间隔
    
    // 单例
    public static ParticleManager Instance { get; private set; }
    
    // 对象池
    private Dictionary<string, Queue<ParticleSystem>> effectPools = new Dictionary<string, Queue<ParticleSystem>>();
    private List<ParticleSystem> activeEffects = new List<ParticleSystem>();
    
    // 预设效果
    private Dictionary<string, ParticleEffectData> effectDatabase = new Dictionary<string, ParticleEffectData>();
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 开始自动清理协程
        StartCoroutine(AutoCleanupCoroutine());
    }
    
    /// <summary>
    /// 初始化管理器
    /// </summary>
    private void InitializeManager()
    {
        // 创建对象池父对象
        if (poolParent == null)
        {
            GameObject parent = new GameObject("ParticleEffectPool");
            poolParent = parent.transform;
            parent.transform.SetParent(transform);
        }
        
        // 初始化效果数据库
        foreach (ParticleEffectData effectData in particleEffects)
        {
            if (!string.IsNullOrEmpty(effectData.effectName))
            {
                effectDatabase[effectData.effectName] = effectData;
                InitializeEffectPool(effectData);
            }
        }
        
        // 添加默认效果
        AddDefaultEffects();
    }
    
    /// <summary>
    /// 添加默认效果
    /// </summary>
    private void AddDefaultEffects()
    {
        // 如果没有配置效果，创建一些默认效果
        if (particleEffects.Count == 0)
        {
            CreateDefaultEffect("hit_effect", Color.red, 20, 0.5f);
            CreateDefaultEffect("heal_effect", Color.green, 15, 1f);
            CreateDefaultEffect("magic_effect", Color.blue, 25, 0.8f);
            CreateDefaultEffect("explosion_effect", new Color(1f, 0.5f, 0f, 1f), 50, 1.2f); // 橙色
            CreateDefaultEffect("death_effect", Color.gray, 30, 2f);
        }
    }
    
    /// <summary>
    /// 创建默认效果
    /// </summary>
    private void CreateDefaultEffect(string name, Color color, int particleCount, float duration)
    {
        GameObject effectObject = new GameObject(name);
        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        
        // 配置粒子系统
        var main = particles.main;
        main.startColor = color;
        main.startLifetime = duration;
        main.startSpeed = 5f;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, particleCount)
        });
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        // 创建效果数据
        ParticleEffectData effectData = new ParticleEffectData
        {
            effectName = name,
            particlePrefab = effectObject,
            duration = duration,
            autoDestroy = true,
            followTarget = false
        };
        
        effectDatabase[name] = effectData;
        InitializeEffectPool(effectData);
        
        // 将预制体设为非活跃
        effectObject.SetActive(false);
    }
    
    /// <summary>
    /// 初始化效果对象池
    /// </summary>
    private void InitializeEffectPool(ParticleEffectData effectData)
    {
        if (effectData.particlePrefab == null) return;
        
        Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
        
        for (int i = 0; i < poolSize / particleEffects.Count; i++)
        {
            GameObject obj = Instantiate(effectData.particlePrefab, poolParent);
            ParticleSystem particles = obj.GetComponent<ParticleSystem>();
            obj.SetActive(false);
            pool.Enqueue(particles);
        }
        
        effectPools[effectData.effectName] = pool;
    }
    
    /// <summary>
    /// 播放粒子效果
    /// </summary>
    public ParticleSystem PlayEffect(string effectName, Vector3 position, Quaternion rotation = default)
    {
        if (!effectDatabase.ContainsKey(effectName))
        {
            Debug.LogWarning($"Particle effect '{effectName}' not found!");
            return null;
        }
        
        ParticleEffectData effectData = effectDatabase[effectName];
        ParticleSystem particles = GetParticleFromPool(effectName);
        
        if (particles == null) return null;
        
        // 设置位置和旋转
        particles.transform.position = position;
        particles.transform.rotation = rotation == default ? Quaternion.identity : rotation;
        
        // 激活并播放
        particles.gameObject.SetActive(true);
        particles.Play();
        
        // 添加到活跃列表
        activeEffects.Add(particles);
        
        // 如果自动销毁，启动协程
        if (effectData.autoDestroy)
        {
            StartCoroutine(AutoReturnToPool(particles, effectData.duration));
        }
        
        return particles;
    }
    
    /// <summary>
    /// 播放跟随目标的粒子效果
    /// </summary>
    public ParticleSystem PlayEffectOnTarget(string effectName, Transform target, Vector3 offset = default)
    {
        ParticleSystem particles = PlayEffect(effectName, target.position + offset);
        
        if (particles != null && effectDatabase[effectName].followTarget)
        {
            StartCoroutine(FollowTarget(particles, target, offset));
        }
        
        return particles;
    }
    
    /// <summary>
    /// 播放攻击特效
    /// </summary>
    public void PlayHitEffect(Vector3 position, DamageType damageType = DamageType.Normal)
    {
        string effectName = GetHitEffectName(damageType);
        PlayEffect(effectName, position);
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("hit_sound");
        }
    }
    
    /// <summary>
    /// 播放治疗特效
    /// </summary>
    public void PlayHealEffect(Vector3 position)
    {
        PlayEffect("heal_effect", position);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("heal_sound");
        }
    }
    
    /// <summary>
    /// 播放技能特效
    /// </summary>
    public void PlaySkillEffect(string skillName, Vector3 position, Quaternion rotation = default)
    {
        string effectName = skillName + "_effect";
        PlayEffect(effectName, position, rotation);
    }
    
    /// <summary>
    /// 播放死亡特效
    /// </summary>
    public void PlayDeathEffect(Vector3 position)
    {
        PlayEffect("death_effect", position);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("death_sound");
        }
    }
    
    /// <summary>
    /// 播放爆炸特效
    /// </summary>
    public void PlayExplosionEffect(Vector3 position, float scale = 1f)
    {
        ParticleSystem particles = PlayEffect("explosion_effect", position);
        if (particles != null)
        {
            particles.transform.localScale = Vector3.one * scale;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("explosion_sound");
        }
    }
    
    /// <summary>
    /// 在目标身上播放燃烧特效
    /// </summary>
    public BurnEffect PlayBurnEffectOnTarget(Transform target, float duration = 3f)
    {
        if (target == null) return null;
        
        // 检查目标是否已经有燃烧特效
        BurnEffect existingBurn = target.GetComponent<BurnEffect>();
        if (existingBurn != null)
        {
            // 如果已经在燃烧，重新开始
            existingBurn.StopBurnEffect();
            existingBurn.StartBurnEffect();
            return existingBurn;
        }
        
        // 创建新的燃烧特效
        GameObject burnObj = new GameObject("BurnEffect");
        burnObj.transform.SetParent(target);
        burnObj.transform.localPosition = Vector3.zero;
        
        BurnEffect burnEffect = burnObj.AddComponent<BurnEffect>();
        burnEffect.burnDuration = duration;
        burnEffect.StartBurnEffect();
        
        // 播放燃烧音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("burn_sound");
        }
        
        return burnEffect;
    }
    
    /// <summary>
    /// 播放火焰击中特效（简化版）
    /// </summary>
    public void PlayFireHitEffect(Vector3 position, Transform target = null)
    {
        // 播放击中特效
        PlayEffect("fire_hit_effect", position);
        
        // 如果有目标，在目标身上添加燃烧效果
        if (target != null)
        {
            PlayBurnEffectOnTarget(target, 3f);
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("fire_hit_sound");
        }
    }
    
    /// <summary>
    /// 停止特效
    /// </summary>
    public void StopEffect(ParticleSystem particles)
    {
        if (particles != null)
        {
            particles.Stop();
            ReturnToPool(particles);
        }
    }
    
    /// <summary>
    /// 停止所有特效
    /// </summary>
    public void StopAllEffects()
    {
        foreach (ParticleSystem particles in activeEffects.ToArray())
        {
            StopEffect(particles);
        }
    }
    
    /// <summary>
    /// 从对象池获取粒子系统
    /// </summary>
    private ParticleSystem GetParticleFromPool(string effectName)
    {
        if (!effectPools.ContainsKey(effectName) || effectPools[effectName].Count == 0)
        {
            // 对象池为空，创建新的
            return CreateNewParticle(effectName);
        }
        
        return effectPools[effectName].Dequeue();
    }
    
    /// <summary>
    /// 创建新的粒子系统
    /// </summary>
    private ParticleSystem CreateNewParticle(string effectName)
    {
        if (!effectDatabase.ContainsKey(effectName)) return null;
        
        ParticleEffectData effectData = effectDatabase[effectName];
        if (effectData.particlePrefab == null) return null;
        
        GameObject obj = Instantiate(effectData.particlePrefab, poolParent);
        return obj.GetComponent<ParticleSystem>();
    }
    
    /// <summary>
    /// 返回对象池
    /// </summary>
    private void ReturnToPool(ParticleSystem particles)
    {
        if (particles == null) return;
        
        // 从活跃列表移除
        if (activeEffects.Contains(particles))
        {
            activeEffects.Remove(particles);
        }
        
        // 重置状态
        particles.Stop();
        particles.Clear();
        particles.gameObject.SetActive(false);
        particles.transform.localScale = Vector3.one;
        
        // 找到对应的池子
        foreach (var pool in effectPools)
        {
            if (particles.name.Contains(pool.Key.Replace("_effect", "")))
            {
                pool.Value.Enqueue(particles);
                break;
            }
        }
    }
    
    /// <summary>
    /// 自动返回对象池协程
    /// </summary>
    private IEnumerator AutoReturnToPool(ParticleSystem particles, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (particles != null && particles.gameObject.activeInHierarchy)
        {
            ReturnToPool(particles);
        }
    }
    
    /// <summary>
    /// 跟随目标协程
    /// </summary>
    private IEnumerator FollowTarget(ParticleSystem particles, Transform target, Vector3 offset)
    {
        while (particles != null && particles.gameObject.activeInHierarchy && target != null)
        {
            particles.transform.position = target.position + offset;
            yield return null;
        }
    }
    
    /// <summary>
    /// 自动清理协程
    /// </summary>
    private IEnumerator AutoCleanupCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoCleanupInterval);
            CleanupInactiveEffects();
        }
    }
    
    /// <summary>
    /// 清理非活跃特效
    /// </summary>
    private void CleanupInactiveEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ParticleSystem particles = activeEffects[i];
            
            if (particles == null || !particles.gameObject.activeInHierarchy || !particles.isPlaying)
            {
                if (particles != null)
                {
                    ReturnToPool(particles);
                }
                else
                {
                    activeEffects.RemoveAt(i);
                }
            }
        }
        
        // 如果活跃特效太多，强制清理一些
        if (activeEffects.Count > maxActiveEffects)
        {
            int removeCount = activeEffects.Count - maxActiveEffects;
            for (int i = 0; i < removeCount; i++)
            {
                ReturnToPool(activeEffects[0]);
            }
        }
    }
    
    /// <summary>
    /// 根据伤害类型获取击中特效名称
    /// </summary>
    private string GetHitEffectName(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Fire:
                return "fire_hit_effect";
            case DamageType.Ice:
                return "ice_hit_effect";
            case DamageType.Magic:
                return "magic_hit_effect";
            case DamageType.Poison:
                return "poison_hit_effect";
            default:
                return "hit_effect";
        }
    }
    
    /// <summary>
    /// 添加新的粒子效果
    /// </summary>
    public void AddEffect(ParticleEffectData effectData)
    {
        if (string.IsNullOrEmpty(effectData.effectName)) return;
        
        effectDatabase[effectData.effectName] = effectData;
        InitializeEffectPool(effectData);
    }
    
    /// <summary>
    /// 获取活跃特效数量
    /// </summary>
    public int GetActiveEffectCount()
    {
        return activeEffects.Count;
    }
    
    /// <summary>
    /// 检查特效是否存在
    /// </summary>
    public bool HasEffect(string effectName)
    {
        return effectDatabase.ContainsKey(effectName);
    }
    
    private void OnDestroy()
    {
        StopAllEffects();
    }
}

/// <summary>
/// 粒子效果数据
/// </summary>
[System.Serializable]
public class ParticleEffectData
{
    [Header("基础设置")]
    public string effectName;               // 效果名称
    public GameObject particlePrefab;       // 粒子预制体
    public float duration = 1f;             // 持续时间
    
    [Header("行为设置")]
    public bool autoDestroy = true;         // 自动销毁
    public bool followTarget = false;       // 跟随目标
    public bool looping = false;            // 循环播放
    
    [Header("音效设置")]
    public string soundEffect;              // 音效名称
    public float soundDelay = 0f;           // 音效延迟
}