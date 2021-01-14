using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Battlegrounds;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.LocalData {
    
    public static class PlayerCompanies {

        private static List<Company> __companies;

        public static void LoadAll() {

            __companies = new List<Company>();

            try {

                string companyFolder = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.COMPANY_FOLDER, string.Empty);
                string[] companies = Directory.GetFiles(companyFolder, "*.json");

                foreach (string companypath in companies) {

                    Company company = Company.ReadCompanyFromFile(companypath);
                    if (!company.VerifyChecksum()) {
                        Trace.WriteLine($"Failed to verify company \"{company.Name}\"");
                    } else {

                        __companies.Add(company);

                    }

                }

                Trace.WriteLine($"Loaded {__companies.Count} user companies");

            } catch {

            }

        }

        public static List<Company> FindAll(Predicate<Company> predicate) {
            List<Company> matches = new List<Company>();
            foreach (Company c in __companies) {
                if (predicate(c)) {
                    matches.Add(c);
                }
            }
            return matches;
        }

        public static Company FromNameAndFaction(string name, Faction faction) => FindAll(x => x.Name.CompareTo(name) == 0 && x.Army == faction).FirstOrDefault();
    }

}
