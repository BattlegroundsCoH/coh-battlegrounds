using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Battlegrounds.Behaviors;

/// <summary>
/// Invokes a command when a <see cref="ToggleButton"/> is checked or unchecked.
/// </summary>
/// <remarks>
/// The counterpart to <see cref="SelectionChangedCommandBehavior"/> for checkboxes: it lets a
/// draft/apply view-model commit implicitly on toggle without the view-model itself having to
/// collapse the two steps — the split stays intact for anything that needs an explicit commit.
/// </remarks>
public static class ToggleCommandBehavior {

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(ToggleCommandBehavior),
        new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
        "CommandParameter",
        typeof(object),
        typeof(ToggleCommandBehavior));

    public static void SetCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(CommandProperty);

    public static void SetCommandParameter(DependencyObject element, object? value) =>
        element.SetValue(CommandParameterProperty, value);

    public static object? GetCommandParameter(DependencyObject element) =>
        element.GetValue(CommandParameterProperty);

    private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
        if (dependencyObject is not ToggleButton toggleButton) {
            return;
        }
        if (args.OldValue is not null) {
            toggleButton.Checked -= OnToggled;
            toggleButton.Unchecked -= OnToggled;
        }
        if (args.NewValue is not null) {
            toggleButton.Checked += OnToggled;
            toggleButton.Unchecked += OnToggled;
        }
    }

    private static void OnToggled(object sender, RoutedEventArgs args) {
        if (sender is not ToggleButton toggleButton) {
            return;
        }
        // Deferred for the same reason SelectionChangedCommandBehavior defers: the bound
        // draft property is written by the binding after the event, so executing inline
        // would send the previous value.
        _ = toggleButton.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
            var command = GetCommand(toggleButton);
            var parameter = GetCommandParameter(toggleButton);
            if (command?.CanExecute(parameter) == true) {
                command.Execute(parameter);
            }
        });
    }

}
