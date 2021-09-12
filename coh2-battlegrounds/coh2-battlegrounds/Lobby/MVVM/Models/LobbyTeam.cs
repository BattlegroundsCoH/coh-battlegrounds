namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyTeam {
    
        public LobbySlot[] Slots { get; }

        public LobbyTeam() {
            this.Slots = new LobbySlot[] {
                new() { LeftDisplayString = "Enterprise", IsSelf = true },
                new() { LeftDisplayString = "Voyager" },
                new() { LeftDisplayString = "Open", IsOpen = true },
                new() { LeftDisplayString = "Locked", IsLocked = true }
            };
        }

    }

}
