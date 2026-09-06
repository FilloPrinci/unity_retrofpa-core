using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Loads the first level, either automatically on startup or on demand
    /// via <see cref="StartGame"/> (e.g. from a main menu's "New Game"
    /// button). Drop this on any GameObject in the persistent bootstrap
    /// scene (alongside <see cref="GameManager"/> and <see cref="LevelSceneManager"/>)
    /// and configure which level/spawn point to start at in the Inspector —
    /// no project-specific code required.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Tooltip("Name of the level scene to load, as it appears in Build Settings.")]
        [SerializeField]
        private string startingLevelSceneName;

        [Tooltip("Id of the SpawnPoint to start at. Leave empty to use the first spawn point found in the scene.")]
        [SerializeField]
        private string startingSpawnPointId;

        [Tooltip("If true, loads the starting level immediately on startup. Turn off when a main menu should gate the load instead (call StartGame from its \"New Game\" button).")]
        [SerializeField]
        private bool autoStartOnAwake = true;

        private void Start()
        {
            if (autoStartOnAwake)
            {
                StartGame();
            }
        }

        /// <summary>Loads the configured starting level. Safe to call more than once (e.g. from a UI button).</summary>
        public void StartGame()
        {
            if (string.IsNullOrEmpty(startingLevelSceneName))
            {
                Debug.LogWarning("[GameBootstrapper] No starting level scene name configured; skipping initial load.", this);
                return;
            }

            if (LevelSceneManager.Instance == null)
            {
                Debug.LogError("[GameBootstrapper] No LevelSceneManager in the scene.", this);
                return;
            }

            LevelSceneManager.Instance.LoadLevel(startingLevelSceneName, startingSpawnPointId);
        }
    }
}
