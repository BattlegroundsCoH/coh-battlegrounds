using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Game.Database.Extensions;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    /// <summary>
    /// Interaction logic for CostControl.xaml
    /// </summary>
    public partial class CostControl : UserControl, INotifyPropertyChanged {

        /// <summary>
        /// Identifies the <see cref="Cost"/> property.
        /// </summary>
        public static readonly DependencyProperty CostProperty
            = DependencyProperty.Register(nameof(Cost), typeof(CostExtension), typeof(CostControl),
                new FrameworkPropertyMetadata(new CostExtension(),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (a, b) => (a as CostControl).Cost = b.NewValue as CostExtension));

        public event PropertyChangedEventHandler PropertyChanged;

        public int ManpowerCost => (int)(this.Cost?.Manpower ?? 0.0f);

        public int MunitionCost => (int)(this.Cost?.Munitions ?? 0.0f);

        public int FuelCost => (int)(this.Cost?.Fuel ?? 0.0f);

        public CostExtension Cost {
            get => this.GetValue(CostProperty) as CostExtension;
            set => this.SetCost(value);
        }

        public CostControl() {
            this.InitializeComponent();
        }

        private void SetCost(CostExtension value) {

            // Set actual value
            this.SetValue(CostProperty, value);

            this.ManpowerCostIcon.Visibility = this.ManpowerCost is 0 ? Visibility.Collapsed : Visibility.Visible;
            this.MunitionCostIcon.Visibility = this.MunitionCost is 0 ? Visibility.Collapsed : Visibility.Visible;
            this.FuelCostIcon.Visibility = this.FuelCost is 0 ? Visibility.Collapsed : Visibility.Visible;

            this.ManpowerCostValue.Content = value.Manpower;
            this.MunitionCostValue.Content = value.Munitions;
            this.FuelCostValue.Content = value.Fuel;

        }

    }

}
