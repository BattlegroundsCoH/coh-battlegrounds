using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Campaigns.Organisations {
    
    public class Formation {

        private List<Regiment> m_regiments;
        private CampaignMapNode m_location;

        /// <summary>
        /// 
        /// </summary>
        public Division[] Divisions => this.m_regiments.Select(x => x.ElementOf).Distinct().ToArray();

        /// <summary>
        /// 
        /// </summary>
        public Regiment[] Regiments => this.m_regiments.ToArray();

        /// <summary>
        /// 
        /// </summary>
        public CampaignMapNode Node => this.m_location;

        /// <summary>
        /// 
        /// </summary>
        public CampaignMapNode Destination { get; set; }

        /// <summary>
        /// Get the name of the dominant army in charge of this formation.
        /// </summary>
        public string Army => this.Divisions.FirstOrDefault().EleemntOf.Faction.Name;

        /// <summary>
        /// Get if the unit formation can be split.
        /// </summary>
        public bool CanSplit => this.m_regiments.Count > 1;

        /// <summary>
        /// 
        /// </summary>
        public CampaignArmyTeam Team { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Formation() {
            this.m_regiments = new List<Regiment>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="div"></param>
        public void FromDivision(Division div) {
            div.Regiments.ForEach(x => {
                if (x.Strength > 0.0f && !x.IsDeployed) {
                    this.m_regiments.Add(x);
                }
            });
            this.Team = div.EleemntOf.Team;
        }

        public void SetNodeLocation(CampaignMapNode node) => this.m_location = node;

        public void ApplyAttrition(double attrition) {

        }

        public void Disband(bool killAll) {

        }

        public void MergeInto(List<Regiment> regiments)
            => this.m_regiments.AddRange(regiments);

        public void MergeInto(Formation mergeInto) {
            this.MergeInto(mergeInto.m_regiments); // Merge regiments
            mergeInto.m_location = null; // Unset location
        }

        public void RemoveOrganisation(List<Regiment> regiments) {

        }

        public Formation Split() => this.Split(2)[1];

        public Formation[] Split(int subElements) {
            
            // Define organisations
            Formation[] organisations = new Formation[subElements];
            organisations[0] = this;

            // Determine regiments per organisation
            int regimentsPerOrg = this.m_regiments.Count / subElements;
            int i = 0;
            int j = 1;

            // Move regiments while there are empty regiments
            while (j != subElements) {
                
                // Create formation if not found
                if (organisations[j] is null) {
                    organisations[j] = new Formation();
                }

                // Move regiment
                organisations[j].m_regiments.Add(this.m_regiments[0]);
                this.m_regiments.RemoveAt(0);

                // Increment counters

                i++;
                if (i >= regimentsPerOrg) {
                    i = 0;
                    j++;
                }
            
            }

            // Return orgs
            return organisations;

        }

    }

}
