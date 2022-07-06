using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Battlegrounds.Game.Database;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.Lobby.Planning;
using BattlegroundsApp.Utilities;

using static BattlegroundsApp.Lobby.MVVM.Models.LobbyPlanningOverviewModel;

namespace BattlegroundsApp.Lobby.MVVM.Views;

/// <summary>
/// Interaction logic for LobbyPlanningOverview.xaml
/// </summary>
public partial class LobbyPlanningOverview : UserControl {

    private record HelperElement(UIElement Element, TranslateTransform Translation, RotateTransform Rotation, Vector OffsetVector);

    private readonly Stack<Point> m_points;
    private readonly List<UIElement> m_lineHelpers;
    private HelperElement? m_planningHelper;

    public LobbyPlanningContextHandler ContextHandler 
        => this.DataContext is LobbyPlanningOverviewModel lpom ? lpom.ContextHandler : throw new Exception();

    public LobbyPlanningOverview() {

        // Init component
        this.InitializeComponent();
        
        // Create lists
        this.m_points = new();
        this.m_lineHelpers = new();

        // Grab context
        this.DataContextChanged += (a, b) => {
            if (b.NewValue is LobbyPlanningOverviewModel lpm) {
                lpm.MinimapItems.CollectionChanged += this.MinimapItems_CollectionChanged;
                if (b.OldValue is LobbyPlanningOverviewModel lpmold) {
                    lpmold.MinimapItems.CollectionChanged -= this.MinimapItems_CollectionChanged;
                }
            }
        };

    }

    private void PlanningCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        if (!this.ContextHandler.HasPlaceElement) {
            return;
        }

        var clickPos = e.GetPosition(this.PlanningCanvas);

        if (this.ContextHandler.RequiresSecond) {
            
            if (this.m_points.Count is 0) {

                this.m_points.Push(clickPos);
                var marker = this.CreateSelectedMarker(clickPos);
                this.m_planningHelper = marker;
                this.PlanningCanvas.Children.Add(marker.Element);

            } else {

                // Grab element
                var placeElement = this.m_points.Pop();

                // Place
                int placedId = this.ContextHandler.PlaceElement(placeElement, clickPos);

                // Subscribe to right click
                if (this.m_planningHelper is not null) {
                    RegisterRemoveEvent(this.m_planningHelper.Element, placedId);
                }

                // Clear
                this.m_points.Clear();
                this.m_planningHelper = null;

            }

        } else {

            // Grab element
            var placeElement = this.CreateSelectedMarker(clickPos).Element;

            // Show it
            this.PlanningCanvas.Children.Add(placeElement);

            // Place
            int placedId = this.ContextHandler.PlaceElement(clickPos);

            // Subscribe to right click
            RegisterRemoveEvent(placeElement, placedId);

        }        

    }

    private void RegisterRemoveEvent(UIElement e, int index) => e.MouseRightButtonUp += (a, b) => {
        this.ContextHandler.RemoveElement(index);
        this.PlanningCanvas.Children.Remove((UIElement)a);
    };

    private HelperElement CreateSelectedMarker(Point p) {
        if (this.ContextHandler.PlaceElementBlueprint is not null) {
            return CreateEntityMarker(this.ContextHandler.PlaceElementBlueprint, p);
        } else if (this.ContextHandler.PlaceElementSquadBlueprint is not null) {
            return CreateSquadMarker(this.ContextHandler.PlaceElementSquadBlueprint, p);
        }
        return CreateMarker(p);
    }

    private static HelperElement CreateEntityMarker(EntityBlueprint ebp, Point p) {

        // Grab blueprint
        var sym = App.ResourceHandler.GetIcon("entity_symbols", ebp.UI.Symbol);
        if (sym is null) {
            return CreateMarker(p);
        }

        // Create marker
        return CreateSomeMarker(sym, p);

    }

    private static HelperElement CreateSquadMarker(SquadBlueprint sbp, Point p) {

        // Grab blueprint
        var sym = App.ResourceHandler.GetIcon("symbol_icons", sbp.UI.Symbol);
        if (sym is null) {
            return CreateMarker(p);
        }

        // Create marker
        return CreateSomeMarker(sym, p);

    }

    private static HelperElement CreateSomeMarker(ImageSource sym, Point p) {

        // Create transform data
        var offset = new Vector(0.5 * sym.Width, 0.5 * sym.Height);
        var translate = new TranslateTransform(p.X - offset.X, p.Y - offset.Y);
        var rotate = new RotateTransform();

        // Create image marker
        var marker = new Image() {
            Source = sym,
            Width = sym.Width,
            Height = sym.Height,
            RenderTransformOrigin = new(0.5, 0.5),
            RenderTransform = new TransformGroup() {
                Children = {
                    rotate, translate
                }
            }
        };

        // Return new marker
        return new(marker, translate, rotate, offset);

    }

    private static HelperElement CreateMarker(Point p) {

        // Create translate
        var translate = new TranslateTransform(p.X - 0.5 * 30, p.Y - 0.5 * 25);
        var rotate = new RotateTransform();

        // Create marker
        Ellipse marker = new() {
            Fill = Brushes.Blue,
            Stroke = Brushes.Black,
            StrokeThickness = 2.5,
            Width = 30,
            Height = 25,
            RenderTransformOrigin = new(0.5,0.5),
            RenderTransform = new TransformGroup() {
                Children = new() { rotate, translate }
            }
        };

        // Return marker
        return new(marker, translate, rotate, new(30,25));

    }

    private void PlanningCanvas_MouseMove(object sender, MouseEventArgs e) {

        // If no place element, bail
        if (!(this.ContextHandler.HasEntityPlacement || this.ContextHandler.HasSquadPlacement)) {
            return;
        }

        // Grab point
        var p = e.GetPosition(this.PlanningCanvas);

        // If helper, rotate if directional
        if (this.m_planningHelper is not null) {

            // Calc pos
            var v0 = Vectors.FromTransform(this.m_planningHelper.Translation) + this.m_planningHelper.OffsetVector;

            // Get if line
            var isln = this.ContextHandler.IsLinePlacement;

            // Compute angle
            var v1 = Vectors.FromPoint(p);
            var v2 = v1 - v0;

            // Grab the angle
            var modAngle = isln ? 0.0 : 90.0;
            var angle = Math.Atan2(v2.Y, v2.X) * 57.29578 + modAngle;

            // Set angle
            this.m_planningHelper.Rotation.Angle = angle;

            // Do line placement
            if (this.ContextHandler.PlaceElementBlueprint is EntityBlueprint ebp && isln) {

                // Calculate distance
                var dist = v1.Length;

                // Get amount of fillers
                var w = this.m_planningHelper.OffsetVector.X * 2.0;
                var lineCount = (int)Math.Max(0, dist / w) / 2;
                var stepSize = 1.0 / lineCount;

                // Clear
                this.m_lineHelpers.ForEach(this.PlanningCanvas.Children.Remove);
                this.m_lineHelpers.Clear();

                // Create
                for (int i = 0; i < lineCount; i++) {

                    // Calculate helper position
                    var v3 = Vectors.Interpolate(v0, v1, i * stepSize);

                    // Create helper
                    var helper = CreateEntityMarker(ebp, v3.ToPoint());
                    helper.Rotation.Angle = angle;
                    helper.Element.MouseLeftButtonUp += this.PlanningCanvas_MouseLeftButtonUp;
                    helper.Element.MouseRightButtonUp += this.UserControl_MouseRightButtonUp;

                    // Add
                    this.m_lineHelpers.Add(helper.Element);
                    this.PlanningCanvas.Children.Add(helper.Element);

                }

            }

        } 

    }

    private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

        if (this.ContextHandler.HasPlaceElement) {
            this.ContextHandler.HasPlaceElement = false;
        }

        // Clear stack and helper
        this.m_points.Clear();
        this.m_planningHelper = null;

        // Clear line helpers (if any)
        if (this.m_lineHelpers.Count > 0) {
            this.m_lineHelpers.ForEach(this.PlanningCanvas.Children.Remove);
            this.m_lineHelpers.Clear();
        }

    }

    private void MinimapItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {

        if (e.Action is NotifyCollectionChangedAction.Remove && e.OldItems is not null) {
            foreach (object o in e.OldItems) {
                if (o is LobbyPlanningMinimapItem mini) {
                    foreach (Image c in this.ScenarioCanvas.Children) {
                        if (c.Tag.Equals(mini)) {
                            this.ScenarioCanvas.Children.Remove(c);
                            break;
                        }
                    }
                }
            }
        } else if (e.Action is NotifyCollectionChangedAction.Add && e.NewItems is not null) {

            foreach (LobbyPlanningMinimapItem item in e.NewItems) {
                Minimap.AddMinimapItem(this.ScenarioCanvas, this.ScenarioCanvas.Width, this.ScenarioCanvas.Height, item.Scenario, item.WorldPos, item.Owner, item.EntityBlueprint);
            }

        }

    }

}
