using UnityEngine;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Main menu screen: "New Game" triggers <see cref="GameBootstrapper.StartGame"/>
    /// and hides this screen, "Quit" exits the application. Visible on
    /// startup by default — pair with a <see cref="GameBootstrapper"/> whose
    /// "Auto Start On Awake" is turned off, so the menu gates the first load.
    /// </summary>
    public class MainMenuUIController : UIScreen
    {
        [SerializeField] private GameBootstrapper bootstrapper;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button quitButton;

        protected override void Awake()
        {
            base.Awake();
            newGameButton?.onClick.AddListener(HandleNewGameClicked);
            quitButton?.onClick.AddListener(HandleQuitClicked);
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

        private void HandleQuitClicked()
        {
            Application.Quit();
        }
    }
}
