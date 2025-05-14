using Battlegrounds.Helpers;

namespace Battlegrounds.Services;

public sealed class DialogService : IDialogService {

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

}
