using System;
using UnityEngine;
using UnityEngine.Localization;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Marks an object as examinable: interacting with it (via the required
    /// <see cref="Interactable"/>) raises <see cref="Checked"/> with its
    /// <see cref="Description"/>, for a UI (e.g. <see cref="DescriptionUIController"/>)
    /// to show. Unlike <see cref="Collectible"/>, the object stays where it
    /// is and can be checked again any number of times.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class Checkable : MonoBehaviour
    {
        [Tooltip("Text shown when the player checks this object.")]
        [SerializeField]
        private LocalizedString description;

        private Interactable interactable;

        /// <summary>Raised whenever any Checkable is checked.</summary>
        public static event Action<Checkable> Checked;

        public LocalizedString Description => description;

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
            if (description.IsEmpty)
            {
                Debug.LogWarning("[Checkable] No description assigned.", this);
                return;
            }

            Checked?.Invoke(this);
        }
    }
}
