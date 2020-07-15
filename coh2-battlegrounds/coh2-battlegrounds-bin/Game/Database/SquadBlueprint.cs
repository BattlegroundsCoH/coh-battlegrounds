using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// Representation of a <see cref="Blueprint"/> with <see cref="Squad"/> specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
    /// </summary>
    public sealed class SquadBlueprint : Blueprint {
    
        /// <summary>
        /// The UI symbol used in-game to show unit type
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The UI icon of the squad
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The army the <see cref="SquadBlueprint"/> can be used by.
        /// </summary>
        public string Army { get; set; }

        /// <summary>
        /// The localized string ID for the display name of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public string LocaleName { get; set; }

        /// <summary>
        /// The localized string ID for the description of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public string LocaleDescription { get; set; }

        /// <summary>
        /// The base <see cref="Gameplay.Cost"/> to field instances of the <see cref="SquadBlueprint"/>.
        /// </summary>
        public Cost Cost { get; set; }

        /// <summary>
        /// Does the squad the bluperint is for, require a crew.
        /// </summary>
        public bool HasCrew { get; set; }

        /// <summary>
        /// Array of types bound to the <see cref="SquadBlueprint"/>.
        /// </summary>
        public HashSet<string> Types { get; set; }

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a heavy artillery piece.
        /// </summary>
        [JsonIgnore] public bool IsHeavyArtillery 
            => this.Types.ToArray().ContainsWithout("team_weapon", "wg_team_weapons", "mortar", "hmg"); // 'wg_team_weapons' is to block the raketenwerfer be considered a heavy artillery piece

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered an anti-tank gun.
        /// </summary>
        [JsonIgnore] public bool IsAntiTank => this.Types.Contains("at_gun");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered infantry.
        /// </summary>
        [JsonIgnore] public bool IsInfantry => this.Types.Contains("infantry") && !IsTeamWeapon;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a team weapon.
        /// </summary>
        [JsonIgnore] public bool IsTeamWeapon => this.Types.Contains("team_weapon") || this.Types.Contains("250_mortar_halftrack");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a vehicle (not a tank).
        /// </summary>
        [JsonIgnore] public bool IsVehicle 
            => ((!IsArmour && this.Types.Contains("vehicle")) || this.Types.Contains("light_vehicle")) && !this.Types.Contains("250_mortar_halftrack"); // Remove the change of mortar vehicles in this category

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a tank.
        /// </summary>
        [JsonIgnore] public bool IsArmour => this.Types.Contains("vehicle") && !this.Types.Contains("light_vehicle") && !IsHeavyArmour;

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a heavy tank.
        /// </summary>
        [JsonIgnore] public bool IsHeavyArmour => this.Types.Contains("heavy_tank");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a vehicle crew.
        /// </summary>
        [JsonIgnore] public bool IsVehicleCrew => this.Types.Contains("aef_vehicle_crew");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a special (elite) infantry.
        /// </summary>
        [JsonIgnore] public bool IsSpecialInfantry 
            => this.Types.Contains("guard_troops") || this.Types.Contains("shock_troops") || this.Types.Contains("stormtrooper");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be an officer unit.
        /// </summary>
        [JsonIgnore] public bool IsOfficer => this.Types.Contains("sov_officer");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be a command unit.
        /// </summary>
        [JsonIgnore] public bool IsCommandUnit => IsOfficer || this.Types.Contains("command_panzer");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered to be artillery.
        /// </summary>
        [JsonIgnore] public bool IsArtillery => this.Types.Contains("artillery");

        /// <summary>
        /// Can the <see cref="SquadBlueprint"/> be considered a sniper unit.
        /// </summary>
        [JsonIgnore] public bool IsSniper => this.Types.Contains("sniper_soviet") || this.Types.Contains("sniper_german");

    }

}
