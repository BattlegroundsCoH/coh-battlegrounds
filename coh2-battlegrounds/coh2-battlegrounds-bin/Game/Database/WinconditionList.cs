using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// A small static class-database over all relevant <see cref="Wincondition"/> instances.
    /// </summary>
    public static class WinconditionList {

        static List<IGamemode> __winconditions;

        /// <summary>
        /// Create and load the <see cref="WinconditionList"/>.
        /// </summary>
        public static void CreateDatabase()
            => __winconditions = new();

        /// <summary>
        /// Register a new <see cref="IGamemode"/> in the database.
        /// </summary>
        /// <param name="wincondition"></param>
        public static void AddWincondition(IGamemode wincondition) => __winconditions.Add(wincondition);

        /// <summary>
        /// Get a list of all valid <see cref="IGamemode"/> instances for specified <paramref name="modGuid"/>.
        /// </summary>
        /// <param name="modGuid">The targetted mod.</param>
        /// <param name="captureBaseGame">Allow vanilla gamemodes</param>
        /// <returns>A list of <see cref="IGamemode"/> instances for <paramref name="modGuid"/>.</returns>
        public static List<IGamemode> GetGamemodes(ModGuid modGuid, bool captureBaseGame = false)
            => __winconditions.Where(x => x.Guid == modGuid || (captureBaseGame && x.Guid == ModGuid.BaseGame)).ToList();

        /// <summary>
        /// Get a <see cref="IGamemode"/> by its gamemode identifier name.
        /// </summary>
        /// <param name="modGuid">The targetted mod.</param>
        /// <param name="gamemodeName">The name of the gamemode to get.</param>
        /// <returns>The found gamemode or null if not full.</returns>
        public static IGamemode GetGamemodeByName(ModGuid modGuid, string gamemodeName)
            => __winconditions.FirstOrDefault(x => x.Guid == modGuid && x.Name == gamemodeName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modGuid"></param>
        /// <param name="gamemodeNames"></param>
        /// <returns></returns>
        public static List<IGamemode> GetGamemodes(ModGuid modGuid, IEnumerable<string> gamemodeNames)
            => gamemodeNames.Select(x => GetGamemodeByName(modGuid, x)).ToList();

    }

}
