using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public partial class SquadSlotSmall : UserControl {

        public string SquadName { get; }
        public string SquadIcon { get; }
        public SquadBlueprint Squad { get; }

        public SquadSlotSmall(SquadBlueprint squad) {
            this.DataContext = this;
            SquadName = GameLocale.GetString(uint.Parse(squad.LocaleName));
            SquadIcon = $"pack://application:,,,/Resources/ingame/unit_icons/{squad.Icon}.png";
            Squad = squad;
            InitializeComponent();
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed) {

                DataObject obj = new DataObject();
                obj.SetData("Squad", Squad);

                DragDrop.DoDragDrop(this, obj, DragDropEffects.Move);

            }
        }
    }
}
