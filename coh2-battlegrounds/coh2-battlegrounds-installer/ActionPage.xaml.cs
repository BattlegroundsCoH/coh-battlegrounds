using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace coh2_battlegrounds_installer {

    /// <summary>
    /// Interaction logic for ActionPage.xaml
    /// </summary>
    public partial class ActionPage : Page, INotifyPropertyChanged {

        public string InstallPath { get; set; }

        public NavigationWindow Window { get; }

        public ActionPage(bool isInstalled, string dir, NavigationWindow window) {
            this.DataContext = this;
            this.Window = window;
            this.InstallPath = dir;
            this.InitializeComponent();
            window.Title = "Company of Heroes 2: Battlegrounds Installer";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Cancel_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);

        private void Continue_Click(object sender, RoutedEventArgs e) {
            if (this.RB_Install.IsChecked ?? false) {
                InstallPage page = new InstallPage(this.Window, this.InstallPath);
                this.Window.Navigate(page);
                page.InstallProduct();
            } else if (this.RB_Update.IsChecked ?? false) {
                InstallPage page = new InstallPage(this.Window, this.InstallPath);
                this.Window.Navigate(page);
            } else {

            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e) {

            SaveFileDialog dialog = new SaveFileDialog {
                InitialDirectory = this.InstallPath, // Use current value for initial dir
                Title = "Select a Directory", // instead of default "Save As"
                Filter = "Directory|*.this.directory", // Prevents displaying files
                FileName = "select" // Filename will then be "select.this.directory"
            };

            if (dialog.ShowDialog() == true) {

                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");

                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                // Our final value is in path
                this.InstallPath = path;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.InstallPath)));

            }
        }

    }

}
