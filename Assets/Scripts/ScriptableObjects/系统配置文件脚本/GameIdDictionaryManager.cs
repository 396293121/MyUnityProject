using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// GameIdDictionary管理器 - 单例模式管理ID字典的加载和访问
/// </summary>
public class GameIdDictionaryManager : MonoBehaviour
{
    private static GameIdDictionaryManager _instance;
    public static GameIdDictionaryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameIdDictionaryManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameIdDictionaryManager");
                    _instance = go.AddComponent<GameIdDictionaryManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [BoxGroup("ID字典配置")]
    [LabelText("ID字典引用")]
    [InfoBox("自动加载的ID字典，无需手动设置")]
    [ReadOnly]
    [ShowInInspector]
    public GameIdDictionary IdDictionary
    {
        get
        {
            if (_idDictionary == null)
            {
                LoadIdDictionary();
            }
            return _idDictionary;
        }
    }
    
    [System.NonSerialized]
    private GameIdDictionary _idDictionary;
    
    // ID字典的固定路径
    private const string ID_DICTIONARY_PATH = "Assets/Data/Configs/GameIdDictionary.asset";
    private const string ID_DICTIONARY_RESOURCE_PATH = "GameIdDictionary";
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadIdDictionary();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 从指定路径加载ID字典
    /// </summary>
    private void LoadIdDictionary()
    {
#if UNITY_EDITOR
        // 编辑器模式下从Assets路径加载
        _idDictionary = UnityEditor.AssetDatabase.LoadAssetAtPath<GameIdDictionary>(ID_DICTIONARY_PATH);
        if (_idDictionary == null)
        {
            Debug.LogWarning($"[GameIdDictionaryManager] 无法在路径 {ID_DICTIONARY_PATH} 找到GameIdDictionary资源文件");
        }
        else
        {
        }
#else
        // 运行时从Resources文件夹加载
        _idDictionary = Resources.Load<GameIdDictionary>(ID_DICTIONARY_RESOURCE_PATH);
        if (_idDictionary == null)
        {
            Debug.LogWarning($"[GameIdDictionaryManager] 无法从Resources加载GameIdDictionary资源文件，请确保文件位于Resources/{ID_DICTIONARY_RESOURCE_PATH}");
        }
        else
        {
            Debug.Log($"[GameIdDictionaryManager] 已从Resources加载ID字典");
        }
#endif
    }
    
    /// <summary>
    /// 手动重新加载ID字典
    /// </summary>
    [BoxGroup("ID字典配置")]
    [Button("重新加载ID字典")]
    [InfoBox("如果ID字典文件有更新，点击此按钮重新加载")]
    public void ReloadIdDictionary()
    {
        _idDictionary = null;
        LoadIdDictionary();
        Debug.Log("[GameIdDictionaryManager] ID字典已重新加载");
    }
    
    /// <summary>
    /// 获取ID字典实例（静态方法）
    /// </summary>
    public static GameIdDictionary GetIdDictionary()
    {
        return Instance.IdDictionary;
    }
    
    /// <summary>
    /// 检查ID字典是否已加载
    /// </summary>
    public static bool IsIdDictionaryLoaded()
    {
        return Instance._idDictionary != null;
    }
}