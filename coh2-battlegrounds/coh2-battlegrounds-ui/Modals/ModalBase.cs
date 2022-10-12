namespace Battlegrounds.UI.Modals;

/// <summary>
/// Abstract class representing the view model modal base.
/// </summary>
public abstract class ModalBase {

    private ModalControl? m_controller;

    /// <summary>
    /// Show this modal in the specified <see cref="ModalControl"/>.
    /// </summary>
    /// <param name="controller">The associated modal controller.</param>
    public void ShowModal(ModalControl controller) {
        this.m_controller = controller;
        this.OnModalShown();
    }

    /// <summary>
    /// Method invoked when a modal is shown.
    /// </summary>
    protected virtual void OnModalShown() { }

    /// <summary>
    /// Instructs the <see cref="ModalControl"/> to close this modal.
    /// </summary>
    public void CloseModal() {
        this.m_controller?.CloseModal();
        if (this.m_controller is not null)
            this.OnModalClosed();
        this.m_controller = null;
    }

    /// <summary>
    /// Method invoked when a modal is closed.
    /// </summary>
    protected virtual void OnModalClosed() { }

}
