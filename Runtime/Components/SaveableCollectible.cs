using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Bridges a world <see cref="Collectible"/> to <see cref="SaveManager"/>:
    /// once collected, tells it this <see cref="SaveableId"/> is done, so it
    /// stays collected (deactivated) if this level loads again, whether from
    /// a save file or just from walking back into it later in the same
    /// session. Kept separate from <see cref="Collectible"/> for the same
    /// reason <see cref="CollectibleItem"/> is - so that component stays
    /// usable without knowing about the save system at all.
    /// </summary>
    [RequireComponent(typeof(Collectible))]
    [RequireComponent(typeof(SaveableId))]
    public class SaveableCollectible : MonoBehaviour
    {
        private Collectible collectible;
        private SaveableId saveableId;

        private void Awake()
        {
            collectible = GetComponent<Collectible>();
            saveableId = GetComponent<SaveableId>();
        }

        private void OnEnable()
        {
            collectible.Collected += HandleCollected;
        }

        private void OnDisable()
        {
            collectible.Collected -= HandleCollected;
        }

        private void HandleCollected(GameObject collector)
        {
            if (SaveManager.Instance == null)
            {
                Debug.LogError("[SaveableCollectible] No SaveManager in the scene.", this);
                return;
            }

            SaveManager.Instance.MarkCollected(saveableId.Id);
        }
    }
}
