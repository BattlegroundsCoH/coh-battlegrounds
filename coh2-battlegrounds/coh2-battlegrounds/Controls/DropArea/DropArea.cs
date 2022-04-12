using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BattlegroundsApp.Controls.DropArea {

    [ContentProperty(nameof(ControlContent))]
    public class DropArea : UserControl {

        public static readonly DependencyProperty ControlContentProperty = DependencyProperty.Register(nameof(ControlContent), typeof(object), typeof(DropArea));

        public object ControlContent {
            get => this.GetValue(ControlContentProperty);
            set => this.SetValue(ControlContentProperty, value);
        }

        static DropArea() {
            try {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(DropArea), new FrameworkPropertyMetadata(typeof(DropArea)));
            } catch { }
        }

    }
}
