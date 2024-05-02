using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies;

public interface IEquipment {

    ushort ItemIndex { get; }

    /// <summary>
    /// Get the index of the company squad that captured this item.
    /// </summary>
    ushort Capturer { get; }

    IBlueprint EquipmentBlueprint { get; }

}
