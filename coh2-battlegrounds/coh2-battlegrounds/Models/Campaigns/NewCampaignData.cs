namespace BattlegroundsApp.Models.Campaigns {
    
    /// <summary>
    /// Readonly struct containing data collected by a <see cref="Dialogs.NewCampaign.NewCampaignDialogViewModel"/>.
    /// </summary>
    public readonly struct NewCampaignData {
        
        /// <summary>
        /// Get the campaign file to load.
        /// </summary>
        public readonly string CampaignToLoad { get; }
        
        /// <summary>
        /// Get the difficulty of the campaign.
        /// </summary>
        public readonly int CampaignDifficulty { get; }
        
        /// <summary>
        /// Get the campaign mode.
        /// </summary>
        public readonly int CampaignMode { get; }
        
        /// <summary>
        /// Get the side of the host to play on.
        /// </summary>
        public readonly int CampaignHostSide { get; }

        public NewCampaignData(string cmp, int diff, int mode, int side) {
            this.CampaignDifficulty = diff;
            this.CampaignHostSide = side;
            this.CampaignMode = mode;
            this.CampaignToLoad = cmp;
        }

    }

}
