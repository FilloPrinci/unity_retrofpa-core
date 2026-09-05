using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Defines how an equippable item behaves once wielded: what happens on
    /// equip/unequip, and what its primary action does. Concrete subclasses
    /// (<see cref="MeleeEquippableBehavior"/>, <see cref="RangedEquippableBehavior"/>)
    /// implement the actual gameplay; hit detection/damage is left to a
    /// future combat system, kept out of scope here.
    /// </summary>
    public abstract class EquippableBehavior : ScriptableObject
    {
        /// <summary>Called once when <paramref name="wielder"/> equips the item using this behavior.</summary>
        public abstract void OnEquip(GameObject wielder);

        /// <summary>Called once when <paramref name="wielder"/> unequips the item.</summary>
        public abstract void OnUnequip(GameObject wielder);

        /// <summary>Called when <paramref name="wielder"/> performs the item's primary action (attack/fire/use).</summary>
        public abstract void PerformAction(GameObject wielder);
    }
}
