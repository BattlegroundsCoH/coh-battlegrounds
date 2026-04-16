using System.IO;
using System.Text;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

namespace Battlegrounds.Serializers;

public sealed class BinaryCompanyDeserializer(IBlueprintService blueprintService) : ICompanyDeserializer {
    
    private readonly IBlueprintService _blueprintService = blueprintService;

    private static readonly uint[] SUPPORTED_VERSIONS = [
        BinaryCompanySerializer.BINARY_COMPANY_VERSION_1, 
        BinaryCompanySerializer.BINARY_COMPANY_VERSION_2, 
        BinaryCompanySerializer.BINARY_COMPANY_VERSION_3,
        BinaryCompanySerializer.BINARY_COMPANY_VERSION_4
    ];

    public bool IgnoreUnknownSquads { get; set; } = true; // Ignore squads that are not recognized by the serializer instead of throwing an exception.

    public static bool IsSupportedVersion(uint version) => SUPPORTED_VERSIONS.Contains(version);

    public Company DeserializeCompany(Stream source) {
        using var reader = new BinaryReader(source, Encoding.UTF8, true);

        var header = reader.ReadBytes(4);
        if (header[0] != 0x42 || header[1] != 0x47 || header[2] != 0x43 || header[3] != 0x00) {
            throw new InvalidDataException("Invalid company file header.");
        }

        var version = reader.ReadUInt32();
        if (!IsSupportedVersion(version)) {
            throw new InvalidDataException($"Unsupported company file version: {version}"); // Current impl, only supports version 1 and 2. Add support for more versions in the future.
        }

        string createdBy, updatedBy;
        var timestamp = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Timestamp for serialization
        var createdAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Created at timestamp
        if (version >= BinaryCompanySerializer.BINARY_COMPANY_VERSION_2) {
            createdBy = ReadUtf8String(reader); // Created by can be UTF-8 in version 2
        } else {
            createdBy = "Unspecified"; // Default value for created by in version 1
        }

        var updatedAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Updated at timestamp
        if (version >= BinaryCompanySerializer.BINARY_COMPANY_VERSION_2) {
            updatedBy = ReadUtf8String(reader); // Updated by can be UTF-8 in version 2
        } else {
            updatedBy = "Unspecified"; // Default value for updated by in version 1
        }

        uint companyVersion = version >= BinaryCompanySerializer.BINARY_COMPANY_VERSION_2 ? reader.ReadUInt32() : 1; // Company version, added in version 2. Default to 1 for version 1 files.

        string id = ReadASCIIString(reader); // Company ID will always be ASCII
        string name = ReadUtf8String(reader); // Company name can be UTF-8

        byte gameIdByte = reader.ReadByte();
        string gameId = gameIdByte switch {
            0x03 => CoH3.GameId, // CoH3
            _ => throw new InvalidDataException($"Unknown game ID: {gameIdByte}") // Unknown game ID
        };

        string faction = ReadASCIIString(reader); // Faction will always be ASCII

        string doctrineId = version >= BinaryCompanySerializer.BINARY_COMPANY_VERSION_4 ? ReadASCIIString(reader) : "faction_default"; // Default doctrine ID for older versions
        uint doctrineVersion = version >= BinaryCompanySerializer.BINARY_COMPANY_VERSION_4 ? reader.ReadUInt32() : 1; // Default doctrine version for older versions

        List<CapturedItem> items;
        Dictionary<int, CapturedItem> inventory = new Dictionary<int, CapturedItem>();
        if (version < BinaryCompanySerializer.BINARY_COMPANY_VERSION_4) {
            items = [];
        } else {
            uint itemCount = reader.ReadUInt32(); // Number of captured items
            items = new List<CapturedItem>((int)itemCount);
            for (int i = 0; i < itemCount; i++) {
                if (ReadItem(gameId, companyVersion, reader) is CapturedItem ci) {
                    items.Add(ci);
                    inventory[ci.Id] = ci;
                } else {
                    // TODO: Allow ignoring unknown captured items in future versions if necessary, but for now we will throw an exception to ensure data integrity.
                    throw new InvalidDataException($"Unknown item encountered at index {i}.");
                }
            }
        }

        uint squadCount = reader.ReadUInt32(); // Number of squads
        var squads = new List<Squad>((int)squadCount);
        for (int i = 0; i < squadCount; i++) {
            if (ReadSquad(gameId, version, inventory, reader) is Squad sq) {
                squads.Add(sq);
            } else if (IgnoreUnknownSquads) {
                // TODO: Log the ignored squad
                // If the squad is not recognized, skip it
                continue;
            } else {
                throw new InvalidDataException($"Unknown squad encountered at index {i}.");
            }
        }

        return new Company {
            Id = id,
            Name = name,
            GameId = gameId,
            Faction = faction,
            DoctrineId = doctrineId,
            DoctrineVersion = doctrineVersion,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy,
            Squads = squads,
            CapturedItems = items,
            Version = companyVersion
        };

    }

    private readonly record struct IntermediateSlotItem(int ItemId, string? EntityId, string? SlotItemId);
    private readonly record struct IntermediateTransportSquad(bool Enabled, string? BlueprintId, bool DropOffOnly);
    private readonly record struct IntermediatePassengerSquad(bool Enabled, int PassengerSquadId);
    private readonly record struct IntermediateCapturedItem(bool Enabled, int Id, string? WeaponBlueprintId, string? CrewBlueprintId);

    private CapturedItem? ReadItem(string gameId, uint companyFileVersion, BinaryReader reader) {

        int itemId = reader.ReadInt32(); // Item ID
        string blueprintId = ReadASCIIString(reader); // Item Blueprint ID will always be ASCII
        int squadCapturer = reader.ReadInt32(); // Squad that captured the item
        DateTime capturedAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Captured at timestamp

        if (!_blueprintService.TryGetBlueprint(gameId, blueprintId, out EntityBlueprint? itemEBP)) {
            return null;
        }

        return new CapturedItem {
            Id = itemId,
            ItemBlueprint = itemEBP,
            CapturedBySquadId = squadCapturer,
            CapturedAt = capturedAt
        };

    }

    private Squad? ReadSquad(string gameId, uint companyFileVersion, Dictionary<int, CapturedItem> inventory, BinaryReader reader) {

        int squadId = reader.ReadInt32(); // Squad ID
        string blueprintId = ReadASCIIString(reader); // Squad Blueprint ID will always be ASCII

        string customName = ReadUtf8String(reader);

        SquadPhase phase = (SquadPhase)reader.ReadByte(); // Squad phase as byte
        float experience = reader.ReadSingle(); // Squad experience
        DateTime addedToCompanyAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Added to company at timestamp
        DateTime lastUpdatedAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Last updated at timestamp

        int totalInfantryKills = reader.ReadInt32(); // Total infantry kills
        int totalVehicleKills = reader.ReadInt32(); // Total vehicle kills
        int matchCounts = reader.ReadInt32(); // Match counts as a 64-bit integer

        ushort slotItemCount = reader.ReadUInt16(); // Number of slot items
        var slotItems = new IntermediateSlotItem[slotItemCount];
        for (int i = 0; i < slotItemCount; i++) {
            if (companyFileVersion < BinaryCompanySerializer.BINARY_COMPANY_VERSION_4) {
                int count = reader.ReadInt32(); // Item count
                byte itemType = reader.ReadByte(); // Item type (1 for Upgrade, 2 for SlotItem)
                if (itemType == 0x01) { // Upgrade item
                    slotItems[i] = new IntermediateSlotItem(count, ReadASCIIString(reader), null); // Upgrade Blueprint ID will always be ASCII
                                                                                                   // Add upgrade logic here
                } else if (itemType == 0x02) { // Slot item
                    slotItems[i] = new IntermediateSlotItem(count, null, ReadASCIIString(reader)); // Slot Item Blueprint ID will always be ASCII
                                                                                                   // Add slot item logic here
                } else {
                    throw new InvalidDataException($"Unknown item type: {itemType}");
                }
            } else {
                int itemId = reader.ReadInt32(); // Item ID
                slotItems[i] = new IntermediateSlotItem(itemId, null, null); // Entity ID will always be ASCII
            }
        }

        ushort upgradeCount = reader.ReadUInt16(); // Number of upgrades
        string[] upgrades = new string[upgradeCount];
        for (int i = 0; i < upgradeCount; i++) {
            upgrades[i] = ReadASCIIString(reader); // Upgrade Blueprint ID will always be ASCII
        }

        IntermediateTransportSquad transport = new IntermediateTransportSquad(false, null, false);
        IntermediatePassengerSquad passenger = new IntermediatePassengerSquad(false, -1);
        IntermediateCapturedItem captureItem = new IntermediateCapturedItem(false, -1, null, null);
        if (companyFileVersion < BinaryCompanySerializer.BINARY_COMPANY_VERSION_4) {

            bool hasTransport = reader.ReadByte() == (byte)0x1; // Transport squad flag
            if (hasTransport) {
                byte transportType = reader.ReadByte(); // Transport type (0 for regular, 1 for drop-off only)
                string transportBlueprintId = ReadASCIIString(reader); // Transport Blueprint ID will always be ASCII
                transport = new IntermediateTransportSquad(true, transportBlueprintId, transportType == 0x01);
            }

            if (companyFileVersion == BinaryCompanySerializer.BINARY_COMPANY_VERSION_3) {
                if (reader.ReadByte() == (byte)0x1) { // Passenger squad flag, added in version 3
                    int passengerSquadId = reader.ReadInt32(); // Passenger squad ID
                    passenger = new IntermediatePassengerSquad(true, passengerSquadId);
                }
            }

        } else {

            byte flags = reader.ReadByte(); // Flags for transport and passenger squads
            bool hasTransport = (flags & 0x01) != 0;
            bool hasPassenger = (flags & 0x02) != 0;
            bool isCapturedWeapon = (flags & 0x04) != 0; // Captured weapon flag, added in version 4

            if (hasTransport) {
                byte transportType = reader.ReadByte(); // Transport type (0 for regular, 1 for drop-off only)
                string transportBlueprintId = ReadASCIIString(reader); // Transport Blueprint ID will always be ASCII
                transport = new IntermediateTransportSquad(true, transportBlueprintId, transportType == 0x01);
            }

            if (hasPassenger) { // Passenger squad flag, added in version 3
                int passengerSquadId = reader.ReadInt32(); // Passenger squad ID
                passenger = new IntermediatePassengerSquad(true, passengerSquadId);
            }

            if (isCapturedWeapon) { // Captured weapon flag, added in version 4
                int capturedItemId = reader.ReadInt32(); // Captured item ID
                string capturedItemBlueprintId = ReadASCIIString(reader); // Captured item Blueprint ID will always be ASCII
                string capturedCrewBlueprintId = ReadASCIIString(reader); // Captured crew Blueprint ID will always be ASCII
                captureItem = new IntermediateCapturedItem(true, capturedItemId, capturedItemBlueprintId, capturedCrewBlueprintId);
            }

        }

        if (!_blueprintService.TryGetBlueprint(gameId, blueprintId, out SquadBlueprint? blueprint)) {
            return null;
        }

        Squad.SlotItem[] parsedSlotItems = new Squad.SlotItem[slotItems.Length];
        for (int i = 0; i < slotItems.Length; i++) {

            if (companyFileVersion < BinaryCompanySerializer.BINARY_COMPANY_VERSION_4) {

                if (!string.IsNullOrEmpty(slotItems[i].SlotItemId)) {
                    throw new NotImplementedException("SlotItemBlueprint handling is not implemented yet.");
                } else if (!string.IsNullOrEmpty(slotItems[i].EntityId)) {
                    if (_blueprintService.TryGetBlueprint(gameId, slotItems[i].EntityId!, out EntityBlueprint? itemEBP)) {
                        parsedSlotItems[i] = new Squad.SlotItem(slotItems[i].ItemId, itemEBP, null);
                    } else {
                        // Log
                        return null; // Return null if the upgrade blueprint is not found
                    }
                } else {
                    throw new InvalidDataException("Slot item must have either an UpgradeBlueprint or a SlotItemBlueprint.");
                }

            } else {
                parsedSlotItems[i] = new Squad.SlotItem(slotItems[i].ItemId, inventory.TryGetValue(slotItems[i].ItemId, out CapturedItem? ci) ? ci.ItemBlueprint : null, null);
            }

        }

        UpgradeBlueprint[] parsedUpgrades = new UpgradeBlueprint[upgrades.Length];
        for (int i = 0; i < upgrades.Length; i++) {
            if (_blueprintService.TryGetBlueprint(gameId, upgrades[i], out UpgradeBlueprint? upgrade)) {
                parsedUpgrades[i] = upgrade;
            } else {
                // Log the unknown upgrade blueprint
                return null; // Return null if the upgrade blueprint is not found
            }
        }

        Squad.TransportSquad? transportSquad = null;
        if (transport.Enabled) {
            if (_blueprintService.TryGetBlueprint(gameId, transport.BlueprintId!, out SquadBlueprint? transportBlueprint)) {
                transportSquad = new Squad.TransportSquad(transportBlueprint, transport.DropOffOnly);
            } else {
                // Log the unknown transport blueprint
                return null; // Return null if the transport blueprint is not found
            }
        }

        Squad.PassengerSquad? passengerSquad = null;
        if (passenger.Enabled) {
            passengerSquad = new Squad.PassengerSquad(passenger.PassengerSquadId);
        }

        Squad.CaptureInfo? captureInfo = null;
        if (captureItem.Enabled) {
            var foundWeapon = _blueprintService.TryGetBlueprint(gameId, captureItem.WeaponBlueprintId!, out EntityBlueprint? capturedItemEBP);
            var foundCrew = _blueprintService.TryGetBlueprint(gameId, captureItem.CrewBlueprintId!, out SquadBlueprint? capturedCrewBP);
            if (foundWeapon && foundCrew) {
                captureInfo = new Squad.CaptureInfo(captureItem.Id, capturedItemEBP, capturedCrewBP);
            } else {
                // Log the unknown captured item blueprint
                return null; // Return null if the captured item blueprint is not found
            }
        }

        return new Squad {
            Id = squadId,
            Blueprint = blueprint,
            Name = customName,
            Experience = experience,
            Phase = phase,
            SlotItems = parsedSlotItems.ToList().AsReadOnly(),
            Upgrades = parsedUpgrades.ToList().AsReadOnly(),
            Transport = transportSquad,
            AddedToCompanyAt = addedToCompanyAt,
            LastUpdatedAt = lastUpdatedAt,
            MatchCounts = matchCounts,
            TotalInfantryKills = totalInfantryKills,
            TotalVehicleKills = totalVehicleKills,
            Passenger = passengerSquad,
            CapturedWeapon = captureInfo
        };

    }

    private static string ReadUtf8String(BinaryReader reader) {
        ushort length = reader.ReadUInt16();
        return Encoding.UTF8.GetString(reader.ReadBytes(length));
    }

    private static string ReadASCIIString(BinaryReader reader) {
        ushort length = reader.ReadUInt16();
        return Encoding.ASCII.GetString(reader.ReadBytes(length));
    }

}
