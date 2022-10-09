namespace Battlegrounds.UI.Modals;

public abstract class ModalBase {

    private ModalControl? m_controller;

    public void ShowModal(ModalControl controller) {
        this.m_controller = controller;
        this.OnModalShown();
    }

    protected virtual void OnModalShown() { }

    public void CloseModal() {
        this.m_controller?.CloseModal();
        if (this.m_controller is not null)
            this.OnModalClosed();
        this.m_controller = null;
    }

    protected virtual void OnModalClosed() { }

}
