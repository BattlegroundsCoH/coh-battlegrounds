using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Enum representing a team a <see cref="ISessionParticipant"/> can be on.
    /// </summary>
    public enum ParticipantTeam : byte {

        /// <summary>
        /// Represents the allies team (Soviets, USF, UKF).
        /// </summary>
        TEAM_ALLIES,

        /// <summary>
        /// Represents the axis team (OKW, Ostheer).
        /// </summary>
        TEAM_AXIS,

    }

    /// <summary>
    /// 
    /// </summary>
    public interface ISessionParticipant {

        /// <summary>
        /// 
        /// </summary>
        string UserDisplayname { get; }

        /// <summary>
        /// 
        /// </summary>
        ulong UserID { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsHuman { get; }
        
        /// <summary>
        /// 
        /// </summary>
        AIDifficulty Difficulty { get; }

        /// <summary>
        /// 
        /// </summary>
        ParticipantTeam TeamIndex { get; }

        /// <summary>
        /// 
        /// </summary>
        byte PlayerIndexOnTeam { get; }

        /// <summary>
        /// 
        /// </summary>
        byte PlayerIngameIndex { get; }
        
        /// <summary>
        /// 
        /// </summary>
        Company SelectedCompany { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetName();
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ulong GetID();

    }

}
