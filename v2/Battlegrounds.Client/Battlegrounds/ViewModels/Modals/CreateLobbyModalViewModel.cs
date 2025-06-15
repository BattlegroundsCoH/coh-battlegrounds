using System.ComponentModel;
using System.Windows.Input;

using Battlegrounds.Helpers;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.Modals;

public record CreateLobbyParameters(bool Create, string Name, string? Password);

public sealed class CreateLobbyModalViewModel : INotifyModalDone, INotifyPropertyChanged {
    
    public event ModalDoneEventHandler? ModalDone;
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly RelayCommand _createCommand;
    private readonly RelayCommand _cancelCommand;

    private string _lobbyName = string.Empty;
    private string _lobbyPassword = string.Empty;

    public string LobbyName {
        get => _lobbyName;
        set {
            if (_lobbyName == value) {
                return;
            }
            _lobbyName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LobbyName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreate)));
            _createCommand.NotifyCanExecuteChanged();
        }
    }

    public string LobbyPassword {
        get => _lobbyPassword;
        set {
            if (_lobbyPassword == value) {
                return;
            }
            _lobbyPassword = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LobbyPassword)));
        }
    }

    public bool CanCreate => !string.IsNullOrWhiteSpace(LobbyName);

    public ICommand CreateCommand => _createCommand;
    public ICommand CancelCommand => _cancelCommand;

    public CreateLobbyModalViewModel() {
        _createCommand = new RelayCommand(OnCreate, () => CanCreate);
        _cancelCommand = new RelayCommand(OnCancel);
    }

    private void OnCreate() {
        ModalDone?.Invoke(this, new CreateLobbyParameters(true, LobbyName, string.IsNullOrWhiteSpace(LobbyPassword) ? null : LobbyPassword));
    }

    private void OnCancel() {
        ModalDone?.Invoke(this, new CreateLobbyParameters(false, string.Empty, null));
    }

}
