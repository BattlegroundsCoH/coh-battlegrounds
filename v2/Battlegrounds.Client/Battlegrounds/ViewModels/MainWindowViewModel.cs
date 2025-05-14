using System.ComponentModel;
using System.Windows.Controls;

using Battlegrounds.Helpers;
using Battlegrounds.Services;
using Battlegrounds.Views;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.ViewModels;

public sealed class MainWindowViewModel : IDialogHost, INotifyPropertyChanged {

    private readonly IServiceProvider _serviceProvider;

    private object? _dialogContent = null;
    private UserControl? _currentContent = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MultiplayerView MultiplayerView => _serviceProvider.GetRequiredService<MultiplayerView>();

    public object? DialogContent {
        get => _dialogContent;
        private set {
            if (_dialogContent == value)
                return;
            _dialogContent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DialogContent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasDialog)));
        }
    }

    public bool HasDialog => _dialogContent != null;

    public UserControl? CurrentContent {
        get => _currentContent;
        set {
            if (_currentContent == value)
                return;
            _currentContent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentContent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasMainContent)));
        }
    }

    public bool HasMainContent => _currentContent != null;

    public MainWindowViewModel(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
        _serviceProvider.GetRequiredService<IDialogService>().RegisterHost(this);
    }

    public void PresentDialog(object dialog) {
        DialogContent = dialog;
    }

    public void CloseDialog() {
        DialogContent = null;
    }

    public void SetContent(UserControl view) {
        CurrentContent = view;
    }

}
