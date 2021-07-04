using System;
using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Game.DataCompany {
    
    /// <summary>
    /// Enumerated representation of recognized types that can be used to describe a <see cref="Company"/>.
    /// </summary>
    public enum CompanyType {
    
        /// <summary>
        /// No specific type is specified
        /// </summary>
        Unspecified,

        /// <summary>
        /// Heavy focus on infantry
        /// </summary>
        Infantry,

        /// <summary>
        /// Infantry and team weapons use motorized transport (Opel Blitz, Zis-6 etc.)
        /// </summary>
        Motorized,

        /// <summary>
        /// Infantry and team weapons use mechanized transport (Halftracks) and light tanks.
        /// </summary>
        Mechanized,

        /// <summary>
        /// Favours the use of heavy and medium tanks.
        /// </summary>
        Armoured,

        /// <summary>
        /// Favours anti-tank units
        /// </summary>
        TankDestroyer,

        /// <summary>
        /// Favours the use of aircraft
        /// </summary>
        Airborne,

        /// <summary>
        /// Favours the use of combat engineers - frontline fortifications
        /// </summary>
        Engineer,

        /// <summary>
        /// Favours the use of artillery
        /// </summary>
        Artillery,

    }

    /// <summary>
    /// 
    /// </summary>
    public enum CompanyAvailabilityType {

        /// <summary>
        /// 
        /// </summary>
        MultiplayerOnly,

        /// <summary>
        /// 
        /// </summary>
        CampaignOnly,

        /// <summary>
        /// 
        /// </summary>
        AnyMode,

    }

    /// <summary>
    /// 
    /// </summary>
    public static class CompanyTypeExtension {

        public static List<CompanyType> CompanyTypes => Enum.GetValues<CompanyType>().ToList();

    }

}
