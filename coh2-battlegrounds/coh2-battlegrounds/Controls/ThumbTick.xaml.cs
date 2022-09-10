using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Controls;

/// <summary>
/// Interaction logic for ThumbTick.xaml
/// </summary>
public partial class ThumbTick : UserControl {

    public static readonly DependencyProperty IsTickedProperty =
        DependencyProperty.Register(nameof(IsTicked), typeof(bool), typeof(ThumbTick), new PropertyMetadata(false, (a, b) => {
            if (a is ThumbTick t) {
                t.GotoTickState((bool)b.NewValue);
                t.GetBindingExpression(IsTickedProperty)?.UpdateSource();
            }
        }));

    [Bindable(true)]
    public bool IsTicked {
        get => (bool)this.GetValue(IsTickedProperty);
        set => this.SetCurrentValue(IsTickedProperty, value);
    }

    public ThumbTick() {
        InitializeComponent();
    }

    private void SelfThumb_MouseDown(object sender, MouseButtonEventArgs e) {
        this.IsTicked = !this.IsTicked;
    }

    private void GotoTickState(bool isTicked) {

        // Grab time
        var dur = TimeSpan.Parse("0:0:0.08");

        // Grab colours
        var disabledColour = (Color)this.FindResource("BackgroundLightGray");
        var enabledColour = (Color)App.Current.FindResource("BackgroundDarkBlue");

        // Create animations
        var backgroundAnimation = new DoubleAnimation(!isTicked ? 92.0 : 0.0, !isTicked ? 0.0 : 92.0, dur);
        var offsetAnimation = new DoubleAnimation(!isTicked ? 46.0 : 0.0, !isTicked ? 0.0 : 46.0, dur);
        var colourAnimation = new ColorAnimation(!isTicked ? enabledColour : disabledColour, !isTicked ? disabledColour : enabledColour, dur);

        // Player animations
        this.BackgroundEnable.BeginAnimation(Ellipse.WidthProperty, backgroundAnimation);
        this.SelfThumb.RenderTransform.BeginAnimation(TranslateTransform.XProperty, offsetAnimation);
        ((SolidColorBrush)this.FindResource("TickColour")).BeginAnimation(SolidColorBrush.ColorProperty, colourAnimation);

    }

}
