# 战士受伤死亡动画AI提示词

## 基础设定参考

### 战士角色基础信息
- **来源**：A区域原始部落的强壮战士
- **外观特征**：身材魁梧，肌肉发达，穿着部分原始部落风格护甲，混合金属元素
- **武器装备**：持有一把大剑
- **觉醒力量**：护甲或武器上有微弱蓝紫色光芒
- **表情特点**：坚毅自信

### 技术规格要求
- **画布尺寸**：213x144像素（与现有精灵图保持一致）
- **锚点设置**：底部中心对齐 (0.5, 1.0)
- **物理体配置**：宽41px，高94px，X偏移0，Y偏移50px
- **动画风格**：类银河战士恶魔城2D动作冒险游戏风格
- **帧数要求**：受伤动画6-8帧，死亡动画8-12帧

## 受伤动画AI提示词

### 核心提示词
```
类银河战士恶魔城2D动作冒险游戏风格，像素艺术风格，强壮的部落战士受伤动画序列，侧面视角，透明背景。

角色描述：身材魁梧肌肉发达的战士，穿着原始部落风格护甲混合金属元素，手持大剑，护甲上有微弱蓝紫色光芒。

动画要求：
- 6-8帧受伤反应动画
- 战士向后轻微后仰，表现受到冲击
- 面部表情从坚毅转为痛苦但仍坚强
- 身体略微弯曲，一只手可能护住受伤部位
- 大剑依然紧握，显示战士意志坚定
- 护甲上的蓝紫色光芒可能闪烁或暗淡
- 动作幅度适中，不要过于夸张

色彩方案：红褐色和金属色为主，点缀蓝紫色光芒，受伤时可添加少量红色效果
画布尺寸：213x144像素，角色居中偏左，底部对齐
高对比度，清晰边缘，适合2D游戏使用
```

### 关键注意事项
1. **动作连贯性**：确保受伤动画能够自然过渡回站立状态
2. **角色一致性**：保持与现有战士精灵图的外观完全一致
3. **伤害表现**：通过姿态和表情表现受伤，避免过于血腥的视觉效果
4. **装备保持**：大剑和护甲应保持完整，体现战士的坚韧
5. **光效变化**：蓝紫色觉醒力量光芒可以有微妙变化，暗示力量波动

## 死亡动画AI提示词

### 核心提示词
```
类银河战士恶魔城2D动作冒险游戏风格，像素艺术风格，强壮的部落战士死亡动画序列，侧面视角，透明背景。

角色描述：身材魁梧肌肉发达的战士，穿着原始部落风格护甲混合金属元素，手持大剑，护甲上有微弱蓝紫色光芒。

动画要求：
- 8-12帧死亡动画序列
- 第1-3帧：战士身体开始失去平衡，向后倾倒
- 第4-6帧：战士跪倒在地，一手撑地，仍握着大剑
- 第7-9帧：战士缓慢倒下，大剑落地
- 第10-12帧：战士完全倒地，保持英勇姿态
- 面部表情：从痛苦逐渐转为平静，体现战士的尊严
- 护甲上的蓝紫色光芒逐渐消散
- 动作要有重量感，体现战士的魁梧体格
- 最终姿态要庄严，不失战士风范

色彩方案：红褐色和金属色为主，蓝紫色光芒逐渐消散，整体色调可略微暗淡
画布尺寸：213x144像素，确保倒地动作完整显示在画布内
高对比度，清晰边缘，适合2D游戏使用
```

### 关键注意事项
1. **尊严保持**：死亡动画应体现战士的英勇和尊严，避免过于凄惨
2. **重量感**：动作要体现战士魁梧身材的重量感，倒下要有力度
3. **装备处理**：大剑的掉落要自然，护甲保持完整
4. **光效消散**：蓝紫色觉醒力量光芒的消散要渐进，暗示生命力流失
5. **最终帧**：最后一帧要稳定，适合作为死亡状态的静止画面

## 生成流程建议

### 1. 基础图片生成
- **工具**：Google Image FX (https://labs.google/fx/zh/tools/image-fx)
- **策略**：分别生成受伤和死亡的关键帧静态图片
- **数量**：每个动画生成5-8个候选版本

### 2. 动画视频生成
- **工具**：即梦AI (https://jimeng.jianying.com/ai-tool/generate?type=video)
- **输入**：使用生成的静态图片作为首帧
- **设置**：确保动画流畅，帧数符合要求

### 3. 精灵图处理
- **工具**：素材管理器
- **处理**：视频转精灵图，调整至213x144像素
- **优化**：确保每帧清晰，背景透明

### 4. 集成配置
- **文件命名**：
  - 受伤动画：`战士受伤_frame_000001.png` 到 `战士受伤_frame_000008.png`
  - 死亡动画：`战士死亡_frame_000001.png` 到 `战士死亡_frame_000012.png`
- **配置更新**：在AnimationConfig.js中更新hurt和die动画配置

## 质量检查清单

### 视觉一致性
- [ ] 角色外观与现有精灵图完全一致
- [ ] 护甲、武器、体型比例正确
- [ ] 色彩方案符合战士角色设定
- [ ] 蓝紫色觉醒力量光芒处理得当

### 技术规格
- [ ] 画布尺寸为213x144像素
- [ ] 背景完全透明
- [ ] 角色底部对齐画布底部
- [ ] 帧数符合性能要求

### 动画质量
- [ ] 动作流畅自然
- [ ] 符合物理规律
- [ ] 表情变化合理
- [ ] 装备处理恰当

### 游戏集成
- [ ] 文件命名规范正确
- [ ] 与现有动画系统兼容
- [ ] 物理体配置无需调整
- [ ] 性能影响可接受

## 扩展建议

1. **受伤等级**：可考虑生成轻伤、重伤两种不同程度的受伤动画
2. **死亡类型**：可根据不同伤害类型生成对应的死亡动画
3. **复活动画**：为后续可能的复活机制预留动画接口
4. **特效增强**：可在代码中添加粒子特效配合动画
5. **音效配合**：为动画配置相应的音效文件


**重斩动画（12帧）：**
- 帧1-3：蓄力阶段，角色后仰，剑举过头顶
- 帧4-6：发力阶段，身体前倾，剑开始下劈
- 帧7-9：命中阶段，剑完全下劈，产生蓝色剑气特效
- 帧10-12：收招阶段，角色恢复站立姿态
- 特效：蓝色能量剑气，长度为剑身1.5倍

**旋风斩动画（12帧）：**
- 帧1-2：准备阶段，双手握剑举起，身体微微下蹲
- 帧3-4：启动阶段，开始旋转，剑刃开始划出弧线
- 帧5-6：加速阶段，旋转速度加快，剑光形成圆弧
- 帧7-8：高潮阶段，全速旋转，剑光形成完整圆环，产生风压特效
- 帧9-10：减速阶段，旋转逐渐放缓，剑光收敛
- 帧11-12：结束阶段，回到战斗姿态，剑指向前方
- 特效：青色旋风特效环绕角色，剑光轨迹呈螺旋状，风压波纹向外扩散
- 视觉要点：角色保持平衡，剑刃在旋转中产生残影效果
- 技术规格：12帧循环动画，适合群体攻击技能表现

**战吼动画（10帧）：**
- 帧1-3：深呼吸阶段，胸部扩张
- 帧4-6：爆发阶段，头部后仰，嘴部张开
- 帧7-8：高潮阶段，全身发出红色光芒
- 帧9-10：结束阶段，恢复正常姿态
- 特效：红色能量光环，从角色中心向外扩散
