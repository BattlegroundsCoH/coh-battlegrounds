using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;

namespace Battlegrounds.Game.Gameplay.Supply {
    
    /// <summary>
    /// 
    /// </summary>
    public class SupplyProfile {

        public class SupplyWeaponProfile {

            public int ClipSize { get; }

            public int ClipCarried { get; }

            public float FireRate { get; }

            public SupplyWeaponProfile(WeaponBlueprint wbp) {
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
                    _ => 0
                };
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public class SupplyFuelData {

            /// <summary>
            /// 
            /// </summary>
            public float Fuel {  get; }

            /// <summary>
            /// 
            /// </summary>
            public float BurnRate { get; }

            public SupplyFuelData(float fuel, float burnRate) {
                this.Fuel = fuel;
                this.BurnRate = burnRate;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, SupplyWeaponProfile> WeaponProfiles { get; }

        /// <summary>
        /// 
        /// </summary>
        public SupplyFuelData FueldData { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sbp"></param>
        public SupplyProfile(SquadBlueprint sbp) {

            // Get relevant blueprints
            var ebps = sbp.Loadout.GetEntities();
            var wbps = ebps.SelectMany(x => x.Hardpoints).Select(x => BlueprintManager.FromBlueprintName<WeaponBlueprint>(x)).Distinct();

            // Get weapon profiles
            this.WeaponProfiles = wbps.Select(x => new KeyValuePair<string, SupplyWeaponProfile>(x.GetScarName(), new SupplyWeaponProfile(x))).ToDictionary();

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

                // Get fuel cap
                float fuelCap = MathF.Pow(fuelBase * fuelBase * modifier / burnRate, 2.0f / 3.0f);

                // Set data
                this.FueldData = new(fuelCap, burnRate);

            }

        }

    }

}
