using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Data-driven description of the game's overall rendering style:
    /// ambient light and post-processing (color adjustments, bloom,
    /// tonemapping, texture filtering). Applied at runtime by
    /// <see cref="StyleManager"/> and kept across every level load, since
    /// this is meant to be the same "look" for the whole game. Per-level
    /// atmosphere (fog, skybox) is intentionally NOT here - see
    /// <see cref="SceneAtmosphereProfile"/> - because two levels in the same
    /// game can reasonably want completely different fog/sky.
    /// </summary>
    [CreateAssetMenu(fileName = "VisualStyleProfile", menuName = "Retro FPA/Visual Style Profile")]
    public class VisualStyleProfile : ScriptableObject
    {
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

        [Header("Texture Filtering")]
        [Tooltip("Point = crisp/pixelated (PS1-style). Bilinear/Trilinear = smoothed (N64-style).")]
        [SerializeField] private FilterMode textureFilterMode = FilterMode.Point;
        [Tooltip("If true, StyleManager also applies textureFilterMode to UI textures " +
                 "(icons, TMP font atlases). If false (default), only textures used by " +
                 "Renderers in the currently loaded scenes are affected - the UI keeps " +
                 "whichever filtering it was imported with.")]
        [SerializeField] private bool applyToUITextures;

        /// <summary>Point (PS1-style) or Bilinear/Trilinear (N64-style) texture filtering.</summary>
        public FilterMode TextureFilterMode => textureFilterMode;

        /// <summary>Whether <see cref="TextureFilterMode"/> should also apply to UI textures.</summary>
        public bool ApplyToUITextures => applyToUITextures;

        /// <summary>Writes ambient light settings into the active scene's global RenderSettings.</summary>
        public void ApplyAmbient()
        {
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
