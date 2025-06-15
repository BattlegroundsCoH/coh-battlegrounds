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
        LobbyViewModel ParentContext) {


    private PickableCompany? _selectedCompany = null;
    private PickableAIDifficulty _selectedAIDifficulty = new PickableAIDifficulty(Slot.Difficulty);
    private string _companyId = Slot.CompanyId;

    public string GameId => ParentContext.GameId;
    
    public bool CanSetAIDifficulty => string.IsNullOrEmpty(Slot.ParticipantId) || Slot.Difficulty != AIDifficulty.HUMAN;
    
    public string DisplayName {
        get {
            if (Slot.Difficulty != AIDifficulty.HUMAN)
                return SelectedAIDifficulty.DisplayName;
            return UserName;
        }
    }
    
    public List<PickableAIDifficulty> AvailableAIDifficulties => [new PickableAIDifficulty(AIDifficulty.HUMAN), new(AIDifficulty.EASY), new(AIDifficulty.NORMAL), new(AIDifficulty.HARD), new(AIDifficulty.EXPERT)];
    
    public PickableAIDifficulty SelectedAIDifficulty {
        get => _selectedAIDifficulty;
        set {
            if (_selectedAIDifficulty == value)
                return;
            _selectedAIDifficulty = value;
            DifficultyCommand.Execute(value.Difficulty);
        }
    }

    public List<PickableCompany> AvailableCompanies {
        get {
            var companies = ParentContext.CompaniesByAlliance[Alliance].Select(x => new PickableCompany(false, false, x));
            var available = (IsAIPlayer ? companies.Append(new PickableCompany(false, true, null)) : companies).ToList();
            // WARNING : SIDE_EFFECT!!!!
            if (string.IsNullOrEmpty(_companyId) && IsAIPlayer) {
                SelectedCompany = available.FirstOrDefault(x => x.Company is not null) ?? new PickableCompany(true, false, null);
                _companyId = _selectedCompany?.Company?.Id ?? string.Empty;
            }
            if (available.Count == 0)
                return [new PickableCompany(true, false, null)];
            else
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
            var company = ParentContext.LobbyCompanies[_companyId];
            return new PickableCompany(false, false, company);
        }
        set {
            if (_selectedCompany == value)
                return;
            _selectedCompany = value;
            _companyId = _selectedCompany?.Company?.Id ?? string.Empty;
            SetCompanyCommand.Execute(value);
        }
    }

    public bool CanSetCompany => (ParentContext.IsHost && Slot.Difficulty != AIDifficulty.HUMAN && !Slot.Locked) || (Slot.ParticipantId == ParentContext.Model.GetLocalPlayerId());

    public bool CanKickOccupant => ParentContext.IsHost && Slot.Difficulty == AIDifficulty.HUMAN && !string.IsNullOrEmpty(Slot.ParticipantId) && Slot.ParticipantId != ParentContext.Model.GetLocalPlayerId();

}
