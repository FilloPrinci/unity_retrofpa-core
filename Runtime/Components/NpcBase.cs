using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Base component for NPC prefabs. The Animator's Controller is meant to
    /// be a shared base AnimatorController defining logical states with
    /// placeholder clips; each character prefab variant assigns its own
    /// <see cref="AnimatorOverrideController"/> here to remap those states
    /// onto its real clips (Unity's native equivalent of the Godot
    /// template's AnimationSet — no custom ScriptableObject needed).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class NpcBase : MonoBehaviour
    {
        [Tooltip("Assign to remap the base Animator Controller's placeholder clips onto this character's real clips.")]
        [SerializeField]
        private AnimatorOverrideController animatorOverride;

        public Animator Animator { get; private set; }

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();

            if (animatorOverride != null)
            {
                Animator.runtimeAnimatorController = animatorOverride;
            }
        }
    }
}
