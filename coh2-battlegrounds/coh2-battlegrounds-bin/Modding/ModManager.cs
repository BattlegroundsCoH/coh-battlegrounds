using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;

namespace Battlegrounds.Modding {

    public static class ModManager {

        private static List<ModPackage> __packages;

        public static void Init() {

            // Create packages
            __packages = new();

            // Get package files
            string[] packageFiles = Directory.GetFiles(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_OTHER_FOLDER), "*.package.json");

            // Load all packages
            foreach (string packageFilepath in packageFiles) {
                string packageName = Path.GetFileNameWithoutExtension(packageFilepath);
                try {

                    // Read the mod package
                    ModPackage package = JsonSerializer.Deserialize<ModPackage>(File.ReadAllText(packageFilepath));
                    if (__packages.Any(x => x.ID == package.ID)) {
                        Trace.WriteLine($"Failed to load mod package '{package.ID}' (Duplicate ID entry).", nameof(ModManager));
                        continue;
                    }

                    // Add mod package
                    __packages.Add(package);

                    // Log
                    Trace.WriteLine($"Loaded mod package '{package.ID}'.", nameof(ModManager));

                } catch (Exception ex) {

                    // Log error and file
                    Trace.WriteLine($"Failed to read mod package '{packageName}'.", nameof(ModManager));
                    Trace.WriteLine($"{packageName} Error is --> {ex}", nameof(ModManager));

                }
            }

        }

    }

}
