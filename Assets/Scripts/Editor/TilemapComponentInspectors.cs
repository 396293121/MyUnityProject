// #if UNITY_EDITOR
// using UnityEngine;
// using UnityEngine.Tilemaps;
// using UnityEditor;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities.Editor;
// using System.Collections.Generic;
// using System.Linq;

// namespace CustomInspectors
// {
//     /// <summary>
//     /// Grid组件的自定义Inspector
//     /// 为网格属性添加中文标签和说明
//     /// </summary>
//     [CustomEditor(typeof(Grid))]
//     public class GridInspector : OdinEditor
//     {
//         private bool showCellSettings = true;
//         private bool showUsageTips = false;
        
//         public override void OnInspectorGUI()
//         {
//             Grid grid = (Grid)target;
            
//             EditorGUILayout.Space();
            
//             // 标题
//             SirenixEditorGUI.Title("Grid (网格)", "定义瓦片地图的网格系统和单元格布局", TextAlignment.Left, true);
            
//             EditorGUILayout.Space();
            
//             var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            
//             // 单元格设置
//             showCellSettings = EditorGUILayout.Foldout(showCellSettings, "Cell Settings (单元格设置)", true, EditorStyles.foldoutHeader);
//             if (showCellSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 每个网格单元格的尺寸大小，影响瓦片的显示大小", helpStyle);
//                 grid.cellSize = EditorGUILayout.Vector3Field(
//                     "Cell Size (单元格大小)", 
//                     grid.cellSize);
                
//                 if (grid.cellSize.x <= 0 || grid.cellSize.y <= 0)
//                 {
//                     EditorGUILayout.HelpBox("单元格大小必须大于0", MessageType.Warning);
//                 }
                
//                 EditorGUILayout.Space();
                
//                 EditorGUILayout.LabelField("ℹ️ 单元格之间的间隙大小，用于创建网格线效果", helpStyle);
//                 grid.cellGap = EditorGUILayout.Vector3Field(
//                     "Cell Gap (单元格间隙)", 
//                     grid.cellGap);
                
//                 EditorGUILayout.Space();
                
//                 EditorGUILayout.LabelField("ℹ️ 网格单元格的布局方式：Rectangle矩形布局，Hexagon六边形布局", helpStyle);
//                 grid.cellLayout = (GridLayout.CellLayout)EditorGUILayout.EnumPopup(
//                     "Cell Layout (单元格布局)", 
//                     grid.cellLayout);
                
//                 EditorGUILayout.Space();
                
//                 EditorGUILayout.LabelField("ℹ️ 坐标轴重新映射：XYZ表示X轴映射到X，Y轴映射到Y，Z轴映射到Z", helpStyle);
//                 grid.cellSwizzle = (GridLayout.CellSwizzle)EditorGUILayout.EnumPopup(
//                     "Cell Swizzle (单元格重排)", 
//                     grid.cellSwizzle);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 网格信息
//             EditorGUILayout.BeginVertical("box");
//             EditorGUILayout.LabelField("Grid Information (网格信息)", EditorStyles.boldLabel);
            
//             EditorGUI.BeginDisabledGroup(true);
            
//             // 显示网格的世界坐标
//             Vector3 worldPos = grid.transform.position;
//             EditorGUILayout.Vector3Field("World Position (世界位置)", worldPos);
            
//             // 显示单元格数量（如果有子对象Tilemap）
//             Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
//             EditorGUILayout.IntField("Child Tilemaps (子瓦片地图数量)", tilemaps.Length);
            
//             EditorGUI.EndDisabledGroup();
            
//             EditorGUILayout.EndVertical();
            
//             EditorGUILayout.Space();
            
//             // 使用说明
//             showUsageTips = EditorGUILayout.Foldout(showUsageTips, "Usage Tips (使用提示)", true, EditorStyles.foldoutHeader);
//             if (showUsageTips)
//             {
//                 EditorGUILayout.BeginVertical("box");
//                 EditorGUILayout.LabelField("Grid 配置指南:", EditorStyles.boldLabel);
                
//                 EditorGUILayout.LabelField("• Cell Size: 根据精灵大小设置，通常为1x1", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Cell Gap: 一般保持为0，除非需要网格线效果", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Cell Layout: Rectangle适用于大多数2D游戏", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Cell Swizzle: 通常保持XYZ，特殊需求时调整", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             if (GUI.changed)
//             {
//                 EditorUtility.SetDirty(grid);
//             }
//         }
//     }
    
//     /// <summary>
//     /// Tilemap组件的自定义Inspector
//     /// 为瓦片地图属性添加中文标签和说明
//     /// </summary>
//     [CustomEditor(typeof(Tilemap))]
//     public class TilemapInspector : OdinEditor
//     {
//         private bool showAnimationSettings = true;
//         private bool showRenderingSettings = true;
//         private bool showTransformSettings = true;
//         private bool showUsageTips = false;
        
//         public override void OnInspectorGUI()
//         {
//             Tilemap tilemap = (Tilemap)target;
            
//             EditorGUILayout.Space();
            
//             // 标题
//             SirenixEditorGUI.Title("Tilemap (瓦片地图)", "存储和管理瓦片数据的核心组件", TextAlignment.Left, true);
            
//             EditorGUILayout.Space();
            
//             var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            
//             // 动画设置
//             showAnimationSettings = EditorGUILayout.Foldout(showAnimationSettings, "Animation Settings (动画设置)", true, EditorStyles.foldoutHeader);
//             if (showAnimationSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 控制瓦片动画的播放速度，单位为帧每秒", helpStyle);
//                 tilemap.animationFrameRate = EditorGUILayout.FloatField(
//                     "Animation Frame Rate (动画帧率)", 
//                     tilemap.animationFrameRate);
                
//                 if (tilemap.animationFrameRate < 0)
//                 {
//                     EditorGUILayout.HelpBox("动画帧率不能为负数", MessageType.Warning);
//                 }
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 渲染设置
//             showRenderingSettings = EditorGUILayout.Foldout(showRenderingSettings, "Rendering Settings (渲染设置)", true, EditorStyles.foldoutHeader);
//             if (showRenderingSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 调制所有瓦片的颜色，白色为原始颜色", helpStyle);
//                 tilemap.color = EditorGUILayout.ColorField(
//                     "Color (颜色)", 
//                     tilemap.color);
                
//                 EditorGUILayout.LabelField("ℹ️ 瓦片在单元格中的对齐方式", helpStyle);
//                 tilemap.tileAnchor = EditorGUILayout.Vector3Field(
//                     "Tile Anchor (瓦片锚点)", 
//                     tilemap.tileAnchor);
                
//                 EditorGUILayout.LabelField("ℹ️ 瓦片地图的朝向，影响瓦片的排列方向", helpStyle);
//                 tilemap.orientation = (Tilemap.Orientation)EditorGUILayout.EnumPopup(
//                     "Orientation (方向)", 
//                     tilemap.orientation);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 变换设置
//             showTransformSettings = EditorGUILayout.Foldout(showTransformSettings, "Transform Settings (变换设置)", true, EditorStyles.foldoutHeader);
//             if (showTransformSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 瓦片地图的位置偏移量", helpStyle);
//                 Vector3 offset = tilemap.transform.localPosition;
//                 Vector3 newOffset = EditorGUILayout.Vector3Field("Offset (偏移)", offset);
//                 if (newOffset != offset)
//                 {
//                     tilemap.transform.localPosition = newOffset;
//                 }
                
//                 EditorGUILayout.LabelField("ℹ️ 瓦片地图的旋转角度", helpStyle);
//                 Vector3 rotation = tilemap.transform.localEulerAngles;
//                 Vector3 newRotation = EditorGUILayout.Vector3Field("Rotation (旋转)", rotation);
//                 if (newRotation != rotation)
//                 {
//                     tilemap.transform.localEulerAngles = newRotation;
//                 }
                
//                 EditorGUILayout.LabelField("ℹ️ 瓦片地图的缩放比例", helpStyle);
//                 Vector3 scale = tilemap.transform.localScale;
//                 Vector3 newScale = EditorGUILayout.Vector3Field("Scale (缩放)", scale);
//                 if (newScale != scale)
//                 {
//                     tilemap.transform.localScale = newScale;
//                 }
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 瓦片地图信息
//             EditorGUILayout.BeginVertical("box");
//             EditorGUILayout.LabelField("Tilemap Information (瓦片地图信息)", EditorStyles.boldLabel);
            
//             EditorGUI.BeginDisabledGroup(true);
            
//             // 显示瓦片数量
//             BoundsInt bounds = tilemap.cellBounds;
//             int tileCount = 0;
//             for (int x = bounds.xMin; x < bounds.xMax; x++)
//             {
//                 for (int y = bounds.yMin; y < bounds.yMax; y++)
//                 {
//                     if (tilemap.HasTile(new Vector3Int(x, y, 0)))
//                         tileCount++;
//                 }
//             }
//             EditorGUILayout.IntField("Tile Count (瓦片数量)", tileCount);
            
//             // 显示边界
//             EditorGUILayout.Vector3IntField("Cell Bounds Min (单元格边界最小值)", bounds.min);
//             EditorGUILayout.Vector3IntField("Cell Bounds Max (单元格边界最大值)", bounds.max);
            
//             EditorGUI.EndDisabledGroup();
            
//             EditorGUILayout.EndVertical();
            
//             EditorGUILayout.Space();
            
//             // 使用说明
//             showUsageTips = EditorGUILayout.Foldout(showUsageTips, "Usage Tips (使用提示)", true, EditorStyles.foldoutHeader);
//             if (showUsageTips)
//             {
//                 EditorGUILayout.BeginVertical("box");
//                 EditorGUILayout.LabelField("Tilemap 配置指南:", EditorStyles.boldLabel);
                
//                 EditorGUILayout.LabelField("• Animation Frame Rate: 控制瓦片动画速度", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Color: 可用于整体色调调整", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Tile Anchor: 影响瓦片在单元格中的位置", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Orientation: XY适用于大多数2D游戏", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Transform: 可用于整体地图的位移和缩放", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             if (GUI.changed)
//             {
//                 EditorUtility.SetDirty(tilemap);
//             }
//         }
//     }
    
//     /// <summary>
//     /// TilemapRenderer组件的自定义Inspector
//     /// 为瓦片地图渲染器属性添加中文标签和说明
//     /// </summary>
//     [CustomEditor(typeof(TilemapRenderer))]
//     public class TilemapRendererInspector : OdinEditor
//     {
//         private bool showMaterialSettings = true;
//         private bool showSortingSettings = true;
//         private bool showOptimizationSettings = true;
//         private bool showUsageTips = false;
        
//         public override void OnInspectorGUI()
//         {
//             TilemapRenderer renderer = (TilemapRenderer)target;
            
//             EditorGUILayout.Space();
            
//             // 标题
//             SirenixEditorGUI.Title("Tilemap Renderer (瓦片地图渲染器)", "负责渲染瓦片地图的视觉效果", TextAlignment.Left, true);
            
//             EditorGUILayout.Space();
            
//             var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            
//             // 材质设置
//             showMaterialSettings = EditorGUILayout.Foldout(showMaterialSettings, "Material Settings (材质设置)", true, EditorStyles.foldoutHeader);
//             if (showMaterialSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 用于渲染瓦片的材质，影响视觉效果和着色器", helpStyle);
//                 renderer.material = (Material)EditorGUILayout.ObjectField(
//                     "Material (材质)", 
//                     renderer.material, 
//                     typeof(Material), 
//                     false);
                
//                 EditorGUILayout.LabelField("ℹ️ 精灵遮罩的交互方式：None无交互，VisibleInsideMask在遮罩内可见", helpStyle);
//                 renderer.maskInteraction = (SpriteMaskInteraction)EditorGUILayout.EnumPopup(
//                     "Mask Interaction (遮罩交互)", 
//                     renderer.maskInteraction);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 排序设置
//             showSortingSettings = EditorGUILayout.Foldout(showSortingSettings, "Sorting Settings (排序设置)", true, EditorStyles.foldoutHeader);
//             if (showSortingSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 渲染排序层，决定与其他对象的渲染顺序", helpStyle);
                
//                 // 排序层
//                 string[] sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
//                 int currentIndex = System.Array.IndexOf(sortingLayerNames, renderer.sortingLayerName);
//                 if (currentIndex == -1) currentIndex = 0;
                
//                 int newIndex = EditorGUILayout.Popup("Sorting Layer (排序层)", currentIndex, sortingLayerNames);
//                 if (newIndex != currentIndex && newIndex >= 0 && newIndex < sortingLayerNames.Length)
//                 {
//                     renderer.sortingLayerName = sortingLayerNames[newIndex];
//                 }
                
//                 EditorGUILayout.LabelField("ℹ️ 在同一排序层内的渲染顺序，数值越大越靠前", helpStyle);
//                 renderer.sortingOrder = EditorGUILayout.IntField(
//                     "Order in Layer (层内顺序)", 
//                     renderer.sortingOrder);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 优化设置
//             showOptimizationSettings = EditorGUILayout.Foldout(showOptimizationSettings, "Optimization Settings (优化设置)", true, EditorStyles.foldoutHeader);
//             if (showOptimizationSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 渲染模式：Individual逐个渲染，Chunk批量渲染（推荐）", helpStyle);
//                 renderer.mode = (TilemapRenderer.Mode)EditorGUILayout.EnumPopup(
//                     "Mode (模式)", 
//                     renderer.mode);
                
//                 if (renderer.mode == TilemapRenderer.Mode.Chunk)
//                 {
//                     EditorGUILayout.LabelField("ℹ️ 每个渲染块的大小，影响批处理效率", helpStyle);
//                     renderer.chunkSize = EditorGUILayout.Vector3IntField(
//                         "Chunk Size (块大小)", 
//                         renderer.chunkSize);
                    
//                     EditorGUILayout.LabelField("ℹ️ 最大渲染块数量，控制内存使用", helpStyle);
//                     renderer.maxChunkCount = EditorGUILayout.IntField(
//                         "Max Chunk Count (最大块数量)", 
//                         renderer.maxChunkCount);
                    
//                     EditorGUILayout.LabelField("ℹ️ 块的最大存活帧数，用于缓存管理", helpStyle);
//                     renderer.maxFrameAge = EditorGUILayout.IntField(
//                         "Max Frame Age (最大帧龄)", 
//                         renderer.maxFrameAge);
                    
//                     EditorGUILayout.LabelField("ℹ️ 自动检测块剔除边界以优化渲染", helpStyle);
//                     renderer.detectChunkCullingBounds = (TilemapRenderer.DetectChunkCullingBounds)EditorGUILayout.EnumPopup(
//                         "Detect Chunk Culling Bounds (检测块剔除边界)", 
//                         renderer.detectChunkCullingBounds);
                    
//                     if (renderer.detectChunkCullingBounds == TilemapRenderer.DetectChunkCullingBounds.Manual)
//                     {
//                         EditorGUILayout.LabelField("ℹ️ 手动设置块剔除边界", helpStyle);
//                         renderer.chunkCullingBounds = EditorGUILayout.Vector3Field(
//                             "Chunk Culling Bounds (块剔除边界)", 
//                             renderer.chunkCullingBounds);
//                     }
//                 }
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 渲染器信息
//             EditorGUILayout.BeginVertical("box");
//             EditorGUILayout.LabelField("Renderer Information (渲染器信息)", EditorStyles.boldLabel);
            
//             EditorGUI.BeginDisabledGroup(true);
            
//             // 显示当前使用的材质信息
//             if (renderer.material != null)
//             {
//                 EditorGUILayout.TextField("Material Name (材质名称)", renderer.material.name);
//                 EditorGUILayout.TextField("Shader Name (着色器名称)", renderer.material.shader.name);
//             }
            
//             // 显示排序信息
//             EditorGUILayout.TextField("Current Sorting Layer (当前排序层)", renderer.sortingLayerName);
//             EditorGUILayout.IntField("Sorting Layer ID (排序层ID)", renderer.sortingLayerID);
            
//             EditorGUI.EndDisabledGroup();
            
//             EditorGUILayout.EndVertical();
            
//             EditorGUILayout.Space();
            
//             // 使用说明
//             showUsageTips = EditorGUILayout.Foldout(showUsageTips, "Usage Tips (使用提示)", true, EditorStyles.foldoutHeader);
//             if (showUsageTips)
//             {
//                 EditorGUILayout.BeginVertical("box");
//                 EditorGUILayout.LabelField("TilemapRenderer 配置指南:", EditorStyles.boldLabel);
                
//                 EditorGUILayout.LabelField("• Mode: 大型地图推荐使用Chunk模式", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Chunk Size: 通常设置为32x32或64x64", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Sorting Layer: 背景层设置较小值，前景层设置较大值", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Material: 使用Sprites/Default或自定义材质", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("• Culling Bounds: Auto模式适用于大多数情况", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             if (GUI.changed)
//             {
//                 EditorUtility.SetDirty(renderer);
//             }
//         }
//     }
    
//     /// <summary>
//     /// RuleTile组件的自定义Inspector
//     /// 为规则瓦片属性添加中文标签和说明
//     /// </summary>
//     [CustomEditor(typeof(RuleTile))]
//     public class RuleTileInspector : OdinEditor
//     {
//         private bool showDefaultSettings = true;
//         private bool showTilingRules = true;
//         private bool showUsageTips = false;
        
//         public override void OnInspectorGUI()
//         {
//             RuleTile ruleTile = (RuleTile)target;
            
//             EditorGUILayout.Space();
            
//             // 标题
//             SirenixEditorGUI.Title("Rule Tile (规则瓦片)", "基于邻居规则自动选择合适精灵的智能瓦片系统", TextAlignment.Left, true);
            
//             EditorGUILayout.Space();
            
//             var helpStyle = new GUIStyle(EditorStyles.helpBox) { fontSize = 11 };
            
//             // 默认设置
//             showDefaultSettings = EditorGUILayout.Foldout(showDefaultSettings, "Default Settings (默认设置)", true, EditorStyles.foldoutHeader);
//             if (showDefaultSettings)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 创建新规则时使用的默认精灵", helpStyle);
//                 ruleTile.m_DefaultSprite = (Sprite)EditorGUILayout.ObjectField(
//                     "Default Sprite (默认精灵)", 
//                     ruleTile.m_DefaultSprite, 
//                     typeof(Sprite), 
//                     false);
                
//                 EditorGUILayout.LabelField("ℹ️ 创建新规则时使用的默认游戏对象", helpStyle);
//                 ruleTile.m_DefaultGameObject = (GameObject)EditorGUILayout.ObjectField(
//                     "Default GameObject (默认游戏对象)", 
//                     ruleTile.m_DefaultGameObject, 
//                     typeof(GameObject), 
//                     false);
                
//                 EditorGUILayout.LabelField("ℹ️ 创建新规则时使用的默认碰撞器类型：None无碰撞器，Sprite精灵形状，Grid网格形状", helpStyle);
//                 ruleTile.m_DefaultColliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup(
//                     "Default Collider Type (默认碰撞器类型)", 
//                     ruleTile.m_DefaultColliderType);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 瓦片规则
//             showTilingRules = EditorGUILayout.Foldout(showTilingRules, $"Tiling Rules (瓦片规则) - {ruleTile.m_TilingRules.Count} 条规则", true, EditorStyles.foldoutHeader);
//             if (showTilingRules)
//             {
//                 EditorGUILayout.BeginVertical("box");
                
//                 EditorGUILayout.LabelField("ℹ️ 规则列表定义了在不同邻居配置下应该显示的精灵", helpStyle);
                
//                 // 规则统计信息
//                 EditorGUILayout.BeginHorizontal();
//                 EditorGUILayout.LabelField($"总规则数: {ruleTile.m_TilingRules.Count}", EditorStyles.miniLabel);
                
//                 int animatedRules = ruleTile.m_TilingRules.Count(r => r.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Animation);
//                 int randomRules = ruleTile.m_TilingRules.Count(r => r.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Random);
//                 int singleRules = ruleTile.m_TilingRules.Count(r => r.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Single);
                
//                 EditorGUILayout.LabelField($"单一: {singleRules} | 随机: {randomRules} | 动画: {animatedRules}", EditorStyles.miniLabel);
//                 EditorGUILayout.EndHorizontal();
                
//                 EditorGUILayout.Space();
                
//                 // 显示前几个规则的简要信息
//                 if (ruleTile.m_TilingRules.Count > 0)
//                 {
//                     EditorGUILayout.LabelField("规则预览 (前5条):", EditorStyles.boldLabel);
                    
//                     for (int i = 0; i < Mathf.Min(5, ruleTile.m_TilingRules.Count); i++)
//                     {
//                         var rule = ruleTile.m_TilingRules[i];
//                         EditorGUILayout.BeginHorizontal("box");
                        
//                         // 显示规则的主要精灵
//                         if (rule.m_Sprites != null && rule.m_Sprites.Length > 0 && rule.m_Sprites[0] != null)
//                         {
//                             Texture2D texture = AssetPreview.GetAssetPreview(rule.m_Sprites[0]);
//                             if (texture != null)
//                             {
//                                 GUILayout.Label(texture, GUILayout.Width(32), GUILayout.Height(32));
//                             }
//                         }
//                         else
//                         {
//                             GUILayout.Label("无精灵", GUILayout.Width(32), GUILayout.Height(32));
//                         }
                        
//                         EditorGUILayout.BeginVertical();
//                         EditorGUILayout.LabelField($"规则 {i + 1}", EditorStyles.boldLabel);
//                         EditorGUILayout.LabelField($"输出类型: {GetOutputTypeDisplayName(rule.m_Output)}");
//                         EditorGUILayout.LabelField($"变换: {GetTransformDisplayName(rule.m_RuleTransform)}");
//                         if (rule.m_Sprites != null && rule.m_Sprites.Length > 1)
//                         {
//                             EditorGUILayout.LabelField($"精灵数量: {rule.m_Sprites.Length}");
//                         }
//                         EditorGUILayout.EndVertical();
                        
//                         EditorGUILayout.EndHorizontal();
//                     }
                    
//                     if (ruleTile.m_TilingRules.Count > 5)
//                     {
//                         EditorGUILayout.LabelField($"... 还有 {ruleTile.m_TilingRules.Count - 5} 条规则", EditorStyles.centeredGreyMiniLabel);
//                     }
//                 }
//                 else
//                 {
//                     EditorGUILayout.HelpBox("暂无规则。请使用Rule Tile Editor窗口添加规则。", MessageType.Info);
//                 }
                
//                 EditorGUILayout.Space();
                
//                 // 打开Rule Tile Editor按钮
//                 EditorGUILayout.BeginHorizontal();
                
//                 if (GUILayout.Button("选中 Rule Tile 资产", GUILayout.Height(25)))
//                 {
//                     // 选中并聚焦到 RuleTile 资产
//                     Selection.activeObject = ruleTile;
//                     EditorGUIUtility.PingObject(ruleTile);
//                 }
                
//                 if (GUILayout.Button("在 Project 中显示", GUILayout.Height(25)))
//                 {
//                     // 在 Project 窗口中高亮显示资产
//                     EditorUtility.FocusProjectWindow();
//                     Selection.activeObject = ruleTile;
//                     EditorGUIUtility.PingObject(ruleTile);
//                 }
                
//                 EditorGUILayout.EndHorizontal();
                
//                 EditorGUILayout.Space(5);
                
//                 // 添加使用说明
//                 EditorGUILayout.BeginVertical("helpbox");
//                 EditorGUILayout.LabelField("💡 编辑规则说明:", EditorStyles.boldLabel);
//                 EditorGUILayout.LabelField("1. 点击上方按钮选中 Rule Tile 资产", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("2. 在 Inspector 中查看和编辑瓦片规则", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("3. 或者直接在 Project 窗口中双击 Rule Tile 资产", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.EndVertical();
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             EditorGUILayout.Space();
            
//             // 规则瓦片信息
//             EditorGUILayout.BeginVertical("box");
//             EditorGUILayout.LabelField("Rule Tile Information (规则瓦片信息)", EditorStyles.boldLabel);
            
//             EditorGUI.BeginDisabledGroup(true);
            
//             // 计算邻居位置数量
//             int neighborPositionCount = ruleTile.neighborPositions?.Count ?? 0;
//             EditorGUILayout.IntField("Neighbor Positions (邻居位置数量)", neighborPositionCount);
            
//             // 显示旋转角度
//             EditorGUILayout.IntField("Rotation Angle (旋转角度)", ruleTile.m_RotationAngle);
            
//             // 显示旋转次数
//             int rotationCount = 360 / Mathf.Max(ruleTile.m_RotationAngle, 1);
//             EditorGUILayout.IntField("Rotation Count (旋转次数)", rotationCount);
            
//             EditorGUI.EndDisabledGroup();
            
//             EditorGUILayout.EndVertical();
            
//             EditorGUILayout.Space();
            
//             // 使用说明
//             showUsageTips = EditorGUILayout.Foldout(showUsageTips, "Usage Tips (使用提示)", true, EditorStyles.foldoutHeader);
//             if (showUsageTips)
//             {
//                 EditorGUILayout.BeginVertical("box");
//                 EditorGUILayout.LabelField("Rule Tile 配置指南:", EditorStyles.boldLabel);
                
//                 EditorGUILayout.LabelField("1. 设置默认精灵", EditorStyles.label);
//                 EditorGUILayout.LabelField("   • 选择一个代表性的精灵作为默认精灵", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • 这将作为新规则的起始精灵", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.Space(5);
                
//                 EditorGUILayout.LabelField("2. 创建瓦片规则", EditorStyles.label);
//                 EditorGUILayout.LabelField("   • 使用Rule Tile Editor定义邻居匹配规则", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • 绿色箭头表示必须有相同瓦片", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • 红色X表示不能有相同瓦片", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • 空白表示忽略该位置", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.Space(5);
                
//                 EditorGUILayout.LabelField("3. 输出类型说明", EditorStyles.label);
//                 EditorGUILayout.LabelField("   • Single: 固定显示一个精灵", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • Random: 随机选择一个精灵", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • Animation: 播放精灵动画序列", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.Space(5);
                
//                 EditorGUILayout.LabelField("4. 变换规则", EditorStyles.label);
//                 EditorGUILayout.LabelField("   • Fixed: 固定方向匹配", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • Rotated: 支持旋转匹配", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • Mirror: 支持镜像匹配", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.Space(5);
                
//                 EditorGUILayout.LabelField("5. 使用建议", EditorStyles.label);
//                 EditorGUILayout.LabelField("   • 从简单规则开始，逐步添加复杂规则", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • 规则顺序很重要，优先级从上到下", EditorStyles.wordWrappedLabel);
//                 EditorGUILayout.LabelField("   • 使用预览功能测试规则效果", EditorStyles.wordWrappedLabel);
                
//                 EditorGUILayout.EndVertical();
//             }
            
//             if (GUI.changed)
//             {
//                 EditorUtility.SetDirty(ruleTile);
//             }
//         }
        
//         private string GetOutputTypeDisplayName(RuleTile.TilingRuleOutput.OutputSprite outputType)
//         {
//             switch (outputType)
//             {
//                 case RuleTile.TilingRuleOutput.OutputSprite.Single:
//                     return "单一精灵";
//                 case RuleTile.TilingRuleOutput.OutputSprite.Random:
//                     return "随机精灵";
//                 case RuleTile.TilingRuleOutput.OutputSprite.Animation:
//                     return "动画序列";
//                 default:
//                     return outputType.ToString();
//             }
//         }
        
//         private string GetTransformDisplayName(RuleTile.TilingRuleOutput.Transform transform)
//         {
//             switch (transform)
//             {
//                 case RuleTile.TilingRuleOutput.Transform.Fixed:
//                     return "固定";
//                 case RuleTile.TilingRuleOutput.Transform.Rotated:
//                     return "旋转";
//                 case RuleTile.TilingRuleOutput.Transform.MirrorX:
//                     return "X轴镜像";
//                 case RuleTile.TilingRuleOutput.Transform.MirrorY:
//                     return "Y轴镜像";
//                 case RuleTile.TilingRuleOutput.Transform.MirrorXY:
//                     return "XY轴镜像";
//                 case RuleTile.TilingRuleOutput.Transform.RotatedMirror:
//                     return "旋转镜像";
//                 default:
//                     return transform.ToString();
//             }
//         }
//     }
// }
// #endif