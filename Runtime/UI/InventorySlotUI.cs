using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// One fixed cell in the inventory grid: an icon (empty/blank when the
    /// slot holds nothing), an optional quantity badge, a selection
    /// highlight, and a click event so <see cref="InventoryUIController"/>
    /// can react to it being picked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Color normalColor = new(1f, 1f, 1f, 0.12f);
        [SerializeField] private Color selectedColor = new(1f, 1f, 1f, 0.45f);

        private Button button;

        /// <summary>Raised when this slot is clicked.</summary>
        public event Action Clicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => Clicked?.Invoke());
        }

        /// <summary>Displays <paramref name="item"/>/<paramref name="quantity"/> in this slot.</summary>
        public void Set(ItemData item, int quantity)
        {
            if (icon != null)
            {
                icon.sprite = item.Icon;
                icon.enabled = item.Icon != null;
            }

            if (quantityText != null)
            {
                quantityText.text = quantity > 1 ? $"x{quantity}" : string.Empty;
            }
        }

        /// <summary>Clears this slot back to its empty look.</summary>
        public void SetEmpty()
        {
            if (icon != null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.text = string.Empty;
            }
        }

        /// <summary>Toggles the selection highlight.</summary>
        public void SetSelected(bool selected)
        {
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }
    }
}
