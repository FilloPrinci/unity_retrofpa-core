using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Displays the dialogue currently running in <see cref="DialogueManager"/>:
    /// speaker name, text, and either a "Continue" button (linear nodes) or
    /// one button per choice (branching nodes).
    /// </summary>
    public class DialogueUIController : UIScreen
    {
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button continueButton;
        [SerializeField] private RectTransform choicesContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Localization")]
        [SerializeField] private LocalizedString continueLabel;

        private readonly List<Button> spawnedChoiceButtons = new();

        protected override void Awake()
        {
            base.Awake();

            if (continueButton != null && !continueLabel.IsEmpty)
            {
                TMP_Text label = continueButton.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = continueLabel.GetLocalizedString();
                }
            }
        }

        private void OnEnable()
        {
            DialogueManager.DialogueStarted += HandleDialogueStarted;
            DialogueManager.NodeChanged += HandleNodeChanged;
            DialogueManager.DialogueEnded += HandleDialogueEnded;
            continueButton?.onClick.AddListener(HandleContinueClicked);
        }

        private void OnDisable()
        {
            DialogueManager.DialogueStarted -= HandleDialogueStarted;
            DialogueManager.NodeChanged -= HandleNodeChanged;
            DialogueManager.DialogueEnded -= HandleDialogueEnded;
            continueButton?.onClick.RemoveListener(HandleContinueClicked);
        }

        private void HandleDialogueStarted(DialogueData dialogue) => Show();

        private void HandleDialogueEnded() => Hide();

        private void HandleNodeChanged(DialogueNode node)
        {
            if (speakerText != null)
            {
                speakerText.text = node.SpeakerName;
            }

            if (bodyText != null)
            {
                bodyText.text = node.Text.GetLocalizedString();
            }

            ClearChoiceButtons();

            bool hasChoices = node.Choices.Count > 0;
            continueButton?.gameObject.SetActive(!hasChoices);
            choicesContainer?.gameObject.SetActive(hasChoices);

            if (hasChoices)
            {
                SpawnChoiceButtons(node.Choices);
            }
        }

        private void SpawnChoiceButtons(List<DialogueChoice> choices)
        {
            if (choicesContainer == null || choiceButtonPrefab == null)
            {
                return;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int choiceIndex = i;
                Button button = Instantiate(choiceButtonPrefab, choicesContainer);
                button.gameObject.SetActive(true);

                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = choices[i].ChoiceText.GetLocalizedString();
                }

                button.onClick.AddListener(() => DialogueManager.Instance.SelectChoice(choiceIndex));
                spawnedChoiceButtons.Add(button);
            }
        }

        private void ClearChoiceButtons()
        {
            foreach (Button button in spawnedChoiceButtons)
            {
                Destroy(button.gameObject);
            }

            spawnedChoiceButtons.Clear();
        }

        private void HandleContinueClicked() => DialogueManager.Instance.Advance();
    }
}
