using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 脚本引用丢失诊断和修复工具
/// </summary>
public class MissingScriptFixer : EditorWindow
{
    private Vector2 scrollPosition;
    private List<GameObject> objectsWithMissingScripts = new List<GameObject>();
    private bool scanCompleted = false;

    [MenuItem("Tools/Missing Script Fixer")]
    public static void ShowWindow()
    {
        GetWindow<MissingScriptFixer>("Missing Script Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("脚本引用丢失诊断工具", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("扫描场景中的丢失脚本", GUILayout.Height(30)))
        {
            ScanForMissingScripts();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("扫描项目中的所有预制体", GUILayout.Height(30)))
        {
            ScanAllPrefabs();
        }
        
        EditorGUILayout.Space();
        
        if (scanCompleted)
        {
            if (objectsWithMissingScripts.Count > 0)
            {
                GUILayout.Label($"发现 {objectsWithMissingScripts.Count} 个对象有脚本引用丢失:", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                foreach (var obj in objectsWithMissingScripts)
                {
                    if (obj != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                        
                        if (GUILayout.Button("选择", GUILayout.Width(60)))
                        {
                            Selection.activeGameObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        }
                        
                        if (GUILayout.Button("移除丢失脚本", GUILayout.Width(100)))
                        {
                            RemoveMissingScripts(obj);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("移除所有丢失的脚本", GUILayout.Height(25)))
                {
                    RemoveAllMissingScripts();
                }
            }
            else
            {
                GUILayout.Label("没有发现脚本引用丢失的对象", EditorStyles.boldLabel);
            }
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "使用说明:\n" +
            "1. 点击'扫描场景中的丢失脚本'来检查当前场景\n" +
            "2. 点击'扫描项目中的所有预制体'来检查所有预制体\n" +
            "3. 对于发现的问题对象，可以选择移除丢失的脚本引用\n" +
            "4. 建议在移除前先备份项目",
            MessageType.Info);
    }

    private void ScanForMissingScripts()
    {
        objectsWithMissingScripts.Clear();
        
        // 获取场景中的所有GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (HasMissingScripts(obj))
            {
                objectsWithMissingScripts.Add(obj);
            }
        }
        
        scanCompleted = true;
        Debug.Log($"[MissingScriptFixer] 场景扫描完成，发现 {objectsWithMissingScripts.Count} 个对象有脚本引用丢失");
    }

    private void ScanAllPrefabs()
    {
        objectsWithMissingScripts.Clear();
        
        // 获取项目中的所有预制体
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null && HasMissingScriptsInPrefab(prefab))
            {
                objectsWithMissingScripts.Add(prefab);
            }
        }
        
        scanCompleted = true;
        Debug.Log($"[MissingScriptFixer] 预制体扫描完成，发现 {objectsWithMissingScripts.Count} 个预制体有脚本引用丢失");
    }

    private bool HasMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        
        foreach (Component component in components)
        {
            if (component == null)
            {
                return true;
            }
        }
        
        return false;
    }

    private bool HasMissingScriptsInPrefab(GameObject prefab)
    {
        // 检查预制体本身
        if (HasMissingScripts(prefab))
        {
            return true;
        }
        
        // 检查预制体的所有子对象
        Transform[] allTransforms = prefab.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (HasMissingScripts(t.gameObject))
            {
                return true;
            }
        }
        
        return false;
    }

    private void RemoveMissingScripts(GameObject obj)
    {
        Undo.RegisterCompleteObjectUndo(obj, "Remove Missing Scripts");
        
        Component[] components = obj.GetComponents<Component>();
        SerializedObject so = new SerializedObject(obj);
        SerializedProperty sp = so.FindProperty("m_Component");
        
        int removedCount = 0;
        for (int i = components.Length - 1; i >= 0; i--)
        {
            if (components[i] == null)
            {
                sp.DeleteArrayElementAtIndex(i);
                removedCount++;
            }
        }
        
        so.ApplyModifiedProperties();
        
        Debug.Log($"[MissingScriptFixer] 从 {obj.name} 移除了 {removedCount} 个丢失的脚本引用");
        
        // 如果是预制体，标记为已修改
        if (PrefabUtility.IsPartOfPrefabAsset(obj))
        {
            EditorUtility.SetDirty(obj);
        }
    }

    private void RemoveAllMissingScripts()
    {
        if (EditorUtility.DisplayDialog("确认操作", 
            $"确定要移除所有 {objectsWithMissingScripts.Count} 个对象的丢失脚本引用吗？\n\n建议先备份项目！", 
            "确定", "取消"))
        {
            foreach (GameObject obj in objectsWithMissingScripts)
            {
                if (obj != null)
                {
                    RemoveMissingScripts(obj);
                }
            }
            
            // 重新扫描
            if (objectsWithMissingScripts.Count > 0)
            {
                ScanForMissingScripts();
            }
            
            AssetDatabase.SaveAssets();
            Debug.Log("[MissingScriptFixer] 所有丢失的脚本引用已移除");
        }
    }
}