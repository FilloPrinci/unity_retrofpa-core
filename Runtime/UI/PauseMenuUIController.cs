using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Pause menu, toggled by <see cref="toggleAction"/> (typically the UI
    /// action map's "Cancel", bound to Escape). Pausing/resuming goes
    /// through <see cref="GameManager.SetGameState"/>, which also drives
    /// <see cref="Time.timeScale"/>. "Settings" opens <see cref="settingsScreen"/>
    /// as an overlay.
    /// </summary>
    public class PauseMenuUIController : UIScreen
    {
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private UIScreen settingsScreen;

        [Header("Localization")]
        [SerializeField] private LocalizedString resumeLabel;
        [SerializeField] private LocalizedString settingsLabel;
        [SerializeField] private LocalizedString quitLabel;

        protected override void Awake()
        {
            base.Awake();
            resumeButton?.onClick.AddListener(Resume);
            settingsButton?.onClick.AddListener(HandleSettingsClicked);
            quitButton?.onClick.AddListener(HandleQuitClicked);

            ApplyLabel(resumeButton, resumeLabel);
            ApplyLabel(settingsButton, settingsLabel);
            ApplyLabel(quitButton, quitLabel);
        }

        private static void ApplyLabel(Button button, LocalizedString label)
        {
            if (button == null || label.IsEmpty)
            {
                return;
            }

            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = label.GetLocalizedString();
            }
        }

        private void OnEnable()
        {
            if (toggleAction != null)
            {
                toggleAction.action.Enable();
                toggleAction.action.performed += HandleTogglePerformed;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (toggleAction != null)
            {
                toggleAction.action.performed -= HandleTogglePerformed;
                toggleAction.action.Disable();
            }
        }

        private void HandleTogglePerformed(InputAction.CallbackContext context)
        {
            if (IsVisible)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Pause()
        {
            Show();
            GameManager.Instance?.SetGameState(GameState.Paused);
        }

        private void Resume()
        {
            Hide();
            GameManager.Instance?.SetGameState(GameState.Playing);
        }

        private void HandleSettingsClicked() => settingsScreen?.Show();

        private void HandleQuitClicked()
        {
            Application.Quit();
        }
    }
}
