using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Loads and unloads level scenes on top of the persistent bootstrap
    /// scene, always additively — never <see cref="LoadSceneMode.Single"/>,
    /// which would tear down the persistent scene and every manager in it.
    /// This is the Unity equivalent of the Godot template's shell
    /// (main.tscn) swapping the child of a "CurrentLevel" node: only one
    /// level scene is ever loaded at a time, and it is unloaded before the
    /// next one loads.
    /// </summary>
    public class LevelSceneManager : PersistentSingleton<LevelSceneManager>
    {
        [Tooltip("Root transform of the persistent player, moved to the " +
                 "requested SpawnPoint after each level load. Can also be " +
                 "assigned at runtime via SetPersistentPlayerRoot, e.g. if " +
                 "the player is instantiated by code instead of being " +
                 "placed in the persistent scene up front.")]
        [SerializeField]
        private Transform persistentPlayerRoot;

        /// <summary>Raised right before a level starts loading (its name).</summary>
        public static event Action<string> LevelLoadStarted;

        /// <summary>Raised once a level has finished loading and the player has been placed (its name).</summary>
        public static event Action<string> LevelLoaded;

        /// <summary>Raised right after the previous level has been unloaded (its name).</summary>
        public static event Action<string> LevelUnloaded;

        private Scene? currentLevelScene;

        /// <summary>
        /// Assigns (or reassigns) the transform that gets repositioned to a
        /// <see cref="SpawnPoint"/> on every level load.
        /// </summary>
        public void SetPersistentPlayerRoot(Transform root)
        {
            persistentPlayerRoot = root;
        }

        /// <summary>
        /// Unloads the current level (if any) and additively loads
        /// <paramref name="sceneName"/>, then places the persistent player
        /// at the matching <see cref="SpawnPoint"/>.
        /// </summary>
        /// <param name="sceneName">Name of the level scene, as it appears in Build Settings.</param>
        /// <param name="spawnPointId">
        /// Id of the <see cref="SpawnPoint"/> to spawn at. If null/empty, or
        /// if no spawn point matches, the first spawn point found in the
        /// scene is used.
        /// </param>
        public void LoadLevel(string sceneName, string spawnPointId = null)
        {
            StartCoroutine(LoadLevelRoutine(sceneName, spawnPointId));
        }

        private IEnumerator LoadLevelRoutine(string sceneName, string spawnPointId)
        {
            LevelLoadStarted?.Invoke(sceneName);

            if (currentLevelScene.HasValue && currentLevelScene.Value.IsValid())
            {
                string previousSceneName = currentLevelScene.Value.name;
                yield return UnitySceneManager.UnloadSceneAsync(currentLevelScene.Value);
                currentLevelScene = null;
                LevelUnloaded?.Invoke(previousSceneName);
            }

            yield return UnitySceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene loadedScene = UnitySceneManager.GetSceneByName(sceneName);
            currentLevelScene = loadedScene;

            // Make the level's own lighting/skybox/fog settings the active
            // ones, so a VisualStyleProfile applied by that level takes effect.
            UnitySceneManager.SetActiveScene(loadedScene);

            PlacePlayerAtSpawnPoint(loadedScene, spawnPointId);

            LevelLoaded?.Invoke(sceneName);
        }

        private void PlacePlayerAtSpawnPoint(Scene scene, string spawnPointId)
        {
            if (persistentPlayerRoot == null)
            {
                return;
            }

            SpawnPoint target = null;
            SpawnPoint[] candidates = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            foreach (SpawnPoint candidate in candidates)
            {
                if (candidate.gameObject.scene != scene)
                {
                    continue;
                }

                // Remember the first spawn point in the scene as a fallback...
                target ??= candidate;

                // ...but prefer an exact id match, if one is requested.
                if (!string.IsNullOrEmpty(spawnPointId) && candidate.Id == spawnPointId)
                {
                    target = candidate;
                    break;
                }
            }

            if (target == null)
            {
                Debug.LogWarning($"[LevelSceneManager] Scene '{scene.name}' has no SpawnPoint " +
                                  $"(requested id: '{spawnPointId}').");
                return;
            }

            if (persistentPlayerRoot.TryGetComponent(out ITeleportable teleportable))
            {
                teleportable.Teleport(target.transform.position, target.transform.rotation);
            }
            else
            {
                persistentPlayerRoot.SetPositionAndRotation(target.transform.position, target.transform.rotation);
            }
        }
    }
}
