﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Modding {

    /// <summary>
    /// Class for loading <see cref="ModPackage"/> instances through their JSON representation.
    /// </summary>
    public class ModPackageLoader : JsonConverter<ModPackage> {

        public override ModPackage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            // Lookup table to store values in temp
            Dictionary<string, object> __lookup = new();

            // Read data
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "ID" => reader.GetString().ToLower(),
                    "Name" => reader.GetString(),
                    "TuningGUID" => reader.GetString(),
                    "GamemodeGUID" => reader.GetString(),
                    "AssetGUID" => reader.GetString(),
                    "ParadropUnits" => reader.GetStringArray(),
                    "VerificationUpgrade" => reader.GetString(),
                    "AllowSupplySystem" => reader.GetBoolean(),
                    "AllowWeatherSystem" => reader.GetBoolean(),
                    "LocaleFiles" => JsonSerializer.Deserialize<ModPackage.ModLocale[]>(ref reader),
                    "FactionData" => JsonSerializer.Deserialize<ModPackage.FactionData[]>(ref reader),
                    "CustomOptions" => JsonSerializer.Deserialize<ModPackage.CustomOptions[]>(ref reader),
                    "Gamemodes" => JsonSerializer.Deserialize<ModPackage.Gamemode[]>(ref reader),
                    "Towing" => ReadTowdata(ref reader),
                    _ => throw new NotImplementedException(prop)
                };
            }

            // Get GUIDs
            ModGuid tuningGUID = __lookup.ContainsKey("TuningGUID") ? ModGuid.FromGuid(__lookup["TuningGUID"] as string) : ModGuid.BaseGame;
            ModGuid gamemodeGUID = __lookup.ContainsKey("GamemodeGUID") ? ModGuid.FromGuid(__lookup["GamemodeGUID"] as string) : ModGuid.BaseGame;
            ModGuid assetGUID = __lookup.ContainsKey("AssetGUID") ? ModGuid.FromGuid(__lookup["AssetGUID"] as string) : ModGuid.BaseGame;

            // Get factions
            Dictionary<Faction, ModPackage.FactionData> factions = (__lookup.GetValueOrDefault("FactionData", Array.Empty<ModPackage.FactionData>()) as ModPackage.FactionData[])
                .ToDictionary(x => Faction.FromName(x.Faction));

            // Get tow data
            (bool hastow, string istow, string istowing) = ((bool, string, string))__lookup.GetValueOrDefault("Towing", (false, string.Empty, string.Empty));

            //// Get locale file
            //List<UcsFile> localizedFile = new();
            //foreach (string locFile in __lookup.GetValueOrDefault("LocaleFiles", Array.Empty<string>()) as string[]) {
            //    try {
            //        string locpath = BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_OTHER_FOLDER, locFile);
            //        if (File.Exists(locpath)) {
            //            localizedFile.Add(UcsFile.LoadFromFile(locpath));
            //        } else if (locpath != BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.MOD_OTHER_FOLDER)) {
            //            Trace.WriteLine($"Failed to locate ucs file '{locpath}'", nameof(ModPackageLoader));
            //        }
            //    } catch (Exception locex) {
            //        Trace.WriteLine($"Failed to load ucs file for mod package '{__lookup.GetValueOrDefault("ID") as string}' ({locex.Message})", nameof(ModPackageLoader));
            //    }
            //}

            // Get ID
            string packageID = __lookup.GetValueOrDefault("ID") as string;
            if (string.IsNullOrEmpty(packageID)) {
                throw new InvalidDataException("Expected package ID but found none. Cannot read the mod package!");
            }

            // Return mod package
            return new ModPackage() {
                ID = packageID,
                PackageName = __lookup.GetValueOrDefault("Name", packageID) as string,
                TuningGUID = tuningGUID,
                GamemodeGUID = gamemodeGUID,
                AssetGUID = assetGUID,
                FactionSettings = factions,
                IsTowEnabled = hastow,
                IsTowedUpgrade = istow,
                IsTowingUpgrade = istowing,
                VerificationUpgrade = __lookup.GetValueOrDefault("VerificationUpgrade", string.Empty) as string,
                ParadropUnits = __lookup.GetValueOrDefault("ParadropUnits", Array.Empty<string>()) as string[],
                LocaleFiles = __lookup.GetValueOrDefault("LocaleFiles", Array.Empty<ModPackage.ModLocale>()) as ModPackage.ModLocale[],
                AllowSupplySystem = (bool)__lookup.GetValueOrDefault("AllowSupplySystem", false),
                AllowWeatherSystem = (bool)__lookup.GetValueOrDefault("AllowWeatherSystem", false),
                Gamemodes = __lookup.GetValueOrDefault("Gamemodes", Array.Empty<ModPackage.Gamemode>()) as ModPackage.Gamemode[]
            };

        }

        private static (bool, string, string) ReadTowdata(ref Utf8JsonReader reader) {
            object[] data = { false, string.Empty, string.Empty };
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                data[prop switch {
                    "IsEnabled" => 0,
                    "IsTowed" => 1,
                    "IsTowing" => 2,
                    _ => throw new NotImplementedException(prop)
                }] = reader.TokenType is JsonTokenType.String ? reader.GetString() : reader.GetBoolean();
            }
            return ((bool)data[0], data[1] as string, data[2] as string);
        }

        public override void Write(Utf8JsonWriter writer, ModPackage value, JsonSerializerOptions options) => throw new NotSupportedException();

    }

}