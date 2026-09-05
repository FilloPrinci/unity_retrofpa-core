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
    [RequireComponent(typeof(Interactable))]
    public class Collectible : MonoBehaviour
    {
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
            onCollected?.Invoke(interactor);
            Collected?.Invoke(interactor);
            gameObject.SetActive(false);
        }
    }
}
