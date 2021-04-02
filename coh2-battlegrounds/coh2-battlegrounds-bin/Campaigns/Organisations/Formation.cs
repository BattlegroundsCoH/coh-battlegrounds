using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns.Organisations {
    
    /// <summary>
    /// Represents a movable organisation of regiments on the campaign map.
    /// </summary>
    public class Formation {

        private List<Regiment> m_regiments;
        private List<CampaignMapNode> m_path;
        private CampaignMapNode m_location;
        private int m_moveDistance;
        private int m_maxMoveDistance;

        /// <summary>
        /// Get the divisions represented in the formation.
        /// </summary>
        public Division[] Divisions => this.m_regiments.Select(x => x.ElementOf).Distinct().ToArray();

        /// <summary>
        /// Get the regiments represented by the formation.
        /// </summary>
        public Regiment[] Regiments => this.m_regiments.ToArray();

        /// <summary>
        /// Get the current node occupied by the formation.
        /// </summary>
        /// <remarks>
        /// Lua-Visible
        /// </remarks>
        [LuaUserobjectProperty]
        public CampaignMapNode Node => this.m_location;

        /// <summary>
        /// Get the destination of this formation
        /// </summary>
        public CampaignMapNode Destination => this.m_path.Count > 0 ? this.m_path[0] : null;

        /// <summary>
        /// Get the name of the dominant army in charge of this formation.
        /// </summary>
        /// <remarks>
        /// Lua-Visible
        /// </remarks>
        [LuaUserobjectProperty]
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
        /// Get the <see cref="CampaignArmyTeam"/> owning the formation.
        /// </summary>
        /// <remarks>
        /// Lua-Visible
        /// </remarks>
        [LuaUserobjectProperty]
        public CampaignArmyTeam Team { get; private set; }

        /// <summary>
        /// Initialise a new <see cref="Formation"/> class with basic properties.
        /// </summary>
        public Formation() {
            this.m_regiments = new List<Regiment>();
            this.m_moveDistance = 1;
            this.m_maxMoveDistance = 1;
        }

        /// <summary>
        /// Generate regiemntal data from divisional data.
        /// </summary>
        /// <param name="div">The <see cref="Division"/> to generate regiments from.</param>
        public void FromDivision(Division div) {
            div.Regiments.ForEach(x => {
                if (x.Strength > 0.0f && !x.IsDeployed) {
                    this.m_regiments.Add(x);
                }
            });
            this.Team = div.EleemntOf.Team;
        }

        /// <summary>
        /// Set the location of the formation and update the affected <see cref="CampaignMapNode"/>.
        /// </summary>
        /// <remarks>
        /// This will not check if node can contain unit.
        /// </remarks>
        /// <param name="node">The new location of the formation.</param>
        public void SetNodeLocation(CampaignMapNode node) {
            
            // Set this location
            this.m_location = node;
            
            // Inform node we've added ourselves as occupant
            node.AddOccupant(this);

        }

        /// <summary>
        /// Set the list of destinations for the formation and move max capacity.
        /// </summary>
        /// <param name="nodes">The node destinations.</param>
        public void SetNodeDestinationsAndMove(List<CampaignMapNode> nodes) {
            this.m_path = nodes;
            this.OnMoved(false);
        }

        /// <summary>
        /// Applies attrition to the formation.
        /// </summary>
        /// <param name="attrition">The amount of attrition to apply.</param>
        public void ApplyAttrition(double attrition) {

        }

        /// <summary>
        /// Disbands the formation, immediately removing it from the map.
        /// </summary>
        /// <remarks>
        /// Lua-invokable
        /// </remarks>
        /// <param name="killAll">Trigger kill-event on all regiments, removing them entirely from the game.</param>
        [LuaUserobjectMethod(UseMarshalling = true)]
        public void Disband(bool killAll) {

        }

        /// <summary>
        /// Merge regiments into the formation.
        /// </summary>
        /// <param name="regiments">The new regiments to merge with.</param>
        public void MergeInto(List<Regiment> regiments)
            => this.m_regiments.AddRange(regiments);

        /// <summary>
        /// Merge an entire formation into the formation.
        /// </summary>
        /// <param name="mergeInto">Formation to merge with.</param>
        public void MergeInto(Formation mergeInto) {
            this.MergeInto(mergeInto.m_regiments); // Merge regiments
            mergeInto.m_location = null; // Unset location
        }

        /// <summary>
        /// Remove all regiments in list.
        /// </summary>
        /// <param name="regiments">The list of regiments to remove.</param>
        public void RemoveRegiment(List<Regiment> regiments) {

        }

        /// <summary>
        /// Split the <see cref="Formation"/> into two smaller formations.
        /// </summary>
        /// <remarks>
        /// Lua-invokable
        /// </remarks>
        /// <returns>The new formation that was split from current formation.</returns>
        [LuaUserobjectMethod(UseMarshalling = true)]
        public Formation Split() => this.Split(2)[1];

        /// <summary>
        /// Split the <see cref="Formation"/> into <paramref name="subElements"/> smaller formations.
        /// </summary>
        /// <param name="subElements"></param>
        /// <returns>The new formations excluding the original formation.</returns>
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

        /// <summary>
        /// Calculate the strength of the formation.
        /// </summary>
        /// <remarks>
        /// Lua-invokable
        /// </remarks>
        /// <returns>The strength of the formation.</returns>
        [LuaUserobjectMethod(UseMarshalling = true)]
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

        /// <summary>
        /// Update end of round values.
        /// </summary>
        public void EndOfRound() {
            // Reset max move distance
            this.m_moveDistance = this.m_maxMoveDistance;

        }

        /// <summary>
        /// Get the current path of the formation.
        /// </summary>
        /// <returns>List of <see cref="CampaignMapNode"/> elements.</returns>
        public List<CampaignMapNode> GetPath() => this.m_path;

    }

}
