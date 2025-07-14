# Unity Sprite Editor 批处理指南 - 战士受伤图片分离

## 概述
本指南将详细说明如何使用Unity的Sprite Editor批量处理战士受伤图片，将1420x960的大尺寸图片分离成单个战士精灵。

## 前期准备

### 1. 导入图片资源
1. 将所有战士受伤图片拖拽到Unity项目的 `Assets/Art/Characters/warrior/sprite` 文件夹中
2. 确保所有图片都已正确导入到项目中

### 2. 设置图片导入设置
1. 选中所有需要处理的图片（按住Ctrl键多选）
2. 在Inspector面板中设置以下参数：
   - **Texture Type**: `Sprite (2D and UI)`
   - **Sprite Mode**: `Multiple` （重要：这允许一张图片包含多个精灵）
   - **Pixels Per Unit**: `100` （根据你的游戏需求调整）
   - **Filter Mode**: `Point (no filter)` （像素艺术推荐）
   - **Compression**: `None` 或 `Normal Quality`
3. 点击 `Apply` 应用设置

## 使用Sprite Editor进行批处理

### 方法一：自动切片（推荐用于规则排列的精灵）

#### 1. 打开Sprite Editor
1. 选中一张图片
2. 在Inspector面板中点击 `Sprite Editor` 按钮
3. Sprite Editor窗口将打开

#### 2. 自动切片设置
1. 在Sprite Editor窗口顶部，点击 `Slice` 按钮
2. 在弹出的Slice窗口中设置参数：
   - **Type**: `Automatic` 或 `Grid By Cell Size`
   - 如果选择 `Grid By Cell Size`：
     - **Pixel Size**: 设置单个精灵的尺寸（例如：X=64, Y=64）
   - 如果选择 `Automatic`：
     - **Method**: `Smart` （智能检测精灵边界）
     - **Pivot**: `Center` 或 `Bottom`
     - **Custom Pivot**: 根据需要调整
3. 点击 `Slice` 执行切片

#### 3. 手动调整切片区域
1. 切片完成后，你会看到图片上出现了多个矩形框
2. 可以手动调整每个矩形框的位置和大小：
   - 拖拽矩形框的边缘来调整大小
   - 拖拽矩形框的中心来移动位置
3. 删除不需要的切片区域：选中后按Delete键
4. 添加新的切片区域：在空白处拖拽创建新矩形

#### 4. 命名精灵
1. 选中每个切片区域
2. 在右侧面板中修改 `Name` 字段
3. 建议使用规范的命名格式：
   - `warrior_hurt_01`
   - `warrior_hurt_02`
   - `warrior_hurt_03`
   - 等等

#### 5. 应用更改
1. 完成所有调整后，点击右上角的 `Apply` 按钮
2. 关闭Sprite Editor窗口

### 方法二：批量处理多张图片

#### 1. 创建切片模板
1. 先对一张图片进行完整的切片设置
2. 记录下切片的参数和位置

#### 2. 批量应用设置
1. 选中所有需要相同切片设置的图片
2. 在Inspector中，如果图片布局相同，可以：
   - 复制第一张图片的切片设置
   - 使用脚本批量应用（见下方脚本部分）

## 高级技巧

### 1. 使用脚本批量处理
创建一个Editor脚本来批量处理精灵切片：

```csharp
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SpriteBatchProcessor : EditorWindow
{
    [MenuItem("Tools/Sprite Batch Processor")]
    public static void ShowWindow()
    {
        GetWindow<SpriteBatchProcessor>("Sprite Batch Processor");
    }

    private Vector2 spriteSize = new Vector2(64, 64);
    private Vector2 offset = Vector2.zero;
    private Vector2 padding = Vector2.zero;

    void OnGUI()
    {
        GUILayout.Label("批量精灵切片工具", EditorStyles.boldLabel);
        
        spriteSize = EditorGUILayout.Vector2Field("精灵尺寸", spriteSize);
        offset = EditorGUILayout.Vector2Field("偏移量", offset);
        padding = EditorGUILayout.Vector2Field("间距", padding);
        
        if (GUILayout.Button("批量处理选中的纹理"))
        {
            ProcessSelectedTextures();
        }
    }

    void ProcessSelectedTextures()
    {
        Object[] selectedObjects = Selection.objects;
        
        foreach (Object obj in selectedObjects)
        {
            if (obj is Texture2D)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    
                    List<SpriteMetaData> spritesheet = new List<SpriteMetaData>();
                    Texture2D texture = obj as Texture2D;
                    
                    int cols = Mathf.FloorToInt((texture.width - offset.x) / (spriteSize.x + padding.x));
                    int rows = Mathf.FloorToInt((texture.height - offset.y) / (spriteSize.y + padding.y));
                    
                    for (int row = 0; row < rows; row++)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            SpriteMetaData sprite = new SpriteMetaData();
                            sprite.name = obj.name + "_" + (row * cols + col).ToString("D2");
                            sprite.rect = new Rect(
                                offset.x + col * (spriteSize.x + padding.x),
                                texture.height - offset.y - (row + 1) * (spriteSize.y + padding.y),
                                spriteSize.x,
                                spriteSize.y
                            );
                            sprite.pivot = new Vector2(0.5f, 0.5f);
                            spritesheet.Add(sprite);
                        }
                    }
                    
                    importer.spritesheet = spritesheet.ToArray();
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
        }
        
        Debug.Log("批量处理完成！");
    }
}
```

### 2. 优化工作流程

#### 批量重命名
1. 处理完成后，可以使用批量重命名工具
2. 选中所有生成的精灵
3. 使用规范的命名约定

#### 创建动画
1. 将切片好的精灵拖拽到Scene中
2. Unity会自动询问是否创建动画
3. 选择保存位置并创建动画文件

## 常见问题解决

### 1. 切片不准确
**问题**: 自动切片无法准确识别精灵边界
**解决方案**:
- 使用手动切片模式
- 调整 `Minimum Size` 参数
- 检查图片是否有透明边框

### 2. 精灵模糊
**问题**: 切片后的精灵显示模糊
**解决方案**:
- 设置 `Filter Mode` 为 `Point (no filter)`
- 确保 `Pixels Per Unit` 设置正确
- 检查相机的 `Projection` 设置

### 3. 批量处理失败
**问题**: 脚本批量处理时出现错误
**解决方案**:
- 检查图片格式是否支持
- 确保图片已正确导入
- 验证切片参数是否合理

### 4. 内存占用过高
**问题**: 处理大量高分辨率图片时内存不足
**解决方案**:
- 分批处理图片
- 降低图片分辨率
- 使用压缩格式

## 最佳实践

### 1. 文件组织
- 将原始大图和切片后的精灵分别存放
- 使用清晰的文件夹结构
- 建立命名规范

### 2. 性能优化
- 合理设置图片压缩
- 使用图集（Sprite Atlas）合并小精灵
- 避免过度细分精灵

### 3. 版本控制
- 保留原始大图文件
- 记录切片参数
- 建立处理流程文档

## 总结

通过以上方法，你可以高效地将1420x960的大尺寸战士受伤图片批量分离成单个精灵。建议先用一张图片测试流程，确定最佳参数后再批量处理所有图片。记住保存好原始文件，以备后续调整使用。