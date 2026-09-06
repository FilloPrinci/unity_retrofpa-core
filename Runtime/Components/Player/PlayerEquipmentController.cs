using UnityEngine;
using UnityEngine.InputSystem;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Fires the currently equipped item's primary action (via
    /// <see cref="InventoryManager.EquippedItem"/>) when the attack input is
    /// performed. Does nothing if no equippable item is currently equipped.
    /// </summary>
    public class PlayerEquipmentController : MonoBehaviour
    {
        [SerializeField] private InputActionReference attackAction;

        private void OnEnable()
        {
            if (attackAction != null)
            {
                attackAction.action.Enable();
                attackAction.action.performed += HandleAttackPerformed;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.action.performed -= HandleAttackPerformed;
                attackAction.action.Disable();
            }
        }

        private void HandleAttackPerformed(InputAction.CallbackContext context)
        {
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[PlayerEquipmentController] No InventoryManager in the scene.", this);
                return;
            }

            ItemData equipped = InventoryManager.Instance.EquippedItem;
            equipped?.EquippableBehavior?.PerformAction(gameObject);
        }
    }
}
