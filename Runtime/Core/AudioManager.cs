using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Plays every sound in the game: UI feedback (through <see cref="AudioProfile"/>),
    /// one-shot world sounds (footstep/pickup/interact - each with an
    /// object-specific clip or the profile's default), and looping music
    /// (main menu, or one level's ambient track via <see cref="SceneAmbientAudio"/> -
    /// only one plays at a time, whichever was requested most recently).
    /// Two <see cref="AudioSource"/>s are created at runtime rather than
    /// scene-authored, since nothing about them needs to be hand-tuned per
    /// project - swap <see cref="Profile"/> to reskin the whole game's audio.
    /// </summary>
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [Tooltip("Audio applied automatically on startup, if set.")]
        [SerializeField]
        private AudioProfile initialProfile;

        [Tooltip("Volume for one-shot world sounds (footstep/pickup/interact), played via AudioSource.PlayClipAtPoint.")]
        [SerializeField, Range(0f, 1f)]
        private float worldSoundVolume = 1f;

        private AudioSource uiSource;
        private AudioSource musicSource;

        public AudioProfile Profile { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                // Duplicate instance, already scheduled for destruction by the base class.
                return;
            }

            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;

            if (initialProfile != null)
            {
                ApplyProfile(initialProfile);
            }
        }

        /// <summary>Applies <paramref name="profile"/> as the current global audio.</summary>
        public void ApplyProfile(AudioProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("[AudioManager] Tried to apply a null AudioProfile.", this);
                return;
            }

            Profile = profile;
        }

        public void PlayUIHover() => PlayOneShotUI(Profile != null ? Profile.UIHoverSound : null);

        public void PlayUIConfirm() => PlayOneShotUI(Profile != null ? Profile.UIConfirmSound : null);

        /// <summary>Plays the main menu music, replacing whatever was already playing.</summary>
        public void PlayMainMenuMusic() => PlayMusic(Profile != null ? Profile.MainMenuMusic : null);

        /// <summary>Plays one level's ambient track (see <see cref="SceneAmbientAudio"/>), replacing whatever was already playing.</summary>
        public void PlaySceneAmbient(AudioClip clip) => PlayMusic(clip);

        /// <summary>Plays a footstep at <paramref name="position"/> - <paramref name="overrideClip"/> if given (e.g. from a <see cref="SurfaceAudio"/>), else the profile's default.</summary>
        public void PlayFootstep(AudioClip overrideClip, Vector3 position) =>
            PlayAtPoint(overrideClip != null ? overrideClip : Profile?.DefaultFootstepSound, position);

        /// <summary>Plays a pickup sound at <paramref name="position"/> - <paramref name="overrideClip"/> if given, else the profile's default.</summary>
        public void PlayPickup(AudioClip overrideClip, Vector3 position) =>
            PlayAtPoint(overrideClip != null ? overrideClip : Profile?.DefaultPickupSound, position);

        /// <summary>Plays an interact sound at <paramref name="position"/> - <paramref name="overrideClip"/> if given, else the profile's default.</summary>
        public void PlayInteract(AudioClip overrideClip, Vector3 position) =>
            PlayAtPoint(overrideClip != null ? overrideClip : Profile?.DefaultInteractSound, position);

        private void PlayOneShotUI(AudioClip clip)
        {
            if (clip != null)
            {
                uiSource.PlayOneShot(clip);
            }
        }

        private void PlayAtPoint(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, worldSoundVolume);
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = clip;

            if (clip != null)
            {
                musicSource.Play();
            }
            else
            {
                musicSource.Stop();
            }
        }
    }
}
