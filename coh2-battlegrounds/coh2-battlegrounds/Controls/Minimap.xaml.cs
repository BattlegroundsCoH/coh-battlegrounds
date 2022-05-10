using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Battlegrounds.Game.Database;

using BattlegroundsApp.Lobby;

namespace BattlegroundsApp.Controls;

/// <summary>
/// Interaction logic for Minimap.xaml
/// </summary>
public partial class Minimap : UserControl {

    #region Properties

    public static readonly DependencyProperty ScenarioProperty = 
        DependencyProperty.Register(nameof(Scenario), typeof(Scenario), typeof(Minimap), 
            new FrameworkPropertyMetadata(
                null, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, 
                new PropertyChangedCallback(ScenarioChanged),
                null) { BindsTwoWayByDefault = true },
            null);

    public Scenario? Scenario {
        get => this.GetValue(ScenarioProperty) as Scenario;
        set {
            this.SetValue(ScenarioProperty, value);
            this.SetDisplay(value);
        }
    }

    #endregion

    #region Callbacks

    private static void ScenarioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

        // Get minimap and new scenario
        Minimap m = (Minimap)d;
        Scenario? scen = e.NewValue as Scenario;

        // Update
        m.Scenario = scen;

    }

    #endregion

    public Minimap() {
        this.InitializeComponent();
    }

    private void SetDisplay(Scenario? scenario) {

        // Try get display image
        this.ScenarioDisplay.Source = LobbySettingsLookup.TryGetMapSource(scenario);

    }

}
