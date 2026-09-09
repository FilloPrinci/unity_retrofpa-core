using System;
using System.Collections.Generic;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>One stack of a given item held in one inventory slot.</summary>
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
    /// Holds the items the player has collected, in a fixed number of slots
    /// (<see cref="Capacity"/>) — a slot is either empty (null in
    /// <see cref="Slots"/>) or holds one stack. Also tracks which equippable
    /// item (if any) is currently equipped. Decoupled from how items enter
    /// the inventory (see <see cref="CollectibleItem"/>) and from who "the
    /// wielder" is (passed in by the caller).
    /// </summary>
    public class InventoryManager : PersistentSingleton<InventoryManager>
    {
        [Tooltip("Number of inventory slots. Fixed for the lifetime of the session — set once here.")]
        [SerializeField]
        private int capacity = 12;

        private InventoryEntry[] slots;

        /// <summary>Raised after slot <c>index</c> changes; <c>entry</c> is null when the slot became/stayed empty.</summary>
        public static event Action<int, InventoryEntry> SlotChanged;

        /// <summary>Raised when the equipped item changes (previous, current). Either can be null.</summary>
        public static event Action<ItemData, ItemData> EquippedItemChanged;

        public int Capacity => slots?.Length ?? 0;

        public IReadOnlyList<InventoryEntry> Slots => slots;

        public ItemData EquippedItem { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            slots = new InventoryEntry[Mathf.Max(0, capacity)];
        }

        /// <summary>
        /// Adds up to <paramref name="quantity"/> of <paramref name="item"/>,
        /// stacking into existing matching slots first, then into empty
        /// slots. Returns how many were actually added — less than
        /// <paramref name="quantity"/> (possibly 0) if the inventory is full.
        /// </summary>
        public int AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            int remaining = quantity;

            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                InventoryEntry entry = slots[i];
                if (entry == null || entry.Item != item || entry.Quantity >= item.MaxStackSize)
                {
                    continue;
                }

                int added = Mathf.Min(item.MaxStackSize - entry.Quantity, remaining);
                entry.Quantity += added;
                remaining -= added;
                SlotChanged?.Invoke(i, entry);
            }

            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i] != null)
                {
                    continue;
                }

                int added = Mathf.Min(item.MaxStackSize, remaining);
                slots[i] = new InventoryEntry(item, added);
                remaining -= added;
                SlotChanged?.Invoke(i, slots[i]);
            }

            return quantity - remaining;
        }

        /// <summary>Removes up to <paramref name="quantity"/> of <paramref name="item"/>. Returns how many were actually removed.</summary>
        public int RemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                return 0;
            }

            int remaining = quantity;
            for (int i = slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                InventoryEntry entry = slots[i];
                if (entry == null || entry.Item != item)
                {
                    continue;
                }

                int taken = Mathf.Min(entry.Quantity, remaining);
                entry.Quantity -= taken;
                remaining -= taken;

                if (entry.Quantity <= 0)
                {
                    slots[i] = null;
                }

                SlotChanged?.Invoke(i, slots[i]);
            }

            int removed = quantity - remaining;
            if (removed > 0 && item == EquippedItem && GetQuantity(item) <= 0)
            {
                Unequip(null);
            }

            return removed;
        }

        public int GetQuantity(ItemData item)
        {
            int total = 0;
            foreach (InventoryEntry entry in slots)
            {
                if (entry != null && entry.Item == item)
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
                Debug.LogWarning($"[InventoryManager] Cannot equip '{item?.name}': not held or not equippable.", this);
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
    }
}
