using System.IO;
using System.Text;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

namespace Battlegrounds.Serializers;

public sealed class BinaryCompanyDeserializer(IBlueprintService blueprintService) : ICompanyDeserializer {
    
    private readonly IBlueprintService _blueprintService = blueprintService;

    private static readonly uint[] SUPPORTED_VERSIONS = [BinaryCompanySerializer.BINARY_COMPANY_VERSION];

    public bool IgnoreUnknownSquads { get; set; } = true; // Ignore squads that are not recognized by the serializer instead of throwing an exception.

    public bool IsSupportedVersion(uint version) => SUPPORTED_VERSIONS.Contains(version);

    public Company DeserializeCompany(Stream source) {
        using var reader = new BinaryReader(source, Encoding.UTF8, true);

        var header = reader.ReadBytes(4);
        if (header[0] != 0x42 || header[1] != 0x47 || header[2] != 0x43 || header[3] != 0x00) {
            throw new InvalidDataException("Invalid company file header.");
        }

        var version = reader.ReadUInt32();
        if (!IsSupportedVersion(version)) {
            throw new InvalidDataException($"Unsupported company file version: {version}"); // Current impl, only supports version 1. Add support for more versions in the future.
        }

        var timestamp = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Timestamp for serialization
        var createdAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Created at timestamp
        var updatedAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc); // Updated at timestamp

        string id = ReadASCIIString(reader); // Company ID will always be ASCII
        string name = ReadUtf8String(reader); // Company name can be UTF-8

        byte gameIdByte = reader.ReadByte();
        string gameId = gameIdByte switch {
            0x03 => CoH3.GameId, // CoH3
            _ => throw new InvalidDataException($"Unknown game ID: {gameIdByte}") // Unknown game ID
        };

        string faction = ReadASCIIString(reader); // Faction will always be ASCII

        uint squadCount = reader.ReadUInt32(); // Number of squads
        var squads = new List<Squad>((int)squadCount);
        for (int i = 0; i < squadCount; i++) {
            if (ReadSquad(gameId, reader) is Squad sq) {
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
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Squads = squads
        };

    }

    private readonly record struct IntermediateSlotItem(int Count, string? UpgradeId, string? SlotItemId);
    private readonly record struct IntermediateTransportSquad(bool Enabled, string? BlueprintId, bool DropOffOnly);

    private Squad? ReadSquad(string gameId, BinaryReader reader) {

        int squadId = reader.ReadInt32(); // Squad ID
        string blueprintId = ReadASCIIString(reader); // Squad Blueprint ID will always be ASCII

        string customName = ReadUtf8String(reader);

        SquadPhase phase = (SquadPhase)reader.ReadByte(); // Squad phase as byte
        float experience = reader.ReadSingle(); // Squad experience

        ushort slotItemCount = reader.ReadUInt16(); // Number of slot items
        var slotItems = new IntermediateSlotItem[slotItemCount];
        for (int i = 0; i < slotItemCount; i++) {
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
        }

        ushort upgradeCount = reader.ReadUInt16(); // Number of upgrades
        string[] upgrades = new string[upgradeCount];
        for (int i = 0; i < upgradeCount; i++) {
            upgrades[i] = ReadASCIIString(reader); // Upgrade Blueprint ID will always be ASCII
        }

        bool hasTransport = reader.ReadByte() == (byte)0x1; // Transport squad flag
        IntermediateTransportSquad transport = new IntermediateTransportSquad(false, null, false);
        if (hasTransport) {
            byte transportType = reader.ReadByte(); // Transport type (0 for regular, 1 for drop-off only)
            string transportBlueprintId = ReadASCIIString(reader); // Transport Blueprint ID will always be ASCII
            transport = new IntermediateTransportSquad(true, transportBlueprintId, transportType == 0x01);
        }

        if (!_blueprintService.TryGetBlueprint(gameId, blueprintId, out SquadBlueprint? blueprint)) {
            return null;
        }

        Squad.SlotItem[] parsedSlotItems = new Squad.SlotItem[slotItems.Length];
        for (int i = 0; i < slotItems.Length; i++) {
            if (!string.IsNullOrEmpty(slotItems[i].SlotItemId)) {
                throw new NotImplementedException("SlotItemBlueprint handling is not implemented yet.");
            } else if (!string.IsNullOrEmpty(slotItems[i].UpgradeId)) {
                if (_blueprintService.TryGetBlueprint(gameId, slotItems[i].UpgradeId!, out UpgradeBlueprint? upgrade)) {
                    parsedSlotItems[i] = new Squad.SlotItem(slotItems[i].Count, upgrade, null);
                } else {
                    // Log
                    return null; // Return null if the upgrade blueprint is not found
                }
            } else {
                throw new InvalidDataException("Slot item must have either an UpgradeBlueprint or a SlotItemBlueprint.");
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

        return new Squad {
            Id = squadId,
            Blueprint = blueprint,
            Name = customName,
            Experience = experience,
            Phase = phase,
            SlotItems = parsedSlotItems.ToList().AsReadOnly(),
            Upgrades = parsedUpgrades.ToList().AsReadOnly(),
            Transport = transportSquad
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
