using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 背包系统 - 管理玩家的物品存储和操作
/// 从原Phaser项目的Inventory.js迁移而来
/// </summary>
[System.Serializable]
public class Inventory : MonoBehaviour
{
    [Header("背包设置")]
    public int maxSlots = 30;           // 最大槽位数
    public bool autoSort = false;       // 自动排序
    public bool autoStack = true;       // 自动堆叠
    
    [Header("调试")]
    public bool showDebugInfo = false;
    
    // 物品槽位
    [SerializeField]
    private List<ItemStack> slots = new List<ItemStack>();
    
    // 事件
    public static event Action<Inventory> OnInventoryChanged;
    public static event Action<Item, int> OnItemAdded;
    public static event Action<Item, int> OnItemRemoved;
    public static event Action<Inventory> OnInventoryFull;
    
    // 属性
    public int SlotCount => slots.Count;
    public int UsedSlots => slots.Count(slot => slot != null && !slot.IsEmpty());
    public int EmptySlots => maxSlots - UsedSlots;
    public bool IsFull => UsedSlots >= maxSlots;
    public bool IsEmpty => UsedSlots == 0;
    
    private void Awake()
    {
        InitializeInventory();
    }
    
    /// <summary>
    /// 初始化背包
    /// </summary>
    private void InitializeInventory()
    {
        // 确保槽位列表有正确的大小
        while (slots.Count < maxSlots)
        {
            slots.Add(null);
        }
        
        // 移除多余的槽位
        while (slots.Count > maxSlots)
        {
            slots.RemoveAt(slots.Count - 1);
        }
        
        if (GameManager.Instance != null && GameManager.Instance.debugMode)
        {
            Debug.Log($"[Inventory] 背包初始化完成，槽位数: {maxSlots}");
        }
    }
    
    /// <summary>
    /// 添加物品到背包
    /// </summary>
    public bool AddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }
        
        int remainingQuantity = quantity;
        
        // 如果启用自动堆叠，先尝试堆叠到现有物品
        if (autoStack && item.maxStackSize > 1)
        {
            remainingQuantity = TryStackItem(item, remainingQuantity);
        }
        
        // 如果还有剩余物品，尝试放入空槽位
        if (remainingQuantity > 0)
        {
            remainingQuantity = TryAddToEmptySlots(item, remainingQuantity);
        }
        
        // 检查是否完全添加成功
        bool success = remainingQuantity == 0;
        int addedQuantity = quantity - remainingQuantity;
        
        if (addedQuantity > 0)
        {
            // 触发事件
            OnItemAdded?.Invoke(item, addedQuantity);
            OnInventoryChanged?.Invoke(this);
            
            // 播放音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("item_pickup");
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[Inventory] 添加物品: {item.itemName} x{addedQuantity}");
            }
        }
        
        // 如果背包满了
        if (remainingQuantity > 0 && IsFull)
        {
            OnInventoryFull?.Invoke(this);
            
            if (showDebugInfo)
            {
                Debug.Log($"[Inventory] 背包已满，无法添加 {item.itemName} x{remainingQuantity}");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 尝试堆叠物品到现有槽位
    /// </summary>
    private int TryStackItem(Item item, int quantity)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && !slots[i].IsEmpty() && slots[i].item.CanStackWith(item))
            {
                quantity = slots[i].AddItems(quantity);
                if (quantity == 0) break;
            }
        }
        return quantity;
    }
    
    /// <summary>
    /// 尝试添加到空槽位
    /// </summary>
    private int TryAddToEmptySlots(Item item, int quantity)
    {
        for (int i = 0; i < slots.Count && quantity > 0; i++)
        {
            if (slots[i] == null || slots[i].IsEmpty())
            {
                int stackSize = Mathf.Min(quantity, item.maxStackSize);
                slots[i] = new ItemStack(item.Clone(), stackSize);
                quantity -= stackSize;
            }
        }
        return quantity;
    }
    
    /// <summary>
    /// 从背包移除物品
    /// </summary>
    public bool RemoveItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }
        
        int remainingToRemove = quantity;
        
        // 从后往前遍历，优先移除后面的物品
        for (int i = slots.Count - 1; i >= 0 && remainingToRemove > 0; i--)
        {
            if (slots[i] != null && !slots[i].IsEmpty() && slots[i].item.Equals(item))
            {
                int removed = slots[i].RemoveItems(remainingToRemove);
                remainingToRemove -= removed;
                
                // 如果槽位空了，清空它
                if (slots[i].IsEmpty())
                {
                    slots[i] = null;
                }
            }
        }
        
        bool success = remainingToRemove == 0;
        int removedQuantity = quantity - remainingToRemove;
        
        if (removedQuantity > 0)
        {
            // 触发事件
            OnItemRemoved?.Invoke(item, removedQuantity);
            OnInventoryChanged?.Invoke(this);
            
            if (showDebugInfo)
            {
                Debug.Log($"[Inventory] 移除物品: {item.itemName} x{removedQuantity}");
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 移除指定槽位的物品
    /// </summary>
    public ItemStack RemoveFromSlot(int slotIndex, int quantity = -1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count || slots[slotIndex] == null || slots[slotIndex].IsEmpty())
        {
            return null;
        }
        
        ItemStack slot = slots[slotIndex];
        
        if (quantity == -1 || quantity >= slot.quantity)
        {
            // 移除整个槽位
            ItemStack removed = slot.Clone();
            slots[slotIndex] = null;
            
            OnItemRemoved?.Invoke(removed.item, removed.quantity);
            OnInventoryChanged?.Invoke(this);
            
            return removed;
        }
        else
        {
            // 移除部分物品
            int actualRemoved = slot.RemoveItems(quantity);
            ItemStack removed = new ItemStack(slot.item.Clone(), actualRemoved);
            
            if (slot.IsEmpty())
            {
                slots[slotIndex] = null;
            }
            
            OnItemRemoved?.Invoke(removed.item, removed.quantity);
            OnInventoryChanged?.Invoke(this);
            
            return removed;
        }
    }
    
    /// <summary>
    /// 获取物品数量
    /// </summary>
    public int GetItemCount(Item item)
    {
        if (item == null) return 0;
        
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot != null && !slot.IsEmpty() && slot.item.Equals(item))
            {
                count += slot.quantity;
            }
        }
        return count;
    }
    
    /// <summary>
    /// 检查是否包含指定数量的物品
    /// </summary>
    public bool HasItem(Item item, int quantity = 1)
    {
        return GetItemCount(item) >= quantity;
    }
    
    /// <summary>
    /// 检查是否可以添加指定数量的物品
    /// </summary>
    public bool CanAddItem(Item item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }
        
        int remainingQuantity = quantity;
        
        // 如果启用自动堆叠，检查现有槽位的堆叠空间
        if (autoStack && item.maxStackSize > 1)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && !slots[i].IsEmpty() && slots[i].item.CanStackWith(item))
                {
                    int spaceLeft = item.maxStackSize - slots[i].quantity;
                    remainingQuantity -= Mathf.Min(remainingQuantity, spaceLeft);
                    if (remainingQuantity <= 0) return true;
                }
            }
        }
        
        // 检查空槽位
        int emptySlots = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null || slots[i].IsEmpty())
            {
                emptySlots++;
            }
        }
        
        // 计算需要的槽位数
        int slotsNeeded = Mathf.CeilToInt((float)remainingQuantity / item.maxStackSize);
        
        return slotsNeeded <= emptySlots;
    }
    
    /// <summary>
    /// 获取指定槽位的物品堆叠
    /// </summary>
    public ItemStack GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return null;
        }
        return slots[slotIndex];
    }
    
    /// <summary>
    /// 设置指定槽位的物品堆叠
    /// </summary>
    public void SetSlot(int slotIndex, ItemStack itemStack)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return;
        }
        
        slots[slotIndex] = itemStack;
        OnInventoryChanged?.Invoke(this);
    }
    
    /// <summary>
    /// 交换两个槽位的物品
    /// </summary>
    public void SwapSlots(int slotA, int slotB)
    {
        if (slotA < 0 || slotA >= slots.Count || slotB < 0 || slotB >= slots.Count)
        {
            return;
        }
        
        ItemStack temp = slots[slotA];
        slots[slotA] = slots[slotB];
        slots[slotB] = temp;
        
        OnInventoryChanged?.Invoke(this);
        
        if (showDebugInfo)
        {
            Debug.Log($"[Inventory] 交换槽位 {slotA} 和 {slotB}");
        }
    }
    
    /// <summary>
    /// 使用物品
    /// </summary>
    public bool UseItem(int slotIndex, Character user)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count || slots[slotIndex] == null || slots[slotIndex].IsEmpty())
        {
            return false;
        }
        
        ItemStack slot = slots[slotIndex];
        Item item = slot.item;
        
        if (!item.isUsable)
        {
            return false;
        }
        
        // 使用物品
        bool success = item.Use(user);
        
        if (success && item.consumeOnUse)
        {
            // 消耗物品
            slot.RemoveItems(1);
            if (slot.IsEmpty())
            {
                slots[slotIndex] = null;
            }
            
            OnInventoryChanged?.Invoke(this);
        }
        
        return success;
    }
    
    /// <summary>
    /// 查找物品的第一个槽位索引
    /// </summary>
    public int FindItemSlot(Item item)
    {
        if (item == null) return -1;
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && !slots[i].IsEmpty() && slots[i].item.Equals(item))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 查找所有包含指定物品的槽位
    /// </summary>
    public List<int> FindAllItemSlots(Item item)
    {
        List<int> result = new List<int>();
        if (item == null) return result;
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && !slots[i].IsEmpty() && slots[i].item.Equals(item))
            {
                result.Add(i);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 获取所有物品列表
    /// </summary>
    public List<ItemStack> GetAllItems()
    {
        return slots.Where(slot => slot != null && !slot.IsEmpty()).ToList();
    }
    
    /// <summary>
    /// 清空背包
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i] = null;
        }
        
        OnInventoryChanged?.Invoke(this);
        
        if (showDebugInfo)
        {
            Debug.Log("[Inventory] 背包已清空");
        }
    }
    
    /// <summary>
    /// 整理背包（移除空槽位，合并相同物品）
    /// </summary>
    public void Organize()
    {
        // 收集所有非空物品
        List<ItemStack> items = new List<ItemStack>();
        foreach (var slot in slots)
        {
            if (slot != null && !slot.IsEmpty())
            {
                items.Add(slot);
            }
        }
        
        // 清空所有槽位
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i] = null;
        }
        
        // 重新添加物品（会自动堆叠）
        foreach (var item in items)
        {
            AddItem(item.item, item.quantity);
        }
        
        OnInventoryChanged?.Invoke(this);
        
        if (showDebugInfo)
        {
            Debug.Log("[Inventory] 背包整理完成");
        }
    }
    
    /// <summary>
    /// 按稀有度排序
    /// </summary>
    public void SortByRarity()
    {
        var items = GetAllItems();
        items.Sort((a, b) => b.item.rarity.CompareTo(a.item.rarity));
        
        // 清空并重新添加
        Clear();
        foreach (var item in items)
        {
            AddItem(item.item, item.quantity);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[Inventory] 按稀有度排序完成");
        }
    }
    
    /// <summary>
    /// 按类型排序
    /// </summary>
    public void SortByType()
    {
        var items = GetAllItems();
        items.Sort((a, b) => a.item.itemType.CompareTo(b.item.itemType));
        
        // 清空并重新添加
        Clear();
        foreach (var item in items)
        {
            AddItem(item.item, item.quantity);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[Inventory] 按类型排序完成");
        }
    }
    
    /// <summary>
    /// 按名称排序
    /// </summary>
    public void SortByName()
    {
        var items = GetAllItems();
        items.Sort((a, b) => string.Compare(a.item.itemName, b.item.itemName, StringComparison.OrdinalIgnoreCase));
        
        // 清空并重新添加
        Clear();
        foreach (var item in items)
        {
            AddItem(item.item, item.quantity);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[Inventory] 按名称排序完成");
        }
    }
    
    /// <summary>
    /// 获取背包状态信息
    /// </summary>
    public string GetStatusInfo()
    {
        return $"背包状态: {UsedSlots}/{maxSlots} 槽位已使用";
    }
    
    /// <summary>
    /// 获取背包的序列化数据
    /// </summary>
    public InventoryData GetSaveData()
    {
        InventoryData data = new InventoryData();
        data.maxSlots = maxSlots;
        data.slots = new List<ItemStackData>();
        
        foreach (var slot in slots)
        {
            if (slot != null && !slot.IsEmpty())
            {
                data.slots.Add(new ItemStackData
                {
                    itemId = slot.item.id,
                    quantity = slot.quantity
                });
            }
            else
            {
                data.slots.Add(null);
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// 从序列化数据加载背包
    /// </summary>
    public void LoadFromData(InventoryData data)
    {
        if (data == null) return;
        
        maxSlots = data.maxSlots;
        InitializeInventory();
        
        for (int i = 0; i < data.slots.Count && i < slots.Count; i++)
        {
            if (data.slots[i] != null)
            {
                // 这里需要通过ItemDatabase获取物品实例
                // Item item = ItemDatabase.GetItem(data.slots[i].itemId);
                // if (item != null)
                // {
                //     slots[i] = new ItemStack(item, data.slots[i].quantity);
                // }
            }
        }
        
        OnInventoryChanged?.Invoke(this);
    }
}

/// <summary>
/// 背包序列化数据
/// </summary>
[System.Serializable]
public class InventoryData
{
    public int maxSlots;
    public List<ItemStackData> slots;
}

/// <summary>
/// 物品堆叠序列化数据
/// </summary>
[System.Serializable]
public class ItemStackData
{
    public string itemId;
    public int quantity;
}