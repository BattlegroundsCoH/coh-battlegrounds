namespace Battlegrounds.Modding.Battlegrounds;

/// <summary>
/// The battlegrounds tuning mod. Implements <see cref="ITuningMod"/>. This class cannot be inherited.
/// </summary>
public sealed class BattlegroundsTuning : ITuningMod {

    public ModGuid Guid { get; }

    public string Name { get; }

    public string VerificationUpgrade { get; }

    public string TowUpgrade { get; }

    public string TowingUpgrade { get; }

    public ModPackage Package { get; }

    public bool IsTowingEnabled => true;

    public ModType GameModeType => ModType.Tuning;

    public BattlegroundsTuning(ModPackage package) {
        this.Guid = package.TuningGUID;
        this.Name = "Battlegrounds";
        this.VerificationUpgrade = package.VerificationUpgrade;
        this.TowUpgrade = package.IsTowedUpgrade;
        this.TowingUpgrade = package.IsTowingUpgrade;
        this.Package = package;
    }

}
