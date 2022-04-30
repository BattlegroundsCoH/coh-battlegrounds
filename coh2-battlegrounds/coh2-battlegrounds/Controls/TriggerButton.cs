using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp.Controls;

public class TriggerButton : Button {

    public bool IsTriggered {
        get => (bool)this.GetValue(IsTriggeredProperty);
        set => this.SetValue(IsTriggeredProperty, value);
    }

    public static readonly DependencyProperty IsTriggeredProperty =
        DependencyProperty.Register(nameof(IsTriggered), typeof(bool), typeof(TriggerButton), new PropertyMetadata(false));

    protected override void OnClick() {
        base.OnClick();
        this.IsTriggered = !this.IsTriggered;
    }

}
