using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events;

public class SurrenderEvent : IMatchEvent {

    public char Identifier => 'S';

    public uint Uid { get; }

    public Player Player { get; }

    public SurrenderEvent(uint eventID, Player player) {
        this.Uid = eventID;
        this.Player = player;
    }

}
