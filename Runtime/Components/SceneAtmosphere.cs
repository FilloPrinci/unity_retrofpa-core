using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Drop one of these in a level scene (alongside its <see cref="SpawnPoint"/>)
    /// and assign a <see cref="SceneAtmosphereProfile"/> to give that level its
    /// own fog/skybox, independent of every other level. Unlike the global
    /// <see cref="VisualStyleProfile"/> (applied once and kept across level
    /// loads by <see cref="StyleManager"/>), this re-applies every time the
    /// scene it lives in loads, since fog/skybox are per-scene RenderSettings
    /// that a fresh scene load always resets.
    /// </summary>
    public class SceneAtmosphere : MonoBehaviour
    {
        [SerializeField] private SceneAtmosphereProfile profile;

        // Not OnEnable/Awake: StyleManager must exist (its Awake must have
        // run) before this can apply anything through it, and Unity only
        // guarantees that by Start() - see SettingsUIController for the same
        // reasoning applied to a UI screen depending on SettingsManager.
        private void Start()
        {
            if (profile == null)
            {
                Debug.LogWarning("[SceneAtmosphere] No SceneAtmosphereProfile assigned.", this);
                return;
            }

            if (StyleManager.Instance == null)
            {
                Debug.LogError("[SceneAtmosphere] No StyleManager in the scene.", this);
                return;
            }

            StyleManager.Instance.ApplySceneAtmosphere(profile);
        }
    }
}
