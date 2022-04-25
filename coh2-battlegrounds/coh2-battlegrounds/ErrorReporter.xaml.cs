using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

using Battlegrounds.Networking.Server;

namespace BattlegroundsApp;
/// <summary>
/// Interaction logic for ErrorReporter.xaml
/// </summary>
public partial class ErrorReporter : Window {

    public ErrorReporter() {
        
        this.InitializeComponent();

    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
        this.Close();
    }

    private async void Send_Click(object sender, RoutedEventArgs e) {
        try {

            // Create API instance
            ServerAPI api = new("194.37.80.249", safeMode: true);

            // Grab additional info
            var info = new TextRange(this.AdditionalInfo.Document.ContentStart, this.AdditionalInfo.Document.ContentEnd).Text;

            // Show Sent
            this.Cancel.IsEnabled = false;
            this.Send.IsEnabled = false;

            // Inform user
            this.Send.Content = "Sending";

            // Send along error
            await Task.Run(() => api.UplouadAppCrashReport(info, Path.GetFullPath("coh2-bg.log"), isScar: false));

            // Update visually
            this.Send.Content = "Sent (Closing)";
            this.Cancel.IsEnabled = true;

            // Wait a bit
            await Task.Delay(1500);

        } catch (Exception ex) {
            File.WriteAllText("err-crash.txt", ex.ToString());
        } finally {
            this.Close();
        }
    }

}
