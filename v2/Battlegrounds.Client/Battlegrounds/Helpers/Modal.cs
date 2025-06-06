using Battlegrounds.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Helpers;

public static class Modal {

    public static async Task<TResult> ShowModalAsync<TModal, TResult>(IServiceProvider serviceProvider) where TModal : DialogUserControl {
        if (serviceProvider is not null) {
            TModal modal = serviceProvider.GetRequiredService<TModal>();
            return await serviceProvider.GetRequiredService<IDialogService>().ShowDialogAsync<TResult>(modal);
        }
        throw new ArgumentNullException(nameof(serviceProvider));
    }

}
