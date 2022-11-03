namespace Battlegrounds.Misc.Values;

/// <summary>
/// 
/// </summary>
public readonly struct ProgressValue {

    /// <summary>
    /// 
    /// </summary>
    public float Value { get; }

    /// <summary>
    /// 
    /// </summary>
    public float Max { get; }

    /// <summary>
    /// 
    /// </summary>
    public float Progress => this.Value / this.Max;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="max"></param>
    public ProgressValue(float value, float max) {
        this.Value = value;
        this.Max = max;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
        => $"{this.Value:0.00}/{this.Max:0}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newProgress"></param>
    /// <returns></returns>
    public ProgressValue Update(float newProgress) => new ProgressValue(newProgress, this.Max);

}
