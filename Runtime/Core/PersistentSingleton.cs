using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Base class for a MonoBehaviour singleton that survives additive scene
    /// loads/unloads for the lifetime of the application. This is the Unity
    /// equivalent of a Godot autoload singleton: place one instance in the
    /// persistent bootstrap scene and it will outlive every level scene
    /// loaded on top of it.
    /// </summary>
    /// <typeparam name="T">The concrete singleton type.</typeparam>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
    {
        /// <summary>
        /// The active instance, or null if none has been created yet (or it
        /// has since been destroyed).
        /// </summary>
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // A persistent instance already exists (e.g. this scene was
                // loaded additively on top of the bootstrap scene by mistake).
                // Keep the original and discard this duplicate.
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
