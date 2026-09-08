using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Settings screen: audio volumes, look sensitivity, language,
    /// vsync/fullscreen/resolution — all read from/written to <see cref="SettingsManager"/>,
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
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [SerializeField] private Button closeButton;

        [Header("Row Labels")]
        [SerializeField] private TMP_Text masterVolumeLabelText;
        [SerializeField] private TMP_Text musicVolumeLabelText;
        [SerializeField] private TMP_Text sfxVolumeLabelText;
        [SerializeField] private TMP_Text lookSensitivityLabelText;
        [SerializeField] private TMP_Text languageLabelText;
        [SerializeField] private TMP_Text vsyncLabelText;
        [SerializeField] private TMP_Text fullscreenLabelText;
        [SerializeField] private TMP_Text resolutionLabelText;

        [Header("Localization")]
        [SerializeField] private LocalizedString closeLabel;
        [SerializeField] private LocalizedString masterVolumeLabel;
        [SerializeField] private LocalizedString musicVolumeLabel;
        [SerializeField] private LocalizedString sfxVolumeLabel;
        [SerializeField] private LocalizedString lookSensitivityLabel;
        [SerializeField] private LocalizedString languageLabel;
        [SerializeField] private LocalizedString vsyncLabel;
        [SerializeField] private LocalizedString fullscreenLabel;
        [SerializeField] private LocalizedString resolutionLabel;

        private readonly List<Locale> availableLocales = new();
        private Resolution[] availableResolutions;

        protected override void Awake()
        {
            base.Awake();
            closeButton?.onClick.AddListener(Hide);

            ApplyLabel(closeButton != null ? closeButton.GetComponentInChildren<TMP_Text>() : null, closeLabel);
            ApplyLabel(masterVolumeLabelText, masterVolumeLabel);
            ApplyLabel(musicVolumeLabelText, musicVolumeLabel);
            ApplyLabel(sfxVolumeLabelText, sfxVolumeLabel);
            ApplyLabel(lookSensitivityLabelText, lookSensitivityLabel);
            ApplyLabel(languageLabelText, languageLabel);
            ApplyLabel(vsyncLabelText, vsyncLabel);
            ApplyLabel(fullscreenLabelText, fullscreenLabel);
            ApplyLabel(resolutionLabelText, resolutionLabel);
        }

        private static void ApplyLabel(TMP_Text target, LocalizedString label)
        {
            if (target != null && !label.IsEmpty)
            {
                target.text = label.GetLocalizedString();
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
            fullscreenToggle?.onValueChanged.AddListener(HandleFullscreenChanged);
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
            fullscreenToggle?.onValueChanged.RemoveListener(HandleFullscreenChanged);
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
            fullscreenToggle?.SetIsOnWithoutNotify(SettingsManager.Instance.FullscreenEnabled);

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

        private void HandleFullscreenChanged(bool value) => SettingsManager.Instance?.SetFullscreenEnabled(value);

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
