using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.UI.Controls;

/// <summary>
/// 
/// </summary>
public class TriggerButton : Button {

    /// <summary>
    /// 
    /// </summary>
    public bool IsTriggered {
        get => (bool)this.GetValue(IsTriggeredProperty);
        set => this.SetCurrentValue(IsTriggeredProperty, value);
    }

    public static readonly DependencyProperty IsTriggeredProperty =
        DependencyProperty.Register(nameof(IsTriggered), typeof(bool), typeof(TriggerButton), new PropertyMetadata(false));

    protected override void OnClick() {
        base.OnClick();
        this.IsTriggered = !this.IsTriggered;
    }

}
