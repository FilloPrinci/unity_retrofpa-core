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

        [Tooltip("Optional sound played on interact. Leave empty for AudioProfile's default interact sound.")]
        [SerializeField]
        private AudioClip interactSound;

        [Tooltip("If set, the interact button must be held for Hold Duration seconds to trigger, instead of just pressed.")]
        [SerializeField]
        private bool requireHold;

        [Tooltip("Seconds the interact button must be held, when Require Hold is set.")]
        [Min(0f)]
        [SerializeField]
        private float holdDuration = 1f;

        [SerializeField]
        private UnityEvent<GameObject> onInteract;

        public LocalizedString PromptText => promptText;

        /// <summary>Whether the interact button must be held (for <see cref="HoldDuration"/>) rather than just pressed.</summary>
        public bool RequiresHold => requireHold;

        public float HoldDuration => holdDuration;

        /// <summary>Raised whenever <see cref="Interact"/> is called, passing the interactor.</summary>
        public event Action<GameObject> Interacted;

        /// <summary>Called by whatever detects the player interacting with this object.</summary>
        public void Interact(GameObject interactor)
        {
            AudioManager.Instance?.PlayInteract(interactSound, transform.position);
            onInteract?.Invoke(interactor);
            Interacted?.Invoke(interactor);
        }
    }
}
