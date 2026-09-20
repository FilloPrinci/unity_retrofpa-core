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

            LevelSceneManager.LevelLoadStarted += HandleLevelLoadStarted;
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.action.performed -= HandleInteractPerformed;
                interactAction.action.Disable();
            }

            LevelSceneManager.LevelLoadStarted -= HandleLevelLoadStarted;
            ClearCurrentTarget();
        }

        // A level load destroys the current target along with the rest of
        // its scene, so clear it up front rather than waiting for next
        // frame's raycast to notice - see the ReferenceEquals note in
        // UpdateLookTarget for why that comparison alone doesn't reliably
        // catch it once the target is already destroyed.
        private void HandleLevelLoadStarted(string sceneName) => ClearCurrentTarget();

        private void ClearCurrentTarget()
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                LookTargetChanged?.Invoke(null);
            }
        }

        private void Update()
        {
            if (!FirstPersonController.IsCursorLocked)
            {
                ClearCurrentTarget();
                return;
            }

            UpdateLookTarget();
        }

        private void UpdateLookTarget()
        {
            Interactable target = Raycast();

            // ReferenceEquals, not ==: Unity's overridden == treats a
            // destroyed object as equal to null, so if currentTarget was
            // destroyed (e.g. its scene got unloaded) while target is a
            // genuine null (nothing hit), target == currentTarget would be
            // true and this would wrongly no-op forever instead of clearing
            // the stale target and its UI prompt.
            if (ReferenceEquals(target, currentTarget))
            {
                return;
            }

            currentTarget = target;
            LookTargetChanged?.Invoke(currentTarget);
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            if (!FirstPersonController.IsCursorLocked)
            {
                return;
            }

            currentTarget?.Interact(gameObject);
        }

        private Interactable Raycast()
        {
            Transform origin = rayOrigin != null ? rayOrigin : transform;
            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, interactRange, interactMask))
            {
                // GetComponentInParent (not TryGetComponent on the hit collider
                // itself) so an Interactable on a root object still works when
                // its collider lives on a child (e.g. an NPC's visual mesh).
                return hit.collider.GetComponentInParent<Interactable>();
            }

            return null;
        }
    }
}
