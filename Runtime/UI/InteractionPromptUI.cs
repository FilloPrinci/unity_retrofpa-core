using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Shows a small prompt (e.g. "[E] Open") while the player is looking at
    /// an <see cref="Interactable"/>, using its optional <see cref="Interactable.PromptText"/>.
    /// For an Interactable that <see cref="Interactable.RequiresHold"/>, also
    /// shows a hold indicator whose fill follows
    /// <see cref="PlayerInteractor.HoldProgressChanged"/>.
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

        [Tooltip("Optional: separate text showing the key label (e.g. on a key icon). Leave empty to prefix the key label to the prompt text instead.")]
        [SerializeField]
        private TMP_Text keyText;

        [Tooltip("Optional: shown only for Interactables that require holding the key.")]
        [SerializeField]
        private GameObject holdIndicator;

        [Tooltip("Optional: a Filled Image whose fill amount follows the hold progress.")]
        [SerializeField]
        private Image holdFill;

        private void OnEnable()
        {
            PlayerInteractor.LookTargetChanged += HandleLookTargetChanged;
            PlayerInteractor.HoldProgressChanged += HandleHoldProgressChanged;
            SetVisible(false);
            HandleHoldProgressChanged(0f);
        }

        private void OnDisable()
        {
            PlayerInteractor.LookTargetChanged -= HandleLookTargetChanged;
            PlayerInteractor.HoldProgressChanged -= HandleHoldProgressChanged;
        }

        private void HandleLookTargetChanged(Interactable target)
        {
            SetVisible(target != null);

            if (target == null)
            {
                return;
            }

            if (holdIndicator != null)
            {
                holdIndicator.SetActive(target.RequiresHold);
            }

            if (keyText != null)
            {
                keyText.text = keyLabel;
            }

            if (promptText != null)
            {
                LocalizedString label = target.PromptText.IsEmpty ? defaultPrompt : target.PromptText;
                string text = label.GetLocalizedString();
                promptText.text = keyText != null ? text : $"{keyLabel} {text}";
            }
        }

        private void HandleHoldProgressChanged(float progress)
        {
            if (holdFill != null)
            {
                holdFill.fillAmount = progress;
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
