using System;
using System.IO;
using System.Windows.Navigation;

namespace coh2_battlegrounds_installer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow {

        private static readonly string[] DefaultPaths = new string[] {
            ""
        };

        public MainWindow() {
            
            this.InitializeComponent();
            
            string path = this.TryFindBattlegrondsInstallDirectory();
            bool isInstalled = File.Exists(Path.Combine(path, "coh2-battlegrounds.exe"));
            this.Navigate(new ActionPage(isInstalled, path, this));

        }

        private string TryFindBattlegrondsInstallDirectory() {
            // TODO: Lookup in windows registry
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "coh2-bg\\");
        }

    }

}
