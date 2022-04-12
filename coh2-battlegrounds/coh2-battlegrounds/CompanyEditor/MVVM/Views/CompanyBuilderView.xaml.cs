using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using BattlegroundsApp.CompanyEditor.MVVM.Models;

namespace BattlegroundsApp.CompanyEditor.MVVM.Views;

/// <summary>
/// Interaction logic for CompanyBuilderView.xaml
/// </summary>
public partial class CompanyBuilderView : UserControl {

    private CompanyBuilderViewModel ViewModel => this.DataContext is CompanyBuilderViewModel vm ? vm : throw new InvalidOperationException();

    private int m_selectedMainTab = -1;
    private int m_selectedSubUnitTab = -1;
    private int m_selectedSubAbilityTab = -1;

    private readonly StackPanel[][] m_panels;

    public CompanyBuilderView() {
        this.InitializeComponent();
        this.m_panels = new StackPanel[][] {
            new StackPanel[] { this.InfantryPanel, this.SupportPanel, this.VehiclePanel },
            new StackPanel[] { this.CommanderAbilitiyPanel, this.UnitAbilityPanel }
        };
    }

    private void OnItemDrop(object sender, DragEventArgs e) 
        => this.ViewModel.Drop?.Invoke(this, this.ViewModel, e);

    private void ChangeMainTab(object sender, SelectionChangedEventArgs e) {
        if (this.DataContext is null) {
            return;
        }
        this.ViewModel.Change?.Invoke(this, this.ViewModel, e);
        this.m_selectedMainTab = this.ViewModel.SelectedMainTab;
        this.RHS_ScrollBar_Refresh();
    }

    private void ChangeSubUnitTab(object sender, SelectionChangedEventArgs e) {
        if (this.DataContext is null) {
            return;
        }
        this.ViewModel.Change?.Invoke(this, this.ViewModel, e);
        this.m_selectedSubUnitTab = this.ViewModel.SelectedUnitTabItem;
        this.RHS_ScrollBar_Refresh();
    }

    private void ChangeSubAbilityTab(object sender, SelectionChangedEventArgs e) {
        if (this.DataContext is null) {
            return;
        }
        this.ViewModel.Change?.Invoke(this, this.ViewModel, e);
        this.m_selectedSubAbilityTab = this.ViewModel.SelectedAbilityTabItem;
        this.RHS_ScrollBar_Refresh();
    }

    private void RHS_ScrollBar_Scroll(object sender, ScrollEventArgs e) {

        // Do basic scroll checks
        if (this.m_selectedMainTab is -1 || this.m_selectedSubUnitTab is -1 || this.m_selectedSubAbilityTab is -1)
            return;

        // Bail if main tab is out of range
        if (this.m_selectedMainTab is < 0 or > 1)
            return;

        // Mark handled
        e.Handled = true;

        // Determine decider
        int j = this.m_selectedMainTab == 0 ? this.m_selectedSubUnitTab : this.m_selectedSubAbilityTab;

        // Set canvas top
        this.m_panels[this.m_selectedMainTab][j].SetValue(Canvas.TopProperty, -e.NewValue);

    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
        this.RHS_ScrollBar_Refresh();
    }

    private void RHS_ScrollBar_Refresh() {

        // Bail if not fully initted
        if (this.m_selectedSubAbilityTab is -1 || this.m_selectedSubUnitTab is -1) {
            return;
        }

        // Grab active element
        var active = this.m_selectedMainTab switch {
            0 => this.m_selectedSubUnitTab switch {
                0 => this.InfantryTab,
                1 => this.SupportTab,
                2 => this.VehicleTab,
                _ => throw new IndexOutOfRangeException()
            },
            1 => this.m_selectedSubAbilityTab switch {
                0 => this.CommanderAbilityTab,
                1 => this.UnitAbilityTab,
                _ => throw new IndexOutOfRangeException()
            },
            _ => null
        };

        // If no active, bail
        if (active is null) {

            // Set default values
            this.RHS_ScrollBar.Minimum = 0;
            this.RHS_ScrollBar.Maximum = 0;
            this.RHS_ScrollBar.Value = 0;
            this.RHS_ScrollBar.ViewportSize = 1;
            
            // Bail
            return;

        }

        // Determine decider
        int j = this.m_selectedMainTab == 0 ? this.m_selectedSubUnitTab : this.m_selectedSubAbilityTab;

        // Grab essentials
        double containerHeight = (active.Content as FrameworkElement)?.ActualHeight ?? 0;
        double canvasHeight = this.m_panels[this.m_selectedMainTab][j].ActualHeight;

        // Bail -> Not ready yet
        if (containerHeight == 0 || canvasHeight == 0)
            return;

        // If canvas height is less than container height, we clean the scroll bar
        if (canvasHeight < containerHeight) {

            // Disable stuff
            this.RHS_ScrollBar.Minimum = 0;
            this.RHS_ScrollBar.Maximum = 0;
            this.RHS_ScrollBar.Value = 0;
            this.RHS_ScrollBar.ViewportSize = 1;

        } else { // Else we calculate new value

            // Calculate diff
            var diff = canvasHeight - containerHeight;

            // Grab existing scroll offset if any
            var off = -(double)this.m_panels[this.m_selectedMainTab][j].GetValue(Canvas.TopProperty);

            // Set
            this.RHS_ScrollBar.Minimum = 0;
            this.RHS_ScrollBar.Maximum = diff;
            this.RHS_ScrollBar.Value = double.IsNaN(off) ? 0 : off;
            this.RHS_ScrollBar.ViewportSize = (containerHeight / canvasHeight) * this.RHS_ScrollBar.ActualHeight;

        }

    }

    private void SupportPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        => this.RHS_ScrollBar_Refresh();

    private void VehiclePanel_SizeChanged(object sender, SizeChangedEventArgs e)
        => this.RHS_ScrollBar_Refresh();

    private void InfantryPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        => this.RHS_ScrollBar_Refresh();

    private void RHS_ScrollBar_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
        if (this.RHS_ScrollBar.Maximum > 0) {
            this.RHS_ScrollBar.Value -= e.Delta * 0.25;
            this.RHS_ScrollBar_Scroll(sender, new(ScrollEventType.ThumbTrack, this.RHS_ScrollBar.Value)); // For some reason this is not triggered when changing the value
        }
    }

}
