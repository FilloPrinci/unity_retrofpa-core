using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Pause menu, toggled by <see cref="toggleAction"/> (typically the UI
    /// action map's "Cancel", bound to Escape). Pausing/resuming goes
    /// through <see cref="GameManager.SetGameState"/>, which also drives
    /// <see cref="Time.timeScale"/>.
    /// </summary>
    public class PauseMenuUIController : UIScreen
    {
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        protected override void Awake()
        {
            base.Awake();
            resumeButton?.onClick.AddListener(Resume);
            quitButton?.onClick.AddListener(HandleQuitClicked);
        }

        private void OnEnable()
        {
            if (toggleAction != null)
            {
                toggleAction.action.Enable();
                toggleAction.action.performed += HandleTogglePerformed;
            }
        }

        private void OnDisable()
        {
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

        private void HandleQuitClicked()
        {
            Application.Quit();
        }
    }
}
