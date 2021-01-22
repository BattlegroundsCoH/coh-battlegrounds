namespace Battlegrounds.Modding {
    
    /// <summary>
    /// Basic interface for mods used by the game.
    /// </summary>
    public interface IGameMod {
    
        /// <summary>
        /// Get the <see cref="ModGuid"/> used to identify the mod.
        /// </summary>
        public ModGuid Guid { get; }

        /// <summary>
        /// Get the display name of the mod.
        /// </summary>
        public string Name { get; }

    }

}
