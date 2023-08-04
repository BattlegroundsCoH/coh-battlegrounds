namespace Battlegrounds.Resources;

/// <summary>
/// 
/// </summary>
public sealed class DefaultIconSource : IIconSource {

    /// <inheritdoc/>
    public string Container { get; }

    /// <inheritdoc/>
    public string Identifier { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    /// <param name="identifier"></param>
    public DefaultIconSource(string container, string identifier) {
        Container = container;
        Identifier = identifier;
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
        HashCode hc = new HashCode();
        hc.Add(Container);
        hc.Add(Identifier);
        return hc.ToHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is DefaultIconSource other && this.Container == other.Container && this.Identifier == other.Identifier;

}
