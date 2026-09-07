using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Base class for a full-screen (or overlay) UI panel driven by a
    /// <see cref="CanvasGroup"/>: handles show/hide and, optionally, cursor
    /// locking so the player can interact with the UI while it's open.
    /// Multiple screens can be visible at once — the cursor stays unlocked
    /// as long as at least one is.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        private static int visibleScreenCount;

        [SerializeField] private bool hiddenOnStart = true;
        [SerializeField] private bool unlocksCursorWhileVisible = true;

        private CanvasGroup canvasGroup;

        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            // Applied directly (not via SetVisible) because a fresh
            // CanvasGroup added in the Inspector defaults to alpha 1
            // regardless of hiddenOnStart — a hidden-on-start screen was
            // never counted as "visible" in visibleScreenCount to begin
            // with, so only screens that start visible need to register.
            IsVisible = !hiddenOnStart;
            canvasGroup.alpha = IsVisible ? 1f : 0f;
            canvasGroup.interactable = IsVisible;
            canvasGroup.blocksRaycasts = IsVisible;

            if (unlocksCursorWhileVisible && IsVisible)
            {
                visibleScreenCount++;
                FirstPersonController.SetCursorLocked(visibleScreenCount == 0);
            }
        }

        protected virtual void OnDisable()
        {
            if (IsVisible)
            {
                SetVisible(false);
            }
        }

        public void Show() => SetVisible(true);

        public void Hide() => SetVisible(false);

        private void SetVisible(bool visible)
        {
            if (visible == IsVisible)
            {
                return;
            }

            IsVisible = visible;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            if (unlocksCursorWhileVisible)
            {
                visibleScreenCount = Mathf.Max(0, visibleScreenCount + (visible ? 1 : -1));
                FirstPersonController.SetCursorLocked(visibleScreenCount == 0);
            }
        }
    }
}
