using System;

using Battlegrounds.Networking.Lobby;

namespace BattlegroundsApp.Models.MockData {

    public class MockLobbyTeamMemberModel : ILobbyMember {

        public ulong ID { get; }

        public string Name { get; }

        public string Army { get; }

        public bool HasCompany => !string.IsNullOrEmpty(this.CompanyName);

        public string CompanyName { get; }

        public double CompanyValue { get; }

        public bool IsLocalMachine => false;

        public MockLobbyTeamMemberModel(ulong id, string name, string army, string company, double companyValue) {
            this.ID = id;
            this.Name = name;
            this.Army = army;
            this.CompanyName = company;
            this.CompanyValue = companyValue;
        }

        public void SetArmy(string army) => throw new NotSupportedException();

        public void SetCompany(string name, double value) => throw new NotSupportedException();

    }

}
