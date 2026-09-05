using System;
using System.Collections.Generic;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>One stack of a given item held in the inventory.</summary>
    [Serializable]
    public class InventoryEntry
    {
        public ItemData Item { get; internal set; }
        public int Quantity { get; internal set; }

        internal InventoryEntry(ItemData item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// Holds the items the player has collected and which equippable item
    /// (if any) is currently equipped. Decoupled from how items enter the
    /// inventory (see <see cref="CollectibleItem"/>) and from who "the
    /// wielder" is (passed in by the caller, e.g. a future player controller).
    /// </summary>
    public class InventoryManager : PersistentSingleton<InventoryManager>
    {
        private readonly List<InventoryEntry> entries = new();

        /// <summary>Raised after an item stack changes (item, new total quantity).</summary>
        public static event Action<ItemData, int> ItemChanged;

        /// <summary>Raised when the equipped item changes (previous, current). Either can be null.</summary>
        public static event Action<ItemData, ItemData> EquippedItemChanged;

        public IReadOnlyList<InventoryEntry> Entries => entries;

        public ItemData EquippedItem { get; private set; }

        /// <summary>Adds <paramref name="quantity"/> of <paramref name="item"/>, stacking up to its MaxStackSize.</summary>
        public void AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                return;
            }

            InventoryEntry entry = FindEntryWithRoom(item);
            if (entry == null)
            {
                entry = new InventoryEntry(item, 0);
                entries.Add(entry);
            }

            entry.Quantity = Mathf.Min(entry.Quantity + quantity, item.MaxStackSize);
            ItemChanged?.Invoke(item, GetQuantity(item));
        }

        /// <summary>Removes up to <paramref name="quantity"/> of <paramref name="item"/>. Returns how many were actually removed.</summary>
        public int RemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            int remaining = quantity;
            for (int i = entries.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventoryEntry entry = entries[i];
                if (entry.Item != item)
                {
                    continue;
                }

                int taken = Mathf.Min(entry.Quantity, remaining);
                entry.Quantity -= taken;
                remaining -= taken;

                if (entry.Quantity <= 0)
                {
                    entries.RemoveAt(i);
                }
            }

            int removed = quantity - remaining;
            if (removed > 0)
            {
                if (item == EquippedItem && GetQuantity(item) <= 0)
                {
                    Unequip(null);
                }

                ItemChanged?.Invoke(item, GetQuantity(item));
            }

            return removed;
        }

        public int GetQuantity(ItemData item)
        {
            int total = 0;
            foreach (InventoryEntry entry in entries)
            {
                if (entry.Item == item)
                {
                    total += entry.Quantity;
                }
            }

            return total;
        }

        public bool HasItem(ItemData item, int quantity = 1) => GetQuantity(item) >= quantity;

        /// <summary>Equips <paramref name="item"/> (must be an equippable item currently held), unequipping whatever was equipped before.</summary>
        public void Equip(ItemData item, GameObject wielder)
        {
            if (item == null || !item.IsEquippable || !HasItem(item))
            {
                Debug.LogWarning($"[InventoryManager] Cannot equip '{item?.DisplayName}': not held or not equippable.", this);
                return;
            }

            if (item == EquippedItem)
            {
                return;
            }

            Unequip(wielder);

            ItemData previous = EquippedItem;
            EquippedItem = item;
            item.EquippableBehavior.OnEquip(wielder);
            EquippedItemChanged?.Invoke(previous, EquippedItem);
        }

        /// <summary>Unequips whatever is currently equipped, if anything.</summary>
        public void Unequip(GameObject wielder)
        {
            if (EquippedItem == null)
            {
                return;
            }

            ItemData previous = EquippedItem;
            previous.EquippableBehavior.OnUnequip(wielder);
            EquippedItem = null;
            EquippedItemChanged?.Invoke(previous, null);
        }

        private InventoryEntry FindEntryWithRoom(ItemData item)
        {
            foreach (InventoryEntry entry in entries)
            {
                if (entry.Item == item && entry.Quantity < item.MaxStackSize)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
