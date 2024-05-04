using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public sealed class VehicleEquipment(ushort itemIndex, ushort capturerSquadIndex, EntityBlueprint vehicleBlueprint) : IEquipment {

    public ushort ItemIndex => itemIndex;

    public ushort Capturer => capturerSquadIndex;

    public IBlueprint EquipmentBlueprint => vehicleBlueprint;

}
