using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Settings screen: audio volumes, look sensitivity, language, vsync and
    /// resolution — all read from/written to <see cref="SettingsManager"/>,
    /// which persists them across sessions. Opened as an overlay from the
    /// main menu and/or pause menu (see their <c>settingsScreen</c> field).
    /// </summary>
    public class SettingsUIController : UIScreen
    {
        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Controls")]
        [SerializeField] private Slider lookSensitivitySlider;

        [Header("Language")]
        [SerializeField] private TMP_Dropdown localeDropdown;

        [Header("Graphics")]
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [SerializeField] private Button closeButton;

        [Header("Localization")]
        [SerializeField] private LocalizedString closeLabel;

        private readonly List<Locale> availableLocales = new();
        private Resolution[] availableResolutions;

        protected override void Awake()
        {
            base.Awake();
            closeButton?.onClick.AddListener(Hide);

            if (closeButton != null && !closeLabel.IsEmpty)
            {
                TMP_Text label = closeButton.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = closeLabel.GetLocalizedString();
                }
            }
        }

        private void OnEnable()
        {
            PopulateFromSettings();

            masterVolumeSlider?.onValueChanged.AddListener(HandleMasterVolumeChanged);
            musicVolumeSlider?.onValueChanged.AddListener(HandleMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.AddListener(HandleSfxVolumeChanged);
            lookSensitivitySlider?.onValueChanged.AddListener(HandleLookSensitivityChanged);
            localeDropdown?.onValueChanged.AddListener(HandleLocaleChanged);
            vsyncToggle?.onValueChanged.AddListener(HandleVSyncChanged);
            resolutionDropdown?.onValueChanged.AddListener(HandleResolutionChanged);
        }

        private void OnDisable()
        {
            masterVolumeSlider?.onValueChanged.RemoveListener(HandleMasterVolumeChanged);
            musicVolumeSlider?.onValueChanged.RemoveListener(HandleMusicVolumeChanged);
            sfxVolumeSlider?.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
            lookSensitivitySlider?.onValueChanged.RemoveListener(HandleLookSensitivityChanged);
            localeDropdown?.onValueChanged.RemoveListener(HandleLocaleChanged);
            vsyncToggle?.onValueChanged.RemoveListener(HandleVSyncChanged);
            resolutionDropdown?.onValueChanged.RemoveListener(HandleResolutionChanged);
        }

        private void PopulateFromSettings()
        {
            if (SettingsManager.Instance == null)
            {
                return;
            }

            masterVolumeSlider?.SetValueWithoutNotify(SettingsManager.Instance.MasterVolume);
            musicVolumeSlider?.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume);
            sfxVolumeSlider?.SetValueWithoutNotify(SettingsManager.Instance.SfxVolume);
            vsyncToggle?.SetIsOnWithoutNotify(SettingsManager.Instance.VSyncEnabled);

            PopulateLocales();
            PopulateResolutions();
        }

        private void PopulateLocales()
        {
            if (localeDropdown == null)
            {
                return;
            }

            availableLocales.Clear();
            availableLocales.AddRange(LocalizationSettings.AvailableLocales.Locales);

            var options = new List<string>();
            int selectedIndex = 0;
            for (int i = 0; i < availableLocales.Count; i++)
            {
                options.Add(availableLocales[i].LocaleName);
                if (availableLocales[i] == LocalizationSettings.SelectedLocale)
                {
                    selectedIndex = i;
                }
            }

            localeDropdown.ClearOptions();
            localeDropdown.AddOptions(options);
            localeDropdown.SetValueWithoutNotify(selectedIndex);
        }

        private void PopulateResolutions()
        {
            if (resolutionDropdown == null)
            {
                return;
            }

            availableResolutions = Screen.resolutions;

            var options = new List<string>();
            int selectedIndex = 0;
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                Resolution r = availableResolutions[i];
                options.Add($"{r.width} x {r.height}");
                if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                {
                    selectedIndex = i;
                }
            }

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(selectedIndex);
        }

        private void HandleMasterVolumeChanged(float value) => SettingsManager.Instance?.SetMasterVolume(value);

        private void HandleMusicVolumeChanged(float value) => SettingsManager.Instance?.SetMusicVolume(value);

        private void HandleSfxVolumeChanged(float value) => SettingsManager.Instance?.SetSfxVolume(value);

        private void HandleLookSensitivityChanged(float value) => SettingsManager.Instance?.SetLookSensitivity(value);

        private void HandleLocaleChanged(int index)
        {
            if (index >= 0 && index < availableLocales.Count)
            {
                SettingsManager.Instance?.SetLocale(availableLocales[index]);
            }
        }

        private void HandleVSyncChanged(bool value) => SettingsManager.Instance?.SetVSyncEnabled(value);

        private void HandleResolutionChanged(int index)
        {
            if (availableResolutions != null && index >= 0 && index < availableResolutions.Length)
            {
                Resolution r = availableResolutions[index];
                SettingsManager.Instance?.SetResolution(r.width, r.height, r.refreshRateRatio);
            }
        }
    }
}
