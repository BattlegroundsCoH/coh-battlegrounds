namespace Battlegrounds.Game.Match.Data.Events;

public class DebugEvent : IMatchEvent {

    public char Identifier => 'P';

    public uint Uid { get; }

    public string Message { get; }

    public DebugEvent(uint uid, string message) {
        this.Uid = uid;
        this.Message = message;
    }

}
