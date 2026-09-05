using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>Continuously rotates this object, e.g. a spinning collectible or key.</summary>
    public class Rotator : MonoBehaviour
    {
        [Tooltip("Rotation speed in degrees/second around each local axis.")]
        [SerializeField]
        private Vector3 degreesPerSecond = new Vector3(0f, 90f, 0f);

        private void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}
