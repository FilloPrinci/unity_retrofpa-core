using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Behavior for a passive equippable item that isn't a weapon (a key, a
    /// tool, a quest item, ...): equipping it just makes it "held"/shown
    /// (e.g. for a puzzle, or to preview it), with no attack/fire action.
    /// </summary>
    [CreateAssetMenu(fileName = "HeldItemEquippableBehavior", menuName = "Retro FPA/Equippable/Held Item")]
    public class HeldItemEquippableBehavior : EquippableBehavior
    {
        public override void OnEquip(GameObject wielder) { }

        public override void OnUnequip(GameObject wielder) { }

        public override void PerformAction(GameObject wielder)
        {
            // Intentionally empty: a held item has no primary action.
        }
    }
}
