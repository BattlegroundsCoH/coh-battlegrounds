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
    public IAsyncRelayCommand CreateCompanyCommand { get; }
    public IAsyncRelayCommand<Company> DeleteCompanyCommand { get; }
    public IRelayCommand<Company> EditCompanyCommand { get; }

    public CompanyBrowserViewModel(ILogger<CompanyBrowserViewModel> logger, ICompanyService companyService, IServiceProvider serviceProvider) {
        _logger = logger;
        _companyService = companyService;
        _serviceProvider = serviceProvider;

        // Initialize commands
        LoadCompaniesCommand = new RelayCommand(async () => await LoadCompaniesAsync());
        CreateCompanyCommand = new AsyncRelayCommand(CreateCompany);
        DeleteCompanyCommand = new AsyncRelayCommand<Company>(DeleteCompanyAsync);
        EditCompanyCommand = new RelayCommand<Company>(EditCompany);

        // Load companies when view model is created
        LoadCompaniesCommand.Execute(null);
    }

    public void EditCompany(Company? company) {
        if (company == null) 
            return;
        var mainWindow = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.SetContent(CompanyEditorViewFactory.CreateCompanyEditorView(_serviceProvider, company));
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

    private async Task DeleteCompanyAsync(Company? companyy) {
        if (companyy is null)
            return;

        if (await DialogModal.ShowModalAsync(DialogType.YesNo, "Delete Company", $"Are you sure you want to delete the company '{companyy.Name}'?") != DialogResult.Yes) {
            return;
        }

        try {
            IsBusy = true;
            StatusMessage = "Deleting company...";
            bool success = await _companyService.DeleteCompany(companyy.Id);
            if (success) {
                StatusMessage = $"Company '{companyy.Name}' deleted successfully";
                await LoadCompaniesAsync(); // Refresh the list
            } else {
                StatusMessage = $"Failed to delete company '{companyy.Name}'";
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting company");
            StatusMessage = $"Error deleting company '{companyy.Name}'";
        } finally {
            IsBusy = false;
        }

    }

}
