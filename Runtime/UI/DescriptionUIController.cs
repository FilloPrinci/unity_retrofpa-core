using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Modal box showing a <see cref="Checkable"/>'s description when it's
    /// checked. While open, gameplay input is halted (via
    /// <see cref="FirstPersonController.BlockGameplayInput"/>) but the cursor
    /// stays locked; performing <see cref="closeAction"/> closes it.
    /// Leave <c>unlocksCursorWhileVisible</c> off on this screen.
    /// </summary>
    public class DescriptionUIController : UIScreen
    {
        [SerializeField] private TMP_Text bodyText;

        [Tooltip("Closes the box when performed - typically the same Interact action used to open it. Not enabled/disabled here: whoever owns the action (PlayerInteractor) does that.")]
        [SerializeField]
        private InputActionReference closeAction;

        private bool blockingGameplay;
        private int openedFrame = -1;

        private void OnEnable()
        {
            Checkable.Checked += HandleChecked;
            LevelSceneManager.LevelLoadStarted += HandleLevelLoadStarted;

            if (closeAction != null)
            {
                closeAction.action.performed += HandleClosePerformed;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Checkable.Checked -= HandleChecked;
            LevelSceneManager.LevelLoadStarted -= HandleLevelLoadStarted;

            if (closeAction != null)
            {
                closeAction.action.performed -= HandleClosePerformed;
            }

            Close();
        }

        private void HandleChecked(Checkable checkable)
        {
            if (bodyText != null)
            {
                bodyText.text = checkable.Description.GetLocalizedString();
            }

            openedFrame = Time.frameCount;
            Show();

            if (!blockingGameplay)
            {
                blockingGameplay = true;
                FirstPersonController.BlockGameplayInput();
            }
        }

        private void HandleClosePerformed(InputAction.CallbackContext context)
        {
            // Same frame: this is the very press that opened the box (the
            // interactor handled it first), not a new one. Cursor unlocked:
            // another screen (e.g. the pause menu) is on top.
            if (!IsVisible || Time.frameCount == openedFrame || !FirstPersonController.IsCursorLocked)
            {
                return;
            }

            Close();
        }

        private void HandleLevelLoadStarted(string sceneName) => Close();

        private void Close()
        {
            Hide();

            if (blockingGameplay)
            {
                blockingGameplay = false;
                FirstPersonController.UnblockGameplayInput();
            }
        }
    }
}
