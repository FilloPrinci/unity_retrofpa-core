using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Plays the global UI selection/confirm sounds (<see cref="AudioProfile"/>,
    /// via <see cref="AudioManager"/>) for the <see cref="Button"/> on this
    /// GameObject. Add it to any button that should make sound. The
    /// selection sound fires when the button becomes the selected one
    /// (gamepad/keyboard navigation) - NOT on mouse hover, and NOT when the
    /// selection comes from a click (that only plays the confirm sound).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour, ISelectHandler
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        public void OnSelect(BaseEventData eventData)
        {
            // A click selects the button too, passing the pointer event along;
            // navigation (keyboard/gamepad) doesn't. Only navigation should
            // play the selection sound, so a click is just the confirm sound.
            if (eventData is PointerEventData)
            {
                return;
            }

            AudioManager.Instance?.PlayUIHover();
        }

        private void HandleClick() => AudioManager.Instance?.PlayUIConfirm();
    }
}
