using System.Collections.Generic;
using UnityEngine;

public class DamagePool : MonoBehaviour
{
    public static DamagePool Instance;
    public GameObject popupPrefab;
         public Transform canvasTransform; // 引用Canvas的Transform
private float lastCleanupTime;
private const float CLEANUP_INTERVAL = 30f; // 每30秒清理一次
private const int MAX_POOL_SIZE = 50; // 最大池容量
    
    private Queue<DamagePopup> pool = new Queue<DamagePopup>();
    
    void Awake()
    {
        Instance = this;
        InitializePool();
    }
    
void Update()
{
    // 定期清理多余对象
    if (Time.time - lastCleanupTime > CLEANUP_INTERVAL)
    {
        CleanupPool();
        lastCleanupTime = Time.time;
    }
}
    void InitializePool()
    {
        for (int i = 0; i < MAX_POOL_SIZE; i++)
        {
                     GameObject obj = Instantiate(popupPrefab, canvasTransform);
            obj.SetActive(false);
            pool.Enqueue(obj.GetComponent<DamagePopup>());
        }
    }
    public DamagePopup GetPopup()
    {
        if (pool.Count == 0) 
            ExpandPool();
            
        DamagePopup popup = pool.Dequeue();
        popup.gameObject.SetActive(true);
        return popup;
    }
    
    public void ReturnPopup(DamagePopup popup)
    {
          // 添加空对象检查
    if (popup == null || popup.gameObject == null) return;

    // 重置状态
    popup.ResetState();
    // 隐藏
    popup.gameObject.SetActive(false);
    // 重新入队
    pool.Enqueue(popup);
    }
    
    void ExpandPool()
    {
        for (int i = 0; i < 5; i++) // 每次扩展5个
        {
            GameObject obj = Instantiate(popupPrefab, canvasTransform, false);
            obj.SetActive(false);
            pool.Enqueue(obj.GetComponent<DamagePopup>());
        }
    }
    public void CleanupPool()
{
    // 计算需要清理的对象数量
    int cleanupCount = pool.Count - MAX_POOL_SIZE;
    if (cleanupCount <= 0) return;

    // 清理多余对象
    for (int i = 0; i < cleanupCount; i++)
    {
        DamagePopup popup = pool.Dequeue();
        if (popup != null && popup.gameObject != null)
        {
            Destroy(popup.gameObject);
        }
    }
}
}