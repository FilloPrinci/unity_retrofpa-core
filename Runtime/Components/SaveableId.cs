using System;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// A stable identity for one specific instance in a level, independent of
    /// Unity's own instance IDs (which are not stable across sessions).
    /// Stack this alongside <see cref="Collectible"/> on a world pickup that
    /// should stay collected after a save is loaded - <see cref="SaveManager"/>
    /// records which ids were already collected and deactivates the matching
    /// instances when their scene loads again.
    /// </summary>
    public class SaveableId : MonoBehaviour
    {
        [Tooltip("Stable id for this specific instance. Auto-filled with a new GUID " +
                 "when the component is first added - regenerate manually (via the " +
                 "context menu) only if you copy-pasted an object and need it to be " +
                 "tracked separately from the original.")]
        [SerializeField]
        private string id;

        public string Id => id;

        private void Reset()
        {
            GenerateId();
        }

        [ContextMenu("Generate New Id")]
        private void GenerateId()
        {
            id = Guid.NewGuid().ToString("N");
        }
    }
}
