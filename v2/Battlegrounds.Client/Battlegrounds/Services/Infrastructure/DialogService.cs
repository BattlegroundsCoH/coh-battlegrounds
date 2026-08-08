using Battlegrounds.Helpers;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views.Modals;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Services.Infrastructure;

public sealed class DialogService(IServiceProvider serviceProvider) : IDialogService {

    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private IDialogHost _dialogHost = null!;

    public void RegisterHost(IDialogHost host) {
        if (_dialogHost != null) {
            throw new InvalidOperationException("Dialog host is already registered.");
        }
        ArgumentNullException.ThrowIfNull(host);
        _dialogHost = host;
    }

    public async Task<T> ShowDialogAsync<T>(DialogUserControl content) {
        if (_dialogHost == null) {
            throw new InvalidOperationException("Dialog host is not registered.");
        }
        _dialogHost.PresentDialog(content);
        var result = await content.Await<T>();
        _dialogHost.CloseDialog();
        return result;
    }

    public async Task<DialogResult> ShowConfirmationAsync(DialogType type = DialogType.Confirm, string? header = null, string? description = null) {
        DialogModalView view = _serviceProvider.GetRequiredService<DialogModalView>();
        if (view.DataContext is not DialogModalViewModel viewModel) {
            return DialogResult.Cancel;
        }
        viewModel.SetType(type, header, description);
        return await ShowDialogAsync<DialogResult>(view);
    }

}
