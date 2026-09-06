using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Toggleable screen listing everything in <see cref="InventoryManager"/>,
    /// one <see cref="InventorySlotUI"/> per stack.
    /// </summary>
    public class InventoryUIController : UIScreen
    {
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private RectTransform slotsContainer;
        [SerializeField] private InventorySlotUI slotPrefab;

        private readonly List<InventorySlotUI> spawnedSlots = new();

        private void OnEnable()
        {
            InventoryManager.ItemChanged += HandleItemChanged;

            if (toggleAction != null)
            {
                toggleAction.action.Enable();
                toggleAction.action.performed += HandleTogglePerformed;
            }
        }

        private void OnDisable()
        {
            InventoryManager.ItemChanged -= HandleItemChanged;

            if (toggleAction != null)
            {
                toggleAction.action.performed -= HandleTogglePerformed;
                toggleAction.action.Disable();
            }
        }

        private void HandleTogglePerformed(InputAction.CallbackContext context)
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
                Refresh();
            }
        }

        private void HandleItemChanged(ItemData item, int quantity)
        {
            if (IsVisible)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            foreach (InventorySlotUI slot in spawnedSlots)
            {
                Destroy(slot.gameObject);
            }
            spawnedSlots.Clear();

            if (InventoryManager.Instance == null || slotsContainer == null || slotPrefab == null)
            {
                return;
            }

            foreach (InventoryEntry entry in InventoryManager.Instance.Entries)
            {
                InventorySlotUI slot = Instantiate(slotPrefab, slotsContainer);
                slot.gameObject.SetActive(true);
                slot.Set(entry.Item, entry.Quantity);
                spawnedSlots.Add(slot);
            }
        }
    }
}
