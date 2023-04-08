namespace Battlegrounds.Modding;

public class CustomTuning : ITuningMod {

    public string VerificationUpgrade { get; }

    public string TowUpgrade { get; }

    public string TowingUpgrade { get; }

    public ModGuid Guid { get; }

    public string Name { get; }

    public IModPackage Package { get; }

    public bool IsTowingEnabled { get; }

    public ModType GameModeType => ModType.Tuning;

    public CustomTuning(IModPackage package) {
        this.Package = package;
        this.Guid = package.TuningGUID;
        this.Name = package.PackageName;
        this.VerificationUpgrade = package.VerificationUpgrade;
        this.TowingUpgrade = package.IsTowingUpgrade;
        this.TowUpgrade = package.IsTowedUpgrade;
        this.IsTowingEnabled = package.IsTowEnabled;
    }

}
