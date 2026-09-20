using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Loads a different level when this object is interacted with (via the
    /// required <see cref="Interactable"/>) - e.g. a door, a ladder, an exit
    /// sign. For a level change triggered by walking into a volume instead
    /// (no interact button press), use <see cref="SceneChangeTrigger"/>.
    /// Delegates the actual load/unload to <see cref="LevelSceneManager"/>.
    /// </summary>
    [RequireComponent(typeof(Interactable))]
    public class InteractableSceneChangeTrigger : MonoBehaviour
    {
        [Tooltip("Name of the level scene to load, as it appears in Build Settings.")]
        [SerializeField]
        private string targetSceneName;

        [Tooltip("Id of the SpawnPoint to arrive at in the target scene. Leave empty to use the first spawn point found.")]
        [SerializeField]
        private string targetSpawnPointId;

        private Interactable interactable;

        private void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void OnEnable()
        {
            interactable.Interacted += HandleInteracted;
        }

        private void OnDisable()
        {
            interactable.Interacted -= HandleInteracted;
        }

        private void HandleInteracted(GameObject interactor)
        {
            if (LevelSceneManager.Instance == null)
            {
                Debug.LogError("[InteractableSceneChangeTrigger] No LevelSceneManager in the scene.", this);
                return;
            }

            LevelSceneManager.Instance.LoadLevel(targetSceneName, targetSpawnPointId);
        }
    }
}
