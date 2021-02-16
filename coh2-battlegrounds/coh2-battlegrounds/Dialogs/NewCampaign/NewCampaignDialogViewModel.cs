using System.Collections.Generic;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Models.Campaigns;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Dialogs.NewCampaign {
    
    public enum NewCampaignDialogResult {
        Cancel,
        NewSingleplayer,
        HostCampaign,
    }

    public class NewCampaignDialogViewModel : DialogControlBase<NewCampaignDialogResult> {

        public ICommand CancelCommand { get; }

        public ICommand BeginCommand { get; }

        public NewCampaignData CampaignData { get; private set; }

        public int SelectedCampaignIndex { get; set; }

        public int SelectedCampaignDifficulty { get; set; }

        public int SelectedCampaignMode { get; set; }

        public int SelectedCampaignSide { get; set; }

        public List<string> Campaigns { get; }

        private NewCampaignDialogViewModel(string title, string[] campaigns) {

            // Set title
            this.Title = title;

            // Set campaigns
            this.Campaigns = new List<string>() { "None Selected" };
            this.Campaigns.AddRange(campaigns);

            // Set defaults
            this.SelectedCampaignIndex = 0;
            this.SelectedCampaignDifficulty = 1;
            this.SelectedCampaignMode = 0;
            this.SelectedCampaignSide = 0;

            // Assign commands
            this.BeginCommand = new RelayCommand<DialogWindow>(this.Begin);
            this.CancelCommand = new RelayCommand<DialogWindow>(this.Cancel);

            // Set default
            this.DialogCloseDefault = NewCampaignDialogResult.Cancel;

        }

        public void Cancel(DialogWindow caller) => this.CloseDialogWithResult(caller, NewCampaignDialogResult.Cancel);

        public void Begin(DialogWindow caller) {            
            this.CampaignData = new NewCampaignData(
                this.Campaigns[this.SelectedCampaignIndex],
                this.SelectedCampaignDifficulty,
                this.SelectedCampaignMode,
                this.SelectedCampaignSide);
            this.CloseDialogWithResult(caller, NewCampaignDialogResult.Cancel);
        }

        public static NewCampaignDialogResult ShowHostGameDialog(string title, out NewCampaignData campaignData, params string[] campaigns) {
            var dialog = new NewCampaignDialogViewModel(title, campaigns);
            var result = dialog.ShowDialog();
            campaignData = dialog.CampaignData;
            return result;
        }

    }

}
