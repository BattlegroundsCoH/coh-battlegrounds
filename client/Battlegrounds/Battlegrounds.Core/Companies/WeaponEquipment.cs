using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public sealed class WeaponEquipment(ushort itemIndex, ushort capturerSquadIndex, IBlueprint blueprint) : IEquipment {

    public ushort ItemIndex => itemIndex;

    public ushort Capturer => capturerSquadIndex;

    public IBlueprint EquipmentBlueprint => blueprint;

}
