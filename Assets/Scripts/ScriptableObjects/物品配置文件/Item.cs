using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// 物品类型枚举
/// </summary>
public enum ItemType
{
    Weapon,     // 武器
    Armor,      // 护甲
    Consumable, // 消耗品
    Material,   // 材料
    Quest,      // 任务物品
    Misc        // 杂项
}

/// <summary>
/// 物品稀有度枚举
/// </summary>
public enum ItemRarity
{
    Common,     // 普通（白色）
    Uncommon,   // 不常见（绿色）
    Rare,       // 稀有（蓝色）
    Epic,       // 史诗（紫色）
    Legendary   // 传说（橙色）
}

/// <summary>
/// 物品来源类型枚举
/// </summary>
public enum ItemSource
{
    [LabelText("商店购买")]
    Shop,
    [LabelText("敌人掉落")]
    EnemyDrop,
    [LabelText("宝箱获得")]
    Chest,
    [LabelText("任务奖励")]
    QuestReward,
    [LabelText("制作获得")]
    Crafting,
    [LabelText("采集获得")]
    Gathering,
    [LabelText("其他")]
    Other
}

/// <summary>
/// 物品基类 - 游戏中所有物品的基础类
/// 从原Phaser项目的Item.js迁移而来
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class Item : ScriptableObject
{
    [BoxGroup("标识信息", Order = 0)]
    [LabelText("物品ID")]
    [InfoBox("唯一标识符，用于任务系统和游戏逻辑识别")]
    public string id;
    
    [BoxGroup("标识信息")]
    [LabelText("物品名称")]
    public string itemName;
    
    [BoxGroup("标识信息")]
    [LabelText("物品描述")]
    [TextArea(3, 5)]
    public string description;
    
    [BoxGroup("标识信息")]
    [LabelText("物品图标")]
    public Sprite icon;
    
    [BoxGroup("标识信息")]
    [LabelText("物品类型")]
    public ItemType itemType;
    
    [BoxGroup("标识信息")]
    [LabelText("稀有度")]
    public ItemRarity rarity;
    
    [BoxGroup("基础属性")]
    [LabelText("物品等级")]
    [PropertyRange(1, 100)]
    public int itemLevel = 1;
    
    [BoxGroup("基础属性")]
    [LabelText("最大堆叠数量")]
    [PropertyRange(1, 999)]
    public int maxStackSize = 1;
    
    [BoxGroup("基础属性")]
    [LabelText("出售价格")]
    [PropertyRange(0, 999999)]
    public int sellPrice;
    
    [BoxGroup("基础属性")]
    [LabelText("购买价格")]
    [PropertyRange(0, 999999)]
    public int buyPrice;
    
    [BoxGroup("基础属性")]
    [LabelText("物品价值")]
    [InfoBox("用于评估物品的相对价值")]
    [PropertyRange(1, 1000)]
    public int itemValue = 1;
    
    [BoxGroup("行为设置")]
    [LabelText("是否可丢弃")]
    public bool isDroppable = true;
    
    [BoxGroup("行为设置")]
    [LabelText("是否可交易")]
    public bool isTradeable = true;
    
    [BoxGroup("行为设置")]
    [LabelText("是否可使用")]
    public bool isUsable = false;
    
    [BoxGroup("行为设置")]
    [LabelText("使用冷却时间")]
    [ShowIf("isUsable")]
    [SuffixLabel("秒")]
    public float cooldown = 0f;
    
    [BoxGroup("行为设置")]
    [LabelText("使用后是否消耗")]
    [ShowIf("isUsable")]
    public bool consumeOnUse = false;
    
    [BoxGroup("获取方式")]
    [LabelText("主要来源")]
    public ItemSource primarySource = ItemSource.Other;
    
    [BoxGroup("获取方式")]
    [LabelText("掉落敌人")]
    [ShowIf("@primarySource == ItemSource.EnemyDrop")]
    public List<string> dropFromEnemies = new List<string>();
    
    [BoxGroup("任务相关配置")]
    [LabelText("可用于收集任务")]
    public bool canBeCollectTarget = true;
    
    [BoxGroup("任务相关配置")]
    [LabelText("可用于寻找任务")]
    public bool canBeFindTarget = true;
    
    [BoxGroup("任务相关配置")]
    [LabelText("可用于使用任务")]
    [ShowIf("isUsable")]
    public bool canBeUseTarget = false;

    
    // 事件
    public static event Action<Item, Character> OnItemUsed;
    public static event Action<Item> OnItemPickedUp;
    public static event Action<Item> OnItemDropped;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public Item()
    {
        // 默认构造函数
    }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public Item(string id, string name, string desc, ItemType type, ItemRarity rarity)
    {
        this.id = id;
        this.itemName = name;
        this.description = desc;
        this.itemType = type;
        this.rarity = rarity;
    }
    
    /// <summary>
    /// 使用物品
    /// </summary>
    public virtual bool Use(Character user)
    {
        if (!isUsable)
        {
            if (GameManager.Instance != null && GameManager.Instance.debugMode)
            {
                Debug.Log($"[Item] 物品 {itemName} 不可使用");
            }
            return false;
        }
        
        // 触发使用事件
        OnItemUsed?.Invoke(this, user);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Item] {user.name} 使用了 {itemName}");
        }
        
        return true;
    }
    
    /// <summary>
    /// 验证配置
    /// </summary>
    public bool ValidateConfig()
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError($"物品配置缺少ID: {name}");
            return false;
        }
        
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogError($"物品配置缺少名称: {id}");
            return false;
        }
        
        if (maxStackSize <= 0)
        {
            Debug.LogError($"物品配置堆叠数量无效: {id}");
            return false;
        }
        
        if (itemValue <= 0)
        {
            Debug.LogError($"物品配置价值无效: {id}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 检查品质是否匹配
    /// </summary>
    public bool MatchesQuality(ItemRarity requiredQuality)
    {
        if (requiredQuality == ItemRarity.Common) return true; // Common表示任意品质
        return rarity == requiredQuality;
    }
    
    /// <summary>
    /// 获取格式化的物品信息
    /// </summary>
    public string GetFormattedInfo()
    {
        string info = $"{itemName} (Lv.{itemLevel})";
        if (maxStackSize > 1)
        {
            info += $" [最大堆叠: {maxStackSize}]";
        }
        return info;
    }
    
    /// <summary>
    /// 检查是否可以从指定敌人掉落
    /// </summary>
    public bool CanDropFromEnemy(string enemyId)
    {
        return primarySource == ItemSource.EnemyDrop && dropFromEnemies.Contains(enemyId);
    }
    
    /// <summary>
    /// 获取物品的完整描述
    /// </summary>
    public virtual string GetFullDescription()
    {
        string fullDesc = $"<color={GetRarityColor()}><b>{itemName}</b></color>\n";
        fullDesc += $"<color=grey>{GetTypeString()}</color>\n\n";
        fullDesc += description;
        
        if (sellPrice > 0)
        {
            fullDesc += $"\n\n<color=yellow>出售价格: {sellPrice} 金币</color>";
        }
        
        return fullDesc;
    }
    
    /// <summary>
    /// 获取稀有度颜色
    /// </summary>
    public string GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "white";
            case ItemRarity.Uncommon: return "green";
            case ItemRarity.Rare: return "blue";
            case ItemRarity.Epic: return "purple";
            case ItemRarity.Legendary: return "orange";
            default: return "white";
        }
    }
    
    /// <summary>
    /// 获取类型字符串
    /// </summary>
    public string GetTypeString()
    {
        switch (itemType)
        {
            case ItemType.Weapon: return "武器";
            case ItemType.Armor: return "护甲";
            case ItemType.Consumable: return "消耗品";
            case ItemType.Material: return "材料";
            case ItemType.Quest: return "任务物品";
            case ItemType.Misc: return "杂项";
            default: return "未知";
        }
    }
    
    /// <summary>
    /// 获取稀有度字符串
    /// </summary>
    public string GetRarityString()
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "普通";
            case ItemRarity.Uncommon: return "不常见";
            case ItemRarity.Rare: return "稀有";
            case ItemRarity.Epic: return "史诗";
            case ItemRarity.Legendary: return "传说";
            default: return "未知";
        }
    }
    
    /// <summary>
    /// 检查是否可以堆叠
    /// </summary>
    public bool CanStackWith(Item other)
    {
        if (other == null) return false;
        return id == other.id && maxStackSize > 1;
    }
    
    /// <summary>
    /// 创建物品副本
    /// </summary>
    public virtual Item Clone()
    {
        Item clone = new Item();
        CopyTo(clone);
        return clone;
    }
    
    /// <summary>
    /// 复制属性到另一个物品
    /// </summary>
    protected virtual void CopyTo(Item target)
    {
        target.id = id;
        target.itemName = itemName;
        target.description = description;
        target.icon = icon;
        target.itemType = itemType;
        target.rarity = rarity;
        target.maxStackSize = maxStackSize;
        target.sellPrice = sellPrice;
        target.buyPrice = buyPrice;
        target.isDroppable = isDroppable;
        target.isTradeable = isTradeable;
        target.isUsable = isUsable;
        target.cooldown = cooldown;
        target.consumeOnUse = consumeOnUse;
    }
    
    /// <summary>
    /// 物品被拾取时调用
    /// </summary>
    public virtual void OnPickedUp()
    {
        OnItemPickedUp?.Invoke(this);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Item] 拾取了 {itemName}");
        }
    }
    
    /// <summary>
    /// 物品被丢弃时调用
    /// </summary>
    public virtual void OnDropped()
    {
        OnItemDropped?.Invoke(this);
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Item] 丢弃了 {itemName}");
        }
    }
    
    /// <summary>
    /// 获取物品的哈希值（用于比较）
    /// </summary>
    public override int GetHashCode()
    {
        return id.GetHashCode();
    }
    
    /// <summary>
    /// 比较两个物品是否相等
    /// </summary>
    public override bool Equals(object obj)
    {
        if (obj is Item other)
        {
            return id == other.id;
        }
        return false;
    }
    
    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        return $"{itemName} (ID: {id}, Type: {itemType}, Rarity: {rarity})";
    }
}

/// <summary>
/// 物品堆叠信息
/// </summary>
[System.Serializable]
public class ItemStack
{
    public Item item;           // 物品引用
    public int quantity;        // 数量
    
    public ItemStack(Item item, int quantity = 1)
    {
        this.item = item;
        this.quantity = Mathf.Max(1, quantity);
    }
    
    /// <summary>
    /// 是否可以添加更多物品
    /// </summary>
    public bool CanAddMore()
    {
        return item != null && quantity < item.maxStackSize;
    }
    
    /// <summary>
    /// 添加物品到堆叠中
    /// </summary>
    public int AddItems(int amount)
    {
        if (item == null) return amount;
        
        int canAdd = Mathf.Min(amount, item.maxStackSize - quantity);
        quantity += canAdd;
        return amount - canAdd; // 返回剩余未添加的数量
    }
    
    /// <summary>
    /// 从堆叠中移除物品
    /// </summary>
    public int RemoveItems(int amount)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        return removed;
    }
    
    /// <summary>
    /// 是否为空
    /// </summary>
    public bool IsEmpty()
    {
        return item == null || quantity <= 0;
    }
    
    /// <summary>
    /// 是否已满
    /// </summary>
    public bool IsFull()
    {
        return item != null && quantity >= item.maxStackSize;
    }
    
    /// <summary>
    /// 克隆堆叠
    /// </summary>
    public ItemStack Clone()
    {
        return new ItemStack(item?.Clone(), quantity);
    }
    
    /// <summary>
    /// 转换为字符串
    /// </summary>
    public override string ToString()
    {
        if (item == null) return "Empty";
        return $"{item.itemName} x{quantity}";
    }
}