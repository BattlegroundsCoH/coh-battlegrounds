using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Util;
using Battlegrounds.Lua;
using Battlegrounds.Gfx;

namespace Battlegrounds.Campaigns {
    
    /// <summary>
    /// 
    /// </summary>
    public enum CampaignMode {
        Undefined = 0x0,
        Singleplayer = 0x1,
        Cooperative = 0x2,
        Competitive = 0x3,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum CampaignTheatre {
        Undefined = 0x0,
        East = 0x1,
        West = 0x2,
        EastWest = 0x3
    }

    /// <summary>
    /// 
    /// </summary>
    public record CampaignDate(int Year, int Month, int Day);

    /// <summary>
    /// 
    /// </summary>
    public class CampaignPackage {

        /// <summary>
        /// 
        /// </summary>
        public record TurnData(int TurnLength, (int Year, int Month, int Day) Start, (int Year, int Month, int Day) End);

        /// <summary>
        /// 
        /// </summary>
        public record WeatherData((int Year, int Month, int Day) WinterStart, (int Year, int Month, int Day) WinterEnd, HashSet<string> WinterAtmospheres, HashSet<string> SummerAtmospheres);

        /// <summary>
        /// 
        /// </summary>
        public record ArmyData(LocaleKey Name, LocaleKey Desc, Faction Army, int MinPlayers, int MaxPlayers, LuaTable FullArmyData, ArmyGoalData[] Goals);

        /// <summary>
        /// 
        /// </summary>
        public record ArmyGoalData(LocaleKey Title, LocaleKey Desc, int Priority, byte Type, bool Hidden, string OnDone, string OnFail, string OnUI, string OnTrigger, ArmyGoalData[] SubGoals);

        /// <summary>
        /// 
        /// </summary>
        public LocaleKey Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public LocaleKey Description { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public LocaleKey Location { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public CampaignMode[] CampaignModes { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public CampaignTheatre Theatre { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int MaxPlayers { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string NormalStartingSide { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TurnData CampaignTurnData { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public WeatherData CampaignWeatherData { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ArmyData[] CampaignArmies { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<GfxMap> GfxResources { get; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> CampaignScripts { get; }

        /// <summary>
        /// 
        /// </summary>
        public CampaignMapData MapData { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Localize LocaleManager { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string LocaleSourceID { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public CampaignPackage() {
            this.GfxResources = new List<GfxMap>();
            this.CampaignScripts = new List<string>();
        }

        #region Binary Data

        private static uint[] SupportedVersions = { 10 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryFilepath"></param>
        /// <returns></returns>
        public bool LoadFromBinary(string binaryFilepath) {

            // Open reader
            using BinaryReader reader = new BinaryReader(File.OpenRead(binaryFilepath));

            // Make sure the first four bytes == 2021
            if (reader.ReadInt32() != 2021) {
                return false;
            }

            // Make sure we get a magic match
            if (!ByteUtil.Match(reader.ReadBytes(6), "BG\x04\x05\x05\x04")) {
                return false;
            }

            // Read version (Use if there will be deviations later on)
            uint v = reader.ReadUInt32();
            if (!SupportedVersions.Contains(v)) {
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' is using a newer format and will not be loaded.", nameof(CampaignPackage));
                return false;
            }

            // Read resoure and army count
            int resourceCount = reader.ReadInt32();
            int armyCount = reader.ReadInt32();

            // Get default locale ID
            this.LocaleSourceID = reader.ReadUnicodeString();

            // Load display data
            if (!LoadBinaryDisplay(reader, this.LocaleSourceID, binaryFilepath)) {
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has invalid display data and cannot be loaded.", nameof(CampaignPackage));
                return false;
            }

            // Load weather data
            if (!LoadBinaryWeather(reader)) {
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has invalid weather data and cannot be loaded.", nameof(CampaignPackage));
                return false;
            }

            // Create campaign array
            this.CampaignArmies = new ArmyData[armyCount];

            // Load army data
            for (int i = 0; i < armyCount; i++) {
                if (!LoadArmyData(reader, i, this.LocaleSourceID, binaryFilepath)) {
                    Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has invalid army data and cannot be loaded.", nameof(CampaignPackage));
                    return false;
                }
            }

            // Read map definition from binary
            if (LuaBinary.FromBinary(new MemoryStream(reader.ReadBytes(reader.ReadInt32())), Encoding.Unicode) is not LuaTable mapdef) {
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' contains no map definition.", nameof(CampaignPackage));
                return false;
            }

            // Create localizer (Based on locale language
            this.LocaleManager = new Localize(BattlegroundsInstance.Localize.Language);

            // Read resources
            for (int i = 0; i < resourceCount; i++) {

                // Read identifier
                string identifier = reader.ReadUnicodeString();

                // Read resource type
                byte resourceType = reader.ReadByte();

                // Read length
                int resourceByteLength = reader.ReadInt32();

                // Read all bytes
                byte[] resourceBytes = reader.ReadBytes(resourceByteLength);

                // From here, determine resource type
                switch (resourceType) {
                    case 0x1:
                        if (this.MapData is null) {
                            // Create map data
                            this.MapData = new CampaignMapData(resourceBytes) {
                                Data = mapdef
                            };
                        } else {
                            Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has conflicting campaign maps.", nameof(CampaignPackage));
                            return false;
                        }
                        break;
                    case 0x2:
                        if (!this.LocaleManager.LoadBinaryLocaleFile(new MemoryStream(resourceBytes) { Position = 0 }, identifier)) {
                            Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' contains faulty locale file.", nameof(CampaignPackage));
                        }
                        break;
                    case 0x3:
                        this.CampaignScripts.Add(Encoding.UTF8.GetString(resourceBytes));
                        break;
                    case 0x4:
                        break;
                    case 0x5:
                        if (GfxMap.FromBinary(new MemoryStream(resourceBytes)) is GfxMap gfx) {
                            this.GfxResources.Add(gfx);
                        }
                        break;
                    default:
                        break;
                }

            }

            // Return true => everything was loaded
            return true;

        }

        private bool LoadBinaryWeather(BinaryReader reader) {

            HashSet<string> winter = new HashSet<string>();
            HashSet<string> summer = new HashSet<string>();

            // Create basic one
            this.CampaignWeatherData = new WeatherData((0, 0, 0), (0, 0, 0), winter, summer);

            // Read winter dates (if any)
            int winterStartYear = reader.ReadInt32();
            if (winterStartYear != -1) {

                // Read values
                int winterStartMonth = reader.ReadInt32();
                int winterStartDay = reader.ReadInt32();
                int winterEndYear = reader.ReadInt32();
                int winterEndMonth = reader.ReadInt32();
                int winterEndDay = reader.ReadInt32();

                // Mutate
                this.CampaignWeatherData = this.CampaignWeatherData with
                {
                    WinterStart = (winterStartYear, winterStartMonth, winterStartDay),
                    WinterEnd = (winterEndYear, winterEndMonth, winterEndDay)
                };

            }

            int summerCount = reader.ReadInt32();
            int winterCount = reader.ReadInt32();

            for (int i = 0; i < summerCount; i++) {
                int count = reader.ReadInt32();
                summer.Add(Encoding.Unicode.GetString(reader.ReadBytes(count)));
            }

            for (int i = 0; i < winterCount; i++) {
                int count = reader.ReadInt32();
                winter.Add(Encoding.Unicode.GetString(reader.ReadBytes(count)));
            }

            return true;

        }

        private bool LoadBinaryDisplay(BinaryReader reader, string locale, string binaryFilepath) {

            // Read lengths
            int[] fe_lengths = {
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32()
            };

            // Read display data
            this.Name = new LocaleKey(reader.ReadUnicodeString(fe_lengths[0]), locale);
            this.Description = new LocaleKey(reader.ReadUnicodeString(fe_lengths[1]), locale);
            this.Location = new LocaleKey(reader.ReadUnicodeString(fe_lengths[2]), locale);

            // Read theatre
            this.Theatre = reader.ReadByte() switch {
                0x1 => CampaignTheatre.East,
                0x2 => CampaignTheatre.West,
                0x3 => CampaignTheatre.EastWest,
                _ => CampaignTheatre.Undefined,
            };

            // If no theatre is assigned
            if (this.Theatre == CampaignTheatre.Undefined) {
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has no valid theatre defined!", nameof(CampaignPackage));
            }

            // Read max player count
            this.MaxPlayers = reader.ReadInt32();

            // Read campaign modes
            this.CampaignModes = new CampaignMode[reader.ReadByte()];
            for (int i = 0; i < this.CampaignModes.Length; i++) {
                this.CampaignModes[i] = reader.ReadByte() switch {
                    0x1 => CampaignMode.Singleplayer,
                    0x2 => CampaignMode.Cooperative,
                    0x3 => CampaignMode.Competitive,
                    _ => CampaignMode.Undefined
                };
                if (this.CampaignModes[i] == CampaignMode.Undefined) {
                    Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has an undefined campaign mode at index {i + 1}.", nameof(CampaignPackage));
                }
            }

            // If all modes are undefined, return false
            if (this.CampaignModes.All(x => x == CampaignMode.Undefined)) {
                return false;
            }

            // Read turn data
            this.CampaignTurnData = new TurnData(
                reader.ReadInt32(),
                (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32())
                );

            // Read normal starting side
            this.NormalStartingSide = reader.ReadUnicodeString();

            // Return true => Display data was read without fatal errors
            return true;

        }

        private bool LoadArmyData(BinaryReader reader, int index, string locale, string binaryFilepath) {

            // Read the army
            string army = reader.ReadASCIIString();

            // Read players
            int[] players = {
                reader.ReadInt32(),
                reader.ReadInt32()
            };

            // Read key lengths
            int[] keyLengths = {
                reader.ReadInt32(),
                reader.ReadInt32(),
            };

            // Read locales
            LocaleKey name = new LocaleKey(reader.ReadUnicodeString(keyLengths[0]), locale);
            LocaleKey desc = new LocaleKey(reader.ReadUnicodeString(keyLengths[1]), locale);

            // Army table
            LuaTable armyTable = null;

            // Read lengths
            int length = reader.ReadInt32();
            if (length > 0) {

                // Read bytes
                using var ms = reader.FillStream(length);

                // Parse
                armyTable = LuaBinary.FromBinary(ms, Encoding.Unicode) as LuaTable;

                // Parse lua table
                if (armyTable is null) {

                    // Log
                    Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has an army ('{army}') with invalid army data.", nameof(CampaignPackage));

                    // Return false
                    return false;

                }

            } else {
                
                // Log
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has an army ('{army}') with no army data.", nameof(CampaignPackage));
                
            }

            // Read goals
            ArmyGoalData[] ReadGoals() {

                // Will fetch string based on flag and mask
                string GetString(byte flag, byte mask) { 
                    if ((flag & mask) != 0) {
                        return reader.ReadUnicodeString();
                    } else {
                        return string.Empty;
                    }
                }

                // Read byte
                byte objCount = reader.ReadByte();

                // Alloc array for goals
                ArmyGoalData[] result = new ArmyGoalData[objCount];

                // Loop over goals
                for (byte i = 0; i < objCount; i++) {

                    // Read lengths 
                    // Read priority and state
                    int gTitleLength = reader.ReadInt32();
                    int gDescLength = reader.ReadInt32();
                    int priority = reader.ReadInt32();
                    bool hidden = reader.ReadBoolean();

                    // Read keys
                    var gTitle = new LocaleKey(reader.ReadUnicodeString(gTitleLength), locale);
                    var gDesc = new LocaleKey(reader.ReadUnicodeString(gDescLength), locale);

                    // Read flag
                    byte flag = reader.ReadByte();

                    // Get goal states
                    string onFail = GetString(flag, 0b_0000_0001);
                    string onDone = GetString(flag, 0b_0000_0010);
                    string onUI = GetString(flag, 0b_0000_0100);
                    string onTrigger = GetString(flag, 0b_0000_1000);
                    byte type = (byte)(flag >> 4);

                    // Read child goals
                    var childGoals = ReadGoals();

                    // Save goal data
                    result[i] = new ArmyGoalData(gTitle, gDesc, priority, type, hidden, onDone, onFail, onUI, onTrigger, childGoals);

                }

                // Return results
                return result;

            }

            // Read goals
            var goals = ReadGoals();

            // Save anyways
            this.CampaignArmies[index] = new ArmyData(name, desc, Faction.FromName(army), players[0], players[1], armyTable, goals);

            // Return true
            return true;

        }

        #endregion

    }

}
