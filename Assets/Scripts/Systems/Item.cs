using UnityEngine;
using System;

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
/// 物品基类 - 游戏中所有物品的基础类
/// 从原Phaser项目的Item.js迁移而来
/// </summary>
[System.Serializable]
public class Item
{
    [Header("基础信息")]
    public string id;                 // 物品ID
    public string itemName;           // 物品名称
    public string description;        // 物品描述
    public Sprite icon;               // 物品图标
    public ItemType itemType;         // 物品类型
    public ItemRarity rarity;         // 稀有度
    
    [Header("属性")]
    public int maxStackSize = 1;      // 最大堆叠数量
    public int sellPrice;             // 出售价格
    public int buyPrice;              // 购买价格
    public bool isDroppable = true;   // 是否可丢弃
    public bool isTradeable = true;   // 是否可交易
    public bool isUsable = false;     // 是否可使用
    
    [Header("效果")]
    public float cooldown = 0f;       // 使用冷却时间
    public bool consumeOnUse = false; // 使用后是否消耗
    
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