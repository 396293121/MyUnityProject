# 技能系统重构说明

## 概述

本次重构将技能逻辑从职业角色脚本中分离出来，创建了独立的技能组件系统。新系统基于 `SkillManager.cs` 和 `skillDataConfig.cs` 的验证实现，支持新输入系统，并提供更好的模块化和可扩展性。

## 新增组件

### 1. SkillComponent.cs
- **功能**: 技能执行和管理的核心组件
- **特性**: 
  - 支持新输入系统
  - 技能冷却管理
  - 动画事件集成
  - 调试可视化
- **使用**: 添加到角色GameObject上，替代原有的技能逻辑

### 2. BuffManager.cs
- **功能**: 角色增益/减益效果管理
- **特性**:
  - 支持多种BUFF类型（攻击力、速度、治疗等）
  - 自动过期处理
  - 效果叠加控制
- **使用**: 与SkillComponent配合使用，处理技能产生的BUFF效果

### 3. CharacterSkillConfig.cs
- **功能**: 技能相关配置的ScriptableObject
- **特性**:
  - 从职业配置中分离技能设置
  - 支持输入映射配置
  - 全局技能倍率设置
- **使用**: 创建配置资源，分配给不同角色类型

### 4. WarriorSkillData.cs
- **功能**: 战士技能的预设配置
- **特性**:
  - 继承自skillDataConfig
  - 提供战士技能的快速设置方法
  - 配置验证功能
- **使用**: 创建战士技能数据资源

## 系统集成步骤

### 1. 角色设置
```csharp
// 在角色GameObject上添加组件
- SkillComponent
- BuffManager
- 保留原有的Character组件（如Warrior）
- 保留PlayerController组件
```

### 2. 配置创建
```csharp
// 创建技能配置
1. 右键 -> Create -> Skills -> Character Skill Config
2. 右键 -> Create -> Skills -> Warrior Skill Data (为每个技能创建)
3. 配置技能数据和输入映射
```

### 3. 组件配置
```csharp
// SkillComponent设置
- 分配技能数据列表
- 设置输入动作引用
- 配置技能释放点
- 设置角色引用

// BuffManager设置
- 设置角色控制器引用
- 配置调试选项
```

## 输入系统集成

### 新输入系统配置
```csharp
// 在Input Actions中创建技能动作
- Skill1 (键盘: Q)
- Skill2 (键盘: W) 
- Skill3 (键盘: E)
- Skill4 (键盘: R)

// 在CharacterSkillConfig中映射
public SkillInputMapping[] inputMappings = {
    new SkillInputMapping { skillIndex = 0, inputActionName = "Skill1" },
    new SkillInputMapping { skillIndex = 1, inputActionName = "Skill2" },
    new SkillInputMapping { skillIndex = 2, inputActionName = "Skill3" }
};
```

## 迁移指南

### 从旧系统迁移

1. **移除旧技能逻辑**
   - 从Warrior.cs等职业脚本中移除技能方法
   - 从PlayerController.cs中移除UseSkill相关方法
   - 从职业配置中移除技能参数

2. **添加新组件**
   - 在角色上添加SkillComponent和BuffManager
   - 创建技能配置资源
   - 设置组件引用

3. **配置技能数据**
   - 使用WarriorSkillData创建技能
   - 配置技能参数（伤害、冷却、范围等）
   - 设置动画触发器

4. **测试验证**
   - 使用SkillSystemExample进行测试
   - 验证技能执行和BUFF效果
   - 检查输入响应

## 技能创建示例

### 创建重斩技能
```csharp
// 在WarriorSkillData中
public void SetupHeavySlash()
{
    skillName = "重斩";
    skillType = SkillTypeTest.SingleTargetBox;
    damage = 0; // 通过倍率计算
    damageMultiplier = 1.5f;
    cooldownTime = 5f;
    manaCost = 20;
    boxWidth = 3f;
    boxHeight = 2.5f;
    animationTrigger = "HeavySlash";
}
```

### 创建BUFF技能
```csharp
// 战吼技能配置
public void SetupBattleCry()
{
    skillName = "战吼";
    skillType = SkillTypeTest.Buff;
    targetType = TargetType.Self;
    buffDuration = 10f;
    attackBonus = 0.5f; // 50%攻击力提升
    cooldownTime = 15f;
}
```

## 调试功能

### 可视化调试
- SkillComponent提供Gizmos绘制
- 显示技能范围（扇形、矩形、圆形）
- 实时技能状态指示

### 编辑器工具
- SkillSystemExample提供运行时测试
- 技能信息显示
- BUFF状态监控
- 一键组件设置

## 扩展性

### 添加新技能类型
1. 在SkillTypeTest枚举中添加新类型
2. 在skillDataConfig.ExecuteSkillEffect中添加处理逻辑
3. 在SkillComponent中添加对应的执行方法

### 添加新BUFF类型
1. 在BuffManager.BuffType中添加新类型
2. 在ApplyBuffToCharacter和RemoveBuffFromCharacter中添加处理
3. 更新BUFF效果计算逻辑

## 注意事项

1. **动画事件**: 确保动画中正确设置技能伤害帧事件
2. **输入映射**: 新输入系统需要正确配置Action Maps
3. **组件依赖**: SkillComponent需要Character和PlayerController引用
4. **性能考虑**: 大量技能时注意Update中的性能开销
5. **调试模式**: 生产环境中关闭调试Gizmos绘制

## 常见问题

### Q: 技能不响应输入？
A: 检查输入系统配置和CharacterSkillConfig中的映射设置

### Q: 技能伤害不生效？
A: 确认动画事件正确调用OnSkillDamageFrame方法

### Q: BUFF效果不显示？
A: 检查BuffManager的角色引用和BUFF类型配置

### Q: 技能范围显示不正确？
A: 验证skillDataConfig中的范围参数设置

---

通过这个新的技能系统，您可以更灵活地管理角色技能，支持数据驱动的技能配置，并且易于扩展和维护。