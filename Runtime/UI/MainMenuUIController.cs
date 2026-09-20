using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Main menu screen: "New Game" triggers <see cref="GameBootstrapper.StartGame"/>
    /// and hides this screen, "Continue" loads the save file via <see cref="SaveManager"/>
    /// instead (disabled when there is none), "Settings" opens
    /// <see cref="settingsScreen"/> as an overlay, "Quit" exits the
    /// application. Visible on startup by default — pair with a
    /// <see cref="GameBootstrapper"/> whose "Auto Start On Awake" is turned
    /// off, so the menu gates the first load.
    /// </summary>
    public class MainMenuUIController : UIScreen
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private UIScreen settingsScreen;

        [Header("Localization")]
        [SerializeField] private LocalizedString newGameLabel;
        [SerializeField] private LocalizedString continueLabel;
        [SerializeField] private LocalizedString settingsLabel;
        [SerializeField] private LocalizedString quitLabel;

        protected override void Awake()
        {
            base.Awake();
            newGameButton?.onClick.AddListener(HandleNewGameClicked);
            continueButton?.onClick.AddListener(HandleContinueClicked);
            settingsButton?.onClick.AddListener(HandleSettingsClicked);
            quitButton?.onClick.AddListener(HandleQuitClicked);

            ApplyLabel(newGameButton, newGameLabel);
            ApplyLabel(continueButton, continueLabel);
            ApplyLabel(settingsButton, settingsLabel);
            ApplyLabel(quitButton, quitLabel);
        }

        protected override void Start()
        {
            base.Start();

            // Not Awake/OnEnable: reading SaveManager.Instance/AudioManager.Instance
            // needs their Awake to have already run, which Unity only
            // guarantees by Start() - see SettingsUIController for the same reasoning.
            if (continueButton != null)
            {
                continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile;
            }

            AudioManager.Instance?.PlayMainMenuMusic();
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

        private void HandleNewGameClicked()
        {
            Hide();

            if (bootstrapper != null)
            {
                bootstrapper.StartGame();
            }
            else
            {
                Debug.LogError("[MainMenuUIController] No GameBootstrapper assigned.", this);
            }
        }

        private void HandleContinueClicked()
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[MainMenuUIController] No SaveManager in the scene.", this);
                return;
            }

            Hide();
            SaveManager.Instance.LoadGame();
        }

        private void HandleSettingsClicked() => settingsScreen?.Show();

        private void HandleQuitClicked()
        {
            Application.Quit();
        }
    }
}
