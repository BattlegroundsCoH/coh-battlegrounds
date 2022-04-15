using System.IO;
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

    private void Send_Click(object sender, RoutedEventArgs e) {
        try {

            // Create API instance
            ServerAPI api = new("192.168.1.107", true);

            // Grab additional info
            var info = new TextRange(this.AdditionalInfo.Document.ContentStart, this.AdditionalInfo.Document.ContentEnd).Text;

            // Send along error
            api.UplouadAppCrashReport(info, Path.GetFullPath("coh2-bg.log"), false);

        } finally {
            this.Close();
        }
    }

}
