using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

using Battlegrounds.Factories;
using Battlegrounds.Helpers;
using Battlegrounds.Models.Companies;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.CompanyHelpers;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views.Modals;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed class CompanyBrowserViewModel : INotifyPropertyChanged {

    private readonly ILogger<CompanyBrowserViewModel> _logger;
    private readonly ICompanyService _companyService;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<GameGroup> GameGroups { get; } = [];
    public ObservableCollection<Company> SelectedCompanies { get; } = [];

    private Company? _selectedCompany;
    private object? _selectedValuePath = null;

    public Company? SelectedCompany {
        get => _selectedCompany;
        set {
            _selectedCompany = value;
        }
    }

    private bool _isBusy;
    public bool IsBusy {
        get => _isBusy;
        set {
            _isBusy = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
        }
    }

    private bool _isSyncing;
    public bool IsSyncing {
        get => _isSyncing;
        set {
            _isSyncing = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSyncing)));
        }
    }

    private string _statusMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusMessage {
        get => _statusMessage;
        set {
            _statusMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
        }
    }

    // Commands
    public ICommand LoadCompaniesCommand { get; }
    public IRelayCommand<Company> SelectCompanyCommand { get; }
    public IAsyncRelayCommand CreateCompanyCommand { get; }
    public ICommand DeleteCompanyCommand { get; }
    public ICommand EditCompanyCommand { get; }
    public ICommand SyncCompanyCommand { get; }
    public ICommand SyncAllCompaniesCommand { get; }

    public CompanyBrowserViewModel(ILogger<CompanyBrowserViewModel> logger, ICompanyService companyService, IServiceProvider serviceProvider) {
        _logger = logger;
        _companyService = companyService;
        _serviceProvider = serviceProvider;

        // Initialize commands
        LoadCompaniesCommand = new RelayCommand(async () => await LoadCompaniesAsync());
        SelectCompanyCommand = new RelayCommand<Company>(company => SelectedCompany = company);
        CreateCompanyCommand = new AsyncRelayCommand(CreateCompany);
        DeleteCompanyCommand = new RelayCommand(async () => await DeleteCompanyAsync(), CanDeleteCompany);
        EditCompanyCommand = new RelayCommand(EditCompany, CanEditCompany);
        SyncCompanyCommand = new RelayCommand(async () => await SyncCompanyAsync(), CanSyncCompany);
        SyncAllCompaniesCommand = new RelayCommand(async () => await SyncAllCompaniesAsync());

        // Load companies when view model is created
        LoadCompaniesCommand.Execute(null);
    }

    private async Task SyncAllCompaniesAsync() {
        throw new NotImplementedException();
    }

    private bool CanSyncCompany() => false;

    private async Task SyncCompanyAsync() {
        throw new NotImplementedException();
    }

    private bool CanEditCompany() => false;

    public void EditCompany(Company company) {
        if (company == null) return;
        var mainWindow = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.SetContent(CompanyEditorViewFactory.CreateCompanyEditorView(_serviceProvider, company));
    }

    private void EditCompany() {
        throw new NotImplementedException();
    }

    private async Task LoadCompaniesAsync() {
        try {
            IsBusy = true;
            StatusMessage = "Loading companies...";

            // Load companies from local storage
            int loaded = await _companyService.LoadPlayerCompaniesAsync();
            if (loaded == 0) {
                StatusMessage = "No companies found";
                return;
            }

            var companies = await _companyService.GetLocalCompaniesAsync();

            // Group by game type and faction
            GameGroups.Clear();
            var groupedCompanies = companies
                .GroupBy(c => c.GameId)
                .Select(g => new GameGroup(g.Key, [.. g]));

            foreach (var group in groupedCompanies) {
                GameGroups.Add(group);
            }

            StatusMessage = $"Loaded {companies.Count()} companies";
        } catch (Exception ex) {
            _logger.LogError(ex, "Error loading companies");
            StatusMessage = "Error loading companies";
        } finally {
            IsBusy = false;
        }
    }

    private async Task CreateCompany() {

        var result = await Modal.ShowModalAsync<CreateCompanyModalView, CreateCompanyParameters>(_serviceProvider);
        if (result is null || !result.Create) {
            return;
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.SetContent(CompanyEditorViewFactory.CreateCompanyEditorView(_serviceProvider, result));

    }

    private bool CanDeleteCompany() {
        return SelectedCompany != null;
    }

    private async Task DeleteCompanyAsync() {
        if (SelectedCompany == null)
            return;

        string companyName = SelectedCompany.Name ?? "Unknown Company";

        try {
            IsBusy = true;
            StatusMessage = $"Deleting company {companyName}...";

            bool result = await _companyService.DeleteCompany(SelectedCompany.Id);

            if (result) {
                StatusMessage = $"Company {companyName} deleted successfully";
                // Refresh the company list
                await LoadCompaniesAsync();
                SelectedCompany = null;
            } else {
                StatusMessage = $"Failed to delete company {companyName}";
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting company");
            StatusMessage = $"Error deleting company {companyName}";
        } finally {
            IsBusy = false;
        }
    }

}
