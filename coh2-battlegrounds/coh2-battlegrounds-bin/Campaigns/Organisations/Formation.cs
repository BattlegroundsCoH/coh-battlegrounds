using System.Collections.Generic;
using System.Linq;

namespace Battlegrounds.Campaigns.Organisations {
    
    /// <summary>
    /// 
    /// </summary>
    public class Formation {

        private List<Regiment> m_regiments;
        private List<CampaignMapNode> m_path;
        private CampaignMapNode m_location;
        private int m_moveDistance;
        private int m_maxMoveDistance;

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
        /// Get the destination of this formation
        /// </summary>
        public CampaignMapNode Destination => this.m_path.Count > 0 ? this.m_path[0] : null;

        /// <summary>
        /// Get the name of the dominant army in charge of this formation.
        /// </summary>
        public string Army => this.Divisions.FirstOrDefault().EleemntOf.Faction.Name;

        /// <summary>
        /// Get if the unit formation can be split.
        /// </summary>
        public bool CanSplit => this.m_regiments.Count > 1;

        /// <summary>
        /// Get if the formation can currently move
        /// </summary>
        public bool CanMove => this.m_moveDistance > 0;

        /// <summary>
        /// 
        /// </summary>
        public CampaignArmyTeam Team { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Formation() {
            this.m_regiments = new List<Regiment>();
            this.m_moveDistance = 1;
            this.m_maxMoveDistance = 1;
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

        public void SetNodeDestinations(List<CampaignMapNode> nodes) {
            this.m_path = nodes;
            this.OnMoved(false);
        }

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

        public void RemoveRegiment(List<Regiment> regiments) {

        }

        /// <summary>
        /// Split the <see cref="Formation"/> into two smaller formations.
        /// </summary>
        /// <returns>The new formation that was split from current formation.</returns>
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
                    organisations[j].Team = this.Team;
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

        /// <summary>
        /// Updates destination and decrements available move distance by 1.
        /// </summary>
        public void OnMoved(bool actualMove = true) {
            if (this.m_path.Count > 0 && this.m_path[0] == this.m_location) {
                this.m_path.RemoveAt(0);
            }
            if (actualMove) {
                this.m_moveDistance--;
            }
        }

        public float CalculateStrength() {
            float avgStrength = 0.0f;
            int count = this.m_regiments.Count;
            var itt = this.m_regiments.GetEnumerator();
            while (itt.MoveNext()) {
                avgStrength += itt.Current.Strength;
                if (itt.Current.Strength <= 0.025) {
                    itt.Current.IsDeployed = false;
                    this.m_regiments.Remove(itt.Current);
                }
            }
            return avgStrength / count;
        }

        public void EndOfRound() {
            // Reset max move distance
            this.m_moveDistance = this.m_maxMoveDistance;

        }

        public List<CampaignMapNode> GetPath() => this.m_path;

    }

}
