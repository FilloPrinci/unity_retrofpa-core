using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Implemented by movement controllers that need special handling when
    /// teleported (e.g. resetting accumulated velocity on a
    /// <see cref="CharacterController"/>) instead of a plain Transform move.
    /// <see cref="LevelSceneManager"/> checks for this on the persistent
    /// player root before falling back to a direct Transform move.
    /// </summary>
    public interface ITeleportable
    {
        void Teleport(Vector3 position, Quaternion rotation);
    }
}
