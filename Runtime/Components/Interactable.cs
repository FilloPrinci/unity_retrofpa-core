using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Generic "this object can be interacted with" building block. Other
    /// systems (a future player interaction controller, level scripting,
    /// etc.) call <see cref="Interact"/>; this component only fires the
    /// resulting event, staying decoupled from however interaction input is
    /// actually detected/triggered.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Optional prompt text a future UI could show while this object is interactable. Leave empty for a generic default.")]
        [SerializeField]
        private LocalizedString promptText;

        [SerializeField]
        private UnityEvent<GameObject> onInteract;

        public LocalizedString PromptText => promptText;

        /// <summary>Raised whenever <see cref="Interact"/> is called, passing the interactor.</summary>
        public event Action<GameObject> Interacted;

        /// <summary>Called by whatever detects the player interacting with this object.</summary>
        public void Interact(GameObject interactor)
        {
            onInteract?.Invoke(interactor);
            Interacted?.Invoke(interactor);
        }
    }
}
