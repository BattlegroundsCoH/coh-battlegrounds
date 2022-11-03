using System.ComponentModel;
using System.Windows.Input;

using Battlegrounds.UI.Modals;
using Battlegrounds.UI;

namespace Battlegrounds.Lobby.Modals;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="result"></param>
public delegate void JoinLobbyCallback(JoinLobby sender, ModalDialogResult result);

/// <summary>
/// 
/// </summary>
public sealed class JoinLobby : INotifyPropertyChanged {

    private string? _password;

    /// <summary>
    /// 
    /// </summary>
    public string Password {
        get {
            return this._password ?? string.Empty;
        }

        set {
            this._password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// 
    /// </summary>
    public ICommand JoinCommand { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand CancelCommand { get; }

    private JoinLobby(JoinLobbyCallback resultCallback) {

        this.JoinCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="resultCallback"></param>
    public static void Show(JoinLobbyCallback resultCallback) {

        // Grab control
        var control = AppContext.GetModalControl();

        // Create dialog view model
        JoinLobby dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
