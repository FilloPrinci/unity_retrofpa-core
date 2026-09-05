using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Starts a <see cref="DialogueData"/> in the <see cref="DialogueManager"/>
    /// when this object is interacted with (via the required <see cref="Interactable"/>).
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueData dialogue;

        private Interactable interactable;

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
            if (dialogue == null)
            {
                Debug.LogWarning("[DialogueTrigger] No DialogueData assigned.", this);
                return;
            }

            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }
}
