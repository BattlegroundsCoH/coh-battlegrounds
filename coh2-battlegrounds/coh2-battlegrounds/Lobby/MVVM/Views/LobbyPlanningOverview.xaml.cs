using System;
using System.Collections.Generic;
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

using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.Lobby.Planning;
using BattlegroundsApp.Modals;

namespace BattlegroundsApp.Lobby.MVVM.Views;

/// <summary>
/// Interaction logic for LobbyPlanningOverview.xaml
/// </summary>
public partial class LobbyPlanningOverview : UserControl {

    private readonly Stack<Point> m_points;
    private readonly List<UIElement> m_planningHelpers;

    public LobbyPlanningContextHandler ContextHandler 
        => this.DataContext is LobbyPlanningOverviewModel lpom ? lpom.ContextHandler : throw new Exception();

    public LobbyPlanningOverview() {

        // Init component
        this.InitializeComponent();
        
        // Create lists
        this.m_points = new();
        this.m_planningHelpers = new();

    }

    private void PlanningCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

        if (!this.ContextHandler.HasPlaceElement) {
            return;
        }

        var clickPos = e.GetPosition(this.PlanningCanvas);

        if (this.ContextHandler.RequiresSecond) {
            if (this.m_points.Count is 0) {
                this.m_points.Push(clickPos);
                var marker = this.CreateMarker();
                this.m_planningHelpers.Add(marker);
                this.PlanningCanvas.Children.Add(marker);
                marker.SetValue(Canvas.TopProperty, clickPos.Y);
                marker.SetValue(Canvas.LeftProperty, clickPos.X);
            } else {
                this.ContextHandler.PlaceElement(this.m_points.Pop(), clickPos);
            }
        } else {
            this.ContextHandler.PlaceElement(clickPos);
        }
        

    }

    private Ellipse CreateMarker() {

        Ellipse marker = new() {
            Fill = Brushes.Blue,
            Stroke = Brushes.Black,
            Width = 25,
            Height = 25,
            //RenderTransformOrigin = new(12.5,12.5)
        };

        return marker;

    }

    private void PlanningCanvas_MouseMove(object sender, MouseEventArgs e) {

        if (!this.ContextHandler.HasPlaceElement) {
            return;
        }



    }

    private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

        if (this.ContextHandler.HasPlaceElement) {
            this.ContextHandler.HasPlaceElement = false;
        }

        // Clear stack
        this.m_points.Clear();

    }

}
