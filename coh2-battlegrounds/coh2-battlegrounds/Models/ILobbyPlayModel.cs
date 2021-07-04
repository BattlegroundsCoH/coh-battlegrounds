namespace BattlegroundsApp.Models {

    public delegate void PlayCancelHandler();

    public interface ILobbyPlayModel {

        bool CanCancel { get; }

        void PlayGame(PlayCancelHandler cancelHandler);

        void CancelGame();

    }

}
