using System.Windows;
using System.Windows.Input;

namespace Battlegrounds.UI.Controls;

/// <summary>
/// 
/// </summary>
public class IconToggleButton : IconButton {

    /// <summary>
    /// 
    /// </summary>
    public bool IsToggled {
        get => (bool)this.GetValue(IsToggledProperty);
        set => this.SetCurrentValue(IsToggledProperty, value);
    }

    public static readonly DependencyProperty IsToggledProperty =
        DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(IconToggleButton), new PropertyMetadata(false));

    protected override void OnMouseDown(MouseButtonEventArgs e) {
        base.OnMouseDown(e);
        this.IsToggled = !this.IsToggled;
    }

}
