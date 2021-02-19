using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Util;
using Battlegrounds.Lua;
using System.Text;

namespace Battlegrounds.Campaigns {
    
    public enum CampaignMode {
        Undefined = 0x0,
        Singleplayer = 0x1,
        Cooperative = 0x2,
        Competitive = 0x3,
    }

    public enum CampaignTheatre {
        Undefined = 0x0,
        East = 0x1,
        West = 0x2,
        EastWest = 0x3
    }

    public record CampaignArmy(LocaleKey Name, LocaleKey Desc, Faction Army, int Min, int Max);

    public record CampaignDate(int Year, int Month, int Day);

    public class CampaignPackage {

        public record TurnData(int TurnLength, (int Year, int Month, int Day) Start, (int Year, int Month, int Day) End);

        public record ArmyData(LocaleKey Name, LocaleKey Desc, Faction Army, int MinPlayers, int MaxPlayers, LuaTable FullArrmyData);

        public LocaleKey Name { get; private set; }

        public LocaleKey Description { get; private set; }

        public LocaleKey Location { get; private set; }

        public CampaignMode[] CampaignModes { get; private set; }

        public CampaignTheatre Theatre { get; private set; }

        public int MaxPlayers { get; private set; }

        public string NormalStartingSide { get; private set; }

        public TurnData CampaignTurnData { get; private set; }

        public ArmyData[] CampaignArmies { get; private set; }

        #region Binary Data

        private static uint[] SupportedVersions = { 10 };

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
            string locale = reader.ReadUnicodeString();

            // Load display data
            if (!LoadBinaryDisplay(reader, locale, binaryFilepath)) {
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has no invalid display data and cannot be loaded.", nameof(CampaignPackage));
                return false;
            }

            // Create campaign array
            this.CampaignArmies = new ArmyData[armyCount];

            // Load army data
            for (int i = 0; i < armyCount; i++) {
                if (!LoadArmyData(reader, i, locale, binaryFilepath)) {
                    Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has invalid army data and cannot be loaded.", nameof(CampaignPackage));
                    return false;
                }
            }

            // Return true => everything was loaded
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

            // Read lengths
            int length = reader.ReadInt32();
            if (length > 0) {

                // Read bytes
                using var ms = reader.FillStream(length);

                // Parse lua table
                if (LuaBinary.FromBinary(ms, Encoding.Unicode) is LuaTable armyTable) {

                    // Assign data
                    this.CampaignArmies[index] = new ArmyData(name, desc, Faction.FromName(army), players[0], players[1], armyTable);

                } else {
                    return false;
                }

            } else {
                
                // Log
                Trace.WriteLine($"Campaign '{Path.GetFileName(binaryFilepath)}' has an army ('{army}') with no army data.", nameof(CampaignPackage));
                
                // Save anyways
                this.CampaignArmies[index] = new ArmyData(name, desc, Faction.FromName(army), players[0], players[1], null);

            }

            return true;

        }

        #endregion

    }

}
