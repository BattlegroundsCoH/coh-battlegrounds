using Battlegrounds.Helpers;
using Battlegrounds.ViewModels.Modals;

namespace Battlegrounds.Services;

public interface IDialogService {

    Task<T> ShowDialogAsync<T>(DialogUserControl content);

    Task<DialogResult> ShowConfirmationAsync(DialogType type = DialogType.Confirm, string? header = null, string? description = null);

    void RegisterHost(IDialogHost host);

}
