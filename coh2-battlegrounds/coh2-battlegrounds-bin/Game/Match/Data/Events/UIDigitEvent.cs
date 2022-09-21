namespace Battlegrounds.Game.Match.Data.Events;

/// <summary>
/// <see cref="IMatchEvent"/> implementation for handling digit events sent from the UI.
/// </summary>
public class UIDigitEvent : IMatchEvent {

    public char Identifier => ',';

    public uint Uid => 0;

    public UIDigitEvent() { }

}
