using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Functional;

namespace BattlegroundsApp.Controls;

public enum SpinnerStatus {
    Spinning,
    Accepted,
    Refused
}

/// <summary>
/// Interaction logic for SpinnerCheck.xaml
/// </summary>
public partial class SpinnerCheck : UserControl {

    private SpinnerStatus m_spinnerStatus;

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(SpinnerStatus), typeof(SpinnerCheck), new(SpinnerStatus.Spinning, (a,b) => a.Cast<SpinnerCheck>(x => x.Status = (SpinnerStatus)b.NewValue)));

    public SpinnerStatus Status {
        get => this.m_spinnerStatus;
        set {
            this.m_spinnerStatus = value;
            this.Circle.Visibility = this.IsSpnningVisible;
            this.Checkmark.Visibility = this.IsAcceptedVisible;
            this.NopeMarkA.Visibility = this.NopeMarkB.Visibility = this.IsRefusalVisible;
        }
    }

    public Visibility IsRefusalVisible => this.Status is SpinnerStatus.Refused ? Visibility.Visible : Visibility.Collapsed;

    public Visibility IsSpnningVisible => this.Status is SpinnerStatus.Spinning ? Visibility.Visible : Visibility.Collapsed;

    public Visibility IsAcceptedVisible => this.Status is SpinnerStatus.Accepted ? Visibility.Visible : Visibility.Collapsed;

    public SpinnerCheck() {
        this.InitializeComponent();
        this.Status = SpinnerStatus.Spinning; // For some reason this has to be manually set, because fuck WPF
    }

}
