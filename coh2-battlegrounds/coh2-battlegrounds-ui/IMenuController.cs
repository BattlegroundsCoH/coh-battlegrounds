namespace Battlegrounds.UI;

public enum MenuButton {
    Dashboard,
    News,
    Companies,
    Campaigns,
    Browser,
    Settings
}

public delegate void MenuButtonCallback(AppViewManager manager);

/// <summary>
/// 
/// </summary>
public interface IMenuController {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="button"></param>
    /// <param name="callback"></param>
    void SetMenuCallback(MenuButton button, MenuButtonCallback callback);

}
