using UnityEngine;
using System.Collections;

/// <summary>
/// 召唤物自动销毁组件
/// 负责管理召唤物的生命周期和销毁效果
/// </summary>
public class SummonAutoDestroy : MonoBehaviour
{
    [Header("销毁设置")]
    [SerializeField] private float lifeTime = 30f;           // 生存时间
    [SerializeField] private float fadeOutDuration = 2f;     // 淡出持续时间
    [SerializeField] private bool showWarningEffect = true;  // 是否显示警告效果
    [SerializeField] private float warningTime = 5f;         // 警告时间（销毁前多久开始警告）
    
    [Header("视觉效果")]
    [SerializeField] private Color warningColor = Color.red; // 警告颜色
    [SerializeField] private float blinkSpeed = 2f;          // 闪烁速度
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isWarning = false;
    private bool isDestroying = false;
    private Coroutine warningCoroutine;
    private Coroutine destroyCoroutine;
    
    /// <summary>
    /// 初始化自动销毁组件
    /// </summary>
    /// <param name="duration">生存时间</param>
    public void Initialize(float duration)
    {
        lifeTime = duration;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // 开始生命周期倒计时
        StartLifeCycle();
        
        Debug.Log($"[SummonAutoDestroy] {gameObject.name} 初始化自动销毁，生存时间: {lifeTime}秒");
    }
    
    private void Start()
    {
        // 如果没有手动初始化，使用默认生存时间
        if (!isDestroying && destroyCoroutine == null)
        {
            Initialize(lifeTime);
        }
    }
    
    /// <summary>
    /// 开始生命周期管理
    /// </summary>
    private void StartLifeCycle()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }
        
        destroyCoroutine = StartCoroutine(LifeCycleCoroutine());
    }
    
    /// <summary>
    /// 生命周期协程
    /// </summary>
    private IEnumerator LifeCycleCoroutine()
    {
        // 等待到警告时间
        float normalTime = lifeTime - warningTime;
        if (normalTime > 0)
        {
            yield return new WaitForSeconds(normalTime);
        }
        
        // 开始警告效果
        if (showWarningEffect && warningTime > 0)
        {
            StartWarningEffect();
            yield return new WaitForSeconds(warningTime);
        }
        
        // 开始销毁过程
        yield return StartCoroutine(DestroyProcess());
    }
    
    /// <summary>
    /// 开始警告效果
    /// </summary>
    private void StartWarningEffect()
    {
        if (spriteRenderer == null) return;
        
        isWarning = true;
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        
        warningCoroutine = StartCoroutine(WarningBlinkCoroutine());
        
        Debug.Log($"[SummonAutoDestroy] {gameObject.name} 开始警告效果");
    }
    
    /// <summary>
    /// 警告闪烁协程
    /// </summary>
    private IEnumerator WarningBlinkCoroutine()
    {
        while (isWarning && !isDestroying)
        {
            // 闪烁到警告颜色
            float elapsedTime = 0f;
            float blinkDuration = 1f / blinkSpeed / 2f; // 半个周期
            
            while (elapsedTime < blinkDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / blinkDuration;
                spriteRenderer.color = Color.Lerp(originalColor, warningColor, t);
                yield return null;
            }
            
            // 闪烁回原色
            elapsedTime = 0f;
            while (elapsedTime < blinkDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / blinkDuration;
                spriteRenderer.color = Color.Lerp(warningColor, originalColor, t);
                yield return null;
            }
        }
    }
    
    /// <summary>
    /// 销毁过程
    /// </summary>
    private IEnumerator DestroyProcess()
    {
        isDestroying = true;
        isWarning = false;
        
        // 停止警告效果
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
        
        Debug.Log($"[SummonAutoDestroy] {gameObject.name} 开始销毁过程");
        
        // 通知Enemy组件即将销毁（如果需要特殊处理）
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            // 可以在这里添加特殊的死亡逻辑
            enemy.SetDie(true);
        }
        
        // 淡出效果
        if (spriteRenderer != null && fadeOutDuration > 0)
        {
            yield return StartCoroutine(FadeOutCoroutine());
        }
        
        // 从场景管理器中移除
        if (SceneController.Instance != null)
        {
            SceneController.Instance.RemoveEnemy(gameObject);
            if (enemy != null)
            {
                SceneController.Instance.RemoveEnemyController(enemy);
            }
        }
        
        // 销毁游戏对象
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 淡出协程
    /// </summary>
    private IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutDuration;
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        spriteRenderer.color = endColor;
    }
    
    /// <summary>
    /// 立即销毁召唤物
    /// </summary>
    public void DestroyImmediately()
    {
        if (isDestroying) return;
        
        // 停止所有协程
        StopAllCoroutines();
        
        // 立即销毁
        if (SceneController.Instance != null)
        {
            SceneController.Instance.RemoveEnemy(gameObject);
            Enemy enemy = GetComponent<Enemy>();
            if (enemy != null)
            {
                SceneController.Instance.RemoveEnemyController(enemy);
            }
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 延长生存时间
    /// </summary>
    /// <param name="additionalTime">额外时间</param>
    public void ExtendLifeTime(float additionalTime)
    {
        if (isDestroying) return;
        
        lifeTime += additionalTime;
        
        // 重新开始生命周期
        StartLifeCycle();
        
        Debug.Log($"[SummonAutoDestroy] {gameObject.name} 延长生存时间 {additionalTime}秒，总时间: {lifeTime}秒");
    }
    
    /// <summary>
    /// 获取剩余生存时间
    /// </summary>
    /// <returns>剩余时间</returns>
    public float GetRemainingTime()
    {
        if (isDestroying) return 0f;
        
        // 这里可以通过计算协程的剩余时间来获取更精确的值
        // 简化实现，返回一个估算值
        return lifeTime;
    }
    
    private void OnDestroy()
    {
        // 清理协程
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
        }
    }
    
    // 调试信息
    private void OnDrawGizmosSelected()
    {
        if (isWarning)
        {
            Gizmos.color = warningColor;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
        
        if (isDestroying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
        }
    }
}