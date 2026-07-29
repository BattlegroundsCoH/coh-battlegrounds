using System.ComponentModel;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.LobbyHelpers;

public sealed record LobbySlotViewModel(
        Team.Slot Slot,
        string UserName,
        string CompanyName,
        bool IsAIPlayer,
        FactionAlliance Alliance,
        IAsyncRelayCommand<AIDifficulty> DifficultyCommand,
        IAsyncRelayCommand<int> LockUnlockCommand,
        IAsyncRelayCommand<PickableCompany> SetCompanyCommand,
        IAsyncRelayCommand<int> MoveToSlotCommand,
        LobbyViewModel ParentContext) : INotifyPropertyChanged {

    private PickableCompany? _selectedCompany;
    private PickableCompany? _draftSelectedCompany;
    private readonly PickableAIDifficulty _selectedAIDifficulty = new(Slot.Difficulty);
    private PickableAIDifficulty _draftSelectedAIDifficulty = new(Slot.Difficulty);
    private string _companyId = Slot.CompanyId;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string GameId => ParentContext.GameId;

    public bool CanSetAIDifficulty => string.IsNullOrEmpty(Slot.ParticipantId) || Slot.Difficulty != AIDifficulty.HUMAN;

    public string DisplayName {
        get {
            if (Slot.Difficulty != AIDifficulty.HUMAN)
                return SelectedAIDifficulty.DisplayName;
            return UserName;
        }
    }

    public bool IsLocalPlayer => ParentContext.LocalParticipant == Slot.ParticipantId;

    public List<PickableAIDifficulty> AvailableAIDifficulties =>
        [new PickableAIDifficulty(AIDifficulty.HUMAN), new(AIDifficulty.EASY), new(AIDifficulty.NORMAL), new(AIDifficulty.HARD), new(AIDifficulty.EXPERT)];

    public PickableAIDifficulty SelectedAIDifficulty => _selectedAIDifficulty;

    public PickableAIDifficulty DraftSelectedAIDifficulty {
        get => _draftSelectedAIDifficulty;
        set {
            if (value is null || _draftSelectedAIDifficulty == value)
                return;
            _draftSelectedAIDifficulty = value;
            PropertyChanged?.Invoke(this, new(nameof(DraftSelectedAIDifficulty)));
            DifficultyCommand.NotifyCanExecuteChanged();
        }
    }

    public List<PickableCompany> AvailableCompanies {
        get {
            var companies = ParentContext.CompaniesByAlliance[Alliance].Select(x => new PickableCompany(false, false, x));
            var available = (IsAIPlayer ? companies.Append(new PickableCompany(false, true, null)) : companies).ToList();
            if (available.Count == 0)
                return [new PickableCompany(true, false, null)];
            return available;
        }
    }

    public PickableCompany SelectedCompany {
        get {
            if (_selectedCompany is not null) {
                return _selectedCompany;
            }
            if (string.IsNullOrEmpty(_companyId)) {
                return new PickableCompany(true, false, null);
            }
            if (ParentContext.GetCompany(_companyId) is Company company) {
                return new PickableCompany(false, false, company);
            }
            return new PickableCompany(true, false, null);
        }
    }

    public PickableCompany DraftSelectedCompany {
        get => _draftSelectedCompany ?? SelectedCompany;
        set {
            if (value is null || DraftSelectedCompany == value)
                return;
            _draftSelectedCompany = value;
            PropertyChanged?.Invoke(this, new(nameof(DraftSelectedCompany)));
            SetCompanyCommand.NotifyCanExecuteChanged();
        }
    }

    public LobbySlotViewModel WithServerCompany(Company company) {
        var updated = this with { };
        updated._selectedCompany = new PickableCompany(false, false, company);
        updated._draftSelectedCompany = updated._selectedCompany;
        updated._companyId = company.Id;
        return updated;
    }

    public bool HasOccupant => !string.IsNullOrEmpty(Slot.ParticipantId);

    public bool IsOccupiable => !HasOccupant && !Slot.Locked;

    public bool CanSetCompany => (ParentContext.IsHost && Slot.Difficulty != AIDifficulty.HUMAN && !Slot.Locked) || (Slot.ParticipantId == ParentContext.Model.GetLocalPlayerId());

    public bool CanKickOccupant => ParentContext.IsHost && Slot.Difficulty == AIDifficulty.HUMAN && !string.IsNullOrEmpty(Slot.ParticipantId) && Slot.ParticipantId != ParentContext.Model.GetLocalPlayerId();

    public IRelayCommand ShowCompanyPreviewCommand => new RelayCommand(() => ParentContext.ShowCompanyPreview(SelectedCompany?.Company));

    public float CompanyDownloadProgress { get; set; }

}
