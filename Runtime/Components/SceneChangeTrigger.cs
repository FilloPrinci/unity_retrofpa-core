using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Loads a different level when the player enters this trigger volume.
    /// Requires a Collider marked "Is Trigger". Delegates the actual
    /// load/unload to <see cref="LevelSceneManager"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SceneChangeTrigger : MonoBehaviour
    {
        [Tooltip("Name of the level scene to load, as it appears in Build Settings.")]
        [SerializeField]
        private string targetSceneName;

        [Tooltip("Id of the SpawnPoint to arrive at in the target scene. Leave empty to use the first spawn point found.")]
        [SerializeField]
        private string targetSpawnPointId;

        [Tooltip("Tag used to recognize the player. Only colliders with this tag trigger the level change.")]
        [SerializeField]
        private string playerTag = "Player";

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            LevelSceneManager.Instance.LoadLevel(targetSceneName, targetSpawnPointId);
        }
    }
}
