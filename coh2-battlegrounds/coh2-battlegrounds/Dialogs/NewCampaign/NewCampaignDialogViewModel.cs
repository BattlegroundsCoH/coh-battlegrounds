using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

using Battlegrounds;
using Battlegrounds.Campaigns;
using Battlegrounds.Game.Gameplay;

using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Models.Campaigns;
using BattlegroundsApp.Resources;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Dialogs.NewCampaign {

    public enum NewCampaignDialogResult {
        Cancel,
        NewSingleplayer,
        HostCampaign,
    }

    #region Option Classes
    public abstract class NewCampaignDialogViewSelectionOption { public abstract CampaignPackage GetPackage(); }

    public class NewCampaignDialogViewSelectedNone : NewCampaignDialogViewSelectionOption {
        public override CampaignPackage GetPackage() => null;
        public override string ToString() => "None Available";
    }

    public class NewCampaignDialogViewSelectedPackage : NewCampaignDialogViewSelectionOption {
        public CampaignPackage Package { get; }
        public NewCampaignDialogViewSelectedPackage(CampaignPackage package) {
            this.Package = package;
        }
        public override string ToString() => this.Package.LocaleManager.GetString(this.Package.Name);
        public override CampaignPackage GetPackage() => this.Package;
    }
    public class NewCampaignDialogViewSelectedMode {
        public CampaignMode Mode { get; }
        public NewCampaignDialogViewSelectedMode(CampaignMode mode) {
            this.Mode = mode;
        }
        public override string ToString() => BattlegroundsInstance.Localize.GetEnum(this.Mode);
    }
    public class NewCampaignDialogViewSelectedArmy {
        public Faction Faction { get; }
        public NewCampaignDialogViewSelectedArmy(Faction faction) {
            this.Faction = faction;
        }
        public override string ToString() => Faction.Name;
    }
    #endregion

    public class NewCampaignDialogViewModel : DialogControlBase<NewCampaignDialogResult> {

        int m_selectedCampaignIndex;

        public ICommand CancelCommand { get; }

        public ICommand BeginCommand { get; }

        public NewCampaignData CampaignData { get; private set; }

        public int SelectedCampaignIndex {
            get => this.m_selectedCampaignIndex;
            set {
                this.m_selectedCampaignIndex = value;
                this.SelectedCampaignChanged(value);
            }
        }

        public int SelectedCampaignDifficulty { get; set; }

        public int SelectedCampaignMode { get; set; }

        public int SelectedCampaignSide { get; set; }

        public ImageSource SelectedCampaignImageSource { get; set; }

        public List<NewCampaignDialogViewSelectionOption> Campaigns { get; }

        public List<NewCampaignDialogViewSelectedMode> AvailableModes { get; }

        public List<NewCampaignDialogViewSelectedArmy> AvailableArmies { get; }

        private NewCampaignDialogViewModel(string title, CampaignPackage[] campaigns) {

            // Set title
            this.Title = title;

            // Init available list
            this.AvailableModes = new List<NewCampaignDialogViewSelectedMode>();
            this.AvailableArmies = new List<NewCampaignDialogViewSelectedArmy>();

            // Set campaigns
            this.Campaigns = new List<NewCampaignDialogViewSelectionOption>();
            if (campaigns.Length == 0) {
                this.Campaigns.Add(new NewCampaignDialogViewSelectedNone());
            } else {
                this.Campaigns.AddRange(campaigns.Select(x => new NewCampaignDialogViewSelectedPackage(x)));
            }

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
            var mode = this.AvailableModes[this.SelectedCampaignMode].Mode;
            this.CampaignData = new NewCampaignData(
                this.Campaigns[this.SelectedCampaignIndex].GetPackage(),
                this.SelectedCampaignDifficulty,
                mode,
                this.AvailableArmies[this.SelectedCampaignSide].Faction);
            this.CloseDialogWithResult(caller, mode == CampaignMode.Singleplayer ? NewCampaignDialogResult.NewSingleplayer : NewCampaignDialogResult.HostCampaign);
        }

        private void SelectedCampaignChanged(int newValue) {
            if (this.Campaigns[newValue].GetPackage() is CampaignPackage package) {

                this.SelectedCampaignImageSource = PngImageSource.FromMemory(package.MapData.RawImageData);

                this.AvailableModes.Clear();
                this.AvailableModes.AddRange(package.CampaignModes.Select(x => new NewCampaignDialogViewSelectedMode(x)));

                this.AvailableArmies.Clear();
                this.AvailableArmies.AddRange(package.CampaignArmies.Select(x => new NewCampaignDialogViewSelectedArmy(x.Army)));

            }
        }

        public static NewCampaignDialogResult ShowHostGameDialog(string title, out NewCampaignData campaignData, params CampaignPackage[] campaigns) {
            var dialog = new NewCampaignDialogViewModel(title, campaigns);
            var result = dialog.ShowDialog();
            campaignData = dialog.CampaignData;
            return result;
        }

    }

}
