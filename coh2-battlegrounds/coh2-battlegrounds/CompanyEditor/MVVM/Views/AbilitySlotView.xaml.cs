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
/// Interaction logic for AbilitySlotView.xaml
/// </summary>
public partial class AbilitySlotView : UserControl {

    public static readonly Brush VIEW_DEFAULT = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334252"));
    public static readonly Brush VIEW_HOVER = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#536375"));

    public AbilitySlotView() {
        InitializeComponent();
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        this.Background = VIEW_HOVER;
        this.IconElement.IsSelected = true;
        if (this.DataContext is AbilitySlotViewModel viewModel && viewModel.IsRemovable) {
            this.RemoveButton.Visibility = Visibility.Visible;
            this.RemoveButton.IsEnabled = true;
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        this.Background = VIEW_DEFAULT;
        this.IconElement.IsSelected = false;
        this.RemoveButton.Visibility = Visibility.Collapsed;
        this.RemoveButton.IsEnabled = false;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e) {
        base.OnMouseDown(e);
        if (this.DataContext is AbilitySlotViewModel vm) {
            vm.Click?.Invoke(this, vm);
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e) {
        if (this.DataContext is AbilitySlotViewModel vm) {
            vm.RemoveClick?.Invoke(this, vm);
        }
    }

}
