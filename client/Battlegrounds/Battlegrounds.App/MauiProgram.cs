using Battlegrounds.App.Services;
using Battlegrounds.Core.Configuration;
using Battlegrounds.Core.Services;
using Battlegrounds.Core.Services.Standard;
using Microsoft.Extensions.Logging;

namespace Battlegrounds.App;

public static class MauiProgram {

    public static MauiApp CreateMauiApp() {

        string cfgSource = "config.yml";
#if DEBUG
        cfgSource = "config-dev.yml";
#endif
        using var configFileSource = FileSystem.Current.OpenAppPackageFileAsync(cfgSource).Result;

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddLogging()
            .AddConfig(configFileSource)
            .AddSingleton(GrpcConfig.LobbyServiceClientFactory)
            .AddSingleton<PreferencesService>()
            .AddSingleton<ILobbyService, LobbyService>()
            .AddSingleton<IUserService, UserService>()
            .AddSingleton<IGameService, GameService>()
            .AddSingleton<IGamemodeService, GamemodeService>()
            .AddSingleton<ICompanyService, CompanyService>()
            .AddSingleton<IScenarioService, ScenarioService>()
            .AddSingleton<AppLoader>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

}
