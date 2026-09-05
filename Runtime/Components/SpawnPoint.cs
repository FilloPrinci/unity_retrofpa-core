using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Marks a position and orientation in a level where the persistent
    /// player (or, in the future, another actor) can be placed after a level
    /// load. A level can contain several spawn points distinguished by
    /// <see cref="Id"/> (e.g. one per level-entry door).
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Tooltip("Identifier used to pick this spawn point out of several in " +
                 "the same scene. Pass it to LevelSceneManager.LoadLevel.")]
        [SerializeField]
        private string id = "Default";

        public string Id => id;

        private void OnDrawGizmos()
        {
            const float radius = 0.3f;
            const float forwardLength = 0.75f;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * forwardLength);
        }
    }
}
