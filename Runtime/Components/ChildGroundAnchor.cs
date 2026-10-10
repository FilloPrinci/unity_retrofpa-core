using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Drops each direct child of this object straight down (world -Y) onto
    /// the first collider below it - e.g. to rest trees or rocks on uneven
    /// ground. A child's own colliders, and those of its siblings, are
    /// ignored, as are triggers; a child with nothing below it stays where
    /// it is. Meant to be applied in the Editor (the Inspector's button), so
    /// the result is saved with the scene and works with static children;
    /// <see cref="anchorOnStart"/> applies it at runtime instead.
    /// </summary>
    public class ChildGroundAnchor : MonoBehaviour
    {
        [Tooltip("Which layers count as ground.")]
        [SerializeField]
        private LayerMask groundMask = ~0;

        [Tooltip("How far below a child to look for ground.")]
        [Min(0f)]
        [SerializeField]
        private float maxDistance = 100f;

        [Tooltip("Start looking this far above each child, so one sitting slightly below the ground is lifted onto it instead of falling through. Anything within this height above a child counts as ground too.")]
        [Min(0f)]
        [SerializeField]
        private float startHeight;

        [Tooltip("Added to the height found - e.g. slightly negative to sink a trunk into the ground.")]
        [SerializeField]
        private float verticalOffset;

        [Tooltip("Also anchor when the scene starts. Static children can't be moved at runtime: for those, anchor in the Editor instead.")]
        [SerializeField]
        private bool anchorOnStart;

        private readonly RaycastHit[] hits = new RaycastHit[32];

        private void Start()
        {
            if (anchorOnStart)
            {
                Anchor();
            }
        }

        /// <summary>Drops every direct child onto the ground below it.</summary>
        /// <returns>How many children found no ground and were left in place.</returns>
        public int Anchor()
        {
            // Colliders moved since the last physics step (e.g. in the
            // Editor, where there is none) would otherwise be missed.
            Physics.SyncTransforms();
            PhysicsScene physicsScene = gameObject.scene.GetPhysicsScene();
            int missed = 0;

            foreach (Transform child in transform)
            {
                Vector3 origin = child.position + Vector3.up * startHeight;
                int count = physicsScene.Raycast(origin, Vector3.down, hits, startHeight + maxDistance, groundMask, QueryTriggerInteraction.Ignore);

                if (TryGetGround(count, out RaycastHit ground))
                {
                    child.position = ground.point + Vector3.up * verticalOffset;
                }
                else
                {
                    missed++;
                }
            }

            return missed;
        }

        // The nearest hit that isn't one of the children themselves.
        private bool TryGetGround(int count, out RaycastHit ground)
        {
            ground = default;
            bool found = false;

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.transform.IsChildOf(transform) || (found && hit.distance >= ground.distance))
                {
                    continue;
                }

                ground = hit;
                found = true;
            }

            return found;
        }
    }
}
