using System.Reflection;
using System.Diagnostics;

using Battlegrounds.Update;

namespace BattlegroundsApp;

public class AppVersionFetcher : IAppVersionFetcher {

    public string ApplicationVersion => GetApplicationVersion();

    public string GetApplicationVersion() {

        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

        return fvi.FileVersion ?? string.Empty;

    }

}
