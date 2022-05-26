﻿using System;
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
        DependencyProperty.Register(nameof(PlanCommand), typeof(ICommand), typeof(Minimap));

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

        // Get scale
        var xs = this.ScenarioCanvas.Width / (scen.TerrainSize.X) * 2;
        var ys = this.ScenarioCanvas.Height / (scen.TerrainSize.Y) * 2;

        // Get origin
        var ox = (this.ScenarioCanvas.Width * 0.5);
        var oy = (this.ScenarioCanvas.Height * 0.5);

        // Pick from points
        for (int i = 0; i < pointData.Length; i++) { 
            if (pointData[i].ebp.Name is "starting_position_shared_territory") {

                // Add
                Image img = new() {
                    Width = 24,
                    Height = 24,
                    Source = App.ResourceHandler.GetIcon("minimap_icons", $"Icons_minimap_mm_starting_point_{pointData[i].owner + 1}"),
                    RenderTransformOrigin = new(0.5, 0.5)
                };

                // Add to canvas group
                this.ScenarioCanvas.Children.Add(img);

                // Define position
                double xpos = pointData[i].pos.X * xs + ox;
                double ypos = -pointData[i].pos.Y * ys + oy;

                // Display
                img.SetValue(Canvas.LeftProperty, xpos);
                img.SetValue(Canvas.TopProperty, ypos);

            }
        }

    }

}