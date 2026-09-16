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

        [Header("Texture Filtering")]
        [Tooltip("Point = crisp/pixelated (PS1-style). Bilinear/Trilinear = smoothed (N64-style).")]
        [SerializeField] private FilterMode textureFilterMode = FilterMode.Point;
        [Tooltip("If true, StyleManager also applies textureFilterMode to UI textures " +
                 "(icons, TMP font atlases). If false (default), only textures used by " +
                 "Renderers in the currently loaded scenes are affected - the UI keeps " +
                 "whichever filtering it was imported with.")]
        [SerializeField] private bool applyToUITextures;

        [Header("Skybox (2-color gradient, e.g. Retro FPA/Gradient Skybox)")]
        [SerializeField] private bool skyboxEnabled = true;
        [Tooltip("Color at/below the horizon. Set close to fogColor so geometry fades into the sky at the horizon instead of cutting against a mismatched skybox.")]
        [SerializeField] private Color skyboxHorizonColor = Color.gray;
        [Tooltip("Color straight up at the zenith.")]
        [SerializeField] private Color skyboxZenithColor = Color.gray;
        [Tooltip("How quickly the gradient shifts from horizon to zenith color as you look up. " +
                 "1 = linear by height; higher keeps the horizon color longer; lower reaches the zenith color sooner.")]
        [SerializeField] private float skyboxCurve = 1f;
        [SerializeField] private float skyboxExposure = 1f;

        private static readonly int HorizonColorId = Shader.PropertyToID("_HorizonColor");
        private static readonly int ZenithColorId = Shader.PropertyToID("_ZenithColor");
        private static readonly int CurveId = Shader.PropertyToID("_Curve");
        private static readonly int ExposureId = Shader.PropertyToID("_Exposure");

        /// <summary>
        /// Whether this profile wants to drive the skybox at all. When
        /// false, <see cref="StyleManager"/> leaves whatever skybox
        /// (RenderSettings.skybox) was already active untouched, so a
        /// level can keep a bespoke skybox instead.
        /// </summary>
        public bool SkyboxEnabled => skyboxEnabled;

        /// <summary>Point (PS1-style) or Bilinear/Trilinear (N64-style) texture filtering.</summary>
        public FilterMode TextureFilterMode => textureFilterMode;

        /// <summary>Whether <see cref="TextureFilterMode"/> should also apply to UI textures.</summary>
        public bool ApplyToUITextures => applyToUITextures;

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

        /// <summary>
        /// Writes this profile's horizon/zenith colors onto
        /// <paramref name="skyboxMaterial"/> (a gradient-skybox instance
        /// owned by <see cref="StyleManager"/>). No-op if
        /// <see cref="skyboxEnabled"/> is false or the material is null.
        /// </summary>
        public void ApplySkybox(Material skyboxMaterial)
        {
            if (!skyboxEnabled || skyboxMaterial == null)
            {
                return;
            }

            skyboxMaterial.SetColor(HorizonColorId, skyboxHorizonColor);
            skyboxMaterial.SetColor(ZenithColorId, skyboxZenithColor);
            skyboxMaterial.SetFloat(CurveId, skyboxCurve);
            skyboxMaterial.SetFloat(ExposureId, skyboxExposure);
        }
    }
}
