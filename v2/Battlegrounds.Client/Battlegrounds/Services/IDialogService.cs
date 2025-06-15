using Battlegrounds.Helpers;

namespace Battlegrounds.Services;

public interface IDialogService {

    Task<T> ShowDialogAsync<T>(DialogUserControl content);

    void RegisterHost(IDialogHost host);

}
