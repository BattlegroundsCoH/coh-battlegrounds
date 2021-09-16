using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// 
    /// </summary>
    public interface ILobbyAIParticipant : ILobbyParticipant {

        /// <summary>
        /// 
        /// </summary>
        AIDifficulty AIDifficulty { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        bool SetDifficulty(AIDifficulty difficulty);

    }

}
