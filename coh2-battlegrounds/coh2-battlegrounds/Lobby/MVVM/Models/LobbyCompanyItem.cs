using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyCompanyItem {

        public const int COMPANY_NONE = 0;
        public const int COMPANY_AUTO = 1;
        public const int COMPANY_SOME = 2;

        public string Name { get; }

        public Faction Army { get; }

        public CompanyType Type { get; }

        public bool IsAuto { get; }

        public bool IsEmpty { get; }

        public string Faction => this.Army?.Name ?? string.Empty;

        public double Strength { get; }

        public LobbyCompanyItem(int type) {
            this.IsEmpty = type <= COMPANY_NONE;
            this.IsAuto = type == COMPANY_AUTO;
        }

        public LobbyCompanyItem(Company company) : this(COMPANY_SOME) {
            this.Army = company.Army;
            this.Name = company.Name;
            this.Type = company.Type;
            this.Strength = company.Strength;
        }

        public LobbyAPIStructs.LobbyCompany GetAPIObject()
            => new LobbyAPIStructs.LobbyCompany() {
                API = null,
                Army = this.IsEmpty ? "?" : this.Army.Name,
                Specialisation = this.IsEmpty ? string.Empty : this.Type.ToString(),
                IsAuto = this.IsAuto,
                IsNone = this.IsEmpty,
                Name = this.Name ?? string.Empty,
                Strength = (float)this.Strength
            };

    }

}
