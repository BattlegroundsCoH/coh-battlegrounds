namespace BattlegroundsApp.MVVM {
    
    public interface IViewModel {

        bool SingleInstanceOnly { get; }

        bool KeepAlive { get; }

        bool UnloadViewModel();

        void Swapback();

    }

}
