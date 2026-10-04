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
    /// interact input is pressed or, for a target that
    /// <see cref="Interactable.RequiresHold"/>, once it has been held for the
    /// target's <see cref="Interactable.HoldDuration"/> (raising
    /// <see cref="HoldProgressChanged"/> meanwhile). The interact action
    /// itself must be a plain press - no Hold interaction on it.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private LayerMask interactMask = ~0;

        /// <summary>Raised whenever the currently looked-at Interactable changes (null when looking at nothing interactable).</summary>
        public static event Action<Interactable> LookTargetChanged;

        /// <summary>Raised every frame while a hold is in progress, with its 0..1 progress, and with 0 when it ends (completed or cancelled).</summary>
        public static event Action<float> HoldProgressChanged;

        private Interactable currentTarget;
        private Interactable holdTarget;
        private float holdElapsed;

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
            CancelHold();

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
            UpdateHold();
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

            // Looking away cancels a hold in progress.
            CancelHold();
            currentTarget = target;
            LookTargetChanged?.Invoke(currentTarget);
        }

        private void HandleInteractPerformed(InputAction.CallbackContext context)
        {
            if (!FirstPersonController.IsCursorLocked || currentTarget == null)
            {
                return;
            }

            if (currentTarget.RequiresHold)
            {
                // Only a fresh press starts a hold, so keeping the button
                // down after one completes doesn't retrigger it.
                holdTarget = currentTarget;
                holdElapsed = 0f;
                HoldProgressChanged?.Invoke(0f);
                return;
            }

            currentTarget.Interact(gameObject);
        }

        private void UpdateHold()
        {
            if (ReferenceEquals(holdTarget, null))
            {
                return;
            }

            if (holdTarget == null || !interactAction.action.IsPressed())
            {
                CancelHold();
                return;
            }

            holdElapsed += Time.deltaTime;
            float duration = holdTarget.HoldDuration;
            float progress = duration > 0f ? Mathf.Clamp01(holdElapsed / duration) : 1f;
            HoldProgressChanged?.Invoke(progress);

            if (progress >= 1f)
            {
                Interactable target = holdTarget;
                CancelHold();
                target.Interact(gameObject);
            }
        }

        private void CancelHold()
        {
            // ReferenceEquals: a hold target destroyed mid-hold must still
            // be cleared (and its progress reset), see UpdateLookTarget.
            if (!ReferenceEquals(holdTarget, null))
            {
                holdTarget = null;
                HoldProgressChanged?.Invoke(0f);
            }
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
