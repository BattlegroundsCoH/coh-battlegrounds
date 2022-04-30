using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Battlegrounds;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Verification;

namespace BattlegroundsApp.LocalData {

    public static class PlayerCompanies {

        public static event Action? PlayerCompaniesLoaded;

        private static readonly List<Company> __companies = new();

        public static bool HasCompanyForBothAlliances() {
            if (__companies.Count is 0) {
                LoadAll();
            }
            return __companies.Any(x => x.Army.IsAxis) && __companies.Any(x => x.Army.IsAllied);
        }

        public static void LoadAll() {

            try {

                string companyFolder = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.COMPANY_FOLDER, string.Empty);
                string[] companies = Directory.GetFiles(companyFolder, "*.json");

                if (__companies.Count == companies.Length) {
                    return;
                }

                __companies.Clear();

                foreach (string companypath in companies) {
                    try {
                        Company? company = CompanySerializer.GetCompanyFromFile(companypath, true);
                        if (company?.Name?.Replace(" ", "_")?.CompareTo(Path.GetFileNameWithoutExtension(companypath)) == 0) {
                            __companies.Add(company);
                        } else {
                            Trace.WriteLine($"Failed to verify company \"{companypath}\" (Name Mismatch)", nameof(PlayerCompanies));
                        }
                    } catch (ChecksumViolationException checksumViolation) {
                        Trace.WriteLine($"Failed to verify company \"{companypath}\" ({checksumViolation.Message})", nameof(PlayerCompanies));
                    }
                }

                PlayerCompaniesLoaded.Invoke();
                Trace.WriteLine($"Loaded {__companies.Count} user companies", nameof(PlayerCompanies));

            } catch (Exception ex) {
                Trace.WriteLine(ex, nameof(PlayerCompanies));
            }

        }

        /// <summary>
        /// Find all companies matching <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate function to apply on all companies.</param>
        /// <returns>List of companies matching specified predicate.</returns>
        public static List<Company> FindAll(Predicate<Company> predicate) {
            List<Company> matches = new List<Company>();
            foreach (Company c in __companies) {
                if (predicate(c)) {
                    matches.Add(c);
                }
            }
            return matches;
        }

        /// <summary>
        /// Get a company based on its name and faction.
        /// </summary>
        /// <param name="name">The fully qualified name of the company.</param>
        /// <param name="faction">The faction the company will belong to.</param>
        /// <returns>The company with <paramref name="name"/> and <paramref name="faction"/> if found; Otherwise <see langword="null"/> is returned.</returns>
        public static Company? FromNameAndFaction(string name, Faction faction) 
            => FindAll(x => x.Name == name && x.Army == faction).FirstOrDefault();

        /// <summary>
        /// Save the <see cref="Company"/> instance safely in the users local company storage folder.
        /// </summary>
        /// <remarks>
        /// This will override companies with the same name!
        /// </remarks>
        /// <param name="company">The <see cref="Company"/> instance to save.</param>
        public static void SaveCompany(Company company) {
            try {
                string filename = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.COMPANY_FOLDER, $"{company.Name.Replace(" ", "_")}.json");
                if (File.Exists(filename)) {
                    Trace.WriteLine($"Deleting existing player company '{company.Name}'", nameof(PlayerCompanies));
                    File.Delete(filename);
                }
                CompanySerializer.SaveCompanyToFile(company, filename);
                Trace.WriteLine($"Saved player company '{company.Name}'.", nameof(PlayerCompanies));
            } catch (IOException ioex) {
                Trace.WriteLine($"Failed to save player company '{company.Name}'. ", nameof(PlayerCompanies));
                Trace.WriteLine(ioex, nameof(PlayerCompanies));
            } finally {
                Company? oldCompany = __companies.FirstOrDefault(x => x.Name == company.Name && x.Army == company.Army);
                if (oldCompany is not null && oldCompany != company) { // If new reference
                    __companies[__companies.IndexOf(oldCompany)] = company;
                    Trace.WriteLine($"Updated player company '{company.Name}' object", nameof(PlayerCompanies));
                } else if (oldCompany is null) {
                    __companies.Add(company);
                }
            }
        }

        /// <summary>
        /// Delete specified company from local storage.
        /// </summary>
        /// <param name="company">The company to delete.</param>
        public static void DeleteCompany(Company company) {
            string filename = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.COMPANY_FOLDER, $"{company.Name.Replace(" ", "_")}.json");
            if (File.Exists(filename)) {
                File.Delete(filename);
            }
        }

        /// <summary>
        /// Get list of all companies stored locally.
        /// </summary>
        /// <returns>List of <see cref="Company"/> instances.</returns>
        public static List<Company> GetAllCompanies() => __companies;

    }

}
