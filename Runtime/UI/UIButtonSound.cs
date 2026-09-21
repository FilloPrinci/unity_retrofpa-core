using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Plays the global UI hover/confirm sounds (<see cref="AudioProfile"/>,
    /// via <see cref="AudioManager"/>) for the <see cref="Button"/> on this
    /// GameObject. Add it to any button that should make sound. The hover
    /// sound fires on mouse pointer-enter and on gamepad/keyboard navigation
    /// (<see cref="ISelectHandler"/>), but not again when a click selects the
    /// button. Don't want a hover sound at all? Leave
    /// <see cref="AudioProfile"/>'s UI Hover Sound empty.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
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

        public void OnPointerEnter(PointerEventData eventData) => AudioManager.Instance?.PlayUIHover();

        public void OnSelect(BaseEventData eventData)
        {
            // A click selects the button too, passing the pointer event along
            // (navigation doesn't) - the pointer already played its hover sound
            // on enter, so don't repeat it on the click.
            if (eventData is PointerEventData)
            {
                return;
            }

            AudioManager.Instance?.PlayUIHover();
        }

        private void HandleClick() => AudioManager.Instance?.PlayUIConfirm();
    }
}
