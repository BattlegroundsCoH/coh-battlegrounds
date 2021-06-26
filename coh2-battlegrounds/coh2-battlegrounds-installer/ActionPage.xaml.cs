using System;
using System.Collections.Generic;
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

namespace coh2_battlegrounds_installer {
    
    /// <summary>
    /// Interaction logic for ActionPage.xaml
    /// </summary>
    public partial class ActionPage : Page {

        public string InstallPath { get; set; }

        public NavigationWindow Window { get; }

        public ActionPage(bool isInstalled, string dir, NavigationWindow window) {
            this.Window = window;
            this.InstallPath = dir;
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {

        }

        private void Continue_Click(object sender, RoutedEventArgs e) {
            if (this.RB_Install.IsChecked ?? false) {
                this.Window.Navigate(new InstallPage(this.Window, this.InstallPath));
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e) {

        }

    }

}
