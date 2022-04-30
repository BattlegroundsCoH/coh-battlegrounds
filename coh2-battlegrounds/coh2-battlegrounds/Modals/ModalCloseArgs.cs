using System;

namespace BattlegroundsApp.Modals;

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
