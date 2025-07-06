# UI 配置系统说明文档

## 概述

本文档描述了对主菜单场景及相关管理类中硬编码参数的配置化改造。通过这次改造，我们将原本硬编码在脚本中的参数移至配置系统，使其更易于配置和维护。

## 改造内容

### 1. 配置数据结构

在 `ConfigManager.cs` 中添加了 `UIConfigData` 类，用于存储 UI 相关的配置参数：

```csharp
public class UIConfigData
{
    public string uiName;
    public float fadeInTime = 1.0f;
    public float buttonHoverScale = 1.1f;
    public float buttonClickScale = 0.95f;
    public bool playBackgroundMusic = true;
    public bool playButtonSounds = true;
    public string nextScene;
    public string backgroundMusicKey;
    public Dictionary<string, string> dialogTexts;
    public Dictionary<string, string> buttonSoundMappings;
}
```

### 2. 配置管理

- 在 `ConfigManager.cs` 中添加了 `GetUIConfig` 和 `UpdateUIConfig` 方法，用于获取和更新 UI 配置。
- 在 `ConfigManager.cs` 的 `InitializeDefaultConfigs` 方法中添加了默认的 UI 配置。

### 3. MainMenuUI 改造

- 将 `MainMenuUI.cs` 中的硬编码参数替换为从配置系统中获取的值。
- 添加了 `LoadUIConfig` 方法，用于从 `ConfigManager` 加载配置。
- 修改了 `PlayBackgroundMusic`、`PlayButtonSound`、`LoadScene` 等方法，使其使用配置中的值。

### 4. 配置工具

#### 4.1 运行时配置

- `UIConfigLoader.cs`: 从 `GameConfig` 加载 UI 配置到 `ConfigManager`。
- `UIConfigSaveLoad.cs`: 将 UI 配置保存到 JSON 文件和从 JSON 文件加载配置。

#### 4.2 编辑器工具

- `UIConfigEditor.cs`: 在 Unity 编辑器中编辑 UI 配置。
- `UIConfigExporter.cs`: 将 Unity 编辑器中的配置导出为 JSON 文件。
- `UIConfigImporter.cs`: 从 JSON 文件导入 UI 配置到 Unity 编辑器。

## 使用方法

### 1. 在代码中使用配置

```csharp
// 获取UI配置
UIConfigData uiConfig = ConfigManager.Instance.GetUIConfig("MainMenuUI");

// 使用配置值
if (uiConfig != null)
{
    float fadeInTime = uiConfig.fadeInTime;
    float buttonHoverScale = uiConfig.buttonHoverScale;
    // ...
}
```

### 2. 编辑配置

#### 2.1 使用编辑器工具

1. 打开 Unity 编辑器
2. 点击菜单 `Game > UI Config Editor`
3. 在弹出的窗口中编辑配置
4. 点击 `保存配置` 按钮

#### 2.2 导出/导入配置

1. 点击菜单 `Game > UI Config Exporter` 或 `Game > UI Config Importer`
2. 按照界面提示操作

### 3. 运行时配置

1. 将 `UIConfigLoader` 组件添加到场景中的 GameObject 上
2. 将 `UIConfigSaveLoad` 组件添加到场景中的 GameObject 上
3. 配置组件参数

## 配置项说明

### MainMenuUI 配置

| 配置项 | 类型 | 说明 |
| --- | --- | --- |
| fadeInTime | float | 菜单淡入时间 |
| buttonHoverScale | float | 按钮悬停缩放比例 |
| buttonClickScale | float | 按钮点击缩放比例 |
| playBackgroundMusic | bool | 是否播放背景音乐 |
| playButtonSounds | bool | 是否播放按钮音效 |
| nextScene | string | 下一个场景名称 |
| backgroundMusicKey | string | 背景音乐键名 |
| dialogTexts | Dictionary | 对话框文本 |
| buttonSoundMappings | Dictionary | 按钮音效映射 |

### CharacterSelectUI 配置

| 配置项 | 类型 | 说明 |
| --- | --- | --- |
| fadeInTime | float | 菜单淡入时间 |
| buttonHoverScale | float | 按钮悬停缩放比例 |
| buttonClickScale | float | 按钮点击缩放比例 |
| playBackgroundMusic | bool | 是否播放背景音乐 |
| playButtonSounds | bool | 是否播放按钮音效 |
| nextScene | string | 下一个场景名称 |
| backgroundMusicKey | string | 背景音乐键名 |
| dialogTexts | Dictionary | 对话框文本 |
| buttonSoundMappings | Dictionary | 按钮音效映射 |

## 注意事项

1. 确保 `ConfigManager` 在场景中已初始化
2. 在使用配置前，先检查配置是否为 null
3. 如果配置中的某个值不存在，使用默认值