namespace Battlegrounds.App.Providers.Lobby;

public sealed class ToggleProvider : ISkirmishSettingsValueProvider {
    private static readonly ToggleProvider instance = new ToggleProvider();
    public static ToggleProvider Instance { get; private set; } = instance;
}
