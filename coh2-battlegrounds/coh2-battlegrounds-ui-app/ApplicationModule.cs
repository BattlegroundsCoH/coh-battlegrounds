using Battlegrounds.UI.Application.Pages;

namespace Battlegrounds.UI.Application;

/// <summary>
/// 
/// </summary>
public sealed class ApplicationModule : IUIModule {

    public void RegisterMenuCallbacks(IMenuController controller) {
        controller.SetMenuCallback(MenuButton.Settings, this.OpenSettings);
        controller.SetMenuCallback(MenuButton.Dashboard, this.OpenDashboard);
    }

    private void OpenDashboard(AppViewManager appViewManager)
        => appViewManager.UpdateDisplay<Dashboard>(AppDisplayTarget.Right);

    private void OpenSettings(AppViewManager appViewManager)
        => appViewManager.UpdateDisplay<Settings>(AppDisplayTarget.Right);

    public void RegisterViewFactories(IViewManager viewManager) {}

}
