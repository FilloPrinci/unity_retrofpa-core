using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Ranged weapon behavior (a pistol, a bow, ...). Actual projectile
    /// spawning/hit detection belong to a future combat system; this only
    /// marks the moment the weapon fires.
    /// </summary>
    [CreateAssetMenu(fileName = "RangedEquippableBehavior", menuName = "Retro FPA/Equippable/Ranged")]
    public class RangedEquippableBehavior : EquippableBehavior
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private float actionsPerSecond = 2f;
        [SerializeField] private GameObject projectilePrefab;

        public float Damage => damage;
        public float ActionsPerSecond => actionsPerSecond;
        public GameObject ProjectilePrefab => projectilePrefab;

        public override void OnEquip(GameObject wielder) { }

        public override void OnUnequip(GameObject wielder) { }

        public override void PerformAction(GameObject wielder)
        {
            // Hook point for a future combat system (projectile spawning/hit detection).
        }
    }
}
