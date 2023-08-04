using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Battlegrounds.Game.Blueprints.Extensions;

namespace Battlegrounds.UI.Controls;

/// <summary>
/// Enum representing the different display methods for a <see cref="CostDisplay"/> control.
/// </summary>
public enum CostDisplayMode {

    /// <summary>
    /// Display numeric cost over the resource icon
    /// </summary>
    CostOverIcon,

    /// <summary>
    /// Display the numeric cost to the right of the resource icon
    /// </summary>
    CostRightOfIcon

}

/// <summary>
/// Interaction logic for CostDisplay.xaml
/// </summary>
public partial class CostDisplay : UserControl {

    private readonly DataTemplate m_costOverIcon;
    private readonly DataTemplate m_costRightOfIcon;
    private readonly ObservableCollection<CostEntry> m_costs;

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty DisplayModeProperty
        = DependencyProperty.Register(nameof(DisplayMode), typeof(CostDisplayMode), typeof(CostDisplay), new PropertyMetadata(CostDisplayMode.CostOverIcon));

    /// <summary>
    /// 
    /// </summary>
    public CostDisplayMode DisplayMode {
        get => (CostDisplayMode)this.GetValue(DisplayModeProperty);
        set {
            this.SetCurrentValue(DisplayModeProperty, value);
            this.RefreshDisplay();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Icon"></param>
    /// <param name="Value"></param>
    public record CostEntry(string Icon, string Value);

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty CostProperty =
        DependencyProperty.Register(nameof(Cost), typeof(CostExtension), typeof(CostDisplay), new PropertyMetadata(null, CostChanged));

    private static void CostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is CostDisplay k) {
            k.Cost = e.NewValue as CostExtension;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public CostExtension? Cost {
        get => (CostExtension?)this.GetValue(CostProperty);
        set {
            this.SetCurrentValue(CostProperty, value);
            this.RefreshDisplay();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static readonly DependencyProperty ShowTimeProperty =
        DependencyProperty.Register(nameof(ShowTime), typeof(bool), typeof(CostDisplay), new PropertyMetadata(true));

    /// <summary>
    /// 
    /// </summary>
    public bool ShowTime {
        get => (bool)this.GetValue(ShowTimeProperty);
        set {
            this.SetCurrentValue(ShowTimeProperty, value);
            this.RefreshDisplay();
        }
    }

    public CostDisplay() {
        
        // Init
        this.InitializeComponent();

        // Fetch templates
        this.m_costOverIcon = (DataTemplate)this.FindResource("ElementMode_CostOverIcon");
        this.m_costRightOfIcon = (DataTemplate)this.FindResource("ElementMode_CostRightOfIcon");

        // Init observable
        this.m_costs = new();
        
        // Set observable
        this.SelfContainer.ItemsSource = this.m_costs;

    }

    private void RefreshDisplay() {

        // Clear display
        this.m_costs.Clear();

        // Set proper item template
        this.SelfContainer.ItemTemplate = this.DisplayMode switch {
            CostDisplayMode.CostOverIcon => this.m_costOverIcon,
            CostDisplayMode.CostRightOfIcon => this.m_costRightOfIcon,
            _ => throw new Exception()
        };

        // Grab costs
        var cost = this.Cost ?? new(0, 0, 0, 0);

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

    private static string FormatTime(TimeSpan time)
        => time.Minutes > 0 ? $"{time.Minutes:0}m {time.Seconds:00}s" : $"{time.Seconds:00}s";

}
