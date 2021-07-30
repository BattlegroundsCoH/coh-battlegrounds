using System.Collections;

using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Controller;
using Battlegrounds.Util.Coroutines;

namespace Battlegrounds.Campaigns.AI {

    public class CampaignMapAI {

        public ICampaignTeam Team { get; }

        public ICampaignController Controller { get; }

        public CampaignMapAI(ICampaignTeam team, ICampaignController controller) {
            this.Team = team;
            this.Controller = controller;
        }

        public void Initialise() {

        }

        public IEnumerator ProcessTurn() {

            // Call AI scripts
            this.DoScripts();
            yield return new WaitCycle();

            // Evaluate Map
            this.EvaluateMap();
            yield return new WaitCycle();

            // Evaluate Forces
            this.EvaluateFormations();
            yield return new WaitCycle();

            // Based on evaluations, generate turn plans.
            this.FormulateTurnPlan();
            yield return new WaitCycle();

            // Here we then carry out plans and react (so move to engage, handle wins/losses)

            // End turn
            this.Controller.EndTurn();

        }

        private void DoScripts() {

        }

        private void EvaluateMap() {

        }

        private void EvaluateFormations() {

        }

        private void FormulateTurnPlan() {

        }

    }

}
