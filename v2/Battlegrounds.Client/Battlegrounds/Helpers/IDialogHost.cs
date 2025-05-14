namespace Battlegrounds.Helpers;

public interface IDialogHost {

    bool HasDialog { get; }

    void CloseDialog();
    void PresentDialog(object dialog);

}
