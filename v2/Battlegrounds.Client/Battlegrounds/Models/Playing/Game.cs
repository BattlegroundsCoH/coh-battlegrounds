namespace Battlegrounds.Models.Playing;

public abstract class Game {

    public abstract string Id { get; }

    public abstract string GameName { get; }

    public abstract string AppExecutableFullPath { get; }

    public abstract string ArchiverExecutable { get; }

    public abstract string[] FactionIds { get; }

    public abstract FactionAlliance GetFactionAlliance(string factionId);

    public abstract string GetFactionName(string factionId);

}
