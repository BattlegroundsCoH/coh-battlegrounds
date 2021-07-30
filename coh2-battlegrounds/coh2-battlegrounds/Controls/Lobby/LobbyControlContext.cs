namespace BattlegroundsApp.Controls.Lobby {

    public struct LobbyControlContext {

        public bool IsHost { get; init; }

        public bool IsAI { get; init; }

        public ulong ClientID { get; init; }

    }

}
