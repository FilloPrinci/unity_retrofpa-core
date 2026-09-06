using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>Displays one <see cref="InventoryEntry"/> (icon, name, quantity) in the inventory screen.</summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text quantityText;

        public void Set(ItemData item, int quantity)
        {
            if (icon != null)
            {
                icon.sprite = item.Icon;
                icon.enabled = item.Icon != null;
            }

            if (nameText != null)
            {
                nameText.text = item.DisplayName;
            }

            if (quantityText != null)
            {
                quantityText.text = quantity > 1 ? $"x{quantity}" : string.Empty;
            }
        }
    }
}
