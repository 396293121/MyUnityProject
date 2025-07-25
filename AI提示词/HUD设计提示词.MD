# HUD设计AI提示词

## 生命值条 (health_bar.png)
**描述**：带有金属质感的横向生命值条，位于屏幕左上角。红色能量液体在金属凹槽内流动，边框有古代符文雕刻。当生命值低于30%时，边框出现裂纹并发出红色脉冲光效。

**风格**：类银河战士恶魔城2D动作冒险游戏风格，金属与魔法结合  
**尺寸**：300x40像素  
**色彩方案**：  
**动态效果**：数值变化时液体有轻微晃动，低生命时边框裂纹发光  

## 魔力值条 (mana_bar.png)
**描述**：蓝色的魔力条
**尺寸**300x40像素  
**色彩方案**：  
**动态效果**：充能时结晶旋转加速，消耗时液体表面产生涟漪  

## 经验值条 (exp_bar.png)
**描述**：细长的青铜色经验条位于屏幕底部中央，填充金色粒子流。每10%进度处有罗马数字刻度，满经验时整体发出脉冲金光。

**尺寸**：600x12像素  
**特效**：升级时数字刻度依次点亮，粒子流向角色位置汇聚  

## 技能快捷栏 (skill_hud.png)
**描述**：横向排列的6个独立技能槽，位于屏幕底部中央。每个技能槽为菱形金属框，中心镶嵌对应职业符文（战士/法师/射手）。激活的技能槽散发蓝紫色觉醒光芒，相邻槽位有能量流动连接。

**单个规格**：
- 尺寸：56x56像素（含2像素间距）
- 常态：暗银金属边框，符文微光
- 交互状态：
  - 就绪：符文剧烈 pulsate 发光，边框浮现觉醒符文
  - 冷却：覆盖半透明齿轮锁，中心显示倒计时数字
  - 禁用：整体变灰，符文出现裂痕

**排列特性**：
1. 槽位间隔有微弱能量流动
2. 当前选中槽位放大110%并轻微浮动
3. 快捷键数字悬浮在槽位左上角（字体带金属投影）

**动态反馈**：
- 鼠标悬停：槽位升高5像素，投射光环
- 技能释放：觉醒符文从槽位迸发并沿技能轨迹消散
- 连招就绪：相邻槽位产生能量共鸣脉冲波
## 状态图标集 (status_icons.png)
**描述**：统一风格的圆形状态图标，包含：  
1. 中毒（紫色骷髅）  
2. 燃烧（橙色火焰）  
3. 冰冻（蓝色冰晶）  
4. 眩晕（黄色星星）  
5. BUFF（绿色上升箭头）

**统一规格**：  
- 尺寸：32x32像素  
- 边框：极细银边  
- 背景：半透明黑圆  
- 动态：持续状态图标轻微浮动  

## 小地图 (minimap.png)
**描述**：圆形青铜罗盘式小地图，边缘有方向刻度。当前区域以发光蓝线勾勒，重要地点用不同符号标记（商店/存档点/BOSS）。

**尺寸**：150x150像素  
**交互**：发现新区域时罗盘指针转动，危险区域显示红色渐晕  

## 通用设计规范
1. 所有HUD元素使用相同金属质感  
2. 数值文本使用"Ancient Runes"字体  
3. 动态光效与主菜单的蓝紫色觉醒力量一致  
4. 重要状态变化伴随0.1秒画面震动  
5. 适配16:9和4:3屏幕比例

## 文件格式要求
- 图片格式：PNG (32位带Alpha通道)  
- 命名规范：[元素名称]_[状态]_[分辨率].png  
- 分层文件：提供各特效单独图层
