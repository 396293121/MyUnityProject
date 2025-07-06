using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 商店系统 - 管理游戏中的商品买卖
/// 从原Phaser项目的商店系统迁移而来
/// </summary>
public class ShopSystem : MonoBehaviour
{
    [Header("商店配置")]
    public List<ShopData> shops = new List<ShopData>();
    
    [Header("商店设置")]
    public float sellPriceMultiplier = 0.5f;     // 出售价格倍数
    public float buyPriceMultiplier = 1.0f;      // 购买价格倍数
    public bool allowSellToAnyShop = true;       // 允许向任何商店出售
    public bool showTransactionEffects = true;   // 显示交易特效
    
    // 单例
    public static ShopSystem Instance { get; private set; }
    
    // 商店状态
    private Dictionary<string, Shop> shopInstances = new Dictionary<string, Shop>();
    private Shop currentShop;
    private PlayerController currentPlayer;
    
    // 事件
    public event Action<Shop> OnShopOpened;
    public event Action<Shop> OnShopClosed;
    public event Action<ShopItem, int> OnItemPurchased;
    public event Action<Item, int> OnItemSold;
    public event Action<Shop> OnShopInventoryUpdated;
    
    private void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeShopSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化商店系统
    /// </summary>
    private void InitializeShopSystem()
    {
        // 创建商店实例
        foreach (ShopData shopData in shops)
        {
            if (!string.IsNullOrEmpty(shopData.shopId))
            {
                Shop shop = new Shop(shopData);
                shopInstances[shopData.shopId] = shop;
            }
        }
        
        // 添加默认商店
        if (shops.Count == 0)
        {
            CreateDefaultShops();
        }
    }
    
    /// <summary>
    /// 创建默认商店
    /// </summary>
    private void CreateDefaultShops()
    {
        // 武器商店
        ShopData weaponShop = new ShopData
        {
            shopId = "weapon_shop",
            shopName = "武器商店",
            description = "各种武器和装备",
            shopType = ShopType.Weapon,
            shopKeeper = "武器商人",
            items = new List<ShopItem>
            {
                new ShopItem { id = "iron_sword", basePrice = 100, currentStock = 5, restockAmount = 2 },
                new ShopItem { id = "steel_sword", basePrice = 250, currentStock = 3, restockAmount = 1 },
                new ShopItem { id = "magic_staff", basePrice = 200, currentStock = 2, restockAmount = 1 },
                new ShopItem { id = "bow", basePrice = 150, currentStock = 4, restockAmount = 2 }
            },
            acceptedItemTypes = new List<ItemType> { ItemType.Weapon },
            restockInterval = 3600f // 1小时补货
        };
        
        // 药水商店
        ShopData potionShop = new ShopData
        {
            shopId = "potion_shop",
            shopName = "药剂师",
            description = "治疗药水和魔法药剂",
            shopType = ShopType.Consumable,
            shopKeeper = "药剂师",
            items = new List<ShopItem>
            {
                new ShopItem { id = "health_potion", basePrice = 25, currentStock = 10, restockAmount = 5 },
                new ShopItem { id = "mana_potion", basePrice = 30, currentStock = 8, restockAmount = 4 },
                new ShopItem { id = "stamina_potion", basePrice = 20, currentStock = 6, restockAmount = 3 },
                new ShopItem { id = "antidote", basePrice = 15, currentStock = 5, restockAmount = 2 }
            },
            acceptedItemTypes = new List<ItemType> { ItemType.Consumable, ItemType.Material },
            restockInterval = 1800f // 30分钟补货
        };
        
        // 杂货商店
        ShopData generalShop = new ShopData
        {
            shopId = "general_shop",
            shopName = "杂货店",
            description = "各种日用品和材料",
            shopType = ShopType.General,
            shopKeeper = "商人",
            items = new List<ShopItem>
            {
                new ShopItem { id = "rope", basePrice = 5, currentStock = 20, restockAmount = 10 },
                new ShopItem { id = "torch", basePrice = 3, currentStock = 15, restockAmount = 8 },
                new ShopItem { id = "food_ration", basePrice = 8, currentStock = 12, restockAmount = 6 }
            },
            acceptedItemTypes = new List<ItemType> { ItemType.Material, ItemType.Misc },
            restockInterval = 7200f // 2小时补货
        };
        
        shops.AddRange(new[] { weaponShop, potionShop, generalShop });
        
        // 更新商店实例
        foreach (ShopData shopData in shops)
        {
            Shop shop = new Shop(shopData);
            shopInstances[shopData.shopId] = shop;
        }
    }
    
    /// <summary>
    /// 打开商店
    /// </summary>
    public bool OpenShop(string shopId, PlayerController player)
    {
        if (!shopInstances.ContainsKey(shopId))
        {
            Debug.LogWarning($"Shop '{shopId}' not found!");
            return false;
        }
        
        currentShop = shopInstances[shopId];
        currentPlayer = player;
        
        // 检查商店是否需要补货
        currentShop.CheckRestock();
        
        // 触发事件
        OnShopOpened?.Invoke(currentShop);
        
        // 显示商店UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowShopUI(currentShop);
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("shop_open");
        }
        
        return true;
    }
    
    /// <summary>
    /// 关闭商店
    /// </summary>
    public void CloseShop()
    {
        if (currentShop == null) return;
        
        Shop closingShop = currentShop;
        currentShop = null;
        currentPlayer = null;
        
        // 触发事件
        OnShopClosed?.Invoke(closingShop);
        
        // 隐藏商店UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideShopUI();
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("shop_close");
        }
    }
    
    /// <summary>
    /// 购买物品
    /// </summary>
    public bool PurchaseItem(string itemId, int quantity = 1)
    {
        if (currentShop == null || currentPlayer == null)
        {
            Debug.LogWarning("No shop or player available!");
            return false;
        }
        
        ShopItem shopItem = currentShop.GetShopItem(itemId);
        if (shopItem == null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("商品不存在！");
            }
            return false;
        }
        
        // 检查库存
        if (shopItem.currentStock < quantity)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("库存不足！");
            }
            return false;
        }
        
        // 计算价格
        int totalPrice = CalculatePurchasePrice(shopItem, quantity);
        
        // 检查玩家金币
        if (!HasEnoughGold(totalPrice))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("金币不足！");
            }
            return false;
        }
        
        // 检查背包空间
        if (!HasInventorySpace(itemId, quantity))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("背包空间不足！");
            }
            return false;
        }
        
        // 执行购买
        DeductGold(totalPrice);
        AddItemToInventory(itemId, quantity);
        shopItem.currentStock -= quantity;
        
        // 触发事件
        OnItemPurchased?.Invoke(shopItem, quantity);
        OnShopInventoryUpdated?.Invoke(currentShop);
        
        // 显示消息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"购买成功: {GetItemName(itemId)} x{quantity}");
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("purchase_success");
        }
        
        // 显示特效
        if (showTransactionEffects)
        {
            ShowPurchaseEffect();
        }
        
        return true;
    }
    
    /// <summary>
    /// 出售物品
    /// </summary>
    public bool SellItem(Item item, int quantity = 1)
    {
        if (currentShop == null || currentPlayer == null)
        {
            Debug.LogWarning("No shop or player available!");
            return false;
        }
        
        // 检查是否可以出售到当前商店
        if (!CanSellToShop(item))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("该商店不收购此类物品！");
            }
            return false;
        }
        
        // 检查玩家是否拥有足够的物品
        if (!HasItemInInventory(item, quantity))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("物品数量不足！");
            }
            return false;
        }
        
        // 计算出售价格
        int sellPrice = CalculateSellPrice(item, quantity);
        
        // 执行出售
        RemoveItemFromInventory(item, quantity);
        AddGold(sellPrice);
        
        // 触发事件
        OnItemSold?.Invoke(item, quantity);
        
        // 显示消息
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage($"出售成功: {item.itemName} x{quantity}, 获得 {sellPrice} 金币");
        }
        
        // 播放音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("sell_success");
        }
        
        // 显示特效
        if (showTransactionEffects)
        {
            ShowSellEffect();
        }
        
        return true;
    }
    
    /// <summary>
    /// 计算购买价格
    /// </summary>
    private int CalculatePurchasePrice(ShopItem shopItem, int quantity)
    {
        float basePrice = shopItem.basePrice * buyPriceMultiplier;
        
        // 根据库存调整价格（库存少价格高）
        float stockMultiplier = 1.0f;
        if (shopItem.currentStock <= shopItem.restockAmount)
        {
            stockMultiplier = 1.2f; // 库存不足时价格上涨20%
        }
        
        return Mathf.RoundToInt(basePrice * stockMultiplier * quantity);
    }
    
    /// <summary>
    /// 计算出售价格
    /// </summary>
    private int CalculateSellPrice(Item item, int quantity)
    {
        float basePrice = item.sellPrice * sellPriceMultiplier;
        
        // 根据物品稀有度调整价格
        float rarityMultiplier = GetRarityPriceMultiplier(item.rarity);
        
        return Mathf.RoundToInt(basePrice * rarityMultiplier * quantity);
    }
    
    /// <summary>
    /// 获取稀有度价格倍数
    /// </summary>
    private float GetRarityPriceMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 1.0f;
            case ItemRarity.Uncommon: return 1.2f;
            case ItemRarity.Rare: return 1.5f;
            case ItemRarity.Epic: return 2.0f;
            case ItemRarity.Legendary: return 3.0f;
            default: return 1.0f;
        }
    }
    
    /// <summary>
    /// 检查是否可以出售到商店
    /// </summary>
    private bool CanSellToShop(Item item)
    {
        if (allowSellToAnyShop) return true;
        
        return currentShop.acceptedItemTypes.Contains(item.itemType);
    }
    
    /// <summary>
    /// 检查是否有足够金币
    /// </summary>
    private bool HasEnoughGold(int amount)
    {
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetPlayerGold() >= amount;
        }
        return false;
    }
    
    /// <summary>
    /// 扣除金币
    /// </summary>
    private void DeductGold(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerGold(-amount);
        }
    }
    
    /// <summary>
    /// 增加金币
    /// </summary>
    private void AddGold(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPlayerGold(amount);
        }
    }
    
    /// <summary>
    /// 检查背包空间
    /// </summary>
    private bool HasInventorySpace(string itemId, int quantity)
    {
        if (currentPlayer != null)
        {
            Inventory inventory = currentPlayer.GetInventory();
            if (inventory != null)
            {
                Item item = GetItemById(itemId);
                if (item != null)
                {
                    return inventory.CanAddItem(item, quantity);
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// 添加物品到背包
    /// </summary>
    private void AddItemToInventory(string itemId, int quantity)
    {
        if (currentPlayer != null)
        {
            Inventory inventory = currentPlayer.GetInventory();
            if (inventory != null)
            {
                Item item = GetItemById(itemId);
                if (item != null)
                {
                    inventory.AddItem(item, quantity);
                }
            }
        }
    }
    
    /// <summary>
    /// 检查背包中是否有物品
    /// </summary>
    private bool HasItemInInventory(Item item, int quantity)
    {
        if (currentPlayer != null)
        {
            Inventory inventory = currentPlayer.GetInventory();
            if (inventory != null)
            {
                return inventory.GetItemCount(item) >= quantity;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 从背包移除物品
    /// </summary>
    private void RemoveItemFromInventory(Item item, int quantity)
    {
        if (currentPlayer != null)
        {
            Inventory inventory = currentPlayer.GetInventory();
            if (inventory != null)
            {
                inventory.RemoveItem(item, quantity);
            }
        }
    }
    
    /// <summary>
    /// 根据ID获取物品对象
    /// </summary>
    private Item GetItemById(string itemId)
    {
        if (GameConfig.Instance != null)
        {
            ItemData itemData = GameConfig.Instance.GetItemData(itemId);
            if (itemData != null)
            {
                return new Item(itemData.itemId, itemData.itemName, itemData.description, itemData.itemType, itemData.rarity);
            }
        }
        return null;
    }
    
    /// <summary>
    /// 获取物品名称
    /// </summary>
    private string GetItemName(string itemId)
    {
        if (GameConfig.Instance != null)
        {
            ItemData itemData = GameConfig.Instance.GetItemData(itemId);
            return itemData?.itemName ?? itemId;
        }
        return itemId;
    }
    
    /// <summary>
    /// 显示购买特效
    /// </summary>
    private void ShowPurchaseEffect()
    {
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect("purchase_effect", currentPlayer.transform.position);
        }
    }
    
    /// <summary>
    /// 显示出售特效
    /// </summary>
    private void ShowSellEffect()
    {
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEffect("sell_effect", currentPlayer.transform.position);
        }
    }
    
    /// <summary>
    /// 获取商店
    /// </summary>
    public Shop GetShop(string shopId)
    {
        return shopInstances.ContainsKey(shopId) ? shopInstances[shopId] : null;
    }
    
    /// <summary>
    /// 获取当前商店
    /// </summary>
    public Shop GetCurrentShop()
    {
        return currentShop;
    }
    
    /// <summary>
    /// 获取所有商店
    /// </summary>
    public List<Shop> GetAllShops()
    {
        return shopInstances.Values.ToList();
    }
    
    /// <summary>
    /// 保存商店数据
    /// </summary>
    public ShopSaveData GetSaveData()
    {
        ShopSaveData saveData = new ShopSaveData();
        
        foreach (var kvp in shopInstances)
        {
            saveData.shopStates.Add(kvp.Value.GetSaveData());
        }
        
        return saveData;
    }
    
    /// <summary>
    /// 加载商店数据
    /// </summary>
    public void LoadSaveData(ShopSaveData saveData)
    {
        foreach (ShopSaveData.ShopState shopState in saveData.shopStates)
        {
            if (shopInstances.ContainsKey(shopState.shopId))
            {
                shopInstances[shopState.shopId].LoadSaveData(shopState);
            }
        }
    }
}

/// <summary>
/// 商店数据
/// </summary>
[Serializable]
public class ShopData
{
    public string shopId;
    public string shopName;
    public string description;
    public ShopType shopType;
    public string shopKeeper;
    public List<ShopItem> items = new List<ShopItem>();
    public List<ItemType> acceptedItemTypes = new List<ItemType>();
    public float restockInterval = 3600f; // 补货间隔（秒）
    public bool isOpen = true;
    public Vector2 openHours = new Vector2(0, 24); // 营业时间
}

/// <summary>
/// 商店实例
/// </summary>
[Serializable]
public class Shop
{
    public string shopId;
    public string shopName;
    public string description;
    public ShopType shopType;
    public string shopKeeper;
    public List<ShopItem> items;
    public List<ItemType> acceptedItemTypes;
    public float restockInterval;
    public bool isOpen;
    public Vector2 openHours;
    public DateTime lastRestockTime;
    
    public Shop(ShopData data)
    {
        shopId = data.shopId;
        shopName = data.shopName;
        description = data.description;
        shopType = data.shopType;
        shopKeeper = data.shopKeeper;
        items = new List<ShopItem>(data.items);
        acceptedItemTypes = new List<ItemType>(data.acceptedItemTypes);
        restockInterval = data.restockInterval;
        isOpen = data.isOpen;
        openHours = data.openHours;
        lastRestockTime = DateTime.Now;
    }
    
    public ShopItem GetShopItem(string itemId)
    {
        return items.FirstOrDefault(item => item.id == itemId);
    }
    
    public void CheckRestock()
    {
        if ((DateTime.Now - lastRestockTime).TotalSeconds >= restockInterval)
        {
            RestockItems();
        }
    }
    
    private void RestockItems()
    {
        foreach (ShopItem item in items)
        {
            item.currentStock = Mathf.Min(item.currentStock + item.restockAmount, item.maxStock);
        }
        lastRestockTime = DateTime.Now;
    }
    
    public bool IsOpen()
    {
        if (!isOpen) return false;
        
        float currentHour = DateTime.Now.Hour + DateTime.Now.Minute / 60f;
        return currentHour >= openHours.x && currentHour <= openHours.y;
    }
    
    public ShopSaveData.ShopState GetSaveData()
    {
        return new ShopSaveData.ShopState
        {
            shopId = shopId,
            items = items.ToList(),
            lastRestockTime = lastRestockTime.ToBinary()
        };
    }
    
    public void LoadSaveData(ShopSaveData.ShopState saveData)
    {
        items = saveData.items;
        lastRestockTime = DateTime.FromBinary(saveData.lastRestockTime);
    }
}

/// <summary>
/// 商店物品
/// </summary>
[Serializable]
public class ShopItem
{
    public string id;
    public int basePrice;
    public int currentStock;
    public int maxStock = 99;
    public int restockAmount = 1;
    public bool isLimited = false;
    public DateTime availableFrom;
    public DateTime availableUntil;
}

/// <summary>
/// 商店保存数据
/// </summary>
[Serializable]
public class ShopSaveData
{
    public List<ShopState> shopStates = new List<ShopState>();
    
    [Serializable]
    public class ShopState
    {
        public string shopId;
        public List<ShopItem> items;
        public long lastRestockTime;
    }
}

/// <summary>
/// 商店类型枚举
/// </summary>
public enum ShopType
{
    General,     // 杂货店
    Weapon,      // 武器店
    Armor,       // 防具店
    Consumable,  // 药水店
    Magic,       // 魔法店
    Special      // 特殊商店
}