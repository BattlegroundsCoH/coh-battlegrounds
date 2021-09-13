using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyCompanyItem {

        public string Name { get; set; }

        public Faction Army { get; set; }

        public string Type { get; set; }

        public bool IsAutoGeneration { get; }

        public bool IsNoneAvailable { get; }

        public LobbyCompanyItem(int type) {
            this.IsNoneAvailable = type <= 0;
            this.IsAutoGeneration = type == 1;
        }

        public LobbyCompanyItem(Company company) : this(2) {
            this.Army = company.Army;
            this.Name = company.Name;
            this.Type = company.Type.ToString();
        }

    }

}
