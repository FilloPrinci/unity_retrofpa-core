using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Bridges a world <see cref="Collectible"/> to the <see cref="InventoryManager"/>:
    /// when collected, adds <see cref="item"/> to the inventory. Kept
    /// separate from <see cref="Collectible"/> so that component stays
    /// usable without knowing about items/inventory at all.
    /// </summary>
    [RequireComponent(typeof(Collectible))]
    public class CollectibleItem : MonoBehaviour
    {
        [SerializeField] private ItemData item;
        [SerializeField, Min(1)] private int quantity = 1;

        private Collectible collectible;

        private void Awake()
        {
            collectible = GetComponent<Collectible>();
        }

        private void OnEnable()
        {
            collectible.Collected += HandleCollected;
        }

        private void OnDisable()
        {
            collectible.Collected -= HandleCollected;
        }

        private void HandleCollected(GameObject collector)
        {
            if (item == null)
            {
                Debug.LogWarning("[CollectibleItem] No ItemData assigned.", this);
                return;
            }

            InventoryManager.Instance.AddItem(item, quantity);
        }
    }
}
