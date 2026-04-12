using System.IO;
using System.Text;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Serializers;

public sealed class BinaryCompanySerializer : ICompanySerializer {

    public static readonly uint BINARY_COMPANY_VERSION_1 = 1;
    public static readonly uint BINARY_COMPANY_VERSION_2 = 2;
    public static readonly uint BINARY_COMPANY_VERSION_3 = 3;
    public static readonly uint BINARY_COMPANY_VERSION_4 = 4;
    public static readonly uint BINARY_COMPANY_VERSION_LATEST = BINARY_COMPANY_VERSION_4;
    public static readonly byte[] BINARY_COMPANY_HEADER = [0x42, 0x47, 0x43, 0x0]; // "BGC" in ASCII

    public void SerializeCompany(Stream destination, Company company) {
        using var writer = new BinaryWriter(destination, Consts.UTF8, true);

        writer.Write(BINARY_COMPANY_HEADER);
        writer.Write(BINARY_COMPANY_VERSION_LATEST);

        writer.Write(DateTime.Now.Ticks); // Timestamp for serialization
        writer.Write(company.CreatedAt.Ticks); // Created at timestamp
        WriteUtf8String(writer, company.CreatedBy); // Created by can be UTF-8
        writer.Write(company.UpdatedAt.Ticks); // Updated at timestamp
        WriteUtf8String(writer, company.UpdatedBy); // Updated by can be UTF-8

        writer.Write(company.Version); // Company version as integer

        WriteASCIIString(writer, company.Id); // Company ID will always be ASCII (And preferably of fixed length, but may change in the future)
        WriteUtf8String(writer, company.Name);

        writer.Write(company.GameId switch {
            CoH3.GameId => (byte)0x03, // CoH3
            _ => (byte)0x00 // Unknown game ID, default to 0x00
        });

        WriteASCIIString(writer, company.Faction); // Faction will always be ASCII

        WriteASCIIString(writer, company.DoctrineId ?? "faction_default"); // Doctrine ID will always be ASCII, but can be null (default to "faction_default")
        writer.Write(company.DoctrineVersion); // Doctrine version as integer

        var items = company.CapturedItems ?? [];
        writer.Write((uint)items.Count);
        for (int i = 0; i < items.Count; i++) {
            WriteCapturedItem(writer, items[i]);
        }

        var squads = company.Squads ?? [];
        writer.Write((uint)squads.Count); // Number of squads
        for (int i = 0; i < squads.Count; i++) {
            WriteSquad(writer, squads[i]);
        }

    }

    private static void WriteCapturedItem(BinaryWriter writer, CapturedItem capturedItem) {

        writer.Write(capturedItem.Id); // Captured item ID
        WriteASCIIString(writer, capturedItem.ItemBlueprint?.Id ?? string.Empty); // Item Blueprint ID will always be ASCII (can be null)
        writer.Write(capturedItem.CapturedBySquadId); // Captured by squad ID
        writer.Write(capturedItem.CapturedAt.Ticks); // Captured at timestamp

    }

    private static void WriteSquad(BinaryWriter writer, Squad squad) {
        writer.Write(squad.Id); // Squad ID
        WriteASCIIString(writer, squad.Blueprint.Id); // Squad Blueprint ID will always be ASCII
        WriteUtf8String(writer, squad.Name); // Squad name can be UTF-8
        writer.Write((byte)squad.Phase); // Squad phase as byte
        writer.Write(squad.Experience); // Squad experience
        writer.Write(squad.AddedToCompanyAt.Ticks); // Added to company at timestamp
        writer.Write(squad.LastUpdatedAt.Ticks); // Last updated at timestamp

        // Write stats
        writer.Write(squad.TotalInfantryKills); // Total infantry kills
        writer.Write(squad.TotalVehicleKills); // Total vehicle kills
        writer.Write(squad.MatchCounts); // Match counts as a 64-bit integer

        // Write slot items
        writer.Write((ushort)squad.SlotItems.Count); // Number of slot items
        for (int i = 0; i < squad.SlotItems.Count; i++) {
            var item = squad.SlotItems[i];
            writer.Write(item.Count); // Item count
            if (item.EntityBlueprint is EntityBlueprint entity) {
                writer.Write((byte)0x01); // Entity item type
                WriteASCIIString(writer, entity.Id); // Entity Blueprint ID will always be ASCII
            } else if (item.SlotItemBlueprint is SlotItemBlueprint slotItem) {
                writer.Write((byte)0x02); // Slot item type
                WriteASCIIString(writer, slotItem.Id); // Slot Item Blueprint ID will always be ASCII
            } else {
                throw new InvalidDataException("Slot item must have either an EntityBlueprint or a SlotItemBlueprint.");
            }
        }

        // Write upgrades
        writer.Write((ushort)squad.Upgrades.Count); // Number of upgrades
        for (int i = 0; i < squad.Upgrades.Count; i++) {
            WriteASCIIString(writer, squad.Upgrades[i].Id); // Upgrade Blueprint ID will always be ASCII
        }

        // Write data flags
        byte dataFlags = 0;
        dataFlags |= (byte)(squad.HasTransport ? 0x01 : 0x00); // Bit 0 indicates presence of transport
        dataFlags |= (byte)(squad.HasPassenger ? 0x02 : 0x00); // Bit 1 indicates presence of passenger
        dataFlags |= (byte)(squad.IsCapturedWeapon ? 0x04 : 0x00); // Bit 2 indicates if this squad has a captured weapon
        writer.Write(dataFlags);

        // Write transport if any
        if (squad.HasTransport) {
            var transport = squad.Transport ?? throw new InvalidDataException("Transport must not be null if HasTransport is true.");
            writer.Write(transport.DropOffOnly switch {
                true => (byte)0x01, // Drop-off only transport
                false => (byte)0x00 // Regular transport
            });
            WriteASCIIString(writer, transport.TransportBlueprint.Id); // Transport Blueprint ID will always be ASCII
        }

        // Write passenger if any
        if (squad.HasPassenger) {
            writer.Write(squad.Passenger.PassengerSquadId); // Passenger squad ID
        }

        // Write captured weapon if any
        if (squad.IsCapturedWeapon) {
            var weapon = squad.CapturedWeapon ?? throw new InvalidDataException("Captured weapon must not be null if IsCapturedWeapon is true.");
            writer.Write(weapon.CompanyItemId); // Company item ID for the captured weapon
            WriteASCIIString(writer, weapon.WeaponEntityBlueprint?.Id ?? string.Empty); // Weapon Entity Blueprint ID will always be ASCII (can be null)
            WriteASCIIString(writer, weapon.CrewBlueprint?.Id ?? string.Empty); // Crew Blueprint ID will always be ASCII (can be null)
        }

    }

    private static void WriteASCIIString(BinaryWriter writer, string value) {
        if (value is null) {
            writer.Write((ushort)0);
            return;
        }
        var bytes = Encoding.ASCII.GetBytes(value);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    private static void WriteUtf8String(BinaryWriter writer, string value) {
        if (value is null) {
            writer.Write((ushort)0);
            return;
        }
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

}
