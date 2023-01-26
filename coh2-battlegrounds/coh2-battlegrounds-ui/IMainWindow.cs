using System.Windows.Controls;

using Battlegrounds.UI.Modals;

namespace Battlegrounds.UI;

/// <summary>
/// Interface representing the functionalities of the main windows.
/// </summary>
public interface IMainWindow {

    /// <summary>
    /// Get the current display state of the window.
    /// </summary>
    AppDisplayState DisplayState { get; }

    /// <summary>
    /// Set the <see cref="IViewModel"/> instance to display in the left panel.
    /// </summary>
    /// <param name="lhs">The view model to display or <see langword="null"/> to clear the left view.</param>
    void SetLeftPanel(IViewModel? lhs);

    /// <summary>
    /// Set the <see cref="IViewModel"/> instance to display in the right panel.
    /// </summary>
    /// <param name="rhs">The view model to display or <see langword="null"/> to clear the right view.</param>
    void SetRightPanel(IViewModel? rhs);

    /// <summary>
    /// Set the <see cref="IViewModel"/> instance to display in one panel and collapsing the other panel.
    /// </summary>
    /// <param name="full">The view model to display or <see langword="null"/> to clear</param>
    void SetFull(IViewModel? full);

    /// <summary>
    /// Get the <see cref="ContentControl"/> that is the left side.
    /// </summary>
    /// <returns>The <see cref="ContentControl"/> representing the left panel.</returns>
    ContentControl GetLeft();

    /// <summary>
    /// Get the <see cref="ContentControl"/> that is the right side.
    /// </summary>
    /// <returns>The <see cref="ContentControl"/> representing the right panel.</returns>
    ContentControl GetRight();

    /// <summary>
    /// Get the window-wide <see cref="ModalControl"/> instance.
    /// </summary>
    /// <returns>The <see cref="ModalControl"/> covering the entire window or <see langword="null"/> if none is defined.</returns>
    ModalControl? GetModalControl();

    /// <summary>
    /// Get the right-panel <see cref="ModalControl"/> instance.
    /// </summary>
    /// <returns>The <see cref="ModalControl"/> covering the entire window or <see langword="null"/> if none is defined.</returns>
    ModalControl? GetRightsideModalControl();

}
