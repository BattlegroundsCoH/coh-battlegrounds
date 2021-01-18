using System.Windows;
using Battlegrounds;
using BattlegroundsApp.Controls.Lobby;

namespace BattlegroundsApp.Views.ViewComponent {

    /// <summary>
    /// Interaction logic for PlayerCardOccupiedState.xaml
    /// </summary>
    public class PlayerCardState : LobbyControlState {

        public static readonly DependencyProperty StateNameProperty = DependencyProperty.Register("StateName", typeof(string), typeof(PlayerCardState));

        public string StateName { get => this.GetValue(StateNameProperty) as string; set => this.SetValue(StateNameProperty, value); }

        public ulong SteamID { get; private set; }

        public bool IsLocalUser { get; private set; }

        public bool IsAI { get; private set; }

        public bool IsHost { get; private set; }

        public override bool IsCorrectState(LobbyControlContext context) => true;

        public override void SetStateIdentifier(ulong ownerID, bool isAI) {
            this.SteamID = ownerID;
            this.IsLocalUser = BattlegroundsInstance.IsLocalUser(this.SteamID);
            this.IsAI = isAI;
        }

        public void SetStateIdentifier(ulong ownerID, bool isAI, bool isHost) {
            this.IsHost = isHost;
            this.SetStateIdentifier(ownerID, isAI);
        }

        public override void StateOnFocus() {}
        
        public override void StateOnLostFocus() {}

    }

}
