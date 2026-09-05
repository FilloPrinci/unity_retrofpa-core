using UnityEngine;
using UnityEngine.InputSystem;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Raycasts forward from <see cref="rayOrigin"/> (typically the player
    /// camera) and calls <see cref="Interactable.Interact"/> on whatever it
    /// hits, when the interact input is performed.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private LayerMask interactMask = ~0;

        private void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.action.Enable();
                interactAction.action.performed += HandleInteractPerformed;
            }
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.action.performed -= HandleInteractPerformed;
                interactAction.action.Disable();
            }
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            Transform origin = rayOrigin != null ? rayOrigin : transform;
            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, interactRange, interactMask)
                && hit.collider.TryGetComponent(out Interactable interactable))
            {
                interactable.Interact(gameObject);
            }
        }
    }
}
