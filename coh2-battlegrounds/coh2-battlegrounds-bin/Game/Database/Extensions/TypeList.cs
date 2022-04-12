using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;

namespace Battlegrounds.Game.Database.Extensions {

    public class TypeList : IEnumerable<string> {

        private HashSet<string> m_types;

        private bool m_isHeavyArtyillery;
        private bool m_isAT;
        private bool m_isInfantry;
        private bool m_isVehicle;
        private bool m_isArmour;
        private bool m_isHeavyArmour;
        private bool m_isCrew;
        private bool m_isSpecialInfantry;
        private bool m_isOfficer;
        private bool m_isCommandUnit;
        private bool m_isArtillery;
        private bool m_isSniper;
        private bool m_isTransport;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a heavy artillery piece.
        /// </summary>
        [JsonIgnore]
        public bool IsHeavyArtillery => this.m_isHeavyArtyillery;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered an anti-tank gun.
        /// </summary>
        [JsonIgnore] public bool IsAntiTank => this.m_isAT;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered infantry.
        /// </summary>
        [JsonIgnore] public bool IsInfantry => this.m_isInfantry;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a vehicle (not a tank).
        /// </summary>
        [JsonIgnore]
        public bool IsVehicle => this.m_isVehicle;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a tank.
        /// </summary>
        [JsonIgnore] public bool IsArmour => this.m_isArmour;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a heavy tank.
        /// </summary>
        [JsonIgnore] public bool IsHeavyArmour => this.m_isHeavyArmour;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a vehicle crew.
        /// </summary>
        [JsonIgnore] public bool IsVehicleCrew => this.m_isCrew;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a special (elite) infantry.
        /// </summary>
        [JsonIgnore]
        public bool IsSpecialInfantry => this.m_isSpecialInfantry;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be an officer unit.
        /// </summary>
        [JsonIgnore] public bool IsOfficer => this.m_isOfficer;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be a command unit.
        /// </summary>
        [JsonIgnore] public bool IsCommandUnit => this.m_isCommandUnit;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be artillery.
        /// </summary>
        [JsonIgnore] public bool IsArtillery => this.m_isArtillery;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a sniper unit.
        /// </summary>
        [JsonIgnore] public bool IsSniper => this.m_isSniper;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a transport unit.
        /// </summary>
        [JsonIgnore] public bool IsTransportVehicle => this.m_isTransport;

        public TypeList(string[] source, bool isTeamWeapon) {

            // Set self data
            this.m_types = new(source);

            // Define property values
            this.m_isHeavyArtyillery = source.ContainsWithout("team_weapon",
                "wg_team_weapons", "mortar", "hmg"); // 'wg_team_weapons' is to block the raketenwerfer be considered a heavy artillery piece
            this.m_isAT = source.Contains("at_gun");
            this.m_isInfantry = source.Contains("infantry") && !isTeamWeapon;
            this.m_isHeavyArmour = source.Contains("heavy_tank");
            this.m_isArmour = source.ContainsWithout("vehicle", "light_vehicle") && !this.m_isHeavyArmour;
            this.m_isVehicle = ((!this.m_isArmour && source.Contains("vehicle"))
                || source.Contains("light_vehicle")) && !source.Contains("250_mortar_halftrack"); // Remove the change of mortar vehicles in this category
            this.m_isCrew = source.Contains("aef_vehicle_crew");
            this.m_isSpecialInfantry = source.Contains("guard_troops") || source.Contains("shock_troops") || source.Contains("stormtrooper");
            this.m_isOfficer = source.Contains("sov_officer");
            this.m_isCommandUnit = this.m_isOfficer || source.Contains("command_panzer");
            this.m_isArtillery = source.Contains("artillery");
            this.m_isSniper = source.Contains("sniper_soviet") || source.Contains("sniper_german");
            this.m_isTransport = source.Contains("m5_halftrack") || source.Contains("m3a1_scout_car") || source.Contains("251_halftrack");

        }

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)this.m_types).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_types).GetEnumerator();

    }

}
