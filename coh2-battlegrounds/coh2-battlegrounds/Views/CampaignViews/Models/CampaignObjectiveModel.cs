using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Battlegrounds.Campaigns.API;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignObjectiveModel {

        public ImageSource ObjectiveIcon { get; }

        public double ObjectiveIconWidth { get; }

        public double ObjectiveIconHeight { get; }

        public string ObjectiveText { get; }

        public string ObjectiveDesc { get; }

        public ObservableCollection<CampaignObjectiveModel> ObjectiveSubGoals { get; }

        public CampaignObjectiveModel(ICampaignGoal goal, CampaignResourceContext resourceContext) {
            this.ObjectiveIcon = goal.Type switch {
                CampaignGoalType.Primary => resourceContext.GetResource("objt_1"),
                CampaignGoalType.Secondary => resourceContext.GetResource("objt_2"),
                CampaignGoalType.Achievement => resourceContext.GetResource("objt_3"),
                _ => throw new NotImplementedException()
            };
            this.ObjectiveIconHeight = this.ObjectiveIconWidth = goal.Type switch {
                CampaignGoalType.Primary => 24,
                _ => 18
            };
            this.ObjectiveText = resourceContext.GetString(goal.Title);
            this.ObjectiveDesc = resourceContext.GetString(goal.Desc);
            this.ObjectiveSubGoals = new ObservableCollection<CampaignObjectiveModel>(goal.SubGoals.Select(x => new CampaignObjectiveModel(x, resourceContext)));
        }

    }

}
