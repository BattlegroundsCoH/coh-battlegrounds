using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using Battlegrounds.Game.Database.Extensions;

namespace BattlegroundsApp.Controls;
/// <summary>
/// Interaction logic for UnitCost.xaml
/// </summary>
public partial class UnitCost : UserControl {
    
    public record CostEntry(string Icon, string Value);

    public static readonly DependencyProperty CostProperty =
        DependencyProperty.Register(nameof(Cost), typeof(CostExtension), typeof(UnitCost), new PropertyMetadata(null, CostChanged));

    private static void CostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is UnitCost k) {
            k.Cost = e.NewValue as CostExtension;
        }
    }

    public CostExtension? Cost {
        get => (CostExtension?)this.GetValue(CostProperty);
        set {
            this.SetCurrentValue(CostProperty, value);
            this.CostUpdated();
        }
    }

    public static readonly DependencyProperty ShowTimeProperty =
        DependencyProperty.Register(nameof(ShowTime), typeof(bool), typeof(UnitCost), new PropertyMetadata(true));

    public bool ShowTime {
        get => (bool)this.GetValue(ShowTimeProperty);
        set {
            this.SetCurrentValue(ShowTimeProperty, value);
            this.CostUpdated();
        }
    }

    private readonly ObservableCollection<CostEntry> m_costs;

    public UnitCost() {
        
        // Init components
        this.InitializeComponent();
        
        // Create observable
        this.m_costs = new();

        // Set observable
        this.SelfContainer.ItemsSource = this.m_costs;

    }

    private void CostUpdated() {

        // Clear contents
        this.m_costs.Clear();

        // Grab costs
        var cost = this.Cost ?? new(0,0,0,0);

        // Add costs
        if (cost.Manpower > 0)
            this.m_costs.Add(new("pack://application:,,,/Resources/ingame/resource_icons/botb_manpower.png", ((int)(cost.Manpower)).ToString()));
        if (cost.Munitions > 0)
            this.m_costs.Add(new("pack://application:,,,/Resources/ingame/resource_icons/botb_munitions.png", ((int)cost.Munitions).ToString()));
        if (cost.Fuel > 0)
            this.m_costs.Add(new("pack://application:,,,/Resources/ingame/resource_icons/botb_fuel.png", ((int)cost.Fuel).ToString()));
        if (cost.FieldTime > 0 && this.ShowTime)
            this.m_costs.Add(new("pack://application:,,,/Resources/ingame/resource_icons/botb_time.png", FormatTime(TimeSpan.FromSeconds(cost.FieldTime))));

    }

    private string FormatTime(TimeSpan time)
        => time.Minutes > 0 ? $"{time.Minutes:0}m {time.Seconds:00}s" : $"{time.Seconds:00}s";

}
