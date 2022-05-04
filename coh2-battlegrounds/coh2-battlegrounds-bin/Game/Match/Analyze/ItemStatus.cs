using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Analyze;

public class ItemStatus {

    public Player PlayerOwner { get; }

    public Blueprint Blueprint { get; }

    public ItemStatus(Player player, Blueprint bp) {
        this.PlayerOwner = player;
        this.Blueprint = bp;
    }

}
