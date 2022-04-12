using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Game.Database.Extensions;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    /// <summary>
    /// Interaction logic for CostControl.xaml
    /// </summary>
    public partial class CostControl : UserControl {


        private static readonly double[] lblLeft = { 0, 30.8333, 61.66 };
        private static readonly double[] icoLeft = { 4, 34.25, 65 };

        private static readonly double[] widths = { 92.5, 92.5 - (92.5 / 3.0), 92.5 / 3.0, 0 };

        private readonly Label[] ValueLabels;
        private readonly Image[] Icons;

        /// <summary>
        /// Identifies the <see cref="Cost"/> property.
        /// </summary>
        public static readonly DependencyProperty CostProperty
            = DependencyProperty.Register(nameof(Cost), typeof(CostExtension), typeof(CostControl),
                new FrameworkPropertyMetadata(new CostExtension(),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (a, b) => (a as CostControl).Cost = b.NewValue as CostExtension));

        public int ManpowerCost => (int)(this.Cost?.Manpower ?? 0.0f);

        public int MunitionCost => (int)(this.Cost?.Munitions ?? 0.0f);

        public int FuelCost => (int)(this.Cost?.Fuel ?? 0.0f);

        private double VisualWidth => widths[(this.Cost.AsCostArray().Count(x => x <= 0))];

        public CostExtension Cost {
            get => this.GetValue(CostProperty) as CostExtension;
            set => this.SetCost(value);
        }

        public CostControl() {
            this.InitializeComponent();
            this.ValueLabels = new Label[] {
                this.ManpowerCostValue, this.MunitionCostValue, this.FuelCostValue
            };
            this.Icons = new Image[] {
                this.ManpowerCostIcon, this.MunitionCostIcon, this.FuelCostIcon
            };
        }

        private void SetCost(CostExtension value) {

            // Set actual value
            this.SetValue(CostProperty, value);

            // Stor left index
            int li = 0;

            // Get values
            var cost = value.AsCostArray();

            // Run through
            for (int i = 0; i < cost.Length; i++) { 
                if (cost[i] > 0.0f) {
                    
                    // Set visible
                    this.ValueLabels[i].Visibility = Visibility.Visible;
                    this.Icons[i].Visibility = Visibility.Visible;

                    // Set offsets
                    this.ValueLabels[i].SetValue(Canvas.LeftProperty, lblLeft[li]);
                    this.Icons[i].SetValue(Canvas.LeftProperty, icoLeft[li]);

                    // Set value
                    this.ValueLabels[i].Content = (int)Math.Ceiling(cost[i]);

                    li++;
                } else {
                    this.ValueLabels[i].Visibility = Visibility.Collapsed;
                    this.Icons[i].Visibility = Visibility.Collapsed;
                }
            }

            this._SelfCanvas.Width = this.VisualWidth;

        }

    }

}
