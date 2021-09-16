using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// 
    /// </summary>
    public interface ILobbyCompany {

        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        CompanyType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        string Faction { get; }

        /// <summary>
        /// 
        /// </summary>
        double Strength { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsAuto { get; }

    }

}
