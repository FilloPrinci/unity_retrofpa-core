using System.Collections.Generic;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Lists every <see cref="ItemData"/> in the project, keyed by its
    /// <see cref="ItemData.ItemId"/>. A save file can only store an item as a
    /// string id (a ScriptableObject reference isn't portable across
    /// sessions/builds), so <see cref="SaveManager"/> uses this to resolve
    /// ids back into asset references when loading. Keep it up to date by
    /// hand as items are added - simplest option for a project this size;
    /// revisit (Resources/Addressables-based auto-discovery) only if the
    /// item count grows enough that this becomes a chore.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Retro FPA/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemData> items = new();

        private Dictionary<string, ItemData> lookup;

        /// <summary>Finds the item with the given <see cref="ItemData.ItemId"/>, if listed here.</summary>
        public bool TryGetById(string itemId, out ItemData item)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                item = null;
                return false;
            }

            if (lookup == null)
            {
                BuildLookup();
            }

            return lookup.TryGetValue(itemId, out item);
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, ItemData>();
            foreach (ItemData item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId))
                {
                    continue;
                }

                if (!lookup.TryAdd(item.ItemId, item))
                {
                    Debug.LogWarning($"[ItemDatabase] Duplicate itemId '{item.ItemId}' " +
                                      $"({item.name} vs {lookup[item.ItemId].name}) - keeping the first.", this);
                }
            }
        }
    }
}
