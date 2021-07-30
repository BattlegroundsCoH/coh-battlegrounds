using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BattlegroundsApp.Controls {

    /// <summary>
    /// Interaction logic for IconTextLabel.xaml
    /// </summary>
    public partial class IconTextLabel : UserControl {

        public ImageSource Icon { get => this.img.Source; set => this.img.Source = value; }

        public string Text { get => this.lbl.Content.ToString(); set => this.lbl.Content = value; }

        public event MouseButtonEventHandler OnLeftMouseButtonClick;

        public IconTextLabel() {
            InitializeComponent();
            this.img.MouseLeftButtonDown += this.Img_MouseLeftButtonDown;
            this.lbl.MouseLeftButtonDown += this.Lbl_MouseLeftButtonDown;
            this.MouseLeftButtonDown += this.IconTextLabel_MouseLeftButtonDown;
        }

        private void IconTextLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.Onclick(sender, e);

        private void Lbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.Onclick(sender, e);

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.Onclick(sender, e);

        private void Onclick(object sender, MouseButtonEventArgs e) => this.OnLeftMouseButtonClick?.Invoke(sender, e);

    }

}
