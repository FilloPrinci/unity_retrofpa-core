using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Data-driven description of one level's atmosphere: fog and skybox.
    /// Unlike <see cref="VisualStyleProfile"/> (the game's overall rendering
    /// style - color grading, bloom, tonemapping, texture filtering - which
    /// applies globally to every scene), fog and skybox are inherently
    /// per-level: two rooms in the same game can want completely different
    /// fog color/density or sky. Applied by <see cref="StyleManager"/> via a
    /// <see cref="SceneAtmosphere"/> component placed in the level scene.
    /// </summary>
    [CreateAssetMenu(fileName = "SceneAtmosphereProfile", menuName = "Retro FPA/Scene Atmosphere Profile")]
    public class SceneAtmosphereProfile : ScriptableObject
    {
        [Header("Fog")]
        [SerializeField] private bool fogEnabled = true;
        [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
        [SerializeField] private Color fogColor = Color.gray;
        [SerializeField] private float fogDensity = 0.02f;
        [SerializeField] private float fogStartDistance;
        [SerializeField] private float fogEndDistance = 100f;

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

        /// <summary>Writes fog settings into the active scene's global RenderSettings.</summary>
        public void ApplyFog()
        {
            RenderSettings.fog = fogEnabled;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
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
