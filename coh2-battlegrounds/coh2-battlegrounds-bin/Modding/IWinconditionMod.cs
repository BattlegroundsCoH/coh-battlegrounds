namespace Battlegrounds.Modding {

    /// <summary>
    /// Interface for representing a win condition mod.
    /// </summary>
    public interface IWinconditionMod : IGameMod {

        /// <summary>
        /// Get all gamemodes in the <see cref="IWinconditionMod"/> pack.
        /// </summary>
        IGamemode[] Gamemodes { get; }

    }

}
