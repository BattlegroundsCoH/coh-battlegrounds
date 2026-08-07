using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

using Battlegrounds.Behaviors;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.Test.Behaviors;

[TestOf(typeof(SelectionChangedCommandBehavior))]
[Apartment(ApartmentState.STA)]
public sealed class SelectionChangedCommandBehaviorTests {

    [Test]
    public void SelectionChange_ExecutesCommandWithCurrentParameter() {
        object? received = null;
        var command = new RelayCommand<object?>(parameter => received = parameter);
        var comboBox = new ComboBox {
            ItemsSource = new[] { "first", "second" }
        };
        SelectionChangedCommandBehavior.SetCommand(comboBox, command);

        comboBox.SelectedItem = "second";
        SelectionChangedCommandBehavior.SetCommandParameter(comboBox, comboBox.SelectedItem);
        PumpDispatcher();

        Assert.That(received, Is.EqualTo("second"));
    }

    [Test]
    public void SelectionChange_WhenCommandCannotExecute_DoesNotExecute() {
        var executions = 0;
        var command = new RelayCommand(
            () => executions++,
            () => false);
        var comboBox = new ComboBox {
            ItemsSource = new[] { "first", "second" }
        };
        SelectionChangedCommandBehavior.SetCommand(comboBox, command);

        comboBox.SelectedItem = "second";
        PumpDispatcher();

        Assert.That(executions, Is.Zero);
    }

    private static void PumpDispatcher() {
        var frame = new DispatcherFrame();
        _ = Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            () => frame.Continue = false);
        Dispatcher.PushFrame(frame);
    }

}
