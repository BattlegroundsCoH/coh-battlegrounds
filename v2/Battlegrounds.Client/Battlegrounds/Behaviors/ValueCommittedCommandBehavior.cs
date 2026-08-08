using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Battlegrounds.Behaviors;

/// <summary>
/// Invokes a command when a <see cref="Slider"/>'s value settles, rather than on every tick.
/// </summary>
/// <remarks>
/// A slider bound straight to a command would fire once per pixel of travel; this waits for the
/// gesture to end. Three inputs can end one: releasing the thumb (which also covers a click on
/// the rail, because IsMoveToPointEnabled starts a drag), releasing an arrow key, and leaving
/// the control. All three are hooked, and firing twice is harmless — a draft/apply view-model
/// reports CanExecute false once the draft matches the confirmed value.
/// </remarks>
public static class ValueCommittedCommandBehavior {

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(ValueCommittedCommandBehavior),
        new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
        "CommandParameter",
        typeof(object),
        typeof(ValueCommittedCommandBehavior));

    public static void SetCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(CommandProperty);

    public static void SetCommandParameter(DependencyObject element, object? value) =>
        element.SetValue(CommandParameterProperty, value);

    public static object? GetCommandParameter(DependencyObject element) =>
        element.GetValue(CommandParameterProperty);

    private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
        if (dependencyObject is not Slider slider) {
            return;
        }
        if (args.OldValue is not null) {
            slider.RemoveHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)OnDragCompleted);
            slider.KeyUp -= OnKeyUp;
            slider.LostKeyboardFocus -= OnLostKeyboardFocus;
        }
        if (args.NewValue is not null) {
            // The thumb raises DragCompleted, not the slider, so it has to be caught on the
            // way up rather than subscribed to directly.
            slider.AddHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)OnDragCompleted);
            slider.KeyUp += OnKeyUp;
            slider.LostKeyboardFocus += OnLostKeyboardFocus;
        }
    }

    private static void OnDragCompleted(object sender, DragCompletedEventArgs args) => Commit(sender);

    private static void OnKeyUp(object sender, KeyEventArgs args) => Commit(sender);

    private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args) => Commit(sender);

    private static void Commit(object sender) {
        if (sender is not Slider slider) {
            return;
        }
        _ = slider.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
            var command = GetCommand(slider);
            var parameter = GetCommandParameter(slider);
            if (command?.CanExecute(parameter) == true) {
                command.Execute(parameter);
            }
        });
    }

}
