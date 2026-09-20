using System;
using System.Collections.Generic;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Everything a save file needs to restore the game: which level, where
    /// in it, the inventory, and which one-shot world objects (collectibles)
    /// were already used up. Plain serializable data, not a ScriptableObject
    /// - a ScriptableObject reference (like an ItemData) can't be written
    /// into a save file that has to survive between sessions/builds, so
    /// items are stored by their string <see cref="ItemData.ItemId"/> and
    /// resolved back through an <see cref="ItemDatabase"/> on load. Written/
    /// read as JSON by <see cref="SaveManager"/>.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public string levelSceneName;

        public float playerPositionX;
        public float playerPositionY;
        public float playerPositionZ;
        public float playerRotationY;

        public List<SavedInventorySlot> inventorySlots = new();
        public string equippedItemId;

        public List<string> collectedIds = new();
    }

    /// <summary>One inventory slot's saved contents. An empty slot has a null/empty <see cref="itemId"/>.</summary>
    [Serializable]
    public class SavedInventorySlot
    {
        public string itemId;
        public int quantity;
    }
}
