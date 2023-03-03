using System.ComponentModel;
using System.Windows.Input;

using Battlegrounds.UI;
using Battlegrounds.UI.Modals;

namespace Battlegrounds.Lobby.Modals;

public delegate void HostLobbyCallback(HostLobby lobby, ModalDialogResult result);

/// <summary>
/// 
/// </summary>
public sealed class HostLobby : INotifyPropertyChanged {

    private string _lobbyName = $"{BattlegroundsInstance.Steam.User.Name}'s Lobby";

    /// <summary>
    /// 
    /// </summary>
    public string LobbyName {
        get => this._lobbyName;
        set {
            this._lobbyName = value;
            OnPropertyChanged(nameof(this.LobbyName));
        }
    }

    private string? _lobbyPassword;

    /// <summary>
    /// 
    /// </summary>
    public string LobbyPassword {
        get => this._lobbyPassword ?? string.Empty;
        set {
            this._lobbyPassword = value;
            this.OnPropertyChanged(nameof(this.LobbyPassword));
        }

    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    public void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// 
    /// </summary>
    public ICommand HostCommand { get; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand CancelCommand { get; }

    private HostLobby(HostLobbyCallback resultCallback) {

        this.HostCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    /// <param name="resultCallback"></param>
    public static void Show(HostLobbyCallback resultCallback) {

        // Grab control
        var control = AppContext.GetModalControl();

        // Create dialog view model
        HostLobby dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
