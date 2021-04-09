using System.Windows.Media;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public interface ICampaignSelectable {

        string Title { get; }

        string Description { get; }

        CampaignSelectableInfoSection[] GetInfoSections();

    }

    public class CampaignSelectableInfoSection {

        public ImageSource Icon { get; }

        public double RequestedWidth { get; }

        public double RequestedHeight { get; }

        public string HelperText { get; }

        public string TooltipText { get; }

        public CampaignSelectableInfoSection(ImageSource icon, double width, double height, string text, string tooltip = null) {
            this.Icon = icon;
            this.RequestedHeight = height;
            this.RequestedWidth = width;
            this.HelperText = text;
            this.TooltipText = tooltip ?? string.Empty;
        }

    }

}
