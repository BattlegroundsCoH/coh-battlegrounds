using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Battlegrounds.Update;

public static class Update {

	private static readonly Release _latestRelease = ProcessLatestRelease().Result;

	private static async Task<Release> ProcessLatestRelease() {

        HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Battlegrounds Mod Launcher");

		try {

            var streamTask = await httpClient.GetStreamAsync("https://api.github.com/repos/JustCodiex/coh2-battlegrounds/releases/latest");
            var release = JsonSerializer.Deserialize<Release>(streamTask);

            return release!;

        } catch (Exception e) {

			Trace.WriteLine(e, nameof(Update));

			return new Release();

		}

	}

	public static bool IsNewVersion() {

		var latestVersion = new Version(Regex.Replace(_latestRelease.TagName, @"[-]?[a-zA-Z]+", ""));
        var assemblyVersion = new Version(BattlegroundsContext.Version.ApplicationVersion);

		if (latestVersion.CompareTo(assemblyVersion) > 0) return true; 

        return false;

	}

	private static async void DownloadNewVersion(Action action) {

        HttpClient httpClient = new HttpClient();

        var downloadName = _latestRelease.Assets[0].Name;
		var downloadUrl = new Uri(_latestRelease.Assets[0].InstallerDownloadUrl, UriKind.Absolute);

		Trace.WriteLine("Starting download", nameof(Update));

		using var stream = await httpClient.GetStreamAsync(downloadUrl);
		using var fileStream = new FileStream($"{BattlegroundsContext.GetRelativePath(BattlegroundsPaths.UPDATE_FOLDER, Path.GetFileName(downloadName))}", FileMode.Create); 
		
		await stream.CopyToAsync(fileStream);

        Trace.WriteLine("Finished download", nameof(Update));

		action.Invoke();

    }

	private static void RunInstallMSI() {

        ProcessStartInfo msiexec_bin = new ProcessStartInfo() {
            UseShellExecute = false,
            FileName = "msiexec.exe",
            Arguments = $"/package {BattlegroundsContext.GetRelativePath(BattlegroundsPaths.UPDATE_FOLDER)}\\{_latestRelease.Assets[0].Name} /passive /norestart",
        };

        // Trigger compile
        Process? msi_install = Process.Start(msiexec_bin);
        if (msi_install is null) {
            Trace.WriteLine("Failed to create MSIExec process", nameof(Update));
			return;
        }

        Environment.Exit(0);

    }

	public static void UpdateApplication() {

		Trace.WriteLine($"New version {_latestRelease.TagName} detected", nameof(Update));

		DownloadNewVersion(() => {

            Trace.WriteLine("Installing new verison", nameof(Update));
			RunInstallMSI();

        });

	}

}
