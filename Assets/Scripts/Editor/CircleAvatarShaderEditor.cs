using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CircleAvatarShaderEditor))]
public class CircleAvatarShaderEditor : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        
        Material material = materialEditor.target as Material;
        
        // 自动计算宽高比
        Texture mainTex = material.GetTexture("_MainTex");
        if (mainTex != null)
        {
            float aspect = (float)mainTex.width / mainTex.height;
            material.SetFloat("_AspectRatio", aspect);
        }
        
        // 添加预览按钮
        if (GUILayout.Button("Update Preview"))
        {
            Texture2D preview = AssetPreview.GetAssetPreview(material);
            if (preview != null)
            {
                // 保存预览图
                byte[] bytes = preview.EncodeToPNG();
                System.IO.File.WriteAllBytes(Application.dataPath + "/AvatarPreview.png", bytes);
                AssetDatabase.Refresh();
            }
        }
    }
}