using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyCompanyItem {

        public string Name { get; }

        public Faction Army { get; }

        public CompanyType Type { get; }

        public bool IsAuto { get; }

        public bool IsEmpty { get; }

        public string Faction => this.Army?.Name ?? string.Empty;

        public double Strength { get; }

        public LobbyCompanyItem(int type) {
            this.IsEmpty = type <= 0;
            this.IsAuto = type == 1;
        }

        public LobbyCompanyItem(Company company) : this(2) {
            this.Army = company.Army;
            this.Name = company.Name;
            this.Type = company.Type;
            this.Strength = company.Strength;
        }

        public LobbyAPIStructs.LobbyCompany GetAPIObject()
            => new LobbyAPIStructs.LobbyCompany() {
                API = null,
                Army = this.Army.Name,
                Specialisation = this.Type.ToString(),
                IsAuto = this.IsAuto,
                IsNone = this.IsEmpty,
                Name = this.Name,
                Strength = (float)this.Strength
            };

    }

}
