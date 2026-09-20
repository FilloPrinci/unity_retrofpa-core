using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Optional per-surface footstep sound - drop this on a ground object
    /// (grass, metal grating, water, ...) to override the default footstep
    /// sound while the player is walking on it. <see cref="FootstepAudio"/>
    /// looks for this via a downward raycast; a surface with none just falls
    /// back to <see cref="AudioProfile.DefaultFootstepSound"/>.
    /// </summary>
    public class SurfaceAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip footstepSound;

        public AudioClip FootstepSound => footstepSound;
    }
}
