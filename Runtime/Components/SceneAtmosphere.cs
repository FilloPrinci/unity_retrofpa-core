using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Drop one of these in a level scene (alongside its <see cref="SpawnPoint"/>)
    /// and assign a <see cref="SceneAtmosphereProfile"/> to give that level its
    /// own fog/skybox, independent of every other level.
    /// </summary>
    /// <remarks>
    /// Applies itself on <see cref="LevelSceneManager.LevelLoaded"/>, not
    /// Awake/OnEnable/Start: <see cref="LevelSceneManager"/> calls
    /// <c>SceneManager.SetActiveScene</c> right after this scene finishes
    /// loading (needed so newly spawned objects default to this scene), and
    /// that call resets RenderSettings (fog, skybox) to whatever this scene
    /// had saved - which would silently undo an apply made any earlier in
    /// the load, including from this component's own Awake/OnEnable/Start.
    /// LevelLoaded only fires once that reset has already happened, so
    /// reacting to it is the only ordering that sticks. Subscribing to a
    /// static event in OnEnable is always safe (no dependency on another
    /// object's Awake having run first) - unlike calling into another
    /// singleton's instance directly, which is why this one isn't the same
    /// class of bug fixed on SettingsUIController/Start().
    /// </remarks>
    public class SceneAtmosphere : MonoBehaviour
    {
        [SerializeField] private SceneAtmosphereProfile profile;

        private void OnEnable()
        {
            LevelSceneManager.LevelLoaded += HandleLevelLoaded;
        }

        private void OnDisable()
        {
            LevelSceneManager.LevelLoaded -= HandleLevelLoaded;
        }

        private void HandleLevelLoaded(string sceneName)
        {
            // Only react to our own scene finishing its load - a
            // SceneAtmosphere belonging to a scene that isn't the one that
            // just loaded has nothing to do here.
            if (gameObject.scene.name != sceneName)
            {
                return;
            }

            Apply();
        }

        private void Apply()
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
