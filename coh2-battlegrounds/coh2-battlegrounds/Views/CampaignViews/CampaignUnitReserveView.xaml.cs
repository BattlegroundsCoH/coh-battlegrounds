using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BattlegroundsApp.Views.CampaignViews.Models;

namespace BattlegroundsApp.Views.CampaignViews {

    /// <summary>
    /// Interaction logic for CampaignUnitReserveView.xaml
    /// </summary>
    public partial class CampaignUnitReserveView : UserControl {

        public CampaignUnitReserveView() {
            InitializeComponent();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => this.HandleButtonDown();

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => this.HandleButtonDown();

        private void HandleButtonDown() {
            if (this.DataContext is CampaignUnitReserveModel model) {
                model.ReserveClicked.Execute(model);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && this.DataContext is CampaignUnitReserveModel model) {
                
                // Create data buffer
                DataObject obj = new DataObject();
                obj.SetData("Unit", model);
                obj.SetData("Object", this);

                // Begin drag/drop
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Move);

            }
        }

    }

}
