using System.Windows;

namespace BattlegroundsApp.Controls {

    public class IconToggleButton : IconButton {

        public bool IsToggled {
            get => (bool)this.GetValue(IsToggledProperty);
            set => this.SetValue(IsToggledProperty, value);
        }

        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(IconToggleButton), new PropertyMetadata(false));

        protected override void OnClick() {
            base.OnClick();
            this.IsToggled = !this.IsToggled;
        }

    }

}
