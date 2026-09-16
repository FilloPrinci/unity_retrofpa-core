using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Applies a <see cref="VisualStyleProfile"/> to the persistent global
    /// URP <see cref="Volume"/>, to RenderSettings (fog/ambient), and to a
    /// runtime skybox Material instance. Changing the game's visual style
    /// is just assigning a different profile to this singleton via
    /// <see cref="ApplyProfile"/>.
    /// </summary>
    public class StyleManager : PersistentSingleton<StyleManager>
    {
        [Tooltip("Global Volume whose profile receives the color adjustments/bloom/tonemapping overrides.")]
        [SerializeField]
        private Volume targetVolume;

        [Tooltip("Style applied automatically on startup, if set.")]
        [SerializeField]
        private VisualStyleProfile initialProfile;

        [Tooltip("Template skybox Material (e.g. built on the 'Retro FPA/Gradient Skybox' shader). " +
                 "StyleManager instantiates its own copy on first use, so each " +
                 "VisualStyleProfile's colors/exposure update that copy instead of the " +
                 "shared template asset.")]
        [SerializeField]
        private Material skyboxMaterialTemplate;

        /// <summary>Raised whenever a new style is applied.</summary>
        public static event Action<VisualStyleProfile> StyleChanged;

        public VisualStyleProfile CurrentProfile { get; private set; }

        private Material runtimeSkyboxMaterial;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                // Duplicate instance, already scheduled for destruction by the base class.
                return;
            }

            LevelSceneManager.LevelLoaded += OnLevelLoaded;

            if (initialProfile != null)
            {
                ApplyProfile(initialProfile);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelSceneManager.LevelLoaded -= OnLevelLoaded;

            if (runtimeSkyboxMaterial != null)
            {
                Destroy(runtimeSkyboxMaterial);
            }
        }

        // LevelSceneManager makes each newly loaded level scene the active
        // scene, and RenderSettings (fog/ambient) are per-scene data, so
        // that silently resets them to the level scene's own (usually
        // empty) values. Reapplying here restores the current style on top
        // of whatever the level scene just loaded.
        private void OnLevelLoaded(string sceneName)
        {
            if (CurrentProfile != null)
            {
                ApplyProfile(CurrentProfile);
            }
        }

        /// <summary>Applies <paramref name="profile"/> as the current visual style.</summary>
        public void ApplyProfile(VisualStyleProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("[StyleManager] Tried to apply a null VisualStyleProfile.", this);
                return;
            }

            CurrentProfile = profile;
            profile.ApplyFogAndAmbient();
            ApplySkybox(profile);

            if (targetVolume != null)
            {
                profile.ApplyToVolumeProfile(targetVolume.profile);
            }
            else
            {
                Debug.LogWarning("[StyleManager] No target Volume assigned; skipped post-processing overrides.", this);
            }

            StyleChanged?.Invoke(profile);
        }

        private void ApplySkybox(VisualStyleProfile profile)
        {
            if (!profile.SkyboxEnabled || skyboxMaterialTemplate == null)
            {
                return;
            }

            if (runtimeSkyboxMaterial == null)
            {
                // Instantiate once, so repeated style changes update this
                // one copy's properties instead of touching the shared
                // template asset (same rule as never animating a shared
                // Material directly).
                runtimeSkyboxMaterial = new Material(skyboxMaterialTemplate);
            }

            RenderSettings.skybox = runtimeSkyboxMaterial;
            profile.ApplySkybox(runtimeSkyboxMaterial);
        }
    }
}
