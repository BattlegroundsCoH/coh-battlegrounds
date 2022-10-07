using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.UI.Modals;

/// <summary>
/// Abstract class with base implementation of a <see cref="Modal"/> dialog control.
/// </summary>
public abstract class Modal : UserControl {

    private ModalControl? m_controller;

    /// <summary>
    /// Get the <see cref="Modals.ModalControl"/> instance in charge of handling the current <see cref="Modal"/> instance.
    /// </summary>
    public ModalControl? ModalControl => this.m_controller;

    public Modal() : base() {
        this.HorizontalAlignment = HorizontalAlignment.Center;
        this.VerticalAlignment = VerticalAlignment.Center;
    }

    public void DisplayModal(ModalControl modal) => this.m_controller = modal;

    protected virtual void OnModalClosing(ModalCloseEventArgs closeArgs) { }

    public void CloseModal() {

        // Create close args
        ModalCloseEventArgs closeArgs = new();
        this.OnModalClosing(closeArgs);

        // Make sure we're supposed to cancel.
        if (!closeArgs.Cancel) {

            // Trigger the modal close
            this.ModalControl?.CloseModal();

        }

    }

}
