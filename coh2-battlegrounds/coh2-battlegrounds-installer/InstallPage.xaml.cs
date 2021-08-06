using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace coh2_battlegrounds_installer {

    /// <summary>
    /// Interaction logic for InstallPage.xaml
    /// </summary>
    public partial class InstallPage : Page {

        private readonly struct ServerAPIGETResponseData {
            public readonly bool Success;
            public readonly MemoryStream Content;

            public string Value => new StreamReader(this.Content).ReadToEnd();

            public ServerAPIGETResponseData(bool b, Stream s, Encoding encoding) {
                this.Success = b;
                this.Content = new MemoryStream();
                using (BinaryReader reader = new BinaryReader(s)) {
                    using (BinaryWriter writer = new BinaryWriter(this.Content, encoding, true)) {
                        byte[] buffer = reader.ReadBytes((int)s.Length);
                        writer.Write(buffer);
                    }
                }
                this.Content.Position = 0;
            }
        }

        private HttpClient m_client;

        public NavigationWindow Window { get; }

        public string TargetPath { get; }

        public InstallPage(NavigationWindow window, string path) {

            // Set properties
            this.Window = window;
            this.TargetPath = path;

            if (!Directory.Exists(this.TargetPath)) {
                Directory.CreateDirectory(this.TargetPath);
            }

            this.m_client = new();

            // Init component
            this.InitializeComponent();

        }

        public async void InstallProduct() {

            await Task.Delay(300);
            this.StatusOutput.AppendText($"Establishing connection to server ...{Environment.NewLine}");

            string[] address_attempts = {
                "localhost",
                "192.168.1.107",
                "194.37.80.249"
            };

            int addr = -1;
            string[] manifest = null;
            for (int i = 0; i < address_attempts.Length; i++) {
                try {
                    var response = await this.GET(address_attempts[i], "dist/manifest");
                    if (response.Success) {
                        addr = i;
                        manifest = JsonSerializer.Deserialize<string[]>(new StreamReader(response.Content).ReadToEnd());
                        this.StatusOutput.AppendText($"Downloaded manifest ({manifest.Length} files){Environment.NewLine}");
                        break;
                    }
                } catch { }
            }

            if (manifest is null || addr is -1) {
                _ = MessageBox.Show("The download and installation process failed to install one or more files.", "Failed to install.", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
                return;
            }

            int errcount = 0;

            for (int i = 0; i < manifest.Length; i++) {
                this.StatusOutput.AppendText($"Downloading {manifest[i]} ...{Environment.NewLine}");
                var response = await this.GET(address_attempts[addr], $"dist/dwfile?fid={i}");
                if (response.Success) {
                    string path = Path.GetFullPath(Path.Combine(this.TargetPath, manifest[i]));
                    this.StatusOutput.AppendText($"Installing {path} ...{Environment.NewLine}");
                    try {
                        string dir = Path.Combine(this.TargetPath, Path.GetDirectoryName(manifest[i]));
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                        File.WriteAllBytes(path, response.Content.ToArray());
                    } catch {
                        this.StatusOutput.AppendText($"Failed to install {path} ...{Environment.NewLine}");
                        errcount++;
                    }
                } else {
                    this.StatusOutput.AppendText($"Failed to download...{Environment.NewLine}");
                    errcount++;
                }
                await Task.Delay(1);
                this.Progress.Value = 100.0 * ((i + 1.0) / manifest.Length);
                this.ProgressPercentage.Content = $"{this.Progress.Value:0.00}%";
                this.StatusOutput.ScrollToEnd();
            }

            await Task.Delay(100);
            this.StatusOutput.AppendText($"Installed {manifest.Length - errcount} files ({errcount} failed to install).{Environment.NewLine}");
            await Task.Delay(200);

            if (errcount > 0) {
                _ = MessageBox.Show("The download and installation process failed to install one or more files.", "Failed to install.", MessageBoxButton.OK, MessageBoxImage.Error);
                // TODO: Cleanup all possibly installed files
            } else {
                var result = MessageBox.Show("The download and installation process has successfully installed Company of Heroes 2: Battlegrounds. Would you like to launch Battlegrounds?", "Battlegrounds installed.", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes) {
                    Process.Start(Path.Combine(TargetPath, "coh2-battlegrounds.exe"));
                }
            }

            Environment.Exit(errcount > 0 ? -1 : 0);

        }

        private async Task<ServerAPIGETResponseData> GET(string addr, string api) {

            // Create request
            var request = new HttpRequestMessage {
                RequestUri = new Uri($"http://{addr}:80/api/{api}"),
                Method = HttpMethod.Get,
            };

            try {

                // Send and get response
                using var response = await this.m_client.SendAsync(request);

                // If success
                if (response.IsSuccessStatusCode) {

                    // Read stream and return
                    return new ServerAPIGETResponseData(true, response.Content.ReadAsStream(), Encoding.ASCII);

                } else {

                    // Return a null response
                    return new ServerAPIGETResponseData(false, response.Content.ReadAsStream(), Encoding.ASCII);

                }

            } catch {

                return new ServerAPIGETResponseData(false, null, null);

            }

        }

    }

}
