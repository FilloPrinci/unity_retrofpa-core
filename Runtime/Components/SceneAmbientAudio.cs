using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Drop one of these in a level scene (alongside its <see cref="SpawnPoint"/>
    /// and <see cref="SceneAtmosphere"/>) and assign an ambient track to give
    /// that level its own looping music/soundscape, independent of every
    /// other level - the audio equivalent of <see cref="SceneAtmosphere"/>.
    /// Applies on <see cref="LevelSceneManager.LevelLoaded"/> (not Awake/
    /// OnEnable/Start), matching <see cref="SceneAtmosphere"/>'s reasoning:
    /// <see cref="AudioManager"/> must exist first, which Unity only
    /// guarantees once the whole scene's Awake/OnEnable have run.
    /// </summary>
    public class SceneAmbientAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip ambientTrack;

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
            if (gameObject.scene.name != sceneName)
            {
                return;
            }

            if (AudioManager.Instance == null)
            {
                Debug.LogError("[SceneAmbientAudio] No AudioManager in the scene.", this);
                return;
            }

            AudioManager.Instance.PlaySceneAmbient(ambientTrack);
        }
    }
}
