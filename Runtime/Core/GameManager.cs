using System;
using UnityEngine;

namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// Tracks the overall <see cref="GameState"/> and reacts to level loads
    /// reported by <see cref="LevelSceneManager"/>. Deliberately does not
    /// call into <see cref="LevelSceneManager"/> directly — it only listens
    /// to its static events, keeping the two managers decoupled the same way
    /// Godot autoloads communicate through signals rather than direct calls.
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        /// <summary>Raised whenever <see cref="CurrentState"/> changes, with (previous, current).</summary>
        public static event Action<GameState, GameState> GameStateChanged;

        public GameState CurrentState { get; private set; } = GameState.Boot;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                // Duplicate instance, already scheduled for destruction by the base class.
                return;
            }

            LevelSceneManager.LevelLoadStarted += HandleLevelLoadStarted;
            LevelSceneManager.LevelLoaded += HandleLevelLoaded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LevelSceneManager.LevelLoadStarted -= HandleLevelLoadStarted;
            LevelSceneManager.LevelLoaded -= HandleLevelLoaded;
        }

        /// <summary>
        /// Moves the game into <paramref name="newState"/> and raises
        /// <see cref="GameStateChanged"/>. Pausing/resuming also drives
        /// <see cref="Time.timeScale"/> so callers don't have to duplicate
        /// that bookkeeping.
        /// </summary>
        public void SetGameState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            GameState previous = CurrentState;
            CurrentState = newState;

            if (newState == GameState.Paused)
            {
                Time.timeScale = 0f;
            }
            else if (previous == GameState.Paused)
            {
                Time.timeScale = 1f;
            }

            GameStateChanged?.Invoke(previous, newState);
        }

        private void HandleLevelLoadStarted(string sceneName) => SetGameState(GameState.Loading);

        private void HandleLevelLoaded(string sceneName) => SetGameState(GameState.Playing);
    }
}
