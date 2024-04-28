namespace Battlegrounds.App.Providers.Lobby;

public sealed class RangeProvider(int min, int max) : ISkirmishSettingsValueProvider {

    public int Min => min;

    public int Max => max;

}
