using Battlegrounds.Services;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views.Modals;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Helpers;

public enum DialogType {
    Confirm,
    YesNo,
    YesNoCancel,
}

public sealed class DialogModal(IServiceProvider serviceProvider) {

    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    private static DialogModal GetDefaultModel() {
        return new DialogModal(BattlegroundsApp.Instance!.ServiceProvider!);
    }

    public async Task<DialogResult> CreateAndShowModalAsync(DialogType dialogType = DialogType.Confirm, string? header = null, string? description = null) {
        DialogModalView view = _serviceProvider.GetRequiredService<DialogModalView>();
        if (view.DataContext is not DialogModalViewModel viewModel) {
            return default!;
        }
        viewModel.SetType(dialogType, header, description);
        IDialogService dialogService = _serviceProvider.GetRequiredService<IDialogService>();
        return await dialogService.ShowDialogAsync<DialogResult>(view);
    }

    public static Task<DialogResult> ShowModalAsync(DialogType dialogType = DialogType.Confirm, string? header = null, string? description = null) {
        return GetDefaultModel().CreateAndShowModalAsync(dialogType, header, description);
    }

}
