using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Saves/loads the game to a single JSON file on disk (one slot): current
    /// level, exact player position/rotation, inventory contents and
    /// equipped item, and which collectibles (by <see cref="SaveableId"/>)
    /// were already picked up. Also tracks collected ids for the whole
    /// session - not just what made it into the last save - so a
    /// collectible stays gone if its level is simply revisited later, save
    /// file or not.
    /// </summary>
    public class SaveManager : PersistentSingleton<SaveManager>
    {
        [Tooltip("Used to resolve a saved item id back into an ItemData asset.")]
        [SerializeField]
        private ItemDatabase itemDatabase;

        private const string SaveFileName = "save.json";

        private readonly HashSet<string> collectedIds = new();
        private SaveData pendingRestore;

        /// <summary>Raised after a save file is written.</summary>
        public static event Action GameSaved;

        /// <summary>Raised once a loaded save has been fully applied (level, inventory, position).</summary>
        public static event Action GameLoaded;

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>Whether a save file exists on disk.</summary>
        public bool HasSaveFile => File.Exists(SavePath);

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                // Duplicate instance, already scheduled for destruction by the base class.
                return;
            }

            LevelSceneManager.LevelLoaded += HideAlreadyCollected;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelSceneManager.LevelLoaded -= HideAlreadyCollected;
        }

        /// <summary>Marks a <see cref="SaveableId"/> as collected for the rest of the session (and any future save).</summary>
        public void MarkCollected(string saveableId)
        {
            if (!string.IsNullOrEmpty(saveableId))
            {
                collectedIds.Add(saveableId);
            }
        }

        /// <summary>Writes the current level, player position, inventory, and collected ids to disk.</summary>
        public void SaveGame()
        {
            if (LevelSceneManager.Instance == null || InventoryManager.Instance == null)
            {
                Debug.LogError("[SaveManager] Missing LevelSceneManager/InventoryManager; cannot save.", this);
                return;
            }

            string levelName = LevelSceneManager.Instance.CurrentLevelName;
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("[SaveManager] No level currently loaded; nothing to save.", this);
                return;
            }

            var data = new SaveData
            {
                levelSceneName = levelName,
                collectedIds = new List<string>(collectedIds),
                equippedItemId = InventoryManager.Instance.EquippedItem != null
                    ? InventoryManager.Instance.EquippedItem.ItemId
                    : null,
            };

            Transform player = LevelSceneManager.Instance.PersistentPlayerRoot;
            if (player != null)
            {
                data.playerPositionX = player.position.x;
                data.playerPositionY = player.position.y;
                data.playerPositionZ = player.position.z;
                data.playerRotationY = player.eulerAngles.y;
            }

            foreach (InventoryEntry entry in InventoryManager.Instance.Slots)
            {
                data.inventorySlots.Add(entry == null
                    ? new SavedInventorySlot()
                    : new SavedInventorySlot { itemId = entry.Item.ItemId, quantity = entry.Quantity });
            }

            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            GameSaved?.Invoke();
        }

        /// <summary>Loads the save file (if any): loads its level, then restores inventory and player position once that finishes.</summary>
        public void LoadGame()
        {
            if (!HasSaveFile)
            {
                Debug.LogWarning("[SaveManager] No save file to load.", this);
                return;
            }

            if (LevelSceneManager.Instance == null)
            {
                Debug.LogError("[SaveManager] No LevelSceneManager in the scene.", this);
                return;
            }

            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

            collectedIds.Clear();
            foreach (string id in data.collectedIds)
            {
                collectedIds.Add(id);
            }

            pendingRestore = data;
            LevelSceneManager.LevelLoaded += RestoreAfterLoad;
            LevelSceneManager.Instance.LoadLevel(data.levelSceneName);
        }

        private void RestoreAfterLoad(string sceneName)
        {
            LevelSceneManager.LevelLoaded -= RestoreAfterLoad;

            SaveData data = pendingRestore;
            pendingRestore = null;
            if (data == null)
            {
                return;
            }

            RestoreInventory(data);
            RestorePlayerPosition(data);

            GameLoaded?.Invoke();
        }

        private void RestoreInventory(SaveData data)
        {
            if (InventoryManager.Instance == null)
            {
                return;
            }

            InventoryManager.Instance.ClearAll();

            if (itemDatabase == null)
            {
                Debug.LogError("[SaveManager] No ItemDatabase assigned; cannot restore inventory.", this);
                return;
            }

            foreach (SavedInventorySlot slot in data.inventorySlots)
            {
                if (string.IsNullOrEmpty(slot.itemId))
                {
                    continue;
                }

                if (itemDatabase.TryGetById(slot.itemId, out ItemData item))
                {
                    InventoryManager.Instance.AddItem(item, slot.quantity);
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] Saved item id '{slot.itemId}' not found in ItemDatabase.", this);
                }
            }

            if (!string.IsNullOrEmpty(data.equippedItemId) && itemDatabase.TryGetById(data.equippedItemId, out ItemData equipped))
            {
                Transform player = LevelSceneManager.Instance.PersistentPlayerRoot;
                InventoryManager.Instance.Equip(equipped, player != null ? player.gameObject : null);
            }
        }

        private void RestorePlayerPosition(SaveData data)
        {
            Transform player = LevelSceneManager.Instance.PersistentPlayerRoot;
            if (player == null)
            {
                return;
            }

            var position = new Vector3(data.playerPositionX, data.playerPositionY, data.playerPositionZ);
            Quaternion rotation = Quaternion.Euler(0f, data.playerRotationY, 0f);

            if (player.TryGetComponent(out ITeleportable teleportable))
            {
                teleportable.Teleport(position, rotation);
            }
            else
            {
                player.SetPositionAndRotation(position, rotation);
            }
        }

        // Keeps collectibles picked up earlier in the session from
        // reappearing if their level loads again - independent of whether
        // this load came from LoadGame() or just normal level navigation.
        private void HideAlreadyCollected(string sceneName)
        {
            if (collectedIds.Count == 0)
            {
                return;
            }

            foreach (SaveableId saveable in FindObjectsByType<SaveableId>(FindObjectsSortMode.None))
            {
                if (saveable.gameObject.scene.name == sceneName && collectedIds.Contains(saveable.Id))
                {
                    saveable.gameObject.SetActive(false);
                }
            }
        }
    }
}
