using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Triggers the very first level load on startup. Drop this on any
    /// GameObject in the persistent bootstrap scene (alongside
    /// <see cref="GameManager"/> and <see cref="LevelSceneManager"/>) and
    /// configure which level/spawn point to start at in the Inspector — no
    /// project-specific code required.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Tooltip("Name of the level scene to load on startup, as it appears in Build Settings.")]
        [SerializeField]
        private string startingLevelSceneName;

        [Tooltip("Id of the SpawnPoint to start at. Leave empty to use the first spawn point found in the scene.")]
        [SerializeField]
        private string startingSpawnPointId;

        private void Start()
        {
            if (string.IsNullOrEmpty(startingLevelSceneName))
            {
                Debug.LogWarning("[GameBootstrapper] No starting level scene name configured; skipping initial load.", this);
                return;
            }

            LevelSceneManager.Instance.LoadLevel(startingLevelSceneName, startingSpawnPointId);
        }
    }
}
