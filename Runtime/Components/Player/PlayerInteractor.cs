using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Raycasts forward from <see cref="rayOrigin"/> (typically the player
    /// camera) every frame to track which <see cref="Interactable"/> (if any)
    /// the player is looking at, raising <see cref="LookTargetChanged"/> when
    /// it changes (for a UI prompt to react to). Calls
    /// <see cref="Interactable.Interact"/> on the current target when the
    /// interact input is performed.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private LayerMask interactMask = ~0;

        /// <summary>Raised whenever the currently looked-at Interactable changes (null when looking at nothing interactable).</summary>
        public static event Action<Interactable> LookTargetChanged;

        private Interactable currentTarget;

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

            if (currentTarget != null)
            {
                currentTarget = null;
                LookTargetChanged?.Invoke(null);
            }
        }

        private void Update()
        {
            UpdateLookTarget();
        }

        private void UpdateLookTarget()
        {
            Interactable target = Raycast();
            if (target == currentTarget)
            {
                return;
            }

            currentTarget = target;
            LookTargetChanged?.Invoke(currentTarget);
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            currentTarget?.Interact(gameObject);
        }

        private Interactable Raycast()
        {
            Transform origin = rayOrigin != null ? rayOrigin : transform;
            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, interactRange, interactMask)
                && hit.collider.TryGetComponent(out Interactable interactable))
            {
                return interactable;
            }

            return null;
        }
    }
}
