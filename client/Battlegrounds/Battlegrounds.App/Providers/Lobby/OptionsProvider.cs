namespace Battlegrounds.App.Providers.Lobby;

public sealed class OptionsProvider : ISkirmishSettingsValueProvider {

    public delegate IEnumerable<(string, string)> SourceFetch();

    private SourceFetch? sourceFetch;

    public IEnumerable<(string value, string text)> Values => sourceFetch?.Invoke() ?? Array.Empty<(string, string)>();

    public void SetOptionsSource(SourceFetch getter) {
        sourceFetch = getter;
    }

}
