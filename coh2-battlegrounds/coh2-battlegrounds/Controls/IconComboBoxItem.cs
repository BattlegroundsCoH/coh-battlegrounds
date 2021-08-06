using System.Windows.Media;

namespace BattlegroundsApp.Controls {

    public class IconComboBoxItem {

        public ImageSource Icon { get; }

        public string Text { get; }

        public object Source { get; set; }

        public bool HasText => !string.IsNullOrEmpty(this.Text);

        public IconComboBoxItem(ImageSource icon, object text, object source) {
            this.Icon = icon;
            this.Text = text?.ToString() ?? string.Empty;
            this.Source = source;
        }

        public IconComboBoxItem(ImageSource icon, object text) : this(icon, text, null) { }

        public IconComboBoxItem(ImageSource icon) : this(icon, null, null) { }

        public bool GetSource<T>(out T source) {
            if (this.Source is T s) {
                source = s;
                return true;
            } else {
                source = default;
                return false;
            }
        }

    }

}
