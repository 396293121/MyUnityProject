using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 对象池接口 - 定义池化对象的基本行为
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 从池中取出时调用
    /// </summary>
    void OnSpawnFromPool();
    
    /// <summary>
    /// 返回池中时调用
    /// </summary>
    void OnReturnToPool();
    
    /// <summary>
    /// 获取对象的Transform
    /// </summary>
    Transform GetTransform();
}

/// <summary>
/// 通用对象池系统 - 用于优化频繁创建销毁的对象
/// 优化目标：减少GC压力，提高性能，支持IPoolable接口
/// </summary>
/// <typeparam name="T">池化对象类型</typeparam>
public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> availableObjects = new Queue<T>();
    private readonly List<T> usedObjects = new List<T>();
    private readonly T prefab;
    private readonly Transform parent;
    private readonly int maxSize;
    private readonly bool allowExpansion;
    private int currentSize;
    
    /// <summary>
    /// 当前池大小
    /// </summary>
    public int CurrentPoolSize => availableObjects.Count + usedObjects.Count;
    
    /// <summary>
    /// 可用对象数量
    /// </summary>
    public int AvailableCount => availableObjects.Count;
    
    /// <summary>
    /// 已使用对象数量
    /// </summary>
    public int UsedCount => usedObjects.Count;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="parent">父对象</param>
    /// <param name="initialSize">初始大小</param>
    /// <param name="maxSize">最大大小</param>
    /// <param name="allowExpansion">是否允许扩展</param>
    public ObjectPool(T prefab, Transform parent = null, int initialSize = 5, int maxSize = 50, bool allowExpansion = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.maxSize = maxSize;
        this.allowExpansion = allowExpansion;
        this.currentSize = 0;
        
        // 验证预制体
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] 预制体不能为空");
            return;
        }
        
        // 预创建初始对象
        PrewarmPool(initialSize);
    }
    
    /// <summary>
    /// 预热池 - 预创建初始对象
    /// </summary>
    /// <param name="count">预创建数量</param>
    private void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T obj = CreateNewObject();
            if (obj != null)
            {
                // 调用池化接口
                if (obj is IPoolable poolable)
                {
                    poolable.OnReturnToPool();
                }
                
                obj.gameObject.SetActive(false);
                availableObjects.Enqueue(obj);
            }
        }
    }
    
    /// <summary>
    /// 从池中获取对象
    /// </summary>
    /// <returns>池化对象</returns>
    public T Get()
    {
        T obj = null;
        
        // 尝试从可用队列获取
        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        // 如果池为空且允许扩展
        else if (allowExpansion && currentSize < maxSize)
        {
            obj = CreateNewObject();
        }
        // 如果达到最大大小，强制回收最老的对象
        else if (usedObjects.Count > 0)
        {
            obj = usedObjects[0];
            usedObjects.RemoveAt(0);
            
            // 调用池化接口
            if (obj is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }
        }
        
        if (obj != null)
        {
            obj.gameObject.SetActive(true);
            
            // 调用池化接口
            if (obj is IPoolable poolable)
            {
                poolable.OnSpawnFromPool();
            }
            
            usedObjects.Add(obj);
        }
        
        return obj;
    }
    
    /// <summary>
    /// 将对象返回池中
    /// </summary>
    /// <param name="obj">要返回的对象</param>
    public void Return(T obj)
    {
        if (obj == null)
        {
            return;
        }
        
        // 从已使用列表中移除
        if (usedObjects.Remove(obj))
        {
            // 调用池化接口
            if (obj is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }
            
            obj.gameObject.SetActive(false);
            
            // 设置父对象
            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }
            
            // 如果池未满，则返回池中
            if (availableObjects.Count < maxSize)
            {
                availableObjects.Enqueue(obj);
            }
            else
            {
                // 池已满，销毁对象
                Object.Destroy(obj.gameObject);
                currentSize--;
            }
        }
    }
    
    /// <summary>
    /// 创建新对象
    /// </summary>
    /// <returns>新创建的对象</returns>
    private T CreateNewObject()
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] 预制体为空，无法创建对象");
            return null;
        }
        
        try
        {
            T newObj = Object.Instantiate(prefab, parent);
            newObj.gameObject.SetActive(false);
            currentSize++;
            return newObj;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ObjectPool] 创建对象失败: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear()
    {
        // 销毁所有可用对象
        while (availableObjects.Count > 0)
        {
            T obj = availableObjects.Dequeue();
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }
        
        // 销毁所有已使用对象
        for (int i = usedObjects.Count - 1; i >= 0; i--)
        {
            T obj = usedObjects[i];
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }
        
        usedObjects.Clear();
        currentSize = 0;
    }
    
    /// <summary>
    /// 获取池的状态信息
    /// </summary>
    public string GetPoolInfo()
    {
        return $"池状态 - 可用: {availableObjects.Count}, 已用: {usedObjects.Count}, 总大小: {currentSize}/{maxSize}";
    }
    
    /// <summary>
    /// 强制回收所有已使用的对象
    /// </summary>
    public void ForceReturnAll()
    {
        // 复制列表以避免在迭代时修改
        var objectsToReturn = new List<T>(usedObjects);
        
        foreach (T obj in objectsToReturn)
        {
            if (obj != null)
            {
                Return(obj);
            }
        }
    }
}

/// <summary>
/// 对象池管理器 - 管理多个对象池
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    [TabGroup("对象池配置")]
    [LabelText("特效池配置")]
    [SerializeField] private EffectPoolConfig effectPoolConfig;
    
    [TabGroup("对象池配置")]
    [LabelText("伤害数字池配置")]
    [SerializeField] private DamageTextPoolConfig damageTextPoolConfig;
    
    [TabGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("特效池状态")]
    private string EffectPoolStatus => effectPool?.GetPoolInfo() ?? "未初始化";
    
    [TabGroup("调试信息")]
    [ShowInInspector, ReadOnly]
    [LabelText("伤害数字池状态")]
    private string DamageTextPoolStatus => damageTextPool?.GetPoolInfo() ?? "未初始化";
    
    // 各种对象池
    private ObjectPool<ParticleSystem> effectPool;
    private ObjectPool<DamageText> damageTextPool;
    
    // 单例模式
    public static ObjectPoolManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化所有对象池
    /// </summary>
    private void InitializePools()
    {
        // 创建特效池容器
        Transform effectParent = new GameObject("EffectPool").transform;
        effectParent.SetParent(transform);
        
        // 创建伤害数字池容器
        Transform damageTextParent = new GameObject("DamageTextPool").transform;
        damageTextParent.SetParent(transform);
        
        // 初始化特效池
        if (effectPoolConfig != null && effectPoolConfig.effectPrefab != null)
        {
            effectPool = new ObjectPool<ParticleSystem>(
                effectPoolConfig.effectPrefab,
                effectParent,
                effectPoolConfig.initialSize,
                effectPoolConfig.maxSize
            );
        }
        
        // 初始化伤害数字池
        if (damageTextPoolConfig != null && damageTextPoolConfig.damageTextPrefab != null)
        {
            damageTextPool = new ObjectPool<DamageText>(
                damageTextPoolConfig.damageTextPrefab,
                damageTextParent,
                damageTextPoolConfig.initialSize,
                damageTextPoolConfig.maxSize
            );
        }
        
        Debug.Log("对象池管理器初始化完成");
    }
    
    #region 公共接口
    
    /// <summary>
    /// 获取特效对象
    /// </summary>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="autoReturn">是否自动回收</param>
    /// <returns>特效对象</returns>
    public ParticleSystem GetEffect(Vector3 position, Quaternion rotation = default, bool autoReturn = true)
    {
        if (effectPool == null) 
        {
            Debug.LogWarning("[ObjectPoolManager] 特效池未初始化");
            return null;
        }
        
        ParticleSystem effect = effectPool.Get();
        if (effect != null)
        {
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            
            // 自动回收
            if (autoReturn)
            {
                StartCoroutine(ReturnEffectAfterPlay(effect));
            }
        }
        
        return effect;
    }
    
    /// <summary>
    /// 获取伤害数字对象
    /// </summary>
    /// <param name="position">显示位置</param>
    /// <param name="damage">伤害数值</param>
    /// <returns>伤害数字对象</returns>
    public DamageText GetDamageText(Vector3 position, float damage)
    {
        if (damageTextPool == null) 
        {
            Debug.LogWarning("[ObjectPoolManager] 伤害数字池未初始化");
            return null;
        }
        
        DamageText damageText = damageTextPool.Get();
        if (damageText != null)
        {
            damageText.transform.position = position;
            damageText.SetDamage(damage);
        }
        
        return damageText;
    }
    
    /// <summary>
    /// 手动返回特效到池中
    /// </summary>
    /// <param name="effect">特效对象</param>
    public void ReturnEffect(ParticleSystem effect)
    {
        effectPool?.Return(effect);
    }
    
    /// <summary>
    /// 手动返回伤害数字到池中
    /// </summary>
    /// <param name="damageText">伤害数字对象</param>
    public void ReturnDamageText(DamageText damageText)
    {
        damageTextPool?.Return(damageText);
    }
    
    #endregion
    
    #region 自动回收协程
    
    /// <summary>
    /// 特效播放完毕后自动回收
    /// </summary>
    private IEnumerator ReturnEffectAfterPlay(ParticleSystem effect)
    {
        if (effect == null) yield break;
        
        // 确保特效开始播放
        if (!effect.isPlaying)
        {
            effect.Play();
        }
        
        // 等待特效播放完毕
        yield return new WaitWhile(() => effect != null && effect.isPlaying);
        
        // 额外等待一点时间确保粒子完全消失
        yield return new WaitForSeconds(0.1f);
        
        if (effect != null)
        {
            ReturnEffect(effect);
        }
    }
    
    #endregion
    
    #region 调试功能
    
    [TabGroup("调试")]
    [Button("清空所有对象池")]
    private void ClearAllPools()
    {
        effectPool?.Clear();
        damageTextPool?.Clear();
        Debug.Log("所有对象池已清空");
    }
    
    [TabGroup("调试")]
    [Button("打印池状态")]
    private void PrintPoolStatus()
    {
        Debug.Log($"对象池状态:\n" +
                 $"特效池: {EffectPoolStatus}\n" +
                 $"伤害数字池: {DamageTextPoolStatus}");
    }
    
    [TabGroup("调试")]
    [Button("强制回收所有对象")]
    private void ForceReturnAllObjects()
    {
        effectPool?.ForceReturnAll();
        damageTextPool?.ForceReturnAll();
        Debug.Log("所有对象已强制回收");
    }
    
    /// <summary>
    /// 获取详细的池状态信息
    /// </summary>
    /// <returns>详细状态字符串</returns>
    public string GetDetailedPoolStatus()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 对象池详细状态 ===");
        
        if (effectPool != null)
        {
            sb.AppendLine($"特效池: {effectPool.GetPoolInfo()}");
        }
        else
        {
            sb.AppendLine("特效池: 未初始化");
        }
        
        if (damageTextPool != null)
        {
            sb.AppendLine($"伤害数字池: {damageTextPool.GetPoolInfo()}");
        }
        else
        {
            sb.AppendLine("伤害数字池: 未初始化");
        }
        
        return sb.ToString();
    }
    
    #endregion
}

/// <summary>
/// 特效池配置
/// </summary>
[System.Serializable]
public class EffectPoolConfig
{
    [LabelText("特效预制体")]
    public ParticleSystem effectPrefab;
    
    [LabelText("初始大小")]
    [Range(1, 20)]
    public int initialSize = 5;
    
    [LabelText("最大大小")]
    [Range(10, 100)]
    public int maxSize = 30;
}

/// <summary>
/// 伤害数字池配置
/// </summary>
[System.Serializable]
public class DamageTextPoolConfig
{
    [LabelText("伤害数字预制体")]
    public DamageText damageTextPrefab;
    
    [LabelText("初始大小")]
    [Range(1, 20)]
    public int initialSize = 3;
    
    [LabelText("最大大小")]
    [Range(5, 50)]
    public int maxSize = 15;
}

/// <summary>
/// 伤害数字组件 - 用于显示伤害数值，实现IPoolable接口
/// </summary>
public class DamageText : MonoBehaviour, IPoolable
{
    [SerializeField] private UnityEngine.UI.Text textComponent;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1f;
    
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Coroutine animationCoroutine;
    
    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<UnityEngine.UI.Text>();
            
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    #region IPoolable接口实现
    
    /// <summary>
    /// 从池中取出时调用
    /// </summary>
    public void OnSpawnFromPool()
    {
        // 重置状态
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
        
        // 停止之前的动画
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
    
    /// <summary>
    /// 返回池中时调用
    /// </summary>
    public void OnReturnToPool()
    {
        // 停止动画
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // 重置状态
        canvasGroup.alpha = 0f;
        transform.position = Vector3.zero;
    }
    
    /// <summary>
    /// 获取Transform
    /// </summary>
    /// <returns>Transform组件</returns>
    public Transform GetTransform()
    {
        return transform;
    }
    
    #endregion
    
    /// <summary>
    /// 设置伤害数值
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void SetDamage(float damage)
    {
        if (textComponent != null)
        {
            textComponent.text = damage.ToString("F0");
            textComponent.color = damage > 20 ? Color.red : Color.white;
        }
        
        startPosition = transform.position;
        canvasGroup.alpha = 1f;
        
        // 停止之前的动画
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(AnimateDamageText());
    }
    
    /// <summary>
    /// 伤害数字动画
    /// </summary>
    private IEnumerator AnimateDamageText()
    {
        float elapsedTime = 0f;
        float duration = 2f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 向上移动
            transform.position = startPosition + Vector3.up * (moveSpeed * elapsedTime);
            
            // 淡出效果
            canvasGroup.alpha = 1f - (progress * fadeSpeed);
            
            yield return null;
        }
        
        // 动画完成后自动返回池中
        animationCoroutine = null;
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnDamageText(this);
        }
    }
    
    /// <summary>
    /// 手动停止动画并返回池中
    /// </summary>
    public void StopAndReturn()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnDamageText(this);
        }
    }
}