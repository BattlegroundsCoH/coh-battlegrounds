using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
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

namespace BattlegroundsApp.Controls.Dashboard;
/// <summary>
/// Interaction logic for UserProfile.xaml
/// </summary>
public partial class UserProfile : UserControl {

    public static readonly DependencyProperty UsernameProperty
        = DependencyProperty.Register(nameof(Username), typeof(string), typeof(UserProfile),
                                      new FrameworkPropertyMetadata(string.Empty,
                                      (a, b) => a.Cast<UserProfile>(x => x.Username = (string)b.NewValue)));

    public string? Username {
        get => this.GetValue(UsernameProperty) as string;
        set {
            this.SetValue(UsernameProperty, value);
        }
    }

    public UserProfile() {
        InitializeComponent();
    }
}
