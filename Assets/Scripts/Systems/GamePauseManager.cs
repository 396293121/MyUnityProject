using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
// GamePauseManager.cs
public class GamePauseManager : MonoBehaviour
{
    // 单例模式
    public static GamePauseManager Instance { get; private set; }
    
    // 暂停状态
    public bool IsPaused { get; private set; }
    
    // 需要暂停的组件列表
    private List<IPausable> pausables = new List<IPausable>();
    
     void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 注册可暂停对象
    public void Register(IPausable pausable) => pausables.Add(pausable);
    
    // 暂停游戏
    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        
        // 更新所有注册组件
        foreach(var p in pausables)
        {
            p.SetPaused(paused);
        }
        
        // 特殊处理：粒子系统
        var particles = FindObjectsOfType<ParticleSystem>();
        foreach(var ps in particles)
        {
            if(paused) ps.Pause();
            else ps.Play();
        }
    }

    // 平滑暂停游戏
    public IEnumerator SmoothPause(bool pause, float duration)
    {
        float elapsed = 0f;
        Color startColor = Time.timeScale == 0 ? Color.black : Color.white;
        Color endColor = pause ? Color.black : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Time.timeScale = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        Time.timeScale = pause ? 0f : 1f;
    }
}

// 可暂停接口
public interface IPausable
{
    void SetPaused(bool paused);
}