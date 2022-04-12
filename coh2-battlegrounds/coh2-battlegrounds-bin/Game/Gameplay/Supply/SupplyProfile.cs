using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;

namespace Battlegrounds.Game.Gameplay.Supply {
    
    /// <summary>
    /// Class representing the supply profile of a <see cref="SquadBlueprint"/>.
    /// </summary>
    public class SupplyProfile {

        /// <summary>
        /// Class representing a weapon supply profile.
        /// </summary>
        public class SupplyWeaponProfile {

            /// <summary>
            /// Get the amount of bullets in a clip.
            /// </summary>
            public int ClipSize { get; }

            /// <summary>
            /// Get the amout of clips carried.
            /// </summary>
            public int ClipCarried { get; }

            /// <summary>
            /// Get the amount of fire calculations per shot.
            /// </summary>
            public float FireRate { get; }

            /// <summary>
            /// Get the amount of entities using the profile.
            /// </summary>
            public int Users { get; }

            /// <summary>
            /// Get the slot index to use in the supply system.
            /// </summary>
            public int SystemSlot { get; }

            /// <summary>
            /// Initialise a new <see cref="SupplyWeaponProfile"/> instance for <paramref name="wbp"/>.
            /// </summary>
            /// <param name="wbp">The weapon blueprint to collect data from.</param>
            /// <param name="users">The amount of entities using the profile.</param>
            public SupplyWeaponProfile(WeaponBlueprint wbp, int users) {
                this.ClipSize = wbp.MagazineSize;
                this.FireRate = wbp.FireRate;
                this.ClipCarried = wbp.Category switch {
                    WeaponCategory.SmallArms => wbp.SmallArmsType switch {
                        WeaponSmallArmsType.Rifle => 12,
                        WeaponSmallArmsType.SubMachineGun => 10,
                        WeaponSmallArmsType.Pistol => 14,
                        WeaponSmallArmsType.LightMachineGun => 6,
                        WeaponSmallArmsType.HeavyMachineGun => 8,
                        _ => 0
                    },
                    WeaponCategory.Ballistic => wbp.BallisticType switch {
                        WeaponBallisticType.TankGun => 24,
                        WeaponBallisticType.AntiTankGun => 18,
                        WeaponBallisticType.InfantryAntiTankGun => 16,
                        _ => 0
                    },
                    _ => 0
                };
                this.Users = users;
                this.SystemSlot = (int)wbp.OnFireCallbackType;
            }

        }

        /// <summary>
        /// Class representing the supply fuel data for a vehicle.
        /// </summary>
        public class SupplyFuelData {

            /// <summary>
            /// The amount of fuel stored.
            /// </summary>
            public float Fuel {  get; }

            /// <summary>
            /// The amount of fuel being burnt while moving.
            /// </summary>
            public float BurnRate { get; }

            /// <summary>
            /// Initialise a new <see cref="SupplyFuelData"/> instance.
            /// </summary>
            /// <param name="fuel">The amount of fuel.</param>
            /// <param name="burnRate">The amount of fuel to burn.</param>
            public SupplyFuelData(float fuel, float burnRate) {
                this.Fuel = fuel;
                this.BurnRate = burnRate;
            }

        }

        /// <summary>
        /// Get the weapon profiles by weapon scar name.
        /// </summary>
        public Dictionary<string, SupplyWeaponProfile> WeaponProfiles { get; }

        /// <summary>
        /// Get the fuel data.
        /// </summary>
        public SupplyFuelData FueldData { get; }

        /// <summary>
        /// Initialise a new <see cref="SupplyProfile"/> instance for <paramref name="sbp"/>.
        /// </summary>
        /// <param name="sbp">The blueprint to generate profile for.</param>
        public SupplyProfile(SquadBlueprint sbp) {

            // Get relevant blueprints
            var ebps = sbp.Loadout.GetEntities();
            var wbps = ebps.SelectMany(x => x.Hardpoints).Select(x => BlueprintManager.FromBlueprintName<WeaponBlueprint>(x)).Distinct();

            // Get weapon profiles
            this.WeaponProfiles = wbps.Select(
                x => new KeyValuePair<string, SupplyWeaponProfile>(x.GetScarName(), new SupplyWeaponProfile(x, ebps.Count(y => y.Hardpoints.Contains(x.Name)))))
                .ToDictionary();

            // If vehicle
            if (sbp.Types.IsVehicle || sbp.Types.IsArmour || sbp.Types.IsHeavyArmour) {

                // Define base
                float fuelBase = sbp.Cost.Fuel;
                if (fuelBase < 40.0f) {
                    fuelBase = 40.0f; 
                }

                // Get values
                float burnRate = sbp.Types.IsVehicle.Then(() => 1.25f).Else(_ => sbp.Types.IsHeavyArmour.Then(() => 3.5f).Else(_ => 1.75f));
                float modifier = sbp.Types.IsVehicle.Then(() => 2.2f).Else(_ => sbp.Types.IsHeavyArmour.Then(() => 0.3f).Else(_ => 1.1f));

                // Get fuel cap (Don't question this, it's solving for f(x)=0 for a function long forgotten).
                float fuelCap = MathF.Pow(fuelBase * fuelBase * modifier / burnRate, 2.0f / 3.0f);

                // Set data
                this.FueldData = new(fuelCap, burnRate);

            }

        }

    }

}
