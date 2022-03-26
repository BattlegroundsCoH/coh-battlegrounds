namespace BattlegroundsApp.Utilities;

public struct ProgressValue {

    public float Value { get; }

    public float Max { get; }

    public float Progress => this.Value / this.Max;

    public ProgressValue(float value, float max) {
        this.Value = value;
        this.Max = max;
    }

    public override string ToString()
        => $"{this.Value:0.00}/{this.Max:0}";

}
