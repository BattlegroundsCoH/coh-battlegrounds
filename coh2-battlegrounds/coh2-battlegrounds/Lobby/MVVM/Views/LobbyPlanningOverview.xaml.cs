using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using Battlegrounds.Game.Database;

using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.Lobby.Planning;
using BattlegroundsApp.Utilities;

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

    }

    private void PlanningCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        if (!this.ContextHandler.HasPlaceElement) {
            return;
        }

        var clickPos = e.GetPosition(this.PlanningCanvas);

        if (this.ContextHandler.RequiresSecond) {
            if (this.m_points.Count is 0) {
                this.m_points.Push(clickPos);
                var marker = this.PlaceInitial(this.ContextHandler.PlaceElementBlueprint, clickPos);
                this.m_planningHelper = marker;
                this.PlanningCanvas.Children.Add(marker.Element);
            } else {
                
                // Place
                this.ContextHandler.PlaceElement(this.m_points.Pop(), clickPos);
                
                // Clear
                this.m_points.Clear();
                this.m_planningHelper = null;

            }
        } else {
            this.ContextHandler.PlaceElement(clickPos);
        }        

    }

    private HelperElement PlaceInitial(EntityBlueprint ebp, Point p) {

        // Grab blueprint
        var sym = App.ResourceHandler.GetIcon("entity_symbols", ebp.UI.Symbol);
        if (sym is null) {
            return this.CreateMarker(p);
        }

        // Create transform data
        var offset = new Vector(0.5 * sym.Width, 0.5 * sym.Height);
        var translate = new TranslateTransform(p.X - offset.X, p.Y - offset.Y);
        var rotate = new RotateTransform();

        // Create image marker
        var marker = new Image() {
            Source = sym,
            Width=sym.Width,
            Height=sym.Height,
            RenderTransformOrigin = new(0.5,0.5),
            RenderTransform = new TransformGroup() {
                Children = {
                    rotate, translate
                }
            }
        };

        // Return new marker
        return new(marker, translate, rotate, offset);

    }

    private HelperElement CreateMarker(Point p) {

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
        if (!this.ContextHandler.HasPlaceElement) {
            return;
        }

        // Grab point
        var p = e.GetPosition(this.PlanningCanvas);

        // If helper, rotate if directional
        if (this.m_planningHelper is not null) {

            // Calc pos
            var v0 = Vectors.FromTransform(this.m_planningHelper.Translation) + this.m_planningHelper.OffsetVector;

            // Grab placement data
            var data = this.ContextHandler.PlaceElementData;

            // Compute angle
            var v1 = Vectors.FromPoint(p);
            var v2 = v1 - v0;

            // Grab the angle
            var modAngle = data.Value.IsLinePlacement ? 0.0 : 90.0;
            var angle = Math.Atan2(v2.Y, v2.X) * 57.29578 + modAngle;

            // Set angle
            this.m_planningHelper.Rotation.Angle = angle;

            // Do line placement
            if (data.Value.IsLinePlacement) {

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
                    var helper = this.PlaceInitial(this.ContextHandler.PlaceElementBlueprint, v3.ToPoint());
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

}
