using System;
using UnityEngine;
using UnityEngine.Events;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Marks an object as collectible: interacting with it (via the required
    /// <see cref="Interactable"/>) raises <see cref="Collected"/> once, then
    /// disables the object. Deliberately knows nothing about inventory or
    /// items — a future InventoryManager/ItemData integration (step 6) hooks
    /// into <see cref="Collected"/> instead, keeping this component decoupled.
    /// </summary>
    /// <remarks>
    /// Both this and the required <see cref="Interactable"/> can play a
    /// sound - the Interactable's on every interact (its own clip, or
    /// AudioProfile's default interact sound), this one specifically for the
    /// pickup. If a distinct pickup sound is enough here and the generic
    /// interact click would be redundant, leave this GameObject's
    /// Interactable field empty and AudioProfile's default interact sound
    /// null (only non-collectible interactables would then use it).
    /// </remarks>
    [RequireComponent(typeof(Interactable))]
    public class Collectible : MonoBehaviour
    {
        [Tooltip("Optional sound played on pickup. Leave empty for AudioProfile's default pickup sound.")]
        [SerializeField]
        private AudioClip pickupSound;

        [SerializeField]
        private UnityEvent<GameObject> onCollected;

        private Interactable interactable;
        private bool collected;

        /// <summary>Raised once, the first time this object is collected.</summary>
        public event Action<GameObject> Collected;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            interactable.Interacted += HandleInteracted;
        }

        private void OnDisable()
        {
            interactable.Interacted -= HandleInteracted;
        }

        private void HandleInteracted(GameObject interactor)
        {
            if (collected)
            {
                return;
            }

            collected = true;
            AudioManager.Instance?.PlayPickup(pickupSound, transform.position);
            onCollected?.Invoke(interactor);
            Collected?.Invoke(interactor);
            gameObject.SetActive(false);
        }
    }
}
