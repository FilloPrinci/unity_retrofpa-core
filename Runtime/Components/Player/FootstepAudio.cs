using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Plays a footstep sound at a steady interval while the player is
    /// walking, using the sound from a <see cref="SurfaceAudio"/> on
    /// whatever's underfoot (found via a downward raycast), or
    /// <see cref="AudioProfile.DefaultFootstepSound"/> if there is none.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FootstepAudio : MonoBehaviour
    {
        [SerializeField] private float stepInterval = 0.5f;
        [Tooltip("Minimum horizontal speed to be considered walking.")]
        [SerializeField]
        private float minSpeed = 0.5f;
        [SerializeField] private float raycastDistance = 1.5f;
        [SerializeField] private LayerMask groundMask = ~0;

        private CharacterController controller;
        private float stepTimer;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 horizontalVelocity = controller.velocity;
            horizontalVelocity.y = 0f;

            if (!controller.isGrounded || horizontalVelocity.magnitude < minSpeed)
            {
                stepTimer = 0f;
                return;
            }

            stepTimer += Time.deltaTime;
            if (stepTimer < stepInterval)
            {
                return;
            }

            stepTimer = 0f;
            PlayFootstep();
        }

        private void PlayFootstep()
        {
            // No LogError-on-missing-AudioManager here unlike most other
            // components: this runs multiple times a second while walking,
            // so silently skipping is the right failure mode.
            if (AudioManager.Instance == null)
            {
                return;
            }

            AudioClip overrideClip = null;
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
            {
                SurfaceAudio surface = hit.collider.GetComponentInParent<SurfaceAudio>();
                if (surface != null)
                {
                    overrideClip = surface.FootstepSound;
                }
            }

            AudioManager.Instance.PlayFootstep(overrideClip, transform.position);
        }
    }
}
