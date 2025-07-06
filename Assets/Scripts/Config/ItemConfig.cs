using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品配置文件
/// 管理游戏中所有物品的配置信息
/// 从原Phaser项目的ItemData.js和EquipmentData.js迁移而来
/// </summary>
[System.Serializable]
public class ItemEffect
{
    [Header("效果类型")]
    public string type; // heal, mana, buff, damage等
    public ItemEffectType effectType; // 枚举类型的效果类型
    
    [Header("效果数值")]
    public float value;
    public float duration = 0f; // 持续时间（毫秒），0表示瞬间效果
    
    [Header("效果描述")]
    public string description;
}

[System.Serializable]
public class ItemConfig
{
    [Header("基础信息")]
    public string id;
    public string name;
    [TextArea(2, 4)]
    public string description;
    public string category; // consumable, equipment, material, quest等
    public string subCategory; // potion, weapon, armor等
    
    [Header("物品属性")]
    public int maxStack = 1;
    public int value = 0; // 商店价格
    public string rarity = "common"; // common, uncommon, rare, epic, legendary
    
    [Header("图标配置")]
    public Sprite icon;
    public string iconPath;
    
    [Header("物品效果")]
    public List<ItemEffect> effects;
    
    [Header("使用配置")]
    public bool consumable = false;
    public bool tradeable = true;
    public bool droppable = true;
    
    [Header("装备属性（仅装备类物品）")]
    public EquipmentStats equipmentStats;
    public string equipmentSlot; // weapon, armor, accessory等
    public int requiredLevel = 1;
    
    /// <summary>
    /// 获取物品效果
    /// </summary>
    /// <param name="effectType">效果类型</param>
    /// <returns>物品效果</returns>
    public ItemEffect GetEffect(string effectType)
    {
        return effects?.Find(effect => effect.type == effectType);
    }
    
    /// <summary>
    /// 是否为消耗品
    /// </summary>
    /// <returns>是否为消耗品</returns>
    public bool IsConsumable()
    {
        return consumable || category == "consumable";
    }
    
    /// <summary>
    /// 是否为装备
    /// </summary>
    /// <returns>是否为装备</returns>
    public bool IsEquipment()
    {
        return category == "equipment" && equipmentStats != null;
    }
    
    /// <summary>
    /// 获取物品数据
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns>物品数据</returns>
    public ItemData GetItemData(string itemId)
    {
        // 将ItemConfig转换为ItemData格式
        if (this.id == itemId)
        {
            var itemData = new ItemData();
            itemData.itemId = this.id;
            itemData.itemName = this.name;
            itemData.description = this.description;
            itemData.icon = this.icon;
            itemData.maxStackSize = this.maxStack;
            itemData.sellPrice = this.value;
            itemData.buyPrice = this.value;
            itemData.canDrop = this.droppable;
            itemData.canTrade = this.tradeable;
            itemData.canUse = this.consumable;
            
            // 转换效果列表
            if (this.effects != null)
            {
                foreach (var effect in this.effects)
                {
                    var itemEffect = new ItemEffect();
                    // 根据效果类型设置对应的ItemEffectType
                    switch (effect.type.ToLower())
                    {
                        case "heal":
                            itemEffect.effectType = ItemEffectType.InstantHeal;
                            break;
                        case "mana":
                            itemEffect.effectType = ItemEffectType.InstantMana;
                            break;
                        case "buff":
                            itemEffect.effectType = ItemEffectType.StrengthBoost;
                            break;
                        default:
                            itemEffect.effectType = ItemEffectType.InstantHeal;
                            break;
                    }
                    itemEffect.value = effect.value;
                    itemEffect.duration = effect.duration;
                    itemData.effects.Add(itemEffect);
                }
            }
            
            return itemData;
        }
        
        return null;
    }
}

[System.Serializable]
public class EquipmentStats
{
    [Header("基础属性加成")]
    public int attackBonus = 0;
    public int defenseBonus = 0;
    public int healthBonus = 0;
    public int manaBonus = 0;
    
    [Header("属性加成")]
    public int strengthBonus = 0;
    public int agilityBonus = 0;
    public int vitalityBonus = 0;
    public int intelligenceBonus = 0;
    
    [Header("特殊属性")]
    public float criticalChance = 0f;
    public float criticalDamage = 0f;
    public float attackSpeed = 0f;
    public float movementSpeed = 0f;
    
    [Header("抗性")]
    public float fireResistance = 0f;
    public float iceResistance = 0f;
    public float lightningResistance = 0f;
    public float poisonResistance = 0f;
}

[System.Serializable]
public class CraftingRecipe
{
    [Header("配方信息")]
    public string recipeId;
    public string resultItemId;
    public int resultQuantity = 1;
    
    [Header("所需材料")]
    public List<CraftingMaterial> materials;
    
    [Header("配方要求")]
    public int requiredLevel = 1;
    public string requiredSkill;
    public int requiredSkillLevel = 1;
    
    [Header("制作参数")]
    public float craftingTime = 1f; // 制作时间（秒）
    public int experienceGain = 10;
}

[System.Serializable]
public class CraftingMaterial
{
    [Header("材料信息")]
    public string itemId;
    public int quantity = 1;
    public bool consumed = true; // 是否在制作时消耗
}

[System.Serializable]
public class ItemCategory
{
    [Header("分类信息")]
    public string categoryId;
    public string displayName;
    public string description;
    public Sprite categoryIcon;
    
    [Header("分类配置")]
    public Color categoryColor = Color.white;
    public int sortOrder = 0;
    public List<string> subCategories;
}

/// <summary>
/// 物品配置管理器
/// 提供统一的物品配置访问接口
/// </summary>
[CreateAssetMenu(fileName = "ItemConfig", menuName = "Game Config/Item Config")]
public class ItemConfigSO : ScriptableObject
{
    [Header("物品配置列表")]
    public List<ItemConfig> itemConfigs;
    
    [Header("物品分类")]
    public List<ItemCategory> itemCategories;
    
    [Header("制作配方")]
    public List<CraftingRecipe> craftingRecipes;
    
    [Header("稀有度配置")]
    public List<RarityConfig> rarityConfigs;
    
    /// <summary>
    /// 获取物品配置
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns>物品配置</returns>
    public ItemConfig GetItemConfig(string itemId)
    {
        return itemConfigs?.Find(config => 
            config.id.Equals(itemId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取分类下的所有物品
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>物品配置列表</returns>
    public List<ItemConfig> GetItemsByCategory(string category)
    {
        return itemConfigs?.FindAll(config => 
            config.category.Equals(category, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取子分类下的所有物品
    /// </summary>
    /// <param name="subCategory">子分类名称</param>
    /// <returns>物品配置列表</returns>
    public List<ItemConfig> GetItemsBySubCategory(string subCategory)
    {
        return itemConfigs?.FindAll(config => 
            config.subCategory.Equals(subCategory, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取制作配方
    /// </summary>
    /// <param name="recipeId">配方ID</param>
    /// <returns>制作配方</returns>
    public CraftingRecipe GetCraftingRecipe(string recipeId)
    {
        return craftingRecipes?.Find(recipe => 
            recipe.recipeId.Equals(recipeId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取物品的制作配方
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns>制作配方列表</returns>
    public List<CraftingRecipe> GetRecipesForItem(string itemId)
    {
        return craftingRecipes?.FindAll(recipe => 
            recipe.resultItemId.Equals(itemId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取物品分类配置
    /// </summary>
    /// <param name="categoryId">分类ID</param>
    /// <returns>分类配置</returns>
    public ItemCategory GetItemCategory(string categoryId)
    {
        return itemCategories?.Find(category => 
            category.categoryId.Equals(categoryId, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取稀有度配置
    /// </summary>
    /// <param name="rarity">稀有度</param>
    /// <returns>稀有度配置</returns>
    public RarityConfig GetRarityConfig(string rarity)
    {
        return rarityConfigs?.Find(config => 
            config.rarity.Equals(rarity, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 验证物品ID是否存在
    /// </summary>
    /// <param name="itemId">物品ID</param>
    /// <returns>是否存在</returns>
    public bool IsValidItemId(string itemId)
    {
        return GetItemConfig(itemId) != null;
    }
    
    /// <summary>
    /// 获取所有可用物品ID
    /// </summary>
    /// <returns>物品ID列表</returns>
    public List<string> GetAllItemIds()
    {
        var ids = new List<string>();
        if (itemConfigs != null)
        {
            foreach (var config in itemConfigs)
            {
                if (!string.IsNullOrEmpty(config.id))
                {
                    ids.Add(config.id);
                }
            }
        }
        return ids;
    }
}

[System.Serializable]
public class RarityConfig
{
    [Header("稀有度信息")]
    public string rarity;
    public string displayName;
    public Color color = Color.white;
    
    [Header("稀有度参数")]
    public float dropChanceMultiplier = 1f;
    public float valueMultiplier = 1f;
    public int sortOrder = 0;
}

/// <summary>
/// 物品配置常量
/// 定义常用的物品配置值
/// </summary>
public static class ItemConfigConstants
{
    // 物品分类常量
    public const string CATEGORY_CONSUMABLE = "consumable";
    public const string CATEGORY_EQUIPMENT = "equipment";
    public const string CATEGORY_MATERIAL = "material";
    public const string CATEGORY_QUEST = "quest";
    
    // 子分类常量
    public const string SUBCATEGORY_POTION = "potion";
    public const string SUBCATEGORY_WEAPON = "weapon";
    public const string SUBCATEGORY_ARMOR = "armor";
    public const string SUBCATEGORY_ACCESSORY = "accessory";
    
    // 稀有度常量
    public const string RARITY_COMMON = "common";
    public const string RARITY_UNCOMMON = "uncommon";
    public const string RARITY_RARE = "rare";
    public const string RARITY_EPIC = "epic";
    public const string RARITY_LEGENDARY = "legendary";
    
    // 效果类型常量
    public const string EFFECT_HEAL = "heal";
    public const string EFFECT_MANA = "mana";
    public const string EFFECT_BUFF = "buff";
    public const string EFFECT_DAMAGE = "damage";
    
    // 装备槽位常量
    public const string SLOT_WEAPON = "weapon";
    public const string SLOT_ARMOR = "armor";
    public const string SLOT_ACCESSORY = "accessory";
}