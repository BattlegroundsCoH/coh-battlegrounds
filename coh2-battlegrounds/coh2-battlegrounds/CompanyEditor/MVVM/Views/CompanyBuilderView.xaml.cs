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
/// Interaction logic for CompanyBuilderView.xaml
/// </summary>
public partial class CompanyBuilderView : UserControl {

    public CompanyBuilderView() {
        InitializeComponent();
    }

    private void OnItemDrop(object sender, DragEventArgs e) {
        if (this.DataContext is CompanyBuilderViewModel vm) {
            vm.Drop?.Invoke(this, vm, e);
        }
    }

    private void ChangeTab(object sender, SelectionChangedEventArgs e) {
        if (this.DataContext is CompanyBuilderViewModel vm) {
           vm.Change?.Invoke(this, vm, e);
        }
    }

}
