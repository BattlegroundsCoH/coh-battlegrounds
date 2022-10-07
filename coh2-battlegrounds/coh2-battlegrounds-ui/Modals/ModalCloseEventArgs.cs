using System;

namespace Battlegrounds.UI.Modals;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="sender"></param>
/// <param name="success"></param>
/// <param name="value"></param>
public delegate void ModalDialogClosed<T>(object sender, bool success, T value) where T : Enum;

/// <summary>
/// Enum representing the result of a dialog close event.
/// </summary>
public enum ModalDialogResult {

    /// <summary>
    /// Dialog result is confirm
    /// </summary>
    Confirm,

    /// <summary>
    /// Dialog result is cancel
    /// </summary>
    Cancel

}

/// <summary>
/// Class represent a modal close event.
/// </summary>
public class ModalCloseEventArgs : EventArgs {

    /// <summary>
    /// Get or set if the modal close event should cancel.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Initialize a new <see cref="ModalCloseEventArgs"/> instance with the cancel property set to <see langword="false"/>.
    /// </summary>
    public ModalCloseEventArgs() {
        this.Cancel = false;
    }

}
