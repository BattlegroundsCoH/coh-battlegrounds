namespace BattlegroundsApp.MVVM {
    
    public interface IViewModel {

        bool SingleInstanceOnly { get; }

        bool UnloadViewModel();

    }

}
