using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.UI.Converters.Icons;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BattlegroundsApp.Controls.Dashboard;

/// <summary>
/// Interaction logic for CompanyCard.xaml
/// </summary>
public partial class CompanyCard : UserControl {

    public double CompanyWinRate { get; set; }

    private static readonly Dictionary<Faction, ImageSource> Flags = new() {
        [Faction.Soviet] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/soviet.png")),
        [Faction.Wehrmacht] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/german.png")),
        [Faction.America] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/aef.png")),
        [Faction.OberkommandoWest] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/west_german.png")),
        [Faction.British] = new BitmapImage(new Uri("pack://application:,,,/coh2-battlegrounds;component/Resources/ingame/british.png"))
    };

    public static readonly DependencyProperty CompanyProperty 
        = DependencyProperty.Register(nameof(Company), typeof(Company), typeof(CompanyCard),
                                      new FrameworkPropertyMetadata(null,
                                      (a, b) => a.Cast<CompanyCard>(x => x.Company = b.NewValue as Company)));

    public Company? Company {
        get => this.GetValue(CompanyProperty) as Company;
        set {
            this.SetValue(CompanyProperty, value);
            this.TrySetCompanyData();
        }
    }

    public CompanyCard() {
        this.InitializeComponent();
        this.TrySetCompanyData();
    }

    private void TrySetCompanyData() => this.TrySetData(this.Company ?? null);

    private void TrySetData(Company? company) {

        // Do nothing if company is null
        if (company is null) {
            companyName.Text = "No Company Data";
            winRateValue.Text = "0%";
            infantryKillsValue.Text = "N/A";
            vehicleKillsValue.Text = "N/A";
            killDeathRatioValue.Text = "N/A";
            gamesPlayedValue.Text = "0";
            return;
        }

        this.CompanyWinRate = Math.Round(company.Statistics.WinRate * 100);

        // Set company faction
        factionIcon.Source = Flags[company.Army];

        // Set company name
        companyName.Text = company.Name;

        // Set company type
        typeIcon.Source = StringToCompanyTypeIcon.GetFromType(company.Type);

        // Set company data
        winRateValue.Text = company.Statistics.WinRate is 0 ? "0%" : $"{Math.Round(company.Statistics.WinRate * 100)}%";
        infantryKillsValue.Text = "N/A"; // TODO : This data is not currently being tracked
        vehicleKillsValue.Text = "N/A"; // TODO : This data is not currently being tracked
        killDeathRatioValue.Text = "N/A"; // TODO : This data is not currently being tracked
        gamesPlayedValue.Text = company.Statistics.TotalMatchCount.ToString();


    }

}
