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

using Battlegrounds.Functional;

namespace Battlegrounds.UI.Application.Components;

/// <summary>
/// Interaction logic for UserAvatar.xaml
/// </summary>
public partial class UserAvatar : UserControl {

    public static readonly DependencyProperty UsernameProperty
        = DependencyProperty.Register(nameof(Username), typeof(string), typeof(UserAvatar),
                                      new FrameworkPropertyMetadata(string.Empty,
                                      (a, b) => a.Cast<UserAvatar>(x => x.Username = (string)b.NewValue)));

    public string? Username {
        get => this.GetValue(UsernameProperty) as string;
        set {
            this.SetValue(UsernameProperty, value);
        }
    }

    public UserAvatar() {
        InitializeComponent();
    }

}
