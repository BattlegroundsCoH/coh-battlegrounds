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
/// Interaction logic for EquipmentSlotView.xaml
/// </summary>
public partial class EquipmentSlotView : UserControl {

    public static readonly Brush VIEW_DEFAULT = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334252"));
    public static readonly Brush VIEW_HOVER = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#536375"));

    public EquipmentSlotView() {
        this.InitializeComponent();
    }

    protected override void OnMouseEnter(MouseEventArgs e) {
        base.OnMouseEnter(e);
        this.Background = VIEW_HOVER;
        this.PortraitElement.IsSelected = true;
        this.EquipButton.Visibility = Visibility.Visible;
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        base.OnMouseLeave(e);
        this.Background = VIEW_DEFAULT;
        this.PortraitElement.IsSelected = false;
        this.EquipButton.Visibility = Visibility.Collapsed;
    }

}
