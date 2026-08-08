using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using Battlegrounds.Behaviors;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.Test.Behaviors;

[TestOf(typeof(ValueCommittedCommandBehavior))]
[Apartment(ApartmentState.STA)]
public sealed class ValueCommittedCommandBehaviorTests {

    [Test]
    public void ValueChangeAlone_DoesNotExecute() {
        var executions = 0;
        var slider = NewSlider(new RelayCommand(() => executions++));

        slider.Value = 700;
        PumpDispatcher();

        Assert.That(executions, Is.Zero, "dragging should not fire the command on every tick");
    }

    [Test]
    public void DragCompleted_ExecutesCommand() {
        var executions = 0;
        var slider = NewSlider(new RelayCommand(() => executions++));

        slider.Value = 700;
        RaiseDragCompleted(slider);
        PumpDispatcher();

        Assert.That(executions, Is.EqualTo(1));
    }

    [Test]
    public void DragCompleted_WhenCommandCannotExecute_DoesNotExecute() {
        var executions = 0;
        var slider = NewSlider(new RelayCommand(() => executions++, () => false));

        slider.Value = 700;
        RaiseDragCompleted(slider);
        PumpDispatcher();

        Assert.That(executions, Is.Zero);
    }

    [Test]
    public void ClearingCommand_Unsubscribes() {
        var executions = 0;
        var slider = NewSlider(new RelayCommand(() => executions++));

        ValueCommittedCommandBehavior.SetCommand(slider, null);
        slider.Value = 700;
        RaiseDragCompleted(slider);
        PumpDispatcher();

        Assert.That(executions, Is.Zero);
    }

    private static Slider NewSlider(ICommand command) {
        var slider = new Slider { Minimum = 100, Maximum = 1000, Value = 500 };
        ValueCommittedCommandBehavior.SetCommand(slider, command);
        return slider;
    }

    /// <summary>The thumb raises this in a real drag; the behavior catches it as it bubbles.</summary>
    private static void RaiseDragCompleted(Slider slider) =>
        slider.RaiseEvent(new DragCompletedEventArgs(0, 0, false) {
            RoutedEvent = Thumb.DragCompletedEvent
        });

    private static void PumpDispatcher() {
        var frame = new DispatcherFrame();
        _ = Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            () => frame.Continue = false);
        Dispatcher.PushFrame(frame);
    }

}
