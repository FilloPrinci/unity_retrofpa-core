using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Data-driven description of a level's overall look: fog, ambient
    /// light, and post-processing (color adjustments, bloom, tonemapping).
    /// Applied at runtime by <see cref="StyleManager"/>. This is the Unity
    /// equivalent of the Godot template's per-level Environment resource.
    /// </summary>
    [CreateAssetMenu(fileName = "VisualStyleProfile", menuName = "Retro FPA/Visual Style Profile")]
    public class VisualStyleProfile : ScriptableObject
    {
        [Header("Fog")]
        [SerializeField] private bool fogEnabled = true;
        [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
        [SerializeField] private Color fogColor = Color.gray;
        [SerializeField] private float fogDensity = 0.02f;
        [SerializeField] private float fogStartDistance;
        [SerializeField] private float fogEndDistance = 100f;

        [Header("Ambient Light")]
        [SerializeField] private AmbientMode ambientMode = AmbientMode.Flat;
        [SerializeField] private Color ambientLight = Color.gray;

        [Header("Color Adjustments")]
        [SerializeField] private bool colorAdjustmentsEnabled = true;
        [SerializeField] private float postExposure;
        [SerializeField] private float contrast;
        [SerializeField] private Color colorFilter = Color.white;
        [SerializeField] private float hueShift;
        [SerializeField] private float saturation;

        [Header("Bloom (used as the template's \"glow\")")]
        [SerializeField] private bool bloomEnabled = true;
        [SerializeField] private float bloomThreshold = 1f;
        [SerializeField] private float bloomIntensity = 0.3f;
        [SerializeField] private float bloomScatter = 0.7f;
        [SerializeField] private Color bloomTint = Color.white;

        [Header("Tonemapping")]
        [SerializeField] private bool tonemappingEnabled = true;
        [SerializeField] private TonemappingMode tonemappingMode = TonemappingMode.Neutral;

        /// <summary>Writes fog and ambient light settings into the active scene's global RenderSettings.</summary>
        public void ApplyFogAndAmbient()
        {
            RenderSettings.fog = fogEnabled;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;

            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientLight;
        }

        /// <summary>
        /// Writes color adjustments, bloom, and tonemapping into
        /// <paramref name="volumeProfile"/>'s overrides, adding an override
        /// component if the profile doesn't already have one.
        /// </summary>
        public void ApplyToVolumeProfile(VolumeProfile volumeProfile)
        {
            if (volumeProfile == null)
            {
                return;
            }

            ApplyColorAdjustments(volumeProfile);
            ApplyBloom(volumeProfile);
            ApplyTonemapping(volumeProfile);
        }

        private void ApplyColorAdjustments(VolumeProfile volumeProfile)
        {
            if (!volumeProfile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments = volumeProfile.Add<ColorAdjustments>(true);
            }

            colorAdjustments.active = colorAdjustmentsEnabled;
            SetOverride(colorAdjustments.postExposure, postExposure);
            SetOverride(colorAdjustments.contrast, contrast);
            SetOverride(colorAdjustments.colorFilter, colorFilter);
            SetOverride(colorAdjustments.hueShift, hueShift);
            SetOverride(colorAdjustments.saturation, saturation);
        }

        private void ApplyBloom(VolumeProfile volumeProfile)
        {
            if (!volumeProfile.TryGet(out Bloom bloom))
            {
                bloom = volumeProfile.Add<Bloom>(true);
            }

            bloom.active = bloomEnabled;
            SetOverride(bloom.threshold, bloomThreshold);
            SetOverride(bloom.intensity, bloomIntensity);
            SetOverride(bloom.scatter, bloomScatter);
            SetOverride(bloom.tint, bloomTint);
        }

        private void ApplyTonemapping(VolumeProfile volumeProfile)
        {
            if (!volumeProfile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping = volumeProfile.Add<Tonemapping>(true);
            }

            tonemapping.active = tonemappingEnabled;
            SetOverride(tonemapping.mode, tonemappingMode);
        }

        private static void SetOverride<T>(VolumeParameter<T> parameter, T value)
        {
            parameter.overrideState = true;
            parameter.value = value;
        }
    }
}
