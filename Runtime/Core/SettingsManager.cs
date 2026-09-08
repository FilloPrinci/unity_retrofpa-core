using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Loads/saves/applies user-facing settings (audio volumes, look
    /// sensitivity, language, vsync/fullscreen/resolution) via <see cref="PlayerPrefs"/>,
    /// persisting them across sessions. Other systems react via static
    /// events (e.g. <see cref="FirstPersonController"/> listens for
    /// <see cref="LookSensitivityChanged"/>) instead of this manager
    /// reaching into them directly.
    /// </summary>
    public class SettingsManager : PersistentSingleton<SettingsManager>
    {
        private const string MasterVolumeKey = "Settings.MasterVolume";
        private const string MusicVolumeKey = "Settings.MusicVolume";
        private const string SfxVolumeKey = "Settings.SfxVolume";
        private const string LookSensitivityKey = "Settings.LookSensitivity";
        private const string LocaleKey = "Settings.Locale";
        private const string VSyncKey = "Settings.VSync";
        private const string FullscreenKey = "Settings.Fullscreen";
        private const string ResolutionWidthKey = "Settings.ResolutionWidth";
        private const string ResolutionHeightKey = "Settings.ResolutionHeight";

        /// <summary>Raised when the master volume changes (also applied directly to <see cref="AudioListener.volume"/>).</summary>
        public static event Action<float> MasterVolumeChanged;

        /// <summary>Raised when the music volume changes — no music system exists yet; a future one reads this.</summary>
        public static event Action<float> MusicVolumeChanged;

        /// <summary>Raised when the sound-effects volume changes — no SFX system exists yet; a future one reads this.</summary>
        public static event Action<float> SfxVolumeChanged;

        /// <summary>Raised when the look sensitivity changes; <see cref="FirstPersonController"/> listens for this.</summary>
        public static event Action<float> LookSensitivityChanged;

        /// <summary>Raised when the active locale changes.</summary>
        public static event Action<Locale> LocaleChanged;

        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 1f;
        public float SfxVolume { get; private set; } = 1f;
        public bool VSyncEnabled { get; private set; } = true;
        public bool FullscreenEnabled { get; private set; } = true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            LoadAndApplyAll();
        }

        /// <summary>Loads every saved setting from PlayerPrefs (falling back to sensible defaults) and applies it.</summary>
        public void LoadAndApplyAll()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
            SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
            SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
            SetVSyncEnabled(PlayerPrefs.GetInt(VSyncKey, 1) == 1);
            SetFullscreenEnabled(PlayerPrefs.GetInt(FullscreenKey, 1) == 1);

            // No default baked in here: leave FirstPersonController's own
            // Inspector-configured default alone until the player actually
            // changes it once, instead of duplicating that magic number.
            if (PlayerPrefs.HasKey(LookSensitivityKey))
            {
                SetLookSensitivity(PlayerPrefs.GetFloat(LookSensitivityKey));
            }

            if (PlayerPrefs.HasKey(ResolutionWidthKey) && PlayerPrefs.HasKey(ResolutionHeightKey))
            {
                SetResolution(
                    PlayerPrefs.GetInt(ResolutionWidthKey),
                    PlayerPrefs.GetInt(ResolutionHeightKey),
                    Screen.currentResolution.refreshRateRatio);
            }

            if (PlayerPrefs.HasKey(LocaleKey))
            {
                // Localization initializes asynchronously; if AvailableLocales
                // isn't ready yet this silently does nothing (falls back to
                // the project's default locale) rather than blocking startup.
                ApplySavedLocale(PlayerPrefs.GetString(LocaleKey));
            }
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            AudioListener.volume = MasterVolume;
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            PlayerPrefs.Save();
            MasterVolumeChanged?.Invoke(MasterVolume);
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            PlayerPrefs.Save();
            MusicVolumeChanged?.Invoke(MusicVolume);
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            PlayerPrefs.Save();
            SfxVolumeChanged?.Invoke(SfxVolume);
        }

        public void SetLookSensitivity(float value)
        {
            PlayerPrefs.SetFloat(LookSensitivityKey, value);
            PlayerPrefs.Save();
            LookSensitivityChanged?.Invoke(value);
        }

        public void SetVSyncEnabled(bool enabled)
        {
            VSyncEnabled = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            PlayerPrefs.SetInt(VSyncKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetFullscreenEnabled(bool enabled)
        {
            FullscreenEnabled = enabled;
            Screen.fullScreenMode = enabled ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            PlayerPrefs.SetInt(FullscreenKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetResolution(int width, int height, RefreshRate refreshRate)
        {
            Screen.SetResolution(width, height, Screen.fullScreenMode, refreshRate);
            PlayerPrefs.SetInt(ResolutionWidthKey, width);
            PlayerPrefs.SetInt(ResolutionHeightKey, height);
            PlayerPrefs.Save();
        }

        public void SetLocale(Locale locale)
        {
            if (locale == null)
            {
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LocaleKey, locale.Identifier.Code);
            PlayerPrefs.Save();
            LocaleChanged?.Invoke(locale);
        }

        private void ApplySavedLocale(string code)
        {
            foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
            {
                if (locale.Identifier.Code == code)
                {
                    SetLocale(locale);
                    return;
                }
            }
        }
    }
}
