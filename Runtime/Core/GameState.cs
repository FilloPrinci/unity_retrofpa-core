namespace FilloPrinci.RetroFpa
{
    /// <summary>
    /// High-level state of the running game, driven by <see cref="GameManager"/>.
    /// </summary>
    public enum GameState
    {
        /// <summary>Application has just started; no level has loaded yet.</summary>
        Boot,

        /// <summary>A level is being loaded or unloaded additively.</summary>
        Loading,

        /// <summary>A level is loaded and the game is running normally.</summary>
        Playing,

        /// <summary>The game is paused (time is stopped).</summary>
        Paused
    }
}
