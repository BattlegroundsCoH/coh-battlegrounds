using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Battlegrounds.Game.Database;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;

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

    private record HelperElement(FrameworkElement Element, TranslateTransform Translation, RotateTransform Rotation, Vector OffsetVector, int ElementId);

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
                lpm.ContextHandler.Elements.CollectionChanged += this.Elements_CollectionChanged;
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
                this.ContextHandler.PlaceElement(placeElement, clickPos);

                // Clear
                this.m_points.Clear();
                this.m_planningHelper = null;

            }

        } else {
            
            // Place the element
            this.ContextHandler.PlaceElement(clickPos);

        }        

    }

    private HelperElement CreateSelectedMarker(Point p) {
        if (this.ContextHandler.PlaceElementBlueprint is not null) {
            return CreateEntityMarker(this.ContextHandler.PlaceElementBlueprint, p);
        } else if (this.ContextHandler.PlaceElementSquadBlueprint is not null) {
            return CreateSquadMarker(this.ContextHandler.PlaceElementSquadBlueprint, p);
        }
        return CreateObjectiveMarker(this.ContextHandler.PlaceElemtObjectiveType, p);
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

    private static HelperElement CreateObjectiveMarker(PlanningObjectiveType objectiveType, Point p) {

        // Grab blueprint
        ImageSource? sym = objectiveType switch { // TODO: Get icons
            PlanningObjectiveType.OT_Attack => null,
            PlanningObjectiveType.OT_Defend => null,
            PlanningObjectiveType.OT_Support => null,
            _ => null
        };
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
        return new(marker, translate, rotate, offset, -1);

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
        return new(marker, translate, rotate, new(30,25), -1);

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

            // Calc vectors
            var v0 = Vectors.FromTransform(this.m_planningHelper.Translation) + this.m_planningHelper.OffsetVector;
            var v1 = Vectors.FromPoint(p);

            // Get if line
            var isln = this.ContextHandler.IsLinePlacement;

            // Lookat point
            var angle = Lookat(this.m_planningHelper.Rotation, v0, v1, isln ? 0.0 : 90.0);

            // Do line placement
            if (this.ContextHandler.PlaceElementBlueprint is EntityBlueprint ebp && isln) {

                // Clear
                this.m_lineHelpers.ForEach(this.PlanningCanvas.Children.Remove);
                this.m_lineHelpers.Clear();

                // Get display elements
                LineTo(angle, ebp, v0, v1, this.m_planningHelper.OffsetVector).ForEach(x => {
                    this.m_lineHelpers.Add(x.Element);
                    this.PlanningCanvas.Children.Add(x.Element);
                    x.Element.MouseLeftButtonUp += this.PlanningCanvas_MouseLeftButtonUp;
                    x.Element.MouseRightButtonUp += this.UserControl_MouseRightButtonUp;
                });

            }

        } 

    }

    private static double Lookat(RotateTransform rotateTransform, Vector origin, Vector lookat, double modAngle = 90.0) {

        // Compute angle
        var dir = lookat - origin;

        // Grab the angle
        var angle = Math.Atan2(dir.Y, dir.X) * Numerics.RAD2DEG + modAngle; // Magic number ==> Constant from radians to degrees

        // Set angle
        rotateTransform.Angle = angle;

        // Return the angle
        return angle;

    }

    private static List<HelperElement> LineTo(double angle, EntityBlueprint ebp, Vector origin, Vector target, Vector offset) {

        // Create container
        var ls = new List<HelperElement>();

        // Calculate distance
        var dist = (origin - target).Length;

        // Get amount of fillers
        var w = offset.X * 2.0;
        var lineCount = (int)Math.Max(0, dist / w) / 2;
        var stepSize = 1.0 / lineCount;

        // Create
        for (int i = 0; i < lineCount; i++) {

            // Calculate helper position
            var v = Vectors.Interpolate(origin, target, i * stepSize);

            // Create helper
            var helper = CreateEntityMarker(ebp, v.ToPoint());
            helper.Rotation.Angle = angle;

            // Add
            ls.Add(helper);

        }

        // Return elements
        return ls;

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

    private void Elements_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {

        // Is add event
        if (e.Action is NotifyCollectionChangedAction.Add) {

            // Bail if no new items
            if (e.NewItems is null) {
                return;
            }

            // Loop over added items
            foreach (LobbyPlanningObject planningObject in e.NewItems) {

                // Create container for helpers
                List<HelperElement> markers = new();

                // Determine what element to visually add
                if (planningObject.IsSquad) {

                    // Get squad blueprint
                    var sbp = (SquadBlueprint)planningObject.Blueprint;

                    // Grab marker
                    var marker = CreateSquadMarker(sbp, planningObject.VisualPosStart);
                    marker.Element.Tag = marker;

                    // Lookat at target
                    if (planningObject.VisualPointEnd is Point lookat) {
                        Lookat(marker.Rotation, Vectors.FromTransform(marker.Translation) + marker.OffsetVector, Vectors.FromPoint(lookat));
                    }

                    // Add element
                    this.PlanningCanvas.Children.Add(marker.Element);

                    // Add to markers
                    markers.Add(marker);

                } else if (planningObject.IsEntity) {

                    // Get ebp
                    var ebp = (EntityBlueprint)planningObject.Blueprint;

                    // Grab marker
                    var marker = CreateEntityMarker(ebp, planningObject.VisualPosStart);
                    marker.Element.Tag = marker;

                    // Lookat at target
                    if (planningObject.VisualPointEnd is Point lookat) {
                        var v0 = Vectors.FromTransform(marker.Translation) + marker.OffsetVector;
                        var v1 = Vectors.FromPoint(lookat);
                        var angle = Lookat(marker.Rotation, v0, v1, planningObject.IsLine ? 0 : 90.0);
                        if (planningObject.IsLine) {
                            LineTo(angle, ebp, v0, v1, marker.OffsetVector).ForEach(x => {
                                this.PlanningCanvas.Children.Add(x.Element);
                                markers.Add(x);
                            });
                        }
                    }

                    // Add element
                    this.PlanningCanvas.Children.Add(marker.Element);

                    // Add to markers
                    markers.Add(marker);

                } else {

                    // Grab marker
                    var marker = CreateObjectiveMarker(planningObject.ObjectiveType, planningObject.VisualPosStart);
                    marker.Element.Tag = marker;

                    // Add element
                    this.PlanningCanvas.Children.Add(marker.Element);

                    // Add to markers
                    markers.Add(marker);

                }

                // Register remove event on self elements
                if (planningObject.Owner == this.ContextHandler.SelfId) {
                    foreach (HelperElement helper in markers) {
                        this.RegisterRemoveEvent(helper.Element, planningObject.ObjectId);
                    }
                }

            }

        } else if (e.Action is NotifyCollectionChangedAction.Remove) { // else if remove event

            // Bail if no old items
            if (e.OldItems is null) {
                return;
            }

            // Loop over removed items
            foreach (LobbyPlanningObject planningObject in e.OldItems) {

                // Loop over canvas elements and remove all with id == planingObject.id
                var removeElements = Select(this.PlanningCanvas.Children, 
                    x => x is FrameworkElement fe && (fe.Tag is HelperElement h && h.ElementId == planningObject.ObjectId));

                // Remove range
                removeElements.ForEach(this.PlanningCanvas.Children.Remove);

            }

        }

    }

    private static List<UIElement> Select(UIElementCollection collection, Predicate<UIElement> predicate) {
        var ls = new List<UIElement>();
        foreach (UIElement c in collection) {
            if (predicate(c)) {
                ls.Add(c);
            }
        }
        return ls;
    }

    private void RegisterRemoveEvent(UIElement e, int index) => e.MouseRightButtonUp += (a, b) => {
        this.ContextHandler.RemoveElement(index);
        this.PlanningCanvas.Children.Remove((UIElement)a);
    };

}
