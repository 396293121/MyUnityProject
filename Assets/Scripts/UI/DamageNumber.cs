using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 伤害数字显示系统 - 显示伤害、治疗等数字效果
/// 从原Phaser项目的伤害显示系统迁移而来
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [Header("显示设置")]
    public float duration = 1f;              // 显示持续时间
    public float moveSpeed = 2f;             // 移动速度
    public Vector2 moveDirection = Vector2.up; // 移动方向
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 移动曲线
    
    [Header("缩放动画")]
    public bool useScaleAnimation = true;    // 使用缩放动画
    public float startScale = 0.5f;          // 起始缩放
    public float maxScale = 1.2f;            // 最大缩放
    public float endScale = 1f;              // 结束缩放
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 缩放曲线
    
    [Header("透明度动画")]
    public bool useFadeAnimation = true;     // 使用透明度动画
    public float startAlpha = 1f;            // 起始透明度
    public float endAlpha = 0f;              // 结束透明度
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // 透明度曲线
    
    [Header("颜色设置")]
    public Color damageColor = Color.red;    // 伤害颜色
    public Color healColor = Color.green;    // 治疗颜色
    public Color criticalColor = Color.yellow; // 暴击颜色
    public Color manaColor = Color.blue;     // 魔法颜色
    public Color expColor = Color.cyan;      // 经验颜色
    
    // 组件引用
    private TextMeshProUGUI textComponent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    // 动画状态
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float animationTimer = 0f;
    private bool isAnimating = false;
    
    // 对象池相关
    private static DamageNumberPool pool;
    
    private void Awake()
    {
        // 获取组件引用
        textComponent = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        // 如果没有CanvasGroup，添加一个
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public static void ShowDamage(Vector3 worldPosition, int damage, DamageType damageType = DamageType.Normal)
    {
        ShowNumber(worldPosition, damage.ToString(), GetColorByType(damageType), damageType == DamageType.Critical);
    }
    
    /// <summary>
    /// 显示治疗数字
    /// </summary>
    public static void ShowHeal(Vector3 worldPosition, int healAmount)
    {
        ShowNumber(worldPosition, "+" + healAmount.ToString(), Color.green, false);
    }
    
    /// <summary>
    /// 显示魔法值变化
    /// </summary>
    public static void ShowMana(Vector3 worldPosition, int manaAmount)
    {
        string text = manaAmount > 0 ? "+" + manaAmount.ToString() : manaAmount.ToString();
        ShowNumber(worldPosition, text, Color.blue, false);
    }
    
    /// <summary>
    /// 显示经验值
    /// </summary>
    public static void ShowExperience(Vector3 worldPosition, int expAmount)
    {
        ShowNumber(worldPosition, "+" + expAmount.ToString() + " EXP", Color.cyan, false);
    }
    
    /// <summary>
    /// 显示自定义文本
    /// </summary>
    public static void ShowText(Vector3 worldPosition, string text, Color color, bool isCritical = false)
    {
        ShowNumber(worldPosition, text, color, isCritical);
    }
    
    /// <summary>
    /// 显示数字的核心方法
    /// </summary>
    private static void ShowNumber(Vector3 worldPosition, string text, Color color, bool isCritical)
    {
        // 获取或创建伤害数字对象
        DamageNumber damageNumber = GetDamageNumber();
        if (damageNumber == null) return;
        
        // 设置位置
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.worldCamera,
                out canvasPosition
            );
            
            damageNumber.rectTransform.anchoredPosition = canvasPosition;
        }
        
        // 设置文本和颜色
        damageNumber.SetText(text, color, isCritical);
        
        // 开始动画
        damageNumber.StartAnimation();
    }
    
    /// <summary>
    /// 获取伤害数字对象（对象池）
    /// </summary>
    private static DamageNumber GetDamageNumber()
    {
        if (pool == null)
        {
            pool = FindObjectOfType<DamageNumberPool>();
            if (pool == null)
            {
                // 创建对象池
                GameObject poolObject = new GameObject("DamageNumberPool");
                pool = poolObject.AddComponent<DamageNumberPool>();
            }
        }
        
        return pool.GetDamageNumber();
    }
    
    /// <summary>
    /// 根据伤害类型获取颜色
    /// </summary>
    private static Color GetColorByType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Normal:
                return Color.white;
            case DamageType.Critical:
                return Color.yellow;
            case DamageType.Magic:
                return Color.magenta;
            case DamageType.Poison:
                return Color.green;
            case DamageType.Fire:
                return Color.red;
            case DamageType.Ice:
                return Color.cyan;
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// 设置文本内容
    /// </summary>
    public void SetText(string text, Color color, bool isCritical = false)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
            
            // 暴击效果
            if (isCritical)
            {
                textComponent.fontSize *= 1.5f;
                textComponent.fontStyle = FontStyles.Bold;
            }
            else
            {
                textComponent.fontSize = 24f; // 默认字体大小
                textComponent.fontStyle = FontStyles.Normal;
            }
        }
    }
    
    /// <summary>
    /// 开始动画
    /// </summary>
    public void StartAnimation()
    {
        // 重置状态
        animationTimer = 0f;
        isAnimating = true;
        
        // 设置起始位置和目标位置
        startPosition = rectTransform.anchoredPosition;
        targetPosition = startPosition + (Vector3)(moveDirection * moveSpeed * duration);
        
        // 设置起始状态
        if (useScaleAnimation)
        {
            transform.localScale = Vector3.one * startScale;
        }
        
        if (useFadeAnimation)
        {
            canvasGroup.alpha = startAlpha;
        }
        
        // 开始动画协程
        StartCoroutine(AnimationCoroutine());
    }
    
    /// <summary>
    /// 动画协程
    /// </summary>
    private IEnumerator AnimationCoroutine()
    {
        while (animationTimer < duration)
        {
            animationTimer += Time.deltaTime;
            float progress = animationTimer / duration;
            
            // 位置动画
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, moveCurve.Evaluate(progress));
            rectTransform.anchoredPosition = currentPosition;
            
            // 缩放动画
            if (useScaleAnimation)
            {
                float scaleProgress = scaleCurve.Evaluate(progress);
                float currentScale;
                
                if (progress < 0.3f)
                {
                    // 前30%时间：从起始缩放到最大缩放
                    currentScale = Mathf.Lerp(startScale, maxScale, progress / 0.3f);
                }
                else
                {
                    // 后70%时间：从最大缩放到结束缩放
                    currentScale = Mathf.Lerp(maxScale, endScale, (progress - 0.3f) / 0.7f);
                }
                
                transform.localScale = Vector3.one * currentScale;
            }
            
            // 透明度动画
            if (useFadeAnimation)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, alphaCurve.Evaluate(progress));
                canvasGroup.alpha = alpha;
            }
            
            yield return null;
        }
        
        // 动画结束
        isAnimating = false;
        ReturnToPool();
    }
    
    /// <summary>
    /// 返回对象池
    /// </summary>
    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnDamageNumber(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 停止动画
    /// </summary>
    public void StopAnimation()
    {
        if (isAnimating)
        {
            StopAllCoroutines();
            isAnimating = false;
            ReturnToPool();
        }
    }
}

/// <summary>
/// 伤害类型枚举
/// </summary>
public enum DamageType
{
    Normal,
    Critical,
    Magic,
    Poison,
    Fire,
    Ice
}

/// <summary>
/// 伤害数字对象池
/// </summary>
public class DamageNumberPool : MonoBehaviour
{
    [Header("对象池设置")]
    public GameObject damageNumberPrefab;    // 伤害数字预制体
    public int poolSize = 20;                // 对象池大小
    public Transform poolParent;             // 对象池父对象
    
    private Queue<DamageNumber> pool = new Queue<DamageNumber>();
    private List<DamageNumber> activeNumbers = new List<DamageNumber>();
    
    private void Start()
    {
        InitializePool();
    }
    
    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        // 如果没有指定父对象，创建一个
        if (poolParent == null)
        {
            GameObject parent = new GameObject("DamageNumbers");
            poolParent = parent.transform;
            
            // 确保在Canvas下
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                parent.transform.SetParent(canvas.transform, false);
            }
        }
        
        // 创建预制体（如果没有指定）
        if (damageNumberPrefab == null)
        {
            damageNumberPrefab = CreateDefaultDamageNumberPrefab();
        }
        
        // 预创建对象
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(damageNumberPrefab, poolParent);
            DamageNumber damageNumber = obj.GetComponent<DamageNumber>();
            obj.SetActive(false);
            pool.Enqueue(damageNumber);
        }
    }
    
    /// <summary>
    /// 创建默认伤害数字预制体
    /// </summary>
    private GameObject CreateDefaultDamageNumberPrefab()
    {
        GameObject prefab = new GameObject("DamageNumber");
        
        // 添加RectTransform
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 50);
        
        // 添加TextMeshProUGUI
        TextMeshProUGUI text = prefab.AddComponent<TextMeshProUGUI>();
        text.text = "0";
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        // 添加CanvasGroup
        prefab.AddComponent<CanvasGroup>();
        
        // 添加DamageNumber组件
        prefab.AddComponent<DamageNumber>();
        
        return prefab;
    }
    
    /// <summary>
    /// 获取伤害数字对象
    /// </summary>
    public DamageNumber GetDamageNumber()
    {
        DamageNumber damageNumber;
        
        if (pool.Count > 0)
        {
            damageNumber = pool.Dequeue();
        }
        else
        {
            // 对象池用完了，创建新的
            GameObject obj = Instantiate(damageNumberPrefab, poolParent);
            damageNumber = obj.GetComponent<DamageNumber>();
        }
        
        damageNumber.gameObject.SetActive(true);
        activeNumbers.Add(damageNumber);
        
        return damageNumber;
    }
    
    /// <summary>
    /// 返回伤害数字对象到对象池
    /// </summary>
    public void ReturnDamageNumber(DamageNumber damageNumber)
    {
        if (activeNumbers.Contains(damageNumber))
        {
            activeNumbers.Remove(damageNumber);
        }
        
        damageNumber.gameObject.SetActive(false);
        pool.Enqueue(damageNumber);
    }
    
    /// <summary>
    /// 清理所有活跃的伤害数字
    /// </summary>
    public void ClearAllActiveNumbers()
    {
        foreach (DamageNumber damageNumber in activeNumbers.ToArray())
        {
            damageNumber.StopAnimation();
        }
    }
}