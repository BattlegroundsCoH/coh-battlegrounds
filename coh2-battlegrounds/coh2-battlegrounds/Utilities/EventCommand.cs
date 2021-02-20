using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BattlegroundsApp.Utilities {

    public class EventCommand : DoubleAnimationBase {

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventCommand), null);
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventCommand), null);

        public ICommand Command {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        protected override Freezable CreateInstanceCore() => new EventCommand();

        protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue, AnimationClock animationClock) {
            Command.Execute(this.CommandParameter);
            return 0.0;
        }
    
    }

}
