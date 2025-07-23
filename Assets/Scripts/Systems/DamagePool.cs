using System.Collections.Generic;
using UnityEngine;

public class DamagePool : MonoBehaviour
{
    public static DamagePool Instance;
    public GameObject popupPrefab;
         public Transform canvasTransform; // 引用Canvas的Transform
    public int poolSize = 20;
    
    private Queue<DamagePopup> pool = new Queue<DamagePopup>();
    
    void Awake()
    {
        Instance = this;
        InitializePool();
    }
    
    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
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
        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }
    
    void ExpandPool()
    {
        for (int i = 0; i < 5; i++) // 每次扩展5个
        {
            GameObject obj = Instantiate(popupPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj.GetComponent<DamagePopup>());
        }
    }
}