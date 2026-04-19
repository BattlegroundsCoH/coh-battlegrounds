using System.ComponentModel;
using System.Windows.Input;

using Battlegrounds.Helpers;
using Battlegrounds.Models.Doctrines;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.Modals;

public record FixDoctrineParameters(bool Confirmed, DoctrineDefinition? Doctrine);

public sealed class FixDoctrineModalViewModel : INotifyModalDone, INotifyPropertyChanged {

    public event ModalDoneEventHandler? ModalDone;
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly RelayCommand _confirmCommand;
    private readonly RelayCommand _cancelCommand;
    private readonly IDoctrineService _doctrineService;

    private ICollection<DoctrineDefinition> _availableDoctrines = [];
    private DoctrineDefinition? _selectedDoctrine;
    private string _companyName = string.Empty;

    public string CompanyName {
        get => _companyName;
        private set {
            if (_companyName == value) return;
            _companyName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompanyName)));
        }
    }

    public ICollection<DoctrineDefinition> AvailableDoctrines {
        get => _availableDoctrines;
        private set {
            if (_availableDoctrines == value) return;
            _availableDoctrines = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableDoctrines)));
        }
    }

    public DoctrineDefinition? SelectedDoctrine {
        get => _selectedDoctrine;
        set {
            if (_selectedDoctrine == value) return;
            _selectedDoctrine = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDoctrine)));
            _confirmCommand.NotifyCanExecuteChanged();
        }
    }

    public ICommand ConfirmCommand => _confirmCommand;
    public ICommand CancelCommand => _cancelCommand;

    public FixDoctrineModalViewModel(IDoctrineService doctrineService) {
        _doctrineService = doctrineService;
        _confirmCommand = new RelayCommand(OnConfirm, () => SelectedDoctrine is not null);
        _cancelCommand = new RelayCommand(OnCancel);
    }

    /// <summary>
    /// Initialises the modal with the company context. Must be called before the modal is shown.
    /// </summary>
    public void SetContext(Game game, string faction, string companyName) {
        CompanyName = companyName;
        AvailableDoctrines = [.. _doctrineService.GetDoctrinesForFaction(game.Id, faction).Where(x => x.IsVisible)];
        SelectedDoctrine = AvailableDoctrines.FirstOrDefault();
    }

    private void OnConfirm() {
        ModalDone?.Invoke(this, new FixDoctrineParameters(true, SelectedDoctrine));
    }

    private void OnCancel() {
        ModalDone?.Invoke(this, new FixDoctrineParameters(false, null));
    }

}
