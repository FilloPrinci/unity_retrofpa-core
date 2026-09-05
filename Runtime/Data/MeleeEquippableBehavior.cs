using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Melee weapon behavior (a knife, a bat, ...). Actual hit detection and
    /// damage application belong to a future combat system; this only marks
    /// the moment the swing happens.
    /// </summary>
    [CreateAssetMenu(fileName = "MeleeEquippableBehavior", menuName = "Retro FPA/Equippable/Melee")]
    public class MeleeEquippableBehavior : EquippableBehavior
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private float range = 1.5f;
        [SerializeField] private float actionsPerSecond = 1f;

        public float Damage => damage;
        public float Range => range;
        public float ActionsPerSecond => actionsPerSecond;

        public override void OnEquip(GameObject wielder) { }

        public override void OnUnequip(GameObject wielder) { }

        public override void PerformAction(GameObject wielder)
        {
            // Hook point for a future combat system (hit detection/damage).
        }
    }
}
