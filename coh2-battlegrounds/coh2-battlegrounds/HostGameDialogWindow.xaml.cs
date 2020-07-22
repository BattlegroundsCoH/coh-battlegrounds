using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BattlegroundsApp
{
    /// <summary>
    /// Interaction logic for HostGameDialogWindow.xaml
    /// </summary>
    public partial class HostGameDialogWindow : Window
    {
        public HostGameDialogWindow()
        {
            InitializeComponent();
        }

        private void CancelHostGameButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HostGameButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
