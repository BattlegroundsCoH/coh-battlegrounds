namespace Battlegrounds.Services;

/// <summary>
/// Asks Windows to put the launcher back in front of the user.
/// </summary>
public interface IWindowActivationService {

    /// <summary>
    /// Brings the main window forward, restoring it first if it was minimised.
    /// </summary>
    void Activate();

}
