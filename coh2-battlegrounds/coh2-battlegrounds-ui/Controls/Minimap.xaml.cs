using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Game.Scenarios;
using Battlegrounds.Game;
using Battlegrounds.Resources;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;

namespace Battlegrounds.UI.Controls;

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
            this.Dispatcher.Invoke(() => this.SetDisplay(value));
        }
    }

    public static readonly DependencyProperty IsDisplayOnlyProperty =
        DependencyProperty.Register(nameof(IsDisplayOnly), typeof(bool), typeof(Minimap), new PropertyMetadata(true));

    public bool IsDisplayOnly {
        get => (bool)this.GetValue(IsDisplayOnlyProperty);
        set => this.SetValue(IsDisplayOnlyProperty, value);
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

    private async void SetDisplay(Scenario? scenario) {

        // Clear existing
        this.ScenarioCanvas.Children.Clear();

        // Try get display image
        this.ScenarioDisplay.Source = ScenarioPreviewLookup.TryGetMapSource(scenario);

        // Now bail if no scenario was set
        if (scenario is null) {
            return;
        }

        // Grab points
        var points = await Task.Run(() => scenario.Points.Map(x => (x.Position, x.Owner switch {
            >= 1000 and < 1008 => (ushort)(x.Owner - 1000),
            _ => ushort.MaxValue
        }, BattlegroundsContext.DataSource.GetBlueprints(BattlegroundsContext.ModManager.GetVanillaPackage(GameCase.CompanyOfHeroes2), GameCase.CompanyOfHeroes2)
        !.FromBlueprintName<EntityBlueprint>(x.EntityBlueprint))));

        // Display basic information
        await this.Dispatcher.BeginInvoke(() => this.TryShowPositions(scenario, points));

        // TODO: Add display elements

        // Return if display only
        if (this.IsDisplayOnly) {
            return;
        }

    }

    private void TryShowPositions(Scenario scen, (GamePosition pos, ushort owner, EntityBlueprint ebp)[] pointData) {

        // Pick from points
        for (int i = 0; i < pointData.Length; i++) {
            var p = pointData[i].pos;
            var o = pointData[i].owner;
            var e = pointData[i].ebp;
            AddMinimapItem(this.ScenarioCanvas, this.ScenarioDisplay.Width, this.ScenarioDisplay.Height, scen, p, o, e);
        }

    }

    public static void AddMinimapItem(Canvas canvas, double width, double height, Scenario scenario, GamePosition pos, ushort owner, EntityBlueprint ebp) {

        // Grab ico
        string ico = ebp.Name switch {
            "starting_position_shared_territory" => $"Icons_minimap_mm_starting_point_{owner + 1}",
            "victory_point" => "Icons_resources_minimap_icon_victory",
            "territory_munitions_point_mp" => "Icons_minimap_mm_ammo_point",
            "territory_fuel_point_mp" => "Icons_minimap_mm_fuel_point_secured",
            "territory_point_mp" or "territory_point_low" => "Icons_minimap_mm_territory_point",
            _ => string.Empty
        };

        // Grab size
        double size = ebp.Name switch {
            "territory_fuel_point_mp" => 18,
            "territory_point_mp" or "territory_point_low" => 16,
            _ => 24
        };
        double size2 = size / 2;

        // Bail if no icon is defined
        if (string.IsNullOrEmpty(ico)) {
            return;
        }

        // Calculate position
        var p = scenario.ToMinimapPosition(width, height, pos);

        // Create image
        Image img = new() {
            Width = size,
            Height = size,
            Source = ResourceHandler.GetIcon("minimap_icons", ico),
            RenderTransformOrigin = new(0.5, 0.5)
        };

        // Add to canvas group
        canvas.Children.Add(img);

        // Display
        img.SetValue(Canvas.LeftProperty, p.X - size2);
        img.SetValue(Canvas.BottomProperty, p.Y - size2);


    }

}
