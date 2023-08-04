using System;
using System.Windows;

using Battlegrounds.Errors.Common;
using Battlegrounds.UI.Modals;

namespace Battlegrounds.UI;

/// <summary>
/// Static utility context class
/// </summary>
public static class AppContext {

    /// <summary>
    /// Get or set the application to retrieve context data from.
    /// </summary>
    public static Application MainApplication { get; set; } = Application.Current;

    /// <summary>
    /// Get the full view modal control.
    /// </summary>
    /// <returns>The <see cref="ModalControl"/> instance covering the entire main window.</returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static ModalControl GetModalControl() {
        if (MainApplication.MainWindow is IMainWindow window) {
            return window.GetModalControl() ?? throw new ObjectNotFoundException("Failed to find main modal control.");
        } else {
            throw new InvalidOperationException("Failed to get window object with modal control access.");
        }
    }

    /// <summary>
    /// Get the right-side modal control.
    /// </summary>
    /// <returns>The <see cref="ModalControl"/> instance covering the right side of the main window.</returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static ModalControl GetRightsideModalControl() {
        if (MainApplication.MainWindow is IMainWindow window) {
            return window.GetRightsideModalControl() ?? throw new ObjectNotFoundException("Failed to find main modal control.");
        } else {
            throw new InvalidOperationException("Failed to get window object with modal control access.");
        }
    }

    /// <summary>
    /// Get the <see cref="AppViewManager"/> responsible for handling the view state of the application.
    /// </summary>
    /// <returns>The <see cref="AppViewManager"/> instance associated with the main application.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static AppViewManager GetViewManager() {
        if (MainApplication is IViewController viewController) {
            return viewController.ViewManager;
        } else {
            throw new InvalidOperationException("Failed to get window object with view manager access.");
        }
    }

}
