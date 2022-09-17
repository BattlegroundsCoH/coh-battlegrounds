using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace Battlegrounds.Update;

public static class Update {

	// 1. Check for new version
	// 2. If new version => download newest version
	// 3. Close application
	// 4. Install newest version

	private static readonly HttpClient _httpClient = new HttpClient();

	private static readonly Release _latestRelease = ProcessLatestRelease().Result;

	private static async Task<Release> ProcessLatestRelease() {

		_httpClient.DefaultRequestHeaders.Accept.Clear();
		_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
		_httpClient.DefaultRequestHeaders.Add("User-Agent", "Battlegrounds Mod Launcher");

		var streamTask = await _httpClient.GetStreamAsync("https://api.github.com/repos/JustCodiex/coh2-battlegrounds/releases/latest");
        var release = JsonSerializer.Deserialize<Release>(streamTask);

		return release!;

	}

	private static bool IsNewVersion() {

		var latestVersion = new Version(Regex.Replace(_latestRelease.TagName, @"[-]?[a-zA-Z]+", ""));
        var assemblyVersion = new Version(BattlegroundsInstance.Version.ApplicationVersion);

		if (latestVersion.CompareTo(assemblyVersion) > 0) return true; 

        return false;

	}

	private static async void DownloadNewVersion(Action action) {

		var downloadName = _latestRelease.Assets[0].Name;
		var downloadUrl = new Uri(_latestRelease.Assets[0].InstallerDownloadUrl, UriKind.Absolute);

		Trace.WriteLine("Starting download", nameof(Update));

		using var stream = await _httpClient.GetStreamAsync(downloadUrl);
		using var fileStream = new FileStream($"{BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.UPDATE_FOLDER, Path.GetFileName(downloadName))}", FileMode.Create); 
		
		await stream.CopyToAsync(fileStream);

        Trace.WriteLine("Finished download", nameof(Update));

		action.Invoke();

    }

	private static void RunInstallMSI() {

        ProcessStartInfo msiexec_bin = new ProcessStartInfo() {
            UseShellExecute = false,
            FileName = "msiexec.exe",
            Arguments = $"/package {BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.UPDATE_FOLDER)}\\{_latestRelease.Assets[0].Name} /passive /norestart",
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

		// TODO: Do the check in App_Startup to also prompt GUI
		//if (!IsNewVersion()) return;

		Trace.WriteLine($"New version {_latestRelease.TagName} detected", nameof(Update));

		DownloadNewVersion(() => {

            Trace.WriteLine("Installing new verison", nameof(Update));
			RunInstallMSI();

        });



	}

}
