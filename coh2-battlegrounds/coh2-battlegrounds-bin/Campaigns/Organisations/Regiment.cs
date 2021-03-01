using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Locale;

namespace Battlegrounds.Campaigns.Organisations {
    
    public class Regiment : IJsonObject {

        public class Company : IJsonObject {
            public string Designation { get; } // Able, Baker.. etc
            public List<Squad> Units { get; }
            public int BaseStrength { get; private set; }
            public float Strength => this.Units.Count / (float)this.BaseStrength;
            public Company(string designation) {
                this.Designation = designation;
                this.Units = new List<Squad>();
            }
            public void SetSquads(List<Squad> units) {
                this.Units.Clear();
                this.Units.AddRange(units);
                this.BaseStrength = units.Count;
            }
            public string ToJsonReference() => throw new NotImplementedException();
        }

        private Regiment.Company[] m_companies;
        private int m_initialCompanyCount;

        public LocaleKey Name { get; }

        public string RegimentType { get; }

        // TODO: Implement some kind of json late-binding
        public Division ElementOf { get; }

        public float Strength => this.m_companies.Select(x => x.Strength).Sum() / this.m_initialCompanyCount;

        public bool IsArtillery { get; }

        public bool IsDeployed { get; set; }

        public Regiment(Division division, LocaleKey name, string regimentType, bool isArty, int companyCount) {
            this.ElementOf = division;
            this.Name = name;
            this.RegimentType = regimentType;
            this.IsArtillery = isArty;
            this.m_companies = new Regiment.Company[companyCount];
            this.m_initialCompanyCount = 0;
        }

        public void CreateCompany(int companyIndex, List<Squad> squads) {
            this.m_companies[companyIndex] = new Company(BattlegroundsInstance.Localize.GetNumberSuffix(companyIndex));
            this.m_companies[companyIndex].Units.AddRange(squads);
            this.m_initialCompanyCount++;
        }

        public string ToJsonReference() => throw new NotSupportedException();

        public void EachCompany(Action<Regiment.Company> action) => this.m_companies.ForEach(action);

        public Regiment.Company FirstCompany() => this.m_companies.FirstOrDefault(x => x.Strength > 0);

    }

}
