using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// The game's global audio: UI feedback sounds, the main menu music, and
    /// the default gameplay sounds (footstep/pickup/interact) used whenever
    /// an object doesn't specify its own. Applied once to <see cref="AudioManager"/>
    /// and kept for the whole session - same split as <see cref="VisualStyleProfile"/>
    /// (global look) vs. <see cref="SceneAtmosphereProfile"/> (per-level fog/sky):
    /// these sounds are the same across every level, unlike a level's own
    /// ambient track (see <see cref="SceneAmbientAudio"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "AudioProfile", menuName = "Retro FPA/Audio Profile")]
    public class AudioProfile : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] private AudioClip uiHoverSound;
        [SerializeField] private AudioClip uiConfirmSound;

        [Header("Main Menu")]
        [SerializeField] private AudioClip mainMenuMusic;

        [Header("Default Gameplay Sounds")]
        [Tooltip("Played when the player is walking and the surface beneath them (its SurfaceAudio, if any) doesn't specify its own footstep sound.")]
        [SerializeField]
        private AudioClip defaultFootstepSound;
        [Tooltip("Played when a Collectible is picked up and doesn't specify its own sound.")]
        [SerializeField]
        private AudioClip defaultPickupSound;
        [Tooltip("Played when an Interactable is interacted with and doesn't specify its own sound.")]
        [SerializeField]
        private AudioClip defaultInteractSound;

        public AudioClip UIHoverSound => uiHoverSound;
        public AudioClip UIConfirmSound => uiConfirmSound;
        public AudioClip MainMenuMusic => mainMenuMusic;
        public AudioClip DefaultFootstepSound => defaultFootstepSound;
        public AudioClip DefaultPickupSound => defaultPickupSound;
        public AudioClip DefaultInteractSound => defaultInteractSound;
    }
}
