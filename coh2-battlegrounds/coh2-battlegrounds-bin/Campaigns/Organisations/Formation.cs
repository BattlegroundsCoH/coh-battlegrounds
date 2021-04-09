using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Lua;
using Battlegrounds.Util.Lists;

namespace Battlegrounds.Campaigns.Organisations {
    
    /// <summary>
    /// Represents a movable organisation of regiments on the campaign map.
    /// </summary>
    public class Formation : ICampaignFormation {

        private List<Regiment> m_regiments;
        private List<ICampaignMapNode> m_path;
        private ICampaignMapNode m_location;
        private int m_moveDistance;
        private int m_maxMoveDistance;
        private int m_deployTime;

        /// <summary>
        /// Get the divisions represented in the formation.
        /// </summary>
        public Division[] Divisions => this.m_regiments.Select(x => x.ElementOf).Distinct().ToArray();

        /// <summary>
        /// Get the regiments represented by the formation.
        /// </summary>
        public Regiment[] Regiments => this.m_regiments.ToArray();

        public ICampaignMapNode Node => this.m_location;

        public ICampaignMapNode Destination => this.m_path.Count > 0 ? this.m_path[0] : null;

        public string Army => this.Divisions.FirstOrDefault().EleemntOf.Faction.Name;

        public bool CanSplit => this.m_regiments.Count > 1;

        public bool CanMove => this.m_moveDistance > 0;

        public event FormationPositionEventHandler FormationMoved;

        public event FormationDisbandedEventHandler FormationDisbanded;

        public CampaignArmyTeam Team { get; private set; }

        /// <summary>
        /// Initialise a new <see cref="Formation"/> class with basic properties.
        /// </summary>
        public Formation() {
            this.m_regiments = new List<Regiment>();
            this.m_moveDistance = 1;
            this.m_maxMoveDistance = 1;
            this.m_path = new List<ICampaignMapNode>();
            this.m_deployTime = 0;
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
        public void SetNodeLocation(ICampaignMapNode node) {

            // Trigger event (if location
            if (this.m_location is not null) {
                this.FormationMoved?.Invoke(this, this.m_location, node);
            }

            // Set this location
            this.m_location = node;

            // Inform node we've added ourselves as occupant
            node.AddOccupant(this);

        }

        /// <summary>
        /// Set the list of destinations for the formation and move max capacity.
        /// </summary>
        /// <param name="nodes">The node destinations.</param>
        public void SetNodeDestinations(List<ICampaignMapNode> nodes) {
            this.m_path.Clear();
            this.m_path.AddRange(nodes);
        }

        /// <summary>
        /// Applies attrition to the formation.
        /// </summary>
        /// <param name="attrition">The amount of attrition to apply.</param>
        public void ApplyAttrition(double attrition) {

            // Get modified attrition
            double modifiedAttrition = attrition * this.Node.Occupants.Count;
            modifiedAttrition = Math.Log10((this.m_regiments.Count / modifiedAttrition) + this.m_deployTime);

            // Determine amount of units to 'off'.
            int killSquads = (int)Math.Ceiling(modifiedAttrition);

            // Loop through amount
            for (int i = 0; i < killSquads; i++) {

                // Find random regiment
                var regiment = this.m_regiments[BattlegroundsInstance.RNG.Next(0, this.m_regiments.Count)];

                // Select company
                var company = regiment.RandomCompany(BattlegroundsInstance.RNG);

                // Select squad
                var weighted = company.Units.ToWeightedList(s => 1.0 - (s.VeterancyRank / 5.1));

                // Pick random
                var pick = weighted.Pick(BattlegroundsInstance.RNG);

                // Remove the picked unit
                company.Units.Remove(pick);

            }

        }

        /// <summary>
        /// Disbands the formation, immediately removing it from the map.
        /// </summary>
        /// <remarks>
        /// Lua-invokable
        /// </remarks>
        /// <param name="killAll">Trigger kill-event on all regiments, removing them entirely from the game.</param>
        public void Disband(bool killAll) {

            // Invoke event
            this.FormationDisbanded?.Invoke(this, killAll);

            // Release from occupied noide
            this.Node.RemoveOccupant(this);

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
        public void RemoveRegiment(List<Regiment> regiments)
            => this.m_regiments.RemoveAll(x => regiments.Contains(x));

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
                    organisations[j] = new Formation {
                        Team = this.Team
                    };
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

        public void MoveToDestination() {
            if (this.m_path.Count > 0 && this.m_path[0] == this.m_location) {
                this.FormationMoved?.Invoke(this, this.m_location, this.m_path[0]);
                this.m_location = this.m_path[0];
                this.m_path.RemoveAt(0);
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
            this.m_deployTime++;

            // Apply attrition
            this.ApplyAttrition(this.Node.Attrition);

        }

        /// <summary>
        /// Get the current path of the formation.
        /// </summary>
        /// <returns>List of <see cref="CampaignMapNode"/> elements.</returns>
        public List<ICampaignMapNode> GetPath() => this.m_path;

    }

}
