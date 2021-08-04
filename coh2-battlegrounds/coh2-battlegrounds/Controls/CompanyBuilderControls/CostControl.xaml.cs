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

        public Visibility ManpowerCostVisible => this.ManpowerCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility MunitionCostVisible => this.MunitionCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public Visibility FuelCostVisible => this.FuelCost is 0 ? Visibility.Collapsed : Visibility.Visible;

        public int ManpowerCost => (int)(this.Cost?.Manpower ?? 0.0f);

        public int MunitionCost => (int)(this.Cost?.Munitions ?? 0.0f);

        public int FuelCost => (int)(this.Cost?.Fuel ?? 0.0f);

        public CostExtension Cost {
            get => this.GetValue(CostProperty) as CostExtension;
            set => this.SetCost(value);
        }

        public CostControl() {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private void SetCost(CostExtension value) {

            // Set actual value
            this.SetValue(CostProperty, value);

            // Notify value changes
            this.PropertyChanged?.Invoke(this, new(nameof(this.ManpowerCost)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.ManpowerCostVisible)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.MunitionCost)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.MunitionCostVisible)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.FuelCost)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.FuelCostVisible)));
            this.PropertyChanged?.Invoke(this, new(nameof(this.Cost)));

        }

    }

}
