using BattlegroundsApp.CompanyEditor.MVVM.Models;
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

namespace BattlegroundsApp.CompanyEditor.MVVM.Views;

/// <summary>
/// Interaction logic for AvailableSquadView.xaml
/// </summary>
public partial class AvailableSquadView : UserControl {

    public AvailableSquadView() {
        InitializeComponent();
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        if (this.DataContext is AvailableSquadViewModel vm) {
            vm.Move?.Invoke(this, vm);
        }
    }

    private void AddButton_Click(object sender, RoutedEventArgs e) { 
        if (this.DataContext is AvailableSquadViewModel vm) {
            vm.AddClick?.Invoke(this, vm);
        }
    }

}
