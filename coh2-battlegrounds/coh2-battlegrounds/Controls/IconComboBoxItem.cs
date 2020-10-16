using System.Windows.Media;

namespace BattlegroundsApp.Controls {
    
    public class IconComboBoxItem {

        ImageSource m_icon;
        string m_text;

        public ImageSource Icon => this.m_icon;

        public string Text => this.m_text;

        public object Source { get; set; }

        public bool HasText => !string.IsNullOrEmpty(this.m_text);

        public IconComboBoxItem(ImageSource icon, object text) {
            this.m_icon = icon;
            this.m_text = text?.ToString() ?? string.Empty;
        }

        public IconComboBoxItem(ImageSource icon) : this(icon, null) { }

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
