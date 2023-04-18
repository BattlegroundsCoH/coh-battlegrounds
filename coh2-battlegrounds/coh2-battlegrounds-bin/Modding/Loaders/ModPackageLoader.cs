using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Logging;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Modding.Verifier;

namespace Battlegrounds.Modding.Loaders;

/// <summary>
/// Class for loading <see cref="ModPackage"/> instances through their JSON representation.
/// </summary>
public class ModPackageLoader : JsonConverter<ModPackage> {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <inheritdoc/>
    public override ModPackage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        // Lookup table to store values in temp
        Dictionary<string, object> __lookup = new();

        // Read data
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            __lookup[prop] = prop switch {
                "ID" => reader.GetString()?.ToLowerInvariant() ?? string.Empty,
                "Name" => reader.GetString() ?? string.Empty,
                "TuningGUID" => reader.GetString() ?? string.Empty,
                "GamemodeGUID" => reader.GetString() ?? string.Empty,
                "AssetGUID" => reader.GetString() ?? string.Empty,
                "ParadropUnits" => reader.GetStringArray(),
                "VerificationUpgrade" => reader.GetString() ?? string.Empty,
                "AllowSupplySystem" => reader.GetBoolean(),
                "AllowWeatherSystem" => reader.GetBoolean(),
                "LocaleFiles" => JsonSerializer.Deserialize<ModLocale[]>(ref reader) ?? Array.Empty<ModLocale>(),
                "FactionData" => JsonSerializer.Deserialize<FactionData[]>(ref reader) ?? Array.Empty<FactionData>(),
                "Gamemodes" => JsonSerializer.Deserialize<Gamemode[]>(ref reader) ?? Array.Empty<Gamemode>(),
                "Towing" => ReadTowdata(ref reader),
                "TeamWeaponCaptureSquads" => JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(ref reader) ?? new(),
                "DataSourcePath" => reader.GetString() ?? string.Empty,
                "SupportedGames" => Enum.TryParse(reader.GetString() ?? string.Empty, true, out GameCase gameCase) ? gameCase : GameCase.Unspecified,
                _ => throw new NotImplementedException(prop)
            };
        }

        // Get GUIDs
        var tuningGUID = __lookup.ContainsKey("TuningGUID") ? ModGuid.FromGuid(__lookup["TuningGUID"] as string ?? string.Empty) : ModGuid.BaseGame;
        var gamemodeGUID = __lookup.ContainsKey("GamemodeGUID") ? ModGuid.FromGuid(__lookup["GamemodeGUID"] as string ?? string.Empty) : ModGuid.BaseGame;
        var assetGUID = __lookup.ContainsKey("AssetGUID") ? ModGuid.FromGuid(__lookup["AssetGUID"] as string ?? string.Empty) : ModGuid.BaseGame;

        // Get factions
        Dictionary<Faction, FactionData> factions = __lookup.GetCastValueOrDefault("FactionData", Array.Empty<FactionData>())
            .ToDictionary(x => Faction.FromName(x.Faction, x.Game));

        // Get tow data
        (bool hastow, string istow, string istowing) = __lookup.GetCastValueOrDefault("Towing", (false, string.Empty, string.Empty));

        // Get ID
        string packageID = __lookup.GetCastValueOrDefault("ID", string.Empty);
        if (string.IsNullOrEmpty(packageID)) {
            throw new InvalidDataException("Expected package ID but found none. Cannot read the mod package!");
        }

        // Return mod package
        ModPackage package = new() {
            ID = packageID,
            PackageName = __lookup.GetCastValueOrDefault("Name", packageID),
            TuningGUID = tuningGUID,
            GamemodeGUID = gamemodeGUID,
            AssetGUID = assetGUID,
            FactionSettings = factions,
            IsTowEnabled = hastow,
            IsTowedUpgrade = istow,
            IsTowingUpgrade = istowing,
            VerificationUpgrade = __lookup.GetCastValueOrDefault("VerificationUpgrade", string.Empty),
            ParadropUnits = __lookup.GetCastValueOrDefault("ParadropUnits", Array.Empty<string>()),
            LocaleFiles = __lookup.GetCastValueOrDefault("LocaleFiles", Array.Empty<ModLocale>()),
            AllowSupplySystem = __lookup.GetCastValueOrDefault("AllowSupplySystem", false),
            AllowWeatherSystem = __lookup.GetCastValueOrDefault("AllowWeatherSystem", false),
            Gamemodes = __lookup.GetCastValueOrDefault("Gamemodes", Array.Empty<Gamemode>()),
            TeamWeaponCaptureSquads = __lookup.GetCastValueOrDefault("TeamWeaponCaptureSquads", new Dictionary<string, Dictionary<string, string>>()),
            DataSourcePath = __lookup.GetCastValueOrDefault("DataSourcePath", ""),
            SupportedGames = __lookup.GetCastValueOrDefault("SupportedGames", GameCase.Unspecified)
        };

        // Set faction data owners and load source files
        package.FactionSettings.Values.ForEach(x => {
            x.Package = package;
            x.Companies = x.Companies with {
                Types = x.Companies.Types.Filter(x => x.Hidden, false).Map(LoadCompanyTypeFromSource).Filter(CompanyTypeWarnings.CheckCompanyType, CompanyTypeState.Valid)
            };
        });

        // Return the package
        return package;

    }

    private static FactionCompanyType LoadCompanyTypeFromSource(FactionCompanyType original) {

        // Compile str
        string logstr = $"Loaded faction company type '{original.Id}' for mod '{original.FactionData!.Package!.PackageName}'";

        // Just return input
        if (string.IsNullOrEmpty(original.SourceFile)) {
            logger.Info(logstr);
            return original;
        }
        
        // Get relative virt path
        string abspath = BattlegroundsContext.GetRelativeVirtualPath(original.SourceFile, ".json");
        if (File.Exists(abspath)) {
            
            // Open read
            using var fs = File.OpenRead(abspath);

            // Deserialise and return result (or original if failure)
            try {
                if (JsonSerializer.Deserialize<FactionCompanyType>(fs) is FactionCompanyType fct) {
                    logger.Info(logstr);
                    fct.ChangeId(original.Id);
                    fct.FactionData = original.FactionData;
                    return fct;
                }
            } catch (Exception ex) {
                logger.Error($"Failed to read faction company type '{original.Id}'.\n{ex}");
            }

        } else {
            
            // Log path not found
            logger.Warning($"Virtual path '{original.SourceFile}' not found");

        }

        // Return original
        return original;

    }

    private static (bool, string, string) ReadTowdata(ref Utf8JsonReader reader) {
        object[] data = { false, string.Empty, string.Empty };
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            data[prop switch {
                "IsEnabled" => 0,
                "IsTowed" => 1,
                "IsTowing" => 2,
                _ => throw new NotImplementedException(prop)
            }] = reader.TokenType is JsonTokenType.String ? (reader.GetString() ?? string.Empty) : reader.GetBoolean();
        }
        return ((bool)data[0], data[1] as string ?? string.Empty, data[2] as string ?? string.Empty);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, ModPackage value, JsonSerializerOptions options) => throw new NotSupportedException();

}
