using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;

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
            this.Dispatcher.Invoke(() => this.SetDisplay(value));
        }
    }

    public static readonly DependencyProperty PlanCommandProperty =
        DependencyProperty.Register(nameof(PlanCommand), typeof(ICommand), typeof(Minimap), new PropertyMetadata(null, PlanCommandChanged));

    public ICommand? PlanCommand {
        get => this.GetValue(PlanCommandProperty) as ICommand;
        set => this.SetValue(PlanCommandProperty, value);
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

    private static void PlanCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

        // Get minimap and new scenario
        Minimap m = (Minimap)d;
        ICommand? cmd = e.NewValue as ICommand;

        // Update
        m.PlanCommand = cmd;

    }

    #endregion

    public Minimap() {
        this.InitializeComponent();
    }

    private async void SetDisplay(Scenario? scenario) {

        // Clear existing
        this.ScenarioCanvas.Children.Clear();

        // Try get display image
        this.ScenarioDisplay.Source = LobbySettingsLookup.TryGetMapSource(scenario);

        // Now bail if no scenario was set
        if (scenario is null) {
            return;
        }

        // Grab points
        var points = await Task.Run(() => scenario.Points.Map(x => (x.Position, x.Owner switch {
            >= 1000 and < 1008 => (ushort)(x.Owner - 1000),
            _ => ushort.MaxValue
        }, BlueprintManager.FromBlueprintName<EntityBlueprint>(x.EntityBlueprint))));


        // Display basic information
        await this.Dispatcher.BeginInvoke(() => this.TryShowPositions(scenario, points));

        // TODO: Add display elements

        // Return if display only
        if (this.IsDisplayOnly) {
            return;
        }

    }

    private void TryShowPositions(Scenario scen, (GamePosition pos, ushort owner, EntityBlueprint ebp)[] pointData) {

        // Get world to minimap scale
        var xs = this.ScenarioDisplay.Source.Width / (scen.TerrainSize.X) * 1.3;
        var ys = this.ScenarioDisplay.Source.Height / (scen.TerrainSize.Y) * 1.3;

        // Get origin
        var ox = this.ScenarioDisplay.Source.Width * 0.5;
        var oy = this.ScenarioDisplay.Source.Height * 0.5;

        // Compute scale
        var mx = (this.ScenarioCanvas.Width / this.ScenarioDisplay.Source.Width);
        var my = (this.ScenarioCanvas.Height / this.ScenarioDisplay.Source.Height);

        // Pick from points
        for (int i = 0; i < pointData.Length; i++) {

            // Grab ico
            string ico = pointData[i].ebp.Name switch {
                "starting_position_shared_territory" => $"Icons_minimap_mm_starting_point_{pointData[i].owner + 1}",
                "victory_point" => "Icons_minimap_mm_victory_point",
                _ => string.Empty
            };

            // Bail if no icon is defined
            if (string.IsNullOrEmpty(ico)) {
                continue;
            }

            // Define position
            double xpos = pointData[i].pos.X * xs + ox;
            double ypos = -pointData[i].pos.Y * ys + oy;

            // Create image
            Image img = new() {
                Width = 24,
                Height = 24,
                Source = App.ResourceHandler.GetIcon("minimap_icons", ico),
                RenderTransformOrigin = new(0.5, 0.5)
            };

            // Add to canvas group
            this.ScenarioCanvas.Children.Add(img);

            // Display
            img.SetValue(Canvas.LeftProperty, xpos * mx);
            img.SetValue(Canvas.TopProperty, ypos * my);

        }

    }

    private void PrepareDefenceButton_Click(object sender, RoutedEventArgs e) => this.PlanCommand?.Execute(null);

}
