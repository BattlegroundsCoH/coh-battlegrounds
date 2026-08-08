using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

using Battlegrounds.Behaviors;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.Test.Behaviors;

[TestOf(typeof(ToggleCommandBehavior))]
[Apartment(ApartmentState.STA)]
public sealed class ToggleCommandBehaviorTests {

    [Test]
    public void Check_ExecutesCommand() {
        var executions = 0;
        var checkBox = new CheckBox { IsChecked = false };
        ToggleCommandBehavior.SetCommand(checkBox, new RelayCommand(() => executions++));

        checkBox.IsChecked = true;
        PumpDispatcher();

        Assert.That(executions, Is.EqualTo(1));
    }

    [Test]
    public void Uncheck_ExecutesCommand() {
        var executions = 0;
        var checkBox = new CheckBox { IsChecked = true };
        ToggleCommandBehavior.SetCommand(checkBox, new RelayCommand(() => executions++));

        checkBox.IsChecked = false;
        PumpDispatcher();

        Assert.That(executions, Is.EqualTo(1));
    }

    [Test]
    public void Toggle_WhenCommandCannotExecute_DoesNotExecute() {
        var executions = 0;
        var checkBox = new CheckBox { IsChecked = false };
        ToggleCommandBehavior.SetCommand(checkBox, new RelayCommand(() => executions++, () => false));

        checkBox.IsChecked = true;
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
