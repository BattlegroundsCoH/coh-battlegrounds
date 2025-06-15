using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Vanilla;
using Battlegrounds.UI;
using Battlegrounds.UI.Modals;

namespace Battlegrounds.Lobby.Modals;

public delegate void HostLobbyCallback(HostLobby lobby, ModalDialogResult result);

/// <summary>
/// 
/// </summary>
public sealed class HostLobby : INotifyPropertyChanged {

    public record LobbyGamePick(GameCase Game) {
        public override string ToString() => Game switch {
            GameCase.CompanyOfHeroes2 => "Company of Heroes 2",
            GameCase.CompanyOfHeroes3 => "Company of Heroes 3",
            _ => throw new InvalidEnumArgumentException(nameof(Game)),
        };
    }

    public record LobbyPackagePick(IModPackage Package) {
        public override string ToString() => Package.PackageName;
    }

    private string _lobbyName = $"{BattlegroundsContext.Steam.User.Name}'s Lobby";

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

    /// <summary>
    /// 
    /// </summary>
    public IModPackage LobbyPackage => Packages[SelectedPackageIndex].Package;

    /// <summary>
    /// 
    /// </summary>
    public GameCase LobbyGame => Games[SelectedGameIndex].Game;

    public int SelectedPackageIndex { get; set; } = 0;

    private int _gameIdx;

    public int SelectedGameIndex {
        get => _gameIdx;
        set {
            _gameIdx = value;
            this.RefreshPackages();
            SelectedPackageIndex = Packages.Count > 0 ? 0 : -1;
            OnPropertyChanged(nameof(SelectedPackageIndex));
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

    /// <summary>
    /// 
    /// </summary>
    public IList<LobbyGamePick> Games { get; }
    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<LobbyPackagePick> Packages { get; }

    private HostLobby(HostLobbyCallback resultCallback) {

        // Create button commands
        this.HostCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

        // Create games pick
        this.Games = new LobbyGamePick[] {
            new LobbyGamePick(GameCase.CompanyOfHeroes2),
            new LobbyGamePick(GameCase.CompanyOfHeroes3)
        };

        // Create package picks
        this.Packages = new();
        RefreshPackages();

    }

    private void RefreshPackages() {
        Packages.Clear();
        BattlegroundsContext.ModManager.GetPackages()
            .Where(x => x is not VanillaModPackage && x.SupportedGames.HasFlag(LobbyGame))
            .Select(x => new LobbyPackagePick(x))
            .ForEach(Packages.Add);
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
