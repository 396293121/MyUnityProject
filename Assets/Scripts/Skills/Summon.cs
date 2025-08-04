using System;
using UnityEngine;

public class Summon:MonoBehaviour
{

    [SerializeField]
    [Tooltip("召唤物预制体")]   
     public Enemy enemyPrefab;

    public GameObject InitializeSummon(int health, Vector3 size, Transform skillSpawnPoint)
    {
        GameObject gameObject = Instantiate(enemyPrefab.gameObject, skillSpawnPoint.position, skillSpawnPoint.rotation);
        Enemy enemy = gameObject.GetComponent<Enemy>();
        enemy.currentHealth = health;
        enemy.transform.localScale = size;

        // 添加自动销毁组件
        _ = gameObject.AddComponent<SummonAutoDestroy>();
        // 注意：实际的生存时间将在SkillDataConfig中设置

        return gameObject;
    }
    
}