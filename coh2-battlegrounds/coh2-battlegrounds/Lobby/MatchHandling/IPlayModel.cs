using Battlegrounds.Modding;

namespace BattlegroundsApp.Lobby.MatchHandling;

delegate void PrepareOverHandler(IPlayModel model);

delegate void PrepareCancelledHandler(IPlayModel model);

delegate void PlayOverHandler(IPlayModel model);

internal interface IPlayModel {

    void Prepare(ModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelledHandler prepareCancelled);

    void Play(PlayOverHandler matchOver);

}
