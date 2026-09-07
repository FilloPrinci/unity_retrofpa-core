using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Shows a small prompt (e.g. "[E] Open") while the player is looking at
    /// an <see cref="Interactable"/>, using its optional <see cref="Interactable.PromptText"/>.
    /// Not a <see cref="UIScreen"/>: always active, never blocks input/cursor.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private LocalizedString defaultPrompt;
        [Tooltip("Not localized — a key/button name, shown as-is regardless of language.")]
        [SerializeField]
        private string keyLabel = "[E]";

        private void OnEnable()
        {
            PlayerInteractor.LookTargetChanged += HandleLookTargetChanged;
            SetVisible(false);
        }

        private void OnDisable()
        {
            PlayerInteractor.LookTargetChanged -= HandleLookTargetChanged;
        }

        private void HandleLookTargetChanged(Interactable target)
        {
            SetVisible(target != null);

            if (target != null && promptText != null)
            {
                LocalizedString label = target.PromptText.IsEmpty ? defaultPrompt : target.PromptText;
                promptText.text = $"{keyLabel} {label.GetLocalizedString()}";
            }
        }

        private void SetVisible(bool visible)
        {
            if (promptRoot != null)
            {
                promptRoot.SetActive(visible);
            }
        }
    }
}
