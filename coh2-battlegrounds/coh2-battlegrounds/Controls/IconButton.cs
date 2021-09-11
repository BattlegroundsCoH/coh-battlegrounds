using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BattlegroundsApp.Controls {

    // https://kmatyaszek.github.io/2020/04/16/wpf-image-button.html
    // ^ That man saved my sanity - WPF plain sucks

    public class IconButton : Button {

        static IconButton() {
            try {
                DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
            } catch {
            }
        }

        public int ImageWidth {
            get => (int)this.GetValue(ImageWidthProperty);
            set => this.SetValue(ImageWidthProperty, value);
        }

        public static readonly DependencyProperty ImageWidthProperty =
            DependencyProperty.Register(nameof(ImageWidth), typeof(int), typeof(IconButton), new PropertyMetadata(30));

        public int ImageHeight {
            get => (int)this.GetValue(ImageHeightProperty);
            set => this.SetValue(ImageHeightProperty, value);
        }

        public static readonly DependencyProperty ImageHeightProperty =
            DependencyProperty.Register(nameof(ImageHeight), typeof(int), typeof(IconButton), new PropertyMetadata(30));

        public ImageSource ImageSource {
            get => (ImageSource)this.GetValue(ImageSourceProperty);
            set => this.SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(IconButton), new PropertyMetadata(null));

        public object HoverColour {
            get => (SolidColorBrush)this.GetValue(HoverColourProperty);
            set {
                if (value is SolidColorBrush brush) {
                    this.SetValue(HoverColourProperty, brush);
                } else if (value is Color col) {
                    this.SetValue(HoverColourProperty, new SolidColorBrush(col));
                } else if (value is string s) {
                    try {
                        this.SetValue(HoverColourProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString(s)));
                    } catch { }
                }
            }
        }

        public static readonly DependencyProperty HoverColourProperty =
            DependencyProperty.Register(nameof(HoverColour), typeof(object), typeof(IconButton), new PropertyMetadata("#536375"));

    }

}
