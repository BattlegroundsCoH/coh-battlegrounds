using System.Windows.Media;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;

using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {

    public class ObjectHoverData {

        public ImageSource Icon { get; }

        public string Name { get; }

        public string Description { get; }

        public CostExtension Cost { get; }

        public ObjectHoverData(SquadBlueprint sbp) {
            this.Name = GameLocale.GetString(sbp.UI.ScreenName);
            this.Description = GameLocale.GetString(sbp.UI.ShortDescription);
            this.Icon = App.ResourceHandler.GetIcon("unit_icons", sbp.UI.Icon);
            this.Cost = sbp.Cost;
        }

        public ObjectHoverData(EntityBlueprint ebp) {
            this.Name = GameLocale.GetString(ebp.UI.ScreenName);
            this.Description = GameLocale.GetString(ebp.UI.ShortDescription);
            //this.Icon = $"pack://application:,,,/Resources/ingame/object_icons/{ebp.Display.Icon}.png";
            this.Cost = ebp.Cost;
        }

    }

}
