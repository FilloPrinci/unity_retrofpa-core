using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Gives each direct child of this object a random rotation and uniform
    /// scale within the configured ranges - e.g. to make copies of the same
    /// tree or rock prefab look different. Meant to be applied in the Editor
    /// (the Inspector's buttons), so the result is saved with the scene and
    /// works with static/lightmapped children; <see cref="randomizeOnAwake"/>
    /// applies it at runtime instead. The values are absolute, not relative
    /// to the children's current ones, and derive from <see cref="seed"/>:
    /// the same seed always gives the same result.
    /// </summary>
    public class ChildTransformRandomizer : MonoBehaviour
    {
        [Tooltip("Minimum local rotation, in degrees, per axis.")]
        [SerializeField]
        private Vector3 minRotation = Vector3.zero;

        [Tooltip("Maximum local rotation, in degrees, per axis.")]
        [SerializeField]
        private Vector3 maxRotation = new Vector3(0f, 360f, 0f);

        [Tooltip("Minimum local scale, applied uniformly on all axes.")]
        [Min(0f)]
        [SerializeField]
        private float minScale = 0.8f;

        [Tooltip("Maximum local scale, applied uniformly on all axes.")]
        [Min(0f)]
        [SerializeField]
        private float maxScale = 1.2f;

        [Tooltip("Same seed, same result. The Inspector's 'New Seed' button picks a new one.")]
        [SerializeField]
        private int seed;

        [Tooltip("Also randomize when the scene starts. Static children can't be moved at runtime: for those, randomize in the Editor instead.")]
        [SerializeField]
        private bool randomizeOnAwake;

        private void Awake()
        {
            if (randomizeOnAwake)
            {
                Randomize();
            }
        }

        /// <summary>Applies a random rotation and scale, derived from the current seed, to every direct child.</summary>
        public void Randomize()
        {
            var random = new System.Random(seed);

            foreach (Transform child in transform)
            {
                child.localRotation = Quaternion.Euler(
                    Range(random, minRotation.x, maxRotation.x),
                    Range(random, minRotation.y, maxRotation.y),
                    Range(random, minRotation.z, maxRotation.z));
                child.localScale = Vector3.one * Range(random, minScale, maxScale);
            }
        }

        private static float Range(System.Random random, float min, float max)
        {
            return Mathf.Lerp(min, max, (float)random.NextDouble());
        }
    }
}
