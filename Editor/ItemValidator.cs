using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FilloPrinci.RetroFpa.Editor
{
    /// <summary>Scans every <see cref="ItemData"/> asset in the project for missing/duplicate data.</summary>
    internal static class ItemValidator
    {
        [MenuItem("Retro FPA/Validate/Items")]
        private static void ValidateAllMenuItem() => ValidateAll();

        public static List<string> ValidateAll()
        {
            var messages = new List<string>();
            var seenIds = new Dictionary<string, string>();

            foreach (string guid in AssetDatabase.FindAssets("t:ItemData"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                ValidateItem(item, path, messages, seenIds);
            }

            return messages;
        }

        private static void ValidateItem(ItemData item, string path, List<string> messages, Dictionary<string, string> seenIds)
        {
            if (string.IsNullOrEmpty(item.ItemId))
            {
                Report(messages, item, $"{path}: Item Id is empty.");
            }
            else if (seenIds.TryGetValue(item.ItemId, out string otherPath))
            {
                Report(messages, item, $"{path}: duplicate Item Id '{item.ItemId}' also used by {otherPath}.");
            }
            else
            {
                seenIds[item.ItemId] = path;
            }

            if (item.DisplayName.IsEmpty)
            {
                Report(messages, item, $"{path}: Display Name is empty.");
            }

            if (item.Icon == null)
            {
                Report(messages, item, $"{path}: Icon is not assigned.");
            }

            if (item.WorldPrefab == null)
            {
                Report(messages, item, $"{path}: World Prefab is not assigned (can't be dropped/spawned in-world).");
            }
        }

        private static void Report(List<string> messages, Object context, string message)
        {
            messages.Add(message);
            Debug.LogWarning($"[ItemValidator] {message}", context);
        }
    }
}
