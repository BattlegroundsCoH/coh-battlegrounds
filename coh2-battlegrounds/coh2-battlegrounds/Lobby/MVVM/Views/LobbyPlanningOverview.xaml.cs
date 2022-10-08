using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Locale;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Resources;
using Battlegrounds.UI.Graphics;
using Battlegrounds.Util;

using BattlegroundsApp.Controls;
using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.Lobby.Planning;

using static BattlegroundsApp.Lobby.MVVM.Models.LobbyPlanningOverviewModel;

namespace BattlegroundsApp.Lobby.MVVM.Views;

/// <summary>
/// Interaction logic for LobbyPlanningOverview.xaml
/// </summary>
public partial class LobbyPlanningOverview : UserControl {

    private static readonly ColourOverlayEffect __PlacementNotAllowedEffect = new ColourOverlayEffect(Colors.Red);

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
                this.ContextHandler.MinimapRenderSize = this.PlanningCanvas.RenderSize;
                lpm.MinimapItems.CollectionChanged += this.MinimapItems_CollectionChanged;
                if (b.OldValue is LobbyPlanningOverviewModel lpmold) {
                    lpmold.MinimapItems.CollectionChanged -= this.MinimapItems_CollectionChanged;
                }
                lpm.ContextHandler.Elements.CollectionChanged += this.Elements_CollectionChanged;
            }
        };

    }

    private void PlanningCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        // Bail if no place element
        if (!this.ContextHandler.HasPlaceElement) {
            return;
        }

        // Grab click position
        var clickPos = e.GetPosition(this.PlanningCanvas);

        // Check if a second position is required
        if (this.ContextHandler.RequiresSecond) {
            
            // Place start point?
            if (this.m_points.Count is 0) {

                this.m_points.Push(clickPos);
                var marker = this.CreateSelectedMarker(clickPos, -1);
                this.m_planningHelper = marker;
                this.PlanningCanvas.Children.Add(marker.Element);

                // Show line helper
                if (this.ContextHandler.IsLinePlacement) {
                    this.LinePlacementHelperBox.Visibility = Visibility.Visible;
                    this.LinePlacementHelperBox.SetValue(Canvas.TopProperty, clickPos.Y);
                    this.LinePlacementHelperBox.SetValue(Canvas.LeftProperty, clickPos.X);
                    this.LinePlacementHelperBoxTitle.Content = GameLocale.GetString(this.ContextHandler.PlaceElementBlueprint!.UI.ScreenName);
                } else {
                    this.LinePlacementHelperBox.Visibility = Visibility.Collapsed;
                }

            } else if (this.m_planningHelper is not null) {

                // Grab element
                var placeElement = this.m_points.Pop();

                // Check if placement is valid
                if (this.ContextHandler.IsLinePlacement && this.ContextHandler.PlaceElementBlueprint is EntityBlueprint ebp) {

                    // Grab amount of ingame items
                    int ingameCount = this.ContextHandler.GetIngameCount(placeElement, clickPos, this.ContextHandler.PlacementWidth);

                    // Perform placement test
                    if (!this.ContextHandler.GetSelfCapacity(ebp.Name).Test(ingameCount)) {
                        this.m_points.Push(placeElement); // Push it back in
                        return;
                    }

                }

                // Place
                this.ContextHandler.PlaceElement(this.PlanningCanvas.RenderSize, placeElement, clickPos);

                // Remove it
                this.PlanningCanvas.Children.Remove(this.m_planningHelper.Element);

                // Clear points and planner
                this.m_points.Clear();
                this.m_planningHelper = null;

                // Clear line helpers
                this.m_lineHelpers.ForEach(this.PlanningCanvas.Children.Remove);
                this.m_lineHelpers.Clear();

                // Clear line helper
                this.LinePlacementHelperBox.Visibility = Visibility.Collapsed;

            }

        } else {
            
            // Place the element
            this.ContextHandler.PlaceElement(this.PlanningCanvas.RenderSize, clickPos);

        }

    }

    private HelperElement CreateSelectedMarker(Point p, int elementId) {
        if (this.ContextHandler.PlaceElementBlueprint is not null) {
            return CreateEntityMarker(this.ContextHandler.PlaceElementBlueprint, p, elementId, true);
        } else if (this.ContextHandler.PlaceElementSquadBlueprint is not null) {
            return CreateSquadMarker(this.ContextHandler.PlaceElementSquadBlueprint, p, elementId);
        }
        return CreateObjectiveMarker(this.ContextHandler.PlaceElemtObjectiveType, p, elementId);
    }

    private static HelperElement CreateEntityMarker(EntityBlueprint ebp, Point p, int elementId, bool legalPlacement) {

        // Grab blueprint
        var sym = ResourceHandler.GetIcon("entity_symbols", ebp.UI.Symbol);
        if (sym is null) {
            return CreateMarker(p);
        }

        // Create marker
        var marker = CreateSomeMarker(sym, p, sym.Width * 0.8, sym.Height * 0.8, elementId);

        // Add overlay if not legal placement
        if (!legalPlacement && marker.Element is Image img) {
            img.Effect = __PlacementNotAllowedEffect;
        }

        // Create marker
        return marker;

    }

    private static HelperElement CreateSquadMarker(SquadBlueprint sbp, Point p, int elementId) {

        // Grab blueprint
        var sym = ResourceHandler.GetIcon("symbol_icons", sbp.UI.Symbol);
        if (sym is null) {
            return CreateMarker(p);
        }

        // Create marker
        return CreateSomeMarker(sym, p, sym.Width * 0.7, sym.Height * 0.7, elementId);

    }

    private static HelperElement CreateObjectiveMarker(PlanningObjectiveType objectiveType, Point p, int elementId) {

        // Grab blueprint
        ImageSource? sym = objectiveType switch {
            PlanningObjectiveType.OT_Attack => LobbyVisualsLookup.ObjectiveTypes[0],
            PlanningObjectiveType.OT_Defend => LobbyVisualsLookup.ObjectiveTypes[1],
            PlanningObjectiveType.OT_Support => LobbyVisualsLookup.ObjectiveTypes[2],
            _ => null
        };
        if (sym is null) {
            return CreateMarker(p);
        }

        // Create marker
        return CreateSomeMarker(sym, p, 24, 24, elementId);

    }

    private static HelperElement CreateSomeMarker(ImageSource sym, Point p, double w, double h, int elementId) {

        // Create transform data
        var offset = new Vector(0.5 * w, 0.5 * h);
        var translate = new TranslateTransform(p.X - offset.X, p.Y - offset.Y);
        var rotate = new RotateTransform();

        // Create image marker
        var marker = new Image() {
            Source = sym,
            Width = w,
            Height = h,
            RenderTransformOrigin = new(0.5, 0.5),
            RenderTransform = new TransformGroup() {
                Children = {
                    rotate, translate
                }
            }
        };

        // Return new marker
        return new(marker, translate, rotate, offset, elementId);

    }

    private static HelperElement CreateMarker(Point p) {

        // Create translate
        var translate = new TranslateTransform(p.X - 0.5 * 20, p.Y - 0.5 * 15);
        var rotate = new RotateTransform();

        // Create marker
        Ellipse marker = new() {
            Fill = Brushes.Blue,
            Stroke = Brushes.Black,
            StrokeThickness = 2.5,
            Width = 20,
            Height = 15,
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

            // Grab origin
            var originTransform = this.m_planningHelper.Translation;
            var origin = new Point(originTransform.X, originTransform.Y);

            // Calc vectors
            var v0 = Vectors.FromTransform(originTransform) + this.m_planningHelper.OffsetVector;
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

                // Grab amount of ingame items
                int ingameCount = this.ContextHandler.GetIngameCount(origin, p, this.ContextHandler.PlacementWidth);

                // Perform placement test
                var selfCap = this.ContextHandler.GetSelfCapacity(ebp.Name);
                bool allowPlacement = selfCap.Test(ingameCount);

                // Get display elements
                LineTo(angle, ebp, v0, v1, this.m_planningHelper.OffsetVector, -1, allowPlacement).ForEach(x => {
                    this.m_lineHelpers.Add(x.Element);
                    this.PlanningCanvas.Children.Add(x.Element);
                    x.Element.MouseLeftButtonUp += this.PlanningCanvas_MouseLeftButtonUp;
                    x.Element.MouseRightButtonUp += this.UserControl_MouseRightButtonUp;
                });

                // Update root
                if (this.m_planningHelper.Element is Image rootElem) {
                    rootElem.Effect = allowPlacement ? null : __PlacementNotAllowedEffect;
                }

                // Show line helper
                this.LinePlacementHelperBox.SetValue(Canvas.TopProperty, p.Y);
                this.LinePlacementHelperBox.SetValue(Canvas.LeftProperty, p.X + 8);
                this.LinePlacementHelperBoxCapacity.Content = BattlegroundsInstance.Localize.GetString("LobbyPlanning_LinePlacing", selfCap.Current + ingameCount, selfCap.Capacity);
                this.LinePlacementHelperBoxCapacity.Foreground = allowPlacement ? Brushes.White : Brushes.Red;

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

    private static List<HelperElement> LineTo(double angle, EntityBlueprint ebp, Vector origin, Vector target, Vector offset, int elementId, bool allowPlacement) {

        // Create container
        var ls = new List<HelperElement>();

        // Calculate distance
        var dist = Vectors.Distance(origin, target);

        // Get amount of fillers
        var w = offset.X * 1.5;
        var lineCount = (int)(dist / w);
        var stepSize = 1.0 / lineCount;

        // Create
        for (int i = 0; i < lineCount; i++) {

            // Calculate helper position
            var v = Vectors.Interpolate(origin, target, i * stepSize);

            // Create helper
            var helper = CreateEntityMarker(ebp, v.ToPoint(), elementId, allowPlacement);
            helper.Rotation.Angle = angle;

            // Add
            ls.Add(helper);

        }

        // Return elements
        return ls;

    }

    private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

        // Reset placement flag
        if (this.ContextHandler.HasPlaceElement) {
            this.ContextHandler.HasPlaceElement = false;
        }

        // Clear stack and helper
        this.m_points.Clear();
        if (this.m_planningHelper?.Element is FrameworkElement fe) {
            this.PlanningCanvas.Children.Remove(fe);
        }
        this.m_planningHelper = null;

        // Clear line helpers (if any)
        if (this.m_lineHelpers.Count > 0) {
            this.m_lineHelpers.ForEach(this.PlanningCanvas.Children.Remove);
            this.m_lineHelpers.Clear();
        }

        // Hide line placement helper if visible
        this.LinePlacementHelperBox.Visibility = Visibility.Collapsed;

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
                    var marker = CreateSquadMarker(sbp, planningObject.VisualPosStart, planningObject.ObjectId);
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
                    var marker = CreateEntityMarker(ebp, planningObject.VisualPosStart, planningObject.ObjectId, true);
                    marker.Element.Tag = marker;

                    // Lookat at target
                    if (planningObject.VisualPointEnd is Point lookat) {
                        var v0 = Vectors.FromTransform(marker.Translation) + marker.OffsetVector;
                        var v1 = Vectors.FromPoint(lookat);
                        var angle = Lookat(marker.Rotation, v0, v1, planningObject.IsLine ? 0 : 90.0);
                        if (planningObject.IsLine) {
                            LineTo(angle, ebp, v0, v1, marker.OffsetVector, planningObject.ObjectId, true).ForEach(x => {
                                x.Element.Tag = marker;
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
                    var marker = CreateObjectiveMarker(planningObject.ObjectiveType, planningObject.VisualPosStart, planningObject.ObjectId);
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

                // Fix pre-place units
                if (planningObject.IsSquad && planningObject.ClientTag is LobbyPlanningUnit u) {

                    // Unpick unit
                    this.ContextHandler.PreplacableUnits.Unpick(u);

                }

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

    private void PlanningCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
        if (this.DataContext is LobbyPlanningOverviewModel lpm) {
            lpm.ContextHandler.MinimapRenderSize = e.NewSize;
        }
    }

    private void DefenceIcon_MouseEnter(object sender, MouseEventArgs e) {
        if (sender is FrameworkElement fe && fe.DataContext is LobbyPlanningDefence def) {
            var cap = def.CapacityFetcher();
            this.DefenceTooltip.Content = $"{GameLocale.GetString(def.Name)} ({cap.Current}/{cap.Capacity})";
            this.DefenceTooltip.Visibility = Visibility.Visible;
        }
    }

    private void DefenceIcon_MouseLeave(object sender, MouseEventArgs e) {
        this.DefenceTooltip.Visibility = Visibility.Hidden;
    }

    private void RegisterRemoveEvent(UIElement e, int index) => e.MouseRightButtonUp += (a, b) => {
        if (a is FrameworkElement element && element.Tag is HelperElement helper) {
            for (int i = 0; i < this.PlanningCanvas.Children.Count; i++) {
                if (this.PlanningCanvas.Children[i] is FrameworkElement fe && fe.Tag is HelperElement otherHelper && helper.ElementId == otherHelper.ElementId) {
                    this.PlanningCanvas.Children.RemoveAt(i--);
                }
            }
            this.ContextHandler.RemoveElement(index);
        }
    };

}
