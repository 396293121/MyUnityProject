# CharacterDialog 渲染层级修复指南

## 问题分析

### 根本原因
1. **坐标系冲突**: Sprite Renderer 使用世界坐标系，而 UI Canvas 使用屏幕坐标系
2. **渲染层级问题**: Sprite Renderer 默认渲染层级可能被 UI Canvas 遮挡
3. **缩放问题**: 世界坐标系的缩放与 UI 坐标系不匹配

### 当前修复内容

#### 1. 渲染层级设置
```csharp
characterSpriteRenderer.sortingLayerName = "UI";
characterSpriteRenderer.sortingOrder = 100;
```

#### 2. UI 坐标系适配
- 自动添加 `RectTransform` 组件
- 设置锚点为中心点 (0.5, 0.5)
- 设置 UI 尺寸为 200x400

## Unity 编辑器配置步骤

### 步骤 1: 检查 Sorting Layers
1. 打开 **Edit → Project Settings → Tags and Layers**
2. 展开 **Sorting Layers** 部分
3. 确保存在名为 "UI" 的 Sorting Layer
4. 如果不存在，点击 "+" 添加 "UI" 层级
5. 将 "UI" 层级拖拽到列表顶部（最高优先级）

### 步骤 2: 配置 CharacterPreview GameObject
1. 在 Hierarchy 中找到 CharacterPreview 对象
2. 确保它是 Canvas 的子对象
3. 检查组件配置：
   - ✅ **Transform** 或 **RectTransform**
   - ✅ **Sprite Renderer**
   - ✅ **Animator**

### 步骤 3: Sprite Renderer 设置
在 Inspector 中配置 Sprite Renderer：
```
Sprite: (角色精灵图)
Color: White (255, 255, 255, 255)
Flip X: false
Flip Y: false
Sorting Layer: UI
Order in Layer: 100
```

### 步骤 4: Canvas 设置检查
确保 Canvas 组件设置：
```
Render Mode: Screen Space - Overlay
Pixel Perfect: true
Sort Order: 0
```

## 测试验证

### 1. Scene 视图测试
- 在 Scene 视图中应该能看到角色动画
- 角色应该显示在 UI 元素之上

### 2. Game 视图测试
- 切换到 Game 视图
- 角色动画应该正常显示
- 尺寸应该合适（不需要设置 500 的缩放）

### 3. 运行时测试
- 播放游戏
- 打开角色对话框
- 验证战士角色播放动画
- 验证其他角色显示静态图

## 故障排除

### 问题 1: Game 视图中看不到角色
**解决方案**:
1. 检查 Sorting Layer 是否正确设置为 "UI"
2. 确认 Order in Layer 足够高（建议 100+）
3. 验证 Canvas 的 Render Mode 为 Screen Space - Overlay

### 问题 2: 角色尺寸仍然很小
**解决方案**:
1. 调整 `characterPreviewScale` 参数（建议 1-5 之间）
2. 检查 RectTransform 的 sizeDelta 设置
3. 确认 Canvas Scaler 设置正确

### 问题 3: 角色位置不正确
**解决方案**:
1. 确认 RectTransform 锚点设置为 (0.5, 0.5)
2. 检查 anchoredPosition 是否为 (0, 0)
3. 验证父对象的 RectTransform 设置

## 最佳实践建议

### 1. 使用 UI Image 替代方案（推荐）
如果问题持续存在，建议改用 UI Image + Animator 方案：
```csharp
public Image characterPreview; // 改回 Image 类型
public Animator characterAnimator;
```

### 2. 渲染纹理方案
对于复杂动画，可以考虑使用 RenderTexture：
1. 创建专用 Camera 渲染角色
2. 将结果输出到 RenderTexture
3. 在 UI 中显示 RenderTexture

### 3. 性能优化
- 仅在需要时激活 Animator
- 使用对象池管理角色预览
- 预加载常用角色精灵图

## 调试日志

运行时查看 Console 输出，应该看到：
```
[CharacterDialog] 设置Sprite Renderer渲染层级: UI, Order: 100
[CharacterDialog] 设置角色预览缩放: (5, 5, 5), 位置: (0, 0)
[CharacterDialog] 播放战士行走动画
```

如果看不到这些日志，说明方法未被调用或组件引用有问题。