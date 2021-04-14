using Battlegrounds.Campaigns;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Models.Campaigns {
    
    /// <summary>
    /// Readonly struct containing data collected by a <see cref="Dialogs.NewCampaign.NewCampaignDialogViewModel"/>.
    /// </summary>
    public readonly struct NewCampaignData {

        /// <summary>
        /// Get the campaign <see cref="CampaignPackage"/> to load.
        /// </summary>
        public readonly CampaignPackage CampaignToLoad { get; }
        
        /// <summary>
        /// Get the difficulty of the campaign.
        /// </summary>
        public readonly int CampaignDifficulty { get; }
        
        /// <summary>
        /// Get the campaign mode.
        /// </summary>
        public readonly CampaignMode CampaignMode { get; }
        
        /// <summary>
        /// Get the side of the host to play on.
        /// </summary>
        public readonly Faction CampaignHostSide { get; }

        public NewCampaignData(CampaignPackage cmp, int diff, CampaignMode mode, Faction side) {
            this.CampaignDifficulty = diff;
            this.CampaignHostSide = side;
            this.CampaignMode = mode;
            this.CampaignToLoad = cmp;
        }

    }

}
