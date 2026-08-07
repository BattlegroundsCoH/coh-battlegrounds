using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Battlegrounds.Behaviors;

public static class SelectionChangedCommandBehavior {

    public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
        "Command",
        typeof(ICommand),
        typeof(SelectionChangedCommandBehavior),
        new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
        "CommandParameter",
        typeof(object),
        typeof(SelectionChangedCommandBehavior));

    public static void SetCommand(DependencyObject element, ICommand? value) =>
        element.SetValue(CommandProperty, value);

    public static ICommand? GetCommand(DependencyObject element) =>
        (ICommand?)element.GetValue(CommandProperty);

    public static void SetCommandParameter(DependencyObject element, object? value) =>
        element.SetValue(CommandParameterProperty, value);

    public static object? GetCommandParameter(DependencyObject element) =>
        element.GetValue(CommandParameterProperty);

    private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
        if (dependencyObject is not ComboBox comboBox) {
            return;
        }
        if (args.OldValue is not null) {
            comboBox.SelectionChanged -= OnSelectionChanged;
        }
        if (args.NewValue is not null) {
            comboBox.SelectionChanged += OnSelectionChanged;
        }
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs args) {
        if (sender is not ComboBox comboBox) {
            return;
        }
        _ = comboBox.Dispatcher.BeginInvoke(DispatcherPriority.Background, () => {
            var command = GetCommand(comboBox);
            var parameter = GetCommandParameter(comboBox);
            if (command?.CanExecute(parameter) == true) {
                command.Execute(parameter);
            }
        });
    }

}
