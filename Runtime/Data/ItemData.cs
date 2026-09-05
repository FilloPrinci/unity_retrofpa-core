using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Data describing one kind of item (a key, a note, a weapon, ...).
    /// Equippable items additionally reference an <see cref="EquippableBehavior"/>
    /// — non-equippable items (keys, notes, quest items) simply leave it null.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "Retro FPA/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Tooltip("Stable unique id, e.g. for save games. Not the same as the asset name.")]
        [SerializeField]
        private string itemId;

        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        [Tooltip("The world prefab spawned when this item is dropped (typically a WorldItemBase_Pickupable variant).")]
        [SerializeField]
        private GameObject worldPrefab;

        [SerializeField, Min(1)] private int maxStackSize = 1;

        [Tooltip("Leave empty for a non-equippable item (key, note, quest item).")]
        [SerializeField]
        private EquippableBehavior equippableBehavior;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject WorldPrefab => worldPrefab;
        public int MaxStackSize => maxStackSize;
        public EquippableBehavior EquippableBehavior => equippableBehavior;
        public bool IsEquippable => equippableBehavior != null;
    }
}
