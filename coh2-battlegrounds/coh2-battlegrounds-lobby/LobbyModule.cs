using Battlegrounds.Lobby.Pages;
using Battlegrounds.UI;

namespace Battlegrounds.Lobby;

public class LobbyModule : IUIModule {
    
    public void RegisterMenuCallbacks(IMenuController controller) {
        controller.SetMenuCallback(MenuButton.Browser, this.OpenLobbyBrowser);   
    }

    public void RegisterViewFactories(IViewManager viewManager) {}

    private void OpenLobbyBrowser(AppViewManager appViewManager) {

        // Show lobby browser
        appViewManager.UpdateDisplay<LobbyBrowser>(AppDisplayTarget.Right, browser => {
            if (browser is LobbyBrowser vm) {
                vm.RefreshLobbies();
            }
        });

    }

}
