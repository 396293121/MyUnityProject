# EnemySystemConfig 配置指南

## 概述

`EnemySystemConfig` 是统一的敌人配置系统，用于管理所有敌人类型的属性和行为。本指南将说明如何配置该系统来在测试场景中生成三只野猪怪物。

## 配置步骤

### 1. 创建 EnemySystemConfig 资源

1. 在 Unity 编辑器中，右键点击 `Assets/Data/ScriptableObjects` 文件夹
2. 选择 `Create > Game > Enemy System Config`
3. 将新创建的配置文件命名为 `TestSceneEnemyConfig`

### 2. 配置野猪属性

在 Inspector 中打开 `TestSceneEnemyConfig`，配置以下参数：

#### 基础属性
- **生命值 (Health)**: 100
- **移动速度 (Move Speed)**: 2.5
- **攻击伤害 (Attack Damage)**: 20
- **攻击范围 (Attack Range)**: 1.5
- **检测范围 (Detection Range)**: 6.0

#### 冲锋属性
- **冲锋速度 (Charge Speed)**: 8.0
- **冲锋距离 (Charge Distance)**: 6.0
- **冲锋冷却时间 (Charge Cooldown)**: 3.0
- **冲锋持续时间 (Charge Duration)**: 2.0
- **冲锋伤害 (Charge Damage)**: 30

#### 巡逻属性
- **巡逻速度 (Patrol Speed)**: 1.0
- **巡逻等待时间 (Patrol Wait Time)**: 2.0
- **巡逻半径 (Patrol Radius)**: 4.0

### 3. 在测试场景中使用配置

#### 方法一：通过 TestSceneController

1. 在测试场景中找到 `TestSceneController` 对象
2. 在 Inspector 中找到 `Unified Config` 组件
3. 将创建的 `TestSceneEnemyConfig` 拖拽到 `Enemy System Config` 字段

#### 方法二：通过代码生成

```csharp
// 在 TestSceneController 或其他管理脚本中
public EnemySystemConfig enemyConfig;
public GameObject wildBoarPrefab;

void Start()
{
    // 创建敌人系统
    var enemySystem = new TestSceneEnemySystem(enemyConfig, eventBus);
    
    // 生成三只野猪
    for (int i = 0; i < 3; i++)
    {
        var spawnConfig = new EnemySpawnConfig
        {
            enemyType = "wild_boar",
            enemyPrefab = wildBoarPrefab,
            spawnPosition = new Vector3(i * 5f, 0, 0), // 间隔5米生成
            autoGeneratePatrolPoints = true,
            patrolRadius = 3f
        };
        
        enemySystem.SpawnEnemy(spawnConfig);
    }
}
```

### 4. 验证配置

运行测试场景后，你应该看到：

1. **三只野猪怪物**在指定位置生成
2. **巡逻行为**：野猪在巡逻半径内来回移动
3. **追击行为**：当玩家进入检测范围时，野猪开始追击
4. **冲锋攻击**：在合适距离时，野猪会发动冲锋攻击
5. **属性正确**：生命值、攻击力等符合配置

## 常见问题

### Q: 野猪不会移动？
**A**: 检查以下项目：
- 确保野猪 Prefab 有 `Rigidbody2D` 组件
- 确保 `Move Speed` 大于 0
- 检查碰撞层设置是否正确

### Q: 野猪不会攻击？
**A**: 检查以下项目：
- 确保玩家在检测范围内
- 确保 `Attack Range` 和 `Detection Range` 设置合理
- 检查玩家的 Layer 设置

### Q: 冲锋攻击不触发？
**A**: 检查以下项目：
- 确保 `Charge Distance` 设置合理（建议 4-8 米）
- 确保 `Charge Cooldown` 不会太长
- 检查野猪是否处于追击状态

## 高级配置

### 自定义掉落物品

在 `Global Drop Config` 中配置：

```
Default Drops:
- Item ID: "coin"
  Chance: 0.8
  Min Quantity: 1
  Max Quantity: 3

- Item ID: "health_potion"
  Chance: 0.3
  Min Quantity: 1
  Max Quantity: 1
```

### 动画配置

在 `Global Animation Config` 中调整：
- 攻击关键帧时机
- 受伤无敌时间
- 死亡动画持续时间

### 物理配置

在 `Global Physics Config` 中设置：
- 碰撞体大小和偏移
- 攻击框参数
- 击退效果

## 总结

通过 `EnemySystemConfig`，你可以轻松配置和管理所有敌人的属性。这个统一的配置系统避免了重复配置，提高了维护效率，并且支持运行时动态调整。

配置完成后，测试场景将自动生成三只具有完整 AI 行为的野猪怪物，包括巡逻、追击、冲锋攻击等功能。