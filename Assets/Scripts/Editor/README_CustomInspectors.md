# Unity组件自定义Inspector使用说明

## 概述

本项目为Unity常用组件创建了自定义Inspector，使用Odin Inspector框架为组件属性添加了中文标签、详细说明和更友好的界面布局。

## 已支持的组件

### 基础组件 (UnityComponentInspectors.cs)

1. **Transform (变换组件)**
   - 位置、旋转、缩放的中文标签
   - 世界坐标和本地坐标的区分显示
   - 层级信息显示
   - 详细的属性说明

2. **Rigidbody2D (2D刚体组件)**
   - 物理属性的中文说明
   - 运动约束设置
   - 运行时速度信息显示
   - 物体类型的详细解释

3. **BoxCollider2D (2D盒子碰撞器)**
   - 碰撞器属性的中文标签
   - 触发器和物理碰撞的区别说明
   - 世界坐标信息显示
   - 形状设置的可视化

4. **Animator (动画控制器)**
   - 动画控制器设置的中文说明
   - 播放模式和剔除模式的详细解释
   - 运行时状态信息显示
   - 动画参数的实时监控

### 扩展组件 (UnityComponentInspectorsExtended.cs)

1. **SpriteRenderer (精灵渲染器)**
   - 精灵设置和颜色控制
   - 翻转选项的中文说明
   - 渲染层级和排序设置
   - 精灵信息的详细显示

2. **AudioSource (音频源)**
   - 音频播放控制的中文界面
   - 音量、音调、立体声设置
   - 3D音效参数配置
   - 运行时播放控制按钮
   - 音频剪辑信息显示

3. **Camera (摄像机)**
   - 渲染设置的详细说明
   - 投影模式的中文标签
   - 视口和深度设置
   - 摄像机信息的实时显示

## 使用方法

### 1. 自动应用

将脚本放置在 `Assets/Scripts/Editor/` 文件夹中后，Unity会自动应用这些自定义Inspector。当你在Inspector面板中查看相应组件时，会看到中文化的界面。

### 2. 功能特点

- **中文标签**: 所有属性都有清晰的中文标签
- **详细说明**: 每个属性都有帮助信息框，解释其作用
- **分组显示**: 相关属性被逻辑分组，便于查找
- **运行时信息**: 在Play模式下显示实时状态信息
- **交互控制**: 部分组件提供运行时控制按钮

### 3. 注意事项

- 这些自定义Inspector只在Unity编辑器中生效
- 不会影响组件的运行时行为
- 需要安装Odin Inspector插件
- 如果不需要某个组件的自定义Inspector，可以删除对应的类

## 扩展指南

### 添加新组件的自定义Inspector

1. **创建新的Inspector类**:
```csharp
[CustomEditor(typeof(YourComponent))]
public class YourComponentInspector : OdinEditor
{
    public override void OnInspectorGUI()
    {
        YourComponent component = (YourComponent)target;
        
        // 添加标题
        SirenixEditorGUI.Title("组件中文名", "组件描述", TextAlignment.Left, true);
        
        // 添加属性分组
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("分组名称", EditorStyles.boldLabel);
        
        // 添加帮助信息
        EditorGUILayout.HelpBox("属性说明", MessageType.Info);
        
        // 添加属性控制
        component.yourProperty = EditorGUILayout.FloatField("属性中文名", component.yourProperty);
        
        EditorGUILayout.EndVertical();
        
        // 标记为已修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(component);
        }
    }
}
```

2. **使用Odin Inspector属性**:
- `SirenixEditorGUI.Title()`: 创建标题
- `EditorGUILayout.HelpBox()`: 添加帮助信息
- `EditorGUILayout.BeginVertical("box")`: 创建分组框
- `EditorStyles.boldLabel`: 粗体标签样式

### 自定义样式

可以通过修改以下元素来自定义界面样式:
- 颜色主题
- 分组框样式
- 字体大小
- 布局间距

### 添加运行时功能

在 `if (Application.isPlaying)` 块中添加运行时特有的功能:
- 实时数据显示
- 控制按钮
- 状态监控
- 调试信息

## 常见问题

### Q: 为什么自定义Inspector没有生效？
A: 确保:
1. 脚本放在Editor文件夹中
2. 已安装Odin Inspector插件
3. 没有编译错误
4. Unity编辑器已重新编译

### Q: 如何禁用某个组件的自定义Inspector？
A: 删除或注释掉对应的 `[CustomEditor]` 类即可。

### Q: 可以同时使用多个自定义Inspector吗？
A: 不可以，Unity只会使用最后一个注册的CustomEditor。如果需要组合功能，请在一个Inspector中实现。

### Q: 如何添加更多的中文说明？
A: 修改 `EditorGUILayout.HelpBox()` 中的文本内容，或添加更多的帮助信息框。

## 更新日志

- **v1.0**: 初始版本，支持Transform、Rigidbody2D、BoxCollider2D、Animator
- **v1.1**: 添加SpriteRenderer、AudioSource、Camera支持
- **v1.2**: 增强运行时信息显示和交互控制

## 贡献

欢迎为项目添加更多组件的自定义Inspector或改进现有功能。请确保:
1. 遵循现有的代码风格
2. 添加详细的中文注释
3. 测试功能的正确性
4. 更新此文档