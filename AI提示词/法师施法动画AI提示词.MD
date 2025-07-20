# 法师施法动画AI提示词

## 角色描述
基于提供的法师精灵图，角色特征如下：
- **发色**: 紫色长发，飘逸动感
- **服装**: 深蓝色法师袍，带有青色装饰边缘
- **法杖**: 顶端有蓝色水晶/宝石的长杖
- **整体风格**: 2D像素艺术风格，优雅神秘

## 核心AI提示词

### 主要提示词
```
Create a 2D pixel art mage casting animation for ranged attack, featuring a female mage with long flowing purple hair, wearing a dark blue robe with cyan trim. She holds a staff with a blue crystal at the top. The staff should be positioned at chest level or lower, never raised above shoulder height. Animation should show magical energy gathering and projectile casting motion.

创建一个2D像素艺术法师施法动画用于远程攻击，特征为紫色长发飘逸的女法师，穿着带青色装饰边缘的深蓝色法袍。她手持顶端有蓝色水晶的法杖。法杖位置应保持在胸部水平或更低，绝不能举过肩膀高度。动画应展示魔法能量聚集和投射法术的动作。
```

### 详细动画序列提示词

#### 第1帧 - 准备姿势 (Preparation Pose)
```
Frame 1: Mage in idle stance, staff held diagonally at chest level, crystal glowing softly, purple hair flowing naturally, calm expression, body slightly turned toward target direction.

第1帧：法师处于待机姿势，法杖斜持在胸部水平，水晶轻微发光，紫发自然飘动，表情平静，身体略微转向目标方向。
```

#### 第2帧 - 能量聚集 (Energy Gathering)
```
Frame 2: Mage begins channeling, staff crystal brightens with blue magical energy, small energy particles swirling around the crystal, hair starts to move with magical wind, focused expression.

第2帧：法师开始施法，法杖水晶发出明亮的蓝色魔法能量，小型能量粒子围绕水晶旋转，头发开始随魔法之风飘动，表情专注。
```

#### 第3帧 - 魔力汇聚 (Power Concentration)
```
Frame 3: Maximum energy buildup, crystal blazing with intense blue light, energy aura expanding around the staff tip, hair flowing dramatically, eyes glowing with magical power, staff held steady at chest level.

第3帧：能量达到最大聚集，水晶发出强烈蓝光，能量光环在法杖顶端扩散，头发戏剧性飘动，眼中闪烁魔法光芒，法杖稳定保持在胸部水平。
```

#### 第4帧 - 施法动作 (Casting Motion)
```
Frame 4: Mage extends staff forward (still at chest level), magical projectile forming at crystal tip, body leaning slightly forward, hair swept back by magical force, determined expression.

第4帧：法师将法杖向前伸出（仍保持胸部水平），魔法弹在水晶顶端形成，身体略微前倾，头发被魔法力量向后吹动，表情坚定。
```

#### 第5帧 - 法术释放 (Spell Release)
```
Frame 5: Projectile launches from staff, bright flash of blue energy, staff recoils slightly, hair and robes flowing from magical discharge, satisfied expression as spell is cast.

第5帧：法术弹从法杖发射，蓝色能量闪光，法杖轻微后坐，头发和法袍因魔法释放而飘动，施法完成后的满意表情。
```

#### 第6帧 - 恢复姿势 (Recovery Pose)
```
Frame 6: Return to neutral stance, staff lowered back to diagonal position, crystal dimming, hair settling down, relaxed expression, ready for next action.

第6帧：回到中性姿势，法杖降回斜持位置，水晶光芒减弱，头发逐渐平静，表情放松，准备下一个动作。
```

## 技术规格要求

### 动画参数
- **帧数**: 6帧循环动画
- **帧率**: 8-12 FPS
- **分辨率**: 64x64 或 128x128 像素
- **颜色**: 16-32色调色板
- **风格**: 2D像素艺术

### 关键约束
- **法杖高度**: 始终保持在肩膀以下
- **动作幅度**: 适中，不过于夸张
- **能量效果**: 蓝色系魔法特效
- **一致性**: 保持角色设计一致

## 特效细节

### 魔法能量效果
```
Magical energy effects: Blue crystal glow, swirling energy particles, magical aura, energy discharge flash, subtle screen shake on projectile launch.

魔法能量效果：蓝色水晶发光，旋转能量粒子，魔法光环，能量释放闪光，法术弹发射时的轻微屏幕震动。
```

### 环境交互
```
Environmental interaction: Hair and robe movement from magical wind, subtle lighting changes during energy buildup, particle effects dissipating after cast.

环境交互：魔法之风带动头发和法袍飘动，能量聚集时的微妙光照变化，施法后粒子效果消散。
```

## 音效建议

### 音效序列
1. **能量聚集**: 低频嗡嗡声，逐渐增强
2. **魔力汇聚**: 高频能量充电音
3. **法术释放**: 清脆的魔法发射音
4. **环境音**: 轻微的魔法回响

## 变体建议

### 不同法术类型
- **火球术**: 红橙色能量，火焰粒子效果
- **冰锥术**: 白蓝色能量，冰晶粒子效果
- **闪电术**: 紫白色能量，电弧效果
- **治疗术**: 绿色或金色能量，温和光芒

### 强度变化
- **基础法术**: 简单能量效果
- **中级法术**: 增强粒子和光芒
- **高级法术**: 复杂特效和更长蓄力

## 实现注意事项

### Unity动画设置
- 使用Animator Controller管理状态
- 设置适当的过渡条件
- 配置循环和触发参数
- 优化性能和内存使用

### 代码集成
- 与SkillProjectile系统配合
- 同步动画和投射物生成
- 处理动画中断和重置
- 实现连击和组合技能

---

**创建时间**: 2024年  
**适用项目**: Unity 2D RPG游戏  
**兼容性**: 与现有技能系统完全兼容