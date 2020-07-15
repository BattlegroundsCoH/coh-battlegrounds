using System;
using System.Collections.Generic;
using System.Text;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Utility class for generating random <see cref="Company"/> data ready for ingame-use.
    /// </summary>
    public static class CompanyGenerator {

        static Random __random = new Random();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="army"></param>
        /// <param name="type"></param>
        /// <param name="tuningGUID"></param>
        /// <param name="borrowVanilla"></param>
        /// <param name="allowVeterancy"></param>
        /// <returns></returns>
        public static Company Generate(Faction army, CompanyType type, string tuningGUID, bool borrowVanilla, bool allowVeterancy, bool allowCapturedEquipment) {

            // Create the company
            Company company = new Company(null, "Auto-Generated Company", type, army, tuningGUID);



            // Return the company
            return company;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="army"></param>
        /// <param name="tuningGUID"></param>
        /// <param name="borrowVanilla"></param>
        /// <param name="allowVeterancy"></param>
        /// <returns></returns>
        public static Company Generate(Faction army, string tuningGUID, bool borrowVanilla, bool allowVeterancy, bool allowCapturedEquipment)
            => Generate(army, CompanyType.Unspecified, tuningGUID, borrowVanilla, allowVeterancy, allowCapturedEquipment);

    }

}
