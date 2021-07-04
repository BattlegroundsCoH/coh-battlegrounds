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

namespace BattlegroundsApp.Views.ViewComponent.BindablePasswordBox {
    /// <summary>
    /// Interaction logic for BindablePasswordBox.xaml
    /// </summary>
    public partial class BindablePasswordBox : UserControl {

        private bool _isPasswordChnaging;

        public string Password {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(BindablePasswordBox), new PropertyMetadata(string.Empty, PasswordPropertyChnaged));

        private static void PasswordPropertyChnaged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is BindablePasswordBox passwordBox) {
                passwordBox.UpdatePassword();
            }
        }

        public BindablePasswordBox() {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) {
            _isPasswordChnaging = true;
            Password = passwordBox.Password;
            _isPasswordChnaging = false;
        }

        private void UpdatePassword() {
            if (!_isPasswordChnaging) {
                passwordBox.Password = Password;
            }
        }
    }
}
